using DecorStore.API.Common;

namespace DecorStore.API.Interfaces.Services
{
    /// <summary>
    /// Interface for data encryption and decryption operations
    /// </summary>
    public interface IDataEncryptionService
    {
        /// <summary>
        /// Encrypt sensitive data
        /// </summary>
        Task<Result<string>> EncryptAsync(string plainText, string? keyId = null);

        /// <summary>
        /// Decrypt sensitive data
        /// </summary>
        Task<Result<string>> DecryptAsync(string encryptedText, string? keyId = null);

        /// <summary>
        /// Encrypt email address
        /// </summary>
        Task<Result<string>> EncryptEmailAsync(string email);

        /// <summary>
        /// Decrypt email address
        /// </summary>
        Task<Result<string>> DecryptEmailAsync(string encryptedEmail);

        /// <summary>
        /// Encrypt phone number
        /// </summary>
        Task<Result<string>> EncryptPhoneAsync(string phone);

        /// <summary>
        /// Decrypt phone number
        /// </summary>
        Task<Result<string>> DecryptPhoneAsync(string encryptedPhone);

        /// <summary>
        /// Encrypt PII data
        /// </summary>
        Task<Result<string>> EncryptPiiAsync(string piiData);

        /// <summary>
        /// Decrypt PII data
        /// </summary>
        Task<Result<string>> DecryptPiiAsync(string encryptedPiiData);

        /// <summary>
        /// Generate a new encryption key
        /// </summary>
        Task<Result<string>> GenerateKeyAsync();

        /// <summary>
        /// Rotate encryption keys
        /// </summary>
        Task<Result> RotateKeysAsync();

        /// <summary>
        /// Hash data for searching (deterministic)
        /// </summary>
        Task<Result<string>> HashForSearchAsync(string data);

        /// <summary>
        /// Mask sensitive data for logging
        /// </summary>
        string MaskSensitiveData(string data, char maskChar = '*', int visibleChars = 2);

        /// <summary>
        /// Detect and mask PII in text
        /// </summary>
        string DetectAndMaskPii(string text);

        /// <summary>
        /// Anonymize data for analytics
        /// </summary>
        Task<Result<string>> AnonymizeDataAsync(string data);

        /// <summary>
        /// Hash data using a secure algorithm
        /// </summary>
        Task<Result<string>> HashAsync(string data);
    }
}
