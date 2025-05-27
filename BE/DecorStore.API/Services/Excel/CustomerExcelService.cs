using AutoMapper;
using DecorStore.API.DTOs.Excel;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;
using DecorStore.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DecorStore.API.Services.Excel
{
    /// <summary>
    /// Service for Customer Excel import/export operations
    /// </summary>
    public class CustomerExcelService : ICustomerExcelService
    {
        private readonly IExcelService _excelService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerExcelService> _logger;

        public CustomerExcelService(
            IExcelService excelService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CustomerExcelService> logger)
        {
            _excelService = excelService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Imports customers from Excel file
        /// </summary>
        public async Task<ExcelImportResultDTO<CustomerExcelDTO>> ImportCustomersAsync(Stream fileStream, bool validateOnly = false)
        {
            var columnMappings = CustomerExcelDTO.GetImportColumnMappings();
            var importResult = await _excelService.ReadExcelAsync<CustomerExcelDTO>(fileStream, columnMappings);

            if (!importResult.IsSuccess)
            {
                return importResult;
            }

            // Additional business validation
            await ValidateBusinessRulesAsync(importResult);

            if (validateOnly || !importResult.IsSuccess)
            {
                return importResult;
            }

            // Save to database
            await SaveCustomersAsync(importResult);

            return importResult;
        }

        /// <summary>
        /// Exports customers to Excel file
        /// </summary>
        public async Task<byte[]> ExportCustomersAsync(CustomerFilterDTO? filter = null, ExcelExportRequestDTO? exportRequest = null)
        {
            filter ??= new CustomerFilterDTO();
            exportRequest ??= new ExcelExportRequestDTO { WorksheetName = "Customers" };

            // Get customers from database
            var customers = await _unitOfWork.Customers.GetAllAsync();

            // Map to Excel DTOs
            var customerExcelDtos = await MapToExcelDtosAsync(customers);

            // Calculate metrics
            customerExcelDtos = await CalculateCustomerMetricsAsync(customerExcelDtos);

            // Get column mappings
            var columnMappings = CustomerExcelDTO.GetColumnMappings();

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

            return await _excelService.CreateExcelAsync(customerExcelDtos, columnMappings, exportRequest.WorksheetName);
        }

        /// <summary>
        /// Creates Excel template for customer import
        /// </summary>
        public async Task<byte[]> CreateCustomerTemplateAsync(bool includeExampleRow = true)
        {
            var columnMappings = CustomerExcelDTO.GetImportColumnMappings();
            return await _excelService.CreateTemplateAsync(columnMappings, "Customer Import Template", includeExampleRow);
        }

        /// <summary>
        /// Validates customer Excel file without importing
        /// </summary>
        public async Task<ExcelValidationResultDTO> ValidateCustomerExcelAsync(Stream fileStream)
        {
            var expectedColumns = CustomerExcelDTO.GetImportColumnMappings().Values;
            return await _excelService.ValidateExcelFileAsync(fileStream, expectedColumns);
        }

        /// <summary>
        /// Imports customers in batches for large files
        /// </summary>
        public async IAsyncEnumerable<ExcelImportResultDTO<CustomerExcelDTO>> ImportCustomersInBatchesAsync(
            Stream fileStream,
            int batchSize = 100,
            IProgress<int>? progressCallback = null)
        {
            var columnMappings = CustomerExcelDTO.GetImportColumnMappings();

            await foreach (var chunk in _excelService.ReadExcelInChunksAsync<CustomerExcelDTO>(fileStream, columnMappings, batchSize, progressCallback))
            {
                var batchResult = new ExcelImportResultDTO<CustomerExcelDTO>
                {
                    Data = chunk.Data,
                    Errors = chunk.Errors,
                    TotalRows = chunk.Data.Count,
                    SuccessfulRows = chunk.Data.Count,
                    ErrorRows = chunk.Errors.Count
                };

                // Validate business rules for this batch
                await ValidateBusinessRulesAsync(batchResult);

                // Save this batch if no errors
                if (batchResult.IsSuccess)
                {
                    await SaveCustomersAsync(batchResult);
                }

                yield return batchResult;
            }
        }

        /// <summary>
        /// Gets customer import statistics
        /// </summary>
        public async Task<CustomerImportStatisticsDTO> GetImportStatisticsAsync(Stream fileStream)
        {
            var validation = await ValidateCustomerExcelAsync(fileStream);
            var columnMappings = CustomerExcelDTO.GetImportColumnMappings();
            var importResult = await _excelService.ReadExcelAsync<CustomerExcelDTO>(fileStream, columnMappings);

            var statistics = new CustomerImportStatisticsDTO
            {
                TotalRows = importResult.TotalRows,
                ErrorRows = importResult.ErrorRows,
                FileSizeBytes = fileStream.Length,
                EstimatedProcessingTime = TimeSpan.FromSeconds(importResult.TotalRows * 0.03) // Fastest processing
            };

            // Analyze data
            var existingCustomerIds = importResult.Data
                .Where(c => c.Id.HasValue)
                .Select(c => c.Id!.Value)
                .ToList();

            if (existingCustomerIds.Any())
            {
                var existingCustomers = await _unitOfWork.Customers.GetAllAsync();
                var existingIds = existingCustomers.Where(c => existingCustomerIds.Contains(c.Id)).Select(c => c.Id).ToHashSet();

                statistics.UpdatedCustomers = existingIds.Count;
                statistics.NewCustomers = importResult.Data.Count - statistics.UpdatedCustomers;
            }
            else
            {
                statistics.NewCustomers = importResult.Data.Count;
            }

            // Geographic analysis
            statistics.Countries = importResult.Data
                .Where(c => !string.IsNullOrEmpty(c.Country))
                .Select(c => c.Country!)
                .Distinct()
                .ToList();

            statistics.States = importResult.Data
                .Where(c => !string.IsNullOrEmpty(c.State))
                .Select(c => c.State!)
                .Distinct()
                .ToList();

            statistics.Cities = importResult.Data
                .Where(c => !string.IsNullOrEmpty(c.City))
                .Select(c => c.City!)
                .Distinct()
                .ToList();

            // Geographic distribution
            statistics.GeographicDistribution = importResult.Data
                .Where(c => !string.IsNullOrEmpty(c.Country))
                .GroupBy(c => c.Country!)
                .ToDictionary(g => g.Key, g => g.Count());

            // Check for duplicate emails
            var emails = importResult.Data.Select(c => c.Email).Where(e => !string.IsNullOrEmpty(e)).ToList();
            statistics.DuplicateEmails = await ValidateEmailUniquenessAsync(emails);

            // Validate email and phone formats
            statistics.InvalidEmails = importResult.Data
                .Where(c => !string.IsNullOrEmpty(c.Email) && !IsValidEmail(c.Email))
                .Select(c => c.Email)
                .ToList();

            statistics.InvalidPhones = importResult.Data
                .Where(c => !string.IsNullOrEmpty(c.Phone) && !IsValidPhoneNumber(c.Phone))
                .Select(c => c.Phone!)
                .ToList();

            return statistics;
        }

        /// <summary>
        /// Validates email uniqueness
        /// </summary>
        public async Task<List<string>> ValidateEmailUniquenessAsync(IEnumerable<string> emails, IEnumerable<int>? excludeCustomerIds = null)
        {
            var emailList = emails.Where(e => !string.IsNullOrEmpty(e)).ToList();
            if (!emailList.Any()) return new List<string>();

            var existingCustomers = await _unitOfWork.Customers.GetAllAsync();
            var existingEmails = existingCustomers
                .Where(c => excludeCustomerIds == null || !excludeCustomerIds.Contains(c.Id))
                .Select(c => c.Email)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return emailList.Where(email => existingEmails.Contains(email)).ToList();
        }

        /// <summary>
        /// Calculates customer metrics for export
        /// </summary>
        public async Task<List<CustomerExcelDTO>> CalculateCustomerMetricsAsync(List<CustomerExcelDTO> customers)
        {
            foreach (var customer in customers)
            {
                customer.CalculateMetrics();
            }

            return await Task.FromResult(customers);
        }

        /// <summary>
        /// Segments customers based on their order history
        /// </summary>
        public async Task<List<CustomerExcelDTO>> SegmentCustomersAsync(List<CustomerExcelDTO> customers)
        {
            foreach (var customer in customers)
            {
                customer.CalculateMetrics(); // This includes segmentation logic
            }

            return await Task.FromResult(customers);
        }

        #region Private Methods

        private async Task ValidateBusinessRulesAsync(ExcelImportResultDTO<CustomerExcelDTO> importResult)
        {
            // Validate email uniqueness
            var emails = importResult.Data.Select(c => c.Email).Where(e => !string.IsNullOrEmpty(e)).ToList();
            var excludeIds = importResult.Data.Where(c => c.Id.HasValue).Select(c => c.Id!.Value).ToList();
            var duplicateEmails = await ValidateEmailUniquenessAsync(emails, excludeIds);

            // Process each customer
            for (int i = 0; i < importResult.Data.Count; i++)
            {
                var customer = importResult.Data[i];
                customer.RowNumber = i + 2; // Excel row number (1-based + header)

                // Validate email uniqueness
                if (duplicateEmails.Contains(customer.Email))
                {
                    importResult.Errors.Add(new ExcelValidationErrorDTO(
                        customer.RowNumber,
                        "Email",
                        $"Email '{customer.Email}' already exists",
                        ExcelErrorCodes.DUPLICATE_VALUE));
                }

                // Additional business validation
                customer.Validate();
                foreach (var error in customer.ValidationErrors)
                {
                    importResult.Errors.Add(new ExcelValidationErrorDTO(
                        customer.RowNumber,
                        "",
                        error,
                        ExcelErrorCodes.BUSINESS_RULE_VIOLATION));
                }
            }

            // Update counts
            importResult.ErrorRows = importResult.Errors.Count;
            importResult.SuccessfulRows = importResult.TotalRows - importResult.ErrorRows;
        }

        private async Task SaveCustomersAsync(ExcelImportResultDTO<CustomerExcelDTO> importResult)
        {
            if (!importResult.IsSuccess) return;

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                foreach (var customerDto in importResult.Data)
                {
                    if (customerDto.Id.HasValue && customerDto.Id.Value > 0)
                    {
                        // Update existing customer
                        var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(customerDto.Id.Value);
                        if (existingCustomer != null)
                        {
                            _mapper.Map(customerDto, existingCustomer);
                            existingCustomer.UpdatedAt = DateTime.UtcNow;
                            await _unitOfWork.Customers.UpdateAsync(existingCustomer);
                        }
                    }
                    else
                    {
                        // Create new customer
                        var newCustomer = _mapper.Map<Customer>(customerDto);
                        newCustomer.CreatedAt = DateTime.UtcNow;
                        newCustomer.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.Customers.CreateAsync(newCustomer);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully imported {Count} customers", importResult.Data.Count);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error saving customers during import");

                importResult.Errors.Add(new ExcelValidationErrorDTO(
                    0,
                    "",
                    $"Database error: {ex.Message}",
                    ExcelErrorCodes.BUSINESS_RULE_VIOLATION,
                    ExcelErrorSeverity.Critical));

                throw;
            }
        }

        private async Task<List<CustomerExcelDTO>> MapToExcelDtosAsync(IEnumerable<Customer> customers)
        {
            var result = new List<CustomerExcelDTO>();

            foreach (var customer in customers)
            {
                var dto = _mapper.Map<CustomerExcelDTO>(customer);

                // Calculate metrics
                dto.CalculateMetrics();

                result.Add(dto);
            }

            return await Task.FromResult(result);
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true;

            // Remove common phone number characters
            var cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");

            // Check if remaining characters are digits and length is reasonable
            return cleanPhone.All(char.IsDigit) && cleanPhone.Length >= 7 && cleanPhone.Length <= 15;
        }

        #endregion
    }
}
