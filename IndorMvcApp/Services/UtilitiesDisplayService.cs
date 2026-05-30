using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class UtilitiesDisplayService
{
    private static readonly UtilitiesTabViewModel[] TabDefs =
    [
        new() { Key = "overview", Label = "Overview", Step = 1 },
        new() { Key = "providers", Label = "Providers", Step = 2 },
        new() { Key = "setup", Label = "Setup", Step = 3 },
        new() { Key = "contacts", Label = "Contacts", Step = 4 }
    ];

    public static UtilitiesIndexViewModel BuildIndex(Propiedad propiedad, PropertyInfoViewModel? info, string? tab = null)
    {
        var items = BuildUtilityItems(propiedad, info);
        var activeTab = NormalizeTab(tab);
        var tabDef = TabDefs.First(t => t.Key == activeTab);

        var model = new UtilitiesIndexViewModel
        {
            PropiedadId = propiedad.Id,
            Address = propiedad.Direccion ?? info?.FormattedAddress ?? "Property address",
            ActiveTab = activeTab,
            CurrentStep = tabDef.Step,
            TotalSteps = 4,
            StepLabel = StepLabelForTab(activeTab),
            Tabs = TabDefs.ToList(),
            Utilities = items,
            ProviderCount = items.Count(i => !string.IsNullOrWhiteSpace(i.ProviderName)),
            HasData = items.Count > 0,
            SavedToHouseFacts = items.Count > 0,
            PageTitle = "Utilities",
            PageSubtitle = SubtitleForTab(activeTab),
            InfoBanner = BannerForTab(activeTab),
            PrimaryActionLabel = PrimaryLabelForTab(activeTab),
            SecondaryActionLabel = SecondaryLabelForTab(activeTab),
            PrimaryActionIcon = PrimaryIconForTab(activeTab),
            SecondaryActionIcon = SecondaryIconForTab(activeTab),
            PrimaryActionTab = PrimaryTabForTab(activeTab),
            SecondaryActionTab = SecondaryTabForTab(activeTab)
        };

        model.ContactRows = items
            .Where(i => !string.IsNullOrWhiteSpace(i.ProviderName))
            .Select(i => new UtilityContactRowViewModel
            {
                UtilityId = i.Id,
                Type = i.Type,
                ProviderName = i.ProviderName,
                Icon = i.Icon,
                Phone = i.Phone,
                Website = FormatWebsiteDisplay(i.Website),
                Status = i.Status is "Available" or "Active" ? "Active" : i.Status,
                StatusTone = i.StatusTone
            })
            .ToList();

        model.SetupItems = BuildSetupItems(items);

        return model;
    }

    public static UtilitiesDetailViewModel? BuildDetail(Propiedad propiedad, PropertyInfoViewModel? info, string utilityId)
    {
        var items = BuildUtilityItems(propiedad, info);
        var item = items.FirstOrDefault(i => i.Id.Equals(utilityId, StringComparison.OrdinalIgnoreCase));
        if (item == null) return null;

        var status = item.Status is "Available" or "Active" ? "Active" : item.Status;
        var model = new UtilitiesDetailViewModel
        {
            PropiedadId = propiedad.Id,
            UtilityId = item.Id,
            Address = propiedad.Direccion ?? info?.FormattedAddress ?? "Property address",
            PageTitle = $"{item.Type} Details",
            ProviderCount = items.Count(i => !string.IsNullOrWhiteSpace(i.ProviderName)),
            UtilityType = item.Type,
            ProviderName = item.ProviderName,
            Icon = item.Icon,
            Status = status,
            StatusTone = item.StatusTone,
            ServiceBadge = status == "Active" ? "Service available" : "Estimated coverage",
            ServiceBadgeTone = item.StatusTone,
            Phone = item.Phone,
            Website = item.Website,
            InfoBanner = "Provider information is provided for convenience and should be verified. Contact the provider directly for the most accurate details.",
            Rows = BuildDetailRows(item)
        };

        return model;
    }

    private static List<UtilityItemViewModel> BuildUtilityItems(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var items = new List<UtilityItemViewModel>();
        var providers = info?.UtilityProviders;

        AddProviderItem(items, "electric", "Electricity", "fa-bolt", providers?.Electric);
        AddProviderItem(items, "water", "Water", "fa-droplet", providers?.Water);
        AddProviderItem(items, "gas", "Gas", "fa-fire-flame-curved", providers?.Gas);

        var internet = MergeInternetCable(providers);
        AddProviderItem(items, "internet", "Internet / Cable", "fa-wifi", internet);

        AddProviderItem(items, "sewer", "Sewer", "fa-water", providers?.Sewer);

        var trash = FindTrashProvider(propiedad, info);
        if (trash != null)
        {
            AddProviderItem(items, "trash", "Trash", "fa-trash-can", trash);
        }
        else
        {
            items.Add(new UtilityItemViewModel
            {
                Id = "trash",
                Type = "Trash",
                ProviderName = "Local service",
                Icon = "fa-trash-can",
                Status = "Estimated",
                StatusTone = "orange",
                ServiceType = "Municipal waste collection",
                Notes = "Contact your municipality for trash and recycling pickup schedules."
            });
        }

        AppendFromHouseFactProfile(items, propiedad, info);

        return items
            .GroupBy(i => i.Id, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(i => OrderFor(i.Id))
            .ToList();
    }

    private static void AppendFromHouseFactProfile(List<UtilityItemViewModel> items, Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var profile = HouseFactDisplayService.BuildProfile(
            propiedad.AttomRawJson,
            info?.DataSource ?? propiedad.AttomSyncStatus,
            propiedad.Direccion ?? info?.FormattedAddress);

        foreach (var section in profile.Sections)
        {
            foreach (var utility in section.Utilities)
            {
                var id = MapUtilityId(utility.Type);
                if (items.Any(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(i.ProviderName)
                    && i.ProviderName != "Local service"))
                {
                    continue;
                }

                var existing = items.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    if (string.IsNullOrWhiteSpace(existing.ProviderName) || existing.ProviderName == "Local service")
                    {
                        existing.ProviderName = utility.Provider;
                    }
                    existing.Notes ??= utility.Notes;
                    ApplyStatus(existing);
                }
                else if (!string.IsNullOrWhiteSpace(utility.Provider))
                {
                    items.Add(new UtilityItemViewModel
                    {
                        Id = id,
                        Type = FormatTypeLabel(id),
                        ProviderName = utility.Provider,
                        Icon = IconFor(id),
                        Notes = utility.Notes,
                        Status = "Estimated",
                        StatusTone = "orange"
                    });
                    ApplyStatus(items[^1]);
                }
            }
        }
    }

    private static UtilityProvider? MergeInternetCable(UtilityProvidersInfo? providers)
    {
        if (providers == null) return null;
        var internet = providers.Internet.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Name));
        if (internet != null) return internet;
        return providers.CableTV.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Name));
    }

    private static UtilityProvider? FindTrashProvider(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var profile = HouseFactDisplayService.BuildProfile(
            propiedad.AttomRawJson,
            info?.DataSource ?? propiedad.AttomSyncStatus,
            propiedad.Direccion ?? info?.FormattedAddress);

        foreach (var section in profile.Sections)
        {
            foreach (var field in section.Fields)
            {
                if (field.Label.Contains("trash", StringComparison.OrdinalIgnoreCase)
                    || field.Label.Contains("recycling", StringComparison.OrdinalIgnoreCase)
                    || field.Label.Contains("waste", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(field.Value) && field.Value != "—")
                    {
                        return new UtilityProvider
                        {
                            Name = field.Value,
                            ServiceType = "Trash and recycling",
                            Phone = ExtractPhone(field.Value) ?? GuessMunicipalPhone(info)
                        };
                    }
                }
            }

            var trashUtility = section.Utilities.FirstOrDefault(u =>
                u.Type.Contains("trash", StringComparison.OrdinalIgnoreCase)
                || u.Type.Contains("waste", StringComparison.OrdinalIgnoreCase));
            if (trashUtility != null && !string.IsNullOrWhiteSpace(trashUtility.Provider))
            {
                return new UtilityProvider
                {
                    Name = trashUtility.Provider,
                    ServiceType = "Trash and recycling",
                    Notes = trashUtility.Notes
                };
            }
        }

        return null;
    }

    private static void AddProviderItem(List<UtilityItemViewModel> items, string id, string type, string icon, UtilityProvider? provider)
    {
        if (provider == null || string.IsNullOrWhiteSpace(provider.Name)) return;

        var item = new UtilityItemViewModel
        {
            Id = id,
            Type = type,
            ProviderName = provider.Name,
            Icon = icon,
            Phone = provider.Phone,
            Website = provider.Website,
            ServiceType = string.IsNullOrWhiteSpace(provider.ServiceType) ? $"{type} utility service" : provider.ServiceType,
            Notes = provider.Notes,
            Coverage = provider.Coverage
        };
        ApplyStatus(item);
        items.Add(item);
    }

    private static void ApplyStatus(UtilityItemViewModel item)
    {
        var hasContact = !string.IsNullOrWhiteSpace(item.Phone) || !string.IsNullOrWhiteSpace(item.Website);
        var isGeneric = item.ProviderName.Contains("local", StringComparison.OrdinalIgnoreCase)
            || item.ProviderName.Contains("unknown", StringComparison.OrdinalIgnoreCase);

        if (item.Id == "trash" && (isGeneric || !hasContact))
        {
            item.Status = "Scheduled";
            item.StatusTone = "orange";
        }
        else if (hasContact && !isGeneric)
        {
            item.Status = "Available";
            item.StatusTone = "green";
        }
        else if (!string.IsNullOrWhiteSpace(item.ProviderName))
        {
            item.Status = "Estimated";
            item.StatusTone = "orange";
        }
        else
        {
            item.Status = "Not confirmed";
            item.StatusTone = "gray";
        }
    }

    private static List<UtilityDetailRowViewModel> BuildDetailRows(UtilityItemViewModel item)
    {
        var rows = new List<UtilityDetailRowViewModel>
        {
            Row("Provider name", item.ProviderName, "fa-building"),
            Row("Service type", item.ServiceType ?? $"{item.Type} utility service", "fa-plug")
        };

        if (!string.IsNullOrWhiteSpace(item.Phone))
        {
            rows.Add(LinkRow("Phone number", item.Phone, "fa-phone", $"tel:{NormalizeTel(item.Phone)}"));
        }

        if (!string.IsNullOrWhiteSpace(item.Website))
        {
            var url = NormalizeUrl(item.Website);
            rows.Add(LinkRow("Website", FormatWebsiteDisplay(item.Website)!, "fa-globe", url));
        }

        if (!string.IsNullOrWhiteSpace(item.Phone))
        {
            rows.Add(LinkRow("Outage / emergency line", item.Phone, "fa-triangle-exclamation", $"tel:{NormalizeTel(item.Phone)}"));
            rows.Add(LinkRow("Billing / account setup", item.Phone, "fa-file-invoice", $"tel:{NormalizeTel(item.Phone)}"));
        }

        if (!string.IsNullOrWhiteSpace(item.Notes))
        {
            rows.Add(Row("Service notes", item.Notes, "fa-note-sticky", false));
        }

        if (!string.IsNullOrWhiteSpace(item.Coverage))
        {
            rows.Add(Row("Coverage note", item.Coverage, "fa-shield-halved", false));
        }
        else if (!string.IsNullOrWhiteSpace(item.ProviderName))
        {
            rows.Add(Row("Coverage note", $"{item.ProviderName} provides service to this address.", "fa-shield-halved", false));
        }

        return rows;
    }

    private static List<UtilitySetupItemViewModel> BuildSetupItems(IReadOnlyList<UtilityItemViewModel> items) =>
    [
        new() { Title = "Review utility providers", Description = "Confirm electricity, water, gas, internet, sewer, and trash providers for this address.", Icon = "fa-list-check", Completed = items.Count > 0 },
        new() { Title = "Start provider accounts", Description = "Contact each provider to open or transfer service before move-in.", Icon = "fa-file-signature", Completed = false },
        new() { Title = "Save contact details", Description = "Keep phone numbers and websites handy for outages and billing questions.", Icon = "fa-address-book", Completed = items.Any(i => !string.IsNullOrWhiteSpace(i.Phone)) },
        new() { Title = "Schedule trash pickup", Description = "Confirm collection days and bin requirements with your local service.", Icon = "fa-trash-can", Completed = items.Any(i => i.Id == "trash" && i.Status != "Estimated") }
    ];

    private static UtilityDetailRowViewModel Row(string label, string value, string icon, bool showChevron = true) =>
        new() { Label = label, Value = value, Icon = icon, ShowChevron = showChevron };

    private static UtilityDetailRowViewModel LinkRow(string label, string value, string icon, string href) =>
        new() { Label = label, Value = value, Icon = icon, IsLink = true, LinkHref = href, ShowChevron = true };

    private static string NormalizeTab(string? tab) =>
        tab?.ToLowerInvariant() switch
        {
            "providers" => "providers",
            "contacts" => "contacts",
            "setup" => "setup",
            _ => "overview"
        };

    private static string StepLabelForTab(string tab) => tab switch
    {
        "providers" => "Providers",
        "setup" => "Setup",
        "contacts" => "Contacts",
        _ => "Overview"
    };

    private static string SubtitleForTab(string tab) => tab switch
    {
        "providers" => "Browse all providers for this property.",
        "contacts" => "Quick access to the providers for this property.",
        "setup" => "Steps to set up utility accounts for this property.",
        _ => "Utility providers and service details for this property."
    };

    private static string BannerForTab(string tab) => tab switch
    {
        "providers" => "Tap any provider to view contact details, service notes, and account information.",
        "contacts" => "Public utility information may change. Please verify details with providers as needed.",
        "setup" => "Use this checklist to transfer or start utility service before you move in.",
        _ => "Tap any category to view more details and provider information."
    };

    private static string PrimaryLabelForTab(string tab) => tab switch
    {
        "overview" => "View providers",
        "providers" => "Open contacts",
        "setup" => "View contacts",
        "contacts" => "Done",
        _ => "Continue"
    };

    private static string SecondaryLabelForTab(string tab) => tab switch
    {
        "overview" => "Save utility info",
        "providers" => "Back to overview",
        "setup" => "Back to providers",
        "contacts" => "Share utility list",
        _ => "Back"
    };

    private static string PrimaryIconForTab(string tab) => tab switch
    {
        "overview" => "fa-list",
        "providers" => "fa-address-book",
        "setup" => "fa-address-book",
        "contacts" => "fa-check",
        _ => "fa-arrow-right"
    };

    private static string SecondaryIconForTab(string tab) => tab switch
    {
        "overview" => "fa-bookmark",
        "providers" => "fa-arrow-left",
        "setup" => "fa-arrow-left",
        "contacts" => "fa-share-from-square",
        _ => "fa-arrow-left"
    };

    private static string? PrimaryTabForTab(string tab) => tab switch
    {
        "overview" => "providers",
        "providers" => "contacts",
        "setup" => "contacts",
        _ => null
    };

    private static string? SecondaryTabForTab(string tab) => tab switch
    {
        "overview" => null,
        "providers" => "overview",
        "setup" => "providers",
        _ => null
    };

    private static int OrderFor(string id) => id switch
    {
        "electric" => 1,
        "water" => 2,
        "gas" => 3,
        "internet" => 4,
        "sewer" => 5,
        "trash" => 6,
        _ => 99
    };

    private static string MapUtilityId(string type)
    {
        var t = type.ToLowerInvariant();
        if (t.Contains("electric")) return "electric";
        if (t.Contains("water") && !t.Contains("sewer")) return "water";
        if (t.Contains("gas")) return "gas";
        if (t.Contains("internet") || t.Contains("cable")) return "internet";
        if (t.Contains("sewer")) return "sewer";
        if (t.Contains("trash") || t.Contains("waste")) return "trash";
        return "electric";
    }

    private static string FormatTypeLabel(string id) => id switch
    {
        "electric" => "Electricity",
        "water" => "Water",
        "gas" => "Gas",
        "internet" => "Internet / Cable",
        "sewer" => "Sewer",
        "trash" => "Trash",
        _ => "Utility"
    };

    private static string IconFor(string id) => id switch
    {
        "electric" => "fa-bolt",
        "water" => "fa-droplet",
        "gas" => "fa-fire-flame-curved",
        "internet" => "fa-wifi",
        "sewer" => "fa-water",
        "trash" => "fa-trash-can",
        _ => "fa-plug"
    };

    private static string? ExtractPhone(string value)
    {
        var digits = new string(value.Where(c => char.IsDigit(c) || c == '(' || c == ')' || c == '-').ToArray());
        return digits.Length >= 10 ? value : null;
    }

    private static string? GuessMunicipalPhone(PropertyInfoViewModel? info)
    {
        var city = info?.City;
        return string.IsNullOrWhiteSpace(city) ? null : $"Contact {city} municipal services";
    }

    private static string NormalizeTel(string phone) =>
        new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());

    private static string NormalizeUrl(string website)
    {
        if (website.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return website;
        return $"https://{website.TrimStart('/')}";
    }

    private static string? FormatWebsiteDisplay(string? website)
    {
        if (string.IsNullOrWhiteSpace(website)) return null;
        return website
            .Replace("https://", "", StringComparison.OrdinalIgnoreCase)
            .Replace("http://", "", StringComparison.OrdinalIgnoreCase)
            .TrimEnd('/');
    }
}
