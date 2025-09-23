using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NhaTro.Application.Interfaces;
using NhaTro.Application.DTOs;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace NhaTro.Web.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class UtilityReadingController : Controller
    {
        private readonly IUtilityReadingService _utilityReadingService;
        private readonly IRoomService _roomService;
        private readonly IMotelService _motelService;
        private readonly IContractService _contractService;

        public UtilityReadingController(
            IUtilityReadingService utilityReadingService,
            IRoomService roomService,
            IMotelService motelService,
            IContractService contractService)
        {
            _utilityReadingService = utilityReadingService;
            _roomService = roomService;
            _motelService = motelService;
            _contractService = contractService;
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

        // Helper: Gán tên phòng và nhà trọ vào DTO
        private async Task PopulateReadingDisplayData(UtilityReadingDto reading)
        {
            var room = await _roomService.GetByIdAsync(reading.RoomId);
            if (room != null)
            {
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel != null)
                {
                    reading.MotelName = motel.MotelName;
                }
                reading.RoomName = room.RoomName;
            }
        }

        // Helper: Lấy danh sách phòng thuộc về chủ nhà cho dropdown
        private async Task PopulateViewDataForForm(int ownerId, int? selectedRoomId)
        {
            var allRooms = await _roomService.GetAllRoomsAsync();
            var allMotels = await _motelService.GetAllMotelsAsync();

            var ownerRooms = allRooms
                .Where(r => allMotels.Any(m => m.MotelId == r.MotelId && m.OwnerId == ownerId))
                .Select(r => new
                {
                    r.RoomId,
                    RoomDisplayName = $"{allMotels.FirstOrDefault(m => m.MotelId == r.MotelId)?.MotelName} - {r.RoomName}"
                })
                .ToList();

            ViewBag.Rooms = new SelectList(ownerRooms, "RoomId", "RoomDisplayName", selectedRoomId);
        }

        public async Task<IActionResult> Index(int? roomId, int? motelId)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var readings = await _utilityReadingService.GetAllUtilityReadingsAsync();
                var ownerReadings = new List<UtilityReadingDto>();

                foreach (var reading in readings)
                {
                    var room = await _roomService.GetByIdAsync(reading.RoomId);
                    if (room != null)
                    {
                        var motel = await _motelService.GetByIdAsync(room.MotelId);
                        if (motel != null && motel.OwnerId == ownerId)
                        {
                            await PopulateReadingDisplayData(reading);
                            ownerReadings.Add(reading);
                        }
                    }
                }

                if (roomId.HasValue && roomId.Value > 0)
                {
                    ownerReadings = ownerReadings.Where(r => r.RoomId == roomId.Value).ToList();
                    var room = await _roomService.GetByIdAsync(roomId.Value);
                    ViewBag.RoomName = room?.RoomName;
                    ViewBag.RoomId = roomId.Value;
                    ViewBag.MotelId = room?.MotelId;
                    var motel = room != null ? await _motelService.GetByIdAsync(room.MotelId) : null;
                    ViewBag.MotelName = motel?.MotelName;
                }
                else if (motelId.HasValue && motelId.Value > 0)
                {
                    var roomsInMotel = (await _roomService.GetAllRoomsAsync()).Where(r => r.MotelId == motelId.Value).Select(r => r.RoomId).ToList();
                    ownerReadings = ownerReadings.Where(r => roomsInMotel.Contains(r.RoomId)).ToList();
                    var motel = await _motelService.GetByIdAsync(motelId.Value);
                    ViewBag.MotelName = motel?.MotelName;
                    ViewBag.MotelId = motelId.Value;
                }

                return View(ownerReadings);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách chỉ số điện nước: " + ex.Message;
                return View(new List<UtilityReadingDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var reading = await _utilityReadingService.GetByIdAsync(id);
                if (reading == null)
                {
                    return NotFound();
                }

                var room = await _roomService.GetByIdAsync(reading.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền truy cập chỉ số điện nước này.";
                    return NotFound();
                }

                await PopulateReadingDisplayData(reading);

                return View(reading);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết chỉ số điện nước: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create(int? roomId, int? motelId)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                await PopulateViewDataForForm(ownerId, roomId);

                var model = new CreateUtilityReadingDto { RoomId = roomId ?? 0, ReadingDate = DateOnly.FromDateTime(DateTime.Today) };

                if (roomId.HasValue && roomId.Value > 0)
                {
                    var room = await _roomService.GetByIdAsync(roomId.Value);
                    if (room != null)
                    {
                        var motel = await _motelService.GetByIdAsync(room.MotelId);
                        ViewBag.MotelName = motel?.MotelName;
                        ViewBag.MotelId = motel?.MotelId;
                    }
                }

                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi chuẩn bị trang tạo chỉ số: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUtilityReadingDto readingDto)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var room = await _roomService.GetByIdAsync(readingDto.RoomId);
                if (room == null)
                {
                    TempData["Error"] = "Phòng không tồn tại.";
                    return RedirectToAction(nameof(Index));
                }
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền tạo chỉ số điện nước cho phòng này.";
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    await PopulateViewDataForForm(ownerId, readingDto.RoomId);
                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    return View(readingDto);
                }

                await _utilityReadingService.AddUtilityReadingAsync(readingDto);
                TempData["Success"] = "Tạo chỉ số điện nước mới thành công!";
                return RedirectToAction(nameof(Index), new { roomId = readingDto.RoomId, motelId = motel.MotelId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateViewDataForForm(GetCurrentOwnerId(), readingDto.RoomId);
                TempData["Error"] = "Lỗi dữ liệu: " + ex.Message;
                return View(readingDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi tạo chỉ số điện nước: " + ex.Message);
                await PopulateViewDataForForm(GetCurrentOwnerId(), readingDto.RoomId);
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                return View(readingDto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var reading = await _utilityReadingService.GetByIdAsync(id);
                if (reading == null)
                {
                    return NotFound();
                }

                var room = await _roomService.GetByIdAsync(reading.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa chỉ số điện nước này.";
                    return NotFound();
                }

                await PopulateViewDataForForm(ownerId, reading.RoomId);
                await PopulateReadingDisplayData(reading);

                return View(reading);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải chỉ số điện nước để chỉnh sửa: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UtilityReadingDto readingDto)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var room = await _roomService.GetByIdAsync(readingDto.RoomId);
                if (room == null)
                {
                    TempData["Error"] = "Phòng không tồn tại.";
                    return RedirectToAction(nameof(Index));
                }
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa chỉ số điện nước cho phòng này.";
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    await PopulateViewDataForForm(ownerId, readingDto.RoomId);
                    await PopulateReadingDisplayData(readingDto);
                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    return View(readingDto);
                }

                await _utilityReadingService.UpdateUtilityReadingAsync(readingDto);
                TempData["Success"] = "Cập nhật chỉ số điện nước thành công!";
                return RedirectToAction(nameof(Index), new { roomId = readingDto.RoomId, motelId = motel.MotelId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateViewDataForForm(GetCurrentOwnerId(), readingDto.RoomId);
                await PopulateReadingDisplayData(readingDto);
                TempData["Error"] = "Lỗi dữ liệu: " + ex.Message;
                return View(readingDto);
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateViewDataForForm(GetCurrentOwnerId(), readingDto.RoomId);
                await PopulateReadingDisplayData(readingDto);
                TempData["Error"] = "Lỗi: " + ex.Message;
                return View(readingDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi cập nhật chỉ số điện nước: " + ex.Message);
                await PopulateViewDataForForm(GetCurrentOwnerId(), readingDto.RoomId);
                await PopulateReadingDisplayData(readingDto);
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                return View(readingDto);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int? currentRoomId = null;
            int? currentMotelId = null;
            try
            {
                var ownerId = GetCurrentOwnerId();
                var reading = await _utilityReadingService.GetByIdAsync(id);
                if (reading == null)
                {
                    return NotFound();
                }
                currentRoomId = reading.RoomId;

                var room = await _roomService.GetByIdAsync(reading.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền xóa chỉ số điện nước này.";
                    return NotFound();
                }
                currentMotelId = motel.MotelId;

                await _utilityReadingService.DeleteUtilityReadingAsync(id);
                TempData["Success"] = "Xóa chỉ số điện nước thành công!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn khi xóa chỉ số điện nước: " + ex.Message;
            }

            return RedirectToAction(nameof(Index), new { roomId = currentRoomId, motelId = currentMotelId });
        }
    }
}