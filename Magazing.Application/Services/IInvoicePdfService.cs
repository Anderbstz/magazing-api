using Magazing.Application.DTOs.Sales;
using Magazing.Application.DTOs.Settings;

namespace Magazing.Application.Services;

public interface IInvoicePdfService
{
    byte[] BuildInvoice(CompanyDto company, SaleDetailDto sale);
}
