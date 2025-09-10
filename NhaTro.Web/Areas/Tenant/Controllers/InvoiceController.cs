using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaTro.Application.Interfaces;
using NhaTro.Application.DTOs;
using System.Threading.Tasks;
using System.Security.Claims;

namespace NhaTro.Web.Areas.Tenant.Controllers
{
    [Area("Tenant")]
    [Authorize(Roles = "Tenant")]
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IContractService _contractService;

        public InvoiceController(IInvoiceService invoiceService, IContractService contractService)
        {
            _invoiceService = invoiceService;
            _contractService = contractService;
        }

        // GET: Tenant/Invoice
        public async Task<IActionResult> Index()
        {
            var tenantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            var tenantInvoices = new List<InvoiceDto>();

            foreach (var invoice in invoices)
            {
                var contract = await _contractService.GetByIdAsync(invoice.ContractId);
                if (contract != null && contract.TenantId == tenantId)
                {
                    tenantInvoices.Add(invoice);
                }
            }

            return View(tenantInvoices);
        }

        // GET: Tenant/Invoice/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var tenantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var invoice = await _invoiceService.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }
            var contract = await _contractService.GetByIdAsync(invoice.ContractId);
            if (contract == null || contract.TenantId != tenantId)
            {
                return NotFound();
            }
            return View(invoice);
        }
    }
}