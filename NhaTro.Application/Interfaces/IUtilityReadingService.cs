using NhaTro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NhaTro.Application.Interfaces
{
    public interface IUtilityReadingService
    {
        Task<IEnumerable<UtilityReadingDto>> GetAllUtilityReadingsAsync();
        Task<UtilityReadingDto> GetByIdAsync(int readingId);
        Task AddUtilityReadingAsync(CreateUtilityReadingDto readingDto);
        Task UpdateUtilityReadingAsync(UtilityReadingDto readingDto);
        Task DeleteUtilityReadingAsync(int readingId);
    }
}