namespace RunEF.WebServer.Web.Models
{
    public class TerminalViewModel
    {
        public int ActiveTerminals { get; set; }
        public int RunningProcesses { get; set; }
        public string SystemLoad { get; set; } = "Unknown";
        public List<ProcessInfo> Processes { get; set; } = new List<ProcessInfo>();
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class ProcessInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long MemoryUsage { get; set; }
        public DateTime? StartTime { get; set; }
        public TimeSpan CpuTime { get; set; }
        
        public string FormattedMemoryUsage => FormatBytes(MemoryUsage);
        public string FormattedCpuTime => CpuTime.ToString(@"hh\:mm\:ss");
        public string FormattedStartTime => StartTime?.ToString("dd/MM/yyyy HH:mm:ss") ?? "Unknown";
        
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }
}