using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs.Excel
{
    /// <summary>
    /// DTO for Order Excel import/export operations
    /// </summary>
    public class OrderExcelDTO
    {
        /// <summary>
        /// Order ID (for updates, leave empty for new orders)
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// User ID who placed the order
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// User email (alternative to UserId for lookup)
        /// </summary>
        [StringLength(255)]
        public string? UserEmail { get; set; }

        /// <summary>
        /// User full name (for reference)
        /// </summary>
        public string? UserFullName { get; set; }

        /// <summary>
        /// Customer ID (optional, for guest orders)
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Customer email (alternative to CustomerId for lookup)
        /// </summary>
        [StringLength(255)]
        public string? CustomerEmail { get; set; }

        /// <summary>
        /// Customer full name (for reference)
        /// </summary>
        public string? CustomerFullName { get; set; }

        /// <summary>
        /// Total order amount
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Order status
        /// </summary>
        [Required]
        [StringLength(50)]
        public string OrderStatus { get; set; } = "Pending";

        /// <summary>
        /// Payment method
        /// </summary>
        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Shipping address
        /// </summary>
        [Required]
        [StringLength(255)]
        public string ShippingAddress { get; set; } = string.Empty;

        /// <summary>
        /// Shipping city
        /// </summary>
        [StringLength(100)]
        public string? ShippingCity { get; set; }

        /// <summary>
        /// Shipping state
        /// </summary>
        [StringLength(50)]
        public string? ShippingState { get; set; }

        /// <summary>
        /// Shipping postal code
        /// </summary>
        [StringLength(20)]
        public string? ShippingPostalCode { get; set; }

        /// <summary>
        /// Shipping country
        /// </summary>
        [StringLength(50)]
        public string? ShippingCountry { get; set; }

        /// <summary>
        /// Contact phone number
        /// </summary>
        [StringLength(100)]
        public string? ContactPhone { get; set; }

        /// <summary>
        /// Contact email address
        /// </summary>
        [StringLength(100)]
        [EmailAddress]
        public string? ContactEmail { get; set; }

        /// <summary>
        /// Order notes
        /// </summary>
        [StringLength(255)]
        public string? Notes { get; set; }

        /// <summary>
        /// Order date
        /// </summary>
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update date (read-only for export)
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Number of items in the order (read-only for export)
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// Order items as comma-separated string (ProductName:Quantity:UnitPrice)
        /// </summary>
        public string? OrderItems { get; set; }

        /// <summary>
        /// Subtotal before taxes and shipping (read-only for export)
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Tax amount (read-only for export)
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Shipping cost (read-only for export)
        /// </summary>
        public decimal ShippingCost { get; set; }

        /// <summary>
        /// Discount amount (read-only for export)
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Days since order was placed (read-only for export)
        /// </summary>
        public int DaysSinceOrder { get; set; }

        /// <summary>
        /// Shipping tracking number (read-only for export)
        /// </summary>
        public string? TrackingNumber { get; set; }

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
        /// Gets the column mappings for Order Excel operations
        /// </summary>
        public static Dictionary<string, string> GetColumnMappings()
        {
            return new Dictionary<string, string>
            {
                { nameof(Id), "Order ID" },
                { nameof(UserId), "User ID" },
                { nameof(UserFullName), "User Name" },
                { nameof(CustomerId), "Customer ID" },
                { nameof(CustomerFullName), "Customer Name" },
                { nameof(TotalAmount), "Total Amount" },
                { nameof(OrderStatus), "Order Status" },
                { nameof(PaymentMethod), "Payment Method" },
                { nameof(ShippingAddress), "Shipping Address" },
                { nameof(ShippingCity), "Shipping City" },
                { nameof(ShippingState), "Shipping State" },
                { nameof(ShippingPostalCode), "Postal Code" },
                { nameof(ShippingCountry), "Shipping Country" },
                { nameof(ContactPhone), "Contact Phone" },
                { nameof(ContactEmail), "Contact Email" },
                { nameof(Notes), "Notes" },
                { nameof(OrderDate), "Order Date" },
                { nameof(UpdatedAt), "Updated Date" },
                { nameof(ItemCount), "Item Count" },
                { nameof(OrderItems), "Order Items" },
                { nameof(Subtotal), "Subtotal" },
                { nameof(TaxAmount), "Tax Amount" },
                { nameof(ShippingCost), "Shipping Cost" },
                { nameof(DiscountAmount), "Discount Amount" },
                { nameof(DaysSinceOrder), "Days Since Order" }
            };
        }

        /// <summary>
        /// Gets the import column mappings (excludes read-only fields)
        /// </summary>
        public static Dictionary<string, string> GetImportColumnMappings()
        {
            return new Dictionary<string, string>
            {
                { nameof(Id), "Order ID" },
                { nameof(UserId), "User ID" },
                { nameof(CustomerId), "Customer ID" },
                { nameof(TotalAmount), "Total Amount" },
                { nameof(OrderStatus), "Order Status" },
                { nameof(PaymentMethod), "Payment Method" },
                { nameof(ShippingAddress), "Shipping Address" },
                { nameof(ShippingCity), "Shipping City" },
                { nameof(ShippingState), "Shipping State" },
                { nameof(ShippingPostalCode), "Postal Code" },
                { nameof(ShippingCountry), "Shipping Country" },
                { nameof(ContactPhone), "Contact Phone" },
                { nameof(ContactEmail), "Contact Email" },
                { nameof(Notes), "Notes" },
                { nameof(OrderDate), "Order Date" },
                { nameof(OrderItems), "Order Items" }
            };
        }

        /// <summary>
        /// Gets example values for template generation
        /// </summary>
        public static Dictionary<string, object> GetExampleValues()
        {
            return new Dictionary<string, object>
            {
                { nameof(UserId), 1 },
                { nameof(TotalAmount), 299.99m },
                { nameof(OrderStatus), "Pending" },
                { nameof(PaymentMethod), "Credit Card" },
                { nameof(ShippingAddress), "123 Main Street" },
                { nameof(ShippingCity), "New York" },
                { nameof(ShippingState), "NY" },
                { nameof(ShippingPostalCode), "10001" },
                { nameof(ShippingCountry), "USA" },
                { nameof(ContactPhone), "+1-555-123-4567" },
                { nameof(ContactEmail), "customer@example.com" },
                { nameof(Notes), "Please deliver after 5 PM" },
                { nameof(OrderDate), DateTime.Now.ToString("yyyy-MM-dd") },
                { nameof(OrderItems), "Modern Table Lamp:2:89.99,Desk Chair:1:120.01" }
            };
        }

        /// <summary>
        /// Validates the order data
        /// </summary>
        public void Validate()
        {
            ValidationErrors.Clear();

            if (UserId <= 0)
                ValidationErrors.Add("User ID is required and must be greater than 0");

            if (TotalAmount <= 0)
                ValidationErrors.Add("Total amount must be greater than 0");

            if (string.IsNullOrWhiteSpace(OrderStatus))
                ValidationErrors.Add("Order status is required");
            else if (OrderStatus.Length > 50)
                ValidationErrors.Add("Order status must not exceed 50 characters");

            if (string.IsNullOrWhiteSpace(PaymentMethod))
                ValidationErrors.Add("Payment method is required");
            else if (PaymentMethod.Length > 50)
                ValidationErrors.Add("Payment method must not exceed 50 characters");

            if (string.IsNullOrWhiteSpace(ShippingAddress))
                ValidationErrors.Add("Shipping address is required");
            else if (ShippingAddress.Length > 255)
                ValidationErrors.Add("Shipping address must not exceed 255 characters");

            if (!string.IsNullOrEmpty(ShippingCity) && ShippingCity.Length > 100)
                ValidationErrors.Add("Shipping city must not exceed 100 characters");

            if (!string.IsNullOrEmpty(ShippingState) && ShippingState.Length > 50)
                ValidationErrors.Add("Shipping state must not exceed 50 characters");

            if (!string.IsNullOrEmpty(ShippingPostalCode) && ShippingPostalCode.Length > 20)
                ValidationErrors.Add("Shipping postal code must not exceed 20 characters");

            if (!string.IsNullOrEmpty(ShippingCountry) && ShippingCountry.Length > 50)
                ValidationErrors.Add("Shipping country must not exceed 50 characters");

            if (!string.IsNullOrEmpty(ContactPhone) && ContactPhone.Length > 100)
                ValidationErrors.Add("Contact phone must not exceed 100 characters");

            if (!string.IsNullOrEmpty(ContactEmail))
            {
                if (ContactEmail.Length > 100)
                    ValidationErrors.Add("Contact email must not exceed 100 characters");
                else if (!IsValidEmail(ContactEmail))
                    ValidationErrors.Add("Contact email must be a valid email address");
            }

            if (!string.IsNullOrEmpty(Notes) && Notes.Length > 255)
                ValidationErrors.Add("Notes must not exceed 255 characters");

            if (CustomerId.HasValue && CustomerId.Value <= 0)
                ValidationErrors.Add("Customer ID must be greater than 0 if provided");

            // Validate order items format if provided
            if (!string.IsNullOrEmpty(OrderItems) && !IsValidOrderItemsFormat(OrderItems))
                ValidationErrors.Add("Order items must be in format 'ProductName:Quantity:UnitPrice' separated by commas");
        }

        /// <summary>
        /// Validates email format
        /// </summary>
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

        /// <summary>
        /// Validates order items format
        /// </summary>
        private static bool IsValidOrderItemsFormat(string orderItems)
        {
            if (string.IsNullOrWhiteSpace(orderItems))
                return true;

            var items = orderItems.Split(',');
            foreach (var item in items)
            {
                var parts = item.Trim().Split(':');
                if (parts.Length != 3)
                    return false;

                if (string.IsNullOrWhiteSpace(parts[0])) // Product name
                    return false;

                if (!int.TryParse(parts[1], out var quantity) || quantity <= 0) // Quantity
                    return false;

                if (!decimal.TryParse(parts[2], out var price) || price <= 0) // Unit price
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates order metrics for export
        /// </summary>
        public void CalculateMetrics()
        {
            // Calculate days since order
            DaysSinceOrder = (DateTime.UtcNow - OrderDate).Days;

            // Calculate subtotal (total - tax - shipping + discount)
            Subtotal = TotalAmount - TaxAmount - ShippingCost + DiscountAmount;

            // Parse order items and calculate item count
            if (!string.IsNullOrEmpty(OrderItems))
            {
                var items = OrderItems.Split(',');
                ItemCount = 0;
                foreach (var item in items)
                {
                    var parts = item.Trim().Split(':');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var quantity))
                    {
                        ItemCount += quantity;
                    }
                }
            }
        }

        /// <summary>
        /// Parses order items string into structured data
        /// </summary>
        /// <returns>List of parsed order items</returns>
        public List<OrderItemExcelDTO> ParseOrderItems()
        {
            var result = new List<OrderItemExcelDTO>();

            if (string.IsNullOrEmpty(OrderItems))
                return result;

            var items = OrderItems.Split(',');
            foreach (var item in items)
            {
                var parts = item.Trim().Split(':');
                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[1], out var quantity) &&
                        decimal.TryParse(parts[2], out var price))
                    {
                        result.Add(new OrderItemExcelDTO
                        {
                            ProductName = parts[0].Trim(),
                            Quantity = quantity,
                            UnitPrice = price
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Formats order items from structured data
        /// </summary>
        /// <param name="items">List of order items</param>
        /// <returns>Formatted string</returns>
        public static string FormatOrderItems(IEnumerable<OrderItemExcelDTO> items)
        {
            return string.Join(",", items.Select(i => $"{i.ProductName}:{i.Quantity}:{i.UnitPrice:F2}"));
        }
    }

    /// <summary>
    /// DTO for Order Item data within OrderExcelDTO
    /// </summary>
    public class OrderItemExcelDTO
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Product SKU
        /// </summary>
        public string ProductSKU { get; set; } = string.Empty;

        /// <summary>
        /// Product name
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Quantity ordered
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price at time of order
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Total price for this item (Quantity * UnitPrice)
        /// </summary>
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
