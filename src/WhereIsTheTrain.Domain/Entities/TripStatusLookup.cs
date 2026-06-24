using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class TripStatusLookup : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public static class TripStatuses
{
    public static readonly Guid Scheduled = Guid.Parse("00000000-0000-0000-0000-000000004001");
    public static readonly Guid Departed = Guid.Parse("00000000-0000-0000-0000-000000004002");
    public static readonly Guid InTransit = Guid.Parse("00000000-0000-0000-0000-000000004003");
    public static readonly Guid Arrived = Guid.Parse("00000000-0000-0000-0000-000000004004");
    public static readonly Guid Cancelled = Guid.Parse("00000000-0000-0000-0000-000000004005");
    public static readonly Guid Delayed = Guid.Parse("00000000-0000-0000-0000-000000004006");
}
