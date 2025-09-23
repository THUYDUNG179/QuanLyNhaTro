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
    public class UtilityReadingService : IUtilityReadingService
    {
        private readonly IRepository<UtilityReading> _utilityReadingRepository;
        private readonly IRepository<Room> _roomRepository; // Giả sử bạn có cái này
        private readonly IRepository<Motel> _motelRepository;

        public UtilityReadingService(IRepository<UtilityReading> utilityReadingRepository, IRepository<Room> roomRepository,
        IRepository<Motel> motelRepository)
        {
            _utilityReadingRepository = utilityReadingRepository;
            _roomRepository = roomRepository;
            _motelRepository = motelRepository;
        }

        public async Task<IEnumerable<UtilityReadingDto>> GetAllUtilityReadingsAsync()
        {
            var utilityReadings = await _utilityReadingRepository.GetAllAsync();
            var rooms = (await _roomRepository.GetAllAsync()).ToList();
            var motels = (await _motelRepository.GetAllAsync()).ToList();

            return utilityReadings.Select(ur =>
            {
                var room = rooms.FirstOrDefault(r => r.RoomId == ur.RoomId);
                var motel = room != null ? motels.FirstOrDefault(m => m.MotelId == room.MotelId) : null;

                return new UtilityReadingDto
                {
                    ReadingId = ur.ReadingId,
                    RoomId = ur.RoomId,
                    UtilityId = ur.UtilityId,
                    ReadingValue = ur.ReadingValue,
                    ReadingDate = ur.ReadingDate,
                    RoomName = room?.RoomName,
                    MotelName = motel?.MotelName
                };
            }).ToList();
        }

        public async Task<UtilityReadingDto> GetByIdAsync(int readingId)
        {
            var utilityReading = await _utilityReadingRepository.GetByIdAsync(readingId);
            if (utilityReading == null)
                throw new KeyNotFoundException($"Chỉ số tiện ích với ID {readingId} không tìm thấy.");

            return new UtilityReadingDto
            {
                ReadingId = utilityReading.ReadingId,
                RoomId = utilityReading.RoomId,
                UtilityId = utilityReading.UtilityId,
                ReadingValue = utilityReading.ReadingValue,
                ReadingDate = utilityReading.ReadingDate
            };
        }

        public async Task AddUtilityReadingAsync(CreateUtilityReadingDto utilityReadingDto)
        {
            var utilityReadings = await _utilityReadingRepository.GetAllAsync();
            if (utilityReadings.Any(ur => ur.RoomId == utilityReadingDto.RoomId
                && ur.UtilityId == utilityReadingDto.UtilityId
                && ur.ReadingDate == utilityReadingDto.ReadingDate))
            {
                throw new InvalidOperationException("Chỉ số tiện ích cho phòng, tiện ích và ngày này đã tồn tại.");
            }

            var utilityReading = new UtilityReading
            {
                RoomId = utilityReadingDto.RoomId,
                UtilityId = utilityReadingDto.UtilityId,
                ReadingValue = utilityReadingDto.ReadingValue,
                ReadingDate = utilityReadingDto.ReadingDate
            };

            await _utilityReadingRepository.AddAsync(utilityReading);
            await _utilityReadingRepository.SaveChangesAsync();
        }

        public async Task UpdateUtilityReadingAsync(UtilityReadingDto utilityReadingDto)
        {
            var utilityReading = await _utilityReadingRepository.GetByIdAsync(utilityReadingDto.ReadingId);
            if (utilityReading == null)
                throw new KeyNotFoundException($"Chỉ số tiện ích với ID {utilityReadingDto.ReadingId} không tìm thấy.");

            var utilityReadings = await _utilityReadingRepository.GetAllAsync();
            if (utilityReadings.Any(ur => ur.RoomId == utilityReadingDto.RoomId
                && ur.UtilityId == utilityReadingDto.UtilityId
                && ur.ReadingDate == utilityReadingDto.ReadingDate
                && ur.ReadingId != utilityReadingDto.ReadingId))
            {
                throw new InvalidOperationException("Chỉ số tiện ích cho phòng, tiện ích và ngày này đã tồn tại.");
            }

            utilityReading.RoomId = utilityReadingDto.RoomId;
            utilityReading.UtilityId = utilityReadingDto.UtilityId;
            utilityReading.ReadingValue = utilityReadingDto.ReadingValue;
            utilityReading.ReadingDate = utilityReadingDto.ReadingDate;

            await _utilityReadingRepository.UpdateAsync(utilityReading);
            await _utilityReadingRepository.SaveChangesAsync();
        }

        public async Task DeleteUtilityReadingAsync(int readingId)
        {
            var utilityReading = await _utilityReadingRepository.GetByIdAsync(readingId);
            if (utilityReading == null)
                throw new KeyNotFoundException($"Chỉ số tiện ích với ID {readingId} không tìm thấy.");

            await _utilityReadingRepository.DeleteAsync(utilityReading);
            await _utilityReadingRepository.SaveChangesAsync();
        }
    }
}