using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IndorMvcApp.Controllers;

[Authorize]
public class RealtorPropertyFileController(
    IRealtorPropertyFileWizardService wizard,
    IRealtorRegistrationService registration,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment env) : Controller
{
    private static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".webp", ".mp4", ".mov"];
    private const long MaxFileBytes = 15_000_000;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            context.Result = Challenge();
            return;
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null ||
            !string.Equals(user.RolUsuario, "Realtor", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = RedirectToAction("Index", "Home");
            return;
        }

        var realtor = await registration.GetRealtorForCurrentUserAsync();
        if (realtor == null || realtor.RegistrationStatus == RealtorRegistrationStatuses.Draft)
        {
            context.Result = RedirectToAction("Profile", "RealtorRegistration");
            return;
        }

        await next();
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Details));

    [HttpGet]
    public async Task<IActionResult> Details(string? q)
    {
        var draft = await wizard.GetDraftAsync();
        if (draft != null && draft.CurrentStep > 1)
        {
            return RedirectToAction(wizard.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await wizard.BuildDetailsAsync(q));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Details(int sourcePropertyId, string filePhase)
    {
        try
        {
            await wizard.SaveDetailsAsync(sourcePropertyId, filePhase);
            return RedirectToAction(nameof(AddItems));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Please select a property and file type.");
            var vm = await wizard.BuildDetailsAsync(null);
            vm.SelectedPropertyId = sourcePropertyId;
            vm.FilePhase = filePhase;
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> AddItems()
    {
        var draft = await wizard.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 2)
        {
            return RedirectToAction(nameof(Details));
        }

        if (draft.CurrentStep > 2)
        {
            return RedirectToAction(wizard.ResolveResumeAction(draft.CurrentStep));
        }

        return View(await wizard.BuildAddItemsAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItems(string[] categoryTypes)
    {
        try
        {
            await wizard.SaveAddItemsAsync(categoryTypes);
            return RedirectToAction(nameof(AddContent));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Select at least one item type.");
            return View(await wizard.BuildAddItemsAsync());
        }
    }

    [HttpGet]
    public async Task<IActionResult> AddContent()
    {
        var draft = await wizard.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 3)
        {
            return RedirectToAction(nameof(AddItems));
        }

        if (draft.CurrentStep > 3)
        {
            return RedirectToAction(nameof(Review));
        }

        return View(await wizard.BuildAddContentAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> UploadContent(
        string categoryType,
        string? itemLabel,
        string? noteText,
        string? warrantyLabel,
        DateTime? expirationDate,
        IFormFile? uploadFile)
    {
        var draft = await wizard.GetDraftAsync();
        if (draft == null)
        {
            return RedirectToAction(nameof(Details));
        }

        if (uploadFile != null && uploadFile.Length > 0)
        {
            var ext = Path.GetExtension(uploadFile.FileName);
            if (AllowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase) && uploadFile.Length <= MaxFileBytes)
            {
                var folder = Path.Combine(env.WebRootPath, "uploads", "realtor-property-files", draft.Id.ToString());
                Directory.CreateDirectory(folder);
                var fileName = $"{categoryType}-{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(folder, fileName);
                await using (var stream = System.IO.File.Create(fullPath))
                {
                    await uploadFile.CopyToAsync(stream);
                }

                var relativeUrl = $"/uploads/realtor-property-files/{draft.Id}/{fileName}";
                await wizard.SaveDraftItemAsync(
                    categoryType,
                    itemLabel ?? uploadFile.FileName,
                    relativeUrl,
                    uploadFile.Length,
                    null,
                    expirationDate?.ToUniversalTime(),
                    CancellationToken.None);
            }
        }
        else if (categoryType == RealtorPropertyFileCategoryTypes.NotesDocuments &&
                 !string.IsNullOrWhiteSpace(noteText))
        {
            await wizard.SaveDraftItemAsync(
                categoryType,
                "Note",
                null,
                null,
                noteText,
                null,
                CancellationToken.None);
        }
        else if (categoryType == RealtorPropertyFileCategoryTypes.Warranties &&
                 !string.IsNullOrWhiteSpace(warrantyLabel))
        {
            await wizard.SaveDraftItemAsync(
                categoryType,
                warrantyLabel,
                null,
                null,
                null,
                expirationDate?.ToUniversalTime(),
                CancellationToken.None);
        }

        return RedirectToAction(nameof(AddContent));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteContent(string? noteText)
    {
        await wizard.CompleteAddContentAsync(noteText);
        return RedirectToAction(nameof(Review));
    }

    [HttpGet]
    public async Task<IActionResult> Review()
    {
        var draft = await wizard.GetDraftAsync();
        if (draft == null || draft.CurrentStep < 4)
        {
            return RedirectToAction(nameof(AddContent));
        }

        return View(await wizard.BuildReviewAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(bool createAndContinueLater = false)
    {
        var fileId = await wizard.CreateFileAsync(createAndContinueLater);
        return RedirectToAction(nameof(Success), new { id = fileId });
    }

    [HttpGet]
    public async Task<IActionResult> Success(int id) =>
        View(await wizard.BuildSuccessAsync(id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel()
    {
        await wizard.CancelDraftAsync();
        return RedirectToAction("Files", "Realtor");
    }
}
