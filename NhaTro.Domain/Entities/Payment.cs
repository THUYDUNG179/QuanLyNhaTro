using System;
using System.Collections.Generic;

namespace NhaTro.Infrastructure;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int InvoiceId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? Method { get; set; }

    public string Status { get; set; } = null!;

    public virtual Invoice Invoice { get; set; } = null!;
}
