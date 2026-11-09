using Magazing.Application.DTOs.Roles;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Roles.Commands;

public record CreateRoleCommand(string Name, string? Description, List<Guid>? PermissionIds) : IRequest<RoleDto>;

internal sealed class CreateRoleCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateRoleCommand, RoleDto>
{
    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim().ToUpper() ?? string.Empty;
        var permissionIds = request.PermissionIds?.Distinct().ToList() ?? [];

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("El nombre del rol es obligatorio.");
        }

        var roles = unitOfWork.Repository<Role>();

        if (await roles.AnyAsync(item => item.Name == name, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe un rol con ese nombre.");
        }

        await RoleReadModel.EnsurePermissionsExistAsync(unitOfWork, permissionIds, cancellationToken);

        var now = DateTime.UtcNow;
        var role = new Role
        {
            RoleId = Guid.NewGuid(),
            Name = name,
            Description = request.Description?.Trim(),
            CreatedAt = now
        };

        await roles.AddAsync(role, cancellationToken);

        foreach (var permissionId in permissionIds)
        {
            await unitOfWork.Repository<RolePermission>().AddAsync(new RolePermission
            {
                RoleId = role.RoleId,
                PermissionId = permissionId,
                GrantedAt = now
            }, cancellationToken);
        }

        await unitOfWork.CompleteAsync(cancellationToken);

        return await RoleReadModel.GetByIdAsync(unitOfWork, role.RoleId, cancellationToken);
    }
}
