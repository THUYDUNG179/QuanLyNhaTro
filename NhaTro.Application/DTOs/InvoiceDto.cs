using System;

namespace NhaTro.Application.DTOs
{
    public class InvoiceDto
    {
        public int InvoiceId { get; set; }
        public int ContractId { get; set; }
        public string BillingPeriod { get; set; } // Giữ string cho YYYY-MM
        public DateOnly DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // <<=== THUỘC TÍNH HIỂN THỊ MỚI
        public string ContractCode { get; set; } // Nếu có mã hợp đồng riêng
        public string RoomName { get; set; }
        public string MotelName { get; set; }
        public string TenantName { get; set; }
        // ===>>
    }

    public class CreateInvoiceDto
    {
        public int ContractId { get; set; }
        public string BillingPeriod { get; set; }
        public DateOnly DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Notes { get; set; }
        // Status không có trong CreateDto, sẽ được xử lý ở backend/service
    }
}
