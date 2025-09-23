// NhaTro.Application.DTOs/NotificationDto.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace NhaTro.Application.DTOs
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int SenderUserId { get; set; }
        public int ReceiverUserId { get; set; }
        public int? RelatedMotelId { get; set; }
        public int? RelatedRoomId { get; set; }
        public int? RelatedContractId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class CreateNotificationDto
    {

        public int ReceiverUserId { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống.")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống.")]
        public string Content { get; set; }

        public int? RelatedMotelId { get; set; }
        public int? RelatedRoomId { get; set; }
        public int? RelatedContractId { get; set; }

        public int SenderUserId { get; set; }
        public string Type { get; set; } = "General";
    }
}