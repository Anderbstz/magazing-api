namespace Magazing.Application.DTOs.Settings;

public class CompanyDto
{
    public Guid CompanyId { get; set; }

    public string Ruc { get; set; } = null!;

    public string BusinessName { get; set; } = null!;

    public string? TradeName { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? LogoUrl { get; set; }
}

public record CompanyRequest(
    string Ruc,
    string BusinessName,
    string? TradeName,
    string? Address,
    string? Phone,
    string? Email,
    string? LogoUrl);

public class BranchDto
{
    public Guid BranchId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public int WarehouseCount { get; set; }

    public DateTime CreatedAt { get; set; }
}

public record BranchRequest(string Code, string Name, string? Address, string? Phone, bool IsActive);

public class DocumentSeriesDto
{
    public Guid DocumentSeriesId { get; set; }

    public Guid BranchId { get; set; }

    public string BranchName { get; set; } = null!;

    public string DocumentType { get; set; } = null!;

    public string Series { get; set; } = null!;

    public int CurrentNumber { get; set; }

    public bool IsActive { get; set; }
}

public record DocumentSeriesRequest(Guid BranchId, string DocumentType, string Series);

public record SetActiveRequest(bool IsActive);

public record UpdatePaymentMethodRequest(string Name, bool IsActive);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
