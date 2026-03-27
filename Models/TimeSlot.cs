using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourtBookingAPI.Models
{
    public class TimeSlot
    {
        [Key]
        public int SlotID { get; set; }
        
        [Required]
        [MaxLength(15)]
        public string CourtID { get; set; } = string.Empty;
        
        [Required]
        public DateTime SlotDate { get; set; }
        
        [Required]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        public TimeSpan EndTime { get; set; }
        
        public bool IsAvailable { get; set; } = true;

        public int? BookingID { get; set; }

        // Navigation properties
        [ForeignKey("CourtID")]
        public virtual Court Court { get; set; } = null!;

        [ForeignKey("BookingID")]
        public virtual Booking? Booking { get; set; }
    }
}
