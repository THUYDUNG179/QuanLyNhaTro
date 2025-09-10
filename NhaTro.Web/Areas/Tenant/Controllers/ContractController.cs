using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaTro.Application.Interfaces;
using NhaTro.Application.DTOs;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

namespace NhaTro.Web.Areas.Tenant.Controllers
{
    [Area("Tenant")]
    [Authorize(Roles = "Tenant")]
    public class ContractController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IRoomService _roomService;
        private readonly IUserService _userService;
        private readonly IMotelService _motelService; // Thêm MotelService

        public ContractController(IContractService contractService,
                                  IRoomService roomService,
                                  IUserService userService,
                                  IMotelService motelService)
        {
            _contractService = contractService;
            _roomService = roomService;
            _userService = userService;
            _motelService = motelService;
        }

        private int GetCurrentTenantId()
        {
            var tenantIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (tenantIdClaim == null || !int.TryParse(tenantIdClaim.Value, out int tenantId))
            {
                throw new InvalidOperationException("Không thể xác định ID người thuê hiện tại.");
            }
            return tenantId;
        }

        // GET: Tenant/Contract/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var tenantId = GetCurrentTenantId();
                var allContracts = await _contractService.GetAllContractsAsync();

                var tenantContracts = allContracts
                    .Where(c => c.TenantId == tenantId)
                    .ToList();

                // Lấy thông tin liên quan và gán vào DTO
                foreach (var contract in tenantContracts)
                {
                    var room = await _roomService.GetByIdAsync(contract.RoomId);
                    if (room != null)
                    {
                        contract.RoomName = $"{room.RoomName} ({ (await _motelService.GetByIdAsync(room.MotelId))?.MotelName })";
                    }

                    var tenant = await _userService.GetByIdAsync(contract.TenantId);
                    if (tenant != null)
                    {
                        contract.TenantName = tenant.FullName;
                    }
                }

                return View(tenantContracts);
            }
            catch (InvalidOperationException)
            {
                TempData["Error"] = "Không thể xác định ID người thuê. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn khi tải danh sách hợp đồng: " + ex.Message;
                return View(new List<ContractDto>());
            }
        }

        // GET: Tenant/Contract/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var tenantId = GetCurrentTenantId();
                var contract = await _contractService.GetByIdAsync(id);

                if (contract == null)
                {
                    TempData["Error"] = "Không tìm thấy hợp đồng.";
                    return NotFound();
                }

                // KIỂM TRA BẢO MẬT: Đảm bảo hợp đồng thuộc về người dùng hiện tại
                if (contract.TenantId != tenantId)
                {
                    TempData["Error"] = "Bạn không có quyền xem hợp đồng này.";
                    return RedirectToAction("Index");
                }

                // Lấy thông tin liên quan và gán vào DTO
                var room = await _roomService.GetByIdAsync(contract.RoomId);
                if (room != null)
                {
                    var motel = await _motelService.GetByIdAsync(room.MotelId);
                    contract.RoomName = $"{room.RoomName} ({motel?.MotelName})";
                }

                var tenant = await _userService.GetByIdAsync(contract.TenantId);
                if (tenant != null)
                {
                    contract.TenantName = tenant.FullName;
                }

                return View(contract);
            }
            catch (InvalidOperationException)
            {
                TempData["Error"] = "Không thể xác định ID người thuê. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn khi tải chi tiết hợp đồng: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}