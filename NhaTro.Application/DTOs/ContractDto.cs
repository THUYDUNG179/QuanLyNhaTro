using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace NhaTro.Application.DTOs
{
    // DTO dùng để hiển thị thông tin hợp đồng
    // Hoặc dùng để cập nhật hợp đồng (Update)
    public class ContractDto
    {
        public int ContractId { get; set; }

        [Required(ErrorMessage = "ID Phòng không được để trống.")]
        public int RoomId { get; set; }
        public string RoomName { get; set; }

        [Required(ErrorMessage = "ID Người thuê không được để trống.")]
        public int TenantId { get; set; }
        public string TenantName { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống.")]
        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? EndDate { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Số tiền đặt cọc phải là số dương hoặc 0.")]
        public decimal DepositAmount { get; set; }

        [StringLength(50, ErrorMessage = "Trạng thái không được vượt quá 50 ký tự.")]
        public string Status { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string Notes { get; set; }
        
        [StringLength(255, ErrorMessage = "Đường dẫn file hợp đồng không được vượt quá 255 ký tự.")]
        public string FileContractPath { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    // DTO dùng để tạo hợp đồng mới
    public class CreateContractDto
    {
        [Required(ErrorMessage = "Vui lòng chọn Phòng.")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Người thuê.")]
        public int TenantId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Ngày bắt đầu.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly? EndDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền cọc.")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Số tiền đặt cọc phải là số dương hoặc 0.")]
        public decimal DepositAmount { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string Notes { get; set; }

        public IFormFile FileContract { get; set; }
    }

        public class UpdateContractDto
        {
            [Required]
            public int ContractId { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn Phòng.")]
            public int RoomId { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn Người thuê.")]
            public int TenantId { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập Ngày bắt đầu.")]
            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            public DateOnly StartDate { get; set; }

            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            public DateOnly? EndDate { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập số tiền cọc.")]
            [Range(0, (double)decimal.MaxValue, ErrorMessage = "Số tiền đặt cọc phải là số dương hoặc 0.")]
            public decimal DepositAmount { get; set; }

            [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
            public string Notes { get; set; }

            // File hợp đồng mới được tải lên
            public IFormFile FileContract { get; set; }

            // Đường dẫn file cũ để giữ lại nếu không có file mới
            public string FileContractPath { get; set; }

            [Required]
            public DateTime CreatedAt { get; set; }
        }
    
}