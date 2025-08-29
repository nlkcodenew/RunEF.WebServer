using Microsoft.EntityFrameworkCore;
using RunEF.WebServer.Domain.Entities;
using RunEF.WebServer.Domain.Enums;
using RunEF.WebServer.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace RunEF.WebServer.Web.Services;

public class DataSeederService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataSeederService> _logger;
    private readonly IConfiguration _configuration; // Add IConfiguration

    public DataSeederService(ApplicationDbContext context, ILogger<DataSeederService> logger, IConfiguration configuration) // Inject IConfiguration
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();
            
            _logger.LogInformation("Database initialized. Manual account creation is enabled through registration page.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing database");
            throw;
        }
    }

    // The original SeedDataAsync method was replaced with the new logic below.
    // The original SeedAsync method is preserved as it handles the initial database creation and checks.
    public async Task SeedDataAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            // Check if API is available and admin user exists through API call
            // A more robust check would involve a dedicated endpoint to check for admin existence.
            // For simplicity, we'll assume a 404 means no admin user exists yet.
            // This part might need adjustment based on actual API behavior.
            var response = await httpClient.GetAsync($"{baseUrl}/api/auth/check-admin"); // Assuming this endpoint returns 404 if no admin user exists.

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Admin user doesn't exist, create through API
                var registerRequest = new
                {
                    Username = "admin",
                    Password = "admin",
                    Role = "Admin" // Assuming the API expects the role as a string
                };

                var jsonContent = JsonSerializer.Serialize(registerRequest);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Assuming the API endpoint for registration is /api/auth/register
                await httpClient.PostAsync($"{baseUrl}/api/auth/register", content);
            }
            else if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Admin user already exists. Skipping seed data creation via API.");
            }
            else
            {
                // Handle other potential API errors (e.g., server error, unauthorized)
                _logger.LogError($"API returned status code {response.StatusCode} when checking for admin user.");
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the application startup
            _logger.LogError(ex, "Could not seed data through API");
            // Consider if a more graceful failure or retry mechanism is needed.
        }
    }
}