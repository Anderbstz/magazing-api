using Magazing.Application.Common;
using Magazing.Application.Notifications;
using Magazing.Application.Security;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;

namespace Magazing.Application.UseCases.Users;

internal static class UserAccess
{
    public static async Task<(string Password, DateTime ExpiresAt)> IssueTemporaryPasswordAsync(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        User user,
        bool isNewAccount,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var password = TemporaryPassword.Generate();
        var expiresAt = now.AddHours(TemporaryPassword.ValidHours);

        user.PasswordHash = passwordHasher.Hash(password);
        user.MustChangePassword = true;
        user.TempPasswordExpiresAt = expiresAt;

        var content = EmailTemplates.AccountAccess(user.FullName, user.Username, password, BusinessTime.ToLima(expiresAt), isNewAccount);
        await EmailQueue.EnqueueAsync(unitOfWork, user.Email, content, cancellationToken);

        return (password, expiresAt);
    }
}
