namespace Magazing.Application.DTOs.Catalog;

public class CategoryDto
{
    public Guid CategoryId { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public string? ParentCategoryName { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
