using System;

namespace NhaTro.Application.DTOs
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } // Thay DateTime bằng DateOnly
        public string Method { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } // THÊM CreatedAt
        // Notes không còn trong DTO

        // <<=== THUỘC TÍNH HIỂN THỊ MỚI
        public string InvoiceBillingPeriod { get; set; } // Kỳ thanh toán của hóa đơn
        public string ContractCode { get; set; } // Mã hợp đồng
        public string RoomName { get; set; }
        public string MotelName { get; set; }
        public string TenantName { get; set; }
        // ===>>
    }

    public class CreatePaymentDto
    {
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public DateOnly PaymentDate { get; set; } // THAY DateTime bằng DateOnly
        public string Method { get; set; }
        // Status và Notes không có trong CreateDto, sẽ được xử lý ở backend/service
    }
}
