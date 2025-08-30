using System.ComponentModel.DataAnnotations;

namespace RunEF.WebServer.Web.Models
{
    public class UserManagementViewModel
    {
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public int TotalUsers { get; set; }
        public int OnlineUsers { get; set; }
        public int LockedUsers { get; set; }
        public List<string> AvailableRoles { get; set; } = new List<string>();
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        
        [Display(Name = "Username")]
        public string UserName { get; set; }
        
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }
        
        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }
        
        [Display(Name = "Lockout Enabled")]
        public bool LockoutEnabled { get; set; }
        
        [Display(Name = "Lockout End")]
        public DateTimeOffset? LockoutEnd { get; set; }
        
        [Display(Name = "Access Failed Count")]
        public int AccessFailedCount { get; set; }
        
        public List<string> Roles { get; set; } = new List<string>();
        
        [Display(Name = "Online Status")]
        public bool IsOnline { get; set; }
        
        [Display(Name = "Last Login")]
        public DateTime? LastLoginTime { get; set; }
        
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        
        // Computed properties
        public string StatusBadge => IsOnline ? "bg-success" : "bg-secondary";
        public string StatusText => IsOnline ? "Online" : "Offline";
        
        public bool IsLocked => LockoutEnd.HasValue && LockoutEnd > DateTimeOffset.Now;
        public string LockStatusBadge => IsLocked ? "bg-danger" : "bg-success";
        public string LockStatusText => IsLocked ? "Locked" : "Active";
        
        public string RolesDisplay => Roles.Any() ? string.Join(", ", Roles) : "No roles";
        
        public string FormattedLastLogin => LastLoginTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never";
        public string FormattedCreatedDate => CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");
        public string FormattedLockoutEnd => LockoutEnd?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Not locked";
    }

    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }
        
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        
        [Display(Name = "Role")]
        public string SelectedRole { get; set; }
        
        public List<string> AvailableRoles { get; set; } = new List<string>();
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }
        
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }
        
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }
        
        [Display(Name = "Lockout Enabled")]
        public bool LockoutEnabled { get; set; }
        
        [Display(Name = "Selected Roles")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
        
        public List<string> AvailableRoles { get; set; } = new List<string>();
    }

    public class UserActivityViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public bool Success { get; set; }
        
        public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        public string StatusBadge => Success ? "bg-success" : "bg-danger";
        public string StatusText => Success ? "Success" : "Failed";
    }

    public class UserStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int OnlineUsers { get; set; }
        public int LockedUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        
        public Dictionary<string, int> UsersByRole { get; set; } = new Dictionary<string, int>();
        public List<UserActivityViewModel> RecentActivities { get; set; } = new List<UserActivityViewModel>();
        
        public double ActiveUserPercentage => TotalUsers > 0 ? (double)ActiveUsers / TotalUsers * 100 : 0;
        public double OnlineUserPercentage => TotalUsers > 0 ? (double)OnlineUsers / TotalUsers * 100 : 0;
    }
}