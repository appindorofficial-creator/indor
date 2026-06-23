using System.Text.RegularExpressions;
using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static partial class NearbyNetworkImageResolver
{
    private static readonly string[] GenericPlaceholders =
    [
        "/inspeccion2.jpeg",
        "/inspeccion1.jpeg"
    ];

    // Real estate exterior photos used for property cards (listings / open houses).
    private static readonly string[] ListingImages =
    [
        "/listing-home-1.png",
        "/listing-home-2.png",
        "/openhouse-home.png"
    ];

    public static string? ResolveFeedImage(IndorNearbyNetworkItem item)
    {
        if (ShouldUseIconOnly(item.CardType))
        {
            return null;
        }

        var isProperty = item.CardType is NearbyNetworkCardTypes.Listing or NearbyNetworkCardTypes.OpenHouse;

        var stored = item.ImageUrl?.Trim();
        // Inspection photos (plumbing, interiors, systems) don't represent a home
        // for sale, so don't keep them for property cards; fall back to a house photo.
        var storedUsable = !string.IsNullOrWhiteSpace(stored)
            && !IsGenericPlaceholder(stored)
            && !(isProperty && IsInspectionPhoto(stored));
        if (storedUsable)
        {
            return stored;
        }

        return item.CardType switch
        {
            NearbyNetworkCardTypes.Listing or NearbyNetworkCardTypes.OpenHouse
                => PickStableListingImage(item.Id),
            NearbyNetworkCardTypes.Provider or NearbyNetworkCardTypes.Promotion
                => ResolveServiceImage(item.Title, item.ProviderName, item.Subtitle),
            _ => null
        };
    }

    public static string? ResolveServiceImage(string? title, string? providerName, string? subtitle)
    {
        var text = $"{title} {providerName} {subtitle}".ToLowerInvariant();

        if (ContainsAny(text, "hvac", "a/c", " ac ", "air ", "tune-up", "tune up", "climate", "heating", "cooling", "furnace"))
        {
            return "/aire.jpeg";
        }

        if (ContainsAny(text, "lawn", "yard", "grass", "landscap", "mow"))
        {
            return "/cesped.jpeg";
        }

        if (ContainsAny(text, "clean", "maid", "housekeep"))
        {
            return "/limpieza.jpeg";
        }

        if (ContainsAny(text, "trash", "waste", "garbage"))
        {
            return "/basura.jpeg";
        }

        if (ContainsAny(text, "plumb", "pipe", "drain", "water heater"))
        {
            return "/servicio2.jpeg";
        }

        if (ContainsAny(text, "roof", "gutter", "siding", "exterior"))
        {
            return "/servicio6.jpeg";
        }

        if (ContainsAny(text, "paint", "remodel", "renov"))
        {
            return "/servicio8.jpeg";
        }

        if (ContainsAny(text, "electric", "panel", "wiring"))
        {
            return "/servicio10.jpeg";
        }

        return "/servicio4.jpeg";
    }

    public static bool ShouldUseIconOnly(string? cardType) =>
        string.Equals(cardType, NearbyNetworkCardTypes.Lead, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(cardType, NearbyNetworkCardTypes.Emergency, StringComparison.OrdinalIgnoreCase);

    private static bool IsGenericPlaceholder(string url)
    {
        var normalized = url.Trim().TrimEnd('/').ToLowerInvariant();
        return GenericPlaceholders.Any(p => normalized.EndsWith(p, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsInspectionPhoto(string url) =>
        InspectionPhotoRegex().IsMatch(url.Trim());

    [GeneratedRegex(@"/inspeccion\d+\.jpe?g$", RegexOptions.IgnoreCase)]
    private static partial Regex InspectionPhotoRegex();

    private static string PickStableListingImage(int itemId)
    {
        var index = Math.Abs(itemId) % ListingImages.Length;
        return ListingImages[index];
    }

    private static bool ContainsAny(string text, params string[] terms) =>
        terms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
}
