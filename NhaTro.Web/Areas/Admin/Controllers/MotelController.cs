using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaTro.Application.Interfaces;
using NhaTro.Application.DTOs;
using System.Threading.Tasks;

namespace NhaTro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MotelController : Controller
    {
        private readonly IMotelService _motelService;

        public MotelController(IMotelService motelService)
        {
            _motelService = motelService;
        }

        public async Task<IActionResult> Index()
        {
            var motels = await _motelService.GetAllMotelsAsync();
            return View(motels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var motel = await _motelService.GetByIdAsync(id);
            if (motel == null)
            {
                return NotFound();
            }
            return View(motel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMotelDto motelDto)
        {
            if (!ModelState.IsValid)
            {
                return View(motelDto);
            }

            await _motelService.AddMotelAsync(motelDto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var motel = await _motelService.GetByIdAsync(id);
            if (motel == null)
            {
                return NotFound();
            }
            return View(motel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MotelDto motelDto)
        {
            if (!ModelState.IsValid)
            {
                return View(motelDto);
            }

            try
            {
                await _motelService.UpdateMotelAsync(motelDto);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(motelDto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _motelService.DeleteMotelAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}