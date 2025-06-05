using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs.Excel
{
    /// <summary>
    /// DTO for Customer Excel import/export operations
    /// </summary>
    public class CustomerExcelDTO
    {
        // ... (other properties)

        /// <summary>
        /// Gets column mappings for Excel import, excluding the Id column.
        /// </summary>
        /// <returns>Dictionary of column mappings.</returns>
        public static Dictionary<string, string> GetImportColumnMappingsWithoutId()
        {
            var mappings = GetImportColumnMappings();
            mappings.Remove(nameof(Id), out _); // Assuming "Id" is the key for the Id column
            return mappings;
        }

        /// <summary>
        /// Customer ID (for updates, leave empty for new customers). This field is not included in the import template.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Customer first name
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Customer last name
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Customer email address
        /// </summary>
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Customer address
        /// </summary>
        [StringLength(255)]
        public string? Address { get; set; }

        /// <summary>
        /// Customer city
        /// </summary>
        [StringLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// Customer state/province
        /// </summary>
        [StringLength(50)]
        public string? State { get; set; }

        /// <summary>
        /// Customer postal code
        /// </summary>
        [StringLength(20)]
        public string? PostalCode { get; set; }

        /// <summary>
        /// Customer country
        /// </summary>
        [StringLength(50)]
        public string? Country { get; set; }

        /// <summary>
        /// Customer phone number
        /// </summary>
        [StringLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Customer date of birth (optional)
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Customer full name (computed, read-only for export)
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Registration date (read-only for export)
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Last update date (read-only for export)
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Number of orders placed by this customer (read-only for export)
        /// </summary>
        public int OrderCount { get; set; }

        /// <summary>
        /// Total amount spent by this customer (read-only for export)
        /// </summary>
        public decimal TotalSpent { get; set; }

        /// <summary>
        /// Average order value for this customer (read-only for export)
        /// </summary>
        public decimal AverageOrderValue { get; set; }

        /// <summary>
        /// Date of last order (read-only for export)
        /// </summary>
        public DateTime? LastOrderDate { get; set; }

        /// <summary>
        /// Days since last order (read-only for export)
        /// </summary>
        public int? DaysSinceLastOrder { get; set; }

        /// <summary>
        /// Customer lifetime value (read-only for export)
        /// </summary>
        public decimal LifetimeValue { get; set; }

        /// <summary>
        /// Customer status (Active, Inactive, VIP, etc.) (read-only for export)
        /// </summary>
        public string CustomerStatus { get; set; } = "Active";

        /// <summary>
        /// Customer segment (New, Regular, VIP, etc.) (read-only for export)
        /// </summary>
        public string CustomerSegment { get; set; } = "New";

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
        /// Gets the column mappings for Customer Excel operations
        /// </summary>
        public static Dictionary<string, string> GetColumnMappings()
        {
            return new Dictionary<string, string>
            {
                { nameof(Id), "Customer ID" },
                { nameof(FirstName), "First Name" },
                { nameof(LastName), "Last Name" },
                { nameof(Email), "Email Address" },
                { nameof(Address), "Address" },
                { nameof(City), "City" },
                { nameof(State), "State/Province" },
                { nameof(PostalCode), "Postal Code" },
                { nameof(Country), "Country" },
                { nameof(Phone), "Phone Number" },
                { nameof(CreatedAt), "Registration Date" },
                { nameof(UpdatedAt), "Last Updated" },
                { nameof(OrderCount), "Order Count" },
                { nameof(TotalSpent), "Total Spent" },
                { nameof(AverageOrderValue), "Average Order Value" },
                { nameof(LastOrderDate), "Last Order Date" },
                { nameof(DaysSinceLastOrder), "Days Since Last Order" },
                { nameof(LifetimeValue), "Lifetime Value" },
                { nameof(CustomerStatus), "Customer Status" },
                { nameof(CustomerSegment), "Customer Segment" }
            };
        }

        /// <summary>
        /// Gets the import column mappings (excludes read-only fields)
        /// </summary>
        public static Dictionary<string, string> GetImportColumnMappings()
        {
            return new Dictionary<string, string>
            {
                { nameof(Id), "Customer ID" },
                { nameof(FirstName), "First Name" },
                { nameof(LastName), "Last Name" },
                { nameof(Email), "Email Address" },
                { nameof(Address), "Address" },
                { nameof(City), "City" },
                { nameof(State), "State/Province" },
                { nameof(PostalCode), "Postal Code" },
                { nameof(Country), "Country" },
                { nameof(Phone), "Phone Number" }
            };
        }

        /// <summary>
        /// Gets example values for template generation
        /// </summary>
        public static Dictionary<string, object> GetExampleValues()
        {
            return new Dictionary<string, object>
            {
                { nameof(FirstName), "John" },
                { nameof(LastName), "Doe" },
                { nameof(Email), "john.doe@example.com" },
                { nameof(Address), "123 Main Street" },
                { nameof(City), "New York" },
                { nameof(State), "NY" },
                { nameof(PostalCode), "10001" },
                { nameof(Country), "USA" },
                { nameof(Phone), "+1-555-123-4567" }
            };
        }

        /// <summary>
        /// Validates the customer data
        /// </summary>
        public void Validate()
        {
            ValidationErrors.Clear();

            if (string.IsNullOrWhiteSpace(FirstName))
                ValidationErrors.Add("First name is required");
            else if (FirstName.Length < 2 || FirstName.Length > 100)
                ValidationErrors.Add("First name must be between 2 and 100 characters");

            if (string.IsNullOrWhiteSpace(LastName))
                ValidationErrors.Add("Last name is required");
            else if (LastName.Length < 2 || LastName.Length > 100)
                ValidationErrors.Add("Last name must be between 2 and 100 characters");

            if (string.IsNullOrWhiteSpace(Email))
                ValidationErrors.Add("Email address is required");
            else if (Email.Length > 100)
                ValidationErrors.Add("Email address must not exceed 100 characters");
            else if (!IsValidEmail(Email))
                ValidationErrors.Add("Email address must be a valid email format");

            if (!string.IsNullOrEmpty(Address) && Address.Length > 255)
                ValidationErrors.Add("Address must not exceed 255 characters");

            if (!string.IsNullOrEmpty(City) && City.Length > 100)
                ValidationErrors.Add("City must not exceed 100 characters");

            if (!string.IsNullOrEmpty(State) && State.Length > 50)
                ValidationErrors.Add("State/Province must not exceed 50 characters");

            if (!string.IsNullOrEmpty(PostalCode) && PostalCode.Length > 20)
                ValidationErrors.Add("Postal code must not exceed 20 characters");

            if (!string.IsNullOrEmpty(Country) && Country.Length > 50)
                ValidationErrors.Add("Country must not exceed 50 characters");

            if (!string.IsNullOrEmpty(Phone))
            {
                if (Phone.Length > 20)
                    ValidationErrors.Add("Phone number must not exceed 20 characters");
                else if (!IsValidPhoneNumber(Phone))
                    ValidationErrors.Add("Phone number format is invalid");
            }
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
        /// Validates phone number format (basic validation)
        /// </summary>
        private static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true;

            // Remove common phone number characters
            var cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");

            // Check if remaining characters are digits and length is reasonable
            return cleanPhone.All(char.IsDigit) && cleanPhone.Length >= 7 && cleanPhone.Length <= 15;
        }

        /// <summary>
        /// Calculates customer metrics for export
        /// </summary>
        public void CalculateMetrics()
        {
            if (OrderCount > 0 && TotalSpent > 0)
            {
                AverageOrderValue = TotalSpent / OrderCount;
            }

            if (LastOrderDate.HasValue)
            {
                DaysSinceLastOrder = (DateTime.UtcNow - LastOrderDate.Value).Days;
            }

            // Determine customer segment based on order count and total spent
            CustomerSegment = (OrderCount, TotalSpent) switch
            {
                (0, _) => "New",
                (>= 1 and < 5, < 500) => "Regular",
                (>= 5 and < 10, >= 500 and < 2000) => "Loyal",
                (>= 10, >= 2000) => "VIP",
                _ => "Regular"
            };

            // Determine customer status
            CustomerStatus = DaysSinceLastOrder switch
            {
                null => "New",
                <= 30 => "Active",
                <= 90 => "At Risk",
                _ => "Inactive"
            };

            LifetimeValue = TotalSpent; // Simplified calculation
        }

        /// <summary>
        /// Formats the customer for display
        /// </summary>
        public override string ToString()
        {
            return $"{FullName} ({Email})";
        }
    }
}
