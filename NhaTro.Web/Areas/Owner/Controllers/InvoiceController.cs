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
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IContractService _contractService;
        private readonly IRoomService _roomService;
        private readonly IMotelService _motelService;
        private readonly IUtilityReadingService _utilityReadingService;
        private readonly IUserService _userService;

        public InvoiceController(IInvoiceService invoiceService, IContractService contractService,
                                 IRoomService roomService, IMotelService motelService,
                                 IUtilityReadingService utilityReadingService,
                                 IUserService userService)
        {
            _invoiceService = invoiceService;
            _contractService = contractService;
            _roomService = roomService;
            _motelService = motelService;
            _utilityReadingService = utilityReadingService;
            _userService = userService;
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

        private async Task PopulateInvoiceDisplayData(InvoiceDto invoice)
        {
            var contract = await _contractService.GetByIdAsync(invoice.ContractId);
            if (contract != null)
            {
                var room = await _roomService.GetByIdAsync(contract.RoomId);
                if (room != null)
                {
                    var motel = await _motelService.GetByIdAsync(room.MotelId);
                    if (motel != null)
                    {
                        invoice.MotelName = motel.MotelName;
                    }
                    invoice.RoomName = room.RoomName;
                }
                invoice.ContractCode = contract.ContractId.ToString(); // Hoặc một trường code khác của Contract
                var tenant = await _userService.GetByIdAsync(contract.TenantId);
                if (tenant != null)
                {
                    invoice.TenantName = tenant.FullName;
                }
            }
        }

        // Action Index đã được cập nhật để lọc theo contractId hoặc motelId
        public async Task<IActionResult> Index(int? contractId, int? motelId)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var invoices = await _invoiceService.GetAllInvoicesAsync();
                var ownerInvoices = new List<InvoiceDto>();

                // Lọc hóa đơn thuộc về chủ nhà hiện tại
                foreach (var invoice in invoices)
                {
                    var contract = await _contractService.GetByIdAsync(invoice.ContractId);
                    if (contract == null) continue;
                    var room = await _roomService.GetByIdAsync(contract.RoomId);
                    if (room == null) continue;
                    var motel = await _motelService.GetByIdAsync(room.MotelId);
                    if (motel != null && motel.OwnerId == ownerId)
                    {
                        await PopulateInvoiceDisplayData(invoice); // Điền dữ liệu hiển thị
                        ownerInvoices.Add(invoice);
                    }
                }

                if (contractId.HasValue && contractId.Value > 0)
                {
                    ownerInvoices = ownerInvoices.Where(inv => inv.ContractId == contractId.Value).ToList();
                    var contract = await _contractService.GetByIdAsync(contractId.Value);
                    if (contract != null)
                    {
                        var room = await _roomService.GetByIdAsync(contract.RoomId);
                        ViewBag.RoomId = room?.RoomId;
                        ViewBag.RoomName = room?.RoomName;
                        ViewBag.MotelId = room?.MotelId;
                        var motel = room != null ? await _motelService.GetByIdAsync(room.MotelId) : null;
                        ViewBag.MotelName = motel?.MotelName;
                        ViewBag.ContractCode = contract.ContractId.ToString(); // Hoặc trường code của Contract
                    }
                }
                else if (motelId.HasValue && motelId.Value > 0)
                {
                    var motel = await _motelService.GetByIdAsync(motelId.Value);
                    ViewBag.MotelName = motel?.MotelName;
                    ViewBag.MotelId = motelId.Value;
                }

                return View(ownerInvoices);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách hóa đơn: " + ex.Message;
                return View(new List<InvoiceDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var invoice = await _invoiceService.GetByIdAsync(id);
                if (invoice == null)
                {
                    return NotFound();
                }

                await PopulateInvoiceDisplayData(invoice); // Điền dữ liệu hiển thị

                var contract = await _contractService.GetByIdAsync(invoice.ContractId);
                var room = await _roomService.GetByIdAsync(contract.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền truy cập hóa đơn này.";
                    return NotFound();
                }

                ViewBag.ContractId = contract.ContractId;
                ViewBag.RoomId = room.RoomId;
                ViewBag.RoomName = room.RoomName;
                ViewBag.MotelId = motel.MotelId;
                ViewBag.MotelName = motel.MotelName;

                return View(invoice);
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
                TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết hóa đơn: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create(int? contractId, int? motelId)
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

            var allContracts = await _contractService.GetAllContractsAsync();
            var allRooms = await _roomService.GetAllRoomsAsync();
            var allMotels = await _motelService.GetAllMotelsAsync();
            var allUsers = await _userService.GetAllUsersAsync();

            var ownerContracts = allContracts
                .Where(c => allRooms.Any(r => r.RoomId == c.RoomId &&
                               allMotels.Any(m => m.MotelId == r.MotelId && m.OwnerId == ownerId)))
                .Select(c => new
                {
                    c.ContractId,
                    ContractDisplayName = $"HĐ {c.ContractId} - Phòng {allRooms.FirstOrDefault(r => r.RoomId == c.RoomId)?.RoomName} - Người thuê {allUsers.FirstOrDefault(u => u.UserId == c.TenantId)?.FullName ?? "N/A"}"
                })
                .ToList();

            if (contractId.HasValue && contractId.Value > 0 && !ownerContracts.Any(c => c.ContractId == contractId.Value))
            {
                TempData["Error"] = "ID hợp đồng không hợp lệ hoặc không thuộc về bạn.";
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
                if (!contractId.HasValue || contractId.Value == 0)
                {
                    var roomsInMotel = allRooms.Where(r => r.MotelId == motel.MotelId).Select(r => r.RoomId).ToList();
                    contractId = allContracts.FirstOrDefault(c => roomsInMotel.Contains(c.RoomId))?.ContractId;
                }
            }


            ViewBag.Contracts = new SelectList(ownerContracts, "ContractId", "ContractDisplayName", contractId);
            ViewBag.MotelId = motelId;
            ViewBag.ContractId = contractId;
            // CreateInvoiceDto không có Status và CreatedAt, sẽ được gán giá trị mặc định ở service layer
            return View(new CreateInvoiceDto { ContractId = contractId ?? 0, DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInvoiceDto invoiceDto)
        {
            int? currentMotelId = null;
            int? currentContractId = null;
            try
            {
                var ownerId = GetCurrentOwnerId();
                var contract = await _contractService.GetByIdAsync(invoiceDto.ContractId);
                if (contract == null)
                {
                    return NotFound();
                }
                var room = await _roomService.GetByIdAsync(contract.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền tạo hóa đơn cho hợp đồng này.";
                    return NotFound();
                }
                currentMotelId = motel.MotelId;
                currentContractId = contract.ContractId;

                if (!ModelState.IsValid)
                {
                    var allContracts = await _contractService.GetAllContractsAsync();
                    var allRooms = await _roomService.GetAllRoomsAsync();
                    var allMotels = await _motelService.GetAllMotelsAsync();
                    var allUsers = await _userService.GetAllUsersAsync();

                    var ownerContracts = allContracts
                        .Where(c => allRooms.Any(r => r.RoomId == c.RoomId &&
                                       allMotels.Any(m => m.MotelId == r.MotelId && m.OwnerId == ownerId)))
                        .Select(c => new
                        {
                            c.ContractId,
                            ContractDisplayName = $"HĐ {c.ContractId} - Phòng {allRooms.FirstOrDefault(r => r.RoomId == c.RoomId)?.RoomName} - Người thuê {allUsers.FirstOrDefault(u => u.UserId == c.TenantId)?.FullName ?? "N/A"}"
                        })
                        .ToList();
                    ViewBag.Contracts = new SelectList(ownerContracts, "ContractId", "ContractDisplayName", invoiceDto.ContractId);
                    ViewBag.MotelId = currentMotelId;
                    ViewBag.ContractId = currentContractId;

                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    return View(invoiceDto);
                }

                await _invoiceService.AddInvoiceAsync(invoiceDto);
                TempData["Success"] = "Tạo hóa đơn mới thành công!";
                return RedirectToAction(nameof(Index), new { contractId = invoiceDto.ContractId, motelId = currentMotelId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = "Lỗi dữ liệu: " + ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Lỗi dữ liệu: " + ex.Message;
                return View(invoiceDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi tạo hóa đơn: " + ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                return View(invoiceDto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var invoice = await _invoiceService.GetByIdAsync(id);
                if (invoice == null)
                {
                    return NotFound();
                }

                await PopulateInvoiceDisplayData(invoice); // Điền dữ liệu hiển thị

                var contract = await _contractService.GetByIdAsync(invoice.ContractId);
                var room = await _roomService.GetByIdAsync(contract.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa hóa đơn này.";
                    return NotFound();
                }

                var allContracts = await _contractService.GetAllContractsAsync();
                var allRooms = await _roomService.GetAllRoomsAsync();
                var allMotels = await _motelService.GetAllMotelsAsync();
                var allUsers = await _userService.GetAllUsersAsync();

                var ownerContracts = allContracts
                    .Where(c => allRooms.Any(r => r.RoomId == c.RoomId &&
                                   allMotels.Any(m => m.MotelId == r.MotelId && m.OwnerId == ownerId)))
                    .Select(c => new
                    {
                        c.ContractId,
                        ContractDisplayName = $"HĐ {c.ContractId} - Phòng {allRooms.FirstOrDefault(r => r.RoomId == c.RoomId)?.RoomName} - Người thuê {allUsers.FirstOrDefault(u => u.UserId == c.TenantId)?.FullName ?? "N/A"}"
                    })
                    .ToList();
                ViewBag.Contracts = new SelectList(ownerContracts, "ContractId", "ContractDisplayName", invoice.ContractId);

                ViewBag.ContractId = contract.ContractId;
                ViewBag.RoomId = room.RoomId;
                ViewBag.RoomName = room.RoomName;
                ViewBag.MotelId = motel.MotelId;
                ViewBag.MotelName = motel.MotelName;

                return View(invoice);
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
                TempData["Error"] = "Đã xảy ra lỗi khi tải hóa đơn để chỉnh sửa: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InvoiceDto invoiceDto)
        {
            int? currentMotelId = null;
            int? currentContractId = null;
            try
            {
                var ownerId = GetCurrentOwnerId();
                var contract = await _contractService.GetByIdAsync(invoiceDto.ContractId);
                if (contract == null)
                {
                    return NotFound();
                }
                var room = await _roomService.GetByIdAsync(contract.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa hóa đơn cho hợp đồng này.";
                    return NotFound();
                }
                currentMotelId = motel.MotelId;
                currentContractId = contract.ContractId;

                if (!ModelState.IsValid)
                {
                    var allContracts = await _contractService.GetAllContractsAsync();
                    var allRooms = await _roomService.GetAllRoomsAsync();
                    var allMotels = await _motelService.GetAllMotelsAsync();
                    var allUsers = await _userService.GetAllUsersAsync();

                    var ownerContracts = allContracts
                        .Where(c => allRooms.Any(r => r.RoomId == c.RoomId &&
                                       allMotels.Any(m => m.MotelId == r.MotelId && m.OwnerId == ownerId)))
                        .Select(c => new
                        {
                            c.ContractId,
                            ContractDisplayName = $"HĐ {c.ContractId} - Phòng {allRooms.FirstOrDefault(r => r.RoomId == c.RoomId)?.RoomName} - Người thuê {allUsers.FirstOrDefault(u => u.UserId == c.TenantId)?.FullName ?? "N/A"}"
                        })
                        .ToList();
                    ViewBag.Contracts = new SelectList(ownerContracts, "ContractId", "ContractDisplayName", invoiceDto.ContractId);
                    ViewBag.MotelId = currentMotelId;
                    ViewBag.ContractId = currentContractId;

                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    return View(invoiceDto);
                }

                await _invoiceService.UpdateInvoiceAsync(invoiceDto);
                TempData["Success"] = "Cập nhật hóa đơn thành công!";
                return RedirectToAction(nameof(Index), new { contractId = invoiceDto.ContractId, motelId = currentMotelId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = "Lỗi dữ liệu: " + ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Lỗi dữ liệu: " + ex.Message;
                return View(invoiceDto);
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Lỗi: " + ex.Message;
                return View(invoiceDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi cập nhật hóa đơn: " + ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                return View(invoiceDto);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int? currentContractId = null;
            int? currentMotelId = null;
            try
            {
                var ownerId = GetCurrentOwnerId();
                var invoice = await _invoiceService.GetByIdAsync(id);
                if (invoice == null)
                {
                    return NotFound();
                }
                currentContractId = invoice.ContractId;

                var contract = await _contractService.GetByIdAsync(invoice.ContractId);
                var room = await _roomService.GetByIdAsync(contract.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền xóa hóa đơn này.";
                    return NotFound();
                }
                currentMotelId = motel.MotelId;

                await _invoiceService.DeleteInvoiceAsync(id);
                TempData["Success"] = "Xóa hóa đơn thành công!";
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
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn khi xóa hóa đơn: " + ex.Message;
            }

            return RedirectToAction(nameof(Index), new { contractId = currentContractId, motelId = currentMotelId });
        }
    }
}
