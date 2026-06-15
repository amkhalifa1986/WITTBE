using NetTopologySuite.Geometries;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Application.Common;

public static class RoutePathBuilder
{
    public static LineString BuildRoutePath(
        IEnumerable<TrainRouteStop> routeStops,
        IEnumerable<RailwayPath> railwayPaths)
    {
        var orderedStops = routeStops.OrderBy(rs => rs.StopOrder).ToList();
        
        if (orderedStops.Count == 0)
        {
            return GeometryFactory.Default.CreateLineString(Array.Empty<Coordinate>());
        }

        if (orderedStops.Count == 1)
        {
            var stop = orderedStops[0].Stop;
            var coord = new Coordinate(stop.Longitude, stop.Latitude);
            var singleCoords = new[] { coord, new Coordinate(coord.X, coord.Y) };
            orderedStops[0].DistanceAlongRoute = 0;
            return GeometryFactory.Default.CreateLineString(singleCoords);
        }

        var pathsList = railwayPaths.ToList();
        var stitchedCoords = new List<Coordinate>();

        for (int i = 0; i < orderedStops.Count - 1; i++)
        {
            var s1 = orderedStops[i].Stop;
            var s2 = orderedStops[i + 1].Stop;

            // Find a path that contains both s1 and s2
            var path = pathsList.FirstOrDefault(p => IsStopOnPath(s1, p) && IsStopOnPath(s2, p));
            if (path != null && path.RoutePath != null)
            {
                var coords = path.RoutePath.Coordinates;
                var tempLine = GeometryFactory.Default.CreateLineString(coords);

                var s1Coord = new Coordinate(s1.Longitude, s1.Latitude);
                var s2Coord = new Coordinate(s2.Longitude, s2.Latitude);

                var (snapped1, _, _) = GeometrySnappingHelper.ProjectPointOntoPolyline(tempLine, s1Coord);
                var (snapped2, _, _) = GeometrySnappingHelper.ProjectPointOntoPolyline(tempLine, s2Coord);

                int idx1 = FindSegmentIndex(coords, snapped1);
                int idx2 = FindSegmentIndex(coords, snapped2);

                if (stitchedCoords.Count == 0) AddCoordinate(stitchedCoords, snapped1);
                else AddCoordinate(stitchedCoords, snapped1); // Will be deduplicated

                if (idx1 <= idx2)
                {
                    for (int j = idx1 + 1; j <= idx2; j++) AddCoordinate(stitchedCoords, coords[j]);
                }
                else
                {
                    for (int j = idx1; j >= idx2 + 1; j--) AddCoordinate(stitchedCoords, coords[j]);
                }

                AddCoordinate(stitchedCoords, snapped2);
            }
        }

        if (stitchedCoords.Count < 2)
        {
            foreach (var stop in orderedStops) stop.DistanceAlongRoute = 0;
            return GeometryFactory.Default.CreateLineString(Array.Empty<Coordinate>());
        }

        var lineString = GeometryFactory.Default.CreateLineString(stitchedCoords.ToArray());

        // For each stop, project onto the polyline to get accurate distance
        for (int i = 0; i < orderedStops.Count; i++)
        {
            var stop = orderedStops[i];
            var stopCoord = new Coordinate(stop.Stop.Longitude, stop.Stop.Latitude);
            var (_, distAlong, _) = GeometrySnappingHelper.ProjectPointOntoPolyline(lineString, stopCoord);
            stop.DistanceAlongRoute = distAlong;
        }

        return lineString;
    }

    public static List<RailwayPath> GetCoveringPaths(
        IEnumerable<TrainRouteStop> routeStops,
        IEnumerable<RailwayPath> railwayPaths)
    {
        var orderedStops = routeStops.OrderBy(rs => rs.StopOrder).Select(rs => rs.Stop).ToList();
        if (orderedStops.Count <= 1)
        {
            return new List<RailwayPath>();
        }

        var pathsList = railwayPaths.ToList();
        var result = new HashSet<RailwayPath>();

        for (int i = 0; i < orderedStops.Count - 1; i++)
        {
            var s1 = orderedStops[i];
            var s2 = orderedStops[i + 1];

            // Prioritize paths that have both
            var path = pathsList.FirstOrDefault(p => IsStopOnPath(s1, p) && IsStopOnPath(s2, p));
            if (path != null)
            {
                result.Add(path);
            }
        }

        return result.ToList();
    }

    private static bool IsStopOnPath(Stop stop, RailwayPath path)
    {
        if (path.StartStationId == stop.Id || path.EndStationId == stop.Id)
            return true;

        if (path.Stops != null && path.Stops.Any(s => s.Id == stop.Id))
            return true;

        if (path.RoutePath != null)
            return IsStopNearPath(stop, path.RoutePath);

        return false;
    }

    private static bool IsStopNearPath(Stop stop, LineString pathGeometry)
    {
        var stopCoord = new Coordinate(stop.Longitude, stop.Latitude);
        var (_, _, distToLine) = GeometrySnappingHelper.ProjectPointOntoPolyline(pathGeometry, stopCoord);
        return distToLine < 5000; // 5km tolerance
    }

    private static int FindSegmentIndex(Coordinate[] coords, Coordinate point)
    {
        double minDist = double.MaxValue;
        int bestIdx = 0;

        for (int i = 0; i < coords.Length - 1; i++)
        {
            var A = coords[i];
            var B = coords[i + 1];

            double dx = B.X - A.X;
            double dy = B.Y - A.Y;
            double denom = dx * dx + dy * dy;
            double t = 0;
            if (denom > 0)
            {
                t = ((point.X - A.X) * dx + (point.Y - A.Y) * dy) / denom;
                t = Math.Clamp(t, 0.0, 1.0);
            }

            double sx = A.X + t * dx;
            double sy = A.Y + t * dy;

            double dist = Math.Sqrt((point.X - sx) * (point.X - sx) + (point.Y - sy) * (point.Y - sy));
            if (dist < minDist)
            {
                minDist = dist;
                bestIdx = i;
            }
        }

        return bestIdx;
    }

    private static void AddCoordinate(List<Coordinate> list, Coordinate coord)
    {
        if (list.Count > 0)
        {
            var last = list[^1];
            if (Math.Abs(last.X - coord.X) < 1e-9 && Math.Abs(last.Y - coord.Y) < 1e-9)
            {
                return;
            }
        }
        list.Add(new Coordinate(coord.X, coord.Y));
    }
}
