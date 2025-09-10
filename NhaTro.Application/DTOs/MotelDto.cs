using System;
using System.ComponentModel.DataAnnotations; // Thêm dòng này

namespace NhaTro.Application.DTOs
{
    public class MotelDto
    {
        public int MotelId { get; set; }
        public string MotelName { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public int OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateMotelDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên nhà trọ.")]
        [StringLength(200, ErrorMessage = "Tên nhà trọ không được vượt quá 200 ký tự.")]
        public string MotelName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
        public string Address { get; set; }

        public string Description { get; set; } // Description thường không bắt buộc

        // OwnerId sẽ được gán từ Controller, không cần [Required] ở đây.
        // Nếu bạn không gán nó trong Controller, nó sẽ mặc định là 0.
        public int OwnerId { get; set; }
    }
}
