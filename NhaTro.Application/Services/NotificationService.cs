using NhaTro.Application.DTOs;
using NhaTro.Application.Interfaces;
using NhaTro.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NhaTro.Infrastructure;

namespace NhaTro.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IRepository<Notification> _notificationRepository;
        private readonly IRepository<Motel> _motelRepository;
        private readonly IRepository<Room> _roomRepository;

        public NotificationService(
            IRepository<Notification> notificationRepository,
            IRepository<Motel> motelRepository,
            IRepository<Room> roomRepository)
        {
            _notificationRepository = notificationRepository;
            _motelRepository = motelRepository;
            _roomRepository = roomRepository;
        }

        public async Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync()
        {
            var notifications = await _notificationRepository.GetAllAsync();
            return notifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                SenderUserId = n.SenderUserId,
                ReceiverUserId = n.ReceiverUserId,
                Title = n.Title,
                Content = n.Content,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt,
                RelatedMotelId = n.RelatedMotelId,
                RelatedRoomId = n.RelatedRoomId,
                RelatedContractId = n.RelatedContractId
            }).ToList();
        }

        public async Task<NotificationDto> GetByIdAsync(int notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null) return null;
            return new NotificationDto
            {
                NotificationId = notification.NotificationId,
                SenderUserId = notification.SenderUserId,
                ReceiverUserId = notification.ReceiverUserId,
                Title = notification.Title,
                Content = notification.Content,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                RelatedMotelId = notification.RelatedMotelId,
                RelatedRoomId = notification.RelatedRoomId,
                RelatedContractId = notification.RelatedContractId
            };
        }

        public async Task AddNotificationAsync(CreateNotificationDto notificationDto)
        {
            var notification = new Notification
            {
                SenderUserId = notificationDto.SenderUserId,
                ReceiverUserId = notificationDto.ReceiverUserId,
                Title = notificationDto.Title,
                Content = notificationDto.Content,
                Type = notificationDto.Type,
                RelatedMotelId = notificationDto.RelatedMotelId,
                RelatedRoomId = notificationDto.RelatedRoomId,
                RelatedContractId = notificationDto.RelatedContractId,
                CreatedAt = DateTime.Now,
                IsRead = false
            };
            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task UpdateNotificationAsync(NotificationDto notificationDto)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationDto.NotificationId);
            if (notification == null)
            {
                throw new KeyNotFoundException($"Thông báo với ID {notificationDto.NotificationId} không tìm thấy.");
            }

            notification.Title = notificationDto.Title;
            notification.Content = notificationDto.Content;
            notification.IsRead = notificationDto.IsRead;
            notification.ReadAt = notificationDto.ReadAt;

            await _notificationRepository.UpdateAsync(notification);
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
            {
                throw new KeyNotFoundException($"Thông báo với ID {notificationId} không tìm thấy.");
            }
            await _notificationRepository.DeleteAsync(notification);
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationsForOwnerAsync(int ownerId, int? motelId, int? roomId, bool? unreadOnly)
        {
            var allNotifications = await _notificationRepository.GetAllAsync();
            var allMotels = await _motelRepository.GetAllAsync();
            var allRooms = await _roomRepository.GetAllAsync();

            var ownerMotelIds = allMotels.Where(m => m.OwnerId == ownerId).Select(m => m.MotelId).ToList();

            var filteredNotifications = allNotifications.AsQueryable();

            filteredNotifications = filteredNotifications.Where(n =>
                n.SenderUserId == ownerId ||
                n.ReceiverUserId == ownerId ||
                (n.RelatedMotelId.HasValue && ownerMotelIds.Contains(n.RelatedMotelId.Value)) ||
                (n.RelatedRoomId.HasValue && allRooms.Any(r => r.RoomId == n.RelatedRoomId.Value && ownerMotelIds.Contains(r.MotelId))));

            if (motelId.HasValue && motelId.Value > 0)
            {
                var roomIdsInMotel = allRooms.Where(r => r.MotelId == motelId.Value).Select(r => r.RoomId).ToList();
                filteredNotifications = filteredNotifications.Where(n =>
                    n.RelatedMotelId == motelId.Value ||
                    (n.RelatedRoomId.HasValue && roomIdsInMotel.Contains(n.RelatedRoomId.Value)));
            }
            else if (roomId.HasValue && roomId.Value > 0)
            {
                filteredNotifications = filteredNotifications.Where(n => n.RelatedRoomId == roomId.Value);
            }

            if (unreadOnly == true)
            {
                filteredNotifications = filteredNotifications.Where(n => !n.IsRead);
            }

            return filteredNotifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                SenderUserId = n.SenderUserId,
                ReceiverUserId = n.ReceiverUserId,
                Title = n.Title,
                Content = n.Content,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt,
                RelatedMotelId = n.RelatedMotelId,
                RelatedRoomId = n.RelatedRoomId,
                RelatedContractId = n.RelatedContractId
            }).ToList();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                await _notificationRepository.UpdateAsync(notification);
                await _notificationRepository.SaveChangesAsync();
            }
        }
    }
}