using NhaTro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NhaTro.Application.Interfaces
{
    public interface ILogService
    {
        Task<IEnumerable<LogDto>> GetAllLogsAsync();
        Task<LogDto> GetByIdAsync(int logId);
        Task<IEnumerable<LogDto>> GetLogsByUserIdAsync(int userId);
        Task AddLogAsync(CreateLogDto logDto);
    }
}