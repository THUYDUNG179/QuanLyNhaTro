using System;
using System.Collections.Generic;

namespace NhaTro.Infrastructure;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int ContractId { get; set; }

    public string BillingPeriod { get; set; } = null!;

    public DateOnly DueDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Contract Contract { get; set; } = null!;

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
