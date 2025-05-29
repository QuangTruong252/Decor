using AutoMapper;
using DecorStore.API.DTOs.Excel;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;
using DecorStore.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DecorStore.API.Services.Excel
{
    /// <summary>
    /// Service for Product Excel import/export operations
    /// </summary>
    public class ProductExcelService : IProductExcelService
    {
        private readonly IExcelService _excelService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductExcelService> _logger;

        public ProductExcelService(
            IExcelService excelService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ProductExcelService> logger)
        {
            _excelService = excelService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Imports products from Excel file
        /// </summary>
        public async Task<ExcelImportResultDTO<ProductExcelDTO>> ImportProductsAsync(Stream fileStream, bool validateOnly = false)
        {
            var columnMappings = ProductExcelDTO.GetImportColumnMappings();
            var importResult = await _excelService.ReadExcelAsync<ProductExcelDTO>(fileStream, columnMappings);

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
            await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () =>
            {
                await SaveProductsAsync(importResult);
                return true;
            });

            return importResult;
        }

        /// <summary>
        /// Exports products to Excel file
        /// </summary>
        public async Task<byte[]> ExportProductsAsync(ProductFilterDTO? filter = null, ExcelExportRequestDTO? exportRequest = null)
        {
            filter ??= new ProductFilterDTO { PageNumber = 1, PageSize = int.MaxValue };
            exportRequest ??= new ExcelExportRequestDTO { WorksheetName = "Products" };
            var products = await _unitOfWork.Products.GetAllAsync(filter);

            // Map to Excel DTOs
            var productExcelDtos = await MapToExcelDtosAsync(products);

            // Get column mappings
            var columnMappings = ProductExcelDTO.GetColumnMappings();

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

            return await _excelService.CreateExcelAsync(productExcelDtos, columnMappings, exportRequest.WorksheetName);
        }

        /// <summary>
        /// Creates Excel template for product import
        /// </summary>
        public async Task<byte[]> CreateProductTemplateAsync(bool includeExampleRow = true)
        {
            var columnMappings = ProductExcelDTO.GetImportColumnMappings();
            return await _excelService.CreateTemplateAsync(columnMappings, "Product Import Template", includeExampleRow);
        }

        /// <summary>
        /// Validates product Excel file without importing
        /// </summary>
        public async Task<ExcelValidationResultDTO> ValidateProductExcelAsync(Stream fileStream)
        {
            var expectedColumns = ProductExcelDTO.GetImportColumnMappings().Values;
            return await _excelService.ValidateExcelFileAsync(fileStream, expectedColumns);
        }

        /// <summary>
        /// Imports products in batches for large files
        /// </summary>
        public async IAsyncEnumerable<ExcelImportResultDTO<ProductExcelDTO>> ImportProductsInBatchesAsync(
            Stream fileStream,
            int batchSize = 100,
            IProgress<int>? progressCallback = null)
        {
            var columnMappings = ProductExcelDTO.GetImportColumnMappings();

            await foreach (var chunk in _excelService.ReadExcelInChunksAsync<ProductExcelDTO>(fileStream, columnMappings, batchSize, progressCallback))
            {
                var batchResult = new ExcelImportResultDTO<ProductExcelDTO>
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
                    await SaveProductsAsync(batchResult);
                }

                yield return batchResult;
            }
        }

        /// <summary>
        /// Gets product import statistics
        /// </summary>
        public async Task<ProductImportStatisticsDTO> GetImportStatisticsAsync(Stream fileStream)
        {
            var validation = await ValidateProductExcelAsync(fileStream);
            var columnMappings = ProductExcelDTO.GetImportColumnMappings();
            var importResult = await _excelService.ReadExcelAsync<ProductExcelDTO>(fileStream, columnMappings);

            var statistics = new ProductImportStatisticsDTO
            {
                TotalRows = importResult.TotalRows,
                ErrorRows = importResult.ErrorRows,
                FileSizeBytes = fileStream.Length,
                EstimatedProcessingTime = TimeSpan.FromSeconds(importResult.TotalRows * 0.1) // Rough estimate
            };

            // Analyze data
            var existingProductIds = importResult.Data
                .Where(p => p.Id.HasValue)
                .Select(p => p.Id!.Value)
                .ToList();

            if (existingProductIds.Any())
            {
                var existingProducts = await _unitOfWork.Products.GetAllAsync(new ProductFilterDTO { PageNumber = 1, PageSize = int.MaxValue });
                var existingIds = existingProducts.Where(p => existingProductIds.Contains(p.Id)).Select(p => p.Id).ToHashSet();

                statistics.UpdatedProducts = existingIds.Count;
                statistics.NewProducts = importResult.Data.Count - statistics.UpdatedProducts;
            }
            else
            {
                statistics.NewProducts = importResult.Data.Count;
            }

            // Get unique categories
            statistics.Categories = importResult.Data
                .Where(p => !string.IsNullOrEmpty(p.CategoryName))
                .Select(p => p.CategoryName!)
                .Distinct()
                .ToList();

            // Check for duplicate SKUs
            var skus = importResult.Data.Select(p => p.SKU).Where(s => !string.IsNullOrEmpty(s)).ToList();
            statistics.DuplicateSkus = await ValidateSkuUniquenessAsync(skus);

            return statistics;
        }

        /// <summary>
        /// Resolves category names to category IDs
        /// </summary>
        public async Task<Dictionary<string, int>> ResolveCategoryNamesAsync(IEnumerable<string> categoryNames)
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return categories
                .Where(c => categoryNames.Contains(c.Name, StringComparer.OrdinalIgnoreCase))
                .ToDictionary(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates SKU uniqueness
        /// </summary>
        public async Task<List<string>> ValidateSkuUniquenessAsync(IEnumerable<string> skus, IEnumerable<int>? excludeProductIds = null)
        {
            var skuList = skus.Where(s => !string.IsNullOrEmpty(s)).ToList();
            if (!skuList.Any()) return new List<string>();

            var existingProducts = await _unitOfWork.Products.GetAllAsync(new ProductFilterDTO { PageNumber = 1, PageSize = int.MaxValue });
            var existingSkus = existingProducts
                .Where(p => excludeProductIds == null || !excludeProductIds.Contains(p.Id))
                .Select(p => p.SKU)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return skuList.Where(sku => existingSkus.Contains(sku)).ToList();
        }

        #region Private Methods

        private async Task ValidateBusinessRulesAsync(ExcelImportResultDTO<ProductExcelDTO> importResult)
        {
            // Resolve category names to IDs
            var categoryNames = importResult.Data
                .Where(p => !string.IsNullOrEmpty(p.CategoryName) && p.CategoryId <= 0)
                .Select(p => p.CategoryName!)
                .Distinct()
                .ToList();

            var categoryMappings = await ResolveCategoryNamesAsync(categoryNames);

            // Validate SKU uniqueness
            var skus = importResult.Data.Select(p => p.SKU).Where(s => !string.IsNullOrEmpty(s)).ToList();
            var excludeIds = importResult.Data.Where(p => p.Id.HasValue).Select(p => p.Id!.Value).ToList();
            var duplicateSkus = await ValidateSkuUniquenessAsync(skus, excludeIds);

            // Process each product
            for (int i = 0; i < importResult.Data.Count; i++)
            {
                var product = importResult.Data[i];
                product.RowNumber = i + 2; // Excel row number (1-based + header)

                // Validate and set category ID
                if (product.CategoryId <= 0 && !string.IsNullOrEmpty(product.CategoryName))
                {
                    if (categoryMappings.TryGetValue(product.CategoryName, out var categoryId))
                    {
                        product.CategoryId = categoryId;
                    }
                    else
                    {
                        importResult.Errors.Add(new ExcelValidationErrorDTO(
                            product.RowNumber,
                            "Category Name",
                            $"Category '{product.CategoryName}' not found",
                            ExcelErrorCodes.FOREIGN_KEY_NOT_FOUND));
                    }
                }

                // Validate SKU uniqueness
                if (duplicateSkus.Contains(product.SKU))
                {
                    importResult.Errors.Add(new ExcelValidationErrorDTO(
                        product.RowNumber,
                        "SKU",
                        $"SKU '{product.SKU}' already exists",
                        ExcelErrorCodes.DUPLICATE_VALUE));
                }

                // Additional business validation
                product.Validate();
                foreach (var error in product.ValidationErrors)
                {
                    importResult.Errors.Add(new ExcelValidationErrorDTO(
                        product.RowNumber,
                        "",
                        error,
                        ExcelErrorCodes.BUSINESS_RULE_VIOLATION));
                }
            }

            // Update counts
            importResult.ErrorRows = importResult.Errors.Count;
            importResult.SuccessfulRows = importResult.TotalRows - importResult.ErrorRows;
        }

        private async Task SaveProductsAsync(ExcelImportResultDTO<ProductExcelDTO> importResult)
        {
            if (!importResult.IsSuccess) return;

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                foreach (var productDto in importResult.Data)
                {
                    if (productDto.Id.HasValue && productDto.Id.Value > 0)
                    {
                        // Update existing product
                        var existingProduct = await _unitOfWork.Products.GetByIdAsync(productDto.Id.Value);
                        if (existingProduct != null)
                        {
                            _mapper.Map(productDto, existingProduct);
                            existingProduct.UpdatedAt = DateTime.UtcNow;
                            await _unitOfWork.Products.UpdateAsync(existingProduct);
                        }
                    }
                    else
                    {
                        // Create new product
                        var newProduct = _mapper.Map<Product>(productDto);
                        newProduct.CreatedAt = DateTime.UtcNow;
                        newProduct.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.Products.CreateAsync(newProduct);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully imported {Count} products", importResult.Data.Count);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error saving products during import");

                importResult.Errors.Add(new ExcelValidationErrorDTO(
                    0,
                    "",
                    $"Database error: {ex.Message}",
                    ExcelErrorCodes.BUSINESS_RULE_VIOLATION,
                    ExcelErrorSeverity.Critical));

                throw;
            }
        }

        private async Task<List<ProductExcelDTO>> MapToExcelDtosAsync(IEnumerable<Product> products)
        {
            var result = new List<ProductExcelDTO>();

            foreach (var product in products)
            {
                var dto = _mapper.Map<ProductExcelDTO>(product);

                // Add calculated fields
                dto.CategoryName = product.Category?.Name;
                dto.ImageUrls = product.Images?.Any() == true
                    ? string.Join(",", product.Images.Select(i => i.FilePath))
                    : null;
                dto.ReviewCount = product.Reviews?.Count ?? 0;
                dto.AverageRating = product.AverageRating;

                result.Add(dto);
            }

            return await Task.FromResult(result);
        }

        #endregion
    }
}
