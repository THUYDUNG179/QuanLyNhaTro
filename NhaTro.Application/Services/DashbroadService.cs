using Microsoft.EntityFrameworkCore;
using NhaTro.Application.Interfaces;
using NhaTro.Infrastructure.Data; // DbContext của bạn
using System.Linq;
using System.Threading.Tasks;

namespace NhaTro.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly QLyNhaTroDbContext _context;

        public DashboardService(QLyNhaTroDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalUsersAsync()
            => await _context.Users.CountAsync();

        public async Task<int> GetTotalMotelsAsync()
            => await _context.Motels.CountAsync();

        public async Task<int> GetTotalRoomsAsync()
            => await _context.Rooms.CountAsync();

        public async Task<int> GetTotalContractsAsync()
            => await _context.Contracts.CountAsync();

        public async Task<decimal> GetMonthlyRevenueAsync()
        {
            var now = DateTime.Now;
            return await _context.Payments
                .Where(p => p.PaymentDate.Month == now.Month && p.PaymentDate.Year == now.Year)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
        }

        public async Task<int> GetPendingIncidentsAsync()
            => await _context.Incidents.CountAsync(i => i.Status == "Reported");

        // === Charts ===

        public async Task<object> GetUserRoleDistributionAsync()
        {
            var data = await _context.Users
                .GroupBy(u => u.RoleId)
                .Select(g => new { RoleId = g.Key, Count = g.Count() })
                .ToListAsync();

            var labels = await _context.Roles
                .Where(r => data.Select(d => d.RoleId).Contains(r.RoleId))
                .Select(r => r.RoleName)
                .ToListAsync();

            return new { labels, values = data.Select(d => d.Count).ToList() };
        }

        public async Task<object> GetRoomStatusDistributionAsync()
        {
            var data = await _context.Rooms
                .GroupBy(r => r.RoomStatusId)
                .Select(g => new { StatusId = g.Key, Count = g.Count() })
                .ToListAsync();

            var labels = await _context.RoomStatuses
                .Where(s => data.Select(d => d.StatusId).Contains(s.RoomStatusId))
                .Select(s => s.StatusName)
                .ToListAsync();

            return new { labels, values = data.Select(d => d.Count).ToList() };
        }

        public async Task<object> GetContractTrendAsync()
        {
            var data = await _context.Contracts
                .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            var labels = data.Select(d => $"{d.Month}/{d.Year}").ToList();
            var values = data.Select(d => d.Count).ToList();

            return new { labels, values };
        }

        public async Task<object> GetRevenueTrendAsync()
        {
            var data = await _context.Payments
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Amount) })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            var labels = data.Select(d => $"{d.Month}/{d.Year}").ToList();
            var values = data.Select(d => d.Total).ToList();

            return new { labels, values };
        }

        public async Task<object> GetIncidentStatusDistributionAsync()
        {
            var data = await _context.Incidents
                .GroupBy(i => i.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var labels = data.Select(d => d.Status).ToList();
            var values = data.Select(d => d.Count).ToList();

            return new { labels, values };
        }
    }
}
