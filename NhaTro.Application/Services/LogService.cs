using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NhaTro.Application.DTOs;
using NhaTro.Application.Interfaces;
using NhaTro.Domain.Interfaces;
using NhaTro.Infrastructure;

namespace NhaTro.Application.Services
{
    public class LogService : ILogService
    {
        private readonly IRepository<Log> _logRepository;
        public LogService(IRepository<Log> logRepository)
        {
            _logRepository = logRepository;
        }
        Task<IEnumerable<LogDto>> ILogService.GetAllLogsAsync()
        {
            throw new NotImplementedException();
        }

        Task<LogDto> ILogService.GetByIdAsync(int logId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<LogDto>> GetLogsByUserIdAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task AddLogAsync(CreateLogDto logDto)
        {
            throw new NotImplementedException();
        }
    }
}
