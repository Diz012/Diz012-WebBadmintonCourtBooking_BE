using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourtBookingAPI.Models
{
    public class Court
    {
        [Key]
        [MaxLength(15)]
        public string CourtID { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(15)]
        public string FacilityID { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string CourtName { get; set; } = string.Empty;
        
        public string? ImageUrl { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal HourlyRate { get; set; }
        
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Maintenance, Closed

        // Navigation properties
        [ForeignKey("FacilityID")]
        public virtual Facility Facility { get; set; } = null!;
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    }
}
