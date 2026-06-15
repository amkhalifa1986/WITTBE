using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class City : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;

    public Guid GovernorateId { get; set; }
    public Governorate Governorate { get; set; } = null!;

    // Navigation property
    public ICollection<Stop> Stops { get; set; } = new List<Stop>();
}
