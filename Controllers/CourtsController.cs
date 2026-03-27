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
    public class CourtsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourtsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<ActionResult<Court>> PostCourt(CourtDto courtDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            
            var facility = await _context.Facilities.FindAsync(courtDto.FacilityID);
            if (facility == null) return BadRequest("Facility not found.");
            
            if (User.IsInRole("Owner") && facility.OwnerID != userId)
            {
                return Forbid();
            }

            var court = new Court
            {
                CourtID = courtDto.CourtID,
                FacilityID = courtDto.FacilityID,
                CourtName = courtDto.CourtName,
                ImageUrl = courtDto.ImageUrl,
                HourlyRate = courtDto.HourlyRate,
                Status = "Active"
            };

            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            // Generate time slots for the remainder of this month and the entire next month
            var today = DateTime.Today;
            var endOfNextMonth = new DateTime(today.Year, today.Month, 1).AddMonths(2).AddDays(-1);
            
            for (var date = today; date <= endOfNextMonth; date = date.AddDays(1))
            {
                for (var hour = facility.OpenTime; hour < facility.CloseTime; hour += TimeSpan.FromHours(1))
                {
                    _context.TimeSlots.Add(new TimeSlot
                    {
                        CourtID = court.CourtID,
                        SlotDate = date,
                        StartTime = hour,
                        EndTime = hour + TimeSpan.FromHours(1),
                        IsAvailable = true
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(court);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> PutCourt(string id, CourtDto courtDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var court = await _context.Courts.Include(c => c.Facility).FirstOrDefaultAsync(c => c.CourtID == id);

            if (court == null) return NotFound();
            if (court.Facility.OwnerID != userId) return Forbid();

            court.CourtName = courtDto.CourtName;
            court.ImageUrl = courtDto.ImageUrl;
            court.HourlyRate = courtDto.HourlyRate;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteCourt(string id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var court = await _context.Courts
                .Include(c => c.Facility)
                .Include(c => c.TimeSlots)
                .FirstOrDefaultAsync(c => c.CourtID == id);

            if (court == null) return NotFound();
            if (court.Facility.OwnerID != userId) return Forbid();

            // Delete associated time slots first
            _context.TimeSlots.RemoveRange(court.TimeSlots);
            
            _context.Courts.Remove(court);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("facility/{facilityId}")]
        public async Task<ActionResult<IEnumerable<Court>>> GetCourtsByFacility(string facilityId)
        {
            return await _context.Courts
                .Include(c => c.TimeSlots)
                .Where(c => c.FacilityID == facilityId)
                .ToListAsync();
        }
    }
}
