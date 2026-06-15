using System;
using NetTopologySuite.Geometries;

namespace WhereIsTheTrain.Application.Common;

public static class GeoUtils
{
    private const double EarthRadiusMeters = 6371000.0;

    public static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Asin(Math.Min(1.0, Math.Sqrt(a)));
        return EarthRadiusMeters * c;
    }

    private static double ToRadians(double val) => (Math.PI / 180.0) * val;

    public static Coordinate ProjectPointOnSegment(Coordinate p, Coordinate a, Coordinate b)
    {
        double ax = p.X - a.X;
        double ay = p.Y - a.Y;
        double bx = b.X - a.X;
        double by = b.Y - a.Y;

        double abLenSq = bx * bx + by * by;
        if (abLenSq == 0) return new Coordinate(a.X, a.Y);

        double t = (ax * bx + ay * by) / abLenSq;
        if (t < 0) t = 0;
        if (t > 1) t = 1;

        return new Coordinate(a.X + t * bx, a.Y + t * by);
    }

    public static (Coordinate snapped, int segmentIndex) ProjectPointOnPolyline(LineString polyline, Point point)
    {
        double minDistance = double.MaxValue;
        Coordinate closestCoord = null!;
        int closestSegmentIdx = -1;

        var coords = polyline.Coordinates;
        var pCoord = point.Coordinate;

        for (int i = 0; i < coords.Length - 1; i++)
        {
            var a = coords[i];
            var b = coords[i + 1];
            var s = ProjectPointOnSegment(pCoord, a, b);

            double dist = HaversineDistance(pCoord.Y, pCoord.X, s.Y, s.X);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestCoord = s;
                closestSegmentIdx = i;
            }
        }

        return (closestCoord, closestSegmentIdx);
    }

    public static double CalculateDistanceAlongPolyline(LineString polyline, Coordinate snappedPoint, int segmentIndex)
    {
        double totalDist = 0;
        var coords = polyline.Coordinates;

        for (int i = 0; i < segmentIndex; i++)
        {
            totalDist += HaversineDistance(coords[i].Y, coords[i].X, coords[i + 1].Y, coords[i + 1].X);
        }

        totalDist += HaversineDistance(coords[segmentIndex].Y, coords[segmentIndex].X, snappedPoint.Y, snappedPoint.X);

        return totalDist;
    }
}
