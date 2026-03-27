using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourtBookingAPI.Models
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }
        
        [Required]
        public int UserID { get; set; }
        
        [Required]
        [MaxLength(15)]
        public string CourtID { get; set; } = string.Empty;
        
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        public DateTime PlayDate { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal TotalHours { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }
        
        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Paid, Refunded
        
        [MaxLength(20)]
        public string BookingStatus { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;
        
        [ForeignKey("CourtID")]
        public virtual Court Court { get; set; } = null!;
        
        public virtual Invoice? Invoice { get; set; }
        
        public virtual ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    }
}
