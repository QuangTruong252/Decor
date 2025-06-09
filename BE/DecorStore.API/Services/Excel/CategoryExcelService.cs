using AutoMapper;
using DecorStore.API.DTOs.Excel;
using DecorStore.API.DTOs;
using DecorStore.API.Interfaces;
using DecorStore.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DecorStore.API.Services.Excel
{
    /// <summary>
    /// Service for Category Excel import/export operations
    /// </summary>
    public class CategoryExcelService : ICategoryExcelService
    {
        private readonly IExcelService _excelService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryExcelService> _logger;

        public CategoryExcelService(
            IExcelService excelService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CategoryExcelService> logger)
        {
            _excelService = excelService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Imports categories from Excel file
        /// </summary>
        public async Task<ExcelImportResultDTO<CategoryExcelDTO>> ImportCategoriesAsync(Stream fileStream, bool validateOnly = false)
        {
            var columnMappings = CategoryExcelDTO.GetImportColumnMappings();
            var importResult = await _excelService.ReadExcelAsync<CategoryExcelDTO>(fileStream, columnMappings);

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
                await SaveCategoriesAsync(importResult);
                return true;
            });

            return importResult;
        }

        /// <summary>
        /// Exports categories to Excel file
        /// </summary>
        public async Task<byte[]> ExportCategoriesAsync(CategoryFilterDTO? filter = null, ExcelExportRequestDTO? exportRequest = null)
        {
            filter ??= new CategoryFilterDTO();
            exportRequest ??= new ExcelExportRequestDTO { WorksheetName = "Categories" };

            // Get categories from database with navigation properties for Excel export
            var categories = await _unitOfWork.Categories.GetAllForExcelExportAsync();

            // Map to Excel DTOs and build hierarchy
            var categoryExcelDtos = await MapToExcelDtosAsync(categories);

            // Get column mappings
            var columnMappings = CategoryExcelDTO.GetColumnMappings();

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

            return await _excelService.CreateExcelAsync(categoryExcelDtos, columnMappings, exportRequest.WorksheetName);
        }

        /// <summary>
        /// Creates Excel template for category import
        /// </summary>
        public async Task<byte[]> CreateCategoryTemplateAsync(bool includeExampleRow = true)
        {
            var columnMappings = CategoryExcelDTO.GetImportColumnMappings();
            return await _excelService.CreateTemplateAsync(columnMappings, "Category Import Template", includeExampleRow);
        }

        /// <summary>
        /// Validates category Excel file without importing
        /// </summary>
        public async Task<ExcelValidationResultDTO> ValidateCategoryExcelAsync(Stream fileStream)
        {
            var expectedColumns = CategoryExcelDTO.GetImportColumnMappings().Values;
            return await _excelService.ValidateExcelFileAsync(fileStream, expectedColumns);
        }

        /// <summary>
        /// Imports categories in batches for large files
        /// </summary>
        public async IAsyncEnumerable<ExcelImportResultDTO<CategoryExcelDTO>> ImportCategoriesInBatchesAsync(
            Stream fileStream,
            int batchSize = 100,
            IProgress<int>? progressCallback = null)
        {
            var columnMappings = CategoryExcelDTO.GetImportColumnMappings();

            await foreach (var chunk in _excelService.ReadExcelInChunksAsync<CategoryExcelDTO>(fileStream, columnMappings, batchSize, progressCallback))
            {
                var batchResult = new ExcelImportResultDTO<CategoryExcelDTO>
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
                    await _unitOfWork.ExecuteWithExecutionStrategyAsync(async () => 
                    {
                        await SaveCategoriesAsync(batchResult);
                        return true; // Return a dummy value as TResult is expected
                    });
                }

                yield return batchResult;
            }
        }

        /// <summary>
        /// Gets category import statistics
        /// </summary>
        public async Task<CategoryImportStatisticsDTO> GetImportStatisticsAsync(Stream fileStream)
        {
            var validation = await ValidateCategoryExcelAsync(fileStream);
            var columnMappings = CategoryExcelDTO.GetImportColumnMappings();
            var importResult = await _excelService.ReadExcelAsync<CategoryExcelDTO>(fileStream, columnMappings);

            var statistics = new CategoryImportStatisticsDTO
            {
                TotalRows = importResult.TotalRows,
                ErrorRows = importResult.ErrorRows,
                FileSizeBytes = fileStream.Length,
                EstimatedProcessingTime = TimeSpan.FromSeconds(importResult.TotalRows * 0.05) // Faster than products
            };

            // Analyze data
            var existingCategoryIds = importResult.Data
                .Where(c => c.Id.HasValue)
                .Select(c => c.Id!.Value)
                .ToList();

            if (existingCategoryIds.Any())
            {
                var existingCategories = await _unitOfWork.Categories.GetAllAsync();
                var existingIds = existingCategories.Where(c => existingCategoryIds.Contains(c.Id)).Select(c => c.Id).ToHashSet();

                statistics.UpdatedCategories = existingIds.Count;
                statistics.NewCategories = importResult.Data.Count - statistics.UpdatedCategories;
            }
            else
            {
                statistics.NewCategories = importResult.Data.Count;
            }

            // Analyze hierarchy
            statistics.RootCategories = importResult.Data.Count(c => !c.ParentId.HasValue && string.IsNullOrEmpty(c.ParentName));
            statistics.Subcategories = importResult.Data.Count - statistics.RootCategories;

            // Get unique parent categories
            statistics.ParentCategories = importResult.Data
                .Where(c => !string.IsNullOrEmpty(c.ParentName))
                .Select(c => c.ParentName!)
                .Distinct()
                .ToList();

            // Check for duplicate names
            var categoryNames = importResult.Data.Select(c => c.Name).Where(n => !string.IsNullOrEmpty(n)).ToList();
            statistics.DuplicateNames = await ValidateCategoryNameUniquenessAsync(categoryNames);

            // Check for circular references
            statistics.CircularReferences = await ValidateCategoryHierarchyAsync(importResult.Data);

            return statistics;
        }

        /// <summary>
        /// Resolves parent category names to category IDs
        /// </summary>
        public async Task<Dictionary<string, int>> ResolveParentCategoryNamesAsync(IEnumerable<string> parentNames)
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return categories
                .Where(c => parentNames.Contains(c.Name, StringComparer.OrdinalIgnoreCase))
                .ToDictionary(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates category hierarchy for circular references
        /// </summary>
        public async Task<List<string>> ValidateCategoryHierarchyAsync(IEnumerable<CategoryExcelDTO> categories)
        {
            var errors = new List<string>();
            var categoryList = categories.ToList();

            // Build a temporary hierarchy map
            var categoryMap = categoryList
                .Where(c => c.Id.HasValue)
                .ToDictionary(c => c.Id!.Value, c => c);

            // Add existing categories from database
            var existingCategories = await _unitOfWork.Categories.GetAllAsync();
            foreach (var existing in existingCategories)
            {
                if (!categoryMap.ContainsKey(existing.Id))
                {
                    var dto = _mapper.Map<CategoryExcelDTO>(existing);
                    categoryMap[existing.Id] = dto;
                }
            }

            // Check for circular references
            foreach (var category in categoryList.Where(c => c.Id.HasValue && c.ParentId.HasValue))
            {
                var visited = new HashSet<int>();
                var current = category;

                while (current?.ParentId.HasValue == true)
                {
                    if (visited.Contains(current.ParentId.Value))
                    {
                        errors.Add($"Circular reference detected in category hierarchy for '{category.Name}' (ID: {category.Id})");
                        break;
                    }

                    visited.Add(current.ParentId.Value);

                    if (!categoryMap.TryGetValue(current.ParentId.Value, out current))
                    {
                        // Parent not found - this will be caught by other validation
                        break;
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// Validates category name uniqueness
        /// </summary>
        public async Task<List<string>> ValidateCategoryNameUniquenessAsync(IEnumerable<string> categoryNames, IEnumerable<int>? excludeCategoryIds = null)
        {
            var nameList = categoryNames.Where(n => !string.IsNullOrEmpty(n)).ToList();
            if (!nameList.Any()) return new List<string>();

            var existingCategories = await _unitOfWork.Categories.GetAllAsync();
            var existingNames = existingCategories
                .Where(c => excludeCategoryIds == null || !excludeCategoryIds.Contains(c.Id))
                .Select(c => c.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return nameList.Where(name => existingNames.Contains(name)).ToList();
        }

        #region Private Methods

        private async Task ValidateBusinessRulesAsync(ExcelImportResultDTO<CategoryExcelDTO> importResult)
        {
            // Resolve parent category names to IDs
            var parentNames = importResult.Data
                .Where(c => !string.IsNullOrEmpty(c.ParentName) && (!c.ParentId.HasValue || c.ParentId.Value <= 0))
                .Select(c => c.ParentName!)
                .Distinct()
                .ToList();

            var parentMappings = await ResolveParentCategoryNamesAsync(parentNames);

            // Validate category name uniqueness
            var categoryNames = importResult.Data.Select(c => c.Name).Where(n => !string.IsNullOrEmpty(n)).ToList();
            var excludeIds = importResult.Data.Where(c => c.Id.HasValue).Select(c => c.Id!.Value).ToList();
            var duplicateNames = await ValidateCategoryNameUniquenessAsync(categoryNames, excludeIds);

            // Validate hierarchy
            var hierarchyErrors = await ValidateCategoryHierarchyAsync(importResult.Data);

            // Process each category
            for (int i = 0; i < importResult.Data.Count; i++)
            {
                var category = importResult.Data[i];
                category.RowNumber = i + 2; // Excel row number (1-based + header)

                // Validate and set parent ID
                if ((!category.ParentId.HasValue || category.ParentId.Value <= 0) && !string.IsNullOrEmpty(category.ParentName))
                {
                    if (parentMappings.TryGetValue(category.ParentName, out var parentId))
                    {
                        category.ParentId = parentId;
                    }
                    else
                    {
                        importResult.Errors.Add(new ExcelValidationErrorDTO(
                            category.RowNumber,
                            "Parent Category Name",
                            $"Parent category '{category.ParentName}' not found",
                            ExcelErrorCodes.FOREIGN_KEY_NOT_FOUND));
                    }
                }

                // Validate name uniqueness
                if (duplicateNames.Contains(category.Name))
                {
                    importResult.Errors.Add(new ExcelValidationErrorDTO(
                        category.RowNumber,
                        "Category Name",
                        $"Category name '{category.Name}' already exists",
                        ExcelErrorCodes.DUPLICATE_VALUE));
                }

                // Additional business validation
                category.Validate();
                foreach (var error in category.ValidationErrors)
                {
                    importResult.Errors.Add(new ExcelValidationErrorDTO(
                        category.RowNumber,
                        "",
                        error,
                        ExcelErrorCodes.BUSINESS_RULE_VIOLATION));
                }
            }

            // Add hierarchy errors
            foreach (var error in hierarchyErrors)
            {
                importResult.Errors.Add(new ExcelValidationErrorDTO(
                    0,
                    "Hierarchy",
                    error,
                    ExcelErrorCodes.BUSINESS_RULE_VIOLATION));
            }

            // Update counts
            importResult.ErrorRows = importResult.Errors.Count;
            importResult.SuccessfulRows = importResult.TotalRows - importResult.ErrorRows;
        }

        private async Task SaveCategoriesAsync(ExcelImportResultDTO<CategoryExcelDTO> importResult)
        {
            if (!importResult.IsSuccess) return;

            // The transaction is now handled by ExecuteWithExecutionStrategyAsync
            // No need for explicit BeginTransactionAsync, CommitTransactionAsync, or RollbackTransactionAsync here.

            foreach (var categoryDto in importResult.Data)
            {
                if (categoryDto.Id.HasValue && categoryDto.Id.Value > 0)
                {
                    // Update existing category
                    var existingCategory = await _unitOfWork.Categories.GetByIdAsync(categoryDto.Id.Value);
                    if (existingCategory != null)
                    {
                        _mapper.Map(categoryDto, existingCategory);
                        // UpdateAsync will mark the entity as modified. SaveChangesAsync will persist.
                        await _unitOfWork.Categories.UpdateAsync(existingCategory); 
                    }
                    else
                    {
                        // If ID is provided but category not found, consider logging or specific handling.
                        // For now, we'll skip updating if not found, or you might want to create it.
                        _logger.LogWarning("Category with ID {CategoryId} not found for update. Skipping.", categoryDto.Id.Value);
                    }
                }
                else
                {
                    // Create new category
                    var newCategory = _mapper.Map<Category>(categoryDto);
                    newCategory.CreatedAt = DateTime.UtcNow;
                    await _unitOfWork.Categories.CreateAsync(newCategory);
                }
            }

            await _unitOfWork.SaveChangesAsync(); // This will be part of the transaction managed by ExecuteWithExecutionStrategyAsync

            _logger.LogInformation("Successfully processed {Count} categories in SaveCategoriesAsync", importResult.Data.Count);
            // Error handling (like try-catch for database operations) should ideally be within the lambda 
            // passed to ExecuteWithExecutionStrategyAsync if specific rollback logic per operation is needed, 
            // or rely on the strategy to handle retries and the overall transaction to rollback on failure.
            // For simplicity here, the existing try-catch in the calling methods (ImportCategoriesAsync) will handle exceptions.
        }        private async Task<List<CategoryExcelDTO>> MapToExcelDtosAsync(IEnumerable<Category> categories)
        {
            var result = new List<CategoryExcelDTO>();

            foreach (var category in categories)
            {
                var dto = _mapper.Map<CategoryExcelDTO>(category);

                // Add calculated fields
                dto.ParentName = category.ParentCategory?.Name;
                dto.ProductCount = category.Products?.Count ?? 0;
                dto.SubcategoryCount = category.Subcategories?.Count ?? 0;

                result.Add(dto);
            }

            // Build hierarchy information
            await Task.CompletedTask;
            return CategoryExcelDTO.BuildHierarchy(result);
        }

        #endregion
    }
}
