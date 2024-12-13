using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using pweb_eas.Data;
using pweb_eas.Models;
using pweb_eas.Models.Entities;

namespace pweb_eas.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IConfiguration configuration;

        public AuthController(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(AddUserDto addUserDto)
        {
            // Check if email already exist
            var userExist = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == addUserDto.Email);
            if (userExist != null)
            {
                return BadRequest(new
                {
                    message = "user already exists"
                });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = addUserDto.Name,
                Email = addUserDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(addUserDto.Password),
                PhoneNumber = addUserDto.PhoneNumber,
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DeletedAt = null
            };

            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "success register user",
                user = new
                {
                    user.Id,
                    user.Email,
                    user.Name,
                    user.Role
                }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash) || user.DeletedAt != null)
            {
                return BadRequest(new
                {
                    message = "invalid credentials"
                });
            }

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "login successful",
                data = new { user.Id, user.Email, user.Name, user.Role, token },
            });
        }
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new ("UserId", user.Id.ToString()),
                new (JwtRegisteredClaimNames.Email, user.Email),
                new ("Role", user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
