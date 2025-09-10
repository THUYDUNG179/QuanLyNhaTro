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
    public class InvoiceDetailService : IInvoiceDetailService
    {
        private readonly IRepository<InvoiceDetail> _invoiceDetailRepository;

        public InvoiceDetailService(IRepository<InvoiceDetail> invoiceDetailRepository)
        {
            _invoiceDetailRepository = invoiceDetailRepository;
        }

        public async Task<IEnumerable<InvoiceDetailDto>> GetAllInvoiceDetailsAsync()
        {
            var invoiceDetails = await _invoiceDetailRepository.GetAllAsync();
            return invoiceDetails.Select(id => new InvoiceDetailDto
            {
                InvoiceDetailId = id.InvoiceDetailId,
                InvoiceId = id.InvoiceId,
                Description = id.Description,
                Amount = id.Amount,
                UtilityReadingId = id.UtilityReadingId
            }).ToList();
        }

        public async Task<InvoiceDetailDto> GetByIdAsync(int invoiceDetailId)
        {
            var invoiceDetail = await _invoiceDetailRepository.GetByIdAsync(invoiceDetailId);
            if (invoiceDetail == null)
                throw new KeyNotFoundException($"Invoice detail with ID {invoiceDetailId} not found.");

            return new InvoiceDetailDto
            {
                InvoiceDetailId = invoiceDetail.InvoiceDetailId,
                InvoiceId = invoiceDetail.InvoiceId,
                Description = invoiceDetail.Description,
                Amount = invoiceDetail.Amount,
                UtilityReadingId = invoiceDetail.UtilityReadingId
            };
        }

        public async Task AddInvoiceDetailAsync(CreateInvoiceDetailDto invoiceDetailDto)
        {
            var invoiceDetail = new InvoiceDetail
            {
                InvoiceId = invoiceDetailDto.InvoiceId,
                Description = invoiceDetailDto.Description,
                Amount = invoiceDetailDto.Amount,
                UtilityReadingId = invoiceDetailDto.UtilityReadingId
            };

            await _invoiceDetailRepository.AddAsync(invoiceDetail);
            await _invoiceDetailRepository.SaveChangesAsync();
        }

        public async Task UpdateInvoiceDetailAsync(InvoiceDetailDto invoiceDetailDto)
        {
            var invoiceDetail = await _invoiceDetailRepository.GetByIdAsync(invoiceDetailDto.InvoiceDetailId);
            if (invoiceDetail == null)
                throw new KeyNotFoundException($"Invoice detail with ID {invoiceDetailDto.InvoiceDetailId} not found.");

            invoiceDetail.InvoiceId = invoiceDetailDto.InvoiceId;
            invoiceDetail.Description = invoiceDetailDto.Description;
            invoiceDetail.Amount = invoiceDetailDto.Amount;
            invoiceDetail.UtilityReadingId = invoiceDetailDto.UtilityReadingId;

            await _invoiceDetailRepository.UpdateAsync(invoiceDetail);
            await _invoiceDetailRepository.SaveChangesAsync();
        }

        public async Task DeleteInvoiceDetailAsync(int invoiceDetailId)
        {
            var invoiceDetail = await _invoiceDetailRepository.GetByIdAsync(invoiceDetailId);
            if (invoiceDetail == null)
                throw new KeyNotFoundException($"Invoice detail with ID {invoiceDetailId} not found.");

            await _invoiceDetailRepository.DeleteAsync(invoiceDetail);
            await _invoiceDetailRepository.SaveChangesAsync();
        }
    }
}