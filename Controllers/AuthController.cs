using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CourtBookingAPI.Data;
using CourtBookingAPI.Models;
using CourtBookingAPI.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CourtBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest("Email already exists.");
            }

            var user = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                Phone = registerDto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = "Booker",
                IsEmailVerified = true // Auto-verify on registration
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "User registered successfully.",
                email = user.Email
            });
        }

        // Verification is no longer required as per new requirements
        /*
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == verifyDto.Email && u.VerificationToken == verifyDto.Token);
            if (user == null) return BadRequest("Invalid verification code.");

            user.IsEmailVerified = true;
            user.VerificationToken = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Email verified successfully. You can now login." });
        }
        */

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác." });
            }

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = user.Role,
                IsEmailVerified = user.IsEmailVerified
            });
        }

        [HttpPost("external-login")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginDto externalLoginDto)
        {
            User? user = null;

            if (externalLoginDto.Provider.ToLower() == "google")
            {
                try
                {
                    using var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync($"https://www.googleapis.com/oauth2/v3/userinfo?access_token={externalLoginDto.IdToken}");

                    if (!response.IsSuccessStatusCode)
                    {
                        return BadRequest("Invalid Google token.");
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    dynamic? googleUser = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                    string email = googleUser?.email;
                    string name = googleUser?.name ?? "Google User";

                    if (string.IsNullOrEmpty(email))
                    {
                        return BadRequest("Could not retrieve email from Google.");
                    }

                    user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                    if (user == null)
                    {
                        user = new User
                        {
                            FullName = name,
                            Email = email,
                            PasswordHash = Guid.NewGuid().ToString(), // Random hash for external users
                            Role = "Booker",
                            IsEmailVerified = true
                        };
                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest("Error authenticating with Google: " + ex.Message);
                }
            }
            else if (externalLoginDto.Provider.ToLower() == "facebook")
            {
                // Facebook token verification (simplified)
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"https://graph.facebook.com/me?fields=id,name,email&access_token={externalLoginDto.IdToken}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    dynamic? fbUser = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                    
                    string email = fbUser?.email ?? $"{fbUser?.id}@facebook.com";
                    string name = fbUser?.name ?? "Facebook User";

                    user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                    if (user == null)
                    {
                        user = new User
                        {
                            FullName = name,
                            Email = email,
                            PasswordHash = Guid.NewGuid().ToString(),
                            Role = "Booker"
                        };
                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    return BadRequest("Invalid Facebook token.");
                }
            }
            else
            {
                return BadRequest("Unsupported provider.");
            }

            if (user == null) return BadRequest("Could not authenticate user.");

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = user.Role,
                IsEmailVerified = user.IsEmailVerified
            });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FullName", user.FullName)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
