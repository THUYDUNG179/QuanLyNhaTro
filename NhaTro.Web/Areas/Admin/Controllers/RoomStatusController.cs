using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaTro.Application.Interfaces;
using NhaTro.Application.DTOs;
using System.Threading.Tasks;

namespace NhaTro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RoomStatusController : Controller
    {
        private readonly IRoomStatusService _roomStatusService;

        public RoomStatusController(IRoomStatusService roomStatusService)
        {
            _roomStatusService = roomStatusService;
        }

        public async Task<IActionResult> Index()
        {
            var roomStatuses = await _roomStatusService.GetAllRoomStatusesAsync();
            return View(roomStatuses);
        }

        public async Task<IActionResult> Details(int id)
        {
            var roomStatus = await _roomStatusService.GetByIdAsync(id);
            if (roomStatus == null)
            {
                return NotFound();
            }
            return View(roomStatus);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoomStatusDto roomStatusDto)
        {
            if (!ModelState.IsValid)
            {
                return View(roomStatusDto);
            }

            try
            {
                await _roomStatusService.AddRoomStatusAsync(roomStatusDto);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(roomStatusDto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var roomStatus = await _roomStatusService.GetByIdAsync(id);
            if (roomStatus == null)
            {
                return NotFound();
            }
            return View(roomStatus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoomStatusDto roomStatusDto)
        {
            if (!ModelState.IsValid)
            {
                return View(roomStatusDto);
            }

            try
            {
                await _roomStatusService.UpdateRoomStatusAsync(roomStatusDto);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(roomStatusDto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _roomStatusService.DeleteRoomStatusAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}