using System.Text.Json;

using IndorMvcApp.Models;

using IndorMvcApp.Services;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Identity;

using IndorMvcApp.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;



namespace IndorMvcApp.Controllers;



[Authorize]

public class ProveedorController(

    UserManager<ApplicationUser> userManager,

    SignInManager<ApplicationUser> signInManager,

    IProviderRegistrationService registration,

    ProviderProDashboardService dashboardService,

    IProviderProDataService proData,

    IProviderProJobWorkflowService jobWorkflow,

    IWebHostEnvironment env) : Controller

{

    [HttpGet]

    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)

    {

        var proveedor = await ResolveProveedorAsync(cancellationToken);

        if (proveedor.Result != null)

        {

            return proveedor.Result;

        }



        var model = await dashboardService.BuildAsync(proveedor.Entity!, cancellationToken);

        return View(model);

    }



    [HttpGet]

    public async Task<IActionResult> Jobs(string? tab, string? q, CancellationToken cancellationToken)

    {

        var proveedor = await ResolveProveedorAsync(cancellationToken);

        if (proveedor.Result != null)

        {

            return proveedor.Result;

        }



        var model = await proData.GetJobsPageAsync(proveedor.Entity!, tab, q, cancellationToken);

        return View(model);

    }



    [HttpGet]
    public async Task<IActionResult> AddCustomer(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        ClearAddCustomerDraft();
        var model = proData.GetAddCustomerInfoViewModel(proveedor.Entity!, null);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCustomer(ProviderProAddCustomerInfoInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (string.IsNullOrWhiteSpace(input.FirstName) || string.IsNullOrWhiteSpace(input.LastName))
        {
            return RedirectToAction(nameof(AddCustomer));
        }

        var draft = GetAddCustomerDraft();
        draft.CustomerType = input.CustomerType;
        draft.FirstName = input.FirstName.Trim();
        draft.LastName = input.LastName.Trim();
        draft.Phone = input.Phone.Trim();
        draft.Email = input.Email.Trim();
        draft.PreferredContactMethod = input.PreferredContactMethod;
        draft.CompanyName = input.CompanyName.Trim();
        SaveAddCustomerDraft(draft);

        return RedirectToAction(nameof(AddCustomerProperty));
    }

    [HttpGet]
    public async Task<IActionResult> AddCustomerProperty(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetAddCustomerDraft();
        if (string.IsNullOrWhiteSpace(draft.FirstName))
        {
            return RedirectToAction(nameof(AddCustomer));
        }

        var model = proData.GetAddCustomerPropertyViewModel(proveedor.Entity!, draft);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCustomerProperty(ProviderProAddCustomerPropertyInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetAddCustomerDraft();
        draft.StreetAddress = input.StreetAddress.Trim();
        draft.AptUnit = input.AptUnit.Trim();
        draft.City = input.City.Trim();
        draft.State = input.State;
        draft.ZipCode = input.ZipCode.Trim();
        draft.PropertyType = input.PropertyType;
        draft.Bedrooms = input.Bedrooms;
        draft.Bathrooms = input.Bathrooms;
        draft.IsBillingAddressSame = input.IsBillingAddressSame;
        draft.AccessNotes = input.AccessNotes.Trim();
        SaveAddCustomerDraft(draft);

        if (string.IsNullOrWhiteSpace(draft.StreetAddress) || string.IsNullOrWhiteSpace(draft.City))
        {
            return RedirectToAction(nameof(AddCustomerProperty));
        }

        return RedirectToAction(nameof(AddCustomerPreferences));
    }

    [HttpGet]
    public async Task<IActionResult> AddCustomerPreferences(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetAddCustomerDraft();
        if (string.IsNullOrWhiteSpace(draft.StreetAddress))
        {
            return RedirectToAction(nameof(AddCustomer));
        }

        var model = proData.GetAddCustomerPreferencesViewModel(proveedor.Entity!, draft);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCustomerPreferences(ProviderProAddCustomerPreferencesInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetAddCustomerDraft();
        draft.EstimateDeliveryPref = input.EstimateDeliveryPref;
        draft.InvoiceDeliveryPref = input.InvoiceDeliveryPref;
        draft.PreferredLanguage = input.PreferredLanguage;
        draft.CustomerSource = input.CustomerSource;
        draft.Tags = input.Tags.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
        draft.InternalNotes = input.InternalNotes.Trim();
        draft.SendIndorInvite = input.SendIndorInvite;
        draft.AllowServiceUpdates = input.AllowServiceUpdates;
        SaveAddCustomerDraft(draft);

        return RedirectToAction(nameof(AddCustomerReview));
    }

    [HttpGet]
    public async Task<IActionResult> AddCustomerReview(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetAddCustomerDraft();
        var model = proData.GetAddCustomerReviewViewModel(proveedor.Entity!, draft);
        if (model == null)
        {
            return RedirectToAction(nameof(AddCustomer));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmAddCustomer(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetAddCustomerDraft();
        var customerId = await proData.SaveAddCustomerFromDraftAsync(proveedor.Entity!.Id, draft, cancellationToken);
        if (!customerId.HasValue)
        {
            return RedirectToAction(nameof(AddCustomer));
        }

        ClearAddCustomerDraft();
        return RedirectToAction(nameof(AddCustomerSuccess), new { id = customerId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> AddCustomerSuccess(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetAddCustomerSuccessAsync(proveedor.Entity!, id, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(Customers));
        }

        return View(model);
    }

    [HttpGet]

    public async Task<IActionResult> Customers(string? tab, string? q, CancellationToken cancellationToken)

    {

        var proveedor = await ResolveProveedorAsync(cancellationToken);

        if (proveedor.Result != null)

        {

            return proveedor.Result;

        }



        var model = await proData.GetCustomersPageAsync(proveedor.Entity!, tab, q, cancellationToken);

        return View(model);

    }



    [HttpGet]

    public async Task<IActionResult> Reports(string? tab, string? q, CancellationToken cancellationToken)

    {

        var proveedor = await ResolveProveedorAsync(cancellationToken);

        if (proveedor.Result != null)

        {

            return proveedor.Result;

        }



        var model = await proData.GetReportsPageAsync(proveedor.Entity!, tab, q, cancellationToken);

        return View(model);

    }

    [HttpGet]
    public async Task<IActionResult> UploadReport(string? q, string? filter, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        ClearUploadReportDraft();
        var model = await proData.GetUploadReportSelectJobAsync(proveedor.Entity!, null, q, filter, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadReport(ProviderProUploadReportSelectJobInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (input.JobId <= 0)
        {
            return RedirectToAction(nameof(UploadReport), new { q = input.Search, filter = input.Filter });
        }

        var draft = GetUploadReportDraft();
        draft.JobId = input.JobId;
        SaveUploadReportDraft(draft);
        return RedirectToAction(nameof(UploadReportType));
    }

    [HttpGet]
    public async Task<IActionResult> UploadReportType(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetUploadReportDraft();
        if (!draft.JobId.HasValue)
        {
            return RedirectToAction(nameof(UploadReport));
        }

        var model = await proData.GetUploadReportTypeAsync(proveedor.Entity!, draft, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(UploadReport));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadReportType(ProviderProUploadReportTypeInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetUploadReportDraft();
        draft.ReportType = input.ReportType;
        SaveUploadReportDraft(draft);
        return RedirectToAction(nameof(UploadReportFiles));
    }

    [HttpGet]
    public async Task<IActionResult> UploadReportFiles(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetUploadReportDraft();
        if (!draft.JobId.HasValue || string.IsNullOrWhiteSpace(draft.ReportType))
        {
            return RedirectToAction(nameof(UploadReport));
        }

        var model = await proData.GetUploadReportFilesAsync(proveedor.Entity!, draft, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(UploadReport));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadReportFiles(
        ProviderProUploadReportFilesInput input,
        IFormFile? photoBefore,
        IFormFile? photoDuring,
        IFormFile? photoAfter,
        IFormFile? photoFinal,
        IFormFile? docInvoice,
        IFormFile? docWarranty,
        IFormFile? docPermit,
        List<IFormFile>? generalFiles,
        CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetUploadReportDraft();
        draft.AttachToHouseFacts = input.AttachToHouseFacts;

        await ApplyReportFileToSlotAsync(proveedor.Entity!.Id, draft.PhotoSlots, "Before", photoBefore);
        await ApplyReportFileToSlotAsync(proveedor.Entity!.Id, draft.PhotoSlots, "During", photoDuring);
        await ApplyReportFileToSlotAsync(proveedor.Entity!.Id, draft.PhotoSlots, "After", photoAfter);
        await ApplyReportFileToSlotAsync(proveedor.Entity!.Id, draft.PhotoSlots, "Final Result", photoFinal);
        await ApplyReportFileToSlotAsync(proveedor.Entity!.Id, draft.DocumentSlots, "Invoice", docInvoice);
        await ApplyReportFileToSlotAsync(proveedor.Entity!.Id, draft.DocumentSlots, "Warranty Document", docWarranty);
        await ApplyReportFileToSlotAsync(proveedor.Entity!.Id, draft.DocumentSlots, "Permit / Receipt", docPermit);

        if (generalFiles != null)
        {
            foreach (var file in generalFiles.Where(f => f.Length > 0))
            {
                var saved = await SaveProviderReportFileAsync(proveedor.Entity!.Id, file);
                if (saved != null)
                {
                    draft.GeneralFiles.Add(new ProviderProUploadReportFileSlot
                    {
                        Slot = "General",
                        Url = saved.Value.Url,
                        FileName = saved.Value.FileName
                    });
                }
            }
        }

        SaveUploadReportDraft(draft);
        return RedirectToAction(nameof(UploadReportDetails));
    }

    [HttpGet]
    public async Task<IActionResult> UploadReportDetails(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetUploadReportDraft();
        if (!draft.JobId.HasValue)
        {
            return RedirectToAction(nameof(UploadReport));
        }

        var model = await proData.GetUploadReportDetailsAsync(proveedor.Entity!, draft, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(UploadReport));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadReportDetails(ProviderProUploadReportDetailsInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetUploadReportDraft();
        draft.Title = input.Title.Trim();
        draft.Summary = input.Summary.Trim();
        draft.WorkCompleted = input.WorkCompleted.Trim();
        draft.MaterialsUsed = input.MaterialsUsed.Trim();
        draft.WarrantyInfo = input.WarrantyInfo.Trim();
        draft.Recommendations = input.Recommendations.Trim();
        draft.InternalNotes = input.InternalNotes.Trim();
        draft.SendToHomeowner = input.SendToHomeowner;
        draft.RequestApproval = input.RequestApproval;
        draft.CreateHouseFactsRecord = input.CreateHouseFactsRecord;
        SaveUploadReportDraft(draft);

        if (string.IsNullOrWhiteSpace(draft.Title))
        {
            return RedirectToAction(nameof(UploadReportDetails));
        }

        var reportId = await proData.SaveUploadReportFromDraftAsync(proveedor.Entity!.Id, draft, cancellationToken);
        if (!reportId.HasValue)
        {
            return RedirectToAction(nameof(UploadReport));
        }

        ClearUploadReportDraft();
        return RedirectToAction(nameof(UploadReportSuccess), new { id = reportId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> UploadReportSuccess(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetUploadReportSuccessAsync(proveedor.Entity!, id, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(Reports));
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Messages(string? tab, string? q, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetMessagesInboxAsync(proveedor.Entity!, tab, q, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Conversation(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetConversationAsync(proveedor.Entity!, id, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(Messages));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Conversation(ProviderProSendMessageInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (!string.IsNullOrWhiteSpace(input.Body))
        {
            await proData.SendConversationMessageAsync(proveedor.Entity!.Id, input, cancellationToken);
        }

        return RedirectToAction(nameof(Conversation), new { id = input.ConversationId });
    }

    [HttpGet]
    public async Task<IActionResult> MessageQuickActions(int id, string? action, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetMessageQuickActionsAsync(proveedor.Entity!, id, action, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(Messages));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MessageQuickActions(ProviderProMessageQuickActionInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (string.IsNullOrWhiteSpace(input.ActionType))
        {
            return RedirectToAction(nameof(MessageQuickActions), new { id = input.ConversationId });
        }

        var draft = new ProviderProMessageActionDraft
        {
            ConversationId = input.ConversationId,
            ActionType = input.ActionType.Trim()
        };

        var sent = await proData.SendMessageQuickActionAsync(proveedor.Entity!.Id, draft, cancellationToken);
        if (!sent)
        {
            return RedirectToAction(nameof(Conversation), new { id = input.ConversationId });
        }

        return RedirectToAction(nameof(MessageSentSuccess), new
        {
            conversationId = input.ConversationId,
            actionLabel = draft.ActionLabel
        });
    }

    [HttpGet]
    public async Task<IActionResult> MessageSentSuccess(int conversationId, string? actionLabel, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetMessageSentSuccessAsync(
            proveedor.Entity!,
            conversationId,
            actionLabel ?? "",
            cancellationToken);

        if (model == null)
        {
            return RedirectToAction(nameof(Messages));
        }

        return View(model);
    }

    [HttpGet]

    public async Task<IActionResult> Profile(CancellationToken cancellationToken)

    {

        var proveedor = await ResolveProveedorAsync(cancellationToken);

        if (proveedor.Result != null)

        {

            return proveedor.Result;

        }



        var model = await proData.GetProfilePageAsync(proveedor.Entity!, cancellationToken);

        return View(model);

    }

    [HttpGet]
    public async Task<IActionResult> NewLeads(string? filter, string? q, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetNewLeadsPageAsync(proveedor.Entity!, filter, q, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> LeadDetails(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetLeadDetailsAsync(proveedor.Entity!, id, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptLead(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var accepted = await proData.AcceptLeadAsync(proveedor.Entity!.Id, id, cancellationToken);
        if (!accepted)
        {
            return RedirectToAction(nameof(LeadDetails), new { id });
        }

        TempData["LeadAccepted"] = true;
        return RedirectToAction(nameof(LeadDetails), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeclineLead(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        await proData.DeclineLeadAsync(proveedor.Entity!.Id, id, cancellationToken);
        return RedirectToAction(nameof(NewLeads));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveHomeowner(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        await proData.ApproveHomeownerRequestAsync(proveedor.Entity!.Id, id, cancellationToken);
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public async Task<IActionResult> CreateEstimate(int? clienteId, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        ClearCreateEstimateDraft();
        var draft = new ProviderProCreateEstimateDraft();
        if (clienteId.HasValue)
        {
            draft.ClienteId = clienteId;
            await proData.ApplyCreateEstimateSourcePrefillAsync(proveedor.Entity!.Id, draft, cancellationToken);
            SaveCreateEstimateDraft(draft);
        }

        var model = await proData.GetCreateEstimateSetupAsync(proveedor.Entity!, draft, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEstimate(ProviderProCreateEstimateSetupInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetCreateEstimateDraft();
        draft.EstimateType = input.EstimateType;
        draft.ClienteId = input.ClienteId;
        draft.CustomerName = input.CustomerName.Trim();
        draft.Address = input.Address.Trim();
        draft.ServiceCategoryId = input.ServiceCategoryId;
        draft.LeadId = input.LeadId;
        draft.JobId = input.JobId;

        await proData.ApplyCreateEstimateSourcePrefillAsync(proveedor.Entity!.Id, draft, cancellationToken);
        SaveCreateEstimateDraft(draft);

        if (string.IsNullOrWhiteSpace(draft.Address) || string.IsNullOrWhiteSpace(draft.CustomerName))
        {
            return RedirectToAction(nameof(CreateEstimate));
        }

        return RedirectToAction(nameof(CreateEstimateDetails));
    }

    [HttpGet]
    public async Task<IActionResult> CreateEstimateDetails(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetCreateEstimateDraft();
        var model = await proData.GetCreateEstimateDetailsAsync(proveedor.Entity!, draft);
        if (model == null)
        {
            return RedirectToAction(nameof(CreateEstimate));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEstimateDetails(ProviderProCreateEstimateDetailsInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetCreateEstimateDraft();
        draft.Title = input.Title.Trim();
        draft.Description = input.Description.Trim();
        draft.Priority = input.Priority;
        draft.EstimatedStartDate = input.EstimatedStartDate;
        draft.EstimatedEndDate = input.EstimatedEndDate;
        draft.Warranty = input.Warranty;
        draft.Notes = input.Notes.Trim();
        SaveCreateEstimateDraft(draft);

        return RedirectToAction(nameof(CreateEstimatePricing));
    }

    [HttpGet]
    public async Task<IActionResult> CreateEstimatePricing(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetCreateEstimateDraft();
        var model = await proData.GetCreateEstimatePricingAsync(proveedor.Entity!, draft);
        if (model == null)
        {
            return RedirectToAction(nameof(CreateEstimate));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEstimatePricing(ProviderProCreateEstimatePricingInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetCreateEstimateDraft();
        draft.TaxRate = input.TaxRate > 0 ? input.TaxRate : 0.0825m;
        draft.LineItems = BuildEstimateLineItemsFromInput(input);
        SaveCreateEstimateDraft(draft);

        return RedirectToAction(nameof(CreateEstimateReview));
    }

    [HttpGet]
    public async Task<IActionResult> CreateEstimateReview(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetCreateEstimateDraft();
        await proData.SaveCreateEstimateFromDraftAsync(proveedor.Entity!.Id, draft, readyForReview: true, cancellationToken);
        SaveCreateEstimateDraft(draft);

        var model = await proData.GetCreateEstimateReviewAsync(proveedor.Entity!, draft);
        if (model == null)
        {
            return RedirectToAction(nameof(CreateEstimate));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmSendEstimate(ProviderProSendEstimateInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetCreateEstimateDraft();
        draft.DeliveryMethod = input.DeliveryMethod;
        var estimateId = await proData.SaveCreateEstimateFromDraftAsync(
            proveedor.Entity!.Id,
            draft,
            readyForReview: !input.SaveAsDraft,
            cancellationToken);

        if (!estimateId.HasValue)
        {
            return RedirectToAction(nameof(CreateEstimate));
        }

        if (input.SaveAsDraft)
        {
            ClearCreateEstimateDraft();
            TempData["EstimateDraftSaved"] = true;
            return RedirectToAction(nameof(PendingEstimates), new { tab = "draft" });
        }

        input.EstimateId = estimateId.Value;
        var sent = await proData.SendEstimateAsync(proveedor.Entity!.Id, input, cancellationToken);
        ClearCreateEstimateDraft();

        if (!sent)
        {
            return RedirectToAction(nameof(CreateEstimateReview));
        }

        return RedirectToAction(nameof(EstimateSent), new { id = estimateId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> QuickEstimate(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetQuickEstimateAsync(proveedor.Entity!, id, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickEstimate(ProviderProQuickEstimateInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var estimateId = await proData.SaveQuickEstimateAsync(proveedor.Entity!.Id, input, cancellationToken);
        if (!estimateId.HasValue)
        {
            return RedirectToAction(nameof(LeadDetails), new { id = input.LeadId });
        }

        if (input.GoToReview)
        {
            return RedirectToAction(nameof(ReviewEstimate), new { id = estimateId.Value });
        }

        TempData["EstimateDraftSaved"] = true;
        return RedirectToAction(nameof(QuickEstimate), new { id = input.LeadId });
    }

    [HttpGet]
    public async Task<IActionResult> ReviewEstimate(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetReviewEstimateAsync(proveedor.Entity!, id, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendEstimate(ProviderProSendEstimateInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetReviewEstimateAsync(proveedor.Entity!, input.EstimateId, cancellationToken);
        var sent = await proData.SendEstimateAsync(proveedor.Entity!.Id, input, cancellationToken);
        if (!sent || model == null)
        {
            return RedirectToAction(nameof(ReviewEstimate), new { id = input.EstimateId });
        }

        return RedirectToAction(nameof(EstimateSent), new { id = input.EstimateId });
    }

    [HttpGet]
    public async Task<IActionResult> PendingEstimates(string? tab, string? q, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetPendingEstimatesPageAsync(proveedor.Entity!, tab, q, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditEstimate(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetEditEstimateAsync(proveedor.Entity!, id, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEstimate(ProviderProQuickEstimateInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var estimateId = await proData.SaveQuickEstimateAsync(proveedor.Entity!.Id, input, cancellationToken);
        if (!estimateId.HasValue)
        {
            return RedirectToAction(nameof(PendingEstimates));
        }

        if (input.GoToReview)
        {
            return RedirectToAction(nameof(ReviewEstimate), new { id = estimateId.Value });
        }

        TempData["EstimateDraftSaved"] = true;
        return RedirectToAction(nameof(EditEstimate), new { id = estimateId.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendEstimateFromHub(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var sent = await proData.SendEstimateAsync(proveedor.Entity!.Id, new ProviderProSendEstimateInput { EstimateId = id }, cancellationToken);
        if (!sent)
        {
            return RedirectToAction(nameof(PendingEstimates));
        }

        return RedirectToAction(nameof(EstimateSent), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> EstimateSent(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetEstimateSentAsync(proveedor.Entity!, id, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConvertEstimateToJob(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var jobId = await proData.ConvertEstimateToJobAsync(proveedor.Entity!.Id, id, cancellationToken);
        if (!jobId.HasValue)
        {
            return RedirectToAction(nameof(EstimateSent), new { id });
        }

        return RedirectToAction(nameof(JobDetails), new { id = jobId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Invoices(string? tab, string? q, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetInvoicesPageAsync(proveedor.Entity!, tab, q, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> InvoiceDetails(int id, string? from, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetInvoiceDetailsAsync(proveedor.Entity!, id, from, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> SendInvoiceReminder(int id, string? from, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetSendInvoiceReminderAsync(proveedor.Entity!, id, from, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendInvoiceReminder(ProviderProSendInvoiceReminderInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var sent = await proData.SendInvoiceReminderAsync(proveedor.Entity!.Id, input, cancellationToken);
        if (!sent)
        {
            return RedirectToAction(nameof(Invoices));
        }

        TempData["InvoiceReminderSent"] = true;
        return RedirectToAction(nameof(InvoiceDetails), new { id = input.InvoiceId, from = input.ReturnTab });
    }

    [HttpGet]
    public async Task<IActionResult> RecordPayment(int id, string? from, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetRecordPaymentAsync(proveedor.Entity!, id, from, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(ProviderProRecordPaymentInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var recorded = await proData.RecordInvoicePaymentAsync(proveedor.Entity!.Id, input, cancellationToken);
        if (!recorded)
        {
            return RedirectToAction(nameof(Invoices), new { tab = input.ReturnTab ?? "pending" });
        }

        TempData["InvoicePaymentRecorded"] = true;
        TempData["InvoicePaymentCode"] = input.InvoiceId;
        return RedirectToAction(nameof(Invoices), new { tab = "paid" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendInvoiceReceipt(int id, string? from, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var sent = await proData.SendInvoiceReceiptAsync(proveedor.Entity!.Id, id, cancellationToken);
        if (!sent)
        {
            return RedirectToAction(nameof(Invoices), new { tab = from ?? "paid" });
        }

        TempData["InvoiceReceiptSent"] = true;
        return RedirectToAction(nameof(InvoiceDetails), new { id, from = from ?? "paid" });
    }

    [HttpGet]
    public async Task<IActionResult> DownloadInvoice(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetInvoiceDetailsAsync(proveedor.Entity!, id, cancellationToken: cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        var lines = string.Join("\n", model.LineItems.Select(l =>
            $"{l.Description}\t{l.Qty}\t{l.Rate:C}\t{l.Amount:C}"));
        var content =
            $"Invoice {model.InvoiceCode}\n" +
            $"{model.ServiceType} — {model.Address}\n" +
            $"Amount Due: {model.Amount:C}\n" +
            $"Due: {model.DueDateLabel}\n\n" +
            $"Item\tQty\tRate\tAmount\n{lines}\n\nTotal\t\t\t{model.Amount:C}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        return File(bytes, "text/plain", $"invoice-{model.InvoiceCode.TrimStart('#')}.txt");
    }

    [HttpGet]
    public async Task<IActionResult> ScheduleVisit(int id, string? kind, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetScheduleVisitAsync(proveedor.Entity!, id, kind, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ScheduleVisit(ProviderProScheduleVisitInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var jobId = await proData.ConfirmScheduleVisitAsync(proveedor.Entity!.Id, input, cancellationToken);
        if (!jobId.HasValue)
        {
            return RedirectToAction(nameof(LeadDetails), new { id = input.LeadId });
        }

        if (input.SaveAsDraft)
        {
            TempData["VisitDraftSaved"] = true;
            return RedirectToAction(nameof(Jobs), new { tab = "today" });
        }

        TempData["VisitConfirmed"] = true;
        return RedirectToAction(nameof(JobDetails), new { id = jobId.Value });
    }

    [HttpGet]
    public IActionResult JobsSchedule(string? view, string? date)
    {
        if (string.Equals(view, "today", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(view))
        {
            return RedirectToAction(nameof(DaySchedule), new { date });
        }

        return RedirectToAction(nameof(Calendar), new { view = view ?? "week" });
    }

    [HttpGet]
    public async Task<IActionResult> Calendar(string? view, string? filter, string? weekStart, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (string.Equals(view, "day", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(DaySchedule), new { date = weekStart });
        }

        var model = await jobWorkflow.GetCalendarOverviewAsync(
            proveedor.Entity!, view, filter, weekStart, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DaySchedule(string? date, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await jobWorkflow.GetDayScheduleAsync(proveedor.Entity!, date, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> RescheduleJob(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await jobWorkflow.GetRescheduleJobAsync(proveedor.Entity!, id, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RescheduleJob(ProviderProRescheduleJobInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var saved = await jobWorkflow.RescheduleJobAsync(proveedor.Entity!.Id, input, cancellationToken);
        if (!saved)
        {
            return RedirectToAction(nameof(RescheduleJob), new { id = input.JobId });
        }

        return RedirectToAction(nameof(CalendarUpdated), new { id = input.JobId });
    }

    [HttpGet]
    public async Task<IActionResult> CalendarUpdated(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await jobWorkflow.GetCalendarUpdatedAsync(proveedor.Entity!, id, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(Calendar));
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> CreateJob(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        ClearCreateJobDraft();
        var model = await jobWorkflow.GetCreateJobCategoriesAsync(proveedor.Entity!, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> CreateJobDetails(string categoryId, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetCreateJobDraft();
        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            draft.ServiceCategoryId = categoryId;
        }

        if (string.IsNullOrWhiteSpace(draft.ServiceCategoryId))
        {
            return RedirectToAction(nameof(CreateJob));
        }

        var model = await jobWorkflow.GetCreateJobDetailsAsync(
            proveedor.Entity!, draft.ServiceCategoryId, draft, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(CreateJob));
        }

        draft.ServiceCategoryLabel = model.ServiceCategoryLabel;
        SaveCreateJobDraft(draft);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJobDetails(ProviderProCreateJobDetailsInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (string.IsNullOrWhiteSpace(input.Title) || string.IsNullOrWhiteSpace(input.Address)
            || (input.ClienteId == null && string.IsNullOrWhiteSpace(input.CustomerName)))
        {
            return RedirectToAction(nameof(CreateJobDetails), new { categoryId = input.ServiceCategoryId });
        }

        var draft = GetCreateJobDraft();
        draft.ServiceCategoryId = input.ServiceCategoryId;
        draft.Title = input.Title.Trim();
        draft.ClienteId = input.ClienteId;
        draft.CustomerName = input.CustomerName.Trim();
        draft.Address = input.Address.Trim();
        draft.Description = input.Description.Trim();
        draft.Priority = input.Priority;
        draft.Notes = input.Notes.Trim();
        SaveCreateJobDraft(draft);

        return RedirectToAction(nameof(CreateJobSchedule));
    }

    [HttpGet]
    public async Task<IActionResult> CreateJobSchedule(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetCreateJobDraft();
        if (string.IsNullOrWhiteSpace(draft.ServiceCategoryId) || string.IsNullOrWhiteSpace(draft.Title))
        {
            return RedirectToAction(nameof(CreateJob));
        }

        var model = await jobWorkflow.GetCreateJobScheduleAsync(proveedor.Entity!, draft, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(CreateJob));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJobSchedule(ProviderProCreateJobScheduleInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetCreateJobDraft();
        if (string.IsNullOrWhiteSpace(draft.Title))
        {
            return RedirectToAction(nameof(CreateJob));
        }

        draft.VisitDate = input.VisitDate;
        draft.StartTimeLabel = input.StartTimeLabel;
        draft.EndTimeLabel = input.EndTimeLabel;
        draft.AddToCalendar = input.AddToCalendar;
        draft.Reminder = input.Reminder;
        draft.AssignedTechnician = input.AssignedTechnician;
        SaveCreateJobDraft(draft);

        return RedirectToAction(nameof(CreateJobReview));
    }

    [HttpGet]
    public async Task<IActionResult> CreateJobReview(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetCreateJobDraft();
        var model = await jobWorkflow.GetCreateJobReviewAsync(proveedor.Entity!, draft);
        if (model == null)
        {
            return RedirectToAction(nameof(CreateJob));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmCreateJob(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetCreateJobDraft();
        if (string.IsNullOrWhiteSpace(draft.Title) || string.IsNullOrWhiteSpace(draft.Address))
        {
            return RedirectToAction(nameof(CreateJob));
        }

        var input = new ProviderProCreateJobInput
        {
            ServiceCategoryId = draft.ServiceCategoryId,
            ServiceCategory = draft.ServiceCategoryLabel,
            ClienteId = draft.ClienteId,
            CustomerName = draft.CustomerName,
            Address = draft.Address,
            Title = draft.Title,
            Description = draft.Description,
            VisitDate = draft.VisitDate,
            StartTimeLabel = draft.StartTimeLabel,
            EndTimeLabel = draft.EndTimeLabel,
            AssignedTechnician = draft.AssignedTechnician,
            Priority = draft.Priority,
            Notes = draft.Notes,
            Reminder = draft.Reminder,
            AddToCalendar = draft.AddToCalendar
        };

        var jobId = await jobWorkflow.CreateJobAsync(proveedor.Entity!.Id, input, cancellationToken);
        ClearCreateJobDraft();
        return RedirectToAction(nameof(CreateJobSuccess), new { id = jobId });
    }

    [HttpGet]
    public async Task<IActionResult> CreateJobSuccess(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await jobWorkflow.GetCreateJobSuccessAsync(proveedor.Entity!, id, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(Jobs));
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> JobDetails(int id, bool fromCalendar = false, string? date = null, CancellationToken cancellationToken = default)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await jobWorkflow.GetJobDetailsAsync(proveedor.Entity!, id, fromCalendar, date, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        ViewBag.FromCalendar = fromCalendar;
        ViewBag.CalendarDate = date;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartJob(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var started = await jobWorkflow.StartJobAsync(proveedor.Entity!.Id, id, cancellationToken);
        if (!started)
        {
            return RedirectToAction(nameof(JobDetails), new { id });
        }

        return RedirectToAction(nameof(ActiveJob), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> ActiveJob(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await jobWorkflow.GetActiveJobAsync(proveedor.Entity!, id, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(JobDetails), new { id });
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteJob(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var completed = await jobWorkflow.CompleteJobAsync(proveedor.Entity!.Id, id, cancellationToken);
        if (!completed)
        {
            return RedirectToAction(nameof(ActiveJob), new { id });
        }

        return RedirectToAction(nameof(JobReport), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> JobReport(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await jobWorkflow.GetJobReportAsync(proveedor.Entity!, id, cancellationToken);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }



    private async Task<(IActionResult? Result, IndorProveedor? Entity)> ResolveProveedorAsync(CancellationToken cancellationToken)

    {

        var user = await userManager.GetUserAsync(User);

        if (user == null ||

            !string.Equals(user.RolUsuario, "ProveedorServicios", StringComparison.OrdinalIgnoreCase))

        {

            return (RedirectToAction("Index", "Home"), null);

        }



        if (!await userManager.IsInRoleAsync(user, "ProveedorServicios"))

        {

            await userManager.AddToRoleAsync(user, "ProveedorServicios");

            await signInManager.RefreshSignInAsync(user);

        }



        var proveedor = await registration.GetProveedorForCurrentUserAsync(cancellationToken);

        if (proveedor == null)

        {

            return (RedirectToAction("Entry", "ProviderRegistration"), null);

        }



        if (proveedor.RegistrationStatus == ProviderRegistrationStatuses.Draft)

        {

            var action = registration.ResolveWizardResumeAction(Math.Max(1, proveedor.CurrentStep));

            return (RedirectToAction(action, "ProviderRegistration"), null);

        }



        return (null, proveedor);

    }

    private const string CreateJobDraftSessionKey = "ProviderProCreateJobDraft";

    private static readonly JsonSerializerOptions CreateJobDraftJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private ProviderProCreateJobDraft GetCreateJobDraft()
    {
        var json = HttpContext.Session.GetString(CreateJobDraftSessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ProviderProCreateJobDraft();
        }

        return JsonSerializer.Deserialize<ProviderProCreateJobDraft>(json, CreateJobDraftJsonOptions)
            ?? new ProviderProCreateJobDraft();
    }

    private void SaveCreateJobDraft(ProviderProCreateJobDraft draft) =>
        HttpContext.Session.SetString(
            CreateJobDraftSessionKey,
            JsonSerializer.Serialize(draft, CreateJobDraftJsonOptions));

    private void ClearCreateJobDraft() =>
        HttpContext.Session.Remove(CreateJobDraftSessionKey);

    private const string CreateEstimateDraftSessionKey = "ProviderProCreateEstimateDraft";

    private ProviderProCreateEstimateDraft GetCreateEstimateDraft()
    {
        var json = HttpContext.Session.GetString(CreateEstimateDraftSessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ProviderProCreateEstimateDraft();
        }

        return JsonSerializer.Deserialize<ProviderProCreateEstimateDraft>(json, CreateJobDraftJsonOptions)
            ?? new ProviderProCreateEstimateDraft();
    }

    private void SaveCreateEstimateDraft(ProviderProCreateEstimateDraft draft) =>
        HttpContext.Session.SetString(
            CreateEstimateDraftSessionKey,
            JsonSerializer.Serialize(draft, CreateJobDraftJsonOptions));

    private void ClearCreateEstimateDraft() =>
        HttpContext.Session.Remove(CreateEstimateDraftSessionKey);

    private const string AddCustomerDraftSessionKey = "ProviderProAddCustomerDraft";

    private ProviderProAddCustomerDraft GetAddCustomerDraft()
    {
        var json = HttpContext.Session.GetString(AddCustomerDraftSessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ProviderProAddCustomerDraft();
        }

        return JsonSerializer.Deserialize<ProviderProAddCustomerDraft>(json, CreateJobDraftJsonOptions)
            ?? new ProviderProAddCustomerDraft();
    }

    private void SaveAddCustomerDraft(ProviderProAddCustomerDraft draft) =>
        HttpContext.Session.SetString(
            AddCustomerDraftSessionKey,
            JsonSerializer.Serialize(draft, CreateJobDraftJsonOptions));

    private void ClearAddCustomerDraft() =>
        HttpContext.Session.Remove(AddCustomerDraftSessionKey);

    private const string UploadReportDraftSessionKey = "ProviderProUploadReportDraft";

    private ProviderProUploadReportDraft GetUploadReportDraft()
    {
        var json = HttpContext.Session.GetString(UploadReportDraftSessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ProviderProUploadReportDraft();
        }

        return JsonSerializer.Deserialize<ProviderProUploadReportDraft>(json, CreateJobDraftJsonOptions)
            ?? new ProviderProUploadReportDraft();
    }

    private void SaveUploadReportDraft(ProviderProUploadReportDraft draft) =>
        HttpContext.Session.SetString(
            UploadReportDraftSessionKey,
            JsonSerializer.Serialize(draft, CreateJobDraftJsonOptions));

    private void ClearUploadReportDraft() =>
        HttpContext.Session.Remove(UploadReportDraftSessionKey);

    private static readonly HashSet<string> AllowedReportFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png"
    };

    private const long MaxReportFileBytes = 10 * 1024 * 1024;

    private async Task ApplyReportFileToSlotAsync(
        int proveedorId,
        List<ProviderProUploadReportFileSlot> slots,
        string slotName,
        IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return;
        }

        var saved = await SaveProviderReportFileAsync(proveedorId, file);
        if (saved == null)
        {
            return;
        }

        var slot = slots.FirstOrDefault(s => s.Slot == slotName);
        if (slot == null)
        {
            return;
        }

        slot.Url = saved.Value.Url;
        slot.FileName = saved.Value.FileName;
    }

    private async Task<(string Url, string FileName)?> SaveProviderReportFileAsync(int proveedorId, IFormFile file)
    {
        if (file.Length > MaxReportFileBytes)
        {
            return null;
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) && file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            ext = ".jpg";
        }

        if (!AllowedReportFileExtensions.Contains(ext))
        {
            return null;
        }

        var folder = Path.Combine(env.WebRootPath, "uploads", "provider-reports", proveedorId.ToString());
        Directory.CreateDirectory(folder);

        var original = Path.GetFileName(file.FileName);
        var stored = $"{Guid.NewGuid():N}_{original}";
        var physical = Path.Combine(folder, stored);

        await using (var stream = System.IO.File.Create(physical))
        {
            await file.CopyToAsync(stream);
        }

        return ($"/uploads/provider-reports/{proveedorId}/{stored}", original);
    }

    private static List<ProviderProEstimateLineItemViewModel> BuildEstimateLineItemsFromInput(
        ProviderProCreateEstimatePricingInput input)
    {
        var items = new List<ProviderProEstimateLineItemViewModel>();
        for (var i = 0; i < input.LineLabels.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(input.LineLabels[i]))
            {
                continue;
            }

            var qty = i < input.LineQtys.Count ? input.LineQtys[i] : 1;
            var unitPrice = i < input.LineUnitPrices.Count ? input.LineUnitPrices[i] : 0;
            var amount = i < input.LineAmounts.Count && input.LineAmounts[i] > 0
                ? input.LineAmounts[i]
                : qty * unitPrice;

            items.Add(new ProviderProEstimateLineItemViewModel
            {
                Category = i < input.LineCategories.Count ? input.LineCategories[i] : "labor",
                Label = input.LineLabels[i],
                Description = i < input.LineDescriptions.Count ? input.LineDescriptions[i] : "",
                Qty = qty,
                Unit = i < input.LineUnits.Count ? input.LineUnits[i] : "ls",
                UnitPrice = unitPrice,
                Amount = amount,
                IsTaxable = i >= input.LineTaxable.Count
                    || !bool.TryParse(input.LineTaxable[i], out var taxable)
                    || taxable
            });
        }

        return items;
    }
}

