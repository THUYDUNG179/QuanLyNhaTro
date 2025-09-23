using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NhaTro.Application.DTOs
{
    public class IncidentDto
    {
        public int IncidentId { get; set; }
        public int RoomId { get; set; }
        public int? TenantId { get; set; } // Có thể là null nếu Owner báo cáo
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string AttachedImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public string RoomName { get; set; }
        public string MotelName { get; set; }
        public string TenantName { get; set; } // Tên người thuê
        public string OwnerName { get; set; }  // Tên chủ nhà (nếu Owner báo cáo)
        // ===>>
    }

    public class CreateIncidentDto
    {
        public int RoomId { get; set; }
        public int? TenantId { get; set; }
        [Required(ErrorMessage = "Tiêu đề không được để trống.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống.")]
        public string Description { get; set; }
        public string Priority { get; set; } = "Normal";
        public IFormFile? AttachedImageFile { get; set; }

        // --- Bổ sung dòng này để lưu đường dẫn ảnh ---
        public string? AttachedImagePath { get; set; }
    }
    public class UpdateIncidentDto
    {
        public int IncidentId { get; set; }
        [Required(ErrorMessage = "Tiêu đề không được để trống.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống.")]
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }

        // This is for the user to upload a NEW file
        public IFormFile? AttachedImageFile { get; set; }

        // This is to keep track of the existing file path
        public string? AttachedImagePath { get; set; }

        public int RoomId { get; set; }
        public int? TenantId { get; set; }
    }
}
