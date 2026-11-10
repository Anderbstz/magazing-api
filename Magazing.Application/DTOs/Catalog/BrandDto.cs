namespace Magazing.Application.DTOs.Catalog;

public class BrandDto
{
    public Guid BrandId { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
