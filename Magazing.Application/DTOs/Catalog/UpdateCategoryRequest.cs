namespace Magazing.Application.DTOs.Catalog;

public record UpdateCategoryRequest(string Name, string? Description, Guid? ParentCategoryId, bool IsActive);
