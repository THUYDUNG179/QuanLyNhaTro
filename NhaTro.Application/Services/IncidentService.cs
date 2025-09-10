using NhaTro.Application.DTOs;
using NhaTro.Application.Interfaces;
using NhaTro.Domain.Interfaces;
using NhaTro.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NhaTro.Application.Services
{
    public class IncidentService : IIncidentService
    {
        private readonly IRepository<Incident> _incidentRepository;

        public IncidentService(IRepository<Incident> incidentRepository)
        {
            _incidentRepository = incidentRepository;
        }

        public async Task<IEnumerable<IncidentDto>> GetAllIncidentsAsync()
        {
            var incidents = await _incidentRepository.GetAllAsync();
            return incidents.Select(i => new IncidentDto
            {
                IncidentId = i.IncidentId,
                RoomId = i.RoomId,
                TenantId = i.TenantId,
                Title = i.Title,
                Description = i.Description,
                Priority = i.Priority,
                Status = i.Status,
                AttachedImagePath = i.AttachedImagePath,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                ResolvedAt = i.ResolvedAt
            }).ToList();
        }

        public async Task<IncidentDto> GetByIdAsync(int incidentId)
        {
            var incident = await _incidentRepository.GetByIdAsync(incidentId);
            if (incident == null)
                throw new KeyNotFoundException($"Incident with ID {incidentId} not found.");

            return new IncidentDto
            {
                IncidentId = incident.IncidentId,
                RoomId = incident.RoomId,
                TenantId = incident.TenantId,
                Title = incident.Title,
                Description = incident.Description,
                Priority = incident.Priority,
                Status = incident.Status,
                AttachedImagePath = incident.AttachedImagePath,
                CreatedAt = incident.CreatedAt,
                UpdatedAt = incident.UpdatedAt,
                ResolvedAt = incident.ResolvedAt
            };
        }

        public async Task AddIncidentAsync(CreateIncidentDto incidentDto)
        {
            var incident = new Incident
            {
                RoomId = incidentDto.RoomId,
                TenantId = (int)incidentDto.TenantId,
                Title = incidentDto.Title,
                Description = incidentDto.Description,
                Priority = incidentDto.Priority,
                Status = "Reported",
                AttachedImagePath = incidentDto.AttachedImagePath,
                CreatedAt = DateTime.UtcNow
            };

            await _incidentRepository.AddAsync(incident);
            await _incidentRepository.SaveChangesAsync();
        }

        public async Task UpdateIncidentAsync(IncidentDto incidentDto)
        {
            var incident = await _incidentRepository.GetByIdAsync(incidentDto.IncidentId);
            if (incident == null)
                throw new KeyNotFoundException($"Incident with ID {incidentDto.IncidentId} not found.");

            incident.RoomId = incidentDto.RoomId;
            incident.TenantId = (int)incidentDto.TenantId;
            incident.Title = incidentDto.Title;
            incident.Description = incidentDto.Description;
            incident.Priority = incidentDto.Priority;
            incident.Status = incidentDto.Status;
            incident.AttachedImagePath = incidentDto.AttachedImagePath;
            incident.UpdatedAt = DateTime.UtcNow;

            await _incidentRepository.UpdateAsync(incident);
            await _incidentRepository.SaveChangesAsync();
        }

        public async Task DeleteIncidentAsync(int incidentId)
        {
            var incident = await _incidentRepository.GetByIdAsync(incidentId);
            if (incident == null)
                throw new KeyNotFoundException($"Incident with ID {incidentId} not found.");

            await _incidentRepository.DeleteAsync(incident);
            await _incidentRepository.SaveChangesAsync();
        }

        public async Task ResolveIncidentAsync(int incidentId)
        {
            var incident = await _incidentRepository.GetByIdAsync(incidentId);
            if (incident == null)
                throw new KeyNotFoundException($"Incident with ID {incidentId} not found.");

            incident.Status = "Resolved";
            incident.ResolvedAt = DateTime.UtcNow;
            incident.UpdatedAt = DateTime.UtcNow;

            await _incidentRepository.UpdateAsync(incident);
            await _incidentRepository.SaveChangesAsync();
        }
        public async Task<int> GetTotalIncidentsCountAsync()
        {
            var allIncidents = await _incidentRepository.GetAllAsync();
            return allIncidents.Count();
        }
    }
}