namespace Magazing.Application.DTOs.Purchases;

public class SupplierDto
{
    public Guid SupplierId { get; set; }

    public string Ruc { get; set; } = null!;

    public string BusinessName { get; set; } = null!;

    public string? ContactName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
