using Magazing.Application.DTOs.Roles;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Roles.Queries;

public record GetPermissionsQuery : IRequest<List<PermissionDto>>;

internal sealed class GetPermissionsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPermissionsQuery, List<PermissionDto>>
{
    public async Task<List<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await unitOfWork.Repository<Permission>().GetAllAsync(cancellationToken);

        return permissions
            .OrderBy(item => item.Module)
            .ThenBy(item => item.Code)
            .Select(item => new PermissionDto
            {
                PermissionId = item.PermissionId,
                Code = item.Code,
                Module = item.Module,
                Description = item.Description
            })
            .ToList();
    }
}
