// NhaTro.Application.Interfaces/INotificationService.cs

using NhaTro.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NhaTro.Application.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync();
        Task<NotificationDto> GetByIdAsync(int notificationId);
        Task AddNotificationAsync(CreateNotificationDto notificationDto);
        Task UpdateNotificationAsync(NotificationDto notificationDto);
        Task DeleteNotificationAsync(int notificationId);

        Task<IEnumerable<NotificationDto>> GetNotificationsForOwnerAsync(int ownerId, int? motelId, int? roomId, bool? unreadOnly);
        Task MarkAsReadAsync(int notificationId);
    }
}