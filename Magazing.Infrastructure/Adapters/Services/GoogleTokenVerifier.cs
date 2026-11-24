using Google.Apis.Auth;
using Magazing.Application.Services;
using Microsoft.Extensions.Configuration;

namespace Magazing.Infrastructure.Adapters.Services;

public class GoogleTokenVerifier(IConfiguration configuration) : IGoogleTokenVerifier
{
    private const string InvalidTokenMessage = "No se pudo verificar tu cuenta de Google.";

    private string? ClientId => configuration["Google:ClientId"];

    public bool IsEnabled => !string.IsNullOrWhiteSpace(ClientId);

    public async Task<GoogleIdentity> VerifyAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            throw new KeyNotFoundException("El acceso con Google no está habilitado.");
        }

        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new UnauthorizedAccessException(InvalidTokenMessage);
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [ClientId!]
            });
        }
        catch (Exception ex) when (ex is InvalidJwtException or Newtonsoft.Json.JsonException or FormatException or ArgumentException)
        {
            throw new UnauthorizedAccessException(InvalidTokenMessage);
        }

        if (!payload.EmailVerified || string.IsNullOrWhiteSpace(payload.Email))
        {
            throw new UnauthorizedAccessException("Tu correo de Google no está verificado.");
        }

        return new GoogleIdentity(payload.Email.Trim().ToLowerInvariant());
    }
}
