using Magazing.Application.DTOs.Settings;
using Magazing.Application.UseCases.Company;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Branches;

internal static class BranchReadModel
{
    private const int MaxCodeLength = 10;

    public static (string Code, string Name) Validate(string? code, string? name)
    {
        var normalizedCode = code?.Trim().ToUpper() ?? string.Empty;
        var normalizedName = name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedCode) || normalizedCode.Length > MaxCodeLength)
        {
            throw new InvalidOperationException($"El codigo es obligatorio y debe tener como maximo {MaxCodeLength} caracteres.");
        }

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new InvalidOperationException("El nombre de la sucursal es obligatorio.");
        }

        return (normalizedCode, normalizedName);
    }

    public static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static async Task<List<BranchDto>> ToDtosAsync(IUnitOfWork unitOfWork, IReadOnlyList<Branch> branches, CancellationToken cancellationToken)
    {
        var branchIds = branches.Select(item => item.BranchId).ToList();
        var warehouseCounts = (await unitOfWork.Repository<Warehouse>().FindAsync(item => branchIds.Contains(item.BranchId), cancellationToken))
            .GroupBy(item => item.BranchId)
            .ToDictionary(group => group.Key, group => group.Count());

        return branches.Select(branch => new BranchDto
        {
            BranchId = branch.BranchId,
            Code = branch.Code,
            Name = branch.Name,
            Address = branch.Address,
            Phone = branch.Phone,
            IsActive = branch.IsActive,
            WarehouseCount = warehouseCounts.GetValueOrDefault(branch.BranchId),
            CreatedAt = branch.CreatedAt
        }).ToList();
    }
}
