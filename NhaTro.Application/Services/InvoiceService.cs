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
    public class InvoiceService : IInvoiceService
    {
        private readonly IRepository<Invoice> _invoiceRepository;

        public InvoiceService(IRepository<Invoice> invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync()
        {
            var invoices = await _invoiceRepository.GetAllAsync();
            return invoices.Select(i => new InvoiceDto
            {
                InvoiceId = i.InvoiceId,
                ContractId = i.ContractId,
                BillingPeriod = i.BillingPeriod,
                DueDate = i.DueDate,
                TotalAmount = i.TotalAmount,
                Status = i.Status,
                Notes = i.Notes,
                CreatedAt = i.CreatedAt
            }).ToList();
        }

        public async Task<InvoiceDto> GetByIdAsync(int invoiceId)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new KeyNotFoundException($"Invoice with ID {invoiceId} not found.");

            return new InvoiceDto
            {
                InvoiceId = invoice.InvoiceId,
                ContractId = invoice.ContractId,
                BillingPeriod = invoice.BillingPeriod,
                DueDate = invoice.DueDate,
                TotalAmount = invoice.TotalAmount,
                Status = invoice.Status,
                Notes = invoice.Notes,
                CreatedAt = invoice.CreatedAt
            };
        }

        public async Task AddInvoiceAsync(CreateInvoiceDto invoiceDto)
        {
            var existingInvoices = await _invoiceRepository.GetAllAsync();
            if (existingInvoices.Any(i => i.ContractId == invoiceDto.ContractId && i.BillingPeriod == invoiceDto.BillingPeriod))
                throw new InvalidOperationException("Invoice for this contract and billing period already exists.");

            var invoice = new Invoice
            {
                ContractId = invoiceDto.ContractId,
                BillingPeriod = invoiceDto.BillingPeriod,
                DueDate = invoiceDto.DueDate,
                TotalAmount = invoiceDto.TotalAmount,
                Status = "Unpaid",
                Notes = invoiceDto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            await _invoiceRepository.AddAsync(invoice);
            await _invoiceRepository.SaveChangesAsync();
        }

        public async Task UpdateInvoiceAsync(InvoiceDto invoiceDto)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceDto.InvoiceId);
            if (invoice == null)
                throw new KeyNotFoundException($"Invoice with ID {invoiceDto.InvoiceId} not found.");

            invoice.ContractId = invoiceDto.ContractId;
            invoice.BillingPeriod = invoiceDto.BillingPeriod;
            invoice.DueDate = invoiceDto.DueDate;
            invoice.TotalAmount = invoiceDto.TotalAmount;
            invoice.Status = invoiceDto.Status;
            invoice.Notes = invoiceDto.Notes;

            await _invoiceRepository.UpdateAsync(invoice);
            await _invoiceRepository.SaveChangesAsync();
        }

        public async Task DeleteInvoiceAsync(int invoiceId)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new KeyNotFoundException($"Invoice with ID {invoiceId} not found.");

            await _invoiceRepository.DeleteAsync(invoice);
            await _invoiceRepository.SaveChangesAsync();
        }
    }
}