using Magazing.Application.DTOs.Settings;
using Magazing.Application.UseCases.Company;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Branches.Queries;

public record GetBranchesQuery : IRequest<List<BranchDto>>;

internal sealed class GetBranchesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBranchesQuery, List<BranchDto>>
{
    public async Task<List<BranchDto>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var branches = (await unitOfWork.Repository<Branch>().GetAllAsync(cancellationToken)).OrderBy(item => item.Code).ToList();
        return await BranchReadModel.ToDtosAsync(unitOfWork, branches, cancellationToken);
    }
}
