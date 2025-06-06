using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs
{
    // DTO for user registration
    public class RegisterDTO
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public required string Username { get; set; }
        
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public required string Password { get; set; }
        
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public required string ConfirmPassword { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public required string LastName { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Required]
        public bool AcceptTerms { get; set; }

        [Required]
        public bool AcceptPrivacyPolicy { get; set; }
    }
    
    // DTO for user login
    public class LoginDTO
    {
        [Required]
        public required string Email { get; set; }
        
        [Required]
        public required string Password { get; set; }

        public bool RememberMe { get; set; }
    }
    
    // DTO for returning user data (without sensitive information)
    public class UserDTO
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
    }
    
    // DTO for authentication response
    public class AuthResponseDTO
    {
        public required string Token { get; set; }
        public required UserDTO User { get; set; }
    }
    
    // DTO for changing password
    public class ChangePasswordDTO
    {
        [Required]
        public required string CurrentPassword { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public required string NewPassword { get; set; }
        
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public required string ConfirmNewPassword { get; set; }
    }
}
