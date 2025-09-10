using System;
using System.Collections.Generic;

namespace NhaTro.Infrastructure;

public partial class RoomStatus
{
    public int RoomStatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
