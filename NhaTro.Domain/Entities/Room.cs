using System;
using System.Collections.Generic;

namespace NhaTro.Infrastructure;

public partial class Room
{
    public int RoomId { get; set; }

    public int MotelId { get; set; }

    public string RoomName { get; set; } = null!;

    public decimal RentalPrice { get; set; }

    public decimal? Area { get; set; }

    public int RoomStatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<Incident> Incidents { get; set; } = new List<Incident>();

    public virtual Motel Motel { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual RoomStatus RoomStatus { get; set; } = null!;

    public virtual ICollection<UtilityReading> UtilityReadings { get; set; } = new List<UtilityReading>();
}
