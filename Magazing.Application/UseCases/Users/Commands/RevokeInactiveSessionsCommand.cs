using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Users.Commands;

public record RevokeInactiveSessionsCommand(int InactiveHours, Guid KeepSessionId) : IRequest<int>;

internal sealed class RevokeInactiveSessionsCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RevokeInactiveSessionsCommand, int>
{
    private const int MinInactiveHours = 1;

    public async Task<int> Handle(RevokeInactiveSessionsCommand request, CancellationToken cancellationToken)
    {
        if (request.InactiveHours < MinInactiveHours)
        {
            throw new InvalidOperationException($"Indica al menos {MinInactiveHours} hora de inactividad.");
        }

        var now = DateTime.UtcNow;
        var threshold = now.AddHours(-request.InactiveHours);
        var keep = request.KeepSessionId;

        var tokens = unitOfWork.Repository<RefreshToken>();
        var stale = (await tokens.FindAsync(
                item => item.RevokedAt == null && item.ExpiresAt > now && item.CreatedAt < threshold && item.RefreshTokenId != keep,
                cancellationToken))
            .ToList();

        foreach (var token in stale)
        {
            token.RevokedAt = now;
            tokens.Update(token);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return stale.Count;
    }
}
