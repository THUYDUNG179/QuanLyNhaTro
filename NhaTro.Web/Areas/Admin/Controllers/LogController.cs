using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NhaTro.Application.Interfaces;
using System.Threading.Tasks;

namespace NhaTro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LogController : Controller
    {
        private readonly ILogService _logService;

        public LogController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var logs = await _logService.GetAllLogsAsync();
            return View(logs);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var log = await _logService.GetByIdAsync(id);
                return View(log);
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}