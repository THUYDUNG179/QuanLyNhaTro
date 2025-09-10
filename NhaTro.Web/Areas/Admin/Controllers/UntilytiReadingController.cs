using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaTro.Application.Interfaces;
using NhaTro.Application.DTOs;
using System.Threading.Tasks;

namespace NhaTro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UtilityReadingController : Controller
    {
        private readonly IUtilityReadingService _utilityReadingService;

        public UtilityReadingController(IUtilityReadingService utilityReadingService)
        {
            _utilityReadingService = utilityReadingService;
        }

        public async Task<IActionResult> Index()
        {
            var utilityReadings = await _utilityReadingService.GetAllUtilityReadingsAsync();
            return View(utilityReadings);
        }

        public async Task<IActionResult> Details(int id)
        {
            var utilityReading = await _utilityReadingService.GetByIdAsync(id);
            if (utilityReading == null)
            {
                return NotFound();
            }
            return View(utilityReading);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUtilityReadingDto utilityReadingDto)
        {
            if (!ModelState.IsValid)
            {
                return View(utilityReadingDto);
            }

            try
            {
                await _utilityReadingService.AddUtilityReadingAsync(utilityReadingDto);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(utilityReadingDto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var utilityReading = await _utilityReadingService.GetByIdAsync(id);
            if (utilityReading == null)
            {
                return NotFound();
            }
            return View(utilityReading);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UtilityReadingDto utilityReadingDto)
        {
            if (!ModelState.IsValid)
            {
                return View(utilityReadingDto);
            }

            try
            {
                await _utilityReadingService.UpdateUtilityReadingAsync(utilityReadingDto);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(utilityReadingDto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _utilityReadingService.DeleteUtilityReadingAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}