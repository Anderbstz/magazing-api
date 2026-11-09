using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Magazing.Application.DTOs.Auth;
using Magazing.Application.Security;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Magazing.Infrastructure.Adapters.Services;

public class AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IConfiguration configuration) : IAuthService
{
    private const string InvalidCredentialsMessage = "Credenciales invalidas.";
    private const string InvalidRefreshTokenMessage = "El refresh token es invalido o expiro.";

    public async Task<AuthResponse> LoginAsync(string username, string password, string? ipAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            throw new UnauthorizedAccessException(InvalidCredentialsMessage);
        }

        var users = unitOfWork.Repository<User>();
        var user = await users.FirstOrDefaultAsync(item => item.Username == username.Trim(), cancellationToken);

        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException(InvalidCredentialsMessage);
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("El usuario esta inactivo.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        users.Update(user);

        return await IssueTokensAsync(user, GenerateRefreshToken(), ipAddress, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedAccessException(InvalidRefreshTokenMessage);
        }

        var refreshTokens = unitOfWork.Repository<RefreshToken>();
        var tokenHash = HashToken(refreshToken);
        var storedToken = await refreshTokens.FirstOrDefaultAsync(item => item.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || storedToken.RevokedAt is not null || storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException(InvalidRefreshTokenMessage);
        }

        var user = await unitOfWork.Repository<User>().FirstOrDefaultAsync(item => item.UserId == storedToken.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException(InvalidRefreshTokenMessage);
        }

        var newRefreshToken = GenerateRefreshToken();

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByTokenHash = HashToken(newRefreshToken);
        refreshTokens.Update(storedToken);

        return await IssueTokensAsync(user, newRefreshToken, ipAddress, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var refreshTokens = unitOfWork.Repository<RefreshToken>();
        var tokenHash = HashToken(refreshToken);
        var storedToken = await refreshTokens.FirstOrDefaultAsync(item => item.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || storedToken.RevokedAt is not null)
        {
            return;
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        refreshTokens.Update(storedToken);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, string refreshToken, string? ipAddress, CancellationToken cancellationToken)
    {
        var (roles, permissions) = await GetRolesAndPermissionsAsync(user.UserId, cancellationToken);

        var now = DateTime.UtcNow;
        var accessTokenExpiresAt = now.AddMinutes(GetIntSetting("Jwt:AccessTokenMinutes", 30));
        var refreshTokenExpiresAt = now.AddDays(GetIntSetting("Jwt:RefreshTokenDays", 7));

        await unitOfWork.Repository<RefreshToken>().AddAsync(new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = user.UserId,
            TokenHash = HashToken(refreshToken),
            ExpiresAt = refreshTokenExpiresAt,
            CreatedAt = now,
            CreatedByIp = ipAddress
        }, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = CreateAccessToken(user, roles, permissions, accessTokenExpiresAt),
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            UserId = user.UserId,
            Username = user.Username,
            Roles = roles,
            Permissions = permissions
        };
    }

    private async Task<(List<string> Roles, List<string> Permissions)> GetRolesAndPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var roleIds = (await unitOfWork.Repository<UserRole>().FindAsync(item => item.UserId == userId, cancellationToken))
            .Select(item => item.RoleId)
            .ToList();

        var roles = (await unitOfWork.Repository<Role>().FindAsync(item => roleIds.Contains(item.RoleId), cancellationToken))
            .Select(item => item.Name)
            .OrderBy(name => name)
            .ToList();

        var permissionIds = (await unitOfWork.Repository<RolePermission>().FindAsync(item => roleIds.Contains(item.RoleId), cancellationToken))
            .Select(item => item.PermissionId)
            .Distinct()
            .ToList();

        var permissions = (await unitOfWork.Repository<Permission>().FindAsync(item => permissionIds.Contains(item.PermissionId), cancellationToken))
            .Select(item => item.Code)
            .OrderBy(code => code)
            .ToList();

        return (roles, permissions);
    }

    private string CreateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions, DateTime expiresAt)
    {
        var secretKey = configuration["Jwt:SecretKey"];
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("La configuracion de JWT esta incompleta.");
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(permissions.Select(permission => new Claim(Permissions.ClaimType, permission)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int GetIntSetting(string key, int defaultValue)
    {
        return int.TryParse(configuration[key], out var value) && value > 0 ? value : defaultValue;
    }

    private static string GenerateRefreshToken()
    {
        return Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
    }

    private static string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
