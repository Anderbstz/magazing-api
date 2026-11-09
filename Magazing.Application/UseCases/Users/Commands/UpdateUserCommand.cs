using Magazing.Application.DTOs.Users;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Users.Commands;

public record UpdateUserCommand(Guid UserId, string Email, string FullName, Guid? BranchId, bool IsActive) : IRequest<UserDto>;

internal sealed class UpdateUserCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim().ToLower() ?? string.Empty;
        var fullName = request.FullName?.Trim() ?? string.Empty;

        if (!email.Contains('@'))
        {
            throw new InvalidOperationException("El correo no es valido.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("El nombre completo es obligatorio.");
        }

        var users = unitOfWork.Repository<User>();
        var user = await users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("El usuario no existe.");

        if (await users.AnyAsync(item => item.Email == email && item.UserId != request.UserId, cancellationToken))
        {
            throw new InvalidOperationException("El correo ya esta registrado.");
        }

        if (request.BranchId is { } branchId
            && !await unitOfWork.Repository<Branch>().AnyAsync(item => item.BranchId == branchId, cancellationToken))
        {
            throw new InvalidOperationException("La sucursal indicada no existe.");
        }

        var now = DateTime.UtcNow;

        if (user.IsActive && !request.IsActive)
        {
            var refreshTokens = unitOfWork.Repository<RefreshToken>();
            var activeTokens = await refreshTokens.FindAsync(
                item => item.UserId == request.UserId && item.RevokedAt == null,
                cancellationToken);

            foreach (var token in activeTokens)
            {
                token.RevokedAt = now;
                refreshTokens.Update(token);
            }
        }

        user.Email = email;
        user.FullName = fullName;
        user.BranchId = request.BranchId;
        user.IsActive = request.IsActive;
        user.UpdatedAt = now;

        await unitOfWork.CompleteAsync(cancellationToken);

        return await UserReadModel.GetAsync(unitOfWork, user.UserId, cancellationToken);
    }
}
