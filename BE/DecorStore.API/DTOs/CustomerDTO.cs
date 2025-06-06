using System;
using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs
{
    // DTO for returning customer data
    public class CustomerDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // DTO for creating a new customer
    public class CreateCustomerDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string LastName { get; set; }
        
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }
        
        [StringLength(255)]
        public string Address { get; set; }
        
        [StringLength(100)]
        public string City { get; set; }
        
        [StringLength(50)]
        public string State { get; set; }
        
        [StringLength(20)]
        public string PostalCode { get; set; }
        
        [StringLength(50)]
        public string Country { get; set; }
        
        [StringLength(20)]
        [Phone]
        public string Phone { get; set; }    }

    // DTO for updating a customer
    public class UpdateCustomerDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }
        
        [StringLength(255)]
        public string Address { get; set; }
        
        [StringLength(100)]
        public string City { get; set; }
        
        [StringLength(50)]
        public string State { get; set; }
        
        [StringLength(20)]
        public string PostalCode { get; set; }
        
        [StringLength(50)]
        public string Country { get; set; }
        
        [StringLength(20)]
        [Phone]
        public string Phone { get; set; }
    }
}
