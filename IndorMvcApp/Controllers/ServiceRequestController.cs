using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Controllers;

/// <summary>
/// Homeowner-facing "Request a service" marketplace: create a request, browse mine,
/// see which provider took it, and cancel while still open.
/// </summary>
[Authorize]
[ResponseCache(NoStore = true, Duration = 0, Location = ResponseCacheLocation.None)]
public class ServiceRequestController(
    IServiceRequestService requests,
    INotificationService notifications,
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IIndorLocalizer localizer) : Controller
{
    private bool IsSpanish => localizer.IsSpanish;

    [HttpGet]
    public async Task<IActionResult> Create(string? category, int? propiedadId, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = await BuildCreateModelAsync(user, cancellationToken);
        model.CategoryId = category;
        model.PropiedadId = propiedadId ?? model.Properties.FirstOrDefault()?.Id;
        model.ContactPhone = string.IsNullOrWhiteSpace(user.Telefono) ? null : user.Telefono;
        if (model.PropiedadId.HasValue)
        {
            model.Address = model.Properties.FirstOrDefault(p => p.Id == model.PropiedadId)?.Address;
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestCreateViewModel form, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var validCategory = !string.IsNullOrWhiteSpace(form.CategoryId)
            && await db.IndorProveedorCategoriasCatalogo.AnyAsync(c => c.Id == form.CategoryId && c.Activo, cancellationToken);

        if (!validCategory || string.IsNullOrWhiteSpace(form.Title))
        {
            var model = await BuildCreateModelAsync(user, cancellationToken);
            model.CategoryId = form.CategoryId;
            model.PropiedadId = form.PropiedadId;
            model.Title = form.Title;
            model.Description = form.Description;
            model.Address = form.Address;
            model.ContactPhone = form.ContactPhone;
            model.PreferredDate = form.PreferredDate;
            model.PreferredTime = form.PreferredTime;
            model.BudgetAmount = form.BudgetAmount;
            model.Urgency = form.Urgency;
            model.ErrorMessage = string.IsNullOrWhiteSpace(form.Title)
                ? localizer.T("Please add a short title for your request.")
                : localizer.T("Please choose a service category.");
            return View(model);
        }

        // Validate property ownership if provided.
        int? propiedadId = form.PropiedadId;
        if (propiedadId.HasValue)
        {
            var owns = await db.Propiedades.AnyAsync(p => p.Id == propiedadId.Value && p.UserId == user.Id, cancellationToken);
            if (!owns)
            {
                propiedadId = null;
            }
        }

        var input = new CreateServiceRequestInput(
            CategoryId: form.CategoryId!,
            Title: form.Title!,
            Description: form.Description,
            PropiedadId: propiedadId,
            Address: form.Address,
            ContactPhone: form.ContactPhone,
            PreferredDate: form.PreferredDate,
            PreferredTime: form.PreferredTime,
            BudgetAmount: form.BudgetAmount,
            Urgency: form.Urgency);

        await requests.CreateAsync(user, input, cancellationToken);
        TempData["ServiceRequestToast"] = localizer.T("Your request was posted. We notified matching providers.");
        return RedirectToAction(nameof(Mine));
    }

    [HttpGet]
    public async Task<IActionResult> Mine(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = await requests.GetHomeownerRequestsAsync(userId, IsSpanish, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = await requests.GetHomeownerRequestDetailAsync(userId, id, IsSpanish, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(Mine));
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var ok = await requests.CancelAsync(userId, id, cancellationToken);
        TempData["ServiceRequestToast"] = ok
            ? localizer.T("Request cancelled.")
            : localizer.T("That request can no longer be cancelled.");
        return RedirectToAction(nameof(Mine));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNotificationsRead(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId != null)
        {
            await notifications.MarkAllReadAsync(userId, cancellationToken);
        }
        return Json(new { ok = true });
    }

    private async Task<ServiceRequestCreateViewModel> BuildCreateModelAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var categories = await requests.GetCategoryOptionsAsync(IsSpanish, cancellationToken);
        var properties = await db.Propiedades.AsNoTracking()
            .Where(p => p.UserId == user.Id && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => new ServiceRequestPropertyOption { Id = p.Id, Address = p.Direccion ?? "" })
            .ToListAsync(cancellationToken);

        return new ServiceRequestCreateViewModel
        {
            Categories = categories,
            Properties = properties,
            Urgency = "Standard"
        };
    }
}
