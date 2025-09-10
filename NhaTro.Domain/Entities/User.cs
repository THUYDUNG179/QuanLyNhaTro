using System;
using System.Collections.Generic;

namespace NhaTro.Infrastructure;

public partial class User
{
    public object UserName;

    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Phone { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<Incident> Incidents { get; set; } = new List<Incident>();

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public virtual ICollection<Motel> Motels { get; set; } = new List<Motel>();

    public virtual ICollection<Notification> NotificationReceiverUsers { get; set; } = new List<Notification>();

    public virtual ICollection<Notification> NotificationSenderUsers { get; set; } = new List<Notification>();

    public virtual Role Role { get; set; } = null!;
}
