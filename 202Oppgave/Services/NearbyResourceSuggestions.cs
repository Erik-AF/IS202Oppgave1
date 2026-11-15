using _202Oppgave.Models.Entities;
using _202Oppgave.ViewModels;

namespace _202Oppgave.Services;

public static class NearbyResourceSuggestions
{
    public static List<NearbyResourceViewModel> Find(Need need, IEnumerable<Resource> resources)
    {
        if (!ValidPosition(need.Latitude, need.Longitude)) return new();

        return resources
            .Where(r => r.Status == "Tilgjengelig" && ValidPosition(r.Latitude, r.Longitude))
            .Select(r => new NearbyResourceViewModel
            {
                Id = r.Id,
                Title = r.Type,
                Category = r.Category?.Name,
                Location = r.Name,
                DistanceKm = DistanceKm(need.Latitude, need.Longitude, r.Latitude!.Value, r.Longitude!.Value)
            })
            .Where(r => r.DistanceKm <= 5)
            .OrderBy(r => r.DistanceKm)
            .ThenBy(r => r.Id)
            .Take(3)
            .ToList();
    }

    private static bool ValidPosition(double? latitude, double? longitude) =>
        latitude is >= -90 and <= 90 && longitude is >= -180 and <= 180;

    // Haversine gir luftlinje i kilometer, ikke kjørelengde eller avstand til kanten av et tegnet område.
    private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double radians = Math.PI / 180;
        var a = Math.Pow(Math.Sin((lat2 - lat1) * radians / 2), 2)
            + Math.Cos(lat1 * radians) * Math.Cos(lat2 * radians)
            * Math.Pow(Math.Sin((lon2 - lon1) * radians / 2), 2);
        return 6371 * 2 * Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1)));
    }
}
