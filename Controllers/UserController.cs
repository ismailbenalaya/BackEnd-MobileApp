using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using BackEnd.Model;
using BackEnd.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
namespace BackEnd.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;

        public UserController(AppDbContext context, IConfiguration configuration, ILogger<UserController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                _logger.LogWarning("Login failed for user {Username}", request.Username);
                return Unauthorized("Invalid credentials");
            }

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token = token,
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    firstName = user.first_name,
                    lastName = user.last_name,
                    roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
                }
            });
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterRequest request)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username already exists");
            }

            // Hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create new user
            var user = new User
            {
                Username = request.Username,
                Password = hashedPassword,
                first_name = request.first_name,
                last_name = request.last_name,
                Telephone = request.Telephone,
                created_at = DateTime.UtcNow,
                modified_at = null,
                deleted_at = null
            };

            // Add user to context
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assign role
            var roleId = request.IsAdmin ? 2 : 1; // 2 for Admin, 1 for Visitor
            var userRole = new UserRole
            {
                user_id = user.Id,
                role_id = roleId,
                created_at = DateTime.UtcNow,
                modified_at = DateTime.UtcNow
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new user with id {Id} at {CreatedAt}", user.Id, user.created_at);

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                firstName = user.first_name,
                lastName = user.last_name,
                roles = new[] { roleId == 2 ? "Administrator" : "Visitor" }
            });
        }

        [HttpGet("visitors")]
        public async Task<ActionResult<IEnumerable<object>>> GetVisitors()
        {
            var visitors = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Visitor") && u.deleted_at == null)
                .Select(u => new
                {
                    id = u.Id,
                    username = u.Username,
                    firstName = u.first_name,
                    lastName = u.last_name,
                    telephone = u.Telephone,
                    created_at = u.created_at,
                    modified_at = u.modified_at
                })
                .ToListAsync();

            if (!visitors.Any())
            {
                return NotFound("No visitors found");
            }

            return Ok(visitors);
        }

        [HttpGet("admins")]
        public async Task<ActionResult<IEnumerable<object>>> GetAdmins()
        {
            var admins = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Administrator") && u.deleted_at == null)
                .Select(u => new
                {
                    id = u.Id,
                    username = u.Username,
                    firstName = u.first_name,
                    lastName = u.last_name,
                    telephone = u.Telephone,
                    created_at = u.created_at,
                    modified_at = u.modified_at
                })
                .ToListAsync();

            if (!admins.Any())
            {
                return NotFound("No administrators found");
            }

            return Ok(admins);
        }

       [HttpDelete("visitor/{id}")]
public async Task<IActionResult> DeleteVisitor(int id)
{
    var strategy = _context.Database.CreateExecutionStrategy();
    
    try
    {
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            // Find user and check if exists
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                _logger.LogWarning("User with id {Id} not found", id);
                throw new InvalidOperationException("User not found");
            }

            // Check if user is a visitor
            if (!user.UserRoles.Any(ur => ur.Role.Name == "Visitor"))
            {
                _logger.LogWarning("User with id {Id} is not a visitor", id);
                throw new InvalidOperationException("User is not a visitor");
            }

            // Delete user roles
            _context.UserRoles.RemoveRange(user.UserRoles);
            await _context.SaveChangesAsync();

            // Delete user
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        });

        _logger.LogInformation("Successfully deleted visitor with id {Id}", id);
        return NoContent();
    }
    catch (InvalidOperationException ex)
    {
        return NotFound(ex.Message);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting visitor with id {Id}", id);
        return StatusCode(500, "An error occurred while deleting the visitor");
    }
}

        private string GenerateJwtToken(User user)
        {
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        
        [Required]
        public string first_name { get; set; }
        
        [Required]
        public string last_name { get; set; }
        
        public int? Telephone { get; set; }
        
        public bool IsAdmin { get; set; } = false;
    }
    
}
