namespace Magazing.Application.DTOs.Catalog;

public class UnitDto
{
    public Guid UnitId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool AllowsDecimals { get; set; }
}
