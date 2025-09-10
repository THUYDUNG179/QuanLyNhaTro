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
    public class UtilityService : IUtilityService
    {
        private readonly IRepository<Utility> _utilityRepository;

        public UtilityService(IRepository<Utility> utilityRepository)
        {
            _utilityRepository = utilityRepository;
        }

        public async Task<IEnumerable<UtilityDto>> GetAllUtilitiesAsync()
        {
            var utilities = await _utilityRepository.GetAllAsync();
            return utilities.Select(u => new UtilityDto
            {
                UtilityId = u.UtilityId,
                UtilityName = u.UtilityName,
                Unit = u.Unit
            }).ToList();
        }

        public async Task<UtilityDto> GetByIdAsync(int utilityId)
        {
            var utility = await _utilityRepository.GetByIdAsync(utilityId);
            if (utility == null)
                throw new KeyNotFoundException($"Utility with ID {utilityId} not found.");

            return new UtilityDto
            {
                UtilityId = utility.UtilityId,
                UtilityName = utility.UtilityName,
                Unit = utility.Unit
            };
        }

        public async Task AddUtilityAsync(CreateUtilityDto utilityDto)
        {
            var utility = new Utility
            {
                UtilityName = utilityDto.UtilityName,
                Unit = utilityDto.Unit
            };

            await _utilityRepository.AddAsync(utility);
            await _utilityRepository.SaveChangesAsync();
        }

        public async Task UpdateUtilityAsync(UtilityDto utilityDto)
        {
            var utility = await _utilityRepository.GetByIdAsync(utilityDto.UtilityId);
            if (utility == null)
                throw new KeyNotFoundException($"Utility with ID {utilityDto.UtilityId} not found.");

            utility.UtilityName = utilityDto.UtilityName;
            utility.Unit = utilityDto.Unit;

            await _utilityRepository.UpdateAsync(utility);
            await _utilityRepository.SaveChangesAsync();
        }

        public async Task DeleteUtilityAsync(int utilityId)
        {
            var utility = await _utilityRepository.GetByIdAsync(utilityId);
            if (utility == null)
                throw new KeyNotFoundException($"Utility with ID {utilityId} not found.");

            await _utilityRepository.DeleteAsync(utility);
            await _utilityRepository.SaveChangesAsync();
        }
    }
}