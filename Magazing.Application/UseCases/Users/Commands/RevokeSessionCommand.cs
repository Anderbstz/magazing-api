using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Users.Commands;

public record RevokeSessionCommand(Guid SessionId) : IRequest;

internal sealed class RevokeSessionCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RevokeSessionCommand>
{
    public async Task Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        var tokens = unitOfWork.Repository<RefreshToken>();
        var token = await tokens.GetByIdAsync(request.SessionId, cancellationToken)
            ?? throw new KeyNotFoundException("La sesión no existe.");

        if (token.RevokedAt is not null)
        {
            return;
        }

        token.RevokedAt = DateTime.UtcNow;
        tokens.Update(token);
        await unitOfWork.CompleteAsync(cancellationToken);
    }
}
