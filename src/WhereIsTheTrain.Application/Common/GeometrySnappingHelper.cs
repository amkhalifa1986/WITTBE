using NetTopologySuite.Geometries;

namespace WhereIsTheTrain.Application.Common;

public static class GeometrySnappingHelper
{
    public static double CalculateHaversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Earth radius in meters
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double val) => (Math.PI / 180) * val;

    public static (Coordinate Snapped, double DistanceAlongLine, double DistanceToLine) ProjectPointOntoPolyline(LineString polyline, Coordinate point)
    {
        var coords = polyline.Coordinates;
        if (coords.Length == 0)
            return (point, 0, 0);

        Coordinate bestSnapped = coords[0];
        double minPerpDistance = double.MaxValue;
        double bestDistanceAlong = 0;
        
        double cumulativeDistance = 0;

        for (int i = 0; i < coords.Length - 1; i++)
        {
            var A = coords[i];
            var B = coords[i + 1];

            // Segment length in meters
            double segLength = CalculateHaversine(A.Y, A.X, B.Y, B.X);

            // Project using local Cartesian coordinate approximation
            double dx = B.X - A.X;
            double dy = B.Y - A.Y;
            double t = 0;
            
            double denom = dx * dx + dy * dy;
            if (denom > 0)
            {
                t = ((point.X - A.X) * dx + (point.Y - A.Y) * dy) / denom;
                t = Math.Clamp(t, 0.0, 1.0);
            }

            // Snapped coordinate
            double snapX = A.X + t * dx;
            double snapY = A.Y + t * dy;
            var S = new Coordinate(snapX, snapY);

            // Distance from raw point to snapped point
            double perpDist = CalculateHaversine(point.Y, point.X, S.Y, S.X);

            if (perpDist < minPerpDistance)
            {
                minPerpDistance = perpDist;
                bestSnapped = S;
                double distFromAToS = CalculateHaversine(A.Y, A.X, S.Y, S.X);
                bestDistanceAlong = cumulativeDistance + distFromAToS;
            }

            cumulativeDistance += segLength;
        }

        return (bestSnapped, bestDistanceAlong, minPerpDistance);
    }
}
