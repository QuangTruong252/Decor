using DecorStore.API.DTOs.Excel;
using DecorStore.API.DTOs;

namespace DecorStore.API.Services.Excel
{
    /// <summary>
    /// Service interface for Category Excel import/export operations
    /// </summary>
    public interface ICategoryExcelService
    {
        /// <summary>
        /// Imports categories from Excel file
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <param name="validateOnly">If true, only validates without saving</param>
        /// <returns>Import result with success/error details</returns>
        Task<ExcelImportResultDTO<CategoryExcelDTO>> ImportCategoriesAsync(Stream fileStream, bool validateOnly = false);

        /// <summary>
        /// Exports categories to Excel file
        /// </summary>
        /// <param name="filter">Filter criteria for categories to export</param>
        /// <param name="exportRequest">Export configuration</param>
        /// <returns>Excel file as byte array</returns>
        Task<byte[]> ExportCategoriesAsync(CategoryFilterDTO? filter = null, ExcelExportRequestDTO? exportRequest = null);

        /// <summary>
        /// Creates Excel template for category import
        /// </summary>
        /// <param name="includeExampleRow">Whether to include example data</param>
        /// <returns>Excel template as byte array</returns>
        Task<byte[]> CreateCategoryTemplateAsync(bool includeExampleRow = true);

        /// <summary>
        /// Validates category Excel file without importing
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <returns>Validation result</returns>
        Task<ExcelValidationResultDTO> ValidateCategoryExcelAsync(Stream fileStream);

        /// <summary>
        /// Imports categories in batches for large files
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <param name="batchSize">Number of categories to process per batch</param>
        /// <param name="progressCallback">Progress reporting callback</param>
        /// <returns>Async enumerable of batch results</returns>
        IAsyncEnumerable<ExcelImportResultDTO<CategoryExcelDTO>> ImportCategoriesInBatchesAsync(
            Stream fileStream, 
            int batchSize = 100, 
            IProgress<int>? progressCallback = null);

        /// <summary>
        /// Gets category import statistics
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <returns>Import statistics</returns>
        Task<CategoryImportStatisticsDTO> GetImportStatisticsAsync(Stream fileStream);

        /// <summary>
        /// Resolves parent category names to category IDs
        /// </summary>
        /// <param name="parentNames">List of parent category names</param>
        /// <returns>Dictionary mapping parent names to IDs</returns>
        Task<Dictionary<string, int>> ResolveParentCategoryNamesAsync(IEnumerable<string> parentNames);

        /// <summary>
        /// Validates category hierarchy for circular references
        /// </summary>
        /// <param name="categories">List of categories to validate</param>
        /// <returns>List of hierarchy validation errors</returns>
        Task<List<string>> ValidateCategoryHierarchyAsync(IEnumerable<CategoryExcelDTO> categories);

        /// <summary>
        /// Validates category name uniqueness
        /// </summary>
        /// <param name="categoryNames">List of category names to validate</param>
        /// <param name="excludeCategoryIds">Category IDs to exclude from uniqueness check</param>
        /// <returns>List of duplicate category names</returns>
        Task<List<string>> ValidateCategoryNameUniquenessAsync(IEnumerable<string> categoryNames, IEnumerable<int>? excludeCategoryIds = null);
    }

    /// <summary>
    /// Statistics for category import operation
    /// </summary>
    public class CategoryImportStatisticsDTO
    {
        /// <summary>
        /// Total number of rows in the file
        /// </summary>
        public int TotalRows { get; set; }

        /// <summary>
        /// Number of new categories to be created
        /// </summary>
        public int NewCategories { get; set; }

        /// <summary>
        /// Number of existing categories to be updated
        /// </summary>
        public int UpdatedCategories { get; set; }

        /// <summary>
        /// Number of rows with validation errors
        /// </summary>
        public int ErrorRows { get; set; }

        /// <summary>
        /// Number of root categories (no parent)
        /// </summary>
        public int RootCategories { get; set; }

        /// <summary>
        /// Number of subcategories
        /// </summary>
        public int Subcategories { get; set; }

        /// <summary>
        /// Maximum hierarchy depth
        /// </summary>
        public int MaxHierarchyDepth { get; set; }

        /// <summary>
        /// Parent categories referenced in the file
        /// </summary>
        public List<string> ParentCategories { get; set; } = new List<string>();

        /// <summary>
        /// Category names that would be duplicated
        /// </summary>
        public List<string> DuplicateNames { get; set; } = new List<string>();

        /// <summary>
        /// Circular reference errors detected
        /// </summary>
        public List<string> CircularReferences { get; set; } = new List<string>();

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
