using System.ComponentModel.DataAnnotations;

namespace CourtBookingAPI.Models.DTOs
{
    public class FacilityDto
    {
        [Required]
        [MaxLength(10)]
        public string FacilityID { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Address { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string OpenTime { get; set; } = string.Empty; // Format: "HH:mm"

        [Required]
        public string CloseTime { get; set; } = string.Empty; // Format: "HH:mm"

        public string? ImageUrl { get; set; }
    }

    public class CourtDto
    {
        [Required]
        [MaxLength(20)]
        public string CourtID { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string FacilityID { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CourtName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        [Required]
        public decimal HourlyRate { get; set; }
    }

    public class BookingRequestDto
    {
        [Required]
        public string CourtID { get; set; } = string.Empty;

        [Required]
        public DateTime PlayDate { get; set; }

        [Required]
        public List<int> SlotIDs { get; set; } = new List<int>();
    }

    public class RevenuePointDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class DashboardStatsDto
    {
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCourts { get; set; }
        public List<RevenuePointDto> RevenueHistory { get; set; } = new List<RevenuePointDto>();
    }
}
