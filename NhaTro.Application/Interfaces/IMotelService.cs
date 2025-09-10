using NhaTro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NhaTro.Application.Interfaces
{
    public interface IMotelService
    {
        Task<IEnumerable<MotelDto>> GetAllMotelsAsync();
        Task<MotelDto> GetByIdAsync(int motelId);
        Task AddMotelAsync(CreateMotelDto motelDto);
        Task UpdateMotelAsync(MotelDto motelDto);
        Task DeleteMotelAsync(int motelId);

        // --- Phương thức mới cần thêm ---
        Task<IEnumerable<MotelDto>> GetMotelsByOwnerIdAsync(int ownerId);
        Task<int> GetTotalMotelsCountAsync();
    }
}