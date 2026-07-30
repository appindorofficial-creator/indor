using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class HomeownerProviderMessageService(AppDbContext db)
{
    private static readonly string[] ActiveProviderStatuses =
    [
        ProviderRegistrationStatuses.IndorProActive,
        ProviderRegistrationStatuses.Approved,
        ProviderRegistrationStatuses.Submitted,
        ProviderRegistrationStatuses.PendingReview
    ];

    public async Task<HomeownerMessageProviderViewModel?> BuildComposeAsync(
        ApplicationUser user,
        int providerId,
        IUrlHelper url,
        CancellationToken cancellationToken = default)
    {
        var provider = await LoadProviderAsync(providerId, cancellationToken);
        if (provider == null)
        {
            return null;
        }

        return await MapComposeAsync(provider, url, body: null, error: null, cancellationToken);
    }

    public async Task<(bool Ok, HomeownerMessageProviderViewModel? Retry, HomeownerMessageSentViewModel? Sent)> SendAsync(
        ApplicationUser user,
        HomeownerMessageProviderInput input,
        IUrlHelper url,
        CancellationToken cancellationToken = default)
    {
        var provider = await LoadProviderAsync(input.ProviderId, cancellationToken);
        if (provider == null)
        {
            return (false, null, null);
        }

        var body = (input.Body ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(body))
        {
            return (false, await MapComposeAsync(provider, url, body, "Please write a message before sending.", cancellationToken), null);
        }

        if (body.Length > 600)
        {
            body = body[..600];
        }

        var displayName = BuildDisplayName(user);
        var email = user.Email?.Trim();
        var phone = string.IsNullOrWhiteSpace(user.Telefono) ? null : user.Telefono.Trim();

        IndorProveedorCliente? cliente = null;
        if (!string.IsNullOrWhiteSpace(email))
        {
            cliente = await db.IndorProveedorClientes
                .FirstOrDefaultAsync(
                    c => c.ProveedorId == provider.Id
                         && c.Email != null
                         && c.Email == email,
                    cancellationToken);
        }

        if (cliente == null)
        {
            cliente = new IndorProveedorCliente
            {
                ProveedorId = provider.Id,
                Name = displayName,
                FirstName = NullIfEmpty(user.Nombre),
                LastName = NullIfEmpty(user.Apellidos),
                Email = email,
                Phone = phone,
                CustomerSource = "INDOR Nearby",
                PreferredContactMethod = "INDOR",
                IsAppConnected = true,
                ConnectionStatus = ProviderCustomerConnectionStatuses.Connected
            };
            db.IndorProveedorClientes.Add(cliente);
            await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(cliente.Name))
            {
                cliente.Name = displayName;
            }

            cliente.Email ??= email;
            cliente.Phone ??= phone;
            cliente.IsAppConnected = true;
        }

        var conversation = await db.IndorProveedorConversations
            .FirstOrDefaultAsync(
                c => c.ProveedorId == provider.Id
                     && c.ClienteId == cliente.Id
                     && c.Category == ProviderConversationCategories.Lead,
                cancellationToken);

        var now = DateTime.UtcNow;
        if (conversation == null)
        {
            conversation = new IndorProveedorConversation
            {
                ProveedorId = provider.Id,
                ClienteId = cliente.Id,
                Category = ProviderConversationCategories.Lead,
                Status = ProviderConversationStatuses.New,
                UnreadCount = 0,
                LastMessageAt = now,
                FechaCreacion = now
            };
            db.IndorProveedorConversations.Add(conversation);
            await db.SaveChangesAsync(cancellationToken);
        }

        db.IndorProveedorMessages.Add(new IndorProveedorMessage
        {
            ConversationId = conversation.Id,
            SenderType = ProviderMessageSenderTypes.Customer,
            Body = body,
            SentAt = now,
            IsRead = false
        });

        conversation.LastMessagePreview = TruncatePreview(body);
        conversation.LastMessageAt = now;
        conversation.UnreadCount += 1;
        if (conversation.Status == ProviderConversationStatuses.New)
        {
            conversation.Status = ProviderConversationStatuses.Pending;
        }

        await db.SaveChangesAsync(cancellationToken);

        return (true, null, new HomeownerMessageSentViewModel
        {
            ProviderId = provider.Id,
            ProviderName = ResolveProviderName(provider),
            BackUrl = BuildProvidersFeedUrl(url),
            ServicesUrl = (url.Action("Index", "Home") ?? "/") + "#section-services"
        });
    }

    private async Task<IndorProveedor?> LoadProviderAsync(int providerId, CancellationToken cancellationToken)
    {
        return await db.IndorProveedores
            .AsNoTracking()
            .Include(p => p.Categorias)
            .FirstOrDefaultAsync(
                p => p.Id == providerId && ActiveProviderStatuses.Contains(p.RegistrationStatus),
                cancellationToken);
    }

    private async Task<HomeownerMessageProviderViewModel> MapComposeAsync(
        IndorProveedor provider,
        IUrlHelper url,
        string? body,
        string? error,
        CancellationToken cancellationToken)
    {
        var categoryId = provider.Categorias.Select(c => c.CategoriaId).FirstOrDefault();
        string? trade = null;
        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            trade = await db.IndorProveedorCategoriasCatalogo
                .AsNoTracking()
                .Where(c => c.Id == categoryId)
                .Select(c => c.LabelEn)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new HomeownerMessageProviderViewModel
        {
            ProviderId = provider.Id,
            ProviderName = ResolveProviderName(provider),
            TradeLabel = trade,
            IconClass = "fa-screwdriver-wrench",
            Body = body ?? string.Empty,
            ErrorMessage = error,
            BackUrl = BuildProvidersFeedUrl(url)
        };
    }

    public static string BuildProviderMessageUrl(IUrlHelper url, int providerId) =>
        url.Action("Provider", "HomeownerMessage", new { id = providerId }) ?? "#";

    private static string BuildProvidersFeedUrl(IUrlHelper url) =>
        (url.Action("Index", "Home", new { filter = NearbyNetworkHomeownerFilters.Providers }) ?? "/")
        + "#section-home";

    private static string ResolveProviderName(IndorProveedor provider) =>
        !string.IsNullOrWhiteSpace(provider.DbaName)
            ? provider.DbaName.Trim()
            : !string.IsNullOrWhiteSpace(provider.BusinessName)
                ? provider.BusinessName.Trim()
                : "Provider";

    private static string BuildDisplayName(ApplicationUser user)
    {
        var full = $"{user.Nombre} {user.Apellidos}".Trim();
        if (!string.IsNullOrWhiteSpace(full))
        {
            return full;
        }

        return string.IsNullOrWhiteSpace(user.Email) ? "Homeowner" : user.Email!;
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string TruncatePreview(string body) =>
        body.Length <= 120 ? body : string.Concat(body.AsSpan(0, 117), "…");
}
