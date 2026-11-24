using Magazing.Application.DTOs.Users;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Users.Commands;

public record CreateUserCommand(
    string Username,
    string Email,
    string FullName,
    Guid? BranchId,
    List<Guid>? RoleIds) : IRequest<UserAccessDto>;

internal sealed class CreateUserCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    : IRequestHandler<CreateUserCommand, UserAccessDto>
{
    public async Task<UserAccessDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var username = request.Username?.Trim() ?? string.Empty;
        var email = request.Email?.Trim().ToLower() ?? string.Empty;
        var fullName = request.FullName?.Trim() ?? string.Empty;
        var roleIds = request.RoleIds?.Distinct().ToList() ?? [];

        if (username.Length < 3)
        {
            throw new InvalidOperationException("El usuario debe tener al menos 3 caracteres.");
        }

        if (!email.Contains('@'))
        {
            throw new InvalidOperationException("El correo no es válido.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("El nombre completo es obligatorio.");
        }

        var users = unitOfWork.Repository<User>();

        if (await users.AnyAsync(item => item.Username == username, cancellationToken))
        {
            throw new InvalidOperationException("El nombre de usuario ya está registrado.");
        }

        if (await users.AnyAsync(item => item.Email == email, cancellationToken))
        {
            throw new InvalidOperationException("El correo ya está registrado.");
        }

        if (request.BranchId is { } branchId
            && !await unitOfWork.Repository<Branch>().AnyAsync(item => item.BranchId == branchId, cancellationToken))
        {
            throw new InvalidOperationException("La sucursal indicada no existe.");
        }

        if (roleIds.Count == 0)
        {
            throw new InvalidOperationException("Asigna al menos un rol: sin rol el usuario no puede usar ningún módulo.");
        }

        await UserReadModel.EnsureRolesExistAsync(unitOfWork, roleIds, cancellationToken);
        await AdminGuard.EnsureAdminIsExclusiveAsync(unitOfWork, roleIds, cancellationToken);

        var now = DateTime.UtcNow;
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = string.Empty,
            FullName = fullName,
            BranchId = request.BranchId,
            IsActive = true,
            EmailConfirmed = false,
            CreatedAt = now
        };

        var (password, expiresAt) = await UserAccess.IssueTemporaryPasswordAsync(unitOfWork, passwordHasher, user, true, cancellationToken);

        await users.AddAsync(user, cancellationToken);

        foreach (var roleId in roleIds)
        {
            await unitOfWork.Repository<UserRole>().AddAsync(new UserRole
            {
                UserId = user.UserId,
                RoleId = roleId,
                AssignedAt = now
            }, cancellationToken);
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
