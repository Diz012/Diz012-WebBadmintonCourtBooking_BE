using System.Security.Claims;
using CourtBookingAPI.Data;
using CourtBookingAPI.Models;
using CourtBookingAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourtBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacilitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FacilitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Facility>>> GetFacilities(
            [FromQuery] string? searchString, 
            [FromQuery] decimal? minPrice, 
            [FromQuery] decimal? maxPrice)
        {
            var query = _context.Facilities.Include(f => f.Courts).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(f => f.Name.Contains(searchString) || f.Address.Contains(searchString));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(f => f.Courts.Any(c => c.HourlyRate >= minPrice.Value));
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(f => f.Courts.Any(c => c.HourlyRate <= maxPrice.Value));
            }

            return await query.ToListAsync();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<Facility>> PostFacility(FacilityDto facilityDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var facility = new Facility
            {
                FacilityID = facilityDto.FacilityID,
                OwnerID = userId,
                Name = facilityDto.Name,
                Address = facilityDto.Address,
                Description = facilityDto.Description,
                OpenTime = TimeSpan.Parse(facilityDto.OpenTime),
                CloseTime = TimeSpan.Parse(facilityDto.CloseTime),
                ImageUrl = facilityDto.ImageUrl
            };

            _context.Facilities.Add(facility);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFacility", new { id = facility.FacilityID }, facility);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<IEnumerable<Facility>>> GetMyFacilities()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            return await _context.Facilities
                .Include(f => f.Courts)
                .Where(f => f.OwnerID == userId)
                .ToListAsync();
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<DashboardStatsDto>> GetOwnerStats()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            
            var facilities = await _context.Facilities
                .Include(f => f.Courts)
                .ThenInclude(c => c.Bookings)
                .Where(f => f.OwnerID == userId)
                .ToListAsync();

            var allBookings = facilities.SelectMany(f => f.Courts).SelectMany(c => c.Bookings).ToList();
            var totalBookings = allBookings.Count;
            var totalRevenue = allBookings.Sum(b => b.TotalPrice);
            var totalCourts = facilities.Sum(f => f.Courts.Count);

            // Calculate 30-day revenue history
            var thirtyDaysAgo = DateTime.UtcNow.Date.AddDays(-30);
            var revenueHistory = allBookings
                .Where(b => b.PlayDate >= thirtyDaysAgo)
                .GroupBy(b => b.PlayDate.Date)
                .Select(g => new RevenuePointDto
                {
                    Date = g.Key.ToString("dd/MM"),
                    Amount = g.Sum(b => b.TotalPrice)
                })
                .OrderBy(r => r.Date)
                .ToList();

            return new DashboardStatsDto
            {
                TotalBookings = totalBookings,
                TotalRevenue = totalRevenue,
                TotalCourts = totalCourts,
                RevenueHistory = revenueHistory
            };
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> PutFacility(string id, FacilityDto facilityDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var facility = await _context.Facilities.FindAsync(id);

            if (facility == null) return NotFound();
            if (facility.OwnerID != userId) return Forbid();

            facility.Name = facilityDto.Name;
            facility.Address = facilityDto.Address;
            facility.Description = facilityDto.Description;
            facility.OpenTime = TimeSpan.Parse(facilityDto.OpenTime);
            facility.CloseTime = TimeSpan.Parse(facilityDto.CloseTime);
            facility.ImageUrl = facilityDto.ImageUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteFacility(string id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var facility = await _context.Facilities
                .Include(f => f.Courts)
                .ThenInclude(c => c.TimeSlots)
                .FirstOrDefaultAsync(f => f.FacilityID == id);

            if (facility == null) return NotFound();
            if (facility.OwnerID != userId) return Forbid();

            // Delete associated time slots and courts
            foreach (var court in facility.Courts)
            {
                _context.TimeSlots.RemoveRange(court.TimeSlots);
            }
            _context.Courts.RemoveRange(facility.Courts);

            _context.Facilities.Remove(facility);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Facility>> GetFacility(string id)
        {
            var facility = await _context.Facilities
                .Include(f => f.Courts)
                .ThenInclude(c => c.TimeSlots)
                .FirstOrDefaultAsync(f => f.FacilityID == id);

            if (facility == null)
            {
                return NotFound();
            }

            return facility;
        }
    }
}
