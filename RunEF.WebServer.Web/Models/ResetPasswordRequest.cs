
using System.ComponentModel.DataAnnotations;

namespace RunEF.WebServer.Web.Models;

public class ResetPasswordRequest
{
    [Required]
    public required string Username { get; set; }

    [Required]
    [Display(Name = "Old Password")]
    public required string OldPassword { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string NewPassword { get; set; }

    [Required]
    [Compare("NewPassword")]
    public required string ConfirmPassword { get; set; }
}
