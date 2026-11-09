using Magazing.Application.DTOs.Users;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Users.Commands;

public record AssignUserRolesCommand(Guid UserId, List<Guid> RoleIds) : IRequest<UserDto>;

internal sealed class AssignUserRolesCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<AssignUserRolesCommand, UserDto>
{
    public async Task<UserDto> Handle(AssignUserRolesCommand request, CancellationToken cancellationToken)
    {
        var roleIds = request.RoleIds?.Distinct().ToList() ?? [];

        if (!await unitOfWork.Repository<User>().AnyAsync(item => item.UserId == request.UserId, cancellationToken))
        {
            throw new KeyNotFoundException("El usuario no existe.");
        }

        await UserReadModel.EnsureRolesExistAsync(unitOfWork, roleIds, cancellationToken);

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
