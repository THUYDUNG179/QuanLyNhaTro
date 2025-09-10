using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaTro.Application.Interfaces;
using NhaTro.Application.DTOs;
using System.Threading.Tasks;

namespace NhaTro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class IncidentController : Controller
    {
        private readonly IIncidentService _incidentService;

        public IncidentController(IIncidentService incidentService)
        {
            _incidentService = incidentService;
        }

        public async Task<IActionResult> Index()
        {
            var incidents = await _incidentService.GetAllIncidentsAsync();
            return View(incidents);
        }

        public async Task<IActionResult> Details(int id)
        {
            var incident = await _incidentService.GetByIdAsync(id);
            if (incident == null)
            {
                return NotFound();
            }
            return View(incident);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateIncidentDto incidentDto)
        {
            if (!ModelState.IsValid)
            {
                return View(incidentDto);
            }

            await _incidentService.AddIncidentAsync(incidentDto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var incident = await _incidentService.GetByIdAsync(id);
            if (incident == null)
            {
                return NotFound();
            }
            return View(incident);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IncidentDto incidentDto)
        {
            if (!ModelState.IsValid)
            {
                return View(incidentDto);
            }

            try
            {
                await _incidentService.UpdateIncidentAsync(incidentDto);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(incidentDto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _incidentService.DeleteIncidentAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}