using System;
using System.Collections.Generic;

namespace NhaTro.Infrastructure;

public partial class Incident
{
    public int IncidentId { get; set; }

    public int RoomId { get; set; }

    public int TenantId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? AttachedImagePath { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual User Tenant { get; set; } = null!;
}
