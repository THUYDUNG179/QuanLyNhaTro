using NhaTro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NhaTro.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetByIdAsync(int userId);
        Task AddUserAsync(CreateUserDto userDto);
        Task UpdateUserAsync(UserDto userDto);
        Task DeleteUserAsync(int userId);
        Task<UserDto> AuthenticateAsync(string email, string password);
        Task<int> GetTotalTenantsCountAsync();
        Task<IEnumerable<UserDto>> GetAllTenantsForOwnerAsync(int ownerId);
    }
}