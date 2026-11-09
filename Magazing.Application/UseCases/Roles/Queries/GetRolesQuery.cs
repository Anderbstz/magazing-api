using Magazing.Application.DTOs.Roles;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Roles.Queries;

public record GetRolesQuery : IRequest<List<RoleDto>>;

internal sealed class GetRolesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    public Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        return RoleReadModel.GetAsync(unitOfWork, null, cancellationToken);
    }
}
