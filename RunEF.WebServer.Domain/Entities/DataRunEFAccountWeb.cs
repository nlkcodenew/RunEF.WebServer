namespace RunEF.WebServer.Domain.Entities;

public class DataRunEFAccountWeb
{
    public int Id { get; set; }
    public required string Username { get; init; }
    public required string PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = "User";
    public DateTime? Changedate { get; set; }
    public string? OldPassword { get; set; }
    public string? GroupControl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public string? ComputerCode { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public DataRunEFAccountWeb(string username, string passwordHash)
    {
        Username = username;
        PasswordHash = passwordHash;
    }

    public DataRunEFAccountWeb() { }

    public bool VerifyPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }

    public void SetPassword(string password)
    {
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        Changedate = DateTime.UtcNow;
    }


}