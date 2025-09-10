namespace NhaTro.Application.DTOs
{
    public class InvoiceDetailDto
    {
        public int InvoiceDetailId { get; set; }
        public int InvoiceId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int? UtilityReadingId { get; set; }
    }

    public class CreateInvoiceDetailDto
    {
        public int InvoiceId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int? UtilityReadingId { get; set; }
    }
}