using System.Text.Json;

namespace Magazing.Application.DTOs.Assistant;

public class AssistantMessageDto
{
    public string Role { get; set; } = null!;

    public string Content { get; set; } = null!;
}

public class AssistantChatRequest
{
    public List<AssistantMessageDto> Messages { get; set; } = [];
}

public class AssistantProposalDto
{
    public string Kind { get; set; } = null!;

    public string Title { get; set; } = null!;

    public List<string> Lines { get; set; } = [];

    public JsonElement Request { get; set; }
}

public class AssistantReplyDto
{
    public string Message { get; set; } = null!;

    public AssistantProposalDto? Proposal { get; set; }

    public List<string> ToolsUsed { get; set; } = [];
}
