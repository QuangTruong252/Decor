using DecorStore.API.DTOs.Excel;
using DecorStore.API.DTOs;

namespace DecorStore.API.Services.Excel
{
    /// <summary>
    /// Service interface for Customer Excel import/export operations
    /// </summary>
    public interface ICustomerExcelService
    {
        /// <summary>
        /// Imports customers from Excel file
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <param name="validateOnly">If true, only validates without saving</param>
        /// <returns>Import result with success/error details</returns>
        Task<ExcelImportResultDTO<CustomerExcelDTO>> ImportCustomersAsync(Stream fileStream, bool validateOnly = false);

        /// <summary>
        /// Exports customers to Excel file
        /// </summary>
        /// <param name="filter">Filter criteria for customers to export</param>
        /// <param name="exportRequest">Export configuration</param>
        /// <returns>Excel file as byte array</returns>
        Task<byte[]> ExportCustomersAsync(CustomerFilterDTO? filter = null, ExcelExportRequestDTO? exportRequest = null);

        /// <summary>
        /// Creates Excel template for customer import
        /// </summary>
        /// <param name="includeExampleRow">Whether to include example data</param>
        /// <returns>Excel template as byte array</returns>
        Task<byte[]> CreateCustomerTemplateAsync(bool includeExampleRow = true);

        /// <summary>
        /// Validates customer Excel file without importing
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <returns>Validation result</returns>
        Task<ExcelValidationResultDTO> ValidateCustomerExcelAsync(Stream fileStream);

        /// <summary>
        /// Imports customers in batches for large files
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <param name="batchSize">Number of customers to process per batch</param>
        /// <param name="progressCallback">Progress reporting callback</param>
        /// <returns>Async enumerable of batch results</returns>
        IAsyncEnumerable<ExcelImportResultDTO<CustomerExcelDTO>> ImportCustomersInBatchesAsync(
            Stream fileStream, 
            int batchSize = 100, 
            IProgress<int>? progressCallback = null);

        /// <summary>
        /// Gets customer import statistics
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <returns>Import statistics</returns>
        Task<CustomerImportStatisticsDTO> GetImportStatisticsAsync(Stream fileStream);

        /// <summary>
        /// Validates email uniqueness
        /// </summary>
        /// <param name="emails">List of email addresses to validate</param>
        /// <param name="excludeCustomerIds">Customer IDs to exclude from uniqueness check</param>
        /// <returns>List of duplicate email addresses</returns>
        Task<List<string>> ValidateEmailUniquenessAsync(IEnumerable<string> emails, IEnumerable<int>? excludeCustomerIds = null);

        /// <summary>
        /// Calculates customer metrics for export
        /// </summary>
        /// <param name="customers">List of customers to calculate metrics for</param>
        /// <returns>Updated customers with calculated metrics</returns>
        Task<List<CustomerExcelDTO>> CalculateCustomerMetricsAsync(List<CustomerExcelDTO> customers);

        /// <summary>
        /// Segments customers based on their order history
        /// </summary>
        /// <param name="customers">List of customers to segment</param>
        /// <returns>Customers with updated segments</returns>
        Task<List<CustomerExcelDTO>> SegmentCustomersAsync(List<CustomerExcelDTO> customers);
    }

    /// <summary>
    /// Statistics for customer import operation
    /// </summary>
    public class CustomerImportStatisticsDTO
    {
        /// <summary>
        /// Total number of rows in the file
        /// </summary>
        public int TotalRows { get; set; }

        /// <summary>
        /// Number of new customers to be created
        /// </summary>
        public int NewCustomers { get; set; }

        /// <summary>
        /// Number of existing customers to be updated
        /// </summary>
        public int UpdatedCustomers { get; set; }

        /// <summary>
        /// Number of rows with validation errors
        /// </summary>
        public int ErrorRows { get; set; }

        /// <summary>
        /// Unique countries in the import
        /// </summary>
        public List<string> Countries { get; set; } = new List<string>();

        /// <summary>
        /// Unique states/provinces in the import
        /// </summary>
        public List<string> States { get; set; } = new List<string>();

        /// <summary>
        /// Unique cities in the import
        /// </summary>
        public List<string> Cities { get; set; } = new List<string>();

        /// <summary>
        /// Email addresses that would be duplicated
        /// </summary>
        public List<string> DuplicateEmails { get; set; } = new List<string>();

        /// <summary>
        /// Invalid email addresses detected
        /// </summary>
        public List<string> InvalidEmails { get; set; } = new List<string>();

        /// <summary>
        /// Invalid phone numbers detected
        /// </summary>
        public List<string> InvalidPhones { get; set; } = new List<string>();

        /// <summary>
        /// Estimated processing time
        /// </summary>
        public TimeSpan EstimatedProcessingTime { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Geographic distribution summary
        /// </summary>
        public Dictionary<string, int> GeographicDistribution { get; set; } = new Dictionary<string, int>();
    }
}
