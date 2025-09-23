using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NhaTro.Application.Interfaces;
using NhaTro.Application.DTOs;
using System.Security.Claims;

namespace NhaTro.Web.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IMotelService _motelService;
        private readonly IRoomService _roomService;
        private readonly IContractService _contractService;

        public NotificationController(
            INotificationService notificationService,
            IMotelService motelService,
            IRoomService roomService,
            IContractService contractService)
        {
            _notificationService = notificationService;
            _motelService = motelService;
            _roomService = roomService;
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

        private async Task PopulateViewDataForCreate(int ownerId)
        {
            // Lấy danh sách nhà trọ của chủ nhà
            var ownerMotels = (await _motelService.GetMotelsByOwnerIdAsync(ownerId)).ToList();

            // Lấy danh sách các phòng có hợp đồng còn hiệu lực
            var activeContracts = (await _contractService.GetAllContractsAsync())
                                    .Where(c => c.EndDate == null || c.EndDate.Value >= DateOnly.FromDateTime(DateTime.Today))
                                    .ToList();

            var roomsWithTenants = new List<object>();
            foreach (var motel in ownerMotels)
            {
                var roomsInMotel = (await _roomService.GetAllRoomsAsync()).Where(r => r.MotelId == motel.MotelId).ToList();
                foreach (var room in roomsInMotel)
                {
                    // Tìm hợp đồng đang hoạt động cho phòng
                    var activeContract = activeContracts.FirstOrDefault(c => c.RoomId == room.RoomId);
                    if (activeContract != null)
                    {
                        roomsWithTenants.Add(new
                        {
                            RoomId = room.RoomId,
                            RoomDisplay = $"{motel.MotelName} - {room.RoomName} (Người thuê: {activeContract.TenantName})"
                        });
                    }
                }
            }

            // Lấy danh sách người thuê thuộc sở hữu của chủ nhà
            var allTenants = await _contractService.GetAllTenantsForOwnerAsync(ownerId);
            var tenantList = allTenants.Select(t => new
            {
                t.UserId,
                t.FullName
            }).ToList();

            ViewBag.MotelId = new SelectList(ownerMotels, "MotelId", "MotelName");
            ViewBag.RoomId = new SelectList(roomsWithTenants, "RoomId", "RoomDisplay");
            ViewBag.ReceiverUserId = new SelectList(tenantList, "UserId", "FullName");
        }



        public async Task<IActionResult> Index(int? motelId, int? roomId, bool? unreadOnly)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var notifications = await _notificationService.GetNotificationsForOwnerAsync(ownerId, motelId, roomId, unreadOnly);

                if (motelId.HasValue && motelId.Value > 0)
                {
                    var motel = await _motelService.GetByIdAsync(motelId.Value);
                    ViewBag.MotelName = motel?.MotelName;
                    ViewBag.MotelId = motelId.Value;
                }
                else if (roomId.HasValue && roomId.Value > 0)
                {
                    var room = await _roomService.GetByIdAsync(roomId.Value);
                    if (room != null)
                    {
                        ViewBag.RoomName = room.RoomName;
                        ViewBag.RoomId = roomId.Value;
                        var motel = await _motelService.GetByIdAsync(room.MotelId);
                        ViewBag.MotelId = motel?.MotelId;
                        ViewBag.MotelName = motel?.MotelName;
                    }
                }

                ViewBag.UnreadOnly = unreadOnly;
                return View(notifications);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách thông báo: " + ex.Message;
                return View(new List<NotificationDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var notification = await _notificationService.GetByIdAsync(id);

                if (notification == null) return NotFound();

                var hasAccess = (notification.SenderUserId == ownerId || notification.ReceiverUserId == ownerId) ||
                                (notification.RelatedMotelId.HasValue && (await _motelService.GetByIdAsync(notification.RelatedMotelId.Value))?.OwnerId == ownerId) ||
                                (notification.RelatedRoomId.HasValue && (await _roomService.GetByIdAsync(notification.RelatedRoomId.Value))?.MotelId == (await _motelService.GetByIdAsync((await _roomService.GetByIdAsync(notification.RelatedRoomId.Value))?.MotelId ?? 0))?.MotelId);

                if (!hasAccess)
                {
                    TempData["Error"] = "Bạn không có quyền truy cập thông báo này.";
                    return NotFound();
                }

                if (notification.ReceiverUserId == ownerId && !notification.IsRead)
                {
                    await _notificationService.MarkAsReadAsync(id);
                    notification.IsRead = true;
                }

                return View(notification);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết thông báo: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create()
        {
            await PopulateViewDataForCreate(GetCurrentOwnerId());
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNotificationDto notificationDto)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                notificationDto.SenderUserId = ownerId;

                if (!ModelState.IsValid)
                {
                    await PopulateViewDataForCreate(ownerId);
                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    return View(notificationDto);
                }

                if (notificationDto.RelatedMotelId.HasValue)
                {
                    var motel = await _motelService.GetByIdAsync(notificationDto.RelatedMotelId.Value);
                    if (motel == null || motel.OwnerId != ownerId)
                    {
                        TempData["Error"] = "Bạn không có quyền gửi thông báo liên quan đến nhà trọ này.";
                        return NotFound();
                    }
                }
                if (notificationDto.RelatedRoomId.HasValue)
                {
                    var room = await _roomService.GetByIdAsync(notificationDto.RelatedRoomId.Value);
                    if (room == null || (await _motelService.GetByIdAsync(room.MotelId))?.OwnerId != ownerId)
                    {
                        TempData["Error"] = "Bạn không có quyền gửi thông báo liên quan đến phòng này.";
                        return NotFound();
                    }
                }

                await _notificationService.AddNotificationAsync(notificationDto);
                TempData["Success"] = "Gửi thông báo thành công!";
                return RedirectToAction(nameof(Index));
            } 
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (Exception ex)
            {
                string errorMessage = "Đã xảy ra lỗi không mong muốn khi gửi thông báo.";
                if (ex.InnerException != null)
                {
                    errorMessage = ex.InnerException.Message;
                }

                ModelState.AddModelError(string.Empty, errorMessage);
                TempData["Error"] = errorMessage; // Hiển thị lỗi chi tiết lên TempData

                await PopulateViewDataForCreate(GetCurrentOwnerId()); // Cần tải lại ViewBag
                return View(notificationDto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var notification = await _notificationService.GetByIdAsync(id);
                if (notification == null) return NotFound();

                if (notification.SenderUserId != ownerId)
                {
                    TempData["Error"] = "Bạn chỉ có thể chỉnh sửa thông báo bạn đã gửi.";
                    return NotFound();
                }

                return View(notification);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải thông báo để chỉnh sửa: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NotificationDto notificationDto)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                if (notificationDto.SenderUserId != ownerId)
                {
                    TempData["Error"] = "Bạn chỉ có thể chỉnh sửa thông báo bạn đã gửi.";
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đã nhập.";
                    return View(notificationDto);
                }

                await _notificationService.UpdateNotificationAsync(notificationDto);
                TempData["Success"] = "Cập nhật thông báo thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn khi cập nhật thông báo: " + ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.";
                return View(notificationDto);
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var ownerId = GetCurrentOwnerId();
                var notification = await _notificationService.GetByIdAsync(id);
                if (notification == null) return NotFound();

                if (notification.SenderUserId != ownerId)
                {
                    TempData["Error"] = "Bạn chỉ có thể xóa thông báo bạn đã gửi.";
                    return NotFound();
                }

                await _notificationService.DeleteNotificationAsync(id);
                TempData["Success"] = "Xóa thông báo thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi không mong muốn khi xóa thông báo: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}