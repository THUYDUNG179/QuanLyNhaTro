using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Thêm dòng này để sử dụng Data Annotations
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhaTro.Application.DTOs
{
    // DTO dùng để hiển thị và cập nhật thông tin phòng
    public class RoomDto
    {
        public int RoomId { get; set; }

        [Required(ErrorMessage = "ID nhà trọ không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID nhà trọ phải là một số dương.")]
        public int MotelId { get; set; }

        [Required(ErrorMessage = "Tên phòng không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên phòng không được vượt quá 100 ký tự.")]
        public string RoomName { get; set; }

        [Required(ErrorMessage = "Giá thuê không được để trống.")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Giá thuê phải lớn hơn 0.")]
        public decimal RentalPrice { get; set; }

        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Diện tích phải là số dương.")] // Diện tích có thể là 0 nếu không áp dụng
        public decimal? Area { get; set; } // int? cho phép null

        [Required(ErrorMessage = "Trạng thái phòng không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID trạng thái phòng không hợp lệ.")]
        public int RoomStatusId { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool HasContract { get; set; } // Thêm thuộc tính này để kiểm tra có hợp đồng nào liên quan đến phòng không
    }

    // DTO dùng để tạo phòng mới
    public class CreateRoomDto
    {
        [Required(ErrorMessage = "ID nhà trọ không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID nhà trọ phải là một số dương.")]
        public int MotelId { get; set; }

        [Required(ErrorMessage = "Tên phòng không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên phòng không được vượt quá 100 ký tự.")]
        public string RoomName { get; set; }

        [Required(ErrorMessage = "Giá thuê không được để trống.")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Giá thuê phải lớn hơn 0.")]
        public decimal RentalPrice { get; set; }

        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Diện tích phải là số dương.")]
        public decimal? Area { get; set; } // decimal? cho phép null

        [Required(ErrorMessage = "Trạng thái phòng không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID trạng thái phòng không hợp lệ.")]
        public int RoomStatusId { get; set; } = 1; // Mặc định là Vacant (ID 1)
    }
}
