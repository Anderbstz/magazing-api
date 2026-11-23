using Magazing.Application.DTOs.Settings;
using Magazing.Application.UseCases.Company;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Branches.Commands;

public record CreateBranchCommand(string Code, string Name, string? Address, string? Phone, bool IsActive) : IRequest<BranchDto>;

internal sealed class CreateBranchCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBranchCommand, BranchDto>
{
    public async Task<BranchDto> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var (code, name) = BranchReadModel.Validate(request.Code, request.Name);
        var branches = unitOfWork.Repository<Branch>();

        if (await branches.AnyAsync(item => item.Code == code, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe una sucursal con ese código.");
        }

        var company = await CompanyReadModel.GetAsync(unitOfWork, cancellationToken);
        var branch = new Branch
        {
            BranchId = Guid.NewGuid(),
            CompanyId = company.CompanyId,
            Code = code,
            Name = name,
            Address = BranchReadModel.Clean(request.Address),
            Phone = BranchReadModel.Clean(request.Phone),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await branches.AddAsync(branch, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return (await BranchReadModel.ToDtosAsync(unitOfWork, [branch], cancellationToken))[0];
    }
}
