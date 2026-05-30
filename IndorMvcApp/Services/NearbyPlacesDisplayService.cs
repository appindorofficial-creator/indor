using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class NearbyPlacesDisplayService
{
    private sealed record PlaceDef(string Id, string Name, string Subtitle, string Distance, string Badge, string BadgeTone, string Icon);

    public static IReadOnlyList<NearbyPlaceCardSummaryViewModel> BuildOverviewCards(HouseFactProfileViewModel profile)
    {
        var address = profile.FormattedAddress ?? string.Empty;
        var parks = BuildParksList(address);
        var airports = BuildAirportsList(address);
        var hospitals = BuildHospitalsList(address);

        return
        [
            new NearbyPlaceCardSummaryViewModel
            {
                Key = "parks",
                Title = "Parks",
                Subtitle = "Nearby parks, trails & recreation",
                Icon = "fa-tree",
                Tone = "green",
                Badge = $"{parks.Count} nearby",
                ItemCount = parks.Count
            },
            new NearbyPlaceCardSummaryViewModel
            {
                Key = "airports",
                Title = "Nearby Airports",
                Subtitle = "Closest airports & travel access",
                Icon = "fa-plane",
                Tone = "blue",
                Badge = $"{airports.Count} airports",
                ItemCount = airports.Count
            },
            new NearbyPlaceCardSummaryViewModel
            {
                Key = "hospitals",
                Title = "Nearby Hospitals",
                Subtitle = "Hospitals, urgent care & ER access",
                Icon = "fa-hospital",
                Tone = "blue",
                Badge = $"{hospitals.Count} nearby",
                ItemCount = hospitals.Count
            }
        ];
    }

    public static NearbyPlacesIndexViewModel BuildParks(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var address = ResolveAddress(propiedad, info);
        var places = BuildParksList(address);
        return new NearbyPlacesIndexViewModel
        {
            PropiedadId = propiedad.Id,
            Address = address,
            PageTitle = "Parks",
            PageSubtitle = "Nearby parks, trails & recreation",
            Icon = "fa-tree",
            Tone = "green",
            Places = places,
            InfoBanner = "Park distances are estimated from your property address and may vary by route."
        };
    }

    public static NearbyPlacesIndexViewModel BuildAirports(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var address = ResolveAddress(propiedad, info);
        var places = BuildAirportsList(address);
        return new NearbyPlacesIndexViewModel
        {
            PropiedadId = propiedad.Id,
            Address = address,
            PageTitle = "Nearby Airports",
            PageSubtitle = "Closest airports & travel access",
            Icon = "fa-plane",
            Tone = "blue",
            Places = places,
            InfoBanner = "Travel times depend on traffic and route. Confirm schedules with each airport."
        };
    }

    public static NearbyPlacesIndexViewModel BuildHospitals(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var address = ResolveAddress(propiedad, info);
        var places = BuildHospitalsList(address);
        return new NearbyPlacesIndexViewModel
        {
            PropiedadId = propiedad.Id,
            Address = address,
            PageTitle = "Nearby Hospitals",
            PageSubtitle = "Hospitals, urgent care & ER access",
            Icon = "fa-hospital",
            Tone = "blue",
            Places = places,
            InfoBanner = "For emergencies, call 911. Distances shown are estimated from your home."
        };
    }

    private static List<NearbyPlaceItemViewModel> BuildParksList(string address)
    {
        if (IsCharlotteArea(address))
        {
            return MapPlaces(
            [
                new("reedy-creek", "Reedy Creek Nature Preserve", "Nature preserve • Trails & wildlife", "4.2 mi", "Park", "green", "fa-tree"),
                new("squirrel-lake", "Squirrel Lake Park", "Lake loop • Playground & picnic areas", "3.1 mi", "Park", "green", "fa-tree"),
                new("veterans-memorial", "Veterans Memorial Park", "Sports fields • Walking paths", "2.4 mi", "Park", "green", "fa-tree"),
                new("wilgrove", "Wilgrove Park", "Neighborhood park • Open green space", "5.0 mi", "Park", "green", "fa-tree")
            ]);
        }

        return MapPlaces(
        [
            new("community-park", "Community Park", "Local park • Trails & recreation", "2.5 mi", "Park", "green", "fa-tree"),
            new("nature-trail", "Nature Trail", "Walking trail • Green space", "3.8 mi", "Trail", "green", "fa-person-hiking"),
            new("neighborhood-park", "Neighborhood Park", "Playground • Picnic area", "1.9 mi", "Park", "green", "fa-tree"),
            new("regional-park", "Regional Park", "Sports fields • Open space", "4.6 mi", "Park", "green", "fa-tree")
        ]);
    }

    private static List<NearbyPlaceItemViewModel> BuildAirportsList(string address)
    {
        if (IsCharlotteArea(address))
        {
            return MapPlaces(
            [
                new("clt", "Charlotte Douglas International (CLT)", "Major hub • Domestic & international flights", "18 mi", "International", "blue", "fa-plane"),
                new("concord", "Concord-Padgett Regional (JQF)", "Regional airport • General aviation", "12 mi", "Regional", "blue", "fa-plane-departure")
            ]);
        }

        return MapPlaces(
        [
            new("regional-airport", "Regional Airport", "Commercial & regional flights", "15 mi", "Regional", "blue", "fa-plane"),
            new("municipal-airport", "Municipal Airport", "General aviation • Private charters", "22 mi", "Municipal", "blue", "fa-plane-departure")
        ]);
    }

    private static List<NearbyPlaceItemViewModel> BuildHospitalsList(string address)
    {
        if (IsCharlotteArea(address))
        {
            return MapPlaces(
            [
                new("atrium-mint-hill", "Atrium Health Mint Hill", "Hospital • Emergency & inpatient care", "3.4 mi", "Hospital", "blue", "fa-hospital"),
                new("novant-matthews", "Novant Health Matthews", "Hospital • ER & specialty care", "6.8 mi", "Hospital", "blue", "fa-hospital"),
                new("atrium-pineville", "Atrium Health Pineville", "Hospital • Urgent & emergency services", "9.2 mi", "Hospital", "blue", "fa-hospital")
            ]);
        }

        return MapPlaces(
        [
            new("community-hospital", "Community Hospital", "Hospital • Emergency department", "4.5 mi", "Hospital", "blue", "fa-hospital"),
            new("urgent-care", "Urgent Care Center", "Walk-in care • Extended hours", "2.1 mi", "Urgent care", "blue", "fa-kit-medical"),
            new("regional-medical", "Regional Medical Center", "Hospital • Specialty services", "8.7 mi", "Hospital", "blue", "fa-hospital")
        ]);
    }

    private static List<NearbyPlaceItemViewModel> MapPlaces(IEnumerable<PlaceDef> defs) =>
        defs.Select(d => new NearbyPlaceItemViewModel
        {
            Id = d.Id,
            Name = d.Name,
            Subtitle = d.Subtitle,
            Distance = d.Distance,
            Badge = d.Badge,
            BadgeTone = d.BadgeTone,
            Icon = d.Icon
        }).ToList();

    private static bool IsCharlotteArea(string address)
    {
        var haystack = address.ToLowerInvariant();
        return haystack.Contains("mint hill")
            || haystack.Contains("charlotte")
            || haystack.Contains("matthews")
            || haystack.Contains("282")
            || haystack.Contains("mecklenburg");
    }

    private static string ResolveAddress(Propiedad propiedad, PropertyInfoViewModel? info) =>
        propiedad.Direccion ?? info?.FormattedAddress ?? "Property address";
}
