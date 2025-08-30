using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RunEF.WebServer.Web.Models;
using System.Security.Claims;

namespace RunEF.WebServer.Web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userViewModels = new List<UserViewModel>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var claims = await _userManager.GetClaimsAsync(user);
                    
                    userViewModels.Add(new UserViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        EmailConfirmed = user.EmailConfirmed,
                        LockoutEnabled = user.LockoutEnabled,
                        LockoutEnd = user.LockoutEnd,
                        AccessFailedCount = user.AccessFailedCount,
                        Roles = roles.ToList(),
                        IsOnline = IsUserOnline(user.Id),
                        LastLoginTime = GetLastLoginTime(user.Id),
                        CreatedDate = user.LockoutEnd?.DateTime ?? DateTime.Now // Placeholder
                    });
                }

                var model = new UserManagementViewModel
                {
                    Users = userViewModels,
                    TotalUsers = userViewModels.Count,
                    OnlineUsers = userViewModels.Count(u => u.IsOnline),
                    LockedUsers = userViewModels.Count(u => u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.Now),
                    AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync()
                };

                _logger.LogInformation($"Loaded {model.TotalUsers} users for management");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users for management");
                return View(new UserManagementViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return Json(new { success = false, error = "All fields are required" });
                }

                var user = new IdentityUser
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(request.Role))
                    {
                        await _userManager.AddToRoleAsync(user, request.Role);
                    }

                    _logger.LogInformation($"User {request.UserName} created successfully");
                    return Json(new { success = true, message = "User created successfully" });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, error = errors });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return Json(new { success = false, error = "An error occurred while creating the user" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return Json(new { success = false, error = "User not found" });
                }

                user.Email = request.Email;
                user.EmailConfirmed = request.EmailConfirmed;
                user.LockoutEnabled = request.LockoutEnabled;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Update roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    
                    if (request.Roles != null && request.Roles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, request.Roles);
                    }

                    _logger.LogInformation($"User {user.UserName} updated successfully");
                    return Json(new { success = true, message = "User updated successfully" });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, error = errors });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return Json(new { success = false, error = "An error occurred while updating the user" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return Json(new { success = false, error = "User not found" });
                }

                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {user.UserName} deleted successfully");
                    return Json(new { success = true, message = "User deleted successfully" });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, error = errors });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return Json(new { success = false, error = "An error occurred while deleting the user" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LockUser([FromBody] LockUserRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return Json(new { success = false, error = "User not found" });
                }

                var lockoutEnd = request.LockoutMinutes > 0 
                    ? DateTimeOffset.UtcNow.AddMinutes(request.LockoutMinutes)
                    : DateTimeOffset.UtcNow.AddYears(100); // Permanent lockout

                var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {user.UserName} locked until {lockoutEnd}");
                    return Json(new { success = true, message = "User locked successfully" });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, error = errors });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user");
                return Json(new { success = false, error = "An error occurred while locking the user" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnlockUser([FromBody] UnlockUserRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return Json(new { success = false, error = "User not found" });
                }

                var result = await _userManager.SetLockoutEndDateAsync(user, null);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {user.UserName} unlocked successfully");
                    return Json(new { success = true, message = "User unlocked successfully" });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, error = errors });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user");
                return Json(new { success = false, error = "An error occurred while unlocking the user" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] AdminResetPasswordRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return Json(new { success = false, error = "User not found" });
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Password reset for user {user.UserName}");
                    return Json(new { success = true, message = "Password reset successfully" });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, error = errors });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return Json(new { success = false, error = "An error occurred while resetting the password" });
            }
        }

        private bool IsUserOnline(string userId)
        {
            // This is a placeholder implementation
            // In a real application, you would check active sessions, SignalR connections, etc.
            return new Random().Next(0, 2) == 1;
        }

        private DateTime? GetLastLoginTime(string userId)
        {
            // This is a placeholder implementation
            // In a real application, you would track login times in the database
            return DateTime.Now.AddMinutes(-new Random().Next(1, 1440));
        }
    }

    // Request models
    public class CreateUserRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class UpdateUserRequest
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public List<string> Roles { get; set; }
    }

    public class DeleteUserRequest
    {
        public string UserId { get; set; }
    }

    public class LockUserRequest
    {
        public string UserId { get; set; }
        public int LockoutMinutes { get; set; } // 0 for permanent
    }

    public class UnlockUserRequest
    {
        public string UserId { get; set; }
    }

    public class AdminResetPasswordRequest
    {
        public string UserId { get; set; }
        public string NewPassword { get; set; }
    }
}