using Magazing.Application.DTOs.Users;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Users.Commands;

public record AssignUserRolesCommand(Guid UserId, List<Guid> RoleIds, Guid ActorId) : IRequest<UserDto>;

internal sealed class AssignUserRolesCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<AssignUserRolesCommand, UserDto>
{
    public async Task<UserDto> Handle(AssignUserRolesCommand request, CancellationToken cancellationToken)
    {
        var roleIds = request.RoleIds?.Distinct().ToList() ?? [];

        var user = await unitOfWork.Repository<User>().GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("El usuario no existe.");

        await UserReadModel.EnsureRolesExistAsync(unitOfWork, roleIds, cancellationToken);
        await AdminGuard.EnsureAdminIsExclusiveAsync(unitOfWork, roleIds, cancellationToken);

        var adminRoleId = await AdminGuard.AdminRoleIdAsync(unitOfWork, cancellationToken);
        var losesAdmin = adminRoleId is { } adminId
            && !roleIds.Contains(adminId)
            && await AdminGuard.IsAdminAsync(unitOfWork, user.UserId, cancellationToken);

        if (losesAdmin)
        {
            if (request.UserId == request.ActorId)
            {
                throw new InvalidOperationException("No puedes quitarte a ti mismo el rol ADMIN.");
            }

            if (user.IsActive)
            {
                await AdminGuard.EnsureAnotherActiveAdminAsync(unitOfWork, user.UserId, cancellationToken);
            }
        }

        var userRoles = unitOfWork.Repository<UserRole>();
        var currentRoles = (await userRoles.FindAsync(item => item.UserId == request.UserId, cancellationToken)).ToList();

        foreach (var userRole in currentRoles.Where(item => !roleIds.Contains(item.RoleId)))
        {
            userRoles.Delete(userRole);
        }

        var currentRoleIds = currentRoles.Select(item => item.RoleId).ToHashSet();
        var now = DateTime.UtcNow;

        foreach (var roleId in roleIds.Where(id => !currentRoleIds.Contains(id)))
        {
            await userRoles.AddAsync(new UserRole
            {
                UserId = request.UserId,
                RoleId = roleId,
                AssignedAt = now
            }, cancellationToken);
        }

        await unitOfWork.CompleteAsync(cancellationToken);

        return await UserReadModel.GetAsync(unitOfWork, request.UserId, cancellationToken);
    }
}
