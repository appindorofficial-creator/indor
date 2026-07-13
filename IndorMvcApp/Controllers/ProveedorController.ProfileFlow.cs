using IndorMvcApp.Models;
using IndorMvcApp.Validation;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

/// <summary>
/// Guided profile completion flow: public profile hub, business profile, documents.
/// </summary>
public partial class ProveedorController
{
    [HttpGet]
    public async Task<IActionResult> ProfileCompletion(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetProfileCompletionAsync(proveedor.Entity!, cancellationToken);
        ViewBag.CompanyInitial = model.CompanyInitial;
        ViewData["ProviderProExtraCss"] = "provider-profile-flow.css";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ProfileBusiness(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetProfileBusinessAsync(proveedor.Entity!, cancellationToken: cancellationToken);
        ViewBag.CompanyInitial = model.CompanyInitial;
        ViewBag.ProfileFlowStep = 2;
        ViewData["ProviderProExtraCss"] = "provider-profile-flow.css";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProfileBusiness(ProviderProfileBusinessInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (!UsPhoneOptionalAttribute.IsValidOptional(input.Phone))
        {
            ModelState.AddModelError(nameof(ProviderProfileBusinessInput.Phone),
                "Enter a valid 10-digit US phone number (e.g. 555 123 4567).");
        }

        if (!ModelState.IsValid)
        {
            var invalid = await proData.GetProfileBusinessAsync(proveedor.Entity!, input, cancellationToken);
            ViewBag.CompanyInitial = invalid.CompanyInitial;
            ViewBag.ProfileFlowStep = 2;
            ViewData["ProviderProExtraCss"] = "provider-profile-flow.css";
            return View(invalid);
        }

        var saved = await proData.SaveProfileBusinessAsync(proveedor.Entity!.Id, input, cancellationToken);
        if (!saved)
        {
            var failed = await proData.GetProfileBusinessAsync(proveedor.Entity!, input, cancellationToken);
            failed.ErrorMessage = localizer["We couldn't save your business profile. Please try again."];
            ViewBag.CompanyInitial = failed.CompanyInitial;
            ViewBag.ProfileFlowStep = 2;
            ViewData["ProviderProExtraCss"] = "provider-profile-flow.css";
            return View(failed);
        }

        TempData["ProfileSaved"] = true;

        if (string.Equals(input.SaveMode, "exit", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Profile));
        }

        if (string.Equals(input.SaveMode, "continue", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(ProfileDocuments));
        }

        return RedirectToAction(nameof(ProfileBusiness));
    }

    [HttpGet]
    public async Task<IActionResult> ProfileDocuments(string? section, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetProfileDocumentsAsync(proveedor.Entity!, section, cancellationToken);
        ViewBag.CompanyInitial = model.CompanyInitial;
        ViewBag.ProfileFlowStep = 3;
        ViewData["ProviderProExtraCss"] = "provider-profile-flow.css";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> ProfileDocuments(
        ProviderProfileDocumentsInput input,
        string? documentType,
        IFormFile? documentFile,
        CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var proveedorId = proveedor.Entity!.Id;

        if (documentFile != null && documentFile.Length > 0 && !string.IsNullOrWhiteSpace(documentType))
        {
            var uploadError = await SaveProviderDocumentAsync(proveedorId, documentType.Trim(), documentFile);
            if (!string.IsNullOrWhiteSpace(uploadError))
            {
                TempData["ProfileSectionError"] = uploadError;
                return RedirectToAction(nameof(ProfileDocuments), new { section = input.Section ?? documentType });
            }

            TempData["ProfileSaved"] = true;

            if (string.Equals(input.SaveMode, "upload", StringComparison.OrdinalIgnoreCase))
            {
                // Persist any filled section fields alongside the file, then stay on this section.
                await proData.SaveProfileDocumentsAsync(proveedorId, input, cancellationToken);
                return RedirectToAction(nameof(ProfileDocuments), new { section = input.Section });
            }
        }
        else if (string.Equals(input.SaveMode, "upload", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ProfileSectionError"] = localizer["Choose a file to upload."].ToString();
            return RedirectToAction(nameof(ProfileDocuments), new { section = input.Section });
        }

        await proData.SaveProfileDocumentsAsync(proveedorId, input, cancellationToken);
        TempData["ProfileSaved"] = true;

        if (string.Equals(input.SaveMode, "exit", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Profile));
        }

        if (string.Equals(input.SaveMode, "continue", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(ProfileCompletion));
        }

        return RedirectToAction(nameof(ProfileDocuments), new { section = input.Section });
    }
}
