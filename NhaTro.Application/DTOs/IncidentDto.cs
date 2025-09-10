using System;
using System.ComponentModel.DataAnnotations;

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

        // <<=== THUỘC TÍNH HIỂN THỊ MỚI
        public string RoomName { get; set; }
        public string MotelName { get; set; }
        public string TenantName { get; set; } // Tên người thuê
        public string OwnerName { get; set; }  // Tên chủ nhà (nếu Owner báo cáo)
        // ===>>
    }

    public class CreateIncidentDto
    {
        public int RoomId { get; set; }
        public int? TenantId { get; set; } // Có thể là null nếu Owner báo cáo
        [Required(ErrorMessage = "Tiêu đề không được để trống.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống.")]
        public string Description { get; set; }
        public string Priority { get; set; } = "Normal";
        public string AttachedImagePath { get; set; }
    }
}
