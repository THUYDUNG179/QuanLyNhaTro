using System;
using System.Collections.Generic;

namespace NhaTro.Infrastructure;

public partial class Motel
{
    public int MotelId { get; set; }

    public string MotelName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? Description { get; set; }

    public int OwnerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
