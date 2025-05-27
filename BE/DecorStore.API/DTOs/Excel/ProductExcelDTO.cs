using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs.Excel
{
    /// <summary>
    /// DTO for Product Excel import/export operations
    /// </summary>
    public class ProductExcelDTO
    {
        /// <summary>
        /// Product ID (for updates, leave empty for new products)
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Product slug (URL-friendly name)
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        /// Product description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Product price
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        /// <summary>
        /// Original price (before discount)
        /// </summary>
        public decimal OriginalPrice { get; set; }

        /// <summary>
        /// Stock quantity
        /// </summary>
        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        /// <summary>
        /// Product SKU (Stock Keeping Unit)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Category ID (must exist in the system)
        /// </summary>
        [Required]
        public int CategoryId { get; set; }

        /// <summary>
        /// Category name (for reference, will be used to lookup CategoryId if CategoryId is not provided)
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// Whether the product is featured
        /// </summary>
        public bool IsFeatured { get; set; } = false;

        /// <summary>
        /// Whether the product is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Average rating (read-only for export)
        /// </summary>
        public float AverageRating { get; set; }

        /// <summary>
        /// Creation date (read-only for export)
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Last update date (read-only for export)
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Image URLs (comma-separated for import, multiple columns for export)
        /// </summary>
        public string? ImageUrls { get; set; }

        /// <summary>
        /// Number of reviews (read-only for export)
        /// </summary>
        public int ReviewCount { get; set; }

        /// <summary>
        /// Total sales quantity (read-only for export)
        /// </summary>
        public int TotalSales { get; set; }

        /// <summary>
        /// Revenue generated (read-only for export)
        /// </summary>
        public decimal Revenue { get; set; }

        /// <summary>
        /// Last sale date (read-only for export)
        /// </summary>
        public DateTime? LastSaleDate { get; set; }

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
        /// Gets the column mappings for Product Excel operations
        /// </summary>
        public static Dictionary<string, string> GetColumnMappings()
        {
            return new Dictionary<string, string>
            {
                { nameof(Id), "Product ID" },
                { nameof(Name), "Product Name" },
                { nameof(Slug), "URL Slug" },
                { nameof(Description), "Description" },
                { nameof(Price), "Price" },
                { nameof(OriginalPrice), "Original Price" },
                { nameof(StockQuantity), "Stock Quantity" },
                { nameof(SKU), "SKU" },
                { nameof(CategoryId), "Category ID" },
                { nameof(CategoryName), "Category Name" },
                { nameof(IsFeatured), "Featured" },
                { nameof(IsActive), "Active" },
                { nameof(AverageRating), "Average Rating" },
                { nameof(CreatedAt), "Created Date" },
                { nameof(UpdatedAt), "Updated Date" },
                { nameof(ImageUrls), "Image URLs" },
                { nameof(ReviewCount), "Review Count" },
                { nameof(TotalSales), "Total Sales" },
                { nameof(Revenue), "Revenue" },
                { nameof(LastSaleDate), "Last Sale Date" }
            };
        }

        /// <summary>
        /// Gets the import column mappings (excludes read-only fields)
        /// </summary>
        public static Dictionary<string, string> GetImportColumnMappings()
        {
            return new Dictionary<string, string>
            {
                { nameof(Id), "Product ID" },
                { nameof(Name), "Product Name" },
                { nameof(Slug), "URL Slug" },
                { nameof(Description), "Description" },
                { nameof(Price), "Price" },
                { nameof(OriginalPrice), "Original Price" },
                { nameof(StockQuantity), "Stock Quantity" },
                { nameof(SKU), "SKU" },
                { nameof(CategoryId), "Category ID" },
                { nameof(CategoryName), "Category Name" },
                { nameof(IsFeatured), "Featured" },
                { nameof(IsActive), "Active" },
                { nameof(ImageUrls), "Image URLs" }
            };
        }

        /// <summary>
        /// Gets example values for template generation
        /// </summary>
        public static Dictionary<string, object> GetExampleValues()
        {
            return new Dictionary<string, object>
            {
                { nameof(Name), "Modern Table Lamp" },
                { nameof(Slug), "modern-table-lamp" },
                { nameof(Description), "Elegant modern table lamp with LED lighting" },
                { nameof(Price), 89.99m },
                { nameof(OriginalPrice), 99.99m },
                { nameof(StockQuantity), 25 },
                { nameof(SKU), "LAMP001" },
                { nameof(CategoryId), 1 },
                { nameof(CategoryName), "Lamps" },
                { nameof(IsFeatured), true },
                { nameof(IsActive), true },
                { nameof(ImageUrls), "https://example.com/image1.jpg,https://example.com/image2.jpg" }
            };
        }

        /// <summary>
        /// Validates the product data
        /// </summary>
        public void Validate()
        {
            ValidationErrors.Clear();

            if (string.IsNullOrWhiteSpace(Name))
                ValidationErrors.Add("Product name is required");
            else if (Name.Length < 3 || Name.Length > 255)
                ValidationErrors.Add("Product name must be between 3 and 255 characters");

            if (string.IsNullOrWhiteSpace(Slug))
                ValidationErrors.Add("Product slug is required");
            else if (Slug.Length > 255)
                ValidationErrors.Add("Product slug must not exceed 255 characters");

            if (Price <= 0)
                ValidationErrors.Add("Price must be greater than 0");

            if (StockQuantity < 0)
                ValidationErrors.Add("Stock quantity cannot be negative");

            if (string.IsNullOrWhiteSpace(SKU))
                ValidationErrors.Add("SKU is required");
            else if (SKU.Length > 50)
                ValidationErrors.Add("SKU must not exceed 50 characters");

            if (CategoryId <= 0 && string.IsNullOrWhiteSpace(CategoryName))
                ValidationErrors.Add("Either Category ID or Category Name must be provided");

            if (OriginalPrice < 0)
                ValidationErrors.Add("Original price cannot be negative");

            if (OriginalPrice > 0 && Price > OriginalPrice)
                ValidationErrors.Add("Price cannot be greater than original price");
        }
    }
}
