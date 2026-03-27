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
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking(BookingRequestDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            
            var slots = await _context.TimeSlots
                .Where(s => request.SlotIDs.Contains(s.SlotID) && s.CourtID == request.CourtID && s.IsAvailable == true)
                .ToListAsync();

            if (slots.Count != request.SlotIDs.Count)
            {
                return BadRequest("One or more slots are unavailable.");
            }

            var court = await _context.Courts.FindAsync(request.CourtID);
            if (court == null) return NotFound("Court not found.");

            decimal totalHours = (decimal)slots.Count * 1.0m; // Assuming 1 hour per slot for simplicity
            decimal totalPrice = totalHours * court.HourlyRate;

            var booking = new Booking
            {
                UserID = userId,
                CourtID = request.CourtID,
                PlayDate = request.PlayDate,
                TotalHours = totalHours,
                TotalPrice = totalPrice,
                BookingStatus = "Pending",
                PaymentStatus = "Unpaid"
            };

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();

                    foreach (var slot in slots)
                    {
                        slot.IsAvailable = false;
                        slot.BookingID = booking.BookingID;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { BookingID = booking.BookingID, TotalPrice = booking.TotalPrice });
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "An error occurred while creating the booking.");
                }
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var booking = await _context.Bookings
                .Include(b => b.TimeSlots)
                .FirstOrDefaultAsync(b => b.BookingID == id && b.UserID == userId);

            if (booking == null) return NotFound("Booking not found.");
            
            if (booking.BookingStatus == "Cancelled" || booking.BookingStatus == "Completed")
            {
                return BadRequest($"Cannot cancel a booking with status: {booking.BookingStatus}");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    booking.BookingStatus = "Cancelled";
                    
                    foreach (var slot in booking.TimeSlots)
                    {
                        slot.IsAvailable = true;
                        slot.BookingID = null;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { Message = "Booking cancelled successfully" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var booking = await _context.Bookings
                .Include(b => b.Court)
                .ThenInclude(c => c.Facility)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) return NotFound();
            if (booking.UserID != userId) return Forbid();

            return Ok(booking);
        }

        [HttpPost("{id}/pay")]
        public async Task<IActionResult> PayBooking(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var booking = await _context.Bookings
                .Include(b => b.Court)
                .FirstOrDefaultAsync(b => b.BookingID == id && b.UserID == userId);

            if (booking == null) return NotFound("Booking not found.");
            if (booking.PaymentStatus == "Paid") return BadRequest("Booking is already paid.");
            if (booking.BookingStatus == "Cancelled") return BadRequest("Cannot pay for a cancelled booking.");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    booking.PaymentStatus = "Paid";
                    booking.BookingStatus = "Confirmed";

                    var invoice = new Invoice
                    {
                        BookingID = booking.BookingID,
                        InvoiceNo = "INV-" + booking.BookingID.ToString().PadLeft(5, '0'),
                        PaymentDate = DateTime.UtcNow,
                        BaseAmount = booking.TotalPrice,
                        Tax = 0,
                        Discount = 0,
                        FinalAmount = booking.TotalPrice,
                        PaymentMethod = "Simulated",
                        TransactionID = "SIM-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
                    };

                    _context.Invoices.Add(invoice);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { Message = "Payment successful", InvoiceID = invoice.InvoiceID });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"An error occurred: {ex.Message} - {ex.InnerException?.Message}");
                }
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var bookings = await _context.Bookings
                .Include(b => b.Court)
                .Where(b => b.UserID == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return Ok(bookings);
        }
    }
}
