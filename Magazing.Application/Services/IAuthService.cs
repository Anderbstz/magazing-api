using Magazing.Application.DTOs.Auth;

namespace Magazing.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(string username, string password, string? ipAddress, CancellationToken cancellationToken = default);

    Task<AuthResponse> RefreshAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default);

    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
