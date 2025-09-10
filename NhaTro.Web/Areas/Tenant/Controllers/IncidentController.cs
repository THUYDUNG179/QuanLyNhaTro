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
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace NhaTro.Web.Areas.Tenant.Controllers
{
    [Area("Tenant")]
    [Authorize(Roles = "Tenant")]
    public class IncidentController : Controller
    {
        private readonly IIncidentService _incidentService;
        private readonly IRoomService _roomService;
        private readonly IContractService _contractService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IncidentController(
            IIncidentService incidentService,
            IRoomService roomService,
            IContractService contractService,
            IUserService userService,
            IWebHostEnvironment webHostEnvironment)
        {
            _incidentService = incidentService;
            _roomService = roomService;
            _contractService = contractService;
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
        }

        protected int GetCurrentTenantId()
        {
            var tenantIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (tenantIdClaim == null || !int.TryParse(tenantIdClaim.Value, out int tenantId))
            {
                throw new InvalidOperationException("Không thể xác định ID người thuê hiện tại.");
            }
            return tenantId;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var tenantId = GetCurrentTenantId();
                var incidents = await _incidentService.GetAllIncidentsAsync();
                var rooms = await _roomService.GetAllRoomsAsync();
                var users = await _userService.GetAllUsersAsync();

                var tenantIncidents = (from incident in incidents
                                       where incident.TenantId == tenantId
                                       select new IncidentDto
                                       {
                                           IncidentId = incident.IncidentId,
                                           Title = incident.Title,
                                           Description = incident.Description,
                                           Status = incident.Status,
                                           CreatedAt = incident.CreatedAt,
                                           TenantId = incident.TenantId,
                                           RoomId = incident.RoomId,
                                           RoomName = rooms.FirstOrDefault(r => r.RoomId == incident.RoomId)?.RoomName,
                                           TenantName = users.FirstOrDefault(u => u.UserId == tenantId)?.FullName
                                       }).ToList();

                return View(tenantIncidents);
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
                var tenantId = GetCurrentTenantId();
                var incident = await _incidentService.GetByIdAsync(id);
                if (incident == null || incident.TenantId != tenantId)
                {
                    return NotFound();
                }

                incident.RoomName = (await _roomService.GetByIdAsync(incident.RoomId))?.RoomName;
                incident.TenantName = (await _userService.GetByIdAsync(tenantId))?.FullName;

                return View(incident);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải chi tiết sự cố: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var tenantId = GetCurrentTenantId();
                var contracts = await _contractService.GetContractsByTenantIdAsync(tenantId);
                var rooms = new List<RoomDto>();
                foreach (var contract in contracts)
                {
                    var room = await _roomService.GetByIdAsync(contract.RoomId);
                    if (room != null) rooms.Add(room);
                }

                ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomName");
                return View(new CreateIncidentDto());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tải form: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateIncidentDto createIncidentDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Error"] = string.Join("; ", errors);
                var tenantId = GetCurrentTenantId();
                var contracts = await _contractService.GetContractsByTenantIdAsync(tenantId);
                var rooms = contracts.Select(c => new RoomDto { RoomId = c.RoomId, RoomName = c.RoomName }).ToList();
                ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomName", createIncidentDto.RoomId);
                return View(createIncidentDto);
            }

            try
            {
                var tenantId = GetCurrentTenantId();
                createIncidentDto.TenantId = tenantId;

                await _incidentService.AddIncidentAsync(createIncidentDto);
                TempData["Success"] = "Đã báo cáo sự cố thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi tạo sự cố: " + ex.Message;
                var tenantId = GetCurrentTenantId();
                var contracts = await _contractService.GetContractsByTenantIdAsync(tenantId);
                var rooms = contracts.Select(c => new RoomDto { RoomId = c.RoomId, RoomName = c.RoomName }).ToList();
                ViewBag.Rooms = new SelectList(rooms, "RoomId", "RoomName", createIncidentDto.RoomId);
                return View(createIncidentDto);
            }
        }
    }
}
