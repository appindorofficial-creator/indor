using System.Globalization;
using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class RealtorNearbyNetworkService(AppDbContext db, RealtorPortalService portalService)
{
    private static readonly IReadOnlyList<RealtorNetworkFilterChipViewModel> FilterChips =
    [
        new() { Label = "All", Value = "All", Icon = "fa-border-all" },
        new() { Label = "Homes", Value = "Homes", Icon = "fa-house" },
        new() { Label = "Providers", Value = "Providers", Icon = "fa-screwdriver-wrench" },
        new() { Label = "Promotions", Value = "Promotions", Icon = "fa-tags" },
        new() { Label = "Emergency", Value = "Emergency", Icon = "fa-triangle-exclamation" },
        new() { Label = "Open Houses", Value = "OpenHouses", Icon = "fa-door-open" }
    ];

    public async Task<RealtorNearbyNetworkViewModel> BuildAsync(
        IndorRealtor realtor,
        string? view,
        string? filter,
        string? search,
        string? scope,
        CancellationToken ct = default)
    {
        var shell = await portalService.BuildShellAsync(realtor, ct);
        var settings = await EnsureSettingsAsync(realtor, ct);
        var activeView = string.Equals(view, "map", StringComparison.OrdinalIgnoreCase) ? "map" : "feed";
        var activeFilter = NormalizeFilter(filter);
        var mineOnly = string.Equals(scope, "mine", StringComparison.OrdinalIgnoreCase);

        var items = await LoadItemsAsync(realtor, settings, activeFilter, search, mineOnly, ct);
        var cards = items.Select(i => MapToCard(i, realtor.Id)).ToList();

        return new RealtorNearbyNetworkViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            ActiveView = activeView,
            ActiveFilter = activeFilter,
            SearchQuery = search,
            Filters = FilterChips,
            RadiusLabel = BuildRadiusLabel(settings, realtor),
            QuickActions = BuildQuickActions(),
            FeedCards = cards,
            MapPins = BuildMapPins(cards),
            MapCenterLabel = settings.CenterLabel
        };
    }

    public async Task<RealtorNetworkListingFormViewModel?> BuildListingFormAsync(
        IndorRealtor realtor,
        int? itemId,
        CancellationToken ct = default)
    {
        var shell = await portalService.BuildShellAsync(realtor, ct);
        IndorNearbyNetworkItem? item = null;

        if (itemId is > 0)
        {
            item = await db.IndorNearbyNetworkItems
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OwnerRealtorId == realtor.Id, ct);
            if (item == null)
            {
                return null;
            }
        }

        var model = new RealtorNetworkListingFormViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            ItemId = item?.Id,
            Title = item?.Title ?? "",
            Address = item?.Subtitle ?? "",
            Price = item?.Price,
            Bedrooms = item?.Bedrooms,
            Bathrooms = item?.Bathrooms,
            SquareFeet = item?.SquareFeet,
            ImageUrl = item?.ImageUrl ?? "/inspeccion2.jpeg",
            StatusBadge = item?.StatusBadge ?? "ACTIVE",
            IsOpenHouse = item?.CardType == NearbyNetworkCardTypes.OpenHouse,
            OpenHouseMeta = item?.MetaLabel
        };

        if (item?.Price is > 0 && string.IsNullOrWhiteSpace(model.Title))
        {
            model.Title = FormatCurrency(item.Price.Value);
        }

        return model;
    }

    public async Task<int?> SaveListingAsync(IndorRealtor realtor, RealtorNetworkListingFormViewModel model, CancellationToken ct = default)
    {
        await EnsureSettingsAsync(realtor, ct);

        IndorNearbyNetworkItem item;
        if (model.ItemId is > 0)
        {
            item = await db.IndorNearbyNetworkItems
                .FirstOrDefaultAsync(i => i.Id == model.ItemId && i.OwnerRealtorId == realtor.Id, ct)
                ?? throw new InvalidOperationException("Listing not found.");
        }
        else
        {
            item = new IndorNearbyNetworkItem
            {
                OwnerRealtorId = realtor.Id,
                IsOwnedListing = true,
                SortOrder = 5,
                CreatedUtc = DateTime.UtcNow
            };
            db.IndorNearbyNetworkItems.Add(item);
        }

        var isOpenHouse = model.IsOpenHouse;
        item.CardType = isOpenHouse ? NearbyNetworkCardTypes.OpenHouse : NearbyNetworkCardTypes.Listing;
        item.FilterCategory = isOpenHouse
            ? NearbyNetworkFilterCategories.OpenHouses
            : NearbyNetworkFilterCategories.Homes;
        item.BadgeLabel = isOpenHouse ? "OPEN HOUSE" : "MY LISTING";
        item.BadgeCss = isOpenHouse ? "openhouse" : "listing";
        item.Title = string.IsNullOrWhiteSpace(model.Title)
            ? FormatCurrency(model.Price ?? 0)
            : model.Title.Trim();
        item.Subtitle = model.Address.Trim();
        item.Price = model.Price;
        item.Bedrooms = model.Bedrooms;
        item.Bathrooms = model.Bathrooms;
        item.SquareFeet = model.SquareFeet;
        item.SpecsLabel = BuildSpecsLabel(model.Bedrooms, model.Bathrooms, model.SquareFeet);
        item.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? "/inspeccion2.jpeg" : model.ImageUrl.Trim();
        item.MetaLabel = isOpenHouse ? model.OpenHouseMeta?.Trim() : null;
        item.StatusBadge = string.IsNullOrWhiteSpace(model.StatusBadge) ? "ACTIVE" : model.StatusBadge.Trim().ToUpperInvariant();
        item.StatusCss = isOpenHouse ? "openhouse" : "active";
        item.PrimaryActionLabel = isOpenHouse ? "View Details" : "View Home";
        item.PrimaryActionUrl = "#";
        item.SecondaryActionLabel = isOpenHouse ? "Share" : "Edit Listing";
        item.SecondaryActionUrl = $"/Realtor/EditNetworkListing/{item.Id}";
        item.IsActive = true;
        item.UpdatedUtc = DateTime.UtcNow;
        item.DistanceMiles = 0;

        await db.SaveChangesAsync(ct);

        if (!isOpenHouse)
        {
            item.SecondaryActionUrl = $"/Realtor/EditNetworkListing/{item.Id}";
            await db.SaveChangesAsync(ct);
        }

        return item.Id;
    }

    private async Task<IndorNearbyNetworkSetting> EnsureSettingsAsync(IndorRealtor realtor, CancellationToken ct)
    {
        var settings = await db.IndorNearbyNetworkSettings
            .FirstOrDefaultAsync(s => s.RealtorId == realtor.Id, ct);

        if (settings != null)
        {
            return settings;
        }

        settings = new IndorNearbyNetworkSetting
        {
            RealtorId = realtor.Id,
            CenterLabel = string.IsNullOrWhiteSpace(realtor.ServiceAreas)
                ? "Charlotte, NC area"
                : realtor.ServiceAreas.Trim(),
            CenterAddress = realtor.ServiceAreas,
            CenterLatitude = 35.227086m,
            CenterLongitude = -80.843124m,
            RadiusMiles = 3m,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorNearbyNetworkSettings.Add(settings);
        await db.SaveChangesAsync(ct);
        return settings;
    }

    private async Task<List<IndorNearbyNetworkItem>> LoadItemsAsync(
        IndorRealtor realtor,
        IndorNearbyNetworkSetting settings,
        string activeFilter,
        string? search,
        bool mineOnly,
        CancellationToken ct)
    {
        var query = db.IndorNearbyNetworkItems
            .AsNoTracking()
            .Where(i => i.IsActive);

        if (mineOnly)
        {
            query = query.Where(i => i.OwnerRealtorId == realtor.Id && i.IsOwnedListing);
        }
        else
        {
            query = query.Where(i =>
                i.DistanceMiles == null || i.DistanceMiles <= settings.RadiusMiles);
        }

        query = activeFilter switch
        {
            "Homes" => query.Where(i => i.FilterCategory == NearbyNetworkFilterCategories.Homes),
            "Providers" => query.Where(i => i.FilterCategory == NearbyNetworkFilterCategories.Providers),
            "Promotions" => query.Where(i => i.FilterCategory == NearbyNetworkFilterCategories.Promotions),
            "Emergency" => query.Where(i => i.FilterCategory == NearbyNetworkFilterCategories.Emergency),
            "OpenHouses" => query.Where(i => i.FilterCategory == NearbyNetworkFilterCategories.OpenHouses),
            "Leads" => query.Where(i => i.FilterCategory == NearbyNetworkFilterCategories.Leads),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(i =>
                i.Title.Contains(term) ||
                (i.Subtitle != null && i.Subtitle.Contains(term)) ||
                i.BadgeLabel.Contains(term) ||
                (i.ProviderName != null && i.ProviderName.Contains(term)));
        }

        return await query
            .OrderBy(i => i.SortOrder)
            .ThenByDescending(i => i.CreatedUtc)
            .ToListAsync(ct);
    }

    private static RealtorNetworkFeedCardViewModel MapToCard(IndorNearbyNetworkItem item, int currentRealtorId)
    {
        var cardType = item.CardType.ToLowerInvariant() switch
        {
            "openhouse" => "openhouse",
            "lead" => "lead",
            "provider" => "provider",
            "promotion" => "promotion",
            "emergency" => "emergency",
            _ => "listing"
        };

        var isOwned = item.OwnerRealtorId == currentRealtorId && item.IsOwnedListing;
        var secondaryUrl = item.SecondaryActionUrl;
        if (isOwned && item.CardType == NearbyNetworkCardTypes.Listing)
        {
            secondaryUrl = $"/Realtor/EditNetworkListing/{item.Id}";
        }

        return new RealtorNetworkFeedCardViewModel
        {
            ItemId = item.Id,
            CardType = cardType,
            BadgeLabel = item.BadgeLabel,
            BadgeCss = item.BadgeCss,
            ImageUrl = item.ImageUrl,
            IconClass = item.IconClass,
            Title = item.CardType == NearbyNetworkCardTypes.Listing && item.Price is > 0 && item.Title.StartsWith('$')
                ? item.Title
                : item.Title,
            PriceLabel = item.Price is > 0 && !item.Title.StartsWith('$')
                ? FormatCurrency(item.Price.Value)
                : null,
            Subtitle = item.Subtitle,
            SpecsLabel = item.SpecsLabel ?? BuildSpecsLabel(item.Bedrooms, item.Bathrooms, item.SquareFeet),
            MetaLabel = item.MetaLabel,
            Tags = ParseTags(item.TagsJson),
            DistanceLabel = item.DistanceMiles is > 0
                ? $"{item.DistanceMiles:0.#} mi away"
                : "Nearby",
            StatusBadge = item.StatusBadge,
            StatusCss = item.StatusCss ?? "active",
            PrimaryActionLabel = item.PrimaryActionLabel,
            PrimaryActionUrl = item.PrimaryActionUrl,
            SecondaryActionLabel = item.SecondaryActionLabel,
            SecondaryActionUrl = secondaryUrl
        };
    }

    private static List<string> ParseTags(string? tagsJson)
    {
        if (string.IsNullOrWhiteSpace(tagsJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(tagsJson) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string BuildRadiusLabel(IndorNearbyNetworkSetting settings, IndorRealtor realtor)
    {
        var radius = settings.RadiusMiles.ToString("0.#", CultureInfo.InvariantCulture);
        var place = string.IsNullOrWhiteSpace(settings.CenterLabel)
            ? (realtor.ServiceAreas ?? "your home")
            : settings.CenterLabel;
        return $"{radius} miles around {place}";
    }

    private static List<RealtorQuickActionViewModel> BuildQuickActions() =>
    [
        new() { Label = "Post Listing", Subtitle = "List a property", Icon = "fa-plus", Url = "/Realtor/CreateNetworkListing" },
        new() { Label = "My Leads", Subtitle = "View leads", Icon = "fa-users", Url = "/Realtor/Network?filter=Leads" },
        new() { Label = "My Listings", Subtitle = "Manage listings", Icon = "fa-house", Url = "/Realtor/Network?filter=Homes&scope=mine" },
        new() { Label = "My Realtor Profile", Subtitle = "View my profile", Icon = "fa-user", Url = "/Realtor/Profile" }
    ];

    private static List<RealtorNetworkMapPinViewModel> BuildMapPins(IReadOnlyList<RealtorNetworkFeedCardViewModel> cards) =>
    [
        new() { Label = "You", PinType = "you", TopPercent = "48%", LeftPercent = "46%" },
        .. cards.Take(6).Select((card, index) => new RealtorNetworkMapPinViewModel
        {
            Label = card.Title,
            PinType = card.CardType switch
            {
                "listing" or "openhouse" => "home",
                "lead" => "lead",
                "provider" or "promotion" => "provider",
                "emergency" => "emergency",
                _ => "home"
            },
            TopPercent = $"{22 + index * 11}%",
            LeftPercent = $"{18 + (index % 3) * 26}%"
        })
    ];

    private static string NormalizeFilter(string? filter) =>
        filter?.Trim() switch
        {
            "Homes" => "Homes",
            "Providers" => "Providers",
            "Promotions" => "Promotions",
            "Emergency" => "Emergency",
            "OpenHouses" or "Open Houses" => "OpenHouses",
            "Leads" => "Leads",
            _ => "All"
        };

    private static string? BuildSpecsLabel(decimal? beds, decimal? baths, int? sqft)
    {
        if (beds is null && baths is null && sqft is null)
        {
            return null;
        }

        var parts = new List<string>();
        if (beds is > 0)
        {
            parts.Add($"{beds:0.#} Beds");
        }

        if (baths is > 0)
        {
            parts.Add($"{baths:0.#} Baths");
        }

        if (sqft is > 0)
        {
            parts.Add($"{sqft:N0} sqft");
        }

        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }

    private static string FormatCurrency(decimal amount) =>
        amount.ToString("C0", CultureInfo.GetCultureInfo("en-US"));
}
