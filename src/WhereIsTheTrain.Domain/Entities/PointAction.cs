using System.Collections.Generic;

namespace WhereIsTheTrain.Domain.Entities;

public class PointAction : Common.BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public int Points { get; set; } = 0;

    public ICollection<UserPointLog> PointLogs { get; set; } = new List<UserPointLog>();
}
