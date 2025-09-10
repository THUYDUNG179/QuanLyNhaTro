using NhaTro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NhaTro.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync();
        Task<PaymentDto> GetByIdAsync(int paymentId);
        Task AddPaymentAsync(CreatePaymentDto paymentDto);
        Task UpdatePaymentAsync(PaymentDto paymentDto);
        Task DeletePaymentAsync(int paymentId);
    }
}