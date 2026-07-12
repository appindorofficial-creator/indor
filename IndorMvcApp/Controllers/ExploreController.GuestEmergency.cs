using IndorMvcApp.Data;
using IndorMvcApp.Localization;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Controllers;

public partial class ExploreController
{
    private static readonly string[] QuickEmergencyNames =
    [
        EmergencyFlowRules.HvacEmergencyName,
        EmergencyFlowRules.PlumbingEmergencyName,
        EmergencyFlowRules.WaterHeaterEmergencyName,
        EmergencyFlowRules.ElectricalEmergencyName
    ];

    private static readonly Dictionary<string, (string En, string Es)> QuickEmergencySubtitles = new(StringComparer.OrdinalIgnoreCase)
    {
        [EmergencyFlowRules.HvacEmergencyName] = ("No AC / Heating", "Sin AC / Calefacción"),
        [EmergencyFlowRules.PlumbingEmergencyName] = ("Leak / Pipe", "Fuga / Tubería"),
        [EmergencyFlowRules.WaterHeaterEmergencyName] = ("No Hot Water", "Sin agua caliente"),
        [EmergencyFlowRules.ElectricalEmergencyName] = ("Issue / Power", "Problema / Energía")
    };

    [HttpGet]
    public async Task<IActionResult> Emergency(int? id, CancellationToken cancellationToken)
    {
        var options = await BuildEmergencyOptionsAsync(cancellationToken);
        if (options.Count == 0)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new GuestEmergencyChooseViewModel
        {
            Options = options,
            SelectedId = id
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Emergency(int serviceId, CancellationToken cancellationToken)
    {
        var service = await FindEmergencyServiceAsync(serviceId, cancellationToken);
        if (service == null)
        {
            return RedirectToAction(nameof(Emergency));
        }

        var draft = new GuestEmergencyDraft
        {
            ServicioEmergenciaId = service.Id,
            ServiceName = service.Nombre,
            ServiceTitle = service.LocalizedTitulo(_localizer.IsSpanish)
        };
        GuestEmergencySession.Set(HttpContext.Session, draft);
        await HttpContext.Session.CommitAsync(cancellationToken);

        return RedirectToAction(nameof(EmergencyDetails));
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyDetails(int? id, CancellationToken cancellationToken)
    {
        if (id.HasValue)
        {
            var service = await FindEmergencyServiceAsync(id.Value, cancellationToken);
            if (service != null)
            {
                var quickDraft = new GuestEmergencyDraft
                {
                    ServicioEmergenciaId = service.Id,
                    ServiceName = service.Nombre,
                    ServiceTitle = service.LocalizedTitulo(_localizer.IsSpanish)
                };
                GuestEmergencySession.Set(HttpContext.Session, quickDraft);
                await HttpContext.Session.CommitAsync(cancellationToken);
            }
        }

        var draft = GuestEmergencySession.Get(HttpContext.Session);
        if (draft == null)
        {
            return RedirectToAction(nameof(Emergency));
        }

        return View(new GuestEmergencyDetailsViewModel
        {
            ServicioEmergenciaId = draft.ServicioEmergenciaId,
            ServiceName = draft.ServiceName,
            ServiceTitle = draft.ServiceTitle ?? draft.ServiceName,
            Description = draft.Description,
            WhenNeeded = string.IsNullOrWhiteSpace(draft.WhenNeeded) ? "ASAP" : draft.WhenNeeded
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyDetails(string? description, string? whenNeeded, CancellationToken cancellationToken)
    {
        var draft = GuestEmergencySession.Get(HttpContext.Session);
        if (draft == null)
        {
            return RedirectToAction(nameof(Emergency));
        }

        draft.Description = description?.Trim();
        draft.WhenNeeded = string.Equals(whenNeeded, "Today", StringComparison.OrdinalIgnoreCase) ? "Today" : "ASAP";
        GuestEmergencySession.Set(HttpContext.Session, draft);
        await HttpContext.Session.CommitAsync(cancellationToken);

        return RedirectToAction(nameof(EmergencySearching));
    }

    [HttpGet]
    public IActionResult EmergencySearching()
    {
        var draft = GuestEmergencySession.Get(HttpContext.Session);
        if (draft == null)
        {
            return RedirectToAction(nameof(Emergency));
        }

        return View(new GuestEmergencySearchingViewModel
        {
            ServiceTitle = draft.ServiceTitle ?? draft.ServiceName
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencySearchingComplete(CancellationToken cancellationToken)
    {
        var draft = GuestEmergencySession.Get(HttpContext.Session);
        if (draft == null)
        {
            return RedirectToAction(nameof(Emergency));
        }

        draft.RequestId = $"INDOR-{Random.Shared.Next(1000, 9999)}";
        GuestEmergencySession.Set(HttpContext.Session, draft);
        await HttpContext.Session.CommitAsync(cancellationToken);

        return RedirectToAction(nameof(EmergencySent));
    }

    [HttpGet]
    public IActionResult EmergencySent()
    {
        var draft = GuestEmergencySession.Get(HttpContext.Session);
        if (draft == null || string.IsNullOrWhiteSpace(draft.RequestId))
        {
            return RedirectToAction(nameof(Emergency));
        }

        return View(new GuestEmergencySentViewModel
        {
            RequestId = draft.RequestId,
            ServiceTitle = draft.ServiceTitle ?? draft.ServiceName,
            ResumeUrl = GuestEmergencySession.BuildResumeUrl(draft, Url)
        });
    }

    [HttpGet]
    public IActionResult EmergencyResume()
    {
        var draft = GuestEmergencySession.Get(HttpContext.Session);
        if (draft == null)
        {
            return RedirectToAction(nameof(Emergency));
        }

        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("LoginForm", "Account", new { returnUrl = Url.Action(nameof(EmergencyResume)) });
        }

        var resumeUrl = GuestEmergencySession.BuildResumeUrl(draft, Url);
        GuestEmergencySession.Clear(HttpContext.Session);
        return Redirect(resumeUrl);
    }

    internal IReadOnlyList<GuestEmergencyQuickCategoryViewModel> BuildQuickEmergencyCategories(
        IEnumerable<ServicioEmergencia> items)
    {
        var isSpanish = _localizer.IsSpanish;
        var lookup = items
            .Where(s => s.Activo)
            .ToDictionary(s => s.Nombre, s => s, StringComparer.OrdinalIgnoreCase);

        var categories = new List<GuestEmergencyQuickCategoryViewModel>();
        foreach (var name in QuickEmergencyNames)
        {
            if (!lookup.TryGetValue(name, out var service))
            {
                continue;
            }

            var subtitle = QuickEmergencySubtitles.TryGetValue(name, out var labels)
                ? (isSpanish ? labels.Es : labels.En)
                : service.LocalizedDescripcion(isSpanish);

            categories.Add(new GuestEmergencyQuickCategoryViewModel
            {
                Id = service.Id,
                Label = service.LocalizedTitulo(isSpanish),
                Subtitle = subtitle,
                IconClass = service.IconoClase,
                Url = Url.Action(nameof(EmergencyDetails), new { id = service.Id }) ?? "/"
            });
        }

        return categories;
    }

    private async Task<IReadOnlyList<GuestEmergencyOptionViewModel>> BuildEmergencyOptionsAsync(CancellationToken ct)
    {
        var catalog = await _catalogCache.GetAsync(_db, ct);
        var isSpanish = _localizer.IsSpanish;

        return catalog.ServiciosEmergencia
            .Where(s => s.Activo)
            .OrderBy(s => s.Orden)
            .ThenBy(s => s.Id)
            .Select(s => new GuestEmergencyOptionViewModel
            {
                Id = s.Id,
                Name = s.Nombre,
                Title = s.LocalizedTitulo(isSpanish),
                Subtitle = s.LocalizedDescripcion(isSpanish),
                IconClass = s.IconoClase
            })
            .ToList();
    }

    private async Task<ServicioEmergencia?> FindEmergencyServiceAsync(int id, CancellationToken ct)
    {
        var catalog = await _catalogCache.GetAsync(_db, ct);
        return catalog.ServiciosEmergencia.FirstOrDefault(s => s.Id == id && s.Activo);
    }
}
