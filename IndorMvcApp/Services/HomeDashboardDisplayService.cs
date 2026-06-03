using System.Globalization;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Services;

public static class HomeDashboardDisplayService
{
    public static HomeDashboardViewModel Build(
        ApplicationUser? user,
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        PropiedadHvacSistema? hvacRecord,
        PropiedadWaterHeaterSistema? waterHeaterRecord,
        IReadOnlyList<PropiedadMantenimiento> mantenimiento,
        IReadOnlyList<PropiedadDocumento> documentos,
        IReadOnlyList<PropiedadHistorial> historial,
        int notificationCount,
        IUrlHelper url)
    {
        var d = info?.PropertyDetails;

        var vm = new HomeDashboardViewModel
        {
            UserFirstName = FirstName(user),
            Greeting = GreetingForHour(DateTime.Now.Hour),
            HasProperty = true,
            PropiedadId = propiedad.Id,
            Address = !string.IsNullOrWhiteSpace(info?.FormattedAddress)
                ? info!.FormattedAddress!
                : (propiedad.Direccion ?? "—"),
            HomeValue = d?.EstimatedValue.HasValue == true
                ? string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C0}", d.EstimatedValue.Value)
                : "—",
            BedsLabel = d?.Bedrooms.HasValue == true ? $"{d.Bedrooms} bed{(d.Bedrooms == 1 ? "" : "s")}" : null,
            BathsLabel = d?.Bathrooms.HasValue == true
                ? $"{d.Bathrooms:0.#} bath{(d.Bathrooms == 1 ? "" : "s")}"
                : null,
            SqftLabel = d?.LivingArea.HasValue == true ? $"{d.LivingArea:N0} sqft" : null,
            HouseFactsUrl = $"{url.Action("Index", "Home")}#section-myhome",
            NotificationCount = notificationCount,
            QuickActions = BuildQuickActions(propiedad.Id, url),
            TodayTasks = BuildTodayTasks(propiedad.Id, hvacRecord, waterHeaterRecord, documentos, mantenimiento, url),
            UpcomingSchedule = BuildSchedule(mantenimiento),
            RecentDocuments = BuildDocuments(propiedad.Id, documentos, url),
            RecentActivity = BuildActivity(historial)
        };

