using DecorStore.API.DTOs.Excel;
using DecorStore.API.DTOs;

namespace DecorStore.API.Services.Excel
{
    /// <summary>
    /// Service interface for Order Excel import/export operations
    /// </summary>
    public interface IOrderExcelService
    {
        /// <summary>
        /// Imports orders from Excel file
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <param name="validateOnly">If true, only validates without saving</param>
        /// <returns>Import result with success/error details</returns>
        Task<ExcelImportResultDTO<OrderExcelDTO>> ImportOrdersAsync(Stream fileStream, bool validateOnly = false);

        /// <summary>
        /// Exports orders to Excel file
        /// </summary>
        /// <param name="filter">Filter criteria for orders to export</param>
        /// <param name="exportRequest">Export configuration</param>
        /// <returns>Excel file as byte array</returns>
        Task<byte[]> ExportOrdersAsync(OrderFilterDTO? filter = null, ExcelExportRequestDTO? exportRequest = null);

        /// <summary>
        /// Creates Excel template for order import
        /// </summary>
        /// <param name="includeExampleRow">Whether to include example data</param>
        /// <returns>Excel template as byte array</returns>
        Task<byte[]> CreateOrderTemplateAsync(bool includeExampleRow = true);

        /// <summary>
        /// Validates order Excel file without importing
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <returns>Validation result</returns>
        Task<ExcelValidationResultDTO> ValidateOrderExcelAsync(Stream fileStream);

        /// <summary>
        /// Imports orders in batches for large files
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <param name="batchSize">Number of orders to process per batch</param>
        /// <param name="progressCallback">Progress reporting callback</param>
        /// <returns>Async enumerable of batch results</returns>
        IAsyncEnumerable<ExcelImportResultDTO<OrderExcelDTO>> ImportOrdersInBatchesAsync(
            Stream fileStream, 
            int batchSize = 50, 
            IProgress<int>? progressCallback = null);

        /// <summary>
        /// Gets order import statistics
        /// </summary>
        /// <param name="fileStream">Excel file stream</param>
        /// <returns>Import statistics</returns>
        Task<OrderImportStatisticsDTO> GetImportStatisticsAsync(Stream fileStream);

        /// <summary>
        /// Resolves user emails to user IDs
        /// </summary>
        /// <param name="userEmails">List of user email addresses</param>
        /// <returns>Dictionary mapping emails to user IDs</returns>
        Task<Dictionary<string, int>> ResolveUserEmailsAsync(IEnumerable<string> userEmails);

        /// <summary>
        /// Resolves customer emails to customer IDs
        /// </summary>
        /// <param name="customerEmails">List of customer email addresses</param>
        /// <returns>Dictionary mapping emails to customer IDs</returns>
        Task<Dictionary<string, int>> ResolveCustomerEmailsAsync(IEnumerable<string> customerEmails);

        /// <summary>
        /// Validates product SKUs and gets product information
        /// </summary>
        /// <param name="productSkus">List of product SKUs</param>
        /// <returns>Dictionary mapping SKUs to product information</returns>
        Task<Dictionary<string, ProductInfo>> ValidateProductSkusAsync(IEnumerable<string> productSkus);

        /// <summary>
        /// Validates order status values
        /// </summary>
        /// <param name="orderStatuses">List of order status values</param>
        /// <returns>List of invalid status values</returns>
        Task<List<string>> ValidateOrderStatusesAsync(IEnumerable<string> orderStatuses);

        /// <summary>
        /// Calculates order totals and validates amounts
        /// </summary>
        /// <param name="orders">List of orders to validate</param>
        /// <returns>List of orders with calculation errors</returns>
        Task<List<string>> ValidateOrderCalculationsAsync(IEnumerable<OrderExcelDTO> orders);

        /// <summary>
        /// Processes complex order relationships and dependencies
        /// </summary>
        /// <param name="orders">List of orders to process</param>
        /// <returns>Processed orders with resolved relationships</returns>
        Task<List<OrderExcelDTO>> ProcessOrderRelationshipsAsync(List<OrderExcelDTO> orders);
    }

    /// <summary>
    /// Statistics for order import operation
    /// </summary>
    public class OrderImportStatisticsDTO
    {
        /// <summary>
        /// Total number of rows in the file
        /// </summary>
        public int TotalRows { get; set; }

        /// <summary>
        /// Number of new orders to be created
        /// </summary>
        public int NewOrders { get; set; }

        /// <summary>
        /// Number of existing orders to be updated
        /// </summary>
        public int UpdatedOrders { get; set; }

        /// <summary>
        /// Number of rows with validation errors
        /// </summary>
        public int ErrorRows { get; set; }

        /// <summary>
        /// Unique users referenced in the import
        /// </summary>
        public List<string> UniqueUsers { get; set; } = new List<string>();

        /// <summary>
        /// Unique customers referenced in the import
        /// </summary>
        public List<string> UniqueCustomers { get; set; } = new List<string>();

        /// <summary>
        /// Unique products referenced in the import
        /// </summary>
        public List<string> UniqueProducts { get; set; } = new List<string>();

        /// <summary>
        /// Order status distribution
        /// </summary>
        public Dictionary<string, int> StatusDistribution { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Payment method distribution
        /// </summary>
        public Dictionary<string, int> PaymentMethodDistribution { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Total order value in the import
        /// </summary>
        public decimal TotalOrderValue { get; set; }

        /// <summary>
        /// Average order value
        /// </summary>
        public decimal AverageOrderValue { get; set; }

        /// <summary>
        /// Date range of orders in the import
        /// </summary>
        public DateRange OrderDateRange { get; set; } = new DateRange();

        /// <summary>
        /// Invalid user emails detected
        /// </summary>
        public List<string> InvalidUserEmails { get; set; } = new List<string>();

        /// <summary>
        /// Invalid customer emails detected
        /// </summary>
        public List<string> InvalidCustomerEmails { get; set; } = new List<string>();

        /// <summary>
        /// Invalid product SKUs detected
        /// </summary>
        public List<string> InvalidProductSkus { get; set; } = new List<string>();

        /// <summary>
        /// Invalid order statuses detected
        /// </summary>
        public List<string> InvalidOrderStatuses { get; set; } = new List<string>();

        /// <summary>
        /// Orders with calculation errors
        /// </summary>
        public List<string> CalculationErrors { get; set; } = new List<string>();

        /// <summary>
        /// Estimated processing time
        /// </summary>
        public TimeSpan EstimatedProcessingTime { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Complex relationship warnings
        /// </summary>
        public List<string> RelationshipWarnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Product information for validation
    /// </summary>
    public class ProductInfo
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Product SKU
        /// </summary>
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Current price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Available stock quantity
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Whether the product is active
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Date range helper class
    /// </summary>
    public class DateRange
    {
        /// <summary>
        /// Start date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Number of days in the range
        /// </summary>
        public int? DaysInRange => StartDate.HasValue && EndDate.HasValue 
            ? (int)(EndDate.Value - StartDate.Value).TotalDays + 1 
            : null;
    }
}
