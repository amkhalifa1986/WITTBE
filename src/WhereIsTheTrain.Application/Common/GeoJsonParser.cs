using System.Text.Json;
using NetTopologySuite.Geometries;

namespace WhereIsTheTrain.Application.Common;

public static class GeoJsonParser
{
    public static LineString ParseGeoJsonToLineString(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        
        JsonElement geomElement = default;
        if (root.TryGetProperty("type", out var typeProp))
        {
            var typeStr = typeProp.GetString();
            if (typeStr == "LineString")
            {
                geomElement = root;
            }
            else if (typeStr == "Feature")
            {
                if (root.TryGetProperty("geometry", out var geomProp))
                    geomElement = geomProp;
            }
            else if (typeStr == "FeatureCollection")
            {
                if (root.TryGetProperty("features", out var featuresProp) && featuresProp.ValueKind == JsonValueKind.Array && featuresProp.GetArrayLength() > 0)
                {
                    var firstFeature = featuresProp[0];
                    if (firstFeature.TryGetProperty("geometry", out var geomProp))
                        geomElement = geomProp;
                }
            }
        }

        if (geomElement.ValueKind == JsonValueKind.Undefined || !geomElement.TryGetProperty("type", out var geomType) || geomType.GetString() != "LineString" || !geomElement.TryGetProperty("coordinates", out var coordsProp) || coordsProp.ValueKind != JsonValueKind.Array)
        {
            throw new ArgumentException("Invalid GeoJSON: Could not locate a LineString geometry.");
        }

        var gf = GeometryFactory.Default;
        var coordinatesList = new List<Coordinate>();
        foreach (var coordArray in coordsProp.EnumerateArray())
        {
            if (coordArray.ValueKind == JsonValueKind.Array && coordArray.GetArrayLength() >= 2)
            {
                double lon = coordArray[0].GetDouble();
                double lat = coordArray[1].GetDouble();
                coordinatesList.Add(new Coordinate(lon, lat));
            }
        }

        if (coordinatesList.Count < 2)
        {
            throw new ArgumentException("LineString must contain at least 2 points.");
        }

        return gf.CreateLineString(coordinatesList.ToArray());
    }
}
