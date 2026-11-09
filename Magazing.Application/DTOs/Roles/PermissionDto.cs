namespace Magazing.Application.DTOs.Roles;

public class PermissionDto
{
    public Guid PermissionId { get; set; }

    public string Code { get; set; } = null!;

    public string Module { get; set; } = null!;

    public string? Description { get; set; }
}
