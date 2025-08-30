using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RunEF.WebServer.Infrastructure.Data;
using System.Text;
using RunEF.WebServer.Domain.Entities;
using RunEF.WebServer.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Data.SqlClient;
using RunEF.WebServer.Web.Services;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using RunEF.WebServer.Application;
using RunEF.WebServer.Infrastructure;
using RunEF.WebServer.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Register DataSeederService
builder.Services.AddScoped<RunEF.WebServer.Web.Services.DataSeederService>();

// Add SignalR with optimized configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
});

// Register RealTimeService
builder.Services.AddScoped<IRealTimeService, RealTimeService>();

// Configure HTTP Client with optimizations
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("ApiClient", client =>
{
    var apiSettings = builder.Configuration.GetSection("ApiSettings");
    client.BaseAddress = new Uri(apiSettings["BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
{
    MaxConnectionsPerServer = 10
});

// Add InMemory cache with optimized configuration
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1000;
    options.CompactionPercentage = 0.25;
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
});

// Configure Cookie Authentication for Web
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    })
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])),
            NameClaimType = ClaimTypes.Name, // Set the name claim type
            RoleClaimType = ClaimTypes.Role    // Set the role claim type
        };
    });

// Add Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// Add Response Caching
builder.Services.AddResponseCaching();

// Add Output Caching (for .NET 7+)
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(5)));
    options.AddPolicy("Dashboard", builder => builder.Expire(TimeSpan.FromSeconds(30)));
    options.AddPolicy("Clients", builder => builder.Expire(TimeSpan.FromMinutes(2)));
});

// Configure CORS for API access
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Add response compression early in pipeline
app.UseResponseCompression();

// Add response caching
app.UseResponseCaching();

// Add output caching
app.UseOutputCache();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 1 day
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=86400");
    }
});

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// API routes
app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action=Index}/{id?}");

// Web routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Map SignalR hubs
app.MapHub<MonitoringHub>("/monitoringHub");

// Initialize data seeder service (Database-based approach)
using (var scope = app.Services.CreateScope())
{
    var seederService = scope.ServiceProvider.GetRequiredService<DataSeederService>();
    await seederService.SeedAsync();
}

// The user mentioned an issue with `dotnet run --urls="http://0.0.0.0:5001"`.
// The application is currently configured to run on port 5000.
// To address the user's command, we will change the port to 5001.
app.Run("http://0.0.0.0:5001");