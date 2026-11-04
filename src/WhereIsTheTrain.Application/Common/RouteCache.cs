using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using NetTopologySuite.Geometries;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Common;

/// <summary>
/// Cached railway paths and per-train route geometry, shared by every map and tracking query.
/// All entries are tied to one change token, so <see cref="Invalidate"/> drops them at once
/// when trains, route stops, stations or railway paths are edited.
/// </summary>
public static class RouteCache
{
    private static CancellationTokenSource _version = new();

    private static IChangeToken CurrentToken => new CancellationChangeToken(_version.Token);

    /// <summary>Drops all cached railway paths and route geometry.</summary>
    public static void Invalidate()
    {
        var previous = Interlocked.Exchange(ref _version, new CancellationTokenSource());
        previous.Cancel();
    }

    public static async Task<IReadOnlyList<RailwayPath>> GetRailwayPathsAsync(IMemoryCache cache, IUnitOfWork unitOfWork, CancellationToken ct)
    {
        return await cache.GetOrCreateAsync("railway_paths", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            entry.AddExpirationToken(CurrentToken);
            return await unitOfWork.RailwayPaths.GetAllWithStopsAsync(ct);
        }) ?? new List<RailwayPath>();
    }

    public static LineString GetRoutePath(IMemoryCache cache, Guid trainId, IEnumerable<TrainRouteStop> routeStops, IReadOnlyList<RailwayPath> railwayPaths)
    {
        return cache.GetOrCreate($"route_path_{trainId}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            entry.AddExpirationToken(CurrentToken);
            return RoutePathBuilder.BuildRoutePath(routeStops, railwayPaths);
        })!;
    }
}
