using System;
using System.Collections.Generic;
using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class TrainType : AuditableEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? MarkerPngUrl { get; set; }

    // Navigation properties
    public ICollection<Train> Trains { get; set; } = new List<Train>();
}
