using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;

namespace Magazing.Application.UseCases.Users;

internal static class AdminGuard
{
    public const string AdminRole = "ADMIN";

    public static async Task EnsureAdminIsExclusiveAsync(IUnitOfWork unitOfWork, IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken)
    {
        if (roleIds.Count < 2)
        {
            return;
        }

        if (await AdminRoleIdAsync(unitOfWork, cancellationToken) is { } adminRoleId && roleIds.Contains(adminRoleId))
        {
            throw new InvalidOperationException("ADMIN ya incluye todos los permisos: no lo combines con otros roles.");
        }
    }

    public static async Task<bool> IsAdminAsync(IUnitOfWork unitOfWork, Guid userId, CancellationToken cancellationToken)
    {
        return await AdminRoleIdAsync(unitOfWork, cancellationToken) is { } adminRoleId
            && await unitOfWork.Repository<UserRole>().AnyAsync(item => item.UserId == userId && item.RoleId == adminRoleId, cancellationToken);
    }

    public static async Task<Guid?> AdminRoleIdAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        return (await unitOfWork.Repository<Role>().FirstOrDefaultAsync(item => item.Name == AdminRole, cancellationToken))?.RoleId;
    }

    public static async Task EnsureAnotherActiveAdminAsync(IUnitOfWork unitOfWork, Guid userId, CancellationToken cancellationToken)
    {
        if (await AdminRoleIdAsync(unitOfWork, cancellationToken) is not { } adminRoleId)
        {
            return;
        }

        var otherAdminIds = (await unitOfWork.Repository<UserRole>()
                .FindAsync(item => item.RoleId == adminRoleId && item.UserId != userId, cancellationToken))
            .Select(item => item.UserId)
            .ToList();

        if (!await unitOfWork.Repository<User>().AnyAsync(item => otherAdminIds.Contains(item.UserId) && item.IsActive, cancellationToken))
        {
            throw new InvalidOperationException("Debe quedar al menos un administrador activo.");
        }
    }
}
