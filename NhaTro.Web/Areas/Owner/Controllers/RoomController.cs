using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaTro.Application.Interfaces;
using NhaTro.Application.DTOs;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NhaTro.Web.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class RoomController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IMotelService _motelService;
        private readonly IContractService _contractService; // Thêm IContractService

        // Constructor để tiêm các dịch vụ cần thiết
        public RoomController(IRoomService roomService, IMotelService motelService, IContractService contractService)
        {
            _roomService = roomService;
            _motelService = motelService;
            _contractService = contractService;
        }

        private int GetCurrentOwnerId()
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null || !int.TryParse(ownerIdClaim.Value, out int ownerId))
            {
                throw new InvalidOperationException("Không thể xác định ID chủ nhà của người dùng hiện tại. Vui lòng đăng nhập lại.");
            }
            return ownerId;
        }

        private async Task LoadRoomStatuses()
        {
            var statuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Trống" },
                new SelectListItem { Value = "2", Text = "Đã thuê" },
                new SelectListItem { Value = "3", Text = "Bảo trì" }
            };
            ViewBag.RoomStatuses = new SelectList(statuses, "Value", "Text");
        }


        // GET: Owner/Room/Index?motelId=X
        // Hiển thị danh sách phòng thuộc một nhà trọ cụ thể của chủ nhà hiện tại
        public async Task<IActionResult> Index(int? motelId)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();

                var motel = await _motelService.GetByIdAsync(motelId.Value);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Không tìm thấy nhà trọ hoặc bạn không có quyền truy cập.";
                    return RedirectToAction("Index", "Motel", new { area = "Owner" });
                }

                // Cập nhật: Gọi phương thức mới để lấy danh sách phòng đã có thông tin hợp đồng
                var motelRooms = await _roomService.GetRoomsByMotelIdAsync(motelId.Value);

                ViewBag.MotelId = motelId.Value;
                ViewBag.MotelName = motel.MotelName;
                ViewBag.MotelAddress = motel.Address;

                return View(motelRooms);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách phòng: " + ex.Message;
                return RedirectToAction("Index", "Motel", new { area = "Owner" });
            }
        }

        // GET: Owner/Room/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            try
            {
                var ownerId = GetCurrentOwnerId();
                var room = await _roomService.GetByIdAsync(id.Value);

                if (room == null)
                {
                    return NotFound();
                }

                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Không tìm thấy nhà trọ liên quan hoặc bạn không có quyền truy cập phòng này.";
                    return NotFound();
                }

                ViewBag.MotelId = room.MotelId;
                ViewBag.MotelName = motel.MotelName;
                return View(room);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết phòng: " + ex.Message;
                return RedirectToAction(nameof(Index), new { motelId = ViewBag.MotelId });
            }
        }

        // Các action khác như Create, Edit, Delete... giữ nguyên
        // ... (phần code còn lại của Controller không thay đổi)
        // Lưu ý: Các phương thức Edit, Create, Delete không cần thay đổi vì chúng chỉ làm việc với một phòng cụ thể.
        // Logic kiểm tra hợp đồng chỉ cần thiết cho việc hiển thị danh sách.
        public async Task<IActionResult> Create(int? motelId)
        {
            if (motelId == null || motelId <= 0)
            {
                TempData["Error"] = "ID nhà trọ không hợp lệ để tạo phòng.";
                return RedirectToAction("Index", "Motel", new { area = "Owner" });
            }

            try
            {
                var ownerId = GetCurrentOwnerId();
                var motel = await _motelService.GetByIdAsync(motelId.Value);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Không tìm thấy nhà trọ hoặc bạn không có quyền tạo phòng tại đây.";
                    return RedirectToAction("Index", "Motel", new { area = "Owner" });
                }

                ViewBag.MotelId = motelId.Value;
                ViewBag.MotelName = motel.MotelName;
                await LoadRoomStatuses();

                return View(new CreateRoomDto { MotelId = motelId.Value });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi chuẩn bị tạo phòng: " + ex.Message;
                return RedirectToAction("Index", "Motel", new { area = "Owner" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoomDto roomDto)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var motel = await _motelService.GetByIdAsync(roomDto.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Không tìm thấy nhà trọ hoặc bạn không có quyền tạo phòng tại đây.";
                    return RedirectToAction("Index", "Motel", new { area = "Owner" });
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.MotelId = roomDto.MotelId;
                    ViewBag.MotelName = motel.MotelName;
                    await LoadRoomStatuses();
                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    return View(roomDto);
                }

                await _roomService.AddRoomAsync(roomDto);
                TempData["Success"] = "Tạo phòng mới thành công!";

                return RedirectToAction(nameof(Index), new { motelId = roomDto.MotelId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi: " + ex.Message;
                ViewBag.MotelId = roomDto.MotelId;
                ViewBag.MotelName = (await _motelService.GetByIdAsync(roomDto.MotelId))?.MotelName ?? "Nhà trọ không xác định";
                await LoadRoomStatuses();
                return View(roomDto);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Lỗi dữ liệu: " + ex.Message;
                ViewBag.MotelId = roomDto.MotelId;
                ViewBag.MotelName = (await _motelService.GetByIdAsync(roomDto.MotelId))?.MotelName ?? "Nhà trọ không xác định";
                await LoadRoomStatuses();
                return View(roomDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi tạo phòng: " + ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                ViewBag.MotelId = roomDto.MotelId;
                ViewBag.MotelName = (await _motelService.GetByIdAsync(roomDto.MotelId))?.MotelName ?? "Nhà trọ không xác định";
                await LoadRoomStatuses();
                return View(roomDto);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            try
            {
                var ownerId = GetCurrentOwnerId();
                var room = await _roomService.GetByIdAsync(id.Value);

                if (room == null)
                {
                    return NotFound();
                }

                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Không tìm thấy nhà trọ liên quan hoặc bạn không có quyền chỉnh sửa phòng này.";
                    return NotFound();
                }

                ViewBag.MotelId = room.MotelId;
                ViewBag.MotelName = motel.MotelName;
                await LoadRoomStatuses();
                return View(room);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải thông tin phòng để chỉnh sửa: " + ex.Message;
                return RedirectToAction(nameof(Index), new { motelId = ViewBag.MotelId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoomDto roomDto)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var motel = await _motelService.GetByIdAsync(roomDto.MotelId);

                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Không tìm thấy nhà trọ hoặc bạn không có quyền chỉnh sửa phòng này.";
                    return RedirectToAction("Index", "Motel", new { area = "Owner" });
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.MotelId = roomDto.MotelId;
                    ViewBag.MotelName = motel.MotelName;
                    await LoadRoomStatuses();
                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    return View(roomDto);
                }

                if (roomDto.RoomId <= 0)
                {
                    return NotFound();
                }

                await _roomService.UpdateRoomAsync(roomDto);
                TempData["Success"] = "Cập nhật phòng thành công!";
                return RedirectToAction(nameof(Index), new { motelId = roomDto.MotelId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi: " + ex.Message;
                ViewBag.MotelId = roomDto.MotelId;
                ViewBag.MotelName = (await _motelService.GetByIdAsync(roomDto.MotelId))?.MotelName ?? "Nhà trọ không xác định";
                await LoadRoomStatuses();
                return View(roomDto);
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Lỗi: " + ex.Message;
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Lỗi dữ liệu: " + ex.Message;
                ViewBag.MotelId = roomDto.MotelId;
                ViewBag.MotelName = (await _motelService.GetByIdAsync(roomDto.MotelId))?.MotelName ?? "Nhà trọ không xác định";
                await LoadRoomStatuses();
                return View(roomDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi cập nhật phòng: " + ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                ViewBag.MotelId = roomDto.MotelId;
                ViewBag.MotelName = (await _motelService.GetByIdAsync(roomDto.MotelId))?.MotelName ?? "Nhà trọ không xác định";
                await LoadRoomStatuses();
                return View(roomDto);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            int? currentMotelId = null;

            try
            {
                var ownerId = GetCurrentOwnerId();
                var room = await _roomService.GetByIdAsync(id.Value);

                if (room == null)
                {
                    return NotFound();
                }
                currentMotelId = room.MotelId;

                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Không tìm thấy nhà trọ liên quan hoặc bạn không có quyền xóa phòng này.";
                    return NotFound();
                }

                await _roomService.DeleteRoomAsync(id.Value);
                TempData["Success"] = "Xóa phòng thành công!";

                return RedirectToAction(nameof(Index), new { motelId = currentMotelId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Lỗi: " + ex.Message;
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Lỗi: " + ex.Message;
                return NotFound();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn khi xóa phòng: " + ex.Message;
                return RedirectToAction(nameof(Index), new { motelId = currentMotelId });
            }
        }
    }
}