using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Services;

public sealed class GuestEmergencyDraft
{
    public int ServicioEmergenciaId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? ServiceTitle { get; set; }
    public string? Description { get; set; }
    public string WhenNeeded { get; set; } = "ASAP";
    public string? RequestId { get; set; }
}

public static class GuestEmergencySession
{
    public const string SessionKey = "GuestEmergencyDraft";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static GuestEmergencyDraft? Get(ISession session)
    {
        var json = session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<GuestEmergencyDraft>(json, JsonOptions);
    }

    public static void Set(ISession session, GuestEmergencyDraft draft)
    {
        session.SetString(SessionKey, JsonSerializer.Serialize(draft, JsonOptions));
    }

    public static void Clear(ISession session)
    {
        session.Remove(SessionKey);
    }

    public static string BuildResumeUrl(GuestEmergencyDraft draft, IUrlHelper url)
    {
        var action = EmergencyFlowRules.GetDetailsActionName(draft.ServiceName);
        return action != null
            ? url.Action(action, "Inspecciones", new { id = draft.ServicioEmergenciaId }) ?? "/"
            : url.Action("Index", "Explore") ?? "/";
    }
}
