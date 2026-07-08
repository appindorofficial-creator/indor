using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

/// <summary>
/// My Requests — the job poster's side of the Contractor Network: track posted
/// jobs, review incoming quotes, compare them, hire a pro, and see the
/// confirmation. Reads/writes jobs, quotes and hires via
/// <see cref="Services.INetworkRequestsService"/>.
/// </summary>
public partial class ProveedorController
{
    [HttpGet]
    public async Task<IActionResult> MyRequests(string? tab, string? q, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await requests.GetMyRequestsAsync(proveedor.Entity!, tab, q, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> RequestDetails(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await requests.GetDetailsAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> CompareQuotes(int id, string? sort, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await requests.GetCompareAsync(proveedor.Entity!, id, sort, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectQuote(int id, int quoteId, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var ok = await requests.SelectQuoteAsync(proveedor.Entity!, id, quoteId, cancellationToken);
        if (!ok)
        {
            return RedirectToAction(nameof(CompareQuotes), new { id });
        }

        return RedirectToAction(nameof(RequestConfirmed), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> RequestConfirmed(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await requests.GetConfirmedAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    // ------------------------------------------------------ Invite to Job

    private static readonly string[] InviteAttachmentExtensions = [".jpg", ".jpeg", ".png", ".webp", ".pdf"];

    [HttpGet]
    public async Task<IActionResult> InviteToJob(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await requests.GetInviteAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(60_000_000)]
    public async Task<IActionResult> InviteToJob(InviteToJobInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var newAttachments = new List<string>();
        if (input.Attachments is { Count: > 0 })
        {
            foreach (var file in input.Attachments.Where(f => f is { Length: > 0 }).Take(8))
            {
                var url = await SaveInviteAttachmentAsync(proveedor.Entity!.Id, file);
                if (!string.IsNullOrWhiteSpace(url))
                {
                    newAttachments.Add(url);
                }
            }
        }

        var isDraft = string.Equals(input.Mode, "draft", StringComparison.OrdinalIgnoreCase);
        if (!isDraft && string.IsNullOrWhiteSpace(input.JobTitle))
        {
            var model = await requests.GetInviteAsync(proveedor.Entity!, input.SubcontractorId, cancellationToken);
            if (model == null)
            {
                return NotFound();
            }

            model.JobTitle = input.JobTitle;
            model.ServiceCategory = input.ServiceCategory;
            model.TradeId = input.TradeId;
            model.PropertyAddress = input.PropertyAddress;
            model.ScheduleToday = input.ScheduleToday;
            model.ScheduleDate = input.ScheduleDate;
            model.BudgetRange = input.BudgetRange;
            model.Description = input.Description;
            model.TimingPreference = input.TimingPreference ?? model.TimingPreference;
            model.Attachments.Clear();
            model.Attachments.AddRange((input.ExistingAttachments ?? []).Where(u => !string.IsNullOrWhiteSpace(u)));
            model.Attachments.AddRange(newAttachments);
            model.ErrorMessage = "Please add a job title before sending.";
            return View(model);
        }

        var invitationId = await requests.SaveInviteAsync(proveedor.Entity!, input, newAttachments, cancellationToken);
        if (invitationId == null)
        {
            return RedirectToAction(nameof(FindSubcontractors));
        }

        if (isDraft)
        {
            TempData["NetworkToast"] = "Invite saved as draft.";
            return RedirectToAction(nameof(SubcontractorProfile), new { id = input.SubcontractorId });
        }

        return RedirectToAction(nameof(InvitationSent), new { id = invitationId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> InvitationSent(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await requests.GetInvitationSentAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    private async Task<string?> SaveInviteAttachmentAsync(int proveedorId, IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) && file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            ext = ".jpg";
        }

        if (!InviteAttachmentExtensions.Contains(ext) || file.Length > 10_000_000)
        {
            return null;
        }

        var folder = Path.Combine(env.WebRootPath, "uploads", "network-invites", proveedorId.ToString());
        Directory.CreateDirectory(folder);

        var stored = $"{Guid.NewGuid():N}{ext}";
        var physical = Path.Combine(folder, stored);
        await using (var stream = System.IO.File.Create(physical))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/network-invites/{proveedorId}/{stored}";
    }
}
