using Magazing.Application.DTOs.Users;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Users.Queries;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto>;

internal sealed class GetUserByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return UserReadModel.GetAsync(unitOfWork, request.UserId, cancellationToken);
    }
}
