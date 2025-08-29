
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RunEF.WebServer.Web.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace RunEF.WebServer.Web.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IHttpClientFactory httpClientFactory, ILogger<AccountController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("http://localhost:5001");
            
            var loginData = new LoginRequest
            {
                Username = model.Username,
                Password = model.Password
            };

            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthResponseModel>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (authResponse != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, authResponse.Username),
                        new Claim(ClaimTypes.Role, authResponse.Role),
                        new Claim("UserId", authResponse.UserId.ToString()),
                        new Claim("AccessToken", authResponse.AccessToken)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity), authProperties);

                    _logger.LogInformation("User {Username} logged in successfully", model.Username);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Dashboard");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", "Invalid username or password.");
                _logger.LogWarning("Login failed for user {Username}: {Error}", model.Username, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", model.Username);
            ModelState.AddModelError("", "An error occurred during login. Please try again.");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("http://localhost:5001");

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Account created successfully. Please log in.";
                return RedirectToAction("Login");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<JsonElement>(errorContent);
                if (errorResponse.TryGetProperty("message", out var message))
                {
                    ModelState.AddModelError("", message.GetString() ?? "Registration failed");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            ModelState.AddModelError("", "An error occurred during registration. Please try again.");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("http://localhost:5001");

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/auth/reset-password", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Password reset successfully. Please log in with your new password.";
                return RedirectToAction("Login");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<JsonElement>(errorContent);
                if (errorResponse.TryGetProperty("message", out var message))
                {
                    ModelState.AddModelError("", message.GetString() ?? "Password reset failed");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            ModelState.AddModelError("", "An error occurred during password reset. Please try again.");
        }

        return View(model);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var accessToken = User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(accessToken))
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("http://localhost:5001");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                await client.PostAsync("/api/Account/Logout", null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout API call");
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User logged out successfully");
        return RedirectToAction("Login");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutPost()
    {
        try
        {
            var accessToken = User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(accessToken))
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("http://localhost:5001");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                await client.PostAsync("/api/Account/Logout", null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout API call");
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User logged out successfully");
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
