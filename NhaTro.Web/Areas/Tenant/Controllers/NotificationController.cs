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
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IContractService _contractService;
        private readonly IRoomService _roomService;
        private readonly IMotelService _motelService;

        public NotificationController(INotificationService notificationService, IContractService contractService, IRoomService roomService, IMotelService motelService)
        {
            _notificationService = notificationService;
            _contractService = contractService;
            _roomService = roomService;
            _motelService = motelService;
        }

        // GET: Tenant/Notification
        public async Task<IActionResult> Index()
        {
            var tenantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var notifications = await _notificationService.GetAllNotificationsAsync();
            var tenantNotifications = notifications.Where(n => n.SenderUserId == tenantId || n.ReceiverUserId == tenantId);
            return View(tenantNotifications);
        }

        // GET: Tenant/Notification/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var tenantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var notification = await _notificationService.GetByIdAsync(id);
            if (notification == null || (notification.SenderUserId != tenantId && notification.ReceiverUserId != tenantId))
            {
                return NotFound();
            }
            return View(notification);
        }

        // GET: Tenant/Notification/Create
        public IActionResult Create(int? contractId)
        {
            var model = new CreateNotificationDto();
            if (contractId.HasValue)
            {
                model.RelatedContractId = contractId;
            }
            return View(model);
        }

        // POST: Tenant/Notification/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNotificationDto notificationDto)
        {
            if (!ModelState.IsValid)
            {
                return View(notificationDto);
            }

            var tenantId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            notificationDto.SenderUserId = tenantId;

            if (notificationDto.RelatedContractId.HasValue)
            {
                var contract = await _contractService.GetByIdAsync(notificationDto.RelatedContractId.Value);
                if (contract == null || contract.TenantId != tenantId)
                {
                    return NotFound();
                }
                var room = await _roomService.GetByIdAsync(contract.RoomId);
                notificationDto.RelatedMotelId = room.MotelId;
            }

            await _notificationService.AddNotificationAsync(notificationDto);
            return RedirectToAction(nameof(Index));
        }
    }
}