using Magazing.Application.DTOs.Roles;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Roles.Commands;

public record UpdateRolePermissionsCommand(Guid RoleId, List<Guid> PermissionIds) : IRequest<RoleDto>;

internal sealed class UpdateRolePermissionsCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateRolePermissionsCommand, RoleDto>
{
    public async Task<RoleDto> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var permissionIds = request.PermissionIds?.Distinct().ToList() ?? [];

        if (!await unitOfWork.Repository<Role>().AnyAsync(item => item.RoleId == request.RoleId, cancellationToken))
        {
            throw new KeyNotFoundException("El rol no existe.");
        }

        await RoleReadModel.EnsurePermissionsExistAsync(unitOfWork, permissionIds, cancellationToken);

        var rolePermissions = unitOfWork.Repository<RolePermission>();
        var current = (await rolePermissions.FindAsync(item => item.RoleId == request.RoleId, cancellationToken)).ToList();

        foreach (var rolePermission in current.Where(item => !permissionIds.Contains(item.PermissionId)))
        {
            rolePermissions.Delete(rolePermission);
        }

        var currentIds = current.Select(item => item.PermissionId).ToHashSet();
        var now = DateTime.UtcNow;

        foreach (var permissionId in permissionIds.Where(id => !currentIds.Contains(id)))
        {
            await rolePermissions.AddAsync(new RolePermission
            {
                RoleId = request.RoleId,
                PermissionId = permissionId,
                GrantedAt = now
            }, cancellationToken);
        }

        await unitOfWork.CompleteAsync(cancellationToken);

        return await RoleReadModel.GetByIdAsync(unitOfWork, request.RoleId, cancellationToken);
    }
}
