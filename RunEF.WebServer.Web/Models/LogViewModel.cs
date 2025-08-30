using System.ComponentModel.DataAnnotations;

namespace RunEF.WebServer.Web.Models
{
    public class LogViewModel
    {
        public List<LogEntry> LogEntries { get; set; } = new List<LogEntry>();
        public int TotalLogs { get; set; }
        public int ErrorLogs { get; set; }
        public int WarningLogs { get; set; }
        public int InfoLogs { get; set; }
        public int DebugLogs { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public List<string> AvailableSources { get; set; } = new List<string>();
        public List<string> AvailableCategories { get; set; } = new List<string>();
    }

    public class LogEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        [Display(Name = "Level")]
        public string Level { get; set; } = "Information";
        
        [Display(Name = "Source")]
        public string Source { get; set; } = "System";
        
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;
        
        [Display(Name = "Details")]
        public string Details { get; set; } = string.Empty;
        
        [Display(Name = "User ID")]
        public string UserId { get; set; } = "System";
        
        [Display(Name = "Category")]
        public string Category { get; set; } = "General";
        
        // Computed properties
        public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
        public string ShortTimestamp => Timestamp.ToString("HH:mm:ss");
        
        public string LevelBadge => Level switch
        {
            "Error" => "bg-danger",
            "Warning" => "bg-warning",
            "Information" => "bg-info",
            "Debug" => "bg-secondary",
            _ => "bg-primary"
        };
        
        public string LevelIcon => Level switch
        {
            "Error" => "fas fa-exclamation-triangle",
            "Warning" => "fas fa-exclamation-circle",
            "Information" => "fas fa-info-circle",
            "Debug" => "fas fa-bug",
            _ => "fas fa-circle"
        };
        
        public string SourceIcon => Source switch
        {
            "System" => "fas fa-cogs",
            "Database" => "fas fa-database",
            "Authentication" => "fas fa-user-shield",
            "API" => "fas fa-code",
            "SignalR" => "fas fa-broadcast-tower",
            "FileSystem" => "fas fa-folder",
            "Network" => "fas fa-network-wired",
            "Security" => "fas fa-shield-alt",
            _ => "fas fa-circle"
        };
        
        public bool IsRecent => Timestamp > DateTime.Now.AddMinutes(-5);
        public bool IsError => Level == "Error";
        public bool IsWarning => Level == "Warning";
        
        public string TruncatedMessage => Message.Length > 100 ? Message.Substring(0, 100) + "..." : Message;
        public string TruncatedDetails => Details?.Length > 200 ? Details.Substring(0, 200) + "..." : Details ?? string.Empty;
    }

    public class LogFilterViewModel
    {
        [Display(Name = "Log Level")]
        public string Level { get; set; } = string.Empty;
        
        [Display(Name = "Source")]
        public string Source { get; set; } = string.Empty;
        
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;
        
        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        public DateTime? StartDate { get; set; }
        
        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        public DateTime? EndDate { get; set; }
        
        [Display(Name = "Search Text")]
        public string SearchText { get; set; } = string.Empty;
        
        [Display(Name = "Page Size")]
        public int PageSize { get; set; } = 50;
        
        public int Page { get; set; } = 1;
        
        public List<string> AvailableLevels { get; set; } = new List<string> { "Error", "Warning", "Information", "Debug" };
        public List<string> AvailableSources { get; set; } = new List<string>();
        public List<string> AvailableCategories { get; set; } = new List<string>();
    }

    public class LogStatsViewModel
    {
        public int TotalLogs { get; set; }
        public int ErrorLogs { get; set; }
        public int WarningLogs { get; set; }
        public int InfoLogs { get; set; }
        public int DebugLogs { get; set; }
        public int RecentLogs { get; set; }
        
        public Dictionary<string, int> LogsBySource { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> LogsByCategory { get; set; } = new Dictionary<string, int>();
        public Dictionary<int, int> LogsByHour { get; set; } = new Dictionary<int, int>();
        
        public double ErrorPercentage => TotalLogs > 0 ? (double)ErrorLogs / TotalLogs * 100 : 0;
        public double WarningPercentage => TotalLogs > 0 ? (double)WarningLogs / TotalLogs * 100 : 0;
        public double InfoPercentage => TotalLogs > 0 ? (double)InfoLogs / TotalLogs * 100 : 0;
        public double DebugPercentage => TotalLogs > 0 ? (double)DebugLogs / TotalLogs * 100 : 0;
        
        public string HealthStatus
        {
            get
            {
                if (ErrorPercentage > 10) return "Critical";
                if (ErrorPercentage > 5 || WarningPercentage > 20) return "Warning";
                return "Healthy";
            }
        }
        
        public string HealthBadge => HealthStatus switch
        {
            "Critical" => "bg-danger",
            "Warning" => "bg-warning",
            "Healthy" => "bg-success",
            _ => "bg-secondary"
        };
    }

    public class LogExportViewModel
    {
        [Display(Name = "Export Format")]
        public string Format { get; set; } = "json";
        
        [Display(Name = "Include Details")]
        public bool IncludeDetails { get; set; } = true;
        
        [Display(Name = "Date Range")]
        public string DateRange { get; set; } = "all";
        
        [Display(Name = "Log Levels")]
        public List<string> SelectedLevels { get; set; } = new List<string>();
        
        [Display(Name = "Sources")]
        public List<string> SelectedSources { get; set; } = new List<string>();
        
        public List<string> AvailableFormats { get; set; } = new List<string> { "json", "csv", "xml" };
        public List<string> AvailableLevels { get; set; } = new List<string> { "Error", "Warning", "Information", "Debug" };
        public List<string> AvailableSources { get; set; } = new List<string>();
    }

    public class LogSearchResult
    {
        public List<LogEntry> Logs { get; set; } = new List<LogEntry>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public string SearchQuery { get; set; } = string.Empty;
        public TimeSpan SearchDuration { get; set; }
    }

    public class LogActivityViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Details { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        
        public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - Timestamp;
                if (timeSpan.TotalMinutes < 1) return "Just now";
                if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} minutes ago";
                if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hours ago";
                return $"{(int)timeSpan.TotalDays} days ago";
            }
        }
    }
}