namespace Magazing.Application.DTOs.Users;

public class SessionDto
{
    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime LastActivityAt { get; set; }

    public DateTime ExpiresAt { get; set; }
}
