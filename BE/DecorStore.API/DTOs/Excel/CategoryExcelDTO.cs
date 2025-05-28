using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs.Excel
{
    /// <summary>
    /// DTO for Category Excel import/export operations
    /// </summary>
    public class CategoryExcelDTO
    {
        /// <summary>
        /// Category ID (for updates, leave empty for new categories)
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Category slug (URL-friendly name)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Category description
        /// </summary>
        [StringLength(255)]
        public string? Description { get; set; }

        /// <summary>
        /// Parent category ID (null for root categories)
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Parent category name (for reference, will be used to lookup ParentId if ParentId is not provided)
        /// </summary>
        public string? ParentName { get; set; }

        /// <summary>
        /// Image URL for the category
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Sort order for category display
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Creation date (read-only for export)
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Number of products in this category (read-only for export)
        /// </summary>
        public int ProductCount { get; set; }

        /// <summary>
        /// Number of subcategories (read-only for export)
        /// </summary>
        public int SubcategoryCount { get; set; }

        /// <summary>
        /// Category level in hierarchy (0 = root, 1 = first level, etc.) (read-only for export)
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Full category path (e.g., "Furniture > Living Room > Sofas") (read-only for export)
        /// </summary>
        public string? CategoryPath { get; set; }

        /// <summary>
        /// Whether this category is active (read-only for export)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Total revenue from products in this category (read-only for export)
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Average product price in this category (read-only for export)
        /// </summary>
        public decimal AverageProductPrice { get; set; }

        /// <summary>
        /// Validation errors for this row (used during import)
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new List<string>();

        /// <summary>
        /// Row number in the Excel file (used during import)
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Whether this row has validation errors
        /// </summary>
        public bool HasErrors => ValidationErrors.Any();

        /// <summary>
        /// Gets the column mappings for Category Excel operations
        /// </summary>
        public static Dictionary<string, string> GetColumnMappings()
        {
            return new Dictionary<string, string>
            {
                { nameof(Id), "Category ID" },
                { nameof(Name), "Category Name" },
                { nameof(Slug), "URL Slug" },
                { nameof(Description), "Description" },
                { nameof(ParentId), "Parent Category ID" },
                { nameof(ParentName), "Parent Category Name" },
                { nameof(ImageUrl), "Image URL" },
                { nameof(CreatedAt), "Created Date" },
                { nameof(ProductCount), "Product Count" },
                { nameof(SubcategoryCount), "Subcategory Count" },
                { nameof(Level), "Category Level" },
                { nameof(CategoryPath), "Category Path" },
                { nameof(IsActive), "Active" },
                { nameof(TotalRevenue), "Total Revenue" },
                { nameof(AverageProductPrice), "Average Product Price" }
            };
        }

        /// <summary>
        /// Gets the import column mappings (excludes read-only fields)
        /// </summary>
        public static Dictionary<string, string> GetImportColumnMappings()
        {
            return new Dictionary<string, string>
            {
                { nameof(Id), "Category ID" },
                { nameof(Name), "Category Name" },
                { nameof(Slug), "URL Slug" },
                { nameof(Description), "Description" },
                { nameof(ParentId), "Parent Category ID" },
                { nameof(ParentName), "Parent Category Name" },
                { nameof(ImageUrl), "Image URL" }
            };
        }

        /// <summary>
        /// Gets example values for template generation
        /// </summary>
        public static Dictionary<string, object> GetExampleValues()
        {
            return new Dictionary<string, object>
            {
                { nameof(Name), "Table Lamps" },
                { nameof(Slug), "table-lamps" },
                { nameof(Description), "Modern and classic table lamps for home decoration" },
                { nameof(ParentId), 1 },
                { nameof(ParentName), "Lamps" },
                { nameof(ImageUrl), "https://example.com/category-image.jpg" }
            };
        }

        /// <summary>
        /// Validates the category data
        /// </summary>
        public void Validate()
        {
            ValidationErrors.Clear();

            if (string.IsNullOrWhiteSpace(Name))
                ValidationErrors.Add("Category name is required");
            else if (Name.Length < 2 || Name.Length > 100)
                ValidationErrors.Add("Category name must be between 2 and 100 characters");

            if (string.IsNullOrWhiteSpace(Slug))
                ValidationErrors.Add("Category slug is required");
            else if (Slug.Length > 100)
                ValidationErrors.Add("Category slug must not exceed 100 characters");

            if (!string.IsNullOrEmpty(Description) && Description.Length > 255)
                ValidationErrors.Add("Description must not exceed 255 characters");

            if (ParentId.HasValue && ParentId.Value <= 0)
                ValidationErrors.Add("Parent category ID must be greater than 0 if provided");

            if (ParentId.HasValue && Id.HasValue && ParentId.Value == Id.Value)
                ValidationErrors.Add("Category cannot be its own parent");

            if (!string.IsNullOrEmpty(ImageUrl) && !IsValidUrl(ImageUrl))
                ValidationErrors.Add("Image URL must be a valid URL");
        }

        /// <summary>
        /// Validates if a string is a valid URL
        /// </summary>
        private static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Gets the hierarchical structure for categories
        /// </summary>
        public static List<CategoryExcelDTO> BuildHierarchy(List<CategoryExcelDTO> categories)
        {
            // Create dictionary for quick lookup, only include categories with valid IDs
            var categoryDict = categories
                .Where(c => c.Id.HasValue)
                .ToDictionary(c => c.Id!.Value, c => c);

            // Process all categories to set hierarchy information
            foreach (var category in categories)
            {
                if (!category.ParentId.HasValue)
                {
                    // Root category
                    category.Level = 0;
                    category.CategoryPath = category.Name;
                }
                else if (categoryDict.TryGetValue(category.ParentId.Value, out var parent))
                {
                    // Child category - calculate level and path based on parent
                    category.Level = parent.Level + 1;
                    category.CategoryPath = $"{parent.CategoryPath} > {category.Name}";
                }
                else
                {
                    // Parent not found, treat as root but mark as orphaned
                    category.Level = 0;
                    category.CategoryPath = $"[Orphaned] {category.Name}";
                }
            }

            // Return all categories sorted by category path
            return categories.OrderBy(c => c.CategoryPath).ToList();
        }

        /// <summary>
        /// Validates category hierarchy for circular references
        /// </summary>
        public static List<string> ValidateHierarchy(List<CategoryExcelDTO> categories)
        {
            var errors = new List<string>();
            var categoryDict = categories.Where(c => c.Id.HasValue).ToDictionary(c => c.Id!.Value, c => c);

            foreach (var category in categories.Where(c => c.Id.HasValue && c.ParentId.HasValue))
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

                    if (!categoryDict.TryGetValue(current.ParentId.Value, out current))
                    {
                        errors.Add($"Parent category with ID {current?.ParentId} not found for category '{category.Name}' (ID: {category.Id})");
                        break;
                    }
                }
            }

            return errors;
        }
    }
}
