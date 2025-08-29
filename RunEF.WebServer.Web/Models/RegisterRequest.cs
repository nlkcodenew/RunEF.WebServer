
using System.ComponentModel.DataAnnotations;

namespace RunEF.WebServer.Web.Models;

public class RegisterRequest
{
    [Required]
    [StringLength(100)]
    public required string Username { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string Password { get; set; }

    [Required]
    [Compare("Password")]
    public required string ConfirmPassword { get; set; }

    [StringLength(50)]
    public string? FirstName { get; set; }

    [StringLength(50)]
    public string? LastName { get; set; }
}
