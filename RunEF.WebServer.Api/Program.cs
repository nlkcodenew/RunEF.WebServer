using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RunEF.WebServer.Application;
using RunEF.WebServer.Infrastructure;
using RunEF.WebServer.Api.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure for Windows Service
builder.Host.UseWindowsService();

// Configure Kestrel to use HTTP only
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5265); // HTTP port
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

builder.Services.AddAuthorization();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("ApiSettings:AllowedOrigins").Get<string[]>();
        policy.WithOrigins(allowedOrigins ?? new[] { "http://172.19.111.246:5000", "http://172.19.111.246:5265", "*" })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Removed HTTPS redirection for Windows Service deployment
app.UseCors("AllowAll");

// Add custom middleware for client authentication
app.UseMiddleware<ClientAuthenticationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
