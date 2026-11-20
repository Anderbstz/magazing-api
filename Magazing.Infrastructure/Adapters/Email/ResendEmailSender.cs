using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Magazing.Application.Services;
using Microsoft.Extensions.Options;

namespace Magazing.Infrastructure.Adapters.Email;

public class ResendEmailSender(HttpClient httpClient, IOptions<EmailOptions> options) : IEmailSender
{
    public async Task<string> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "emails")
        {
            Content = JsonContent.Create(new ResendRequest(options.Value.From, [message.To], message.Subject, message.HtmlBody))
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ResendApiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Resend respondio {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<ResendResponse>(cancellationToken);
        return result?.Id ?? throw new InvalidOperationException("Resend no devolvio el id del correo.");
    }

    private sealed record ResendRequest(
        [property: JsonPropertyName("from")] string From,
        [property: JsonPropertyName("to")] string[] To,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("html")] string Html);

    private sealed record ResendResponse([property: JsonPropertyName("id")] string Id);
}
