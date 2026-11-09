using Magazing.Application.DTOs.Roles;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;

namespace Magazing.Application.UseCases.Roles;

internal static class RoleReadModel
{
    public static async Task<List<RoleDto>> GetAsync(IUnitOfWork unitOfWork, List<Guid>? roleIds, CancellationToken cancellationToken)
    {
        var roles = roleIds is null
            ? (await unitOfWork.Repository<Role>().GetAllAsync(cancellationToken)).ToList()
            : (await unitOfWork.Repository<Role>().FindAsync(item => roleIds.Contains(item.RoleId), cancellationToken)).ToList();

        var ids = roles.Select(item => item.RoleId).ToList();
        var rolePermissions = (await unitOfWork.Repository<RolePermission>().FindAsync(item => ids.Contains(item.RoleId), cancellationToken)).ToList();
        var permissionIds = rolePermissions.Select(item => item.PermissionId).Distinct().ToList();

        var permissionCodes = (await unitOfWork.Repository<Permission>().FindAsync(item => permissionIds.Contains(item.PermissionId), cancellationToken))
            .ToDictionary(item => item.PermissionId, item => item.Code);

        return roles
            .OrderBy(item => item.Name)
            .Select(role => new RoleDto
            {
                RoleId = role.RoleId,
                Name = role.Name,
                Description = role.Description,
                Permissions = rolePermissions
                    .Where(item => item.RoleId == role.RoleId && permissionCodes.ContainsKey(item.PermissionId))
                    .Select(item => permissionCodes[item.PermissionId])
                    .OrderBy(code => code)
                    .ToList()
            })
            .ToList();
    }

    public static async Task<RoleDto> GetByIdAsync(IUnitOfWork unitOfWork, Guid roleId, CancellationToken cancellationToken)
    {
        var roles = await GetAsync(unitOfWork, [roleId], cancellationToken);
        return roles.FirstOrDefault() ?? throw new KeyNotFoundException("El rol no existe.");
    }

    public static async Task EnsurePermissionsExistAsync(IUnitOfWork unitOfWork, List<Guid> permissionIds, CancellationToken cancellationToken)
    {
        if (permissionIds.Count == 0)
        {
            return;
        }

        var found = (await unitOfWork.Repository<Permission>().FindAsync(item => permissionIds.Contains(item.PermissionId), cancellationToken)).Count();
        if (found != permissionIds.Count)
        {
            throw new InvalidOperationException("Uno o mas permisos indicados no existen.");
        }
    }
}
