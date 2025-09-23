using NhaTro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NhaTro.Application.Interfaces
{
    public interface IRoomStatusService
    {
        Task<IEnumerable<RoomStatusDto>> GetAllRoomStatusesAsync();
        Task<RoomStatusDto> GetByIdAsync(int roomStatusId);
        Task AddRoomStatusAsync(CreateRoomStatusDto roomStatusDto);
        Task UpdateRoomStatusAsync(RoomStatusDto roomStatusDto);
        Task DeleteRoomStatusAsync(int roomStatusId);

    }
}