using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaTro.Application.Interfaces;
using NhaTro.Application.DTOs;
using System.Threading.Tasks;

namespace NhaTro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RoomController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IMotelService _motelService;

        public RoomController(IRoomService roomService, IMotelService motelService)
        {
            _roomService = roomService;
            _motelService = motelService;
        }

        public async Task<IActionResult> Index(int? motelId)
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            if (motelId.HasValue)
            {
                rooms = rooms.Where(r => r.MotelId == motelId.Value);
            }
            ViewBag.MotelId = motelId;
            return View(rooms);
        }

        public async Task<IActionResult> Details(int id)
        {
            var room = await _roomService.GetByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        public async Task<IActionResult> Create(int motelId)
        {
            ViewBag.MotelId = motelId;
            return View(new CreateRoomDto { MotelId = motelId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoomDto roomDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MotelId = roomDto.MotelId;
                return View(roomDto);
            }

            try
            {
                await _roomService.AddRoomAsync(roomDto);
                return RedirectToAction(nameof(Index), new { motelId = roomDto.MotelId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.MotelId = roomDto.MotelId;
                return View(roomDto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomService.GetByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            ViewBag.MotelId = room.MotelId;
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoomDto roomDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MotelId = roomDto.MotelId;
                return View(roomDto);
            }

            try
            {
                await _roomService.UpdateRoomAsync(roomDto);
                return RedirectToAction(nameof(Index), new { motelId = roomDto.MotelId });
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.MotelId = roomDto.MotelId;
                return View(roomDto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _roomService.GetByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            try
            {
                await _roomService.DeleteRoomAsync(id);
                return RedirectToAction(nameof(Index), new { motelId = room.MotelId });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}