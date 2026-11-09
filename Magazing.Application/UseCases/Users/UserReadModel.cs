using Magazing.Application.DTOs.Users;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;

namespace Magazing.Application.UseCases.Users;

internal static class UserReadModel
{
    public static async Task<UserDto> GetAsync(IUnitOfWork unitOfWork, Guid userId, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Repository<User>().FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("El usuario no existe.");

        var roles = await LoadRoleNamesAsync(unitOfWork, [user.UserId], cancellationToken);

        return ToDto(user, roles[user.UserId]);
    }

    public static async Task<Dictionary<Guid, List<string>>> LoadRoleNamesAsync(IUnitOfWork unitOfWork, List<Guid> userIds, CancellationToken cancellationToken)
    {
        var userRoles = (await unitOfWork.Repository<UserRole>().FindAsync(item => userIds.Contains(item.UserId), cancellationToken)).ToList();
        var roleIds = userRoles.Select(item => item.RoleId).Distinct().ToList();

        var roleNames = (await unitOfWork.Repository<Role>().FindAsync(item => roleIds.Contains(item.RoleId), cancellationToken))
            .ToDictionary(item => item.RoleId, item => item.Name);

        return userIds.Distinct().ToDictionary(
            userId => userId,
            userId => userRoles
                .Where(item => item.UserId == userId && roleNames.ContainsKey(item.RoleId))
                .Select(item => roleNames[item.RoleId])
                .OrderBy(name => name)
                .ToList());
    }

    public static async Task EnsureRolesExistAsync(IUnitOfWork unitOfWork, List<Guid> roleIds, CancellationToken cancellationToken)
    {
        if (roleIds.Count == 0)
        {
            return;
        }

        var found = (await unitOfWork.Repository<Role>().FindAsync(item => roleIds.Contains(item.RoleId), cancellationToken)).Count();
        if (found != roleIds.Count)
        {
            throw new InvalidOperationException("Uno o mas roles indicados no existen.");
        }
    }

    public static UserDto ToDto(User user, List<string> roles)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            BranchId = user.BranchId,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            Roles = roles
        };
    }
}
