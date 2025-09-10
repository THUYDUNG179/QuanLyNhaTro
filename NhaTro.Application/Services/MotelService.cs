using NhaTro.Application.DTOs;
using NhaTro.Application.Interfaces;
using NhaTro.Domain.Interfaces;
using NhaTro.Infrastructure; // Đảm bảo đã có using cho Motel
using System;
using System.Collections.Generic;
using System.Linq; // Thêm dòng này cho .Select()
using System.Threading.Tasks;

namespace NhaTro.Application.Services
{
    public class MotelService : IMotelService
    {
        private readonly IRepository<Motel> _motelRepository;

        public MotelService(IRepository<Motel> motelRepository)
        {
            _motelRepository = motelRepository;
        }

        public async Task<IEnumerable<MotelDto>> GetAllMotelsAsync()
        {
            var motels = await _motelRepository.GetAllAsync();
            return motels.Select(m => new MotelDto
            {
                MotelId = m.MotelId,
                MotelName = m.MotelName,
                Address = m.Address,
                Description = m.Description,
                OwnerId = m.OwnerId,
                CreatedAt = m.CreatedAt
            }).ToList();
        }

        public async Task<MotelDto> GetByIdAsync(int id)
        {
            if (id <= 0) // Kiểm tra ID không hợp lệ ngay từ Service
            {
                throw new ArgumentException("ID nhà trọ không hợp lệ.", nameof(id));
            }
            var motel = await _motelRepository.GetByIdAsync(id);
            if (motel == null)
            {
                return null; // THAY ĐỔI: Trả về NULL nếu không tìm thấy
            }

            return new MotelDto
            {
                MotelId = motel.MotelId,
                MotelName = motel.MotelName,
                Address = motel.Address,
                Description = motel.Description,
                OwnerId = motel.OwnerId,
                CreatedAt = motel.CreatedAt
            };
        }

        public async Task AddMotelAsync(CreateMotelDto motelDto)
        {
            // Thêm validation cơ bản nếu cần
            if (string.IsNullOrWhiteSpace(motelDto.MotelName))
            {
                throw new ArgumentException("Tên nhà trọ không được để trống.", nameof(motelDto.MotelName));
            }
            if (string.IsNullOrWhiteSpace(motelDto.Address))
            {
                throw new ArgumentException("Địa chỉ nhà trọ không được để trống.", nameof(motelDto.Address));
            }
            if (motelDto.OwnerId <= 0)
            {
                throw new ArgumentException("Owner ID không hợp lệ.", nameof(motelDto.OwnerId));
            }

            var motel = new Motel
            {
                MotelName = motelDto.MotelName,
                Address = motelDto.Address,
                Description = motelDto.Description,
                OwnerId = motelDto.OwnerId,
                CreatedAt = DateTime.UtcNow
            };

            await _motelRepository.AddAsync(motel);
            await _motelRepository.SaveChangesAsync();
        }

        public async Task UpdateMotelAsync(MotelDto motelDto)
        {
            if (motelDto.MotelId <= 0) // Kiểm tra ID không hợp lệ
            {
                throw new ArgumentException("ID nhà trọ không hợp lệ.", nameof(motelDto.MotelId));
            }
            // Thêm validation cơ bản nếu cần
            if (string.IsNullOrWhiteSpace(motelDto.MotelName))
            {
                throw new ArgumentException("Tên nhà trọ không được để trống.", nameof(motelDto.MotelName));
            }
            if (string.IsNullOrWhiteSpace(motelDto.Address))
            {
                throw new ArgumentException("Địa chỉ nhà trọ không được để trống.", nameof(motelDto.Address));
            }
            if (motelDto.OwnerId <= 0)
            {
                throw new ArgumentException("Owner ID không hợp lệ.", nameof(motelDto.OwnerId));
            }

            var motel = await _motelRepository.GetByIdAsync(motelDto.MotelId);
            if (motel == null)
            {
                // Vẫn ném exception cho Update vì không thể cập nhật đối tượng không tồn tại
                throw new KeyNotFoundException($"Nhà trọ với ID {motelDto.MotelId} không tìm thấy để cập nhật.");
            }

            motel.MotelName = motelDto.MotelName;
            motel.Address = motelDto.Address;
            motel.Description = motelDto.Description;
            motel.OwnerId = motelDto.OwnerId;

            await _motelRepository.UpdateAsync(motel);
            await _motelRepository.SaveChangesAsync();
        }

        public async Task DeleteMotelAsync(int id)
        {
            if (id <= 0) // Kiểm tra ID không hợp lệ
            {
                throw new ArgumentException("ID nhà trọ không hợp lệ.", nameof(id));
            }

            var motel = await _motelRepository.GetByIdAsync(id);
            if (motel == null)
            {
                // Vẫn ném exception cho Delete vì không thể xóa đối tượng không tồn tại
                throw new KeyNotFoundException($"Nhà trọ với ID {id} không tìm thấy để xóa.");
            }

            await _motelRepository.DeleteAsync(motel);
            await _motelRepository.SaveChangesAsync();
        }
        public async Task<IEnumerable<MotelDto>> GetMotelsByOwnerIdAsync(int ownerId)
        {
            var motels = (await _motelRepository.GetAllAsync())
                .Where(m => m.OwnerId == ownerId)
                .Select(m => new MotelDto
                {
                    MotelId = m.MotelId,
                    MotelName = m.MotelName,
                    // ... các thuộc tính khác
                }).ToList();

            return motels;
        }
        public async Task<int> GetTotalMotelsCountAsync()
        {
            var allMotels = await _motelRepository.GetAllAsync();
            return allMotels.Count();
        }
    }
}
