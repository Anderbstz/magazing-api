using Magazing.Application.DTOs.Settings;
using Magazing.Application.UseCases.Company;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Branches.Commands;

public record UpdateBranchCommand(Guid BranchId, string Code, string Name, string? Address, string? Phone, bool IsActive) : IRequest<BranchDto>;

internal sealed class UpdateBranchCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBranchCommand, BranchDto>
{
    public async Task<BranchDto> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var (code, name) = BranchReadModel.Validate(request.Code, request.Name);
        var branches = unitOfWork.Repository<Branch>();

        var branch = await branches.GetByIdAsync(request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException("La sucursal no existe.");

        if (await branches.AnyAsync(item => item.Code == code && item.BranchId != request.BranchId, cancellationToken))
        {
            throw new InvalidOperationException("Ya existe una sucursal con ese codigo.");
        }

        if (branch.IsActive && !request.IsActive
            && await unitOfWork.Repository<Warehouse>().AnyAsync(item => item.BranchId == request.BranchId && item.IsActive, cancellationToken))
        {
            throw new InvalidOperationException("No se puede desactivar una sucursal con almacenes activos.");
        }

        branch.Code = code;
        branch.Name = name;
        branch.Address = BranchReadModel.Clean(request.Address);
        branch.Phone = BranchReadModel.Clean(request.Phone);
        branch.IsActive = request.IsActive;
        branch.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(cancellationToken);

        return (await BranchReadModel.ToDtosAsync(unitOfWork, [branch], cancellationToken))[0];
    }
}
