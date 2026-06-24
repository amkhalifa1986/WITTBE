using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class GenderLookup : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
}
