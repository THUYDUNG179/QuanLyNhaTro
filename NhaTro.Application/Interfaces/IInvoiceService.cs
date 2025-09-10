using NhaTro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NhaTro.Application.Interfaces
{
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync();
        Task<InvoiceDto> GetByIdAsync(int invoiceId);
        Task AddInvoiceAsync(CreateInvoiceDto invoiceDto);
        Task UpdateInvoiceAsync(InvoiceDto invoiceDto);
        Task DeleteInvoiceAsync(int invoiceId);
    }
}