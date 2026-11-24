using Magazing.Application.DTOs.Users;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Users.Queries;

public record GetActiveSessionsQuery(Guid? UserId = null) : IRequest<List<SessionDto>>;

internal sealed class GetActiveSessionsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetActiveSessionsQuery, List<SessionDto>>
{
    public async Task<List<SessionDto>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var userId = request.UserId;

        var tokens = (await unitOfWork.Repository<RefreshToken>().FindAsync(
                item => item.RevokedAt == null && item.ExpiresAt > now && (userId == null || item.UserId == userId),
                cancellationToken))
            .ToList();

        var userIds = tokens.Select(item => item.UserId).Distinct().ToList();
        var users = (await unitOfWork.Repository<User>().FindAsync(item => userIds.Contains(item.UserId) && item.IsActive, cancellationToken))
            .ToDictionary(item => item.UserId);

        return tokens
            .Where(item => users.ContainsKey(item.UserId))
            .Select(item => new SessionDto
            {
                SessionId = item.RefreshTokenId,
                UserId = item.UserId,
                Username = users[item.UserId].Username,
                FullName = users[item.UserId].FullName,
                IpAddress = item.CreatedByIp,
                UserAgent = item.UserAgent,
                StartedAt = item.SessionStartedAt ?? item.CreatedAt,
                LastActivityAt = item.CreatedAt,
                ExpiresAt = item.ExpiresAt
            })
            .OrderByDescending(item => item.LastActivityAt)
            .ToList();
    }
}
