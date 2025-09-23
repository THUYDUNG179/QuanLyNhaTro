using NhaTro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NhaTro.Application.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDto>> GetAllRoomsAsync();
        Task<RoomDto> GetByIdAsync(int roomId);
        Task AddRoomAsync(CreateRoomDto roomDto);
        Task UpdateRoomAsync(RoomDto roomDto);
        Task DeleteRoomAsync(int roomId);
        Task<int> GetTotalRoomsCountAsync();
        Task<IEnumerable<RoomDto>> GetRoomsByMotelIdAsync(int motelId);
        Task<IEnumerable<RoomDto>> GetRoomsByOwnerIdAsync(int ownerId);
    }
}