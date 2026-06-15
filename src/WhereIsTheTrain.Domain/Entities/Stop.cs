using NetTopologySuite.Geometries;
using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class Stop : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public Guid CityId { get; set; }
    public City City { get; set; } = null!;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public Point? Location { get; set; }

    // Navigation properties
    public ICollection<TrainRouteStop> TrainRouteStops { get; set; } = new List<TrainRouteStop>();
    public ICollection<RailwayPath> RailwayPaths { get; set; } = new List<RailwayPath>();
}
