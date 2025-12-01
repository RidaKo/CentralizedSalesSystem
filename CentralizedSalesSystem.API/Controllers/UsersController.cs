using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Models.Auth.enums;
using System.Security.Cryptography;
using System.Text;

namespace CentralizedSalesSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly CentralizedSalesDbContext _db;
        //testai DB veikimui

        public UsersController(CentralizedSalesDbContext db)
        {
            _db = db;
        }

        // DTO for GET
        public class UserDto
        {
            public int Id { get; set; }
            public int BusinessId { get; set; }
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }

        // DTO for POST
        public class CreateUserDto
        {
            public int BusinessId { get; set; }
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _db.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    BusinessId = u.BusinessId,
                    Email = u.Email,
                    Phone = u.Phone,
                    Status = u.Status.ToString()
                })
                .ToListAsync();

            return Ok(users);
        }

        // POST: api/users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("Email already exists.");
            }

            var user = new User
            {
                BusinessId = dto.BusinessId,
                Email = dto.Email,
                Phone = dto.Phone,
                Status = Status.Active,
                PasswordHash = HashPassword(dto.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllUsers), new { id = user.Id }, new UserDto
            {
                Id = user.Id,
                BusinessId = user.BusinessId,
                Email = user.Email,
                Phone = user.Phone,
                Status = user.Status.ToString()
            });
        }

        // Simple SHA256 password hash
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
