using System.ComponentModel.DataAnnotations;

namespace CourtBookingAPI.Models.DTOs
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
    }

    public class ExternalLoginDto
    {
        public string IdToken { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty; // google or facebook
    }

    public class VerifyEmailDto
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