        return vm;
    }

    public static HomeDashboardViewModel BuildEmpty(ApplicationUser? user)
    {
        return new HomeDashboardViewModel
        {
            UserFirstName = FirstName(user),
            Greeting = GreetingForHour(DateTime.Now.Hour),
            HasProperty = false,
            QuickActions = BuildQuickActions(null, null)
        };
    }

    public static HomeDashboardViewModel BuildBasic(
        ApplicationUser? user,
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        IUrlHelper url)
    {
        var d = info?.PropertyDetails;
        return new HomeDashboardViewModel
        {
            UserFirstName = FirstName(user),
            Greeting = GreetingForHour(DateTime.Now.Hour),
            HasProperty = true,
            PropiedadId = propiedad.Id,
            Address = !string.IsNullOrWhiteSpace(info?.FormattedAddress)
                ? info!.FormattedAddress!
                : (propiedad.Direccion ?? "—"),
            HomeValue = d?.EstimatedValue.HasValue == true
                ? string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C0}", d.EstimatedValue.Value)
                : "—",
            BedsLabel = d?.Bedrooms.HasValue == true ? $"{d.Bedrooms} bed{(d.Bedrooms == 1 ? "" : "s")}" : null,
            BathsLabel = d?.Bathrooms.HasValue == true
                ? $"{d.Bathrooms:0.#} bath{(d.Bathrooms == 1 ? "" : "s")}"
                : null,
            SqftLabel = d?.LivingArea.HasValue == true ? $"{d.LivingArea:N0} sqft" : null,
            HouseFactsUrl = $"{url.Action("Index", "Home")}#section-myhome",
            QuickActions = BuildQuickActions(propiedad.Id, url),
            TodayTasks =
            [
                new HomeTodayTaskViewModel
                {
                    Icon = "fa-fan",
                    Title = "Add your HVAC system details",
                    Subtitle = "Get personalized maintenance tips",
                    Url = url.Action("Add", "HvacSetup", new { propiedadId = propiedad.Id }) ?? "#"
                },
                new HomeTodayTaskViewModel
                {
                    Icon = "fa-fire-burner",
                    Title = "Add water heater info",
                    Subtitle = "Help us keep track age, model & warranty",
                    Url = url.Action("Add", "WaterHeaterSetup", new { propiedadId = propiedad.Id }) ?? "#"
                }
            ]
        };
    }

    private static string FirstName(ApplicationUser? user)
    {
        if (!string.IsNullOrWhiteSpace(user?.Nombre))
        {
            return user!.Nombre.Trim();
        }

        return "there";
    }

    private static string GreetingForHour(int hour) =>
        hour switch
        {
            >= 5 and < 12 => "Good morning",
            >= 12 and < 17 => "Good afternoon",
            >= 17 and < 22 => "Good evening",
            _ => "Good evening"
        };

    private static List<HomeQuickActionViewModel> BuildQuickActions(int? propiedadId, IUrlHelper? url)
    {
        var maintenanceUrl = propiedadId.HasValue && url != null
            ? url.Action("Maintenance", "MyHome", new { id = propiedadId.Value })
            : null;
        var documentsUrl = propiedadId.HasValue && url != null
            ? url.Action("Documents", "MyHome", new { id = propiedadId.Value })
            : null;

        return
        [
            new() { Icon = "fa-screwdriver-wrench", Label = "Request Service", TargetSection = "section-services" },
            new()
            {
                Icon = "fa-house-chimney",
                Label = "Home Maintenance",
                TargetSection = "section-myhome",
                Url = maintenanceUrl
            },
            new()
            {
                Icon = "fa-triangle-exclamation",
                Label = "Emergency Help",
                TargetSection = "section-services",
                ScrollTarget = "emergency-services",
                Tone = "red"
            },
            new()
            {
                Icon = "fa-file-lines",
                Label = "Documents",
                TargetSection = "section-myhome",
                Url = documentsUrl
            }
        ];
    }

    private static List<HomeTodayTaskViewModel> BuildTodayTasks(
        int propiedadId,
        PropiedadHvacSistema? hvacRecord,
        PropiedadWaterHeaterSistema? waterHeaterRecord,
        IReadOnlyList<PropiedadDocumento> documentos,
        IReadOnlyList<PropiedadMantenimiento> mantenimiento,
        IUrlHelper url)
    {
        var tasks = new List<HomeTodayTaskViewModel>();

        if (hvacRecord == null)
        {
            tasks.Add(new HomeTodayTaskViewModel
            {
                Icon = "fa-fan",
                Title = "Add your HVAC system details",
                Subtitle = "Get personalized maintenance tips",
                Url = url.Action("Add", "HvacSetup", new { propiedadId }) ?? "#"
            });
        }
        else if (hvacRecord.FilterRemindersEnabled && !hvacRecord.FilterReminderSetupComplete)
        {
            tasks.Add(new HomeTodayTaskViewModel
            {
                Icon = "fa-filter",
                Title = "Set up HVAC filter reminders",
                Subtitle = "Personalize your replacement schedule",
                Url = url.Action("Pets", "HvacFilterReplacement", new { id = propiedadId }) ?? "#"
            });
        }

        if (!documentos.Any(d => d.Category.Contains("Inspection", StringComparison.OrdinalIgnoreCase)
                                 || d.Title.Contains("inspection", StringComparison.OrdinalIgnoreCase)))
        {
            tasks.Add(new HomeTodayTaskViewModel
            {
                Icon = "fa-file-circle-plus",
                Title = "Upload your inspection report",
                Subtitle = "Keep your home records up to date",
                Url = url.Action("Upload", "InspectionReportUpload", new { propiedadId }) ?? "#"
            });
        }

        var dueSoon = mantenimiento
            .Where(m => m.DueDate.HasValue && m.Status != "Completed")
            .Count(m => (m.DueDate!.Value.Date - DateTime.Today).Days <= 30);

        if (dueSoon > 0)
        {
            tasks.Add(new HomeTodayTaskViewModel
            {
                Icon = "fa-bell",
                Title = "Review this month's reminders",
                Subtitle = $"{dueSoon} task{(dueSoon == 1 ? "" : "s")} • Due soon",
                Badge = "Due soon",
                Url = url.Action("Maintenance", "MyHome", new { id = propiedadId }) ?? "#section-myhome"
            });
        }

        if (waterHeaterRecord == null)
        {
            tasks.Add(new HomeTodayTaskViewModel
            {
                Icon = "fa-fire-burner",
                Title = "Add water heater info",
                Subtitle = "Help us keep track age, model & warranty",
                Url = url.Action("Add", "WaterHeaterSetup", new { propiedadId }) ?? "#"
            });
        }
        else if (waterHeaterRecord.FlushRemindersEnabled && !waterHeaterRecord.FlushReminderSetupComplete)
        {
            tasks.Add(new HomeTodayTaskViewModel
            {
                Icon = "fa-droplet",
                Title = "Set up annual water heater flush",
                Subtitle = "Personalize your flush reminder schedule",
                Url = url.Action("Intro", "WaterHeaterFlushReminder", new { id = propiedadId }) ?? "#"
            });
        }

        return tasks;
    }

    private static List<HomeScheduleItemViewModel> BuildSchedule(IReadOnlyList<PropiedadMantenimiento> mantenimiento)
    {
        return mantenimiento
            .Where(m => m.DueDate.HasValue && m.Status != "Completed")
            .OrderBy(m => m.DueDate)
            .Take(3)
            .Select(m =>
            {
                var days = (m.DueDate!.Value.Date - DateTime.Today).Days;
                var dueLabel = days switch
                {
                    < 0 => $"Overdue {Math.Abs(days)} day{(Math.Abs(days) == 1 ? "" : "s")}",
                    0 => "Due today",
                    1 => "Due tomorrow",
                    _ => $"Due in {days} days"
                };

                return new HomeScheduleItemViewModel
                {
                    Icon = m.Title.Contains("filter", StringComparison.OrdinalIgnoreCase) ? "fa-fan"
                        : m.Title.Contains("water", StringComparison.OrdinalIgnoreCase) ? "fa-fire-burner"
                        : "fa-calendar-check",
                    Title = m.Title,
                    Location = ExtractLocation(m.Notes),
                    DueLabel = dueLabel
                };
            })
            .ToList();
    }

    private static string ExtractLocation(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return "Home";
        var parts = notes.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : "Home";
    }

    private static List<HomeDocumentItemViewModel> BuildDocuments(
        int propiedadId,
        IReadOnlyList<PropiedadDocumento> documentos,
        IUrlHelper url)
    {
        return documentos
            .OrderByDescending(d => d.FechaCreacion)
            .Take(3)
            .Select(d => new HomeDocumentItemViewModel
            {
                Icon = d.Category.Contains("Warranty", StringComparison.OrdinalIgnoreCase) ? "fa-shield-halved" : "fa-file-lines",
                Title = d.Title,
                Meta = $"Uploaded {d.FechaCreacion.ToLocalTime():MMM d, yyyy} • {FileTypeLabel(d.FileName, d.ContentType)}",
                Url = url.Action("Documents", "MyHome", new { id = propiedadId }) ?? "#"
            })
            .ToList();
    }

    private static string FileTypeLabel(string? fileName, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType) && contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
        {
            return "PDF";
        }

        var ext = Path.GetExtension(fileName ?? string.Empty).Trim('.').ToUpperInvariant();
        return string.IsNullOrWhiteSpace(ext) ? "File" : ext;
    }

    private static List<HomeActivityItemViewModel> BuildActivity(IReadOnlyList<PropiedadHistorial> historial)
    {
        return historial
            .OrderByDescending(h => h.FechaCreacion)
            .Take(3)
            .Select(h => new HomeActivityItemViewModel
            {
                Icon = h.RecordType.Contains("Water", StringComparison.OrdinalIgnoreCase) ? "fa-droplet"
                    : h.Title.Contains("filter", StringComparison.OrdinalIgnoreCase) ? "fa-fan"
                    : h.Title.Contains("upload", StringComparison.OrdinalIgnoreCase) ? "fa-file-arrow-up"
                    : "fa-circle-check",
                Title = h.Title,
                Timestamp = h.FechaCreacion.ToLocalTime().ToString("MMM d, yyyy 'at' h:mm tt", CultureInfo.GetCultureInfo("en-US")),
                Description = h.Description ?? h.ProviderName ?? "Updated in your home profile."
            })
            .ToList();
    }
}
