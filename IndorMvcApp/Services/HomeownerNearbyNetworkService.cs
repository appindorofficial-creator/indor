using System.Globalization;
using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IndorMvcApp.Services;

public class HomeownerNearbyNetworkService(
    AppDbContext db,
    IAddressLookupService addressLookup,
    IOptions<GoogleMapsOptions> googleMapsOptions)
{
    private readonly GoogleMapsOptions _googleMaps = googleMapsOptions.Value;
    private const decimal DefaultRadiusMiles = 3m;
    private const double NeighborRequestRadiusMiles = 15d;

    private static readonly IReadOnlyList<RealtorNetworkFilterChipViewModel> FilterChips =
    [
        new() { Label = "All", Value = NearbyNetworkHomeownerFilters.All, Icon = "fa-border-all" },
        new() { Label = "Listings", Value = NearbyNetworkHomeownerFilters.Homes, Icon = "fa-house" },
        new() { Label = "Services", Value = NearbyNetworkHomeownerFilters.Providers, Icon = "fa-screwdriver-wrench" },
        new() { Label = "Quick Help", Value = NearbyNetworkHomeownerFilters.NeighborRequests, Icon = "fa-people-carry-box" },
        new() { Label = "Promotions", Value = NearbyNetworkHomeownerFilters.Promotions, Icon = "fa-tags" }
    ];

    public async Task<HomeownerNearbyNetworkViewModel> BuildAsync(
        Propiedad? propiedad,
        PropertyInfoViewModel? propertyInfo,
        int notificationCount,
        string? view,
        string? filter,
        string? search,
        IUrlHelper url,
        string? userId = null,
        CancellationToken ct = default)
    {
        var activeView = string.Equals(view, "map", StringComparison.OrdinalIgnoreCase) ? "map" : "feed";
        var activeFilter = NormalizeFilter(filter);

        if (propiedad == null)
        {
            return new HomeownerNearbyNetworkViewModel
            {
                HasProperty = false,
                ActiveView = activeView,
                ActiveFilter = activeFilter,
                SearchQuery = search,
                Filters = FilterChips,
                NotificationCount = notificationCount,
                GoogleMapsApiKey = _googleMaps.BrowserApiKey,
                QuickActions = BuildQuickActions(null, activeFilter, search, url)
            };
        }

        var (centerLat, centerLng) = await ResolveCenterAsync(propiedad, propertyInfo, ct);
        var radiusMiles = (double)DefaultRadiusMiles;
        var radiusLabel = $"{DefaultRadiusMiles.ToString("0.#", CultureInfo.InvariantCulture)} miles around your home";

        var networkCards = await LoadNetworkCardsAsync(
            propiedad.Id,
            centerLat,
            centerLng,
            radiusMiles,
            activeFilter,
            search,
            url,
            ct);

        var neighborCards = await LoadNeighborRequestCardsAsync(
            userId,
            propiedad,
            propertyInfo,
            centerLat,
            centerLng,
            activeFilter,
            search,
            url,
            ct);

        var providerCards = await LoadProviderCardsAsync(
            centerLat,
            centerLng,
            radiusMiles,
            activeFilter,
            search,
            url,
            ct);

        var myPostCards = !string.IsNullOrWhiteSpace(userId)
            ? await LoadMyPostCardsAsync(userId, propiedad.Id, activeFilter, search, url, ct)
            : [];

        var feedCards = myPostCards
            .Concat(networkCards)
            .Concat(neighborCards)
            .Concat(providerCards)
            .OrderBy(c => c.SortOrder)
            .ThenByDescending(c => c.CreatedUtc)
            .Select(c => c.Card)
            .ToList();

        var mapProviders = await LoadNearbyProvidersAsync(centerLat, centerLng, radiusMiles, activeFilter, ct);
        var mapItems = await LoadMapItemsAsync(centerLat, centerLng, radiusMiles, activeFilter, ct);
        var mapPins = BuildMapPins(centerLat, centerLng, radiusMiles, mapItems, neighborCards);
        var carouselItems = BuildMapCarouselItems(
            centerLat,
            centerLng,
            mapProviders,
            mapItems,
            propiedad.Id,
            url,
            url.Action("Index", "Home") + "#section-services",
            url.Action("Index", "Home") + "#section-more");
        var propertyAddress = !string.IsNullOrWhiteSpace(propertyInfo?.FormattedAddress)
            ? propertyInfo!.FormattedAddress
            : propiedad.Direccion;

        var quickHelpUrls = BuildQuickHelpUrls(propiedad.Id, url);

        return new HomeownerNearbyNetworkViewModel
        {
            HasProperty = true,
            PropiedadId = propiedad.Id,
            NotificationCount = notificationCount,
            ActiveView = activeView,
            ActiveFilter = activeFilter,
            SearchQuery = search,
            Filters = FilterChips,
            RadiusLabel = radiusLabel,
            PropertyAddress = propertyAddress,
            PostQuickJobUrl = quickHelpUrls.PostQuickJobUrl,
            BrowseHelpersUrl = quickHelpUrls.BrowseHelpersUrl,
            MyQuickJobsUrl = quickHelpUrls.MyQuickJobsUrl,
            MyRequestsUrl = quickHelpUrls.MyRequestsUrl,
            QuickActions = BuildQuickActions(propiedad.Id, activeFilter, search, url),
            FeedCards = feedCards,
            MapPins = mapPins,
            MapProviders = mapProviders,
            MapCarouselItems = carouselItems,
            MapCenterLabel = "Your home",
            CenterLatitude = centerLat,
            CenterLongitude = centerLng,
            RadiusMiles = radiusMiles,
            GoogleMapsApiKey = _googleMaps.BrowserApiKey,
            MapNearbyCount = carouselItems.Count
        };
    }

    /// <summary>
    /// Builds the Map view (Google map + nearby providers + carousel) centered on a free-form
    /// address. Used by roles that are not tied to a single homeowner property (e.g. property
    /// administrators with multiple homes). Falls back to the configured default center.
    /// </summary>
    public async Task<HomeownerNearbyNetworkViewModel> BuildMapForAddressAsync(
        string? address,
        string centerLabel,
        string? filter,
        IUrlHelper url,
        string servicesUrl,
        string messageUrl,
        CancellationToken ct = default)
    {
        var activeFilter = NormalizeFilter(filter);
        double centerLat = _googleMaps.DefaultLatitude;
        double centerLng = _googleMaps.DefaultLongitude;

        if (!string.IsNullOrWhiteSpace(address))
        {
            var coordinates = await addressLookup.GeocodeAddressAsync(address.Trim(), ct);
            if (coordinates is { } coords)
            {
                centerLat = (double)coords.Latitude;
                centerLng = (double)coords.Longitude;
            }
        }

        var radiusMiles = (double)DefaultRadiusMiles;
        var mapProviders = await LoadNearbyProvidersAsync(centerLat, centerLng, radiusMiles, activeFilter, ct);
        var mapItems = await LoadMapItemsAsync(centerLat, centerLng, radiusMiles, activeFilter, ct);
        var carouselItems = BuildMapCarouselItems(
            centerLat,
            centerLng,
            mapProviders,
            mapItems,
            0,
            url,
            servicesUrl,
            messageUrl);

        return new HomeownerNearbyNetworkViewModel
        {
            HasProperty = true,
            ActiveView = "map",
            ActiveFilter = activeFilter,
            Filters = FilterChips,
            RadiusLabel = $"{DefaultRadiusMiles.ToString("0.#", CultureInfo.InvariantCulture)} miles around your portfolio",
            PropertyAddress = address,
            MapProviders = mapProviders,
            MapCarouselItems = carouselItems,
            MapCenterLabel = centerLabel,
            CenterLatitude = centerLat,
            CenterLongitude = centerLng,
            RadiusMiles = radiusMiles,
            GoogleMapsApiKey = _googleMaps.BrowserApiKey,
            MapNearbyCount = carouselItems.Count
        };
    }

    public async Task<RealtorNetworkMapDataViewModel?> GetMapDataAsync(
        Propiedad propiedad,
        PropertyInfoViewModel? propertyInfo,
        double? lat,
        double? lng,
        string? addressQuery,
        string? filter,
        CancellationToken ct = default)
    {
        var activeFilter = NormalizeFilter(filter);
        var (centerLat, centerLng) = await ResolveCenterAsync(propiedad, propertyInfo, ct);
        var radiusMiles = (double)DefaultRadiusMiles;
        var centerLabel = "Your home";

        if (!string.IsNullOrWhiteSpace(addressQuery))
        {
            var coordinates = await addressLookup.GeocodeAddressAsync(addressQuery.Trim(), ct);
            if (coordinates is not { } coords)
            {
                return null;
            }

            lat = (double)coords.Latitude;
            lng = (double)coords.Longitude;
            centerLabel = addressQuery.Trim();
        }
        else if (lat is null || lng is null)
        {
            lat = centerLat;
            lng = centerLng;
        }

        var providers = await LoadNearbyProvidersAsync(lat.Value, lng.Value, radiusMiles, activeFilter, ct);
        var listings = await LoadMapListingsNearAsync(lat.Value, lng.Value, radiusMiles, activeFilter, ct);

        return new RealtorNetworkMapDataViewModel
        {
            Lat = lat.Value,
            Lng = lng.Value,
            CenterLabel = centerLabel,
            RadiusMiles = radiusMiles,
            ActiveFilter = activeFilter,
            Providers = providers,
            Listings = listings,
            ProviderCount = providers.Count,
            ListingCount = listings.Count,
            TotalCount = providers.Count + listings.Count
        };
    }

    private async Task<(double Lat, double Lng)> ResolveCenterAsync(
        Propiedad propiedad,
        PropertyInfoViewModel? propertyInfo,
        CancellationToken ct)
    {
        if (propertyInfo is { Latitude: not 0, Longitude: not 0 })
        {
            return ((double)propertyInfo.Latitude, (double)propertyInfo.Longitude);
        }

        var address = !string.IsNullOrWhiteSpace(propertyInfo?.FormattedAddress)
            ? propertyInfo!.FormattedAddress
            : propiedad.Direccion;

        if (!string.IsNullOrWhiteSpace(address))
        {
            var coordinates = await addressLookup.GeocodeAddressAsync(address.Trim(), ct);
            if (coordinates is { } coords)
            {
                return ((double)coords.Latitude, (double)coords.Longitude);
            }
        }

        return (_googleMaps.DefaultLatitude, _googleMaps.DefaultLongitude);
    }

    private async Task<List<FeedCardSortable>> LoadNetworkCardsAsync(
        int currentPropiedadId,
        double centerLat,
        double centerLng,
        double radiusMiles,
        string activeFilter,
        string? search,
        IUrlHelper url,
        CancellationToken ct)
    {
        if (activeFilter is NearbyNetworkHomeownerFilters.NeighborRequests)
        {
            return [];
        }

        var query = db.IndorNearbyNetworkItems
            .AsNoTracking()
            .Where(i => i.IsActive)
            .Where(i => i.FilterCategory != NearbyNetworkFilterCategories.Leads);

        query = activeFilter switch
        {
            NearbyNetworkHomeownerFilters.Homes => query.Where(i =>
                i.FilterCategory == NearbyNetworkFilterCategories.Homes ||
                i.FilterCategory == NearbyNetworkFilterCategories.OpenHouses),
            NearbyNetworkHomeownerFilters.Providers => query.Where(i =>
                i.FilterCategory == NearbyNetworkFilterCategories.Providers),
            NearbyNetworkHomeownerFilters.Promotions => query.Where(i =>
                i.FilterCategory == NearbyNetworkFilterCategories.Promotions),
            NearbyNetworkHomeownerFilters.Emergency => query.Where(i =>
                i.FilterCategory == NearbyNetworkFilterCategories.Emergency),
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

        var items = await query
            .OrderBy(i => i.SortOrder)
            .ThenByDescending(i => i.CreatedUtc)
            .ToListAsync(ct);

        return items
            .Where(i => !string.Equals(i.BadgeLabel, "MY LISTING", StringComparison.OrdinalIgnoreCase))
            .Select(i =>
            {
                var distance = ResolveItemDistanceMiles(i, centerLat, centerLng);
                return new { Item = i, Distance = distance };
            })
            .Where(x => x.Distance is null || x.Distance <= (decimal)radiusMiles)
            .Select(x => new FeedCardSortable
            {
                SortOrder = x.Item.SortOrder,
                CreatedUtc = x.Item.CreatedUtc,
                Card = MapNetworkItemToCard(x.Item, x.Distance, currentPropiedadId, url)
            })
            .ToList();
    }

    private async Task<List<FeedCardSortable>> LoadMyPostCardsAsync(
        string userId,
        int propiedadId,
        string activeFilter,
        string? search,
        IUrlHelper url,
        CancellationToken ct)
    {
        if (activeFilter is not (NearbyNetworkHomeownerFilters.All or NearbyNetworkHomeownerFilters.NeighborRequests))
        {
            return [];
        }

        List<IndorNeighborRequest> requests;
        try
        {
            requests = await db.IndorNeighborRequests
                .AsNoTracking()
                .Include(r => r.Category)
                .Include(r => r.Photos)
                .Include(r => r.Propiedad)
                .Where(r => r.UserId == userId
                            && r.PropiedadId == propiedadId
                            && r.Status != NeighborRequestStatuses.Completed
                            && r.Status != NeighborRequestStatuses.Cancelled)
                .OrderByDescending(r => r.PublishedUtc ?? r.CreatedUtc)
                .Take(5)
                .ToListAsync(ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            requests = requests
                .Where(r =>
                    r.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (r.Description != null && r.Description.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        return requests
            .Select(r => new FeedCardSortable
            {
                SortOrder = -100,
                CreatedUtc = r.PublishedUtc ?? r.CreatedUtc,
                Card = MapMyPostToCard(r, url)
            })
            .ToList();
    }

    private async Task<List<FeedCardSortable>> LoadNeighborRequestCardsAsync(
        string? currentUserId,
        Propiedad viewerPropiedad,
        PropertyInfoViewModel? viewerPropertyInfo,
        double centerLat,
        double centerLng,
        string activeFilter,
        string? search,
        IUrlHelper url,
        CancellationToken ct)
    {
        if (activeFilter is not (NearbyNetworkHomeownerFilters.All or NearbyNetworkHomeownerFilters.NeighborRequests))
        {
            return [];
        }

        var viewerCity = ResolveViewerCity(viewerPropertyInfo, viewerPropiedad);
        var viewerState = ResolveViewerState(viewerPropertyInfo, viewerPropiedad);

        List<IndorNeighborRequest> requests;
        try
        {
            var query = db.IndorNeighborRequests
                .AsNoTracking()
                .Include(r => r.Category)
                .Where(r => r.IsActive
                    && r.Status == NeighborRequestStatuses.Active
                    && (r.AudienceCode == NeighborRequestAudienceCodes.Neighbors
                        || r.AudienceCode == NeighborRequestAudienceCodes.Both));

            if (!string.IsNullOrWhiteSpace(currentUserId))
            {
                query = query.Where(r => r.UserId != currentUserId);
            }

            requests = await query
                .OrderByDescending(r => r.PublishedUtc ?? r.CreatedUtc)
                .Take(60)
                .ToListAsync(ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            requests = requests
                .Where(r =>
                    r.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (r.Description != null && r.Description.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        var cards = new List<FeedCardSortable>();
        foreach (var request in requests)
        {
            var (requestLat, requestLng) = await ResolveRequestCoordinatesAsync(request, ct);
            var distance = requestLat is not null && requestLng is not null
                ? CalculateDistanceMiles(centerLat, centerLng, requestLat.Value, requestLng.Value)
                : null;

            if (!IsNeighborRequestVisible(
                    distance,
                    request.LocationAddress,
                    viewerCity,
                    viewerState))
            {
                continue;
            }

            cards.Add(new FeedCardSortable
            {
                SortOrder = 1000,
                CreatedUtc = request.PublishedUtc ?? request.CreatedUtc,
                Card = MapNeighborRequestToCard(request, distance, url)
            });
        }

        return cards;
    }

    private async Task<(double? Lat, double? Lng)> ResolveRequestCoordinatesAsync(
        IndorNeighborRequest request,
        CancellationToken ct)
    {
        if (request.Latitude is not null && request.Longitude is not null)
        {
            return ((double)request.Latitude.Value, (double)request.Longitude.Value);
        }

        if (string.IsNullOrWhiteSpace(request.LocationAddress))
        {
            return (null, null);
        }

        var coords = await addressLookup.GeocodeAddressAsync(request.LocationAddress.Trim(), ct);
        if (coords is not { } resolved)
        {
            return (null, null);
        }

        return ((double)resolved.Latitude, (double)resolved.Longitude);
    }

    private static bool IsNeighborRequestVisible(
        decimal? distanceMiles,
        string? requestAddress,
        string? viewerCity,
        string? viewerState)
    {
        if (distanceMiles is <= (decimal)NeighborRequestRadiusMiles)
        {
            return true;
        }

        return IsSameCityAndState(requestAddress, viewerCity, viewerState);
    }

    private static string? ResolveViewerCity(PropertyInfoViewModel? info, Propiedad propiedad) =>
        !string.IsNullOrWhiteSpace(info?.City) ? info!.City.Trim() : ExtractAddressPart(propiedad.Direccion, part: "city");

    private static string? ResolveViewerState(PropertyInfoViewModel? info, Propiedad propiedad) =>
        !string.IsNullOrWhiteSpace(info?.State) ? info!.State.Trim() : ExtractAddressPart(propiedad.Direccion, part: "state");

    private static bool IsSameCityAndState(
        string? requestAddress,
        string? viewerCity,
        string? viewerState)
    {
        if (string.IsNullOrWhiteSpace(requestAddress)
            || string.IsNullOrWhiteSpace(viewerCity)
            || string.IsNullOrWhiteSpace(viewerState))
        {
            return false;
        }

        var normalized = requestAddress.Trim();
        return normalized.Contains(viewerCity, StringComparison.OrdinalIgnoreCase)
            && normalized.Contains(viewerState, StringComparison.OrdinalIgnoreCase);
    }

    private static string? ExtractAddressPart(string? address, string part)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }

        var segments = address.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            return null;
        }

        return part switch
        {
            "city" when segments.Length >= 2 => segments[^2],
            "state" => segments[^1].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault(),
            _ => null
        };
    }

    private async Task<List<FeedCardSortable>> LoadProviderCardsAsync(
        double centerLat,
        double centerLng,
        double radiusMiles,
        string activeFilter,
        string? search,
        IUrlHelper url,
        CancellationToken ct)
    {
        if (activeFilter is not (NearbyNetworkHomeownerFilters.All or NearbyNetworkHomeownerFilters.Providers))
        {
            return [];
        }

        var providers = await LoadNearbyProvidersAsync(centerLat, centerLng, radiusMiles, activeFilter, ct);
        if (providers.Count == 0)
        {
            return [];
        }

        var existingProviderNames = await db.IndorNearbyNetworkItems
            .AsNoTracking()
            .Where(i => i.IsActive && i.FilterCategory == NearbyNetworkFilterCategories.Providers)
            .Select(i => i.ProviderName ?? i.Title)
            .ToListAsync(ct);

        var cards = new List<FeedCardSortable>();
        var sort = 2000;

        foreach (var provider in providers)
        {
            if (existingProviderNames.Any(n =>
                    string.Equals(n, provider.Name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(search) &&
                !provider.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) &&
                !(provider.Category?.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ?? false))
            {
                continue;
            }

            cards.Add(new FeedCardSortable
            {
                SortOrder = sort++,
                CreatedUtc = DateTime.UtcNow,
                Card = new RealtorNetworkFeedCardViewModel
                {
                    CardType = "provider",
                    BadgeLabel = "PROVIDER",
                    BadgeCss = "provider",
                    ImageUrl = NearbyNetworkImageResolver.ResolveServiceImage(provider.Name, provider.Category, null),
                    Title = provider.Name,
                    Subtitle = provider.Category,
                    Tags = provider.IsVerified
                        ? ["Verified Provider", "Insurance Active"]
                        : [],
                    DistanceLabel = provider.DistanceMiles is > 0
                        ? $"{provider.DistanceMiles:0.#} mi away"
                        : "Nearby",
                    StatusBadge = provider.IsVerified ? "Verified Provider" : "",
                    StatusCss = "active",
                    PrimaryActionLabel = "Message in INDOR",
                    PrimaryActionUrl = url.Action("Index", "Home") + "#section-more",
                    SecondaryActionLabel = "Request Service",
                    SecondaryActionUrl = url.Action("Index", "Home") + "#section-services"
                }
            });
        }

        return cards;
    }

    private async Task<List<IndorNearbyNetworkItem>> LoadMapItemsAsync(
        double centerLat,
        double centerLng,
        double radiusMiles,
        string activeFilter,
        CancellationToken ct)
    {
        if (activeFilter is NearbyNetworkHomeownerFilters.NeighborRequests)
        {
            return [];
        }

        var query = db.IndorNearbyNetworkItems
            .AsNoTracking()
            .Where(i => i.IsActive)
            .Where(i => i.FilterCategory != NearbyNetworkFilterCategories.Leads);

        query = activeFilter switch
        {
            NearbyNetworkHomeownerFilters.Homes => query.Where(i =>
                i.FilterCategory == NearbyNetworkFilterCategories.Homes ||
                i.FilterCategory == NearbyNetworkFilterCategories.OpenHouses),
            NearbyNetworkHomeownerFilters.Providers => query.Where(i =>
                i.FilterCategory == NearbyNetworkFilterCategories.Providers),
            NearbyNetworkHomeownerFilters.Promotions => query.Where(i =>
                i.FilterCategory == NearbyNetworkFilterCategories.Promotions),
            NearbyNetworkHomeownerFilters.Emergency => query.Where(i =>
                i.FilterCategory == NearbyNetworkFilterCategories.Emergency),
            _ => query
        };

        var items = await query.ToListAsync(ct);
        return items
            .Where(i => i.Latitude is not null && i.Longitude is not null)
            .Where(i =>
            {
                var distance = CalculateDistanceMiles(
                    centerLat,
                    centerLng,
                    (double)i.Latitude!.Value,
                    (double)i.Longitude!.Value);
                return distance <= (decimal)radiusMiles;
            })
            .Take(25)
            .ToList();
    }

    private async Task<List<RealtorNetworkMapPinViewModel>> LoadMapListingsNearAsync(
        double centerLat,
        double centerLng,
        double radiusMiles,
        string activeFilter,
        CancellationToken ct)
    {
        var items = await LoadMapItemsAsync(centerLat, centerLng, radiusMiles, activeFilter, ct);
        return items.Select(i =>
        {
            var pinType = i.CardType.ToLowerInvariant() switch
            {
                "listing" or "openhouse" => "home",
                "provider" or "promotion" => "provider",
                "emergency" => "emergency",
                _ => "home"
            };

            return new RealtorNetworkMapPinViewModel
            {
                Label = i.Title,
                PinType = pinType,
                Latitude = (double)i.Latitude!.Value,
                Longitude = (double)i.Longitude!.Value
            };
        }).ToList();
    }

    private static List<RealtorNetworkMapPinViewModel> BuildMapPins(
        double centerLat,
        double centerLng,
        double radiusMiles,
        IReadOnlyList<IndorNearbyNetworkItem> items,
        IReadOnlyList<FeedCardSortable> neighborCards)
    {
        var pins = new List<RealtorNetworkMapPinViewModel>
        {
            CreatePin("Your home", "you", "fa-house", centerLat, centerLng, centerLat, centerLng, radiusMiles)
        };

        foreach (var item in items.Where(i => i.Latitude is not null && i.Longitude is not null))
        {
            var pinType = item.CardType.ToLowerInvariant() switch
            {
                "listing" or "openhouse" => "home",
                "provider" or "promotion" => "provider",
                "emergency" => "emergency",
                _ => "home"
            };

            pins.Add(CreatePin(
                item.Title,
                pinType,
                pinType == "provider" ? "fa-screwdriver-wrench" : "fa-house",
                (double)item.Latitude!.Value,
                (double)item.Longitude!.Value,
                centerLat,
                centerLng,
                radiusMiles));
        }

        return pins;
    }

    private static RealtorNetworkFeedCardViewModel MapNetworkItemToCard(
        IndorNearbyNetworkItem item,
        decimal? distanceMiles,
        int propiedadId,
        IUrlHelper url)
    {
        var cardType = item.CardType.ToLowerInvariant() switch
        {
            "openhouse" => "openhouse",
            "provider" => "provider",
            "promotion" => "promotion",
            "emergency" => "emergency",
            _ => "listing"
        };

        var primaryUrl = NormalizeActionUrl(item.PrimaryActionUrl, url);
        var secondaryUrl = NormalizeActionUrl(item.SecondaryActionUrl, url);

        if (item.FilterCategory == NearbyNetworkFilterCategories.Emergency)
        {
            primaryUrl = url.Action("Index", "Home") + "#emergency-services";
            secondaryUrl = url.Action("Index", "Home") + "#section-more";
        }
        else if (item.FilterCategory == NearbyNetworkFilterCategories.Providers)
        {
            if (string.IsNullOrWhiteSpace(item.PrimaryActionUrl) || item.PrimaryActionUrl == "#")
            {
                primaryUrl = url.Action("Index", "Home") + "#section-more";
            }

            if (string.IsNullOrWhiteSpace(item.SecondaryActionUrl) || item.SecondaryActionUrl == "#")
            {
                secondaryUrl = url.Action("Index", "Home") + "#section-services";
            }
        }
        else if (item.FilterCategory is NearbyNetworkFilterCategories.Homes or NearbyNetworkFilterCategories.OpenHouses)
        {
            if (string.IsNullOrWhiteSpace(item.SecondaryActionUrl) || item.SecondaryActionUrl == "#")
            {
                secondaryUrl = url.Action("Request", "RealtorRequest", new { propiedadId });
            }
        }

        var imageUrl = NearbyNetworkImageResolver.ResolveFeedImage(item);

        return new RealtorNetworkFeedCardViewModel
        {
            ItemId = item.Id,
            CardType = cardType,
            BadgeLabel = item.BadgeLabel,
            BadgeCss = item.BadgeCss,
            ImageUrl = imageUrl,
            IconClass = string.IsNullOrWhiteSpace(imageUrl) ? item.IconClass : null,
            Title = item.Title,
            PriceLabel = item.Price is > 0 && !item.Title.StartsWith('$')
                ? FormatCurrency(item.Price.Value)
                : null,
            Subtitle = item.Subtitle,
            SpecsLabel = item.SpecsLabel ?? BuildSpecsLabel(item.Bedrooms, item.Bathrooms, item.SquareFeet),
            MetaLabel = item.MetaLabel,
            Tags = ParseTags(item.TagsJson),
            DistanceLabel = distanceMiles is > 0
                ? $"{distanceMiles:0.#} mi away"
                : item.DistanceMiles is > 0
                    ? $"{item.DistanceMiles:0.#} mi away"
                    : "Nearby",
            StatusBadge = item.StatusBadge ?? string.Empty,
            StatusCss = item.StatusCss ?? "active",
            PrimaryActionLabel = item.PrimaryActionLabel,
            PrimaryActionUrl = primaryUrl,
            SecondaryActionLabel = item.SecondaryActionLabel,
            SecondaryActionUrl = secondaryUrl
        };
    }

    private static RealtorNetworkFeedCardViewModel MapMyPostToCard(
        IndorNeighborRequest request,
        IUrlHelper url)
    {
        var photo = request.Photos.OrderBy(p => p.SortOrder).FirstOrDefault();
        var imageUrl = photo?.FilePath ?? ResolveCategoryImage(request.Category?.Code);
        var statusLabel = request.Status switch
        {
            NeighborRequestStatuses.Active => "ACTIVE",
            NeighborRequestStatuses.InProgress => "IN PROGRESS",
            _ => request.Status.ToUpperInvariant()
        };

        return new RealtorNetworkFeedCardViewModel
        {
            ItemId = request.Id,
            CardType = "mypost",
            BadgeLabel = "MY POST",
            BadgeCss = "mypost",
            ImageUrl = imageUrl,
            Title = request.Title,
            Subtitle = request.Description,
            MetaLabel = !string.IsNullOrWhiteSpace(request.LocationAddress)
                ? request.LocationAddress
                : request.Propiedad?.Direccion,
            StatusBadge = statusLabel,
            StatusCss = request.Status == NeighborRequestStatuses.Active ? "active" : "pending",
            PrimaryActionLabel = "View post",
            PrimaryActionUrl = url.Action("Detail", "NeighborRequest", new { id = request.Id }) ?? "#",
            SecondaryActionLabel = "Edit post",
            SecondaryActionUrl = url.Action("Edit", "NeighborRequest", new { id = request.Id }) ?? "#"
        };
    }

    private static string? ResolveCategoryImage(string? categoryCode) => categoryCode switch
    {
        "yard-patio" or "home-improvements" => "/cesped.jpeg",
        "cleaning" => "/img/cleaning-service.jpg",
        _ => "/cesped.jpeg"
    };

    private static RealtorNetworkFeedCardViewModel MapNeighborRequestToCard(
        IndorNeighborRequest request,
        decimal? distanceMiles,
        IUrlHelper url) =>
        new()
        {
            ItemId = request.Id,
            CardType = "neighborrequest",
            BadgeLabel = "NEIGHBOR REQUEST",
            BadgeCss = "lead",
            IconClass = request.Category?.IconClass ?? "fa-users",
            Title = request.Title,
            Subtitle = request.Description,
            MetaLabel = NeighborRequestWizardService.FormatRelativeTime(request.PublishedUtc ?? request.CreatedUtc),
            DistanceLabel = distanceMiles is > 0 ? $"{distanceMiles:0.#} mi away" : "Nearby",
            PrimaryActionLabel = "See Providers",
            PrimaryActionUrl = url.Action("Index", "Home") + "#section-services",
            SecondaryActionLabel = "Message in INDOR",
            SecondaryActionUrl = url.Action("Index", "Home") + "#section-more"
        };

    private async Task<List<RealtorNetworkMapProviderViewModel>> LoadNearbyProvidersAsync(
        double centerLat,
        double centerLng,
        double radiusMiles,
        string activeFilter,
        CancellationToken ct)
    {
        if (activeFilter is not (NearbyNetworkHomeownerFilters.All or NearbyNetworkHomeownerFilters.Providers))
        {
            return [];
        }

        var activeStatuses = new[]
        {
            ProviderRegistrationStatuses.IndorProActive,
            ProviderRegistrationStatuses.Approved,
            ProviderRegistrationStatuses.Submitted,
            ProviderRegistrationStatuses.PendingReview
        };

        var providers = await db.IndorProveedores
            .AsNoTracking()
            .Include(p => p.Categorias)
            .Where(p => activeStatuses.Contains(p.RegistrationStatus))
            .Where(p => p.BusinessAddress != null || p.PrimaryCity != null)
            .OrderByDescending(p => p.FechaActualizacion)
            .Take(80)
            .ToListAsync(ct);

        if (providers.Count == 0)
        {
            return [];
        }

        var categoryLabels = await db.IndorProveedorCategoriasCatalogo
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Id, c => c.LabelEn, ct);

        var results = new List<RealtorNetworkMapProviderViewModel>();
        var geocodeBudget = 6;

        foreach (var provider in providers)
        {
            if (provider.Latitude is null || provider.Longitude is null)
            {
                if (geocodeBudget <= 0)
                {
                    continue;
                }

                var tracked = await db.IndorProveedores.FirstOrDefaultAsync(p => p.Id == provider.Id, ct);
                if (tracked == null)
                {
                    continue;
                }

                await ProviderGeolocationHelper.ApplyGeocodeAsync(tracked, addressLookup, ct);
                geocodeBudget--;
                if (tracked.Latitude is null || tracked.Longitude is null)
                {
                    continue;
                }

                await db.SaveChangesAsync(ct);
                provider.Latitude = tracked.Latitude;
                provider.Longitude = tracked.Longitude;
            }

            var lat = (double)provider.Latitude!.Value;
            var lng = (double)provider.Longitude!.Value;
            var distance = CalculateDistanceMiles(centerLat, centerLng, lat, lng);
            if (distance > (decimal)radiusMiles)
            {
                continue;
            }

            var categoryId = provider.Categorias.Select(c => c.CategoriaId).FirstOrDefault();
            categoryLabels.TryGetValue(categoryId ?? "", out var categoryLabel);

            var name = !string.IsNullOrWhiteSpace(provider.DbaName)
                ? provider.DbaName.Trim()
                : provider.BusinessName?.Trim() ?? "Provider";

            var isVerified = string.Equals(provider.RegistrationStatus, ProviderRegistrationStatuses.IndorProActive, StringComparison.OrdinalIgnoreCase)
                || string.Equals(provider.RegistrationStatus, ProviderRegistrationStatuses.Approved, StringComparison.OrdinalIgnoreCase);

            results.Add(new RealtorNetworkMapProviderViewModel
            {
                ProviderId = provider.Id,
                Name = name,
                Category = string.IsNullOrWhiteSpace(categoryLabel) ? null : categoryLabel,
                Latitude = lat,
                Longitude = lng,
                DistanceMiles = (double?)distance,
                IsVerified = isVerified
            });
        }

        return results
            .OrderBy(p => p.DistanceMiles ?? double.MaxValue)
            .Take(40)
            .ToList();
    }

    private static (string PostQuickJobUrl, string BrowseHelpersUrl, string MyQuickJobsUrl, string MyRequestsUrl) BuildQuickHelpUrls(
        int propiedadId,
        IUrlHelper url)
    {
        var postUrl = url.Action("Create", "NeighborRequest", new { propiedadId }) ?? "#";
        var mineUrl = url.Action("Mine", "NeighborRequest", new { propiedadId }) ?? "#";
        var browseUrl = BuildNetworkFeedUrl(url, NearbyNetworkHomeownerFilters.NeighborRequests, null);
        return (postUrl, browseUrl, mineUrl, mineUrl);
    }

    private static List<RealtorQuickActionViewModel> BuildQuickActions(
        int? propiedadId,
        string activeFilter,
        string? searchQuery,
        IUrlHelper url)
    {
        var postUrl = propiedadId is > 0
            ? url.Action("Create", "NeighborRequest", new { propiedadId }) ?? "#"
            : url.Action("EditarPerfil", "Perfil") + "#home";
        var mineUrl = propiedadId is > 0
            ? url.Action("Mine", "NeighborRequest", new { propiedadId }) ?? "#"
            : url.Action("EditarPerfil", "Perfil") + "#home";
        var servicesUrl = (url.Action("Index", "Home") ?? "/") + "#section-services";

        return activeFilter switch
        {
            NearbyNetworkHomeownerFilters.Homes =>
            [
                new()
                {
                    Label = "Post a Request",
                    Subtitle = "Ask neighbors for help",
                    Icon = "fa-plus",
                    Url = postUrl
                },
                new()
                {
                    Label = "Certified Providers",
                    Subtitle = "Verified professionals nearby",
                    Icon = "fa-shield-halved",
                    Url = BuildNetworkFeedUrl(url, NearbyNetworkHomeownerFilters.Providers, searchQuery)
                }
            ],
            NearbyNetworkHomeownerFilters.Providers =>
            [
                new()
                {
                    Label = "Request a Service",
                    Subtitle = "Browse all home services",
                    Icon = "fa-screwdriver-wrench",
                    Url = servicesUrl
                },
                new()
                {
                    Label = "Post a Request",
                    Subtitle = "Ask neighbors for help",
                    Icon = "fa-plus",
                    Url = postUrl
                }
            ],
            NearbyNetworkHomeownerFilters.Promotions =>
            [
                new()
                {
                    Label = "View all nearby",
                    Subtitle = "Listings, services, and more",
                    Icon = "fa-border-all",
                    Url = BuildNetworkFeedUrl(url, NearbyNetworkHomeownerFilters.All, searchQuery)
                },
                new()
                {
                    Label = "Post a Request",
                    Subtitle = "Ask neighbors for help",
                    Icon = "fa-plus",
                    Url = postUrl
                }
            ],
            NearbyNetworkHomeownerFilters.Emergency =>
            [
                new()
                {
                    Label = "Emergency Help",
                    Subtitle = "24/7 urgent home problems",
                    Icon = "fa-triangle-exclamation",
                    Url = servicesUrl
                },
                new()
                {
                    Label = "View all nearby",
                    Subtitle = "See everything in your area",
                    Icon = "fa-border-all",
                    Url = BuildNetworkFeedUrl(url, NearbyNetworkHomeownerFilters.All, searchQuery)
                }
            ],
            NearbyNetworkHomeownerFilters.NeighborRequests =>
            [
                new()
                {
                    Label = "Post a Request",
                    Subtitle = "Ask neighbors for help",
                    Icon = "fa-plus",
                    Url = postUrl
                },
                new()
                {
                    Label = "My Requests",
                    Subtitle = "Track & manage your requests",
                    Icon = "fa-comment-dots",
                    Url = mineUrl
                }
            ],
            _ =>
            [
                new()
                {
                    Label = "Certified Providers",
                    Subtitle = "Verified professionals in your area",
                    Icon = "fa-shield-halved",
                    Url = BuildNetworkFeedUrl(url, NearbyNetworkHomeownerFilters.Providers, searchQuery)
                },
                new()
                {
                    Label = "Homes for sale",
                    Subtitle = "Listings near you",
                    Icon = "fa-house",
                    Url = BuildNetworkFeedUrl(url, NearbyNetworkHomeownerFilters.Homes, searchQuery)
                },
                new()
                {
                    Label = "Post a Request",
                    Subtitle = "Ask neighbors for help",
                    Icon = "fa-plus",
                    Url = postUrl
                },
                new()
                {
                    Label = "My Requests",
                    Subtitle = "Track & manage your requests",
                    Icon = "fa-comment-dots",
                    Url = mineUrl
                }
            ]
        };
    }

    private static string BuildNetworkFeedUrl(IUrlHelper url, string filter, string? searchQuery)
    {
        var path = url.Action("Index", "Home", new { filter, view = "feed", q = searchQuery }) ?? "/";
        return path + "#section-home";
    }

    private static string NormalizeActionUrl(string? actionUrl, IUrlHelper url)
    {
        if (string.IsNullOrWhiteSpace(actionUrl) || actionUrl == "#")
        {
            return "#";
        }

        return actionUrl.StartsWith('/') ? actionUrl : url.Content("~" + actionUrl);
    }

    private static decimal? ResolveItemDistanceMiles(
        IndorNearbyNetworkItem item,
        double centerLat,
        double centerLng)
    {
        if (item.Latitude is not null && item.Longitude is not null)
        {
            return CalculateDistanceMiles(
                centerLat,
                centerLng,
                (double)item.Latitude.Value,
                (double)item.Longitude.Value);
        }

        return item.DistanceMiles;
    }

    private static RealtorNetworkMapPinViewModel CreatePin(
        string label,
        string pinType,
        string iconClass,
        double lat,
        double lng,
        double centerLat,
        double centerLng,
        double radiusMiles)
    {
        var (top, left) = ProjectToMapPercent(lat, lng, centerLat, centerLng, radiusMiles);
        return new RealtorNetworkMapPinViewModel
        {
            Label = label,
            PinType = pinType,
            IconClass = iconClass,
            Latitude = lat,
            Longitude = lng,
            TopPercent = top,
            LeftPercent = left
        };
    }

    private static (string TopPercent, string LeftPercent) ProjectToMapPercent(
        double lat,
        double lng,
        double centerLat,
        double centerLng,
        double radiusMiles)
    {
        const double milesPerLatDegree = 69.0;
        var milesPerLngDegree = 69.0 * Math.Cos(centerLat * Math.PI / 180.0);
        var latMiles = (lat - centerLat) * milesPerLatDegree;
        var lngMiles = (lng - centerLng) * milesPerLngDegree;
        var scale = 38.0 / Math.Max(radiusMiles, 0.1);

        var top = 50.0 - latMiles * scale;
        var left = 50.0 + lngMiles * scale;
        top = Math.Clamp(top, 8, 92);
        left = Math.Clamp(left, 8, 92);

        return ($"{top:0.#}%", $"{left:0.#}%");
    }

    private static decimal? CalculateDistanceMiles(double centerLat, double centerLng, double lat, double lng)
    {
        const double earthRadiusMiles = 3958.8;
        static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

        var dLat = ToRadians(lat - centerLat);
        var dLng = ToRadians(lng - centerLng);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(centerLat)) * Math.Cos(ToRadians(lat)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return (decimal)Math.Round(earthRadiusMiles * c, 1);
    }

    private static string NormalizeFilter(string? filter) =>
        filter?.Trim() switch
        {
            NearbyNetworkHomeownerFilters.Homes or "Listings" => NearbyNetworkHomeownerFilters.Homes,
            NearbyNetworkHomeownerFilters.Providers or "Services" => NearbyNetworkHomeownerFilters.Providers,
            NearbyNetworkHomeownerFilters.Promotions => NearbyNetworkHomeownerFilters.Promotions,
            NearbyNetworkHomeownerFilters.Emergency => NearbyNetworkHomeownerFilters.Emergency,
            "NeighborRequests" or "Neighbor Requests" => NearbyNetworkHomeownerFilters.NeighborRequests,
            _ => NearbyNetworkHomeownerFilters.All
        };

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

    private static string? BuildSpecsLabel(decimal? beds, decimal? baths, int? sqft)
    {
        if (beds is null && baths is null && sqft is null)
        {
            return null;
        }

        var parts = new List<string>();
        if (beds is > 0)
        {
            parts.Add($"{beds:0.#} bd");
        }

        if (baths is > 0)
        {
            parts.Add($"{baths:0.#} ba");
        }

        if (sqft is > 0)
        {
            parts.Add($"{sqft:N0} sqft");
        }

        return string.Join(" • ", parts);
    }

    private static string FormatCurrency(decimal value) =>
        string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C0}", value);

    private static string FormatRelativeTime(DateTime createdUtc)
    {
        var elapsed = DateTime.UtcNow - createdUtc;
        if (elapsed.TotalMinutes < 60)
        {
            var mins = Math.Max(1, (int)elapsed.TotalMinutes);
            return $"Posted {mins} min{(mins == 1 ? "" : "s")} ago";
        }

        if (elapsed.TotalHours < 24)
        {
            var hours = Math.Max(1, (int)elapsed.TotalHours);
            return $"Posted {hours} hour{(hours == 1 ? "" : "s")} ago";
        }

        var days = Math.Max(1, (int)elapsed.TotalDays);
        return $"Posted {days} day{(days == 1 ? "" : "s")} ago";
    }

    private static List<HomeownerMapCarouselItemViewModel> BuildMapCarouselItems(
        double centerLat,
        double centerLng,
        IReadOnlyList<RealtorNetworkMapProviderViewModel> providers,
        IReadOnlyList<IndorNearbyNetworkItem> mapItems,
        int propiedadId,
        IUrlHelper url,
        string servicesUrl,
        string messageUrl)
    {
        var items = new List<HomeownerMapCarouselItemViewModel>();

        foreach (var provider in providers)
        {
            items.Add(new HomeownerMapCarouselItemViewModel
            {
                Id = $"provider-{provider.ProviderId}",
                ItemType = "provider",
                BadgeLabel = "PROVIDER WORKING NEARBY",
                Title = provider.Name,
                Subtitle = provider.IsVerified ? "Working in your neighborhood today." : provider.Category,
                IconClass = "fa-leaf",
                Latitude = provider.Latitude,
                Longitude = provider.Longitude,
                DistanceMiles = provider.DistanceMiles,
                Tags = provider.IsVerified
                    ? ["Verified Provider", "Insurance Active"]
                    : [],
                IsVerified = provider.IsVerified,
                PrimaryActionLabel = "Message in INDOR",
                PrimaryActionUrl = messageUrl,
                SecondaryActionLabel = "Request Service",
                SecondaryActionUrl = servicesUrl
            });
        }

        foreach (var item in mapItems)
        {
            if (item.Latitude is null || item.Longitude is null)
            {
                continue;
            }

            var lat = (double)item.Latitude.Value;
            var lng = (double)item.Longitude.Value;
            var distance = CalculateDistanceMiles(centerLat, centerLng, lat, lng);
            var itemType = item.CardType.ToLowerInvariant() switch
            {
                "openhouse" or "listing" => "home",
                "promotion" => "promotion",
                "emergency" => "emergency",
                _ => "home"
            };

            var card = MapNetworkItemToCard(item, distance, propiedadId, url);
            items.Add(new HomeownerMapCarouselItemViewModel
            {
                Id = $"network-{item.Id}",
                ItemType = itemType,
                BadgeLabel = string.IsNullOrWhiteSpace(item.BadgeLabel) ? itemType.ToUpperInvariant() : item.BadgeLabel,
                Title = card.Title,
                Subtitle = card.Subtitle ?? card.SpecsLabel,
                ImageUrl = card.ImageUrl,
                IconClass = card.IconClass ?? (itemType switch
                {
                    "emergency" => "fa-triangle-exclamation",
                    "promotion" => "fa-tags",
                    _ => "fa-house"
                }),
                Latitude = lat,
                Longitude = lng,
                DistanceMiles = (double?)distance,
                MetaLabel = card.MetaLabel,
                Tags = card.Tags,
                PrimaryActionLabel = card.PrimaryActionLabel,
                PrimaryActionUrl = card.PrimaryActionUrl,
                SecondaryActionLabel = card.SecondaryActionLabel,
                SecondaryActionUrl = card.SecondaryActionUrl
            });
        }

        return items
            .OrderBy(i => i.DistanceMiles ?? double.MaxValue)
            .ToList();
    }

    private sealed class FeedCardSortable
    {
        public int SortOrder { get; init; }
        public DateTime CreatedUtc { get; init; }
        public RealtorNetworkFeedCardViewModel Card { get; init; } = null!;
    }
}
