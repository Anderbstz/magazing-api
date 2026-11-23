using Magazing.Application.DTOs.Users;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Users.Commands;

public record ResetUserAccessCommand(Guid UserId) : IRequest<UserAccessDto>;

internal sealed class ResetUserAccessCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    : IRequestHandler<ResetUserAccessCommand, UserAccessDto>
{
    public async Task<UserAccessDto> Handle(ResetUserAccessCommand request, CancellationToken cancellationToken)
    {
        var users = unitOfWork.Repository<User>();
        var user = await users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("El usuario no existe.");

        if (!user.IsActive)
        {
            throw new InvalidOperationException("El usuario está inactivo. Actívalo antes de reenviar el acceso.");
        }

        var (password, expiresAt) = await UserAccess.IssueTemporaryPasswordAsync(unitOfWork, passwordHasher, user, false, cancellationToken);
        user.UpdatedAt = DateTime.UtcNow;
        users.Update(user);

        var refreshTokens = unitOfWork.Repository<RefreshToken>();
        foreach (var token in await refreshTokens.FindAsync(item => item.UserId == user.UserId && item.RevokedAt == null, cancellationToken))
        {
            token.RevokedAt = DateTime.UtcNow;
            refreshTokens.Update(token);
        }

        await unitOfWork.CompleteAsync(cancellationToken);

        return new UserAccessDto
        {
            User = await UserReadModel.GetAsync(unitOfWork, user.UserId, cancellationToken),
            TemporaryPassword = password,
            ExpiresAt = expiresAt
        };
    }
}
