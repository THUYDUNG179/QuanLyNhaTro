using NhaTro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NhaTro.Application.Interfaces
{
    public interface IIncidentService
    {
        Task<IEnumerable<IncidentDto>> GetAllIncidentsAsync();
        Task<IncidentDto> GetByIdAsync(int incidentId);
        Task AddIncidentAsync(CreateIncidentDto incidentDto);
        Task UpdateIncidentAsync(IncidentDto incidentDto);
        Task DeleteIncidentAsync(int incidentId);
        Task<int> GetTotalIncidentsCountAsync();

    }
}