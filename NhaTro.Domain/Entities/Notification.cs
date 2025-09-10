using System;
using System.Collections.Generic;

namespace NhaTro.Infrastructure;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int SenderUserId { get; set; }

    public int ReceiverUserId { get; set; }

    public int? RelatedMotelId { get; set; }

    public int? RelatedRoomId { get; set; }

    public int? RelatedContractId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string Type { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual User ReceiverUser { get; set; } = null!;

    public virtual Contract? RelatedContract { get; set; }

    public virtual Motel? RelatedMotel { get; set; }

    public virtual Room? RelatedRoom { get; set; }

    public virtual User SenderUser { get; set; } = null!;
}
