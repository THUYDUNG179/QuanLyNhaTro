using NhaTro.Application.DTOs;
using NhaTro.Application.Interfaces;
using NhaTro.Domain.Interfaces;
using NhaTro.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NhaTro.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<Payment> _paymentRepository;

        public PaymentService(IRepository<Payment> paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await _paymentRepository.GetAllAsync();
            return payments.Select(p => new PaymentDto
            {
                PaymentId = p.PaymentId,
                InvoiceId = p.InvoiceId,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Method = p.Method,
                Status = p.Status
            }).ToList();
        }

        public async Task<PaymentDto> GetByIdAsync(int paymentId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {paymentId} not found.");

            return new PaymentDto
            {
                PaymentId = payment.PaymentId,
                InvoiceId = payment.InvoiceId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                Method = payment.Method,
                Status = payment.Status
            };
        }

        public async Task AddPaymentAsync(CreatePaymentDto paymentDto)
        {
            var payment = new Payment
            {
                InvoiceId = paymentDto.InvoiceId,
                Amount = paymentDto.Amount,
                PaymentDate = DateTime.UtcNow,
                Method = paymentDto.Method,
                Status = "Success"
            };

            await _paymentRepository.AddAsync(payment);
            await _paymentRepository.SaveChangesAsync();
        }

        public async Task UpdatePaymentAsync(PaymentDto paymentDto)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentDto.PaymentId);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {paymentDto.PaymentId} not found.");

            payment.InvoiceId = paymentDto.InvoiceId;
            payment.Amount = paymentDto.Amount;
            payment.Method = paymentDto.Method;
            payment.Status = paymentDto.Status;

            await _paymentRepository.UpdateAsync(payment);
            await _paymentRepository.SaveChangesAsync();
        }

        public async Task DeletePaymentAsync(int paymentId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {paymentId} not found.");

            await _paymentRepository.DeleteAsync(payment);
            await _paymentRepository.SaveChangesAsync();
        }
    }
}