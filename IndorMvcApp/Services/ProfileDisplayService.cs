using IndorMvcApp.Helpers;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

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
    public static MoreProfileViewModel Build(
        ApplicationUser? user,
        MembresiaUsuario? membresia,
        int homeCount,
        int documentCount,
        int serviceCount)
    {
        var fullName = UserDisplayName.Format(user);
        var hasName = !string.IsNullOrWhiteSpace(fullName);
        var hasEmail = !string.IsNullOrWhiteSpace(user?.Email);
        var hasPhone = !string.IsNullOrWhiteSpace(user?.Telefono);
        var hasPhoto = !string.IsNullOrWhiteSpace(user?.FotoUrl);
        var hasMembership = membresia?.Plan != null;
        var hasHome = homeCount > 0;

        var checks = new[] { hasName, hasEmail, hasPhone, hasPhoto, hasMembership, hasHome };
        var percent = (int)Math.Round(checks.Count(c => c) / (double)checks.Length * 100);

        return new MoreProfileViewModel
        {
            FullName = hasName ? fullName : "Your name",
            Email = hasEmail ? user!.Email! : "Add your email",
            Phone = hasPhone ? user!.Telefono! : "Add your phone",
            PhotoUrl = user?.FotoUrl,
            ProfileCompletionPercent = percent,
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
