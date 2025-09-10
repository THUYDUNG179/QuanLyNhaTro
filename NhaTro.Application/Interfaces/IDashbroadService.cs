using System.Threading.Tasks;

namespace NhaTro.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<int> GetTotalUsersAsync();
        Task<int> GetTotalMotelsAsync();
        Task<int> GetTotalRoomsAsync();
        Task<int> GetTotalContractsAsync();
        Task<decimal> GetMonthlyRevenueAsync();
        Task<int> GetPendingIncidentsAsync();

        Task<object> GetUserRoleDistributionAsync(); // Pie
        Task<object> GetRoomStatusDistributionAsync(); // Bar
        Task<object> GetContractTrendAsync(); // Line
        Task<object> GetRevenueTrendAsync(); // Line
        Task<object> GetIncidentStatusDistributionAsync(); // Bar
    }
}
