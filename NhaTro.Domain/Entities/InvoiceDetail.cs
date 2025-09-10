using System;
using System.Collections.Generic;

namespace NhaTro.Infrastructure;

public partial class InvoiceDetail
{
    public int InvoiceDetailId { get; set; }

    public int InvoiceId { get; set; }

    public string Description { get; set; } = null!;

    public decimal Amount { get; set; }

    public int? UtilityReadingId { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual UtilityReading? UtilityReading { get; set; }
}
