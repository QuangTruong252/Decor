using DecorStore.API.DTOs.Excel;
using DecorStore.API.DTOs;

namespace DecorStore.API.Services.Excel
{
    /// <summary>
    /// Service interface for Product Excel import/export operations
    /// </summary>
    public interface IProductExcelService
    {
        /// <summary>
        /// Imports products from Excel file
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <param name="validateOnly">If true, only validates without saving</param>
        /// <returns>Import result with success/error details</returns>
        Task<ExcelImportResultDTO<ProductExcelDTO>> ImportProductsAsync(Stream fileStream, bool validateOnly = false);

        /// <summary>
        /// Exports products to Excel file
        /// </summary>
        /// <param name="filter">Filter criteria for products to export</param>
        /// <param name="exportRequest">Export configuration</param>
        /// <returns>Excel file as byte array</returns>
        Task<byte[]> ExportProductsAsync(ProductFilterDTO? filter = null, ExcelExportRequestDTO? exportRequest = null);

        /// <summary>
        /// Creates Excel template for product import
        /// </summary>
        /// <param name="includeExampleRow">Whether to include example data</param>
        /// <returns>Excel template as byte array</returns>
        Task<byte[]> CreateProductTemplateAsync(bool includeExampleRow = true);

        /// <summary>
        /// Validates product Excel file without importing
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <returns>Validation result</returns>
        Task<ExcelValidationResultDTO> ValidateProductExcelAsync(Stream fileStream);

        /// <summary>
        /// Imports products in batches for large files
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <param name="batchSize">Number of products to process per batch</param>
        /// <param name="progressCallback">Progress reporting callback</param>
        /// <returns>Async enumerable of batch results</returns>
        IAsyncEnumerable<ExcelImportResultDTO<ProductExcelDTO>> ImportProductsInBatchesAsync(
            Stream fileStream, 
            int batchSize = 100, 
            IProgress<int>? progressCallback = null);

        /// <summary>
        /// Gets product import statistics
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <returns>Import statistics</returns>
        Task<ProductImportStatisticsDTO> GetImportStatisticsAsync(Stream fileStream);

        /// <summary>
        /// Resolves category names to category IDs
        /// </summary>
        /// <param name="categoryNames">List of category names</param>
        /// <returns>Dictionary mapping category names to IDs</returns>
        Task<Dictionary<string, int>> ResolveCategoryNamesAsync(IEnumerable<string> categoryNames);

        /// <summary>
        /// Validates SKU uniqueness
        /// </summary>
        /// <param name="skus">List of SKUs to validate</param>
        /// <param name="excludeProductIds">Product IDs to exclude from uniqueness check</param>
        /// <returns>List of duplicate SKUs</returns>
        Task<List<string>> ValidateSkuUniquenessAsync(IEnumerable<string> skus, IEnumerable<int>? excludeProductIds = null);
    }

    /// <summary>
    /// Statistics for product import operation
    /// </summary>
    public class ProductImportStatisticsDTO
    {
        /// <summary>
        /// Total number of rows in the file
        /// </summary>
        public int TotalRows { get; set; }

        /// <summary>
        /// Number of new products to be created
        /// </summary>
        public int NewProducts { get; set; }

        /// <summary>
        /// Number of existing products to be updated
        /// </summary>
        public int UpdatedProducts { get; set; }

        /// <summary>
        /// Number of rows with validation errors
        /// </summary>
        public int ErrorRows { get; set; }

        /// <summary>
        /// Unique categories referenced in the file
        /// </summary>
        public List<string> Categories { get; set; } = new List<string>();

        /// <summary>
        /// SKUs that would be duplicated
        /// </summary>
        public List<string> DuplicateSkus { get; set; } = new List<string>();

        /// <summary>
        /// Estimated processing time
        /// </summary>
        public TimeSpan EstimatedProcessingTime { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }
    }
}
