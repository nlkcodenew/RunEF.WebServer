using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RunEF.WebServer.Web.Hubs;
using RunEF.WebServer.Web.Models;
using System.Diagnostics;
using System.Text.Json;

namespace RunEF.WebServer.Web.Controllers
{
    [Authorize]
    public class LogController : Controller
    {
        private readonly ILogger<LogController> _logger;
        private readonly IHubContext<MonitoringHub> _hubContext;
        private static readonly List<LogEntry> _logEntries = new List<LogEntry>();
        private static readonly object _lockObject = new object();

        public LogController(ILogger<LogController> logger, IHubContext<MonitoringHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)]
        public IActionResult Index()
        {
            var model = new LogViewModel
            {
                LogEntries = GetFilteredLogs(),
                TotalLogs = _logEntries.Count,
                ErrorLogs = _logEntries.Count(l => l.Level == "Error"),
                WarningLogs = _logEntries.Count(l => l.Level == "Warning"),
                InfoLogs = _logEntries.Count(l => l.Level == "Information"),
                LastUpdated = DateTime.Now
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult GetLogs(string level = "", string source = "", DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 50)
        {
            try
            {
                var filteredLogs = GetFilteredLogs(level, source, startDate, endDate);
                var totalCount = filteredLogs.Count;
                var pagedLogs = filteredLogs
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Json(new
                {
                    success = true,
                    logs = pagedLogs,
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving logs");
                return Json(new { success = false, error = "Failed to retrieve logs" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddLog([FromBody] AddLogRequest request)
        {
            try
            {
                var logEntry = new LogEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    Level = request.Level ?? "Information",
                    Source = request.Source ?? "Manual",
                    Message = request.Message,
                    Details = request.Details,
                    UserId = User.Identity?.Name ?? "System",
                    Category = request.Category ?? "General"
                };

                lock (_lockObject)
                {
                    _logEntries.Add(logEntry);
                    
                    // Keep only last 10000 logs to prevent memory issues
                    if (_logEntries.Count > 10000)
                    {
                        _logEntries.RemoveRange(0, _logEntries.Count - 10000);
                    }
                }

                // Broadcast to all connected clients
                await _hubContext.Clients.Group("Monitoring").SendAsync("LogAdded", logEntry);

                _logger.LogInformation($"Log entry added: {request.Message}");
                return Json(new { success = true, message = "Log entry added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding log entry");
                return Json(new { success = false, error = "Failed to add log entry" });
            }
        }

        [HttpPost]
        public IActionResult ClearLogs([FromBody] ClearLogsRequest request)
        {
            try
            {
                lock (_lockObject)
                {
                    if (request.Level != null)
                    {
                        _logEntries.RemoveAll(l => l.Level == request.Level);
                    }
                    else if (request.Source != null)
                    {
                        _logEntries.RemoveAll(l => l.Source == request.Source);
                    }
                    else if (request.Category != null)
                    {
                        _logEntries.RemoveAll(l => l.Category == request.Category);
                    }
                    else
                    {
                        _logEntries.Clear();
                    }
                }

                _logger.LogInformation($"Logs cleared by {User.Identity?.Name}");
                return Json(new { success = true, message = "Logs cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing logs");
                return Json(new { success = false, error = "Failed to clear logs" });
            }
        }

        [HttpGet]
        public IActionResult ExportLogs(string level = "", string source = "", DateTime? startDate = null, DateTime? endDate = null, string format = "json")
        {
            try
            {
                var filteredLogs = GetFilteredLogs(level, source, startDate, endDate);
                var fileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (format.ToLower() == "csv")
                {
                    var csv = GenerateCsv(filteredLogs);
                    return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"{fileName}.csv");
                }
                else
                {
                    var json = JsonSerializer.Serialize(filteredLogs, new JsonSerializerOptions { WriteIndented = true });
                    return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", $"{fileName}.json");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting logs");
                return BadRequest("Failed to export logs");
            }
        }

        [HttpGet]
        public IActionResult GetLogStats()
        {
            try
            {
                var stats = new
                {
                    totalLogs = _logEntries.Count,
                    errorLogs = _logEntries.Count(l => l.Level == "Error"),
                    warningLogs = _logEntries.Count(l => l.Level == "Warning"),
                    infoLogs = _logEntries.Count(l => l.Level == "Information"),
                    debugLogs = _logEntries.Count(l => l.Level == "Debug"),
                    recentLogs = _logEntries.Where(l => l.Timestamp > DateTime.Now.AddHours(-1)).Count(),
                    logsBySource = _logEntries.GroupBy(l => l.Source).ToDictionary(g => g.Key, g => g.Count()),
                    logsByCategory = _logEntries.GroupBy(l => l.Category).ToDictionary(g => g.Key, g => g.Count()),
                    logsByHour = _logEntries
                        .Where(l => l.Timestamp > DateTime.Now.AddDays(-1))
                        .GroupBy(l => l.Timestamp.Hour)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return Json(new { success = true, stats = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting log statistics");
                return Json(new { success = false, error = "Failed to get log statistics" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SimulateSystemLogs()
        {
            try
            {
                var random = new Random();
                var sources = new[] { "System", "Database", "Authentication", "API", "SignalR", "FileSystem" };
                var categories = new[] { "Security", "Performance", "Error", "User Activity", "System Health" };
                var levels = new[] { "Information", "Warning", "Error", "Debug" };
                var messages = new[]
                {
                    "User login successful",
                    "Database connection established",
                    "File upload completed",
                    "Cache cleared",
                    "Background task started",
                    "Memory usage high",
                    "Disk space low",
                    "Network timeout occurred",
                    "Authentication failed",
                    "Service restarted"
                };

                for (int i = 0; i < 20; i++)
                {
                    var logEntry = new LogEntry
                    {
                        Id = Guid.NewGuid().ToString(),
                        Timestamp = DateTime.Now.AddSeconds(-random.Next(0, 3600)),
                        Level = levels[random.Next(levels.Length)],
                        Source = sources[random.Next(sources.Length)],
                        Message = messages[random.Next(messages.Length)],
                        Details = $"Additional details for log entry {i + 1}",
                        UserId = "System",
                        Category = categories[random.Next(categories.Length)]
                    };

                    lock (_lockObject)
                    {
                        _logEntries.Add(logEntry);
                    }

                    // Broadcast to all connected clients
                    await _hubContext.Clients.Group("Monitoring").SendAsync("LogAdded", logEntry);
                    
                    await Task.Delay(100); // Small delay between logs
                }

                return Json(new { success = true, message = "Simulated logs generated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error simulating system logs");
                return Json(new { success = false, error = "Failed to simulate logs" });
            }
        }

        private List<LogEntry> GetFilteredLogs(string level = "", string source = "", DateTime? startDate = null, DateTime? endDate = null)
        {
            lock (_lockObject)
            {
                var query = _logEntries.AsQueryable();

                if (!string.IsNullOrEmpty(level))
                    query = query.Where(l => l.Level == level);

                if (!string.IsNullOrEmpty(source))
                    query = query.Where(l => l.Source.Contains(source, StringComparison.OrdinalIgnoreCase));

                if (startDate.HasValue)
                    query = query.Where(l => l.Timestamp >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(l => l.Timestamp <= endDate.Value);

                return query.OrderByDescending(l => l.Timestamp).ToList();
            }
        }

        private string GenerateCsv(List<LogEntry> logs)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Timestamp,Level,Source,Category,Message,Details,UserId");

            foreach (var log in logs)
            {
                csv.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.Level},{log.Source},{log.Category},\"{log.Message}\",\"{log.Details}\",{log.UserId}");
            }

            return csv.ToString();
        }

        // Initialize with some sample logs
        static LogController()
        {
            _logEntries.AddRange(new[]
            {
                new LogEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now.AddMinutes(-30),
                    Level = "Information",
                    Source = "System",
                    Message = "Application started successfully",
                    Details = "RunEF WebServer application initialized",
                    UserId = "System",
                    Category = "System Health"
                },
                new LogEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now.AddMinutes(-25),
                    Level = "Information",
                    Source = "Database",
                    Message = "Database connection established",
                    Details = "Connected to SQL Server database",
                    UserId = "System",
                    Category = "Database"
                },
                new LogEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now.AddMinutes(-20),
                    Level = "Warning",
                    Source = "Authentication",
                    Message = "Failed login attempt",
                    Details = "Invalid credentials provided for user 'admin'",
                    UserId = "Unknown",
                    Category = "Security"
                }
            });
        }
    }

    // Request models
    public class AddLogRequest
    {
        public string Level { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public string Category { get; set; }
    }

    public class ClearLogsRequest
    {
        public string Level { get; set; }
        public string Source { get; set; }
        public string Category { get; set; }
    }
}