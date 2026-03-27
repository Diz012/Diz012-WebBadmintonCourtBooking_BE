using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourtBookingAPI.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceID { get; set; }
        
        [Required]
        public int BookingID { get; set; }
        
        public string? InvoiceNo { get; set; }
        
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BaseAmount { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Tax { get; set; } = 0;
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Discount { get; set; } = 0;
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal FinalAmount { get; set; }
        
        [MaxLength(50)]
        public string? PaymentMethod { get; set; } // Credit Card, E-wallet, Bank Transfer, Cash
        
        [MaxLength(100)]
        public string? TransactionID { get; set; }
        
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("BookingID")]
        public virtual Booking Booking { get; set; } = null!;
    }
}
