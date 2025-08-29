using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RunEF.WebServer.Domain.Entities;
using RunEF.WebServer.Infrastructure.Data;
using RunEF.WebServer.Web.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace RunEF.WebServer.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseModel>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _context.DataRunEFAccountWebs
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null || !user.VerifyPassword(request.Password))
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var token = GenerateJwtToken(user);
            var refreshToken = Guid.NewGuid().ToString();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            user.LastLoginAt = DateTime.UtcNow;
            user.IpAddress = GetClientIpAddress();

            await _context.SaveChangesAsync();

            return Ok(new AuthResponseModel
            {
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role,
                AccessToken = token,
                RefreshToken = refreshToken
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return BadRequest(new { message = "An error occurred during login" });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (await _context.DataRunEFAccountWebs.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest(new { message = "Username already exists" });
            }

            var user = new DataRunEFAccountWeb(request.Username, "");
            user.SetPassword(request.Password);
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Role = "User";
            user.IsActive = true;
            user.IpAddress = GetClientIpAddress();

            _context.DataRunEFAccountWebs.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Account created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return BadRequest(new { message = "An error occurred during registration" });
        }
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var user = await _context.DataRunEFAccountWebs
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            // Verify old password
            if (!user.VerifyPassword(request.OldPassword))
            {
                return BadRequest(new { message = "Old password is incorrect" });
            }

            user.OldPassword = user.PasswordHash;
            user.SetPassword(request.NewPassword);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return BadRequest(new { message = "An error occurred during password reset" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userId, out var id))
            {
                var user = await _context.DataRunEFAccountWebs.FindAsync(id);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiryTime = null;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return BadRequest(new { message = "An error occurred during logout" });
        }
    }

    private string GenerateJwtToken(DataRunEFAccountWeb user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var key = Encoding.ASCII.GetBytes(secretKey);

        var claims = new[]
        {
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("FirstName", user.FirstName ?? ""),
            new Claim("LastName", user.LastName ?? "")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(int.Parse(jwtSettings["ExpiryInHours"])),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}