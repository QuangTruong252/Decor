using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs
{
    /// <summary>
    /// Filter DTO for Order entities
    /// </summary>
    public class OrderFilterDTO : PaginationParameters
    {
        /// <summary>
        /// Search term for order-related fields
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by user ID
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Filter by customer ID
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Filter by order status
        /// </summary>
        public string? OrderStatus { get; set; }

        /// <summary>
        /// Filter by payment method
        /// </summary>
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Minimum total amount
        /// </summary>
        public decimal? MinAmount { get; set; }

        /// <summary>
        /// Maximum total amount
        /// </summary>
        public decimal? MaxAmount { get; set; }

        /// <summary>
        /// Order date range start
        /// </summary>
        public DateTime? OrderDateFrom { get; set; }

        /// <summary>
        /// Order date range end
        /// </summary>
        public DateTime? OrderDateTo { get; set; }

        /// <summary>
        /// Filter by shipping city
        /// </summary>
        public string? ShippingCity { get; set; }

        /// <summary>
        /// Filter by shipping state
        /// </summary>
        public string? ShippingState { get; set; }

        /// <summary>
        /// Filter by shipping country
        /// </summary>
        public string? ShippingCountry { get; set; }

        /// <summary>
        /// Include deleted orders
        /// </summary>
        public bool IncludeDeleted { get; set; } = false;
    }

    /// <summary>
    /// Filter DTO for Category entities
    /// </summary>
    public class CategoryFilterDTO : PaginationParameters
    {
        /// <summary>
        /// Search term for category name and description
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by parent category ID (null for root categories)
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Include only root categories (no parent)
        /// </summary>
        public bool? IsRootCategory { get; set; }

        /// <summary>
        /// Include subcategories in results
        /// </summary>
        public bool IncludeSubcategories { get; set; } = false;

        /// <summary>
        /// Include product count for each category
        /// </summary>
        public bool IncludeProductCount { get; set; } = false;

        /// <summary>
        /// Created date range start
        /// </summary>
        public DateTime? CreatedAfter { get; set; }

        /// <summary>
        /// Created date range end
        /// </summary>
        public DateTime? CreatedBefore { get; set; }

        /// <summary>
        /// Include deleted categories
        /// </summary>
        public bool IncludeDeleted { get; set; } = false;
    }

    /// <summary>
    /// Filter DTO for Customer entities
    /// </summary>
    public class CustomerFilterDTO : PaginationParameters
    {
        /// <summary>
        /// Search term for customer name, email, and contact info
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by email address
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Filter by city
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Filter by state
        /// </summary>
        public string? State { get; set; }

        /// <summary>
        /// Filter by country
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// Filter by postal code
        /// </summary>
        public string? PostalCode { get; set; }

        /// <summary>
        /// Registration date range start
        /// </summary>
        public DateTime? RegisteredAfter { get; set; }

        /// <summary>
        /// Registration date range end
        /// </summary>
        public DateTime? RegisteredBefore { get; set; }

        /// <summary>
        /// Include customers with orders only
        /// </summary>
        public bool? HasOrders { get; set; }

        /// <summary>
        /// Include order count for each customer
        /// </summary>
        public bool IncludeOrderCount { get; set; } = false;

        /// <summary>
        /// Include total spent amount for each customer
        /// </summary>
        public bool IncludeTotalSpent { get; set; } = false;

        /// <summary>
        /// Include deleted customers
        /// </summary>
        public bool IncludeDeleted { get; set; } = false;
    }
}
