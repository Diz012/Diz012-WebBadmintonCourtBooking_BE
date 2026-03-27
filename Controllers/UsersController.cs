using CourtBookingAPI.Data;
using CourtBookingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourtBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.UserID,
                    u.FullName,
                    u.Email,
                    u.Phone,
                    u.Role,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] RoleUpdateDto roleDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (roleDto.Role != "Admin" && roleDto.Role != "Owner" && roleDto.Role != "Booker")
                return BadRequest("Invalid role.");

            user.Role = roleDto.Role;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Role updated successfully." });
        }
    }

    public class RoleUpdateDto
    {
        public string Role { get; set; } = string.Empty;
    }
}
