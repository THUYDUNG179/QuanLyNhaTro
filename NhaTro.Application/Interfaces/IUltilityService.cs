using NhaTro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NhaTro.Application.Interfaces
{
    public interface IUtilityService
    {
        Task<IEnumerable<UtilityDto>> GetAllUtilitiesAsync();
        Task<UtilityDto> GetByIdAsync(int utilityId);
        Task AddUtilityAsync(CreateUtilityDto utilityDto);
        Task UpdateUtilityAsync(UtilityDto utilityDto);
        Task DeleteUtilityAsync(int utilityId);
    }
}