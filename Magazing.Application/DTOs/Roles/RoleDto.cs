namespace Magazing.Application.DTOs.Roles;

public class RoleDto
{
    public Guid RoleId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public List<string> Permissions { get; set; } = [];
}
