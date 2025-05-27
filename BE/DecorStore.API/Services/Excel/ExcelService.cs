using OfficeOpenXml;
using OfficeOpenXml.Style;
using DecorStore.API.DTOs.Excel;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace DecorStore.API.Services.Excel
{
    /// <summary>
    /// Core Excel service implementation for import/export operations
    /// </summary>
    public class ExcelService : IExcelService
    {
        private readonly ILogger<ExcelService> _logger;

        public ExcelService(ILogger<ExcelService> logger)
        {
            _logger = logger;

            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Reads Excel file and converts to list of DTOs
        /// </summary>
        public async Task<ExcelImportResultDTO<T>> ReadExcelAsync<T>(
            Stream fileStream,
            Dictionary<string, string> columnMappings,
            bool hasHeader = true) where T : class, new()
        {
            var startTime = DateTime.UtcNow;
            var result = new ExcelImportResultDTO<T>();

            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    result.Errors.Add(new ExcelValidationErrorDTO(0, "", "No worksheets found in the Excel file", ExcelErrorCodes.WORKSHEET_NOT_FOUND));
                    return result;
                }

                var startRow = hasHeader ? 2 : 1;
                var endRow = worksheet.Dimension?.End.Row ?? 0;
                var endCol = worksheet.Dimension?.End.Column ?? 0;

                result.TotalRows = Math.Max(0, endRow - startRow + 1);

                // Get header mappings
                var headerMappings = GetHeaderMappings(worksheet, columnMappings, hasHeader);

                // Process each row
                for (int row = startRow; row <= endRow; row++)
                {
                    try
                    {
                        var item = await ProcessRowAsync<T>(worksheet, row, headerMappings, result.Errors);
                        if (item != null)
                        {
                            result.Data.Add(item);
                            result.SuccessfulRows++;
                        }
                        else
                        {
                            result.ErrorRows++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing row {Row}", row);
                        result.Errors.Add(new ExcelValidationErrorDTO(row, "", $"Unexpected error: {ex.Message}", ExcelErrorCodes.FILE_FORMAT_ERROR));
                        result.ErrorRows++;
                    }
                }

                result.ProcessingTime = DateTime.UtcNow - startTime;
                result.Metadata["WorksheetName"] = worksheet.Name;
                result.Metadata["TotalColumns"] = endCol;

                _logger.LogInformation("Excel import completed. Processed {TotalRows} rows, {SuccessfulRows} successful, {ErrorRows} errors",
                    result.TotalRows, result.SuccessfulRows, result.ErrorRows);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Excel file");
                result.Errors.Add(new ExcelValidationErrorDTO(0, "", $"Failed to read Excel file: {ex.Message}", ExcelErrorCodes.FILE_FORMAT_ERROR));
                result.ProcessingTime = DateTime.UtcNow - startTime;
                return result;
            }
        }

        /// <summary>
        /// Creates Excel file from list of DTOs
        /// </summary>
        public async Task<byte[]> CreateExcelAsync<T>(
            IEnumerable<T> data,
            Dictionary<string, string> columnMappings,
            string worksheetName = "Sheet1") where T : class
        {
            try
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add(worksheetName);

                var dataList = data.ToList();
                if (!dataList.Any())
                {
                    // Create empty worksheet with headers only
                    await CreateHeaderRowAsync(worksheet, columnMappings);
                }
                else
                {
                    await PopulateWorksheetAsync(worksheet, dataList, columnMappings);
                }

                // Apply formatting
                ApplyDefaultFormatting(worksheet, columnMappings.Count);

                return await Task.FromResult(package.GetAsByteArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Excel file");
                throw new InvalidOperationException($"Failed to create Excel file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates Excel template file with headers only
        /// </summary>
        public async Task<byte[]> CreateTemplateAsync(
            Dictionary<string, string> columnMappings,
            string worksheetName = "Template",
            bool includeExampleRow = true)
        {
            try
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add(worksheetName);

                // Create header row
                await CreateHeaderRowAsync(worksheet, columnMappings);

                // Add example row if requested
                if (includeExampleRow)
                {
                    await CreateExampleRowAsync(worksheet, columnMappings);
                }

                // Apply formatting
                ApplyTemplateFormatting(worksheet, columnMappings.Count, includeExampleRow);

                return await Task.FromResult(package.GetAsByteArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Excel template");
                throw new InvalidOperationException($"Failed to create Excel template: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates Excel file structure and content
        /// </summary>
        public async Task<ExcelValidationResultDTO> ValidateExcelFileAsync(
            Stream fileStream,
            IEnumerable<string> expectedColumns,
            long maxFileSize = 10 * 1024 * 1024)
        {
            var result = new ExcelValidationResultDTO();

            try
            {
                // Check file size
                if (fileStream.Length > maxFileSize)
                {
                    result.Errors.Add($"File size ({fileStream.Length:N0} bytes) exceeds maximum allowed size ({maxFileSize:N0} bytes)");
                    return result;
                }

                result.FileInfo.FileSizeBytes = fileStream.Length;

                using var package = new ExcelPackage(fileStream);

                result.FileInfo.WorksheetCount = package.Workbook.Worksheets.Count;
                result.FileInfo.WorksheetNames = package.Workbook.Worksheets.Select(w => w.Name).ToList();
                result.FileInfo.FileFormat = "xlsx";

                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    result.Errors.Add("No worksheets found in the Excel file");
                    return result;
                }

                if (worksheet.Dimension == null)
                {
                    result.Errors.Add("Worksheet is empty");
                    return result;
                }

                result.FileInfo.RowCount = worksheet.Dimension.End.Row;
                result.FileInfo.ColumnCount = worksheet.Dimension.End.Column;

                // Get detected columns from first row
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var headerValue = worksheet.Cells[1, col].Text?.Trim();
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        result.DetectedColumns.Add(headerValue);
                    }
                }

                // Check for missing and extra columns
                var expectedColumnsList = expectedColumns.ToList();
                result.MissingColumns = expectedColumnsList.Except(result.DetectedColumns, StringComparer.OrdinalIgnoreCase).ToList();
                result.ExtraColumns = result.DetectedColumns.Except(expectedColumnsList, StringComparer.OrdinalIgnoreCase).ToList();

                if (result.MissingColumns.Any())
                {
                    result.Warnings.Add($"Missing expected columns: {string.Join(", ", result.MissingColumns)}");
                }

                if (result.ExtraColumns.Any())
                {
                    result.Warnings.Add($"Extra columns found: {string.Join(", ", result.ExtraColumns)}");
                }

                result.IsValid = !result.Errors.Any();

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Excel file");
                result.Errors.Add($"Failed to validate Excel file: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Gets column mappings for a specific entity type
        /// </summary>
        public Dictionary<string, string> GetDefaultColumnMappings<T>() where T : class
        {
            var mappings = new Dictionary<string, string>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                // Skip complex types and collections
                if (IsSimpleType(property.PropertyType))
                {
                    var displayName = GetDisplayName(property);
                    mappings[property.Name] = displayName;
                }
            }

            return mappings;
        }

        /// <summary>
        /// Processes large Excel files in chunks
        /// </summary>
        public async IAsyncEnumerable<ExcelImportChunkDTO<T>> ReadExcelInChunksAsync<T>(
            Stream fileStream,
            Dictionary<string, string> columnMappings,
            int chunkSize = 1000,
            IProgress<int>? progressCallback = null) where T : class, new()
        {
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet?.Dimension == null)
                yield break;

            var headerMappings = GetHeaderMappings(worksheet, columnMappings, true);
            var totalRows = worksheet.Dimension.End.Row - 1; // Exclude header
            var processedRows = 0;
            var chunkNumber = 1;

            for (int startRow = 2; startRow <= worksheet.Dimension.End.Row; startRow += chunkSize)
            {
                var endRow = Math.Min(startRow + chunkSize - 1, worksheet.Dimension.End.Row);
                var chunk = new ExcelImportChunkDTO<T>
                {
                    ChunkNumber = chunkNumber++,
                    StartRow = startRow,
                    EndRow = endRow,
                    IsLastChunk = endRow >= worksheet.Dimension.End.Row
                };

                for (int row = startRow; row <= endRow; row++)
                {
                    var item = await ProcessRowAsync<T>(worksheet, row, headerMappings, chunk.Errors);
                    if (item != null)
                    {
                        chunk.Data.Add(item);
                    }
                    processedRows++;
                }

                progressCallback?.Report((int)((double)processedRows / totalRows * 100));
                yield return chunk;
            }
        }

        #region Private Helper Methods

        private Dictionary<string, int> GetHeaderMappings(ExcelWorksheet worksheet, Dictionary<string, string> columnMappings, bool hasHeader)
        {
            var headerMappings = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            if (!hasHeader)
            {
                // Use column positions if no headers
                int col = 1;
                foreach (var mapping in columnMappings)
                {
                    headerMappings[mapping.Key] = col++;
                }
                return headerMappings;
            }

            // Map headers to column positions
            var reverseMapping = columnMappings.ToDictionary(kvp => kvp.Value, kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);

            for (int col = 1; col <= worksheet.Dimension?.End.Column; col++)
            {
                var headerValue = worksheet.Cells[1, col].Text?.Trim();
                if (!string.IsNullOrEmpty(headerValue) && reverseMapping.TryGetValue(headerValue, out var propertyName))
                {
                    headerMappings[propertyName] = col;
                }
            }

            return headerMappings;
        }

        private async Task<T?> ProcessRowAsync<T>(ExcelWorksheet worksheet, int row, Dictionary<string, int> headerMappings, List<ExcelValidationErrorDTO> errors) where T : class, new()
        {
            var item = new T();
            var hasData = false;
            var rowErrors = new List<ExcelValidationErrorDTO>();

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && IsSimpleType(p.PropertyType))
                .ToList();

            foreach (var property in properties)
            {
                if (!headerMappings.TryGetValue(property.Name, out var columnIndex))
                    continue;

                try
                {
                    var cellValue = worksheet.Cells[row, columnIndex].Text?.Trim();

                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        hasData = true;
                        var convertedValue = ConvertCellValue(cellValue, property.PropertyType);
                        property.SetValue(item, convertedValue);
                    }
                    else if (IsRequired(property))
                    {
                        var columnName = headerMappings.FirstOrDefault(kvp => kvp.Value == columnIndex).Key ?? $"Column {columnIndex}";
                        rowErrors.Add(new ExcelValidationErrorDTO(row, columnName, "Required field is missing", ExcelErrorCodes.REQUIRED_FIELD_MISSING));
                    }
                }
                catch (Exception ex)
                {
                    var columnName = headerMappings.FirstOrDefault(kvp => kvp.Value == columnIndex).Key ?? $"Column {columnIndex}";
                    var cellValue = worksheet.Cells[row, columnIndex].Text;
                    rowErrors.Add(new ExcelValidationErrorDTO(row, columnName, $"Invalid value: {ex.Message}", ExcelErrorCodes.INVALID_DATA_TYPE, ExcelErrorSeverity.Error, cellValue));
                }
            }

            errors.AddRange(rowErrors);

            // Return item only if it has data and no critical errors
            return hasData && !rowErrors.Any(e => e.Severity == ExcelErrorSeverity.Critical) ? item : null;
        }

        private object? ConvertCellValue(string cellValue, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(cellValue))
                return null;

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            return underlyingType.Name switch
            {
                nameof(String) => cellValue,
                nameof(Int32) => int.Parse(cellValue, CultureInfo.InvariantCulture),
                nameof(Decimal) => decimal.Parse(cellValue, CultureInfo.InvariantCulture),
                nameof(Double) => double.Parse(cellValue, CultureInfo.InvariantCulture),
                nameof(Single) => float.Parse(cellValue, CultureInfo.InvariantCulture),
                nameof(Boolean) => ParseBoolean(cellValue),
                nameof(DateTime) => DateTime.Parse(cellValue, CultureInfo.InvariantCulture),
                nameof(Guid) => Guid.Parse(cellValue),
                _ => underlyingType.IsEnum ? Enum.Parse(underlyingType, cellValue, true) : Convert.ChangeType(cellValue, underlyingType, CultureInfo.InvariantCulture)
            };
        }

        private bool ParseBoolean(string value)
        {
            return value.ToLowerInvariant() switch
            {
                "true" or "yes" or "y" or "1" or "on" => true,
                "false" or "no" or "n" or "0" or "off" => false,
                _ => bool.Parse(value)
            };
        }

        private bool IsSimpleType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return underlyingType.IsPrimitive ||
                   underlyingType.IsEnum ||
                   underlyingType == typeof(string) ||
                   underlyingType == typeof(decimal) ||
                   underlyingType == typeof(DateTime) ||
                   underlyingType == typeof(Guid);
        }

        private bool IsRequired(PropertyInfo property)
        {
            return property.GetCustomAttribute<RequiredAttribute>() != null;
        }

        private string GetDisplayName(PropertyInfo property)
        {
            var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute?.Name != null)
                return displayAttribute.Name;

            // Convert PascalCase to Title Case
            return string.Concat(property.Name.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private async Task CreateHeaderRowAsync(ExcelWorksheet worksheet, Dictionary<string, string> columnMappings)
        {
            int col = 1;
            foreach (var mapping in columnMappings)
            {
                worksheet.Cells[1, col].Value = mapping.Value;
                col++;
            }
            await Task.CompletedTask;
        }

        private async Task CreateExampleRowAsync(ExcelWorksheet worksheet, Dictionary<string, string> columnMappings)
        {
            // This would be implemented with entity-specific example data
            // For now, just add placeholder text
            int col = 1;
            foreach (var mapping in columnMappings)
            {
                worksheet.Cells[2, col].Value = $"Example {mapping.Value}";
                col++;
            }
            await Task.CompletedTask;
        }

        private async Task PopulateWorksheetAsync<T>(ExcelWorksheet worksheet, List<T> data, Dictionary<string, string> columnMappings) where T : class
        {
            // Create headers
            await CreateHeaderRowAsync(worksheet, columnMappings);

            // Populate data
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => columnMappings.ContainsKey(p.Name))
                .ToList();

            for (int row = 0; row < data.Count; row++)
            {
                int col = 1;
                foreach (var mapping in columnMappings)
                {
                    var property = properties.FirstOrDefault(p => p.Name == mapping.Key);
                    if (property != null)
                    {
                        var value = property.GetValue(data[row]);
                        worksheet.Cells[row + 2, col].Value = value;
                    }
                    col++;
                }
            }
        }

        private void ApplyDefaultFormatting(ExcelWorksheet worksheet, int columnCount)
        {
            // Header formatting
            using var headerRange = worksheet.Cells[1, 1, 1, columnCount];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            headerRange.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();
        }

        private void ApplyTemplateFormatting(ExcelWorksheet worksheet, int columnCount, bool hasExampleRow)
        {
            ApplyDefaultFormatting(worksheet, columnCount);

            if (hasExampleRow)
            {
                // Format example row
                using var exampleRange = worksheet.Cells[2, 1, 2, columnCount];
                exampleRange.Style.Font.Italic = true;
                exampleRange.Style.Font.Color.SetColor(System.Drawing.Color.Gray);
            }
        }

        #endregion
    }
}
