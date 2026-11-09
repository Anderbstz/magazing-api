using System.Linq.Expressions;
using Magazing.Application.DTOs.Users;
using Magazing.Domain.Common;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Users.Queries;

public record GetUsersQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<PagedResult<UserDto>>;

internal sealed class GetUsersQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<User, bool>>? predicate = null;

        var search = request.Search?.Trim().ToLower();
        if (!string.IsNullOrEmpty(search))
        {
            predicate = item => item.Username.ToLower().Contains(search)
                || item.FullName.ToLower().Contains(search)
                || item.Email.ToLower().Contains(search);
        }

        var page = await unitOfWork.Repository<User>().GetPagedAsync(
            request.Page,
            request.PageSize,
            predicate,
            item => item.Username,
            cancellationToken: cancellationToken);

        var roles = await UserReadModel.LoadRoleNamesAsync(unitOfWork, page.Items.Select(item => item.UserId).ToList(), cancellationToken);

        return page.Map(item => UserReadModel.ToDto(item, roles[item.UserId]));
    }
}
