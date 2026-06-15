using NetTopologySuite.Geometries;
using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class Train : AuditableEntity
{
    public string TrainNumber { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CreatedById { get; set; }
    public Guid? TrainTypeId { get; set; }

    // Navigation properties
    public User? CreatedBy { get; set; }
    public TrainType? TrainType { get; set; }
    public ICollection<TrainRouteStop> RouteStops { get; set; } = new List<TrainRouteStop>();
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
