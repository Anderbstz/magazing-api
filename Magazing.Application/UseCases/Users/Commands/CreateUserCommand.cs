using Magazing.Application.DTOs.Users;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Users.Commands;

public record CreateUserCommand(
    string Username,
    string Email,
    string Password,
    string FullName,
    Guid? BranchId,
    List<Guid>? RoleIds) : IRequest<UserDto>;

internal sealed class CreateUserCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    : IRequestHandler<CreateUserCommand, UserDto>
{
    private const int MinPasswordLength = 8;

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
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
            throw new InvalidOperationException("El correo no es valido.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("El nombre completo es obligatorio.");
        }

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < MinPasswordLength)
        {
            throw new InvalidOperationException($"La contrasena debe tener al menos {MinPasswordLength} caracteres.");
        }

        var users = unitOfWork.Repository<User>();

        if (await users.AnyAsync(item => item.Username == username, cancellationToken))
        {
            throw new InvalidOperationException("El nombre de usuario ya esta registrado.");
        }

        if (await users.AnyAsync(item => item.Email == email, cancellationToken))
        {
            throw new InvalidOperationException("El correo ya esta registrado.");
        }

        if (request.BranchId is { } branchId
            && !await unitOfWork.Repository<Branch>().AnyAsync(item => item.BranchId == branchId, cancellationToken))
        {
            throw new InvalidOperationException("La sucursal indicada no existe.");
        }

        await UserReadModel.EnsureRolesExistAsync(unitOfWork, roleIds, cancellationToken);

        var now = DateTime.UtcNow;
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = fullName,
            BranchId = request.BranchId,
            IsActive = true,
            EmailConfirmed = false,
            CreatedAt = now
        };

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

        return await UserReadModel.GetAsync(unitOfWork, user.UserId, cancellationToken);
    }
}
