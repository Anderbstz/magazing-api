using System.Linq.Expressions;
using Magazing.Application.DTOs.Notifications;
using Magazing.Domain.Services.Common;
using Magazing.Domain.Entities;
using Magazing.Domain.Services.Notifications;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Emails.Queries;

public record GetEmailsQuery(string? Status = null, string? Search = null, int Page = 1, int PageSize = 20, string? Template = null) : IRequest<PagedResult<EmailDto>>;

internal sealed class GetEmailsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetEmailsQuery, PagedResult<EmailDto>>
{
    public async Task<PagedResult<EmailDto>> Handle(GetEmailsQuery request, CancellationToken cancellationToken)
    {
        var status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim().ToUpper();
        if (status is not null && !EmailStatuses.IsValid(status))
        {
            throw new InvalidOperationException("El estado debe ser PENDING, SENT o FAILED.");
        }

        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToLower();
        var template = string.IsNullOrWhiteSpace(request.Template) ? null : request.Template.Trim().ToUpper();

        Expression<Func<EmailOutbox, bool>> predicate = item =>
            (status == null || item.Status == status)
            && (template == null || item.Template == template)
            && (search == null || item.ToEmail.Contains(search) || item.Subject.ToLower().Contains(search));

        var page = await unitOfWork.Repository<EmailOutbox>().GetPagedAsync(
            request.Page,
            request.PageSize,
            predicate,
            item => item.CreatedAt,
            descending: true,
            cancellationToken: cancellationToken);

        return page.Map(EmailReadModel.ToDto);
    }
}
