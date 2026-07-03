using System.Globalization;
using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IndorMvcApp.Services;

public class RealtorNearbyNetworkService(
    AppDbContext db,
    RealtorPortalService portalService,
    IAddressLookupService addressLookup,
    IOptions<GoogleMapsOptions> googleMapsOptions,
    IWebHostEnvironment webHostEnvironment)
{
    private readonly GoogleMapsOptions _googleMaps = googleMapsOptions.Value;
    private const long MaxListingPhotoPdfBytes = 15 * 1024 * 1024;
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

        const bool useDeviceLocation = true;
        var centerLat = ToDouble(settings.CenterLatitude, _googleMaps.DefaultLatitude);
        var centerLng = ToDouble(settings.CenterLongitude, _googleMaps.DefaultLongitude);
        var radiusMiles = (double)settings.RadiusMiles;
        var mapProviders = await LoadNearbyProvidersAsync(centerLat, centerLng, radiusMiles, activeFilter, ct);
        var mapPins = BuildMapPins(settings, items, useDeviceLocation);
        var listingPinCount = mapPins.Count;

        return new RealtorNearbyNetworkViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            RecentNotifications = shell.RecentNotifications,
            ActiveView = activeView,
            ActiveFilter = activeFilter,
            SearchQuery = search,
            Filters = FilterChips,
            RadiusLabel = BuildRadiusLabel(settings, useDeviceLocation),
            QuickActions = BuildQuickActions(),
            FeedCards = cards,
            MapPins = mapPins,
            MapProviders = mapProviders,
            MapCenterLabel = useDeviceLocation ? "Your location" : settings.CenterLabel,
            CenterLatitude = ToDouble(settings.CenterLatitude, _googleMaps.DefaultLatitude),
            CenterLongitude = ToDouble(settings.CenterLongitude, _googleMaps.DefaultLongitude),
            RadiusMiles = (double)settings.RadiusMiles,
            GoogleMapsApiKey = _googleMaps.BrowserApiKey,
            UseDeviceLocation = useDeviceLocation,
            MapNearbyCount = mapProviders.Count + listingPinCount
        };
    }

    public async Task<RealtorNetworkMapDataViewModel?> GetMapDataAsync(
        IndorRealtor realtor,
        double? lat,
        double? lng,
        string? addressQuery,
        string? filter,
        CancellationToken ct = default)
    {
        var settings = await EnsureSettingsAsync(realtor, ct);
        var activeFilter = NormalizeFilter(filter);
        var radiusMiles = (double)settings.RadiusMiles;
        var centerLabel = "Your location";

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
            lat = ToDouble(settings.CenterLatitude, _googleMaps.DefaultLatitude);
            lng = ToDouble(settings.CenterLongitude, _googleMaps.DefaultLongitude);
            centerLabel = settings.CenterLabel;
        }

        var providers = await LoadNearbyProvidersAsync(lat.Value, lng.Value, radiusMiles, activeFilter, ct);
        var listings = await LoadMapListingsNearAsync(realtor, lat.Value, lng.Value, radiusMiles, activeFilter, ct);

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

    private async Task<List<RealtorNetworkMapPinViewModel>> LoadMapListingsNearAsync(
        IndorRealtor realtor,
        double centerLat,
        double centerLng,
        double radiusMiles,
        string activeFilter,
        CancellationToken ct)
    {
        if (activeFilter is not ("All" or "Homes" or "OpenHouses" or "Leads" or "Promotions" or "Emergency"))
        {
            return [];
        }

        var settings = await EnsureSettingsAsync(realtor, ct);
        var items = await LoadItemsAsync(realtor, settings, activeFilter, search: null, mineOnly: false, ct);

        return items
            .Where(i => i.Latitude is not null && i.Longitude is not null)
            .Select(i =>
            {
                var lat = (double)i.Latitude!.Value;
                var lng = (double)i.Longitude!.Value;
                var distance = CalculateDistanceMiles(centerLat, centerLng, lat, lng);
                return new { Item = i, Lat = lat, Lng = lng, Distance = distance };
            })
            .Where(x => x.Distance <= (decimal)radiusMiles)
            .OrderBy(x => x.Distance)
            .Take(25)
            .Select(x =>
            {
                var pinType = x.Item.CardType.ToLowerInvariant() switch
                {
                    "listing" or "openhouse" => "home",
                    "lead" => "lead",
                    "provider" or "promotion" => "provider",
                    "emergency" => "emergency",
                    _ => "home"
                };

                return new RealtorNetworkMapPinViewModel
                {
                    Label = x.Item.Title,
                    PinType = pinType,
                    Latitude = x.Lat,
                    Longitude = x.Lng
                };
            })
            .ToList();
    }

    public async Task<RealtorNetworkListingFormViewModel?> BuildListingFormAsync(
        IndorRealtor realtor,
        int? itemId,
        CancellationToken ct = default)
    {
        var shell = await portalService.BuildShellAsync(realtor, ct);
        var settings = await EnsureSettingsAsync(realtor, ct);
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
            RecentNotifications = shell.RecentNotifications,
            ItemId = item?.Id,
            IsEdit = itemId is > 0,
            Title = item?.Title ?? "",
            Address = item?.Subtitle ?? "",
            Price = item?.Price,
            Bedrooms = item?.Bedrooms,
            Bathrooms = item?.Bathrooms,
            SquareFeet = item?.SquareFeet,
            ImageUrl = item?.ImageUrl ?? "/inspeccion2.jpeg",
            StatusBadge = item?.StatusBadge ?? "ACTIVE",
            IsOpenHouse = item?.CardType == NearbyNetworkCardTypes.OpenHouse || itemId is null,
            OpenHouseMeta = item?.MetaLabel,
            VisibilityRadiusMiles = settings.RadiusMiles
        };

        if (item != null)
        {
            ApplyListingExtras(model, ParseListingExtras(item.TagsJson));
            if (item.Latitude is not null && item.Longitude is not null)
            {
                model.AddressLatitude = (double)item.Latitude.Value;
                model.AddressLongitude = (double)item.Longitude.Value;
            }
        }
        else
        {
            model.PromoteInNearbyFeed = true;
            model.PromoteOpenHouseProgram = true;
        }

        if (item?.Price is > 0 && string.IsNullOrWhiteSpace(model.Title))
        {
            model.Title = FormatCurrency(item.Price.Value);
        }

        return model;
    }

    public async Task<RealtorNetworkListingDetailViewModel?> BuildListingDetailAsync(
        IndorRealtor realtor,
        int itemId,
        CancellationToken ct = default)
    {
        var shell = await portalService.BuildShellAsync(realtor, ct);
        var settings = await EnsureSettingsAsync(realtor, ct);

        var item = await db.IndorNearbyNetworkItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == itemId, ct);

        if (item == null || !CanViewListing(item, realtor.Id, settings))
        {
            return null;
        }

        var extras = ParseListingExtras(item.TagsJson);
        var isOwned = item.OwnerRealtorId == realtor.Id && item.IsOwnedListing;
        var isOpenHouse = item.CardType == NearbyNetworkCardTypes.OpenHouse;
        var photos = BuildPhotoList(item.ImageUrl, extras.AdditionalPhotoUrls);
        var highlightLabels = ResolveHighlightLabels(extras.Highlights);

        return new RealtorNetworkListingDetailViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            RecentNotifications = shell.RecentNotifications,
            ItemId = item.Id,
            IsOwned = isOwned,
            IsOpenHouse = isOpenHouse,
            ListingBadgeLabel = ResolveListingBadgeLabel(item, isOwned),
            ListingBadgeCss = item.BadgeCss,
            StatusBadge = item.StatusBadge ?? "",
            StatusCss = item.StatusCss,
            PriceLabel = item.Price is > 0 ? FormatCurrency(item.Price.Value) : item.Title,
            Title = item.Title,
            Address = item.Subtitle ?? "",
            SpecsLabel = item.SpecsLabel,
            DistanceLabel = item.DistanceMiles is > 0 ? $"{item.DistanceMiles:0.#} mi away" : null,
            ListingTypeLabel = extras.ListingType == "rent" ? "For Rent" : "For Sale",
            PropertySubtypeLabel = ResolvePropertySubtypeLabel(extras.PropertySubtype),
            Description = extras.Description,
            OpenHouseMeta = item.MetaLabel,
            PhotoUrls = photos,
            HighlightLabels = highlightLabels,
            PhotoGalleryLink = extras.PhotoGalleryLink,
            PhotoPdfUrl = extras.PhotoPdfUrl,
            PhotoPdfFileName = extras.PhotoPdfFileName,
            Latitude = item.Latitude is not null ? (double)item.Latitude.Value : null,
            Longitude = item.Longitude is not null ? (double)item.Longitude.Value : null,
            GoogleMapsApiKey = _googleMaps.BrowserApiKey,
            Bedrooms = item.Bedrooms,
            Bathrooms = item.Bathrooms,
            SquareFeet = item.SquareFeet,
            YearBuilt = extras.YearBuilt,
            ExpressInterestUrl = isOwned ? "#" : $"/Realtor/ExpressNetworkInterest/{item.Id}"
        };
    }

    public async Task<RealtorNetworkLeadDetailViewModel?> BuildLeadDetailAsync(
        IndorRealtor realtor,
        int itemId,
        bool showContact,
        bool interestRecorded,
        CancellationToken ct = default)
    {
        var shell = await portalService.BuildShellAsync(realtor, ct);

        var item = await db.IndorNearbyNetworkItems.AsNoTracking()
            .Include(i => i.RelatedClient)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.IsActive, ct);

        if (item == null ||
            !string.Equals(item.CardType, NearbyNetworkCardTypes.Lead, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var client = item.RelatedClient;
        var contactUrl = BuildLeadContactUrl(client?.Email, client?.FullName ?? item.Title, item.Subtitle);
        var canContact = !string.IsNullOrWhiteSpace(contactUrl);

        return new RealtorNetworkLeadDetailViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            RecentNotifications = shell.RecentNotifications,
            ItemId = item.Id,
            Title = item.Title,
            Subtitle = item.Subtitle,
            SpecsLabel = item.SpecsLabel,
            MetaLabel = item.MetaLabel,
            StatusBadge = item.StatusBadge ?? "LEAD REQUEST",
            StatusCss = item.StatusCss ?? "lead",
            DistanceLabel = item.DistanceMiles is > 0 ? $"{item.DistanceMiles:0.#} mi away" : "Nearby",
            ShowContactPanel = showContact,
            RelatedClientId = item.RelatedClientId,
            BuyerName = client?.FullName,
            BuyerEmail = client?.Email,
            ContactUrl = contactUrl,
            ContactActionLabel = canContact ? "Email Buyer" : "Contact Buyer",
            CanContactBuyer = canContact || item.RelatedClientId is > 0,
            InterestRecorded = interestRecorded
        };
    }

    public async Task<bool> RecordListingInterestAsync(
        IndorRealtor realtor, int itemId, CancellationToken ct = default)
    {
        var item = await db.IndorNearbyNetworkItems.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == itemId && i.IsActive, ct);

        if (item == null ||
            item.OwnerRealtorId == realtor.Id ||
            item.CardType is not (NearbyNetworkCardTypes.Listing or NearbyNetworkCardTypes.OpenHouse))
        {
            return false;
        }

        var address = item.Subtitle ?? item.Title;
        var now = DateTime.UtcNow;

        db.IndorRealtorActivities.Add(new IndorRealtorActivity
        {
            RealtorId = realtor.Id,
            ActivityType = "network",
            Description = $"You expressed interest in {address}",
            CategoryTag = "Nearby",
            OccurredUtc = now
        });

        if (item.OwnerRealtorId is > 0 && item.OwnerRealtorId != realtor.Id)
        {
            db.IndorRealtorActivities.Add(new IndorRealtorActivity
            {
                RealtorId = item.OwnerRealtorId.Value,
                ActivityType = "network",
                Description = $"{realtor.DisplayName ?? "A realtor"} is interested in your listing at {address}",
                CategoryTag = "Nearby",
                OccurredUtc = now
            });
        }

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RecordLeadContactAsync(
        IndorRealtor realtor, int itemId, CancellationToken ct = default)
    {
        var item = await db.IndorNearbyNetworkItems.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == itemId && i.IsActive, ct);

        if (item == null ||
            !string.Equals(item.CardType, NearbyNetworkCardTypes.Lead, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        db.IndorRealtorActivities.Add(new IndorRealtorActivity
        {
            RealtorId = realtor.Id,
            ActivityType = "lead",
            Description = $"Contact initiated for lead: {item.Title}",
            CategoryTag = "Leads",
            OccurredUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<(string? Url, string? Error)> SaveListingPhotoPdfAsync(
        int realtorId,
        IFormFile file,
        string? previousPdfUrl,
        CancellationToken ct = default)
    {
        if (file.Length == 0)
        {
            return (null, null);
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf")
        {
            return (null, "Only PDF files are allowed for photo brochures.");
        }

        if (file.Length > MaxListingPhotoPdfBytes)
        {
            return (null, "PDF must be 15 MB or smaller.");
        }

        var folder = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "nearby-listings", realtorId.ToString());
        Directory.CreateDirectory(folder);

        var storedName = $"photos-{Guid.NewGuid():N}.pdf";
        var physicalPath = Path.Combine(folder, storedName);
        await using (var stream = File.Create(physicalPath))
        {
            await file.CopyToAsync(stream, ct);
        }

        DeleteStoredListingFile(previousPdfUrl);
        return ($"/uploads/nearby-listings/{realtorId}/{storedName}", null);
    }

    public void DeleteStoredListingFile(string? relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl) || !relativeUrl.StartsWith("/uploads/nearby-listings/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var relativePath = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(webHostEnvironment.WebRootPath, relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    public async Task<RealtorNetworkListingWizardViewModel> BuildListingWizardShellAsync(
        IndorRealtor realtor,
        int displayStep,
        string title,
        string? subtitle,
        bool showStepper,
        CancellationToken ct = default)
    {
        var shell = await portalService.BuildShellAsync(realtor, ct);
        return new RealtorNetworkListingWizardViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            RecentNotifications = shell.RecentNotifications,
            DisplayStep = displayStep,
            TotalSteps = 4,
            Title = title,
            Subtitle = subtitle,
            ShowStepper = showStepper
        };
    }

    public async Task<int?> SaveListingAsync(IndorRealtor realtor, RealtorNetworkListingFormViewModel model, CancellationToken ct = default)
    {
        var settings = await EnsureSettingsAsync(realtor, ct);

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

        var isOpenHouse = model.IsOpenHouse || model.PromoteOpenHouseProgram;
        item.CardType = isOpenHouse ? NearbyNetworkCardTypes.OpenHouse : NearbyNetworkCardTypes.Listing;
        item.FilterCategory = isOpenHouse
            ? NearbyNetworkFilterCategories.OpenHouses
            : NearbyNetworkFilterCategories.Homes;
        item.BadgeLabel = isOpenHouse ? "OPEN HOUSE" : model.ListingType == "rent" ? "FOR RENT" : "MY LISTING";
        item.BadgeCss = isOpenHouse ? "openhouse" : "listing";
        item.Title = string.IsNullOrWhiteSpace(model.Title)
            ? FormatCurrency(model.Price ?? 0)
            : model.Title.Trim();
        item.Subtitle = model.Address.Trim();
        item.Price = model.Price;
        item.Bedrooms = model.Bedrooms;
        item.Bathrooms = model.Bathrooms;
        item.SquareFeet = model.SquareFeet;
        item.SpecsLabel = BuildSpecsLabel(model.Bedrooms, model.Bathrooms, model.SquareFeet, model.YearBuilt);
        item.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? "/inspeccion2.jpeg" : model.ImageUrl.Trim();
        item.MetaLabel = isOpenHouse ? model.OpenHouseMeta?.Trim() : null;
        item.StatusBadge = model.SaveAsDraft
            ? "DRAFT"
            : string.IsNullOrWhiteSpace(model.StatusBadge) ? "ACTIVE" : model.StatusBadge.Trim().ToUpperInvariant();
        item.StatusCss = model.SaveAsDraft ? "draft" : isOpenHouse ? "openhouse" : "active";
        item.PrimaryActionLabel = isOpenHouse ? "View Details" : "View Home";
        item.SecondaryActionLabel = isOpenHouse ? "Share" : "Edit Listing";
        item.IsActive = !model.SaveAsDraft;

        var previousExtras = model.ItemId is > 0 ? ParseListingExtras(item.TagsJson) : null;
        if (!string.IsNullOrWhiteSpace(previousExtras?.PhotoPdfUrl) && string.IsNullOrWhiteSpace(model.PhotoPdfUrl))
        {
            DeleteStoredListingFile(previousExtras.PhotoPdfUrl);
        }

        item.TagsJson = SerializeListingExtras(model);
        item.UpdatedUtc = DateTime.UtcNow;

        if (model.VisibilityRadiusMiles is > 0 && model.VisibilityRadiusMiles != settings.RadiusMiles)
        {
            settings.RadiusMiles = model.VisibilityRadiusMiles;
            settings.FechaActualizacion = DateTime.UtcNow;
        }

        if (model.AddressLatitude is double lat && model.AddressLongitude is double lng)
        {
            item.Latitude = (decimal)lat;
            item.Longitude = (decimal)lng;
            item.DistanceMiles = CalculateDistanceMiles(
                ToDouble(settings.CenterLatitude, _googleMaps.DefaultLatitude),
                ToDouble(settings.CenterLongitude, _googleMaps.DefaultLongitude),
                lat,
                lng);
        }
        else
        {
            var coordinates = await addressLookup.GeocodeAddressAsync(model.Address.Trim(), ct);
            if (coordinates is { } coords)
            {
                item.Latitude = coords.Latitude;
                item.Longitude = coords.Longitude;
                item.DistanceMiles = CalculateDistanceMiles(
                    ToDouble(settings.CenterLatitude, _googleMaps.DefaultLatitude),
                    ToDouble(settings.CenterLongitude, _googleMaps.DefaultLongitude),
                    (double)coords.Latitude,
                    (double)coords.Longitude);
            }
            else
            {
                item.Latitude = null;
                item.Longitude = null;
                item.DistanceMiles = null;
            }
        }

        await db.SaveChangesAsync(ct);

        item.PrimaryActionUrl = $"/Realtor/ViewNetworkListing/{item.Id}";
        item.SecondaryActionUrl = isOpenHouse
            ? ListingViewUrl(item.Id)
            : $"/Realtor/EditNetworkListing/{item.Id}";
        await db.SaveChangesAsync(ct);

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

        var centerLabel = string.IsNullOrWhiteSpace(realtor.ServiceAreas)
            ? "Your service area"
            : realtor.ServiceAreas.Trim();
        var centerAddress = realtor.ServiceAreas?.Trim();
        decimal? centerLat = null;
        decimal? centerLng = null;

        if (!string.IsNullOrWhiteSpace(centerAddress))
        {
            var coordinates = await addressLookup.GeocodeAddressAsync(centerAddress, ct);
            if (coordinates is { } coords)
            {
                centerLat = coords.Latitude;
                centerLng = coords.Longitude;
            }
        }

        settings = new IndorNearbyNetworkSetting
        {
            RealtorId = realtor.Id,
            CenterLabel = centerLabel,
            CenterAddress = centerAddress,
            CenterLatitude = centerLat ?? (decimal)_googleMaps.DefaultLatitude,
            CenterLongitude = centerLng ?? (decimal)_googleMaps.DefaultLongitude,
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
            .Where(i => mineOnly || i.IsActive);

        if (mineOnly)
        {
            query = query.Where(i => i.OwnerRealtorId == realtor.Id && i.IsOwnedListing);
        }
        else
        {
            query = query.Where(i =>
                i.OwnerRealtorId == realtor.Id ||
                i.DistanceMiles == null ||
                i.DistanceMiles <= settings.RadiusMiles);
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

    private static (string? Primary, string? Secondary) ResolveFeedActionUrls(
        IndorNearbyNetworkItem item,
        string cardType,
        bool isOwned)
    {
        switch (cardType)
        {
            case "lead":
                return (
                    $"/Realtor/ViewNetworkLead/{item.Id}",
                    item.RelatedClientId is > 0
                        ? $"/Realtor/ClientDetail/{item.RelatedClientId}"
                        : $"/Realtor/ContactNetworkLead/{item.Id}");

            case "provider":
            case "promotion":
            {
                var providerSearch = !string.IsNullOrWhiteSpace(item.ProviderName)
                    ? item.ProviderName.Trim()
                    : item.Subtitle?.Trim();
                var providerUrl = string.IsNullOrWhiteSpace(providerSearch)
                    ? "/Realtor/ProviderNetwork"
                    : $"/Realtor/ProviderNetwork?q={Uri.EscapeDataString(providerSearch)}";

                var primary = NeedsActionUrl(item.PrimaryActionUrl) ? providerUrl : item.PrimaryActionUrl;
                var secondary = NeedsActionUrl(item.SecondaryActionUrl) ? "/Realtor/Quotes" : item.SecondaryActionUrl;
                return (primary, secondary);
            }

            case "emergency":
                return (
                    NeedsActionUrl(item.PrimaryActionUrl)
                        ? "/Realtor/ProviderNetwork?filter=Verified"
                        : item.PrimaryActionUrl,
                    null);

            case "openhouse":
                return (
                    ListingViewUrl(item.Id),
                    ListingViewUrl(item.Id));

            default:
                if (isOwned)
                {
                    return (
                        ListingViewUrl(item.Id),
                        $"/Realtor/EditNetworkListing/{item.Id}");
                }

                return (
                    ListingViewUrl(item.Id),
                    $"/Realtor/ExpressNetworkInterest/{item.Id}");
        }
    }

    private static (string Primary, string? Secondary) ResolveFeedActionLabels(
        IndorNearbyNetworkItem item,
        string cardType,
        bool isOwned) =>
        cardType switch
        {
            "openhouse" => ("View Details", "Share"),
            "lead" => ("View Lead", "Contact Buyer"),
            "provider" => ("View Offer", null),
            "promotion" => ("Ask for Info", "Request Service"),
            "emergency" => ("Call Now", null),
            _ when isOwned => ("View Home", "Edit Listing"),
            _ => ("View Listing", "I'm Interested")
        };

    private static bool NeedsActionUrl(string? url) =>
        string.IsNullOrWhiteSpace(url) || url == "#";

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
        var (primaryUrl, secondaryUrl) = ResolveFeedActionUrls(item, cardType, isOwned);
        var (primaryLabel, secondaryLabel) = ResolveFeedActionLabels(item, cardType, isOwned);
        var badgeLabel = ResolveListingBadgeLabel(item, isOwned);

        var imageUrl = NearbyNetworkImageResolver.ResolveFeedImage(item);
        if (cardType is "lead" or "emergency")
        {
            imageUrl = null;
        }

        return new RealtorNetworkFeedCardViewModel
        {
            ItemId = item.Id,
            CardType = cardType,
            BadgeLabel = badgeLabel,
            BadgeCss = item.BadgeCss,
            ImageUrl = imageUrl,
            IconClass = NearbyNetworkImageResolver.ResolveIconClass(item, imageUrl),
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
            PrimaryActionLabel = primaryLabel,
            PrimaryActionUrl = primaryUrl,
            SecondaryActionLabel = secondaryLabel,
            SecondaryActionUrl = secondaryUrl,
            SecondaryActionOpensShare = cardType == "openhouse"
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

    private static string BuildRadiusLabel(IndorNearbyNetworkSetting settings, bool useDeviceLocation)
    {
        var radius = settings.RadiusMiles.ToString("0.#", CultureInfo.InvariantCulture);
        return useDeviceLocation
            ? $"{radius} miles around you"
            : $"{radius} miles around {settings.CenterLabel}";
    }

    private async Task<List<RealtorNetworkMapProviderViewModel>> LoadNearbyProvidersAsync(
        double centerLat,
        double centerLng,
        double radiusMiles,
        string activeFilter,
        CancellationToken ct)
    {
        if (activeFilter is not ("All" or "Providers"))
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

    private static List<RealtorQuickActionViewModel> BuildQuickActions() =>
    [
        new() { Label = "Post Listing", Subtitle = "List a property", Icon = "fa-plus", Url = "/Realtor/CreateNetworkListing" },
        new() { Label = "My Leads", Subtitle = "View leads", Icon = "fa-users", Url = "/Realtor/Network?filter=Leads" },
        new() { Label = "My Listings", Subtitle = "Manage listings", Icon = "fa-house", Url = "/Realtor/Network?filter=Homes&scope=mine" },
        new() { Label = "My Realtor Profile", Subtitle = "View my profile", Icon = "fa-user", Url = "/Realtor/Profile" }
    ];

    private static List<RealtorNetworkMapPinViewModel> BuildMapPins(
        IndorNearbyNetworkSetting settings,
        IReadOnlyList<IndorNearbyNetworkItem> items,
        bool useDeviceLocation)
    {
        var centerLat = ToDouble(settings.CenterLatitude, 35.2271);
        var centerLng = ToDouble(settings.CenterLongitude, -80.8431);
        var radiusMiles = (double)settings.RadiusMiles;

        var pins = new List<RealtorNetworkMapPinViewModel>();
        if (!useDeviceLocation)
        {
            pins.Add(CreatePin("You", "you", "fa-location-crosshairs", centerLat, centerLng, centerLat, centerLng, radiusMiles));
        }

        foreach (var item in items.Where(i => i.Latitude is not null && i.Longitude is not null).Take(25))
        {
            var pinType = item.CardType.ToLowerInvariant() switch
            {
                "listing" or "openhouse" => "home",
                "lead" => "lead",
                "provider" or "promotion" => "provider",
                "emergency" => "emergency",
                _ => "home"
            };

            var iconClass = pinType switch
            {
                "you" => "fa-location-crosshairs",
                "lead" => "fa-user",
                "provider" => "fa-screwdriver-wrench",
                "emergency" => "fa-bell",
                _ => "fa-house"
            };

            pins.Add(CreatePin(
                item.Title,
                pinType,
                iconClass,
                (double)item.Latitude!.Value,
                (double)item.Longitude!.Value,
                centerLat,
                centerLng,
                radiusMiles));
        }

        return pins;
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

    private static double ToDouble(decimal? value, double fallback) =>
        value is decimal d ? (double)d : fallback;

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
            "Homes" => "Homes",
            "Providers" => "Providers",
            "Promotions" => "Promotions",
            "Emergency" => "Emergency",
            "OpenHouses" or "Open Houses" => "OpenHouses",
            "Leads" => "Leads",
            _ => "All"
        };

    private static string? BuildSpecsLabel(decimal? beds, decimal? baths, int? sqft, int? yearBuilt = null)
    {
        if (beds is null && baths is null && sqft is null && yearBuilt is null)
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

        if (yearBuilt is > 0)
        {
            parts.Add($"Built {yearBuilt}");
        }

        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }

    private sealed class ListingExtrasPayload
    {
        public string ListingType { get; set; } = "sale";
        public string? PropertySubtype { get; set; }
        public int? YearBuilt { get; set; }
        public string? Description { get; set; }
        public string? Highlights { get; set; }
        public bool PromoteInNearbyFeed { get; set; } = true;
        public bool FeaturedListing { get; set; }
        public string? AdditionalPhotoUrls { get; set; }
        public string? PhotoGalleryLink { get; set; }
        public string? PhotoPdfUrl { get; set; }
        public string? PhotoPdfFileName { get; set; }
    }

    private static ListingExtrasPayload ParseListingExtras(string? tagsJson)
    {
        if (string.IsNullOrWhiteSpace(tagsJson))
        {
            return new ListingExtrasPayload();
        }

        try
        {
            return JsonSerializer.Deserialize<ListingExtrasPayload>(tagsJson) ?? new ListingExtrasPayload();
        }
        catch
        {
            return new ListingExtrasPayload();
        }
    }

    private static void ApplyListingExtras(RealtorNetworkListingFormViewModel model, ListingExtrasPayload extras)
    {
        model.ListingType = string.IsNullOrWhiteSpace(extras.ListingType) ? "sale" : extras.ListingType;
        model.PropertySubtype = extras.PropertySubtype;
        model.YearBuilt = extras.YearBuilt;
        model.Description = extras.Description;
        model.Highlights = extras.Highlights;
        model.PromoteInNearbyFeed = extras.PromoteInNearbyFeed;
        model.FeaturedListing = extras.FeaturedListing;
        model.AdditionalPhotoUrls = extras.AdditionalPhotoUrls;
        model.PhotoGalleryLink = extras.PhotoGalleryLink;
        model.PhotoPdfUrl = extras.PhotoPdfUrl;
        model.PhotoPdfFileName = extras.PhotoPdfFileName;
    }

    private static string SerializeListingExtras(RealtorNetworkListingFormViewModel model) =>
        JsonSerializer.Serialize(new ListingExtrasPayload
        {
            ListingType = model.ListingType,
            PropertySubtype = model.PropertySubtype,
            YearBuilt = model.YearBuilt,
            Description = model.Description?.Trim(),
            Highlights = model.Highlights?.Trim(),
            PromoteInNearbyFeed = model.PromoteInNearbyFeed,
            FeaturedListing = model.FeaturedListing,
            AdditionalPhotoUrls = model.AdditionalPhotoUrls?.Trim(),
            PhotoGalleryLink = model.PhotoGalleryLink?.Trim(),
            PhotoPdfUrl = model.PhotoPdfUrl?.Trim(),
            PhotoPdfFileName = model.PhotoPdfFileName?.Trim()
        });

    private static string FormatCurrency(decimal amount) =>
        amount.ToString("C0", CultureInfo.GetCultureInfo("en-US"));

    private static string ListingViewUrl(int itemId) => $"/Realtor/ViewNetworkListing/{itemId}";

    private static string? BuildLeadContactUrl(string? email, string buyerName, string? area)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var greeting = buyerName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "there";
        var subject = Uri.EscapeDataString("INDOR buyer lead follow-up");
        var body = Uri.EscapeDataString(
            $"Hi {greeting}, I saw your home search request{(string.IsNullOrWhiteSpace(area) ? "" : $" for {area}")} on INDOR Nearby and would love to help.");
        return $"mailto:{email.Trim()}?subject={subject}&body={body}";
    }

    private static string ResolveListingBadgeLabel(IndorNearbyNetworkItem item, bool isOwned) =>
        !isOwned && string.Equals(item.BadgeLabel, "MY LISTING", StringComparison.OrdinalIgnoreCase)
            ? "HOME FOR SALE"
            : item.BadgeLabel;

    private static bool CanViewListing(IndorNearbyNetworkItem item, int realtorId, IndorNearbyNetworkSetting settings)
    {
        if (!item.IsActive && item.OwnerRealtorId != realtorId)
        {
            return false;
        }

        if (item.OwnerRealtorId == realtorId)
        {
            return true;
        }

        return item.DistanceMiles == null || item.DistanceMiles <= settings.RadiusMiles;
    }

    private static List<string> BuildPhotoList(string? coverUrl, string? additionalUrls)
    {
        var photos = new List<string>();
        if (!string.IsNullOrWhiteSpace(coverUrl))
        {
            photos.Add(coverUrl.Trim());
        }

        if (!string.IsNullOrWhiteSpace(additionalUrls))
        {
            foreach (var url in additionalUrls.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!photos.Contains(url, StringComparer.OrdinalIgnoreCase))
                {
                    photos.Add(url);
                }
            }
        }

        return photos;
    }

    private static List<string> ResolveHighlightLabels(string? highlightsCsv)
    {
        if (string.IsNullOrWhiteSpace(highlightsCsv))
        {
            return [];
        }

        var labels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["updated-kitchen"] = "Updated Kitchen",
            ["big-backyard"] = "Big Backyard",
            ["new-roof"] = "New Roof",
            ["move-in-ready"] = "Move-in Ready",
            ["corner-lot"] = "Corner Lot",
            ["gated-community"] = "Gated Community"
        };

        return highlightsCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => labels.TryGetValue(value, out var label) ? label : value)
            .ToList();
    }

    private static string? ResolvePropertySubtypeLabel(string? subtype) =>
        subtype switch
        {
            "single-family" => "Single Family",
            "townhouse" => "Townhouse",
            "condo" => "Condo",
            "multi-family" => "Multi-Family",
            "land" => "Land / Lot",
            "other" => "Other",
            _ => null
        };
}
