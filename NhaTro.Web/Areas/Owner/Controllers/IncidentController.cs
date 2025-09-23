using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaTro.Application.Interfaces;
using NhaTro.Application.DTOs;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NhaTro.Web.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class IncidentController : Controller
    {
        private readonly IIncidentService _incidentService;
        private readonly IRoomService _roomService;
        private readonly IMotelService _motelService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IncidentController(IIncidentService incidentService, IRoomService roomService, IMotelService motelService, IUserService userService)
        {
            _incidentService = incidentService;
            _roomService = roomService;
            _motelService = motelService;
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

        private async Task PopulateIncidentDisplayData(IncidentDto incident)
        {
            var room = await _roomService.GetByIdAsync(incident.RoomId);
            if (room != null)
            {
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel != null)
                {
                    incident.MotelName = motel.MotelName;
                }
                incident.RoomName = room.RoomName;
            }

            if (incident.TenantId.HasValue)
            {
                var tenant = await _userService.GetByIdAsync(incident.TenantId.Value);
                if (tenant != null)
                {
                    incident.TenantName = tenant.FullName;
                }
            }
            var owner = await _userService.GetByIdAsync(GetCurrentOwnerId());
            if (owner != null)
            {
                incident.OwnerName = owner.FullName;
            }
        }

        // Action Index đã được cập nhật để lọc theo roomId hoặc motelId
        public async Task<IActionResult> Index(int? roomId, int? motelId)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var incidents = await _incidentService.GetAllIncidentsAsync();
                var ownerIncidents = new List<IncidentDto>();

                foreach (var incident in incidents)
                {
                    var room = await _roomService.GetByIdAsync(incident.RoomId);
                    if (room != null)
                    {
                        var motel = await _motelService.GetByIdAsync(room.MotelId);
                        if (motel != null && motel.OwnerId == ownerId)
                        {
                            await PopulateIncidentDisplayData(incident); 
                            ownerIncidents.Add(incident);
                        }
                    }
                }

                if (roomId.HasValue && roomId.Value > 0)
                {
                    ownerIncidents = ownerIncidents.Where(i => i.RoomId == roomId.Value).ToList();
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
                    ownerIncidents = ownerIncidents.Where(i => roomsInMotel.Contains(i.RoomId)).ToList();
                    var motel = await _motelService.GetByIdAsync(motelId.Value);
                    ViewBag.MotelName = motel?.MotelName;
                    ViewBag.MotelId = motelId.Value;
                }

                return View(ownerIncidents);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách sự cố: " + ex.Message;
                return View(new List<IncidentDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var incident = await _incidentService.GetByIdAsync(id);
                if (incident == null)
                {
                    return NotFound();
                }

                await PopulateIncidentDisplayData(incident); 

                var room = await _roomService.GetByIdAsync(incident.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền truy cập sự cố này.";
                    return NotFound();
                }

                ViewBag.RoomId = room.RoomId;
                ViewBag.RoomName = room.RoomName;
                ViewBag.MotelId = motel.MotelId;
                ViewBag.MotelName = motel.MotelName;

                return View(incident);
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
                TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết sự cố: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create(int? roomId, int? motelId)
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

            var allRooms = await _roomService.GetAllRoomsAsync();
            var allMotels = await _motelService.GetAllMotelsAsync();

            var ownerRooms = allRooms
                .Where(r => allMotels.Any(m => m.MotelId == r.MotelId && m.OwnerId == ownerId))
                .Select(r => new { r.RoomId, RoomDisplayName = $"{allMotels.FirstOrDefault(m => m.MotelId == r.MotelId)?.MotelName} - {r.RoomName}" })
                .ToList();

            if (roomId.HasValue && roomId.Value > 0 && !ownerRooms.Any(r => r.RoomId == roomId.Value))
            {
                TempData["Error"] = "ID phòng không hợp lệ hoặc không thuộc về bạn.";
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
                if (!roomId.HasValue || roomId.Value == 0)
                {
                    roomId = ownerRooms.FirstOrDefault(r => r.RoomDisplayName.Contains(motel.MotelName))?.RoomId;
                }
            }


            ViewBag.Rooms = new SelectList(ownerRooms, "RoomId", "RoomDisplayName", roomId);
            ViewBag.MotelId = motelId;
            // TenantId sẽ là null hoặc 0 nếu Owner tạo
            return View(new CreateIncidentDto { RoomId = roomId ?? 0, TenantId = null });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateIncidentDto incidentDto)
        {
            int? currentMotelId = null;
            try
            {
                var ownerId = GetCurrentOwnerId();
                var room = await _roomService.GetByIdAsync(incidentDto.RoomId);
                if (room == null)
                {
                    return NotFound();
                }
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền báo cáo sự cố cho phòng này.";
                    return NotFound();
                }
                currentMotelId = motel.MotelId;

                if (!ModelState.IsValid)
                {
                    var allRooms = await _roomService.GetAllRoomsAsync();
                    var allMotels = await _motelService.GetAllMotelsAsync();

                    var ownerRooms = allRooms
                        .Where(r => allMotels.Any(m => m.MotelId == r.MotelId && m.OwnerId == ownerId))
                        .Select(r => new { r.RoomId, RoomDisplayName = $"{allMotels.FirstOrDefault(m => m.MotelId == r.MotelId)?.MotelName} - {r.RoomName}" })
                        .ToList();
                    ViewBag.Rooms = new SelectList(ownerRooms, "RoomId", "RoomDisplayName", incidentDto.RoomId);
                    ViewBag.MotelId = currentMotelId;

                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    return View(incidentDto);
                }

                // Xử lý upload ảnh
                if (incidentDto.AttachedImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "incidents");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + incidentDto.AttachedImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await incidentDto.AttachedImageFile.CopyToAsync(fileStream);
                    }
                    incidentDto.AttachedImagePath = Path.Combine("images", "incidents", uniqueFileName);
                }

                incidentDto.TenantId = null;
                await _incidentService.AddIncidentAsync(incidentDto);
                TempData["Success"] = "Tạo sự cố mới thành công!";
                return RedirectToAction(nameof(Index), new { roomId = incidentDto.RoomId, motelId = currentMotelId });
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
                return View(incidentDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi tạo sự cố: " + ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                return View(incidentDto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var incident = await _incidentService.GetByIdAsync(id);
                if (incident == null)
                {
                    return NotFound();
                }

                await PopulateIncidentDisplayData(incident);

                var room = await _roomService.GetByIdAsync(incident.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa sự cố này.";
                    return NotFound();
                }

                var allRooms = await _roomService.GetAllRoomsAsync();
                var allMotels = await _motelService.GetAllMotelsAsync();

                var ownerRooms = allRooms
                    .Where(r => allMotels.Any(m => m.MotelId == r.MotelId && m.OwnerId == ownerId))
                    .Select(r => new { r.RoomId, RoomDisplayName = $"{allMotels.FirstOrDefault(m => m.MotelId == r.MotelId)?.MotelName} - {r.RoomName}" })
                    .ToList();
                ViewBag.Rooms = new SelectList(ownerRooms, "RoomId", "RoomDisplayName", incident.RoomId);
                ViewBag.RoomId = room.RoomId;
                ViewBag.RoomName = room.RoomName;
                ViewBag.MotelId = motel.MotelId;
                ViewBag.MotelName = motel.MotelName;

                return View(incident);
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
                TempData["Error"] = "Đã xảy ra lỗi khi tải sự cố để chỉnh sửa: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IncidentDto incidentDto)
        {
            int? currentMotelId = null;
            int? currentRoomId = null;
            try
            {
                var ownerId = GetCurrentOwnerId();
                var room = await _roomService.GetByIdAsync(incidentDto.RoomId);
                if (room == null)
                {
                    return NotFound();
                }
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa sự cố cho phòng này.";
                    return NotFound();
                }
                currentMotelId = motel.MotelId;
                currentRoomId = room.RoomId;

                if (!ModelState.IsValid)
                {
                    var allRooms = await _roomService.GetAllRoomsAsync();
                    var allMotels = await _motelService.GetAllMotelsAsync();

                    var ownerRooms = allRooms
                        .Where(r => allMotels.Any(m => m.MotelId == r.MotelId && m.OwnerId == ownerId))
                        .Select(r => new { r.RoomId, RoomDisplayName = $"{allMotels.FirstOrDefault(m => m.MotelId == r.MotelId)?.MotelName} - {r.RoomName}" })
                        .ToList();
                    ViewBag.Rooms = new SelectList(ownerRooms, "RoomId", "RoomDisplayName", incidentDto.RoomId);
                    ViewBag.MotelId = currentMotelId;
                    ViewBag.RoomId = currentRoomId;

                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    return View(incidentDto);
                }

                var existingIncident = await _incidentService.GetByIdAsync(incidentDto.IncidentId);
                if (existingIncident != null)
                {
                    incidentDto.CreatedAt = existingIncident.CreatedAt;
                    incidentDto.TenantId = existingIncident.TenantId; 
                }
                incidentDto.UpdatedAt = DateTime.UtcNow;

                await _incidentService.UpdateIncidentAsync(incidentDto);
                TempData["Success"] = "Cập nhật sự cố thành công!";
                return RedirectToAction(nameof(Index), new { roomId = incidentDto.RoomId, motelId = currentMotelId });
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
                return View(incidentDto);
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["Error"] = "Lỗi: " + ex.Message;
                return View(incidentDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi cập nhật sự cố: " + ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                return View(incidentDto);
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
                var incident = await _incidentService.GetByIdAsync(id);
                if (incident == null)
                {
                    return NotFound();
                }
                currentRoomId = incident.RoomId;

                var room = await _roomService.GetByIdAsync(incident.RoomId);
                var motel = await _motelService.GetByIdAsync(room.MotelId);
                if (motel == null || motel.OwnerId != ownerId)
                {
                    TempData["Error"] = "Bạn không có quyền xóa sự cố này.";
                    return NotFound();
                }
                currentMotelId = motel.MotelId;

                await _incidentService.DeleteIncidentAsync(id);
                TempData["Success"] = "Xóa sự cố thành công!";
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
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn khi xóa sự cố: " + ex.Message;
            }

            return RedirectToAction(nameof(Index), new { roomId = currentRoomId, motelId = currentMotelId });
        }
    }
}
