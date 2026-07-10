using IndorMvcApp.Helpers;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Services;

public enum MembershipPlanKind
{
    Free,
    Filter,
    HomeCare,
    Premium,
    Other
}

public static class ProfileDisplayService
{
    /// <summary>Paid membership signup is disabled (e.g. iOS App Store — no in-app subscriptions).</summary>
    public const bool PaidMembershipEnabled = false;

    /// <summary>INDOR PRO provider insurance plan upsell on the home dashboard.</summary>
    public const bool ProviderInsuranceSalesEnabled = true;

    public static MoreProfileViewModel Build(
        ApplicationUser? user,
        MembresiaUsuario? membresia,
        int homeCount,
        int documentCount,
        int serviceCount,
        IUrlHelper? url = null)
    {
        var fullName = UserDisplayName.Format(user);
        var hasName = !string.IsNullOrWhiteSpace(fullName);
        var hasEmail = !string.IsNullOrWhiteSpace(user?.Email);
        var hasPhone = !string.IsNullOrWhiteSpace(user?.Telefono);
        var hasPhoto = !string.IsNullOrWhiteSpace(user?.FotoUrl);
        var hasMembership = membresia?.Plan != null;
        var hasHome = homeCount > 0;

        var profileChecks = new[] { hasName, hasEmail, hasPhone, hasPhoto };
        var percent = (int)Math.Round(profileChecks.Count(c => c) / (double)profileChecks.Length * 100);

        var items = new List<ProfileCompletionItemViewModel>
        {
            new()
            {
                Label = "Add your name",
                IsComplete = hasName,
                ActionUrl = url?.Action("EditarPerfil", "Perfil") + "#personal"
            },
            new()
            {
                Label = "Confirm your email",
                IsComplete = hasEmail,
                ActionUrl = url?.Action("Opciones", "Perfil") + "#contact"
            },
            new()
            {
                Label = "Add your phone number",
                IsComplete = hasPhone,
                ActionUrl = url?.Action("EditarPerfil", "Perfil") + "#personal"
            },
            new()
            {
                Label = "Upload a profile photo",
                IsComplete = hasPhoto,
                ActionUrl = url?.Action("EditarPerfil", "Perfil") + "#photo"
            }
        };

        if (url != null)
        {
            items.Add(new ProfileCompletionItemViewModel
            {
                Label = "Register your home",
                IsComplete = hasHome,
                ActionUrl = url.Action("EditarPerfil", "Perfil") + "#home"
            });
            if (PaidMembershipEnabled)
            {
                items.Add(new ProfileCompletionItemViewModel
                {
                    Label = "Choose a membership plan",
                    IsComplete = hasMembership,
                    ActionUrl = url.Action("Suscripciones", "Perfil")
                });
            }
        }

        return new MoreProfileViewModel
        {
            FullName = hasName ? fullName : "Your name",
            Email = hasEmail ? user!.Email! : "Add your email",
            Phone = hasPhone ? user!.Telefono! : "Add your phone",
            PhotoUrl = user?.FotoUrl,
            ProfileCompletionPercent = percent,
            CompletionItems = items,
            HasActiveMembership = hasMembership,
            MembershipLabel = hasMembership ? membresia!.Plan!.Nombre : "No Membership",
            HomeCount = homeCount,
            DocumentCount = documentCount,
            ServiceCount = serviceCount
        };
    }

    public static MembershipPlanKind GetPlanKind(string? planName, decimal precioMensual = -1)
    {
        var n = (planName ?? "").ToLowerInvariant();
        if (precioMensual == 0 || n.Contains("free") || n.Contains("gratis"))
            return MembershipPlanKind.Free;
        if (n.Contains("filter") || n.Contains("filtro"))
            return MembershipPlanKind.Filter;
        if (n.Contains("home care") || n.Contains("hogar"))
            return MembershipPlanKind.HomeCare;
        if (n.Contains("premium"))
            return MembershipPlanKind.Premium;
        return MembershipPlanKind.Other;
    }

    public static MembershipPlanKind GetPlanKind(PlanMembresia plan) =>
        GetPlanKind(plan.Nombre, plan.PrecioMensual);

    public static bool IsFilterPlan(string? planName) =>
        GetPlanKind(planName) == MembershipPlanKind.Filter;

    public static bool IsHomeCarePlan(string? planName) =>
        GetPlanKind(planName) == MembershipPlanKind.HomeCare;

    public static bool IsPremiumPlan(string? planName) =>
        GetPlanKind(planName) == MembershipPlanKind.Premium;

    public static bool UsesSixStepWizard(MembershipPlanKind kind) =>
        kind is MembershipPlanKind.Filter or MembershipPlanKind.HomeCare or MembershipPlanKind.Premium;

    public static string PlanIconClass(string? planName) =>
        GetPlanKind(planName) switch
        {
            MembershipPlanKind.Free => "fa-house",
            MembershipPlanKind.Filter => "fa-wind",
            MembershipPlanKind.HomeCare => "fa-house-heart",
            MembershipPlanKind.Premium => "fa-shield-halved",
            _ => "fa-house"
        };

    /// <summary>Outline checkmarks for Free/Filter; solid for Home Care/Premium.</summary>
    public static bool UseSolidCheckmarks(MembershipPlanKind kind) =>
        kind is MembershipPlanKind.HomeCare or MembershipPlanKind.Premium;
}
