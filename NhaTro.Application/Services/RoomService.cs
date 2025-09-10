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
    public class RoomService : IRoomService
    {
        private readonly IRepository<Room> _roomRepository;
        private readonly IRepository<Contract> _contractRepository; // Thêm Contract Repository

        public RoomService(IRepository<Room> roomRepository, IRepository<Contract> contractRepository)
        {
            _roomRepository = roomRepository;
            _contractRepository = contractRepository;
        }

        // Lấy tất cả các phòng
        // Phương thức này có thể không cần thiết nếu bạn chỉ truy cập phòng theo nhà trọ
        // nhưng tôi sẽ để lại và sửa nó để đồng nhất với các phương thức khác.
        public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();

            // Lấy danh sách ID phòng có hợp đồng đang có hiệu lực
            var activeContractRoomIds = (await _contractRepository.GetAllAsync())
                                            .Where(c => c.EndDate.HasValue && c.EndDate.Value >= DateOnly.FromDateTime(DateTime.Now))
                                            .Select(c => c.RoomId)
                                            .ToHashSet(); // Sử dụng ToHashSet để truy vấn nhanh hơn

            return rooms.Select(r => new RoomDto
            {
                RoomId = r.RoomId,
                MotelId = r.MotelId,
                RoomName = r.RoomName,
                RentalPrice = r.RentalPrice,
                Area = r.Area,
                RoomStatusId = r.RoomStatusId,
                CreatedAt = r.CreatedAt,
                HasContract = activeContractRoomIds.Contains(r.RoomId) // Gán giá trị HasContract
            }).ToList();
        }

        // Lấy phòng theo ID
        public async Task<RoomDto> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phòng không hợp lệ.", nameof(id));
            }
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
            {
                return null;
            }

            // Kiểm tra xem phòng có hợp đồng không
            var hasContract = (await _contractRepository.GetAllAsync())
                                .Any(c => c.RoomId == id && c.EndDate.HasValue && c.EndDate.Value >= DateOnly.FromDateTime(DateTime.Now));

            return new RoomDto
            {
                RoomId = room.RoomId,
                MotelId = room.MotelId,
                RoomName = room.RoomName,
                RentalPrice = room.RentalPrice,
                Area = room.Area,
                RoomStatusId = room.RoomStatusId,
                CreatedAt = room.CreatedAt,
                HasContract = hasContract
            };
        }

        // Phương thức mới: Lấy danh sách phòng theo MotelId và kiểm tra trạng thái hợp đồng
        public async Task<IEnumerable<RoomDto>> GetRoomsByMotelIdAsync(int motelId)
        {
            var rooms = await _roomRepository.GetAllAsync();
            var motelRooms = rooms.Where(r => r.MotelId == motelId).ToList();

            var activeContractRoomIds = (await _contractRepository.GetAllAsync())
                                            .Where(c => c.EndDate.HasValue && c.EndDate.Value >= DateOnly.FromDateTime(DateTime.Now) && motelRooms.Select(r => r.RoomId).Contains(c.RoomId))
                                            .Select(c => c.RoomId)
                                            .ToHashSet();

            return motelRooms.Select(r => new RoomDto
            {
                RoomId = r.RoomId,
                MotelId = r.MotelId,
                RoomName = r.RoomName,
                RentalPrice = r.RentalPrice,
                Area = r.Area,
                RoomStatusId = r.RoomStatusId,
                CreatedAt = r.CreatedAt,
                HasContract = activeContractRoomIds.Contains(r.RoomId)
            }).ToList();
        }

        public async Task AddRoomAsync(CreateRoomDto roomDto)
        {
            // Các kiểm tra business logic của bạn
            if (roomDto.MotelId <= 0)
            {
                throw new ArgumentException("ID nhà trọ không hợp lệ khi tạo phòng.", nameof(roomDto.MotelId));
            }
            if (string.IsNullOrWhiteSpace(roomDto.RoomName))
            {
                throw new ArgumentException("Tên phòng không được để trống.", nameof(roomDto.RoomName));
            }
            if (roomDto.RentalPrice <= 0)
            {
                throw new ArgumentException("Giá thuê phải lớn hơn 0.", nameof(roomDto.RentalPrice));
            }

            var room = new Room
            {
                MotelId = roomDto.MotelId,
                RoomName = roomDto.RoomName,
                RentalPrice = roomDto.RentalPrice,
                Area = roomDto.Area,
                RoomStatusId = roomDto.RoomStatusId,
                CreatedAt = DateTime.UtcNow
            };

            await _roomRepository.AddAsync(room);
            await _roomRepository.SaveChangesAsync();
        }

        public async Task UpdateRoomAsync(RoomDto roomDto)
        {
            // Các kiểm tra business logic của bạn
            if (roomDto.RoomId <= 0)
            {
                throw new ArgumentException("ID phòng không hợp lệ.", nameof(roomDto.RoomId));
            }
            if (string.IsNullOrWhiteSpace(roomDto.RoomName))
            {
                throw new ArgumentException("Tên phòng không được để trống.", nameof(roomDto.RoomName));
            }
            if (roomDto.RentalPrice <= 0)
            {
                throw new ArgumentException("Giá thuê phải lớn hơn 0.", nameof(roomDto.RentalPrice));
            }

            var room = await _roomRepository.GetByIdAsync(roomDto.RoomId);
            if (room == null)
            {
                throw new KeyNotFoundException($"Phòng với ID {roomDto.RoomId} không tìm thấy để cập nhật.");
            }

            room.MotelId = roomDto.MotelId;
            room.RoomName = roomDto.RoomName;
            room.RentalPrice = roomDto.RentalPrice;
            room.Area = roomDto.Area;
            room.RoomStatusId = roomDto.RoomStatusId;

            await _roomRepository.UpdateAsync(room);
            await _roomRepository.SaveChangesAsync();
        }

        public async Task DeleteRoomAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID phòng không hợp lệ.", nameof(id));
            }

            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
            {
                throw new KeyNotFoundException($"Phòng với ID {id} không tìm thấy để xóa.");
            }

            await _roomRepository.DeleteAsync(room);
            await _roomRepository.SaveChangesAsync();
        }

        public async Task<int> GetTotalRoomsCountAsync()
        {
            var allRooms = await _roomRepository.GetAllAsync();
            return allRooms.Count();
        }
    }
}