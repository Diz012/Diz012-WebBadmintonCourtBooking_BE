using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourtBookingAPI.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string? Phone { get; set; }
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string Role { get; set; } = "Booker"; // Admin, Owner, Booker

        public bool IsEmailVerified { get; set; } = false;
        public string? VerificationToken { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Facility> OwnedFacilities { get; set; } = new List<Facility>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
