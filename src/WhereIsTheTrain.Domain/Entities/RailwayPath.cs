using NetTopologySuite.Geometries;
using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class RailwayPath : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;

    public Guid StartStationId { get; set; }
    public Stop StartStation { get; set; } = null!;

    public Guid EndStationId { get; set; }
    public Stop EndStation { get; set; } = null!;

    public LineString RoutePath { get; set; } = null!;

    // Navigation properties
    public ICollection<Stop> Stops { get; set; } = new List<Stop>();
}
