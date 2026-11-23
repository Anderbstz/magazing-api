namespace Magazing.Application.DTOs.Users;

public class UserAccessDto
{
    public UserDto User { get; set; } = null!;

    public string TemporaryPassword { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }
}
