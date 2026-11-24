namespace Magazing.Application.Services;

public interface IGoogleTokenVerifier
{
    bool IsEnabled { get; }

    Task<GoogleIdentity> VerifyAsync(string idToken, CancellationToken cancellationToken = default);
}

public sealed record GoogleIdentity(string Email);
