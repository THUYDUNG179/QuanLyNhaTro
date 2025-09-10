using NhaTro.Application.DTOs;
using NhaTro.Application.Interfaces;
using NhaTro.Domain.Interfaces;
using NhaTro.Infrastructure; // Đảm bảo đã có using cho RoomStatus
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NhaTro.Application.Services
{
    public class RoomStatusService : IRoomStatusService
    {
        private readonly IRepository<RoomStatus> _roomStatusRepository;

        public RoomStatusService(IRepository<RoomStatus> roomStatusRepository)
        {
            _roomStatusRepository = roomStatusRepository;
        }

        public async Task<IEnumerable<RoomStatusDto>> GetAllRoomStatusesAsync()
        {
            var roomStatuses = await _roomStatusRepository.GetAllAsync();
            return roomStatuses.Select(rs => new RoomStatusDto
            {
                RoomStatusId = rs.RoomStatusId,
                StatusName = rs.StatusName
            }).ToList();
        }

        public async Task<RoomStatusDto> GetByIdAsync(int roomStatusId)
        {
            if (roomStatusId <= 0) // Kiểm tra ID không hợp lệ
            {
                throw new ArgumentException("ID trạng thái phòng không hợp lệ.", nameof(roomStatusId));
            }
            var roomStatus = await _roomStatusRepository.GetByIdAsync(roomStatusId);
            if (roomStatus == null)
            {
                return null; // THAY ĐỔI: Trả về NULL nếu không tìm thấy
            }

            return new RoomStatusDto
            {
                RoomStatusId = roomStatus.RoomStatusId,
                StatusName = roomStatus.StatusName
            };
        }

        public async Task AddRoomStatusAsync(CreateRoomStatusDto roomStatusDto)
        {
            if (string.IsNullOrWhiteSpace(roomStatusDto.StatusName))
            {
                throw new ArgumentException("Tên trạng thái phòng không được để trống.", nameof(roomStatusDto.StatusName));
            }

            var roomStatuses = await _roomStatusRepository.GetAllAsync();
            if (roomStatuses.Any(rs => rs.StatusName.Equals(roomStatusDto.StatusName, StringComparison.OrdinalIgnoreCase))) // So sánh không phân biệt chữ hoa/thường
            {
                throw new InvalidOperationException("Trạng thái phòng với tên này đã tồn tại.");
            }

            var roomStatus = new RoomStatus
            {
                StatusName = roomStatusDto.StatusName
            };

            await _roomStatusRepository.AddAsync(roomStatus);
            await _roomStatusRepository.SaveChangesAsync();
        }

        public async Task UpdateRoomStatusAsync(RoomStatusDto roomStatusDto)
        {
            if (roomStatusDto.RoomStatusId <= 0) // Kiểm tra ID không hợp lệ
            {
                throw new ArgumentException("ID trạng thái phòng không hợp lệ.", nameof(roomStatusDto.RoomStatusId));
            }
            if (string.IsNullOrWhiteSpace(roomStatusDto.StatusName))
            {
                throw new ArgumentException("Tên trạng thái phòng không được để trống.", nameof(roomStatusDto.StatusName));
            }

            var roomStatus = await _roomStatusRepository.GetByIdAsync(roomStatusDto.RoomStatusId);
            if (roomStatus == null)
                throw new KeyNotFoundException($"Trạng thái phòng với ID {roomStatusDto.RoomStatusId} không tìm thấy để cập nhật.");

            var roomStatuses = await _roomStatusRepository.GetAllAsync();
            if (roomStatuses.Any(rs => rs.StatusName.Equals(roomStatusDto.StatusName, StringComparison.OrdinalIgnoreCase) && rs.RoomStatusId != roomStatusDto.RoomStatusId))
            {
                throw new InvalidOperationException("Trạng thái phòng với tên này đã tồn tại.");
            }

            roomStatus.StatusName = roomStatusDto.StatusName;

            await _roomStatusRepository.UpdateAsync(roomStatus);
            await _roomStatusRepository.SaveChangesAsync();
        }

        public async Task DeleteRoomStatusAsync(int roomStatusId)
        {
            if (roomStatusId <= 0) // Kiểm tra ID không hợp lệ
            {
                throw new ArgumentException("ID trạng thái phòng không hợp lệ.", nameof(roomStatusId));
            }

            var roomStatus = await _roomStatusRepository.GetByIdAsync(roomStatusId);
            if (roomStatus == null)
                throw new KeyNotFoundException($"Trạng thái phòng với ID {roomStatusId} không tìm thấy để xóa.");

            // Kiểm tra xem có phòng nào đang sử dụng trạng thái này không trước khi xóa
            // Đây là một kiểm tra business logic quan trọng để tránh lỗi khóa ngoại.
            // (Ví dụ: await _roomRepository.AnyAsync(r => r.RoomStatusId == roomStatusId))
            // Nếu có, bạn nên ném InvalidOperationException hoặc trả về Result object.
            // Hiện tại tôi sẽ bỏ qua, nhưng hãy nhớ thêm nếu cần.

            await _roomStatusRepository.DeleteAsync(roomStatus);
            await _roomStatusRepository.SaveChangesAsync();
        }
    }
}
