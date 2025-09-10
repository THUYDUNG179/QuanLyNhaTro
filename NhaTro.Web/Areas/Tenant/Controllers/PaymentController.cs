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
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceService _invoiceService;
        private readonly IContractService _contractService;

        public PaymentController(IPaymentService paymentService, IInvoiceService invoiceService, IContractService contractService)
        {
            _paymentService = paymentService;
            _invoiceService = invoiceService;
            _contractService = contractService;
        }

        // GET: Tenant/Payment
        public async Task<IActionResult> Index(int? invoiceId)
        {
            var tenantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var payments = await _paymentService.GetAllPaymentsAsync();
            var tenantPayments = new List<PaymentDto>();

            foreach (var payment in payments)
            {
                var invoice = await _invoiceService.GetByIdAsync(payment.InvoiceId);
                var contract = await _contractService.GetByIdAsync(invoice.ContractId);
                if (contract != null && contract.TenantId == tenantId)
                {
                    tenantPayments.Add(payment);
                }
            }

            if (invoiceId.HasValue)
            {
                tenantPayments = tenantPayments.Where(p => p.InvoiceId == invoiceId.Value).ToList();
            }

            ViewBag.InvoiceId = invoiceId;
            return View(tenantPayments);
        }

        // GET: Tenant/Payment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var tenantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            var invoice = await _invoiceService.GetByIdAsync(payment.InvoiceId);
            var contract = await _contractService.GetByIdAsync(invoice.ContractId);
            if (contract == null || contract.TenantId != tenantId)
            {
                return NotFound();
            }
            return View(payment);
        }
    }
}