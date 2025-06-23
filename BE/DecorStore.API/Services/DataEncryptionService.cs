using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DecorStore.API.Common;
using DecorStore.API.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace DecorStore.API.Services
{
    /// <summary>
    /// Service for encrypting and decrypting sensitive data with key rotation support
    /// </summary>
    public class DataEncryptionService : IDataEncryptionService
    {
        private readonly ILogger<DataEncryptionService> _logger;
        private readonly DataEncryptionSettings _settings;
        private readonly ISecurityEventLogger _securityLogger;
        private readonly Dictionary<string, byte[]> _contextKeys;
        private readonly object _keyLock = new();

        public DataEncryptionService(
            ILogger<DataEncryptionService> logger,
            IOptions<DataEncryptionSettings> settings,
            ISecurityEventLogger securityLogger)
        {
            _logger = logger;
            _settings = settings.Value;
            _securityLogger = securityLogger;
            _contextKeys = new Dictionary<string, byte[]>();

            InitializeKeys();
        }

        private void InitializeKeys()
        {
            try
            {
                lock (_keyLock)
                {
                    // Initialize default context key
                    _contextKeys["default"] = DeriveKey(_settings.MasterKey, "default");
                    _contextKeys["email"] = DeriveKey(_settings.MasterKey, "email");
                    _contextKeys["phone"] = DeriveKey(_settings.MasterKey, "phone");
                    _contextKeys["payment"] = DeriveKey(_settings.MasterKey, "payment");
                    _contextKeys["pii"] = DeriveKey(_settings.MasterKey, "pii");
                }

                _logger.LogInformation("Data encryption keys initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize encryption keys");
                throw;
            }
        }

        private byte[] DeriveKey(string masterKey, string context)
        {
            using var rfc2898 = new Rfc2898DeriveBytes(
                Encoding.UTF8.GetBytes(masterKey),
                Encoding.UTF8.GetBytes(context + _settings.Salt),
                _settings.KeyDerivationIterations,
                HashAlgorithmName.SHA256);

            return rfc2898.GetBytes(32); // 256-bit key
        }

        public async Task<Result<string>> EncryptAsync(string plainText, string? keyId = null)        {
            try
            {
                if (string.IsNullOrEmpty(plainText))
                {
                    return Result<string>.Success(string.Empty);
                }

                var context = keyId ?? "default";
                if (!_contextKeys.TryGetValue(context, out var key))
                {
                    return Result<string>.Failure($"Unknown encryption context: {context}");
                }

                var encryptedData = await EncryptWithKeyAsync(plainText, key, context);
                
                await _securityLogger.LogSecurityViolationAsync(
                    "DATA_ENCRYPTED",
                    null,
                    "System",
                    $"Data encrypted with context: {context}",
                    0.1m);

                return Result<string>.Success(encryptedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to encrypt data with context {Context}", keyId ?? "default");
                return Result<string>.Failure("Encryption failed");
            }
        }

        public async Task<Result<string>> DecryptAsync(string encryptedData, string? keyId = null)        {
            try
            {
                if (string.IsNullOrEmpty(encryptedData))
                {
                    return Result<string>.Success(string.Empty);
                }

                var decryptionResult = await DecryptWithContextAsync(encryptedData);
                
                if (!decryptionResult.IsSuccess)
                {
                    await _securityLogger.LogSecurityViolationAsync(
                        "DECRYPTION_FAILED",
                        null,
                        "System",
                        $"Failed to decrypt data with context: {keyId ?? "default"}",
                        0.6m);
                }

                return decryptionResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt data with context {Context}", keyId ?? "default");
                return Result<string>.Failure("Decryption failed");
            }
        }

        public async Task<Result<string>> EncryptEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return Result<string>.Success(string.Empty);

            return await EncryptAsync(email, "email");
        }

        public async Task<Result<string>> DecryptEmailAsync(string encryptedEmail)
        {
            if (string.IsNullOrEmpty(encryptedEmail))
                return Result<string>.Success(string.Empty);

            return await DecryptAsync(encryptedEmail, "email");
        }

        public async Task<Result<string>> EncryptPhoneAsync(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return Result<string>.Success(string.Empty);

            return await EncryptAsync(phone, "phone");
        }

        public async Task<Result<string>> DecryptPhoneAsync(string encryptedPhone)
        {
            if (string.IsNullOrEmpty(encryptedPhone))
                return Result<string>.Success(string.Empty);

            return await DecryptAsync(encryptedPhone, "phone");
        }

        public async Task<Result<string>> EncryptPiiAsync(string piiData)
        {
            if (string.IsNullOrEmpty(piiData))
                return Result<string>.Success(string.Empty);

            return await EncryptAsync(piiData, "pii");
        }

        public async Task<Result<string>> DecryptPiiAsync(string encryptedPiiData)
        {
            if (string.IsNullOrEmpty(encryptedPiiData))
                return Result<string>.Success(string.Empty);

            return await DecryptAsync(encryptedPiiData, "pii");
        }

        public async Task<Result<string>> GenerateKeyAsync()
        {            try
            {
                var newKey = GenerateNewMasterKey();
                var keyString = Convert.ToBase64String(newKey);
                return Result<string>.Success(keyString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate new key");
                return Result<string>.Failure("Key generation failed");
            }
        }

        public async Task<Result<string>> HashForSearchAsync(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return Result<string>.Success(string.Empty);

                using var sha256 = SHA256.Create();
                var saltedData = data + _settings.Salt;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedData));
                var hashString = Convert.ToBase64String(hashedBytes);

                return Result<string>.Success(hashString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to hash data for search");
                return Result<string>.Failure("Hashing failed");
            }
        }

        public string MaskSensitiveData(string data, char maskChar = '*', int visibleChars = 2)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;

            if (data.Length <= visibleChars * 2)
                return new string(maskChar, data.Length);

            var start = data[..visibleChars];
            var end = data[^visibleChars..];
            var middle = new string(maskChar, data.Length - (visibleChars * 2));

            return start + middle + end;
        }

        public string DetectAndMaskPii(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Email pattern
            text = System.Text.RegularExpressions.Regex.Replace(text, 
                @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", 
                "***@***.***");

            // Phone pattern (various formats)
            text = System.Text.RegularExpressions.Regex.Replace(text, 
                @"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b", 
                "***-***-****");

            // SSN pattern
            text = System.Text.RegularExpressions.Regex.Replace(text, 
                @"\b\d{3}-\d{2}-\d{4}\b", 
                "***-**-****");

            // Credit card pattern
            text = System.Text.RegularExpressions.Regex.Replace(text, 
                @"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b", 
                "**** **** **** ****");

            return text;
        }        public async Task<Result<string>> AnonymizeDataAsync(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return Result<string>.Success(string.Empty);

                // Basic anonymization by hashing
                using var sha256 = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(data);
                var hashBytes = sha256.ComputeHash(bytes);
                var anonymized = Convert.ToBase64String(hashBytes)[..16]; // Use first 16 chars
                
                return Result<string>.Success(anonymized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to anonymize data");
                return Result<string>.Failure("Data anonymization failed");
            }
        }

        public async Task<Result<string>> HashAsync(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return Result<string>.Success(string.Empty);

                using var sha256 = SHA256.Create();
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                var hashString = Convert.ToBase64String(hashedBytes);

                return Result<string>.Success(hashString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to hash data");
                return Result<string>.Failure("Hashing failed");
            }
        }

        public async Task<Result> RotateKeysAsync()
        {
            try
            {
                _logger.LogInformation("Starting key rotation process");

                lock (_keyLock)
                {                // Generate new master key
                var newMasterKey = GenerateNewMasterKey();
                _contextKeys["master"] = newMasterKey;
                    
                    // Update key version
                    _settings.CurrentKeyVersion++;
                    
                    _logger.LogInformation("Key rotation completed successfully. New key version: {Version}", _settings.CurrentKeyVersion);
                }

                await _securityLogger.LogSecurityViolationAsync(
                    "KEY_ROTATION",
                    0,
                    "System",
                    "Encryption keys rotated successfully",
                    0.2m);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rotate encryption keys");
                await _securityLogger.LogSecurityViolationAsync(
                    "KEY_ROTATION_FAILED",
                    0,
                    "System",
                    $"Key rotation failed: {ex.Message}",
                    0.8m);
                return Result.Failure("Key rotation failed");
            }
        }

        private async Task<string> EncryptWithKeyAsync(string plainText, byte[] key, string context)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = key;
                aes.GenerateIV();

                var encrypted = new byte[aes.IV.Length + plainText.Length];
                Array.Copy(aes.IV, 0, encrypted, 0, aes.IV.Length);

                using var encryptor = aes.CreateEncryptor();
                using var msEncrypt = new MemoryStream();
                using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using var swEncrypt = new StreamWriter(csEncrypt);

                await swEncrypt.WriteAsync(plainText);
                await swEncrypt.FlushAsync();
                await csEncrypt.FlushFinalBlockAsync();

                var encryptedBytes = msEncrypt.ToArray();
                var result = new byte[aes.IV.Length + encryptedBytes.Length];
                Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                return Convert.ToBase64String(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to encrypt data with key for context {Context}", context);
                throw;
            }
        }

        private async Task<Result<string>> DecryptWithContextAsync(string encryptedData)
        {
            try
            {
                var encryptedBytes = Convert.FromBase64String(encryptedData);
                
                if (encryptedBytes.Length < 16) // Minimum IV size
                {
                    return Result<string>.Failure("Invalid encrypted data format");
                }

                using var aes = Aes.Create();
                var iv = new byte[16];
                Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);
                aes.IV = iv;

                // Try to find the correct key by context
                foreach (var kvp in _contextKeys)
                {
                    try
                    {
                        aes.Key = kvp.Value;
                        var cipherBytes = new byte[encryptedBytes.Length - iv.Length];
                        Array.Copy(encryptedBytes, iv.Length, cipherBytes, 0, cipherBytes.Length);

                        using var decryptor = aes.CreateDecryptor();
                        using var msDecrypt = new MemoryStream(cipherBytes);
                        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                        using var srDecrypt = new StreamReader(csDecrypt);

                        var result = await srDecrypt.ReadToEndAsync();
                        return Result<string>.Success(result);
                    }
                    catch
                    {
                        // Try next key
                        continue;
                    }
                }

                return Result<string>.Failure("Failed to decrypt data - no valid key found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt data");
                return Result<string>.Failure($"Decryption failed: {ex.Message}");
            }
        }

        private byte[] GenerateNewMasterKey()
        {
            try
            {
                using var rng = RandomNumberGenerator.Create();
                var key = new byte[32]; // 256-bit key
                rng.GetBytes(key);
                
                _logger.LogInformation("Generated new master encryption key");
                return key;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate new master key");
                throw;
            }
        }

        private class EncryptedDataEnvelope
        {
            public string Data { get; set; } = string.Empty;
            public string IV { get; set; } = string.Empty;
            public string Context { get; set; } = string.Empty;
            public int KeyVersion { get; set; }
            public string Algorithm { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
        }
    }

    /// <summary>
    /// Configuration settings for data encryption
    /// </summary>
    public class DataEncryptionSettings
    {
        public string MasterKey { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public int KeyDerivationIterations { get; set; } = 100000;
        public int CurrentKeyVersion { get; set; } = 1;
        public bool EnableFieldLevelEncryption { get; set; } = true;
        public bool EnableDataAnonymization { get; set; } = true;
        public bool RequireEncryptionValidation { get; set; } = true;
        public int KeyRotationDays { get; set; } = 90;
        public List<string> EncryptedFields { get; set; } = new() { "Email", "Phone", "Address" };
    }
}
