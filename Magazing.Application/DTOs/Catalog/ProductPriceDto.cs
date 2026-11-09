namespace Magazing.Application.DTOs.Catalog;

public class ProductPriceDto
{
    public Guid ProductPriceId { get; set; }

    public decimal Price { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public bool IsCurrent { get; set; }

    public Guid? CreatedBy { get; set; }
}
