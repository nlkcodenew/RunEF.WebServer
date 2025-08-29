using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using RunEF.WebServer.Application.Commands.Auth;
using RunEF.WebServer.Application.DTOs;
using RunEF.WebServer.Infrastructure.Data;
using RunEF.WebServer.Domain.Entities;
using RunEF.WebServer.Domain.Enums;
using RunEF.WebServer.Api.Models;

namespace RunEF.WebServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _context;

    public AuthController(IMediator mediator, ApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var command = new LogoutCommand(GetCurrentUserIdAsInt());
            await _mediator.Send(command);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("check-admin")]
    public async Task<ActionResult> CheckAdminExists()
    {
        var adminExists = await _context.DataRunEFAccountWebs.AnyAsync(u => u.Username == "admin");

        if (!adminExists)
            return NotFound();

        return Ok();
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterUserRequest request)
    {
        try
        {
            var existingUser = await _context.DataRunEFAccountWebs
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
                return BadRequest("Username already exists");

            var newUser = new DataRunEFAccountWeb
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role ?? "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.DataRunEFAccountWebs.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User created successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating user: {ex.Message}");
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim ?? "", out var userId) ? userId : Guid.Empty;
    }

    private int GetCurrentUserIdAsInt()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        return int.TryParse(userIdClaim ?? "", out var userId) ? userId : 0;
    }
}