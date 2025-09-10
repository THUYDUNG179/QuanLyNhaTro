using System;
using System.Collections.Generic;

namespace NhaTro.Infrastructure;

public partial class UtilityReading
{
    public int ReadingId { get; set; }

    public int RoomId { get; set; }

    public int UtilityId { get; set; }

    public decimal ReadingValue { get; set; }

    public DateOnly ReadingDate { get; set; }

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

    public virtual Room Room { get; set; } = null!;

    public virtual Utility Utility { get; set; } = null!;
}
