using AutoMapper;
using DecorStore.API.DTOs.Excel;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;
using DecorStore.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DecorStore.API.Services.Excel
{
    /// <summary>
    /// Service for Order Excel import/export operations
    /// </summary>
    public class OrderExcelService : IOrderExcelService
    {
        private readonly IExcelService _excelService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderExcelService> _logger;

        // Valid order statuses
        private static readonly HashSet<string> ValidOrderStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Pending", "Processing", "Shipped", "Delivered", "Cancelled", "Refunded", "On Hold"
        };

        public OrderExcelService(
            IExcelService excelService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<OrderExcelService> logger)
        {
            _excelService = excelService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Imports orders from Excel file
        /// </summary>
        public async Task<ExcelImportResultDTO<OrderExcelDTO>> ImportOrdersAsync(Stream fileStream, bool validateOnly = false)
        {
            var columnMappings = OrderExcelDTO.GetImportColumnMappings();
            var importResult = await _excelService.ReadExcelAsync<OrderExcelDTO>(fileStream, columnMappings);

            if (!importResult.IsSuccess)
            {
                return importResult;
            }

            // Process complex relationships
            importResult.Data = await ProcessOrderRelationshipsAsync(importResult.Data);

            // Additional business validation
            await ValidateBusinessRulesAsync(importResult);

            if (validateOnly || !importResult.IsSuccess)
            {
                return importResult;
            }

            // Save to database
            await SaveOrdersAsync(importResult);

            return importResult;
        }

        /// <summary>
        /// Exports orders to Excel file
        /// </summary>
        public async Task<byte[]> ExportOrdersAsync(OrderFilterDTO? filter = null, ExcelExportRequestDTO? exportRequest = null)
        {
            filter ??= new OrderFilterDTO { PageNumber = 1, PageSize = int.MaxValue };
            exportRequest ??= new ExcelExportRequestDTO { WorksheetName = "Orders" };

            // Get orders from database
            var orders = await _unitOfWork.Orders.GetAllAsync();

            // Map to Excel DTOs
            var orderExcelDtos = await MapToExcelDtosAsync(orders);

            // Calculate metrics
            foreach (var order in orderExcelDtos)
            {
                order.CalculateMetrics();
            }

            // Get column mappings
            var columnMappings = OrderExcelDTO.GetColumnMappings();

            // Filter columns based on export request
            if (exportRequest.ColumnsToInclude?.Any() == true)
            {
                columnMappings = columnMappings
                    .Where(kvp => exportRequest.ColumnsToInclude.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            if (exportRequest.ColumnsToExclude?.Any() == true)
            {
                columnMappings = columnMappings
                    .Where(kvp => !exportRequest.ColumnsToExclude.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            return await _excelService.CreateExcelAsync(orderExcelDtos, columnMappings, exportRequest.WorksheetName);
        }

        /// <summary>
        /// Creates Excel template for order import
        /// </summary>
        public async Task<byte[]> CreateOrderTemplateAsync(bool includeExampleRow = true)
        {
            var columnMappings = OrderExcelDTO.GetImportColumnMappings();
            return await _excelService.CreateTemplateAsync(columnMappings, "Order Import Template", includeExampleRow);
        }

        /// <summary>
        /// Validates order Excel file without importing
        /// </summary>
        public async Task<ExcelValidationResultDTO> ValidateOrderExcelAsync(Stream fileStream)
        {
            var expectedColumns = OrderExcelDTO.GetImportColumnMappings().Values;
            return await _excelService.ValidateExcelFileAsync(fileStream, expectedColumns);
        }

        /// <summary>
        /// Imports orders in batches for large files
        /// </summary>
        public async IAsyncEnumerable<ExcelImportResultDTO<OrderExcelDTO>> ImportOrdersInBatchesAsync(
            Stream fileStream, 
            int batchSize = 50, 
            IProgress<int>? progressCallback = null)
        {
            var columnMappings = OrderExcelDTO.GetImportColumnMappings();
            
            await foreach (var chunk in _excelService.ReadExcelInChunksAsync<OrderExcelDTO>(fileStream, columnMappings, batchSize, progressCallback))
            {
                var batchResult = new ExcelImportResultDTO<OrderExcelDTO>
                {
                    Data = chunk.Data,
                    Errors = chunk.Errors,
                    TotalRows = chunk.Data.Count,
                    SuccessfulRows = chunk.Data.Count,
                    ErrorRows = chunk.Errors.Count
                };

                // Process relationships for this batch
                batchResult.Data = await ProcessOrderRelationshipsAsync(batchResult.Data);

                // Validate business rules for this batch
                await ValidateBusinessRulesAsync(batchResult);

                // Save this batch if no errors
                if (batchResult.IsSuccess)
                {
                    await SaveOrdersAsync(batchResult);
                }

                yield return batchResult;
            }
        }

        /// <summary>
        /// Gets order import statistics
        /// </summary>
        public async Task<OrderImportStatisticsDTO> GetImportStatisticsAsync(Stream fileStream)
        {
            var validation = await ValidateOrderExcelAsync(fileStream);
            var columnMappings = OrderExcelDTO.GetImportColumnMappings();
            var importResult = await _excelService.ReadExcelAsync<OrderExcelDTO>(fileStream, columnMappings);

            var statistics = new OrderImportStatisticsDTO
            {
                TotalRows = importResult.TotalRows,
                ErrorRows = importResult.ErrorRows,
                FileSizeBytes = fileStream.Length,
                EstimatedProcessingTime = TimeSpan.FromSeconds(importResult.TotalRows * 0.1) // Slowest processing due to complexity
            };

            // Analyze data
            var existingOrderIds = importResult.Data
                .Where(o => o.Id.HasValue)
                .Select(o => o.Id!.Value)
                .ToList();

            if (existingOrderIds.Any())
            {
                var existingOrders = await _unitOfWork.Orders.GetAllAsync();
                var existingIds = existingOrders.Where(o => existingOrderIds.Contains(o.Id)).Select(o => o.Id).ToHashSet();
                
                statistics.UpdatedOrders = existingIds.Count;
                statistics.NewOrders = importResult.Data.Count - statistics.UpdatedOrders;
            }
            else
            {
                statistics.NewOrders = importResult.Data.Count;
            }

            // Analyze users
            statistics.UniqueUsers = importResult.Data
                .Where(o => !string.IsNullOrEmpty(o.UserEmail))
                .Select(o => o.UserEmail!)
                .Distinct()
                .ToList();

            // Analyze customers
            statistics.UniqueCustomers = importResult.Data
                .Where(o => !string.IsNullOrEmpty(o.CustomerEmail))
                .Select(o => o.CustomerEmail!)
                .Distinct()
                .ToList();

            // Analyze products from order items
            var allProducts = new HashSet<string>();
            foreach (var order in importResult.Data)
            {
                var items = order.ParseOrderItems();
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.ProductName))
                        allProducts.Add(item.ProductName);
                }
            }
            statistics.UniqueProducts = allProducts.ToList();

            // Status distribution
            statistics.StatusDistribution = importResult.Data
                .GroupBy(o => o.OrderStatus)
                .ToDictionary(g => g.Key, g => g.Count());

            // Payment method distribution
            statistics.PaymentMethodDistribution = importResult.Data
                .GroupBy(o => o.PaymentMethod)
                .ToDictionary(g => g.Key, g => g.Count());

            // Financial analysis
            statistics.TotalOrderValue = importResult.Data.Sum(o => o.TotalAmount);
            statistics.AverageOrderValue = importResult.Data.Any() ? statistics.TotalOrderValue / importResult.Data.Count : 0;

            // Date range analysis
            if (importResult.Data.Any())
            {
                statistics.OrderDateRange.StartDate = importResult.Data.Min(o => o.OrderDate);
                statistics.OrderDateRange.EndDate = importResult.Data.Max(o => o.OrderDate);
            }

            // Validation checks
            statistics.InvalidUserEmails = await GetInvalidUserEmailsAsync(statistics.UniqueUsers);
            statistics.InvalidCustomerEmails = await GetInvalidCustomerEmailsAsync(statistics.UniqueCustomers);
            statistics.InvalidOrderStatuses = await ValidateOrderStatusesAsync(statistics.StatusDistribution.Keys);
            statistics.CalculationErrors = await ValidateOrderCalculationsAsync(importResult.Data);

            return statistics;
        }

        /// <summary>
        /// Resolves user emails to user IDs
        /// </summary>
        public async Task<Dictionary<string, int>> ResolveUserEmailsAsync(IEnumerable<string> userEmails)
        {
            // This would need to be implemented based on your User repository
            // For now, return empty dictionary
            return await Task.FromResult(new Dictionary<string, int>());
        }

        /// <summary>
        /// Resolves customer emails to customer IDs
        /// </summary>
        public async Task<Dictionary<string, int>> ResolveCustomerEmailsAsync(IEnumerable<string> customerEmails)
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            return customers
                .Where(c => customerEmails.Contains(c.Email, StringComparer.OrdinalIgnoreCase))
                .ToDictionary(c => c.Email, c => c.Id, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates product SKUs and gets product information
        /// </summary>
        public async Task<Dictionary<string, ProductInfo>> ValidateProductSkusAsync(IEnumerable<string> productSkus)
        {
            var products = await _unitOfWork.Products.GetAllAsync(new ProductFilterDTO { PageNumber = 1, PageSize = int.MaxValue });
            return products
                .Where(p => productSkus.Contains(p.SKU, StringComparer.OrdinalIgnoreCase))
                .ToDictionary(p => p.SKU, p => new ProductInfo
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    IsActive = !p.IsDeleted
                }, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates order status values
        /// </summary>
        public async Task<List<string>> ValidateOrderStatusesAsync(IEnumerable<string> orderStatuses)
        {
            return await Task.FromResult(orderStatuses.Where(status => !ValidOrderStatuses.Contains(status)).ToList());
        }

        /// <summary>
        /// Calculates order totals and validates amounts
        /// </summary>
        public async Task<List<string>> ValidateOrderCalculationsAsync(IEnumerable<OrderExcelDTO> orders)
        {
            var errors = new List<string>();

            foreach (var order in orders)
            {
                var items = order.ParseOrderItems();
                if (items.Any())
                {
                    var calculatedSubtotal = items.Sum(i => i.TotalPrice);
                    var calculatedTotal = calculatedSubtotal + order.TaxAmount + order.ShippingCost - order.DiscountAmount;

                    if (Math.Abs(calculatedTotal - order.TotalAmount) > 0.01m)
                    {
                        errors.Add($"Order {order.Id ?? 0} (Row {order.RowNumber}): Total amount mismatch. Expected: {calculatedTotal:C}, Actual: {order.TotalAmount:C}");
                    }
                }
            }

            return await Task.FromResult(errors);
        }

        /// <summary>
        /// Processes complex order relationships and dependencies
        /// </summary>
        public async Task<List<OrderExcelDTO>> ProcessOrderRelationshipsAsync(List<OrderExcelDTO> orders)
        {
            // Resolve user emails to IDs
            var userEmails = orders.Where(o => !string.IsNullOrEmpty(o.UserEmail)).Select(o => o.UserEmail!).Distinct();
            var userMappings = await ResolveUserEmailsAsync(userEmails);

            // Resolve customer emails to IDs
            var customerEmails = orders.Where(o => !string.IsNullOrEmpty(o.CustomerEmail)).Select(o => o.CustomerEmail!).Distinct();
            var customerMappings = await ResolveCustomerEmailsAsync(customerEmails);

            // Process each order
            foreach (var order in orders)
            {
                // Resolve user ID from email if needed
                if ((!order.UserId.HasValue || order.UserId.Value <= 0) && !string.IsNullOrEmpty(order.UserEmail))
                {
                    if (userMappings.TryGetValue(order.UserEmail, out var userId))
                    {
                        order.UserId = userId;
                    }
                }

                // Resolve customer ID from email if needed
                if ((!order.CustomerId.HasValue || order.CustomerId.Value <= 0) && !string.IsNullOrEmpty(order.CustomerEmail))
                {
                    if (customerMappings.TryGetValue(order.CustomerEmail, out var customerId))
                    {
                        order.CustomerId = customerId;
                    }
                }
            }

            return orders;
        }

        #region Private Methods

        private async Task ValidateBusinessRulesAsync(ExcelImportResultDTO<OrderExcelDTO> importResult)
        {
            // Get all user and customer emails for validation
            var userEmails = importResult.Data.Where(o => !string.IsNullOrEmpty(o.UserEmail)).Select(o => o.UserEmail!).Distinct().ToList();
            var customerEmails = importResult.Data.Where(o => !string.IsNullOrEmpty(o.CustomerEmail)).Select(o => o.CustomerEmail!).Distinct().ToList();

            var invalidUserEmails = await GetInvalidUserEmailsAsync(userEmails);
            var invalidCustomerEmails = await GetInvalidCustomerEmailsAsync(customerEmails);

            // Process each order
            for (int i = 0; i < importResult.Data.Count; i++)
            {
                var order = importResult.Data[i];
                order.RowNumber = i + 2; // Excel row number (1-based + header)

                // Validate user email
                if (!string.IsNullOrEmpty(order.UserEmail) && invalidUserEmails.Contains(order.UserEmail))
                {
                    importResult.Errors.Add(new ExcelValidationErrorDTO(
                        order.RowNumber, 
                        "User Email", 
                        $"User with email '{order.UserEmail}' not found", 
                        ExcelErrorCodes.FOREIGN_KEY_NOT_FOUND));
                }

                // Validate customer email
                if (!string.IsNullOrEmpty(order.CustomerEmail) && invalidCustomerEmails.Contains(order.CustomerEmail))
                {
                    importResult.Errors.Add(new ExcelValidationErrorDTO(
                        order.RowNumber, 
                        "Customer Email", 
                        $"Customer with email '{order.CustomerEmail}' not found", 
                        ExcelErrorCodes.FOREIGN_KEY_NOT_FOUND));
                }

                // Validate order status
                if (!ValidOrderStatuses.Contains(order.OrderStatus))
                {
                    importResult.Errors.Add(new ExcelValidationErrorDTO(
                        order.RowNumber, 
                        "Order Status", 
                        $"Invalid order status '{order.OrderStatus}'. Valid values: {string.Join(", ", ValidOrderStatuses)}", 
                        ExcelErrorCodes.INVALID_VALUE));
                }

                // Additional business validation
                order.Validate();
                foreach (var error in order.ValidationErrors)
                {
                    importResult.Errors.Add(new ExcelValidationErrorDTO(
                        order.RowNumber, 
                        "", 
                        error, 
                        ExcelErrorCodes.BUSINESS_RULE_VIOLATION));
                }
            }

            // Update counts
            importResult.ErrorRows = importResult.Errors.Count;
            importResult.SuccessfulRows = importResult.TotalRows - importResult.ErrorRows;
        }

        private async Task SaveOrdersAsync(ExcelImportResultDTO<OrderExcelDTO> importResult)
        {
            if (!importResult.IsSuccess) return;

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                foreach (var orderDto in importResult.Data)
                {
                    if (orderDto.Id.HasValue && orderDto.Id.Value > 0)
                    {
                        // Update existing order
                        var existingOrder = await _unitOfWork.Orders.GetByIdAsync(orderDto.Id.Value);
                        if (existingOrder != null)
                        {
                            _mapper.Map(orderDto, existingOrder);
                            existingOrder.UpdatedAt = DateTime.UtcNow;
                            await _unitOfWork.Orders.UpdateAsync(existingOrder);
                        }
                    }
                    else
                    {
                        // Create new order
                        var newOrder = _mapper.Map<Order>(orderDto);
                        newOrder.CreatedAt = DateTime.UtcNow;
                        newOrder.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.Orders.CreateAsync(newOrder);

                        // Create order items
                        var orderItems = orderDto.ParseOrderItems();
                        foreach (var itemDto in orderItems)
                        {
                            var orderItem = new OrderItem
                            {
                                Order = newOrder,
                                ProductId = itemDto.ProductId,
                                Quantity = itemDto.Quantity,
                                UnitPrice = itemDto.UnitPrice
                            };
                            _unitOfWork.Orders.AddOrderItem(orderItem);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully imported {Count} orders", importResult.Data.Count);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error saving orders during import");
                
                importResult.Errors.Add(new ExcelValidationErrorDTO(
                    0, 
                    "", 
                    $"Database error: {ex.Message}", 
                    ExcelErrorCodes.BUSINESS_RULE_VIOLATION, 
                    ExcelErrorSeverity.Critical));
                
                throw;
            }
        }

        private async Task<List<OrderExcelDTO>> MapToExcelDtosAsync(IEnumerable<Order> orders)
        {
            var result = new List<OrderExcelDTO>();
            
            foreach (var order in orders)
            {
                var dto = _mapper.Map<OrderExcelDTO>(order);
                
                // Format order items
                if (order.OrderItems?.Any() == true)
                {
                    var itemDtos = order.OrderItems.Select(oi => new OrderItemExcelDTO
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product?.Name ?? "Unknown",
                        ProductSKU = oi.Product?.SKU ?? "",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    });
                    dto.OrderItems = OrderExcelDTO.FormatOrderItems(itemDtos);
                }
                
                result.Add(dto);
            }

            return await Task.FromResult(result);
        }

        private async Task<List<string>> GetInvalidUserEmailsAsync(IEnumerable<string> userEmails)
        {
            // This would need to be implemented based on your User repository
            // For now, return empty list (assuming all emails are valid)
            return await Task.FromResult(new List<string>());
        }

        private async Task<List<string>> GetInvalidCustomerEmailsAsync(IEnumerable<string> customerEmails)
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            var existingEmails = customers.Select(c => c.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);
            return customerEmails.Where(email => !existingEmails.Contains(email)).ToList();
        }

        #endregion
    }
}
