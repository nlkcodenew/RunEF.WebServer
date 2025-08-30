using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RunEF.WebServer.Web.Models;
using System.Diagnostics;
using System.Text;

namespace RunEF.WebServer.Web.Controllers
{
    [Authorize]
    public class TerminalController : Controller
    {
        private readonly ILogger<TerminalController> _logger;

        public TerminalController(ILogger<TerminalController> logger)
        {
            _logger = logger;
        }

        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public IActionResult Index()
        {
            var model = new TerminalViewModel
            {
                ActiveTerminals = GetActiveTerminalCount(),
                RunningProcesses = Process.GetProcesses().Length,
                SystemLoad = GetSystemLoad(),
                Processes = GetRunningProcesses()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteCommand([FromBody] TerminalCommandRequest request)
        {
            try
            {
                var result = await ExecuteTerminalCommand(request.Command);
                return Json(new { success = true, output = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing terminal command: {Command}", request.Command);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetSystemInfo()
        {
            try
            {
                var info = new
                {
                    activeTerminals = GetActiveTerminalCount(),
                    runningProcesses = Process.GetProcesses().Length,
                    systemLoad = GetSystemLoad(),
                    timestamp = DateTime.Now
                };

                return Json(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system info");
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult KillProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                process.Kill();
                return Json(new { success = true, message = $"Process {processId} terminated" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error killing process {ProcessId}", processId);
                return Json(new { success = false, error = ex.Message });
            }
        }

        private async Task<string> ExecuteTerminalCommand(string command)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    output.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    error.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            var result = output.ToString();
            if (error.Length > 0)
            {
                result += "\n\nErrors:\n" + error.ToString();
            }

            return result;
        }

        private int GetActiveTerminalCount()
        {
            try
            {
                return Process.GetProcesses()
                    .Where(p => p.ProcessName.ToLower().Contains("cmd") ||
                               p.ProcessName.ToLower().Contains("powershell") ||
                               p.ProcessName.ToLower().Contains("pwsh"))
                    .Count();
            }
            catch
            {
                return 0;
            }
        }

        private string GetSystemLoad()
        {
            try
            {
                var processCount = Process.GetProcesses().Length;
                if (processCount < 50) return "Low";
                if (processCount < 100) return "Medium";
                return "High";
            }
            catch
            {
                return "Unknown";
            }
        }

        private List<ProcessInfo> GetRunningProcesses()
        {
            try
            {
                return Process.GetProcesses()
                    .Where(p => !string.IsNullOrEmpty(p.ProcessName))
                    .OrderByDescending(p => p.WorkingSet64)
                    .Take(20)
                    .Select(p => new ProcessInfo
                    {
                        Id = p.Id,
                        Name = p.ProcessName,
                        MemoryUsage = p.WorkingSet64,
                        StartTime = GetProcessStartTime(p),
                        CpuTime = GetProcessCpuTime(p)
                    })
                    .ToList();
            }
            catch
            {
                return new List<ProcessInfo>();
            }
        }

        private DateTime? GetProcessStartTime(Process process)
        {
            try
            {
                return process.StartTime;
            }
            catch
            {
                return null;
            }
        }

        private TimeSpan GetProcessCpuTime(Process process)
        {
            try
            {
                return process.TotalProcessorTime;
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }
    }

    public class TerminalCommandRequest
    {
        public string Command { get; set; } = string.Empty;
    }
}