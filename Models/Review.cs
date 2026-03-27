using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourtBookingAPI.Models
{
    public class Review
    {
        [Key]
        public int ReviewID { get; set; }
        
        [Required]
        public int UserID { get; set; }
        
        [Required]
        [MaxLength(15)]
        public string FacilityID { get; set; } = string.Empty;
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [MaxLength(500)]
        public string Comment { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsFlagged { get; set; } = false;

        // Navigation
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;
        
        [ForeignKey("FacilityID")]
        public virtual Facility Facility { get; set; } = null!;
    }
}
