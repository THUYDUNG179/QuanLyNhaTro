using NhaTro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NhaTro.Application.Interfaces
{
    public interface IInvoiceDetailService
    {
        Task<IEnumerable<InvoiceDetailDto>> GetAllInvoiceDetailsAsync();
        Task<InvoiceDetailDto> GetByIdAsync(int invoiceDetailId);
        Task AddInvoiceDetailAsync(CreateInvoiceDetailDto invoiceDetailDto);
        Task UpdateInvoiceDetailAsync(InvoiceDetailDto invoiceDetailDto);
        Task DeleteInvoiceDetailAsync(int invoiceDetailId);
    }
}