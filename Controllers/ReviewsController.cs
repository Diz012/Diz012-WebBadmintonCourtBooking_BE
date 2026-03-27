using System.Security.Claims;
using CourtBookingAPI.Data;
using CourtBookingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CourtBookingAPI.Services;

namespace CourtBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ChatbotService _chatbot;

        public ReviewsController(ApplicationDbContext context, ChatbotService chatbot)
        {
            _context = context;
            _chatbot = chatbot;
        }

        [HttpGet("facility/{facilityId}")]
        public async Task<IActionResult> GetFacilityReviews(string facilityId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.FacilityID == facilityId && !r.IsFlagged)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new {
                    r.ReviewID,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    UserEmail = r.User.Email,
                    UserName = r.User.FullName
                })
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostReview(ReviewDto reviewDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            
            // Optional: Check if user already reviewed this facility
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserID == userId && r.FacilityID == reviewDto.FacilityID);
            
            if (existingReview != null)
            {
                existingReview.Rating = reviewDto.Rating;
                existingReview.Comment = reviewDto.Comment;
                existingReview.CreatedAt = DateTime.UtcNow;
                _context.Entry(existingReview).State = EntityState.Modified;
            }
            else
            {
                var isFlagged = _chatbot.AnalyzeReview(reviewDto.Comment);

                var review = new Review
                {
                    UserID = userId,
                    FacilityID = reviewDto.FacilityID,
                    Rating = reviewDto.Rating,
                    Comment = reviewDto.Comment,
                    CreatedAt = DateTime.UtcNow,
                    IsFlagged = isFlagged
                };
                _context.Reviews.Add(review);
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Review submitted successfully" });
        }

        [HttpGet("flagged")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> GetFlaggedReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Facility)
                .Where(r => r.IsFlagged)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new {
                    r.ReviewID,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    UserEmail = r.User.Email,
                    UserName = r.User.FullName,
                    FacilityName = r.Facility.Name
                })
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}/unflag")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnflagReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            review.IsFlagged = false;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    public class ReviewDto
    {
        public string FacilityID { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
