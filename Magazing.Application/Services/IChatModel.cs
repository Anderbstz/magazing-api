namespace Magazing.Application.Services;

public interface IChatModel
{
    Task<ChatCompletion> CompleteAsync(
        IReadOnlyList<ChatModelMessage> messages,
        IReadOnlyList<ChatToolDefinition> tools,
        CancellationToken cancellationToken);
}

public sealed record ChatModelMessage(string Role, string? Content, string? ToolCallId = null, IReadOnlyList<ChatToolCall>? ToolCalls = null);

public sealed record ChatToolDefinition(string Name, string Description, string ParametersJsonSchema);

public sealed record ChatToolCall(string Id, string Name, string ArgumentsJson);

public sealed record ChatCompletion(string? Content, IReadOnlyList<ChatToolCall> ToolCalls);
