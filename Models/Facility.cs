using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourtBookingAPI.Models
{
    public class Facility
    {
        [Key]
        [MaxLength(15)]
        public string FacilityID { get; set; } = string.Empty;
        
        [Required]
        public int OwnerID { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public TimeSpan OpenTime { get; set; }
        
        [Required]
        public TimeSpan CloseTime { get; set; }
        
        public string? ImageUrl { get; set; }

        // Navigation properties
        [ForeignKey("OwnerID")]
        public virtual User Owner { get; set; } = null!;
        public virtual ICollection<Court> Courts { get; set; } = new List<Court>();
    }
}
