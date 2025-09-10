using System;
using System.Collections.Generic;

namespace NhaTro.Infrastructure;

public partial class Utility
{
    public int UtilityId { get; set; }

    public string UtilityName { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public virtual ICollection<UtilityReading> UtilityReadings { get; set; } = new List<UtilityReading>();
}
