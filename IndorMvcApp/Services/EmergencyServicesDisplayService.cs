using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Services;

public static class EmergencyServicesDisplayService
{
    public static EmergencyServicesGuideViewModel? BuildGuide(
        IEnumerable<ServicioEmergencia> items,
        IUrlHelper urlHelper,
        bool isSpanish = false)
    {
        var cards = items
            .Where(s => s.Activo)
            .OrderBy(s => s.Orden)
            .ThenBy(s => s.Id)
            .Select(s =>
            {
                var action = EmergencyFlowRules.GetDetailsActionName(s.Nombre);
                return new EmergencyGuideCardViewModel
                {
                    Id = s.Id,
                    Title = s.LocalizedTitulo(isSpanish),
                    Subtitle = isSpanish
                        ? $"{s.TiempoLlegadaMinutos} min prom · 24/7"
                        : $"{s.TiempoLlegadaMinutos} min avg · 24/7",
                    ImageUrl = s.ImagenUrl,
                    IconoClase = s.IconoClase,
                    Url = action != null
                        ? urlHelper.Action(action, "Inspecciones", new { id = s.Id })
                        : null
                };
            })
            .ToList();

        if (cards.Count == 0)
        {
            return null;
        }

        return new EmergencyServicesGuideViewModel
        {
            Title = isSpanish ? "Servicios de emergencia" : "Emergency Services",
            Subtitle = isSpanish ? "Ayuda rápida para problemas urgentes del hogar." : "Fast help for urgent home problems.",
            Items = cards
        };
    }

    public static EmergencyServicesSectionViewModel? BuildSection(
        IEnumerable<ServicioEmergencia> items,
        IUrlHelper urlHelper,
        bool isSpanish = false)
    {
        var list = items.Where(s => s.Activo).OrderBy(s => s.Orden).ThenBy(s => s.Id).ToList();
        if (list.Count == 0)
        {
            return null;
        }

        var cards = list.Select(s =>
        {
            var pipe = s.LocalizedCaracteristicas(isSpanish);
            var texts = SplitPipe(pipe);
            var icons = SplitPipe(s.IconosCaracteristicas);
            var features = new List<EmergencyServiceFeatureViewModel>();
            for (var i = 0; i < texts.Length; i++)
            {
                features.Add(new EmergencyServiceFeatureViewModel
                {
                    Icon = i < icons.Length && !string.IsNullOrWhiteSpace(icons[i]) ? icons[i] : "fa-clock",
                    Text = texts[i]
                });
            }

            return new EmergencyServiceCardViewModel
            {
                Id = s.Id,
                Nombre = s.Nombre,
                TituloEmergencia = s.LocalizedTitulo(isSpanish),
                Descripcion = s.LocalizedDescripcion(isSpanish),
                TiempoLlegadaMinutos = s.TiempoLlegadaMinutos,
                IconoClase = s.IconoClase,
                ImagenUrl = s.ImagenUrl,
                BadgeTexto = s.LocalizedBadgeTexto(isSpanish),
                CtaTexto = s.LocalizedCtaTexto(isSpanish),
                Caracteristicas = features
            };
        }).ToList();

        return new EmergencyServicesSectionViewModel
        {
            Items = cards,
            SelectedId = null,
            ViewAllUrl = urlHelper.Action("EmergencyGuide", "Home")
        };
    }

    private static string[] SplitPipe(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
