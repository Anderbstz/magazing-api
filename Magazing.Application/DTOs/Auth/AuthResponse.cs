namespace Magazing.Application.DTOs.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = null!;

    public DateTime AccessTokenExpiresAt { get; set; }

    public string RefreshToken { get; set; } = null!;

    public DateTime RefreshTokenExpiresAt { get; set; }

    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    public List<string> Roles { get; set; } = [];

    public List<string> Permissions { get; set; } = [];
}
