using Magazing.Application.DTOs.Assistant;
using Magazing.Application.Services;
using MediatR;

namespace Magazing.Application.UseCases.Assistant.Commands;

public record AssistantChatCommand(List<AssistantMessageDto> Messages, string Username, IReadOnlyCollection<string> Permissions)
    : IRequest<AssistantReplyDto>;

internal sealed class AssistantChatCommandHandler(IChatModel chatModel, ISender sender)
    : IRequestHandler<AssistantChatCommand, AssistantReplyDto>
{
    private const int MaxHistory = 12;
    private const int MaxMessageLength = 2000;
    private const int MaxToolRounds = 6;

    public async Task<AssistantReplyDto> Handle(AssistantChatCommand request, CancellationToken cancellationToken)
    {
        var history = (request.Messages ?? [])
            .Where(m => m.Role is "user" or "assistant" && !string.IsNullOrWhiteSpace(m.Content))
            .TakeLast(MaxHistory)
            .ToList();

        if (history.Count == 0 || history[^1].Role != "user")
        {
            throw new InvalidOperationException("Escribe una consulta para el asistente.");
        }

        if (history.Any(m => m.Content.Length > MaxMessageLength))
        {
            throw new InvalidOperationException($"Cada mensaje puede tener como máximo {MaxMessageLength} caracteres.");
        }

        var tools = AssistantTools.For(request.Permissions);
        var executor = new AssistantTools(sender);
        var definitions = tools.Select(t => new ChatToolDefinition(t.Name, t.Description, t.Schema)).ToList();

        var messages = new List<ChatModelMessage> { new("system", SystemPrompt(request.Username, tools)) };
        messages.AddRange(history.Select(m => new ChatModelMessage(m.Role, m.Content)));

        var reply = new AssistantReplyDto();

        for (var round = 0; round < MaxToolRounds; round++)
        {
            var completion = await chatModel.CompleteAsync(messages, definitions, cancellationToken);

            if (completion.ToolCalls.Count == 0)
            {
                reply.Message = string.IsNullOrWhiteSpace(completion.Content) ? "Listo." : completion.Content.Trim();
                return reply;
            }

            messages.Add(new ChatModelMessage("assistant", completion.Content, ToolCalls: completion.ToolCalls));

            foreach (var call in completion.ToolCalls)
            {
                var allowed = tools.Any(t => t.Name == call.Name);
                var result = allowed
                    ? await executor.ExecuteAsync(call.Name, call.ArgumentsJson, cancellationToken)
                    : new ToolResult("""{"error":"El usuario no tiene permiso para esta acción."}""");

                reply.ToolsUsed.Add(call.Name);
                if (result.Proposal is not null)
                {
                    reply.Proposal = result.Proposal;
                }

                messages.Add(new ChatModelMessage("tool", result.Content, ToolCallId: call.Id));
            }
        }

        reply.Message = reply.Proposal is null
            ? "No pude completar la consulta. Intenta formularla de otra manera."
            : "Revisa la propuesta y confírmala si está correcta.";
        return reply;
    }

    private static string SystemPrompt(string username, IReadOnlyList<AssistantTool> tools)
    {
        var canWrite = tools.Any(t => t.IsProposal);
        return $"""
            Eres el asistente de Magazing, un sistema de inventario, compras y ventas para una pyme peruana.
            Hablas con el usuario "{username}". Hoy es {AssistantTools.Today()} (hora de Lima). Moneda: soles (S/). IGV: 18 %.

            Reglas:
            - Responde en español, breve y claro. Usa listas cortas cuando ayuden.
            - Para cualquier dato del negocio (productos, stock, ventas, proveedores, alertas) usa SIEMPRE las herramientas. Nunca inventes cifras, nombres, SKU ni IDs.
            - Si una herramienta devuelve un error o varias coincidencias, explícalo y pregunta al usuario; no adivines.
            - {(canWrite
                ? "Para crear algo usa las herramientas proponer_*: solo preparan una propuesta que el usuario confirma en pantalla. Nunca digas que algo ya se guardó; di que revise y confirme la propuesta."
                : "Este usuario solo puede consultar: si pide crear o modificar algo, explícale que no tiene permiso para hacerlo.")}
            - Si te falta un dato obligatorio (por ejemplo el precio, la categoría o el proveedor), pregúntalo antes de proponer. Usa listar_catalogos para ver valores válidos.
            - No preguntes lo que tiene un valor por defecto; asúmelo y menciónalo en la propuesta: el producto es afecto a IGV, la unidad es NIU, la marca es opcional, el precio de venta incluye IGV, el costo de compra es sin IGV y el almacén de destino es el principal. El usuario puede corregirlo antes de confirmar.
            - Si solo hay una categoría válida, úsala sin preguntar.
            - No ofrezcas acciones que el usuario no puede hacer con sus herramientas.
            - Solo puedes ayudar con Magazing. Si te piden otra cosa, dilo amablemente.
            """;
    }
}
