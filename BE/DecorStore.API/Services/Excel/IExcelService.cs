using DecorStore.API.DTOs.Excel;

namespace DecorStore.API.Services.Excel
{
    /// <summary>
    /// Generic Excel service interface for import/export operations
    /// </summary>
    public interface IExcelService
    {
        /// <summary>
        /// Reads Excel file and converts to list of DTOs
        /// </summary>
        /// <typeparam name="T">DTO type to convert to</typeparam>
        /// <param name="fileStream">Excel file stream</param>
        /// <param name="columnMappings">Column mapping configuration</param>
        /// <param name="hasHeader">Whether the first row contains headers</param>
        /// <returns>List of DTOs and validation results</returns>
        Task<ExcelImportResultDTO<T>> ReadExcelAsync<T>(
            Stream fileStream, 
            Dictionary<string, string> columnMappings, 
            bool hasHeader = true) where T : class, new();

        /// <summary>
        /// Creates Excel file from list of DTOs
        /// </summary>
        /// <typeparam name="T">DTO type to export</typeparam>
        /// <param name="data">Data to export</param>
        /// <param name="columnMappings">Column mapping configuration</param>
        /// <param name="worksheetName">Name of the worksheet</param>
        /// <returns>Excel file as byte array</returns>
        Task<byte[]> CreateExcelAsync<T>(
            IEnumerable<T> data, 
            Dictionary<string, string> columnMappings, 
            string worksheetName = "Sheet1") where T : class;

        /// <summary>
        /// Creates Excel template file with headers only
        /// </summary>
        /// <param name="columnMappings">Column mapping configuration</param>
        /// <param name="worksheetName">Name of the worksheet</param>
        /// <param name="includeExampleRow">Whether to include an example row</param>
        /// <returns>Excel template as byte array</returns>
        Task<byte[]> CreateTemplateAsync(
            Dictionary<string, string> columnMappings, 
            string worksheetName = "Template",
            bool includeExampleRow = true);

        /// <summary>
        /// Validates Excel file structure and content
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <param name="expectedColumns">Expected column names</param>
        /// <param name="maxFileSize">Maximum file size in bytes</param>
        /// <returns>Validation result</returns>
        Task<ExcelValidationResultDTO> ValidateExcelFileAsync(
            Stream fileStream, 
            IEnumerable<string> expectedColumns, 
            long maxFileSize = 10 * 1024 * 1024); // 10MB default

        /// <summary>
        /// Gets column mappings for a specific entity type
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <returns>Default column mappings</returns>
        Dictionary<string, string> GetDefaultColumnMappings<T>() where T : class;

        /// <summary>
        /// Processes large Excel files in chunks
        /// </summary>
        /// <typeparam name="T">DTO type</typeparam>
        /// <param name="fileStream">Excel file stream</param>
        /// <param name="columnMappings">Column mapping configuration</param>
        /// <param name="chunkSize">Number of rows to process at once</param>
        /// <param name="progressCallback">Progress reporting callback</param>
        /// <returns>Async enumerable of chunks</returns>
        IAsyncEnumerable<ExcelImportChunkDTO<T>> ReadExcelInChunksAsync<T>(
            Stream fileStream,
            Dictionary<string, string> columnMappings,
            int chunkSize = 1000,
            IProgress<int>? progressCallback = null) where T : class, new();
    }
}
