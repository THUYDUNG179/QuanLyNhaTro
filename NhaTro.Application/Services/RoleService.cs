using NhaTro.Application.DTOs;
using NhaTro.Application.Interfaces;
using NhaTro.Domain.Interfaces;
using NhaTro.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NhaTro.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<Role> _roleRepository;

        public RoleService(IRepository<Role> roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName
            }).ToList();
        }

        public async Task<RoleDto> GetByIdAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                throw new KeyNotFoundException($"Vai trò với ID {id} không tìm thấy.");

            return new RoleDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName
            };
        }

        public async Task AddRoleAsync(RoleDto roleDto)
        {
            var role = new Role
            {
                RoleName = roleDto.RoleName
            };

            await _roleRepository.AddAsync(role);
            await _roleRepository.SaveChangesAsync();
        }

        public async Task UpdateRoleAsync(RoleDto roleDto)
        {
            var role = await _roleRepository.GetByIdAsync(roleDto.RoleId);
            if (role == null)
                throw new KeyNotFoundException($"Vai trò với ID {roleDto.RoleId} không tìm thấy.");

            role.RoleName = roleDto.RoleName;

            await _roleRepository.UpdateAsync(role);
            await _roleRepository.SaveChangesAsync();
        }

        public async Task DeleteRoleAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                throw new KeyNotFoundException($"Vai trò với ID {id} không tìm thấy.");

            await _roleRepository.DeleteAsync(role);
            await _roleRepository.SaveChangesAsync();
        }
    }

}