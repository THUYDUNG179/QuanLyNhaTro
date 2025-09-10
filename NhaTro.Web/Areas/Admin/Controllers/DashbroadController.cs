using Microsoft.AspNetCore.Mvc;
using NhaTro.Application.Interfaces;
using System.Threading.Tasks;

namespace NhaTro.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                ViewBag.TotalUsers = await _dashboardService.GetTotalUsersAsync();
                ViewBag.TotalMotels = await _dashboardService.GetTotalMotelsAsync();
                ViewBag.TotalRooms = await _dashboardService.GetTotalRoomsAsync();
                ViewBag.TotalContracts = await _dashboardService.GetTotalContractsAsync();
                ViewBag.MonthlyRevenue = await _dashboardService.GetMonthlyRevenueAsync();
                ViewBag.PendingIncidents = await _dashboardService.GetPendingIncidentsAsync();
                // Bổ sung thêm các phương thức khác nếu cần thiết
            }
            catch
            {
                // Xử lý lỗi một cách an toàn để tránh NullReferenceException
                ViewBag.TotalUsers = 0;
                ViewBag.TotalMotels = 0;
                ViewBag.TotalRooms = 0;
                ViewBag.TotalContracts = 0;
                ViewBag.MonthlyRevenue = 0;
                ViewBag.PendingIncidents = 0;
            }

            return View();
        }

        // API cho Chart.js
        [HttpGet]
        public async Task<IActionResult> GetUserRoleDistribution()
            => Json(await _dashboardService.GetUserRoleDistributionAsync());

        [HttpGet]
        public async Task<IActionResult> GetRoomStatusDistribution()
            => Json(await _dashboardService.GetRoomStatusDistributionAsync());

        [HttpGet]
        public async Task<IActionResult> GetContractTrend()
            => Json(await _dashboardService.GetContractTrendAsync());

        [HttpGet]
        public async Task<IActionResult> GetRevenueTrend()
            => Json(await _dashboardService.GetRevenueTrendAsync());

        [HttpGet]
        public async Task<IActionResult> GetIncidentStatusDistribution()
            => Json(await _dashboardService.GetIncidentStatusDistributionAsync());
    }
}
