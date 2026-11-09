namespace Magazing.Application.DTOs.Users;

public record UpdateUserRequest(string Email, string FullName, Guid? BranchId, bool IsActive);
