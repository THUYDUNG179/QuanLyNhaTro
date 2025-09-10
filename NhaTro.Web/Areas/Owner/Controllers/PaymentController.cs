using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaTro.Application.Interfaces;
using NhaTro.Application.DTOs;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc.Rendering; // Thêm using này cho SelectList

namespace NhaTro.Web.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceService _invoiceService;
        private readonly IContractService _contractService;
        private readonly IRoomService _roomService;
        private readonly IMotelService _motelService;
        private readonly IUserService _userService; // THÊM IUserService

        public PaymentController(IPaymentService paymentService, IInvoiceService invoiceService, IContractService contractService, IRoomService roomService, IMotelService motelService, IUserService userService)
        {
            _paymentService = paymentService;
            _invoiceService = invoiceService;
            _contractService = contractService;
            _roomService = roomService;
            _motelService = motelService;
            _userService = userService; // KHỞI TẠO IUserService
        }

        // Helper: Lấy ID của Owner hiện tại
        protected int GetCurrentOwnerId()
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null || !int.TryParse(ownerIdClaim.Value, out int ownerId))
            {
                throw new InvalidOperationException("Không thể xác định ID chủ nhà của người dùng hiện tại. Vui lòng đăng nhập lại.");
            }
            return ownerId;
        }

        private async Task PopulatePaymentDisplayData(PaymentDto payment)
        {
            var invoice = await _invoiceService.GetByIdAsync(payment.InvoiceId);
            if (invoice != null)
            {
                payment.InvoiceBillingPeriod = invoice.BillingPeriod; // Điền BillingPeriod của Invoice
                var contract = await _contractService.GetByIdAsync(invoice.ContractId);
                if (contract != null)
                {
                    payment.ContractCode = contract.ContractId.ToString(); // Hoặc trường code của Contract
                    var room = await _roomService.GetByIdAsync(contract.RoomId);
                    if (room != null)
                    {
                        var motel = await _motelService.GetByIdAsync(room.MotelId);
                        if (motel != null)
                        {
                            payment.MotelName = motel.MotelName;
                        }
                        payment.RoomName = room.RoomName;
                    }
                    var tenant = await _userService.GetByIdAsync(contract.TenantId);
                    if (tenant != null)
                    {
                        payment.TenantName = tenant.FullName;
                    }
                }
            }
        }

        // Action Index đã được cập nhật để lọc theo invoiceId hoặc motelId
        public async Task<IActionResult> Index(int? invoiceId, int? motelId)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var payments = await _paymentService.GetAllPaymentsAsync();
                var ownerPayments = new List<PaymentDto>();

                foreach (var payment in payments)
                {
                    var invoice = await _invoiceService.GetByIdAsync(payment.InvoiceId);
                    if (invoice == null) continue;
                    var contract = await _contractService.GetByIdAsync(invoice.ContractId);
                    if (contract == null) continue;
                    var room = await _roomService.GetByIdAsync(contract.RoomId);
                    if (room == null) continue;
                    var motel = await _motelService.GetByIdAsync(room.MotelId);
                    if (motel != null && motel.OwnerId == ownerId)
                    {
                        await PopulatePaymentDisplayData(payment); // Điền dữ liệu hiển thị
                        ownerPayments.Add(payment);
                    }
                }

                if (invoiceId.HasValue && invoiceId.Value > 0)
                {
                    ownerPayments = ownerPayments.Where(p => p.InvoiceId == invoiceId.Value).ToList();
                    var invoice = await _invoiceService.GetByIdAsync(invoiceId.Value);
                    ViewBag.InvoiceId = invoiceId.Value;
                    ViewBag.InvoiceBillingPeriod = invoice?.BillingPeriod; // DTO mới dùng BillingPeriod
                    if (invoice != null)
                    {
                        var contract = await _contractService.GetByIdAsync(invoice.ContractId);
                        var room = await _roomService.GetByIdAsync(contract.RoomId);
                        ViewBag.MotelId = room?.MotelId;
                        var motel = room != null ? await _motelService.GetByIdAsync(room.MotelId) : null;
                        ViewBag.MotelName = motel?.MotelName;
                    }
                }
                else if (motelId.HasValue && motelId.Value > 0)
                {
                    var motel = await _motelService.GetByIdAsync(motelId.Value);
                    ViewBag.MotelName = motel?.MotelName;
                    ViewBag.MotelId = motelId.Value;
                }

                return View(ownerPayments);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách thanh toán: " + ex.Message;
                return View(new List<PaymentDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var payment = await _paymentService.GetByIdAsync(id);
                if (payment == null)
                {
                    return NotFound();
                }

                await PopulatePaymentDisplayData(payment); // Điền dữ liệu hiển thị

                var invoice = await _invoiceService.GetByIdAsync(payment.InvoiceId);
                var contract = await _contractService.GetByIdAsync(invoice.ContractId);
                var room = await _roomService.GetByIdAsync(contract.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền truy cập thanh toán này.";
                    return NotFound();
                }

                ViewBag.InvoiceId = invoice.InvoiceId;
                ViewBag.InvoiceBillingPeriod = invoice.BillingPeriod;
                ViewBag.ContractId = contract.ContractId;
                ViewBag.RoomId = room.RoomId;
                ViewBag.RoomName = room.RoomName;
                ViewBag.MotelId = motel.MotelId;
                ViewBag.MotelName = motel.MotelName;

                return View(payment);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return NotFound();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết thanh toán: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create(int? invoiceId, int? motelId)
        {
            int ownerId = 0;
            try
            {
                ownerId = GetCurrentOwnerId();
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var allInvoices = await _invoiceService.GetAllInvoicesAsync();
            var allContracts = await _contractService.GetAllContractsAsync();
            var allRooms = await _roomService.GetAllRoomsAsync();
            var allMotels = await _motelService.GetAllMotelsAsync();

            var ownerInvoices = allInvoices
                .Where(inv => allContracts.Any(c => c.ContractId == inv.ContractId &&
                               allRooms.Any(r => r.RoomId == c.RoomId &&
                                   allMotels.Any(m => m.MotelId == r.MotelId && m.OwnerId == ownerId))))
                .Select(inv => new { inv.InvoiceId, InvoiceDisplayName = inv.BillingPeriod })
                .ToList();

            if (invoiceId.HasValue && invoiceId.Value > 0 && !ownerInvoices.Any(inv => inv.InvoiceId == invoiceId.Value))
            {
                TempData["Error"] = "ID hóa đơn không hợp lệ hoặc không thuộc về bạn.";
                return RedirectToAction(nameof(Index), new { motelId = motelId });
            }
            if (motelId.HasValue && motelId.Value > 0)
            {
                var motel = await _motelService.GetByIdAsync(motelId.Value);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "ID nhà trọ không hợp lệ hoặc không thuộc về bạn.";
                    return RedirectToAction(nameof(Index));
                }
                if (!invoiceId.HasValue || invoiceId.Value == 0)
                {
                    var roomsInMotel = allRooms.Where(r => r.MotelId == motel.MotelId).Select(r => r.RoomId).ToList();
                    var contractsInMotel = allContracts.Where(c => roomsInMotel.Contains(c.RoomId)).Select(c => c.ContractId).ToList();
                    invoiceId = allInvoices.FirstOrDefault(inv => contractsInMotel.Contains(inv.ContractId))?.InvoiceId;
                }
            }


            ViewBag.Invoices = new SelectList(ownerInvoices, "InvoiceId", "InvoiceDisplayName", invoiceId);
            ViewBag.MotelId = motelId;
            ViewBag.InvoiceId = invoiceId;
            // CreatePaymentDto không có Status và Notes, sẽ được gán giá trị mặc định ở service layer
            return View(new CreatePaymentDto { InvoiceId = invoiceId ?? 0, PaymentDate = DateOnly.FromDateTime(DateTime.Today) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePaymentDto paymentDto)
        {
            int? currentMotelId = null;
            int? currentInvoiceId = null;
            try
            {
                var ownerId = GetCurrentOwnerId();
                var invoice = await _invoiceService.GetByIdAsync(paymentDto.InvoiceId);
                if (invoice == null)
                {
                    return NotFound();
                }
                var contract = await _contractService.GetByIdAsync(invoice.ContractId);
                var room = await _roomService.GetByIdAsync(contract.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền tạo thanh toán cho hóa đơn này.";
                    return NotFound();
                }
                currentMotelId = motel.MotelId;
                currentInvoiceId = invoice.InvoiceId;

                if (!ModelState.IsValid)
                {
                    var allInvoices = await _invoiceService.GetAllInvoicesAsync();
                    var allContracts = await _contractService.GetAllContractsAsync();
                    var allRooms = await _roomService.GetAllRoomsAsync();
                    var allMotels = await _motelService.GetAllMotelsAsync();

                    var ownerInvoices = allInvoices
                        .Where(inv => allContracts.Any(c => c.ContractId == inv.ContractId &&
                                       allRooms.Any(r => r.RoomId == c.RoomId &&
                                           allMotels.Any(m => m.MotelId == r.MotelId && m.OwnerId == ownerId))))
                        .Select(inv => new { inv.InvoiceId, InvoiceDisplayName = inv.BillingPeriod })
                        .ToList();
                    ViewBag.Invoices = new SelectList(ownerInvoices, "InvoiceId", "InvoiceDisplayName", paymentDto.InvoiceId);
                    ViewBag.MotelId = currentMotelId;
                    ViewBag.InvoiceId = currentInvoiceId;

                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    return View(paymentDto);
                }

                await _paymentService.AddPaymentAsync(paymentDto);
                TempData["Success"] = "Tạo thanh toán mới thành công!";
                return RedirectToAction(nameof(Index), new { invoiceId = paymentDto.InvoiceId, motelId = currentMotelId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Lỗi dữ liệu: " + ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Lỗi dữ liệu: " + ex.Message;
                return View(paymentDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi tạo thanh toán: " + ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                return View(paymentDto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var payment = await _paymentService.GetByIdAsync(id);
                if (payment == null)
                {
                    return NotFound();
                }

                await PopulatePaymentDisplayData(payment); // Điền dữ liệu hiển thị

                var invoice = await _invoiceService.GetByIdAsync(payment.InvoiceId);
                var contract = await _contractService.GetByIdAsync(invoice.ContractId);
                var room = await _roomService.GetByIdAsync(contract.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa thanh toán này.";
                    return NotFound();
                }

                var allInvoices = await _invoiceService.GetAllInvoicesAsync();
                var allContracts = await _contractService.GetAllContractsAsync();
                var allRooms = await _roomService.GetAllRoomsAsync();
                var allMotels = await _motelService.GetAllMotelsAsync();

                var ownerInvoices = allInvoices
                    .Where(inv => allContracts.Any(c => c.ContractId == inv.ContractId &&
                                   allRooms.Any(r => r.RoomId == c.RoomId &&
                                       allMotels.Any(m => m.MotelId == r.MotelId && m.OwnerId == ownerId))))
                    .Select(inv => new { inv.InvoiceId, InvoiceDisplayName = inv.BillingPeriod })
                    .ToList();
                ViewBag.Invoices = new SelectList(ownerInvoices, "InvoiceId", "InvoiceDisplayName", payment.InvoiceId);

                ViewBag.InvoiceId = invoice.InvoiceId;
                ViewBag.InvoiceBillingPeriod = invoice.BillingPeriod;
                ViewBag.ContractId = contract.ContractId;
                ViewBag.RoomId = room.RoomId;
                ViewBag.RoomName = room.RoomName;
                ViewBag.MotelId = motel.MotelId;
                ViewBag.MotelName = motel.MotelName;

                return View(payment);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return NotFound();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải thanh toán để chỉnh sửa: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PaymentDto paymentDto)
        {
            int? currentMotelId = null;
            int? currentInvoiceId = null;
            try
            {
                var ownerId = GetCurrentOwnerId();
                var invoice = await _invoiceService.GetByIdAsync(paymentDto.InvoiceId);
                if (invoice == null)
                {
                    return NotFound();
                }
                var contract = await _contractService.GetByIdAsync(invoice.ContractId);
                var room = await _roomService.GetByIdAsync(contract.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa thanh toán cho hóa đơn này.";
                    return NotFound();
                }
                currentMotelId = motel.MotelId;
                currentInvoiceId = invoice.InvoiceId;

                if (!ModelState.IsValid)
                {
                    var allInvoices = await _invoiceService.GetAllInvoicesAsync();
                    var allContracts = await _contractService.GetAllContractsAsync();
                    var allRooms = await _roomService.GetAllRoomsAsync();
                    var allMotels = await _motelService.GetAllMotelsAsync();

                    var ownerInvoices = allInvoices
                        .Where(inv => allContracts.Any(c => c.ContractId == inv.ContractId &&
                                       allRooms.Any(r => r.RoomId == c.RoomId &&
                                           allMotels.Any(m => m.MotelId == r.MotelId && m.OwnerId == ownerId))))
                        .Select(inv => new { inv.InvoiceId, InvoiceDisplayName = inv.BillingPeriod })
                        .ToList();
                    ViewBag.Invoices = new SelectList(ownerInvoices, "InvoiceId", "InvoiceDisplayName", paymentDto.InvoiceId);
                    ViewBag.MotelId = currentMotelId;
                    ViewBag.InvoiceId = currentInvoiceId;

                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    return View(paymentDto);
                }

                var existingPayment = await _paymentService.GetByIdAsync(paymentDto.PaymentId);
                if (existingPayment != null)
                {
                    paymentDto.CreatedAt = existingPayment.CreatedAt; 
                }

                await _paymentService.UpdatePaymentAsync(paymentDto);
                TempData["Success"] = "Cập nhật thanh toán thành công!";
                return RedirectToAction(nameof(Index), new { invoiceId = paymentDto.InvoiceId, motelId = currentMotelId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Lỗi dữ liệu: " + ex.Message;
                return View(paymentDto);
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Lỗi: " + ex.Message;
                return View(paymentDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi cập nhật thanh toán: " + ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                return View(paymentDto);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int? currentInvoiceId = null;
            int? currentMotelId = null;
            try
            {
                var ownerId = GetCurrentOwnerId();
                var payment = await _paymentService.GetByIdAsync(id);
                if (payment == null)
                {
                    return NotFound();
                }
                currentInvoiceId = payment.InvoiceId;

                var invoice = await _invoiceService.GetByIdAsync(payment.InvoiceId);
                var contract = await _contractService.GetByIdAsync(invoice.ContractId);
                var room = await _roomService.GetByIdAsync(contract.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền xóa thanh toán này.";
                    return NotFound();
                }
                currentMotelId = motel.MotelId;

                await _paymentService.DeletePaymentAsync(id);
                TempData["Success"] = "Xóa thanh toán thành công!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
                return NotFound();
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return NotFound();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn khi xóa thanh toán: " + ex.Message;
            }

            return RedirectToAction(nameof(Index), new { invoiceId = currentInvoiceId, motelId = currentMotelId });
        }
    }
}
