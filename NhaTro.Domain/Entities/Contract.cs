using System;
using System.Collections.Generic;

namespace NhaTro.Infrastructure;

public partial class Contract
{
    public int ContractId { get; set; }

    public int RoomId { get; set; }

    public int TenantId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal? DepositAmount { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public string? FileContractPath { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Room Room { get; set; } = null!;

    public virtual User Tenant { get; set; } = null!;
}
