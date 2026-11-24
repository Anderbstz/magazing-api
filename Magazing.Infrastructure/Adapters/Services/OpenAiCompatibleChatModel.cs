using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Magazing.Application.Services;
using Microsoft.Extensions.Options;

namespace Magazing.Infrastructure.Adapters.Services;

public sealed class AssistantOptions
{
    public string BaseUrl { get; set; } = "https://api.deepseek.com";

    public string Model { get; set; } = "deepseek-chat";

    public string ApiKey { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 60;
}

public sealed class OpenAiCompatibleChatModel(HttpClient httpClient, IOptions<AssistantOptions> options) : IChatModel
{
    public async Task<ChatCompletion> CompleteAsync(
        IReadOnlyList<ChatModelMessage> messages,
        IReadOnlyList<ChatToolDefinition> tools,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException("El asistente no está configurado: falta Assistant:ApiKey.");
        }

        var payload = new JsonObject
        {
            ["model"] = settings.Model,
            ["temperature"] = 0.2,
            ["messages"] = new JsonArray(messages.Select(ToJson).ToArray<JsonNode?>())
        };

        if (tools.Count > 0)
        {
            payload["tools"] = new JsonArray(tools.Select(tool => (JsonNode)new JsonObject
            {
                ["type"] = "function",
                ["function"] = new JsonObject
                {
                    ["name"] = tool.Name,
                    ["description"] = tool.Description,
                    ["parameters"] = JsonNode.Parse(tool.ParametersJsonSchema)
                }
            }).ToArray<JsonNode?>());
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, settings.BaseUrl.TrimEnd('/') + "/chat/completions")
        {
            Content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException("El asistente tardó demasiado en responder. Intenta de nuevo.");
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("No se pudo conectar con el servicio del asistente.");
        }

        using (response)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => "La key del asistente no es válida.",
                    System.Net.HttpStatusCode.PaymentRequired => "La cuenta del asistente no tiene saldo.",
                    System.Net.HttpStatusCode.TooManyRequests => "El asistente está recibiendo demasiadas consultas. Espera un momento.",
                    _ => $"El servicio del asistente respondió con error ({(int)response.StatusCode})."
                });
            }

            return Parse(body);
        }
    }

    private static JsonNode ToJson(ChatModelMessage message)
    {
        var node = new JsonObject { ["role"] = message.Role, ["content"] = message.Content };

        if (message.ToolCallId is not null)
        {
            node["tool_call_id"] = message.ToolCallId;
        }

        if (message.ToolCalls is { Count: > 0 })
        {
            node["tool_calls"] = new JsonArray(message.ToolCalls.Select(call => (JsonNode)new JsonObject
            {
                ["id"] = call.Id,
                ["type"] = "function",
                ["function"] = new JsonObject { ["name"] = call.Name, ["arguments"] = call.ArgumentsJson }
            }).ToArray<JsonNode?>());
        }

        return node;
    }

    private static ChatCompletion Parse(string body)
    {
        using var document = JsonDocument.Parse(body);
        var message = document.RootElement.GetProperty("choices")[0].GetProperty("message");

        var content = message.TryGetProperty("content", out var contentElement) && contentElement.ValueKind == JsonValueKind.String
            ? contentElement.GetString()
            : null;

        var calls = new List<ChatToolCall>();
        if (message.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.ValueKind == JsonValueKind.Array)
        {
            foreach (var call in toolCalls.EnumerateArray())
            {
                var function = call.GetProperty("function");
                calls.Add(new ChatToolCall(
                    call.GetProperty("id").GetString() ?? Guid.NewGuid().ToString(),
                    function.GetProperty("name").GetString() ?? string.Empty,
                    function.TryGetProperty("arguments", out var args) ? args.GetString() ?? "{}" : "{}"));
            }
        }

        return new ChatCompletion(content, calls);
    }
}
