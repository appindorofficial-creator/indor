using System.Text.Json;

using IndorMvcApp.Helpers;

using IndorMvcApp.Models;

using IndorMvcApp.Services;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Identity;

using IndorMvcApp.ViewModels;

using IndorMvcApp.Validation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;



namespace IndorMvcApp.Controllers;



[Authorize]

public partial class ProveedorController(

    UserManager<ApplicationUser> userManager,

    SignInManager<ApplicationUser> signInManager,

    IProviderRegistrationService registration,

    ProviderProDashboardService dashboardService,

    IProviderProDataService proData,

    IProviderProJobWorkflowService jobWorkflow,

    IProviderNetworkService network,

    IContractorVerificationService verification,

    INetworkRequestsService requests,

    IWebHostEnvironment env,

    IInsuranceStripeCheckoutService insuranceStripeCheckout,

    IServiceRequestService serviceRequests,

    INotificationService notificationService,

    IIndorLocalizer localizer) : Controller

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
    public async Task<IActionResult> AddCustomer(bool? fresh, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (fresh == true)
        {
            ClearAddCustomerDraft();
        }

        var draft = fresh == true ? null : GetAddCustomerDraft();
        if (draft != null && string.IsNullOrWhiteSpace(draft.FirstName))
        {
            draft = null;
        }

        var model = proData.GetAddCustomerInfoViewModel(proveedor.Entity!, draft);
        ApplyAddCustomerNavigation();
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
            TempData["AddCustomerError"] = localizer["First name and last name are required."];
            var invalidModel = proData.GetAddCustomerInfoViewModel(proveedor.Entity!, new ProviderProAddCustomerDraft
            {
                CustomerType = string.IsNullOrWhiteSpace(input.CustomerType) ? "Homeowner" : input.CustomerType,
                FirstName = TrimOrEmpty(input.FirstName),
                LastName = TrimOrEmpty(input.LastName),
                Phone = TrimOrEmpty(input.Phone),
                Email = TrimOrEmpty(input.Email),
                PreferredContactMethod = string.IsNullOrWhiteSpace(input.PreferredContactMethod) ? "SMS" : input.PreferredContactMethod,
                CompanyName = TrimOrEmpty(input.CompanyName)
            });
            ApplyAddCustomerNavigation();
            return View(invalidModel);
        }

        var draft = GetAddCustomerDraft();
        draft.CustomerType = string.IsNullOrWhiteSpace(input.CustomerType) ? "Homeowner" : input.CustomerType;
        draft.FirstName = input.FirstName.Trim();
        draft.LastName = input.LastName.Trim();
        draft.Phone = TrimOrEmpty(input.Phone);
        draft.Email = TrimOrEmpty(input.Email);
        draft.PreferredContactMethod = string.IsNullOrWhiteSpace(input.PreferredContactMethod) ? "SMS" : input.PreferredContactMethod;
        draft.CompanyName = TrimOrEmpty(input.CompanyName);
        SaveAddCustomerDraft(draft);
        await HttpContext.Session.CommitAsync(cancellationToken);

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
        ApplyAddCustomerNavigation();
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
        draft.StreetAddress = TrimOrEmpty(input.StreetAddress);
        draft.AptUnit = TrimOrEmpty(input.AptUnit);
        draft.City = TrimOrEmpty(input.City);
        draft.State = input.State ?? "";
        draft.ZipCode = TrimOrEmpty(input.ZipCode);
        draft.PropertyType = string.IsNullOrWhiteSpace(input.PropertyType) ? "Single Family" : input.PropertyType;
        draft.Bedrooms = input.Bedrooms;
        draft.Bathrooms = input.Bathrooms;
        draft.IsBillingAddressSame = input.IsBillingAddressSame;
        draft.AccessNotes = TrimOrEmpty(input.AccessNotes);
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
        ApplyAddCustomerNavigation();
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
        draft.InternalNotes = TrimOrEmpty(input.InternalNotes);
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

        ApplyAddCustomerNavigation();
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
    public async Task<IActionResult> ReportTemplates(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var p = proveedor.Entity!;
        var companyName = !string.IsNullOrWhiteSpace(p.DbaName) ? p.DbaName
            : !string.IsNullOrWhiteSpace(p.BusinessName) ? p.BusinessName
            : !string.IsNullOrWhiteSpace(p.PrimaryContact) ? p.PrimaryContact
            : "Your company";

        return View(model: companyName);
    }

    // ---------------------------------------------------------------
    // Insurance — provider coverage acquisition
    // ---------------------------------------------------------------

    /// <summary>INDOR Pro membership plan picker (Basic / Standard / Premium).</summary>
    [HttpGet]
    public async Task<IActionResult> InsurancePlans(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        ViewBag.NavActive = "home";
        return View();
    }

    // Step 1 — Coverage (plan summary)
    [HttpGet]
    public async Task<IActionResult> InsuranceQuote(string? plan, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var p = proveedor.Entity!;
        var draft = GetInsuranceDraft();

        if (!string.IsNullOrWhiteSpace(plan))
        {
            draft.Plan = NormalizePlan(plan);
        }
        else if (string.IsNullOrWhiteSpace(draft.Plan))
        {
            draft.Plan = "Basic";
        }

        draft.Coverages = ["General Liability"];

        SaveInsuranceDraft(draft);
        await HttpContext.Session.CommitAsync(cancellationToken);

        return View(new InsuranceCoverageStepViewModel
        {
            CompanyInitial = CompanyInitial(p),
            Plan = draft.Plan
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InsuranceQuote(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetInsuranceDraft();
        if (string.IsNullOrWhiteSpace(draft.Plan)) draft.Plan = "Basic";
        draft.Coverages = ["General Liability"];
        SaveInsuranceDraft(draft);
        await HttpContext.Session.CommitAsync(cancellationToken);
        return RedirectToAction(nameof(InsuranceBusinessInfo));
    }

    // Step 2 — Business & Owner info
    [HttpGet]
    public async Task<IActionResult> InsuranceBusinessInfo(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var p = proveedor.Entity!;
        var draft = GetInsuranceDraft();

        // Prefill from the provider profile the first time.
        if (string.IsNullOrWhiteSpace(draft.BusinessName)) draft.BusinessName = ResolveCompanyName(p);
        if (string.IsNullOrWhiteSpace(draft.StreetAddress)) draft.StreetAddress = p.BusinessAddress ?? string.Empty;
        if (string.IsNullOrWhiteSpace(draft.OwnerName)) draft.OwnerName = p.PrimaryContact ?? string.Empty;
        if (string.IsNullOrWhiteSpace(draft.OwnerPhone)) draft.OwnerPhone = p.Phone ?? string.Empty;
        if (string.IsNullOrWhiteSpace(draft.OwnerEmail)) draft.OwnerEmail = p.Email ?? string.Empty;
        SaveInsuranceDraft(draft);
        await HttpContext.Session.CommitAsync(cancellationToken);

        return View(new InsuranceBusinessInfoStepViewModel
        {
            CompanyInitial = CompanyInitial(p),
            Plan = draft.Plan,
            Trade = draft.Trade,
            BusinessName = draft.BusinessName,
            StreetAddress = draft.StreetAddress,
            City = draft.City,
            State = string.IsNullOrWhiteSpace(draft.State) ? "NC" : draft.State,
            ZipCode = draft.ZipCode,
            OwnerName = draft.OwnerName,
            OwnerDateOfBirth = draft.OwnerDateOfBirth,
            OwnerPhone = draft.OwnerPhone,
            OwnerEmail = draft.OwnerEmail
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InsuranceBusinessInfo(InsuranceBusinessInfoStepViewModel model, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetInsuranceDraft();
        draft.Trade = TrimOrEmpty(model.Trade);
        draft.BusinessName = TrimOrEmpty(model.BusinessName);
        draft.StreetAddress = TrimOrEmpty(model.StreetAddress);
        draft.City = TrimOrEmpty(model.City);
        draft.State = string.IsNullOrWhiteSpace(model.State) ? "NC" : TrimOrEmpty(model.State);
        draft.ZipCode = TrimOrEmpty(model.ZipCode);
        draft.OwnerName = TrimOrEmpty(model.OwnerName);
        draft.OwnerDateOfBirth = TrimOrEmpty(model.OwnerDateOfBirth);
        draft.OwnerPhone = TrimOrEmpty(model.OwnerPhone);
        draft.OwnerEmail = TrimOrEmpty(model.OwnerEmail);

        if (string.IsNullOrWhiteSpace(draft.Trade))
            ModelState.AddModelError(nameof(model.Trade), "Select your trade or industry.");
        if (string.IsNullOrWhiteSpace(draft.BusinessName))
            ModelState.AddModelError(nameof(model.BusinessName), "Business name is required.");
        if (string.IsNullOrWhiteSpace(draft.OwnerName))
            ModelState.AddModelError(nameof(model.OwnerName), "Owner name is required.");
        if (string.IsNullOrWhiteSpace(draft.OwnerEmail))
            ModelState.AddModelError(nameof(model.OwnerEmail), "Owner email is required.");

        if (!ModelState.IsValid)
        {
            model.CompanyInitial = CompanyInitial(proveedor.Entity!);
            model.Plan = draft.Plan;
            return View(model);
        }

        SaveInsuranceDraft(draft);
        await HttpContext.Session.CommitAsync(cancellationToken);
        return RedirectToAction(nameof(InsuranceBusinessDetails));
    }

    // Step 3 — Business details + payment setup
    [HttpGet]
    public async Task<IActionResult> InsuranceBusinessDetails(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetInsuranceDraft();
        return View(new InsuranceBusinessDetailsStepViewModel
        {
            CompanyInitial = CompanyInitial(proveedor.Entity!),
            Plan = draft.Plan,
            NumberOfEmployees = draft.NumberOfEmployees,
            EmployeePayroll = draft.EmployeePayroll,
            CompanyGrossRevenue = draft.CompanyGrossRevenue,
            YearsInBusiness = draft.YearsInBusiness,
            WorksAtCustomerHomes = draft.WorksAtCustomerHomes,
            UsesSubcontractors = draft.UsesSubcontractors,
            NeedsCOI = draft.NeedsCOI,
            AutoPayMonthly = draft.AutoPayMonthly,
            CardLast4 = draft.CardLast4,
            FirstBillingDate = DateTime.Today.AddDays(30)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InsuranceBusinessDetails(InsuranceBusinessDetailsStepViewModel model, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetInsuranceDraft();
        draft.NumberOfEmployees = TrimOrEmpty(model.NumberOfEmployees);
        draft.EmployeePayroll = model.EmployeePayroll;
        draft.CompanyGrossRevenue = model.CompanyGrossRevenue;
        draft.YearsInBusiness = TrimOrEmpty(model.YearsInBusiness);
        draft.WorksAtCustomerHomes = model.WorksAtCustomerHomes;
        draft.UsesSubcontractors = model.UsesSubcontractors;
        draft.NeedsCOI = model.NeedsCOI;
        draft.AutoPayMonthly = model.AutoPayMonthly;

        SaveInsuranceDraft(draft);
        await HttpContext.Session.CommitAsync(cancellationToken);
        return RedirectToAction(nameof(InsuranceReview));
    }

    // Step 4 — Review & payment
    [HttpGet]
    public async Task<IActionResult> InsuranceReview(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetInsuranceDraft();
        if (string.IsNullOrWhiteSpace(draft.BusinessName))
        {
            return RedirectToAction(nameof(InsuranceBusinessInfo));
        }

        return View(new InsuranceReviewViewModel
        {
            CompanyInitial = CompanyInitial(proveedor.Entity!),
            Plan = draft.Plan,
            Draft = draft,
            PaymentMethod = string.IsNullOrWhiteSpace(draft.PaymentMethod) ? "Card" : draft.PaymentMethod
        });
    }

    // Step 4 → Stripe Checkout for pay-today amount → Step 5 after payment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InsurancePay(string? paymentMethod, bool authorize, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetInsuranceDraft();
        if (string.IsNullOrWhiteSpace(draft.BusinessName))
        {
            return RedirectToAction(nameof(InsuranceBusinessInfo));
        }

        if (!authorize)
        {
            TempData["InsuranceConfirmError"] = localizer["Please confirm and authorize to continue."];
            return RedirectToAction(nameof(InsuranceReview));
        }

        if (!insuranceStripeCheckout.IsConfigured)
        {
            TempData["InsuranceConfirmError"] = localizer["Payment is temporarily unavailable. Please try again later."];
            return RedirectToAction(nameof(InsuranceReview));
        }

        draft.PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "Card" : paymentMethod;
        SaveInsuranceDraft(draft);
        await HttpContext.Session.CommitAsync(cancellationToken);

        var quote = await proData.SavePendingInsuranceQuoteAsync(proveedor.Entity!.Id, draft, cancellationToken);

        var successUrl = Url.Action(
            nameof(InsurancePaymentSuccess),
            "Proveedor",
            new { session_id = "{CHECKOUT_SESSION_ID}" },
            Request.Scheme)!;
        // Stripe replaces the literal {CHECKOUT_SESSION_ID} token in the success URL.
        successUrl = successUrl.Replace("%7BCHECKOUT_SESSION_ID%7D", "{CHECKOUT_SESSION_ID}", StringComparison.Ordinal);

        var cancelUrl = Url.Action(nameof(InsurancePaymentCancel), "Proveedor", null, Request.Scheme)!;

        try
        {
            var checkout = await insuranceStripeCheckout.CreateCheckoutSessionAsync(
                quote,
                successUrl,
                cancelUrl,
                draft.PaymentMethod,
                cancellationToken);
            return Redirect(checkout.Url);
        }
        catch (Exception)
        {
            TempData["InsuranceConfirmError"] = localizer["We could not start Stripe Checkout. Please try again."];
            return RedirectToAction(nameof(InsuranceReview));
        }
    }

    [HttpGet]
    public async Task<IActionResult> InsurancePaymentSuccess(string? session_id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (string.IsNullOrWhiteSpace(session_id))
        {
            TempData["InsuranceConfirmError"] = localizer["Payment could not be verified. Please try again."];
            return RedirectToAction(nameof(InsuranceReview));
        }

        var completion = await insuranceStripeCheckout.CompleteCheckoutSessionAsync(session_id, cancellationToken);
        if (completion is null)
        {
            TempData["InsuranceConfirmError"] = localizer["Payment could not be verified. Please try again."];
            return RedirectToAction(nameof(InsuranceReview));
        }

        ClearInsuranceDraft();
        await HttpContext.Session.CommitAsync(cancellationToken);

        var ci = System.Globalization.CultureInfo.InvariantCulture;
        TempData["InsuranceQuotePlan"] = completion.Plan;
        TempData["InsurancePaidToday"] = completion.PaidToday.ToString(ci);
        TempData["InsuranceMonthly"] = completion.Monthly.ToString(ci);
        TempData["InsuranceReceipt"] = completion.ReceiptNumber;
        TempData["InsuranceCarrierEmailStatus"] = completion.CarrierEmailStatus;
        return RedirectToAction(nameof(InsuranceQuoteSubmitted));
    }

    [HttpGet]
    public async Task<IActionResult> InsurancePaymentCancel(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        TempData["InsuranceConfirmError"] = localizer["Payment was cancelled. You can try again when ready."];
        return RedirectToAction(nameof(InsuranceReview));
    }

    // Step 5 — Success
    [HttpGet]
    public async Task<IActionResult> InsuranceQuoteSubmitted(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var plan = TempData["InsuranceQuotePlan"] as string ?? "Basic";
        var (payToday, monthly) = InsuranceCatalog.Pricing(plan);
        decimal.TryParse(TempData["InsurancePaidToday"] as string, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var paid);
        decimal.TryParse(TempData["InsuranceMonthly"] as string, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var mon);

        return View(new InsuranceSubmittedViewModel
        {
            CompanyInitial = CompanyInitial(proveedor.Entity!),
            Plan = plan,
            PaidToday = paid > 0 ? paid : payToday,
            MonthlyPlan = mon > 0 ? mon : monthly,
            Status = "Pending Carrier Approval",
            ReceiptNumber = TempData["InsuranceReceipt"] as string ?? string.Empty
        });
    }

    private static string NormalizePlan(string? plan) => plan?.Trim().ToLowerInvariant() switch
    {
        "standard" => "Standard",
        "premium" => "Premium",
        _ => "Basic"
    };

    private static string ResolveCompanyName(IndorProveedor p) =>
        !string.IsNullOrWhiteSpace(p.DbaName) ? p.DbaName!
        : !string.IsNullOrWhiteSpace(p.BusinessName) ? p.BusinessName!
        : !string.IsNullOrWhiteSpace(p.PrimaryContact) ? p.PrimaryContact!
        : "Your company";

    private static string CompanyInitial(IndorProveedor p)
    {
        var name = ResolveCompanyName(p);
        return string.IsNullOrWhiteSpace(name) ? "P" : name.Trim()[..1].ToUpperInvariant();
    }

    // ---------------------------------------------------------------
    // Templates flow (list → details → ready) — DB-backed
    // ---------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Templates(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetReportTemplatesAsync(proveedor.Entity!.Id, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> TemplateDetails(string key, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var template = await proData.GetReportTemplateAsync(proveedor.Entity!.Id, key, cancellationToken);
        if (template == null)
        {
            return RedirectToAction(nameof(Templates));
        }

        return View(template);
    }

    [HttpGet]
    public async Task<IActionResult> TemplateReady(string key, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var template = await proData.GetReportTemplateAsync(proveedor.Entity!.Id, key, cancellationToken);
        if (template == null)
        {
            return RedirectToAction(nameof(Templates));
        }

        return View(template);
    }

    // ---------------------------------------------------------------
    // Export Report flow (select job → details/photos → review)
    // Session-draft wizard that persists to IndorProveedorReports.
    // ---------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> ExportReport(string? q, bool? fresh, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (fresh == true)
        {
            ClearExportReportDraft();
        }

        SetExportAvatar(proveedor.Entity);

        var draft = GetExportReportDraft();
        draft.ReportName ??= "";
        draft.PreparedBy ??= proveedor.Entity!.PrimaryContact ?? "";
        draft.ReportDate ??= DateTime.Now.ToString("yyyy-MM-dd");
        ViewBag.ExportDraft = draft;

        var model = await proData.GetUploadPhotosSelectJobAsync(proveedor.Entity!, q, null, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportReport(int jobId, string? reportName, string? reportDate, string? preparedBy, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (jobId <= 0)
        {
            return RedirectToAction(nameof(ExportReport));
        }

        var draft = GetExportReportDraft();
        draft.JobId = jobId;
        draft.ReportName = TrimOrEmpty(reportName);
        draft.ReportDate = TrimOrEmpty(reportDate);
        draft.PreparedBy = TrimOrEmpty(preparedBy);
        SaveExportReportDraft(draft);
        await HttpContext.Session.CommitAsync(cancellationToken);

        return RedirectToAction(nameof(ExportReportDetails));
    }

    [HttpGet]
    public async Task<IActionResult> ExportReportDetails(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetExportReportDraft();
        if (!draft.JobId.HasValue)
        {
            return RedirectToAction(nameof(ExportReport));
        }

        var summary = await proData.GetExportJobSummaryAsync(proveedor.Entity!, draft.JobId.Value, cancellationToken);
        if (summary == null)
        {
            return RedirectToAction(nameof(ExportReport));
        }

        SetExportAvatar(proveedor.Entity);
        return View(new ProviderProExportReportViewModel { Job = summary, Draft = draft });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExportPhotos(string? category, List<IFormFile>? photos, string? action,
        string? reportCategory, string? location, string? priority, string? weather, string? description,
        CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetExportReportDraft();
        if (!draft.JobId.HasValue)
        {
            return RedirectToAction(nameof(ExportReport));
        }

        draft.Category = TrimOrEmpty(reportCategory);
        draft.Location = TrimOrEmpty(location);
        draft.Priority = TrimOrEmpty(priority);
        draft.Weather = TrimOrEmpty(weather);
        draft.Description = TrimOrEmpty(description);

        var slot = string.IsNullOrWhiteSpace(category) ? "Damage" : category.Trim();
        if (photos != null)
        {
            foreach (var file in photos.Where(f => f.Length > 0))
            {
                var saved = await SaveProviderReportFileAsync(proveedor.Entity!.Id, file);
                if (saved != null)
                {
                    draft.Photos.Add(new ProviderProUploadReportFileSlot
                    {
                        Slot = slot,
                        Url = saved.Value.Url,
                        FileName = saved.Value.FileName
                    });
                }
            }
        }

        SaveExportReportDraft(draft);
        await HttpContext.Session.CommitAsync(cancellationToken);

        if (string.Equals(action, "continue", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(ExportReportReview));
        }

        return RedirectToAction(nameof(ExportReportDetails));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveExportPhoto(int index, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetExportReportDraft();
        if (index >= 0 && index < draft.Photos.Count)
        {
            draft.Photos.RemoveAt(index);
            SaveExportReportDraft(draft);
            await HttpContext.Session.CommitAsync(cancellationToken);
        }

        return RedirectToAction(nameof(ExportReportDetails));
    }

    [HttpGet]
    public async Task<IActionResult> ExportReportReview(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetExportReportDraft();
        if (!draft.JobId.HasValue)
        {
            return RedirectToAction(nameof(ExportReport));
        }

        var summary = await proData.GetExportJobSummaryAsync(proveedor.Entity!, draft.JobId.Value, cancellationToken);
        if (summary == null)
        {
            return RedirectToAction(nameof(ExportReport));
        }

        SetExportAvatar(proveedor.Entity);
        return View(new ProviderProExportReportViewModel { Job = summary, Draft = draft });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportReportComplete(string? mode, string? notes, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetExportReportDraft();
        if (!draft.JobId.HasValue)
        {
            return RedirectToAction(nameof(ExportReport));
        }

        draft.Notes = TrimOrEmpty(notes);
        SaveExportReportDraft(draft);

        var send = string.Equals(mode, "send", StringComparison.OrdinalIgnoreCase);
        var reportId = await proData.SaveExportReportFromDraftAsync(proveedor.Entity!.Id, draft, send, cancellationToken);
        if (reportId == null)
        {
            return RedirectToAction(nameof(ExportReportReview));
        }

        ClearExportReportDraft();
        await HttpContext.Session.CommitAsync(cancellationToken);

        TempData["ProviderProToast"] = send ? localizer["Export report sent."] : localizer["Export report saved."];
        return RedirectToAction(nameof(Reports));
    }

    private void SetExportAvatar(IndorProveedor? proveedor)
    {
        var source = !string.IsNullOrWhiteSpace(proveedor?.DbaName) ? proveedor!.DbaName
            : !string.IsNullOrWhiteSpace(proveedor?.BusinessName) ? proveedor!.BusinessName
            : !string.IsNullOrWhiteSpace(proveedor?.PrimaryContact) ? proveedor!.PrimaryContact
            : "Provider";
        ViewBag.ExportAvatarInitial = source!.Trim()[0].ToString().ToUpperInvariant();
    }

    [HttpGet]
    public async Task<IActionResult> UploadReport(string? q, string? filter, bool? fresh, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (fresh == true || (string.IsNullOrWhiteSpace(q) && string.IsNullOrWhiteSpace(filter)))
        {
            ClearUploadReportDraft();
        }

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
        await HttpContext.Session.CommitAsync(cancellationToken);
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
    public async Task<IActionResult> UploadReportDetails(ProviderProUploadReportDetailsInput? input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        input ??= new ProviderProUploadReportDetailsInput();
        var draft = GetUploadReportDraft();
        if (!draft.JobId.HasValue)
        {
            TempData["UploadReportError"] = localizer["Select a job before submitting the report."].ToString();
            return RedirectToAction(nameof(UploadReport));
        }

        draft.Title = TrimOrEmpty(input.Title);
        draft.Summary = TrimOrEmpty(input.Summary);
        draft.WorkCompleted = TrimOrEmpty(input.WorkCompleted);
        draft.MaterialsUsed = TrimOrEmpty(input.MaterialsUsed);
        draft.WarrantyInfo = TrimOrEmpty(input.WarrantyInfo);
        draft.Recommendations = TrimOrEmpty(input.Recommendations);
        draft.InternalNotes = TrimOrEmpty(input.InternalNotes);
        draft.SendToHomeowner = input.SendToHomeowner;
        draft.RequestApproval = input.RequestApproval;
        draft.CreateHouseFactsRecord = input.CreateHouseFactsRecord;
        SaveUploadReportDraft(draft);

        if (string.IsNullOrWhiteSpace(draft.Title)
            || string.IsNullOrWhiteSpace(draft.Summary)
            || string.IsNullOrWhiteSpace(draft.WorkCompleted))
        {
            TempData["UploadReportError"] = localizer["Please complete the required report fields before submitting."].ToString();
            return RedirectToAction(nameof(UploadReportDetails));
        }

        var reportId = await proData.SaveUploadReportFromDraftAsync(proveedor.Entity!.Id, draft, cancellationToken);
        if (!reportId.HasValue)
        {
            TempData["UploadReportError"] = localizer["We couldn't save your report. Please select the job again and try once more."].ToString();
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

    // ---------------------------------------------------------------
    // Upload Photos flow (select job → add photos → review → save)
    // ---------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> UploadPhotos(string? q, string? filter, bool? fresh, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (fresh == true || (string.IsNullOrWhiteSpace(q) && string.IsNullOrWhiteSpace(filter)))
        {
            ClearUploadPhotosDraft();
        }

        var model = await proData.GetUploadPhotosSelectJobAsync(proveedor.Entity!, q, filter, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPhotos(int jobId, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (jobId <= 0)
        {
            return RedirectToAction(nameof(UploadPhotos));
        }

        var draft = GetUploadPhotosDraft();
        draft.JobId = jobId;
        SaveUploadPhotosDraft(draft);
        await HttpContext.Session.CommitAsync(cancellationToken);
        return RedirectToAction(nameof(AddPhotos));
    }

    [HttpGet]
    public async Task<IActionResult> AddPhotos(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetUploadPhotosDraft();
        if (!draft.JobId.HasValue)
        {
            return RedirectToAction(nameof(UploadPhotos));
        }

        var model = await proData.GetUploadPhotosAddAsync(proveedor.Entity!, draft, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(UploadPhotos));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPhotos(string? category, List<IFormFile>? photos, string? action, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetUploadPhotosDraft();
        if (!draft.JobId.HasValue)
        {
            return RedirectToAction(nameof(UploadPhotos));
        }

        var slot = string.IsNullOrWhiteSpace(category) ? "After" : category.Trim();

        if (photos != null)
        {
            foreach (var file in photos.Where(f => f.Length > 0))
            {
                var saved = await SaveProviderReportFileAsync(proveedor.Entity!.Id, file);
                if (saved != null)
                {
                    draft.Photos.Add(new ProviderProUploadReportFileSlot
                    {
                        Slot = slot,
                        Url = saved.Value.Url,
                        FileName = saved.Value.FileName
                    });
                }
            }
        }

        SaveUploadPhotosDraft(draft);
        await HttpContext.Session.CommitAsync(cancellationToken);

        if (string.Equals(action, "continue", StringComparison.OrdinalIgnoreCase) && draft.Photos.Count > 0)
        {
            return RedirectToAction(nameof(ReviewPhotos));
        }

        return RedirectToAction(nameof(AddPhotos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePhoto(int index, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetUploadPhotosDraft();
        if (index >= 0 && index < draft.Photos.Count)
        {
            draft.Photos.RemoveAt(index);
            SaveUploadPhotosDraft(draft);
            await HttpContext.Session.CommitAsync(cancellationToken);
        }

        return RedirectToAction(nameof(AddPhotos));
    }

    [HttpGet]
    public async Task<IActionResult> ReviewPhotos(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetUploadPhotosDraft();
        if (!draft.JobId.HasValue || draft.Photos.Count == 0)
        {
            return RedirectToAction(nameof(AddPhotos));
        }

        var model = await proData.GetUploadPhotosReviewAsync(proveedor.Entity!, draft, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(UploadPhotos));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewPhotos(string? notes, string? action, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetUploadPhotosDraft();
        draft.Notes = TrimOrEmpty(notes);
        SaveUploadPhotosDraft(draft);

        if (!draft.JobId.HasValue || draft.Photos.Count == 0)
        {
            return RedirectToAction(nameof(AddPhotos));
        }

        var reportId = await proData.SaveUploadPhotosFromDraftAsync(proveedor.Entity!.Id, draft, cancellationToken);
        if (!reportId.HasValue)
        {
            return RedirectToAction(nameof(ReviewPhotos));
        }

        ClearUploadPhotosDraft();

        if (string.Equals(action, "send", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Reports), new { tab = "ready" });
        }

        return RedirectToAction(nameof(UploadReportSuccess), new { id = reportId.Value });
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile? photo, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return IsAjaxPhotoUploadRequest() ? Unauthorized() : proveedor.Result;
        }

        if (photo == null || photo.Length == 0)
        {
            return PhotoUploadResult(localizer["Please choose a photo to upload."].ToString());
        }

        var (photoError, photoUrl) = await SaveProfilePhotoAsync(proveedor.Entity!.Id, photo);
        if (!string.IsNullOrWhiteSpace(photoError))
        {
            return PhotoUploadResult(photoError);
        }

        return PhotoUploadResult(null, photoUrl);
    }

    [HttpGet]
    public async Task<IActionResult> PublicProfile(CancellationToken cancellationToken)
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
    public async Task<IActionResult> Settings(CancellationToken cancellationToken)
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
    public async Task<IActionResult> HelpCenter(CancellationToken cancellationToken)
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
    public async Task<IActionResult> Subscription(CancellationToken cancellationToken)
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
    public async Task<IActionResult> Notifications(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetNotificationsPageAsync(proveedor.Entity!, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNotificationsViewed(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return Unauthorized();
        }

        proData.MarkNotificationsViewed(proveedor.Entity!.Id);
        return Ok(new { hasNotifications = false });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Notifications(ProviderProNotificationsInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        await proData.SaveNotificationPreferencesAsync(proveedor.Entity!.Id, input, cancellationToken);
        TempData["NotificationsSaved"] = true;
        return RedirectToAction(nameof(Notifications));
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile(string? section, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetEditProfileAsync(proveedor.Entity!, cancellationToken: cancellationToken);
        var normalizedSection = string.IsNullOrWhiteSpace(section) ? "full" : section.Trim().ToLowerInvariant();
        ViewBag.ActiveSection = normalizedSection;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> EditProfile(
        [FromForm] ProviderProEditProfileInput input,
        IFormFile? profilePhoto,
        [FromForm] string? activeSection,
        CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var section = string.IsNullOrWhiteSpace(activeSection) ? "full" : activeSection.Trim().ToLowerInvariant();
        ViewBag.ActiveSection = section;

        if (section != "areas" && string.IsNullOrWhiteSpace(input.BusinessName) && string.IsNullOrWhiteSpace(input.DbaName))
        {
            ModelState.AddModelError(nameof(ProviderProEditProfileInput.BusinessName), "Business name or DBA is required.");
        }

        if (!UsPhoneOptionalAttribute.IsValidOptional(input.Phone))
        {
            ModelState.AddModelError(nameof(ProviderProEditProfileInput.Phone),
                "Enter a valid 10-digit US phone number (e.g. 555 123 4567).");
        }

        if (!ModelState.IsValid)
        {
            var invalidModel = await proData.GetEditProfileAsync(proveedor.Entity!, input, cancellationToken);
            return View(invalidModel);
        }

        var saved = await proData.SaveEditProfileAsync(proveedor.Entity!.Id, input, cancellationToken);
        if (!saved)
        {
            ModelState.AddModelError(string.Empty, "We couldn't save your profile. Please try again in a moment.");
            var failedModel = await proData.GetEditProfileAsync(proveedor.Entity!, input, cancellationToken);
            return View(failedModel);
        }

        if (profilePhoto != null && profilePhoto.Length > 0)
        {
            var (logoError, _) = await SaveProfilePhotoAsync(proveedor.Entity.Id, profilePhoto);
            if (!string.IsNullOrWhiteSpace(logoError))
            {
                TempData["ProfilePhotoError"] = logoError;
            }
        }

        TempData["ProfileSaved"] = true;
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public async Task<IActionResult> EditProfileServices(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetEditProfileServicesAsync(proveedor.Entity!, cancellationToken);
        ViewBag.CompanyInitial = model.CompanyInitial;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfileServices(string[]? serviceIds, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (serviceIds == null || serviceIds.Length == 0)
        {
            TempData["ProfileSectionError"] = localizer["Select at least one service or category."];
            return RedirectToAction(nameof(EditProfileServices));
        }

        var saved = await proData.SaveEditProfileServicesAsync(
            proveedor.Entity!.Id,
            serviceIds ?? [],
            cancellationToken);

        if (!saved)
        {
            TempData["ProfileSectionError"] = localizer["We couldn't save your services. Please try again."];
            return RedirectToAction(nameof(EditProfileServices));
        }

        TempData["ProfileSaved"] = true;
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public async Task<IActionResult> EditProfileVerification(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetEditProfileVerificationAsync(proveedor.Entity!, cancellationToken);
        ViewBag.CompanyInitial = model.CompanyInitial;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> EditProfileVerification(string documentType, IFormFile? documentFile, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (documentFile == null || documentFile.Length == 0 || string.IsNullOrWhiteSpace(documentType))
        {
            TempData["ProfileSectionError"] = localizer["Choose a file to upload."];
            return RedirectToAction(nameof(EditProfileVerification));
        }

        var uploadError = await SaveProviderDocumentAsync(proveedor.Entity!.Id, documentType.Trim(), documentFile);
        if (!string.IsNullOrWhiteSpace(uploadError))
        {
            TempData["ProfileSectionError"] = uploadError;
            return RedirectToAction(nameof(EditProfileVerification));
        }

        TempData["ProfileSaved"] = true;
        return RedirectToAction(nameof(EditProfileVerification));
    }

    [HttpGet]
    public async Task<IActionResult> EditProfilePayments(CancellationToken cancellationToken)
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

    [HttpGet]
    public async Task<IActionResult> InspectionFindings(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null) return proveedor.Result;

        var model = await proData.GetInspectionFindingsAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InspectionFindings(int id, int[] selectedFindingIndices, string? action, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null) return proveedor.Result;

        await proData.SaveLeadFindingSelectionAsync(id, selectedFindingIndices ?? []);
        if (string.Equals(action, "estimate", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(SelectRepairItems), new { id });
        }

        return RedirectToAction(nameof(InspectionFindings), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> SelectRepairItems(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null) return proveedor.Result;

        var model = await proData.GetSelectRepairItemsAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectRepairItems(int id)
    {
        return RedirectToAction(nameof(QuickEstimate), new { id });
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

    [HttpGet]
    public async Task<IActionResult> SendEstimate(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await proData.GetSendEstimatePageAsync(proveedor.Entity!, id, cancellationToken);
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

        var model = await proData.GetSendEstimatePageAsync(proveedor.Entity!, input.EstimateId, cancellationToken);
        var sent = await proData.SendEstimateAsync(proveedor.Entity!.Id, input, cancellationToken);
        if (!sent || model == null)
        {
            return RedirectToAction(nameof(SendEstimate), new { id = input.EstimateId });
        }

        if (input.SaveAsDraft)
        {
            TempData["EstimateDraftSaved"] = true;
            return RedirectToAction(nameof(SendEstimate), new { id = input.EstimateId });
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

        if (input.GoToSend)
        {
            return RedirectToAction(nameof(SendEstimate), new { id = estimateId.Value });
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
    public async Task<IActionResult> EstimateAccepted(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null) return proveedor.Result;

        var model = await proData.GetEstimateAcceptedAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveEstimate(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null) return proveedor.Result;

        await proData.ApproveEstimateAsync(proveedor.Entity!.Id, id, cancellationToken);
        return RedirectToAction(nameof(EstimateAccepted), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> CreateInvoice(int estimateId, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null) return proveedor.Result;

        var model = await proData.GetCreateInvoiceAsync(proveedor.Entity!, estimateId, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInvoice(ProviderProCreateInvoiceInput input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null) return proveedor.Result;

        var invoiceId = await proData.SaveCreateInvoiceAsync(proveedor.Entity!.Id, input, cancellationToken);
        if (!invoiceId.HasValue)
        {
            return RedirectToAction(nameof(CreateInvoice), new { estimateId = input.EstimateId });
        }

        return input.GoToReview
            ? RedirectToAction(nameof(ReviewInvoice), new { id = invoiceId.Value })
            : RedirectToAction(nameof(CreateInvoice), new { estimateId = input.EstimateId });
    }

    [HttpGet]
    public async Task<IActionResult> ReviewInvoice(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null) return proveedor.Result;

        var model = await proData.GetReviewInvoiceAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendInvoice(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null) return proveedor.Result;

        await proData.SendInvoiceAsync(proveedor.Entity!.Id, id, cancellationToken);
        return RedirectToAction(nameof(InvoiceSent), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> InvoiceSent(int id, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null) return proveedor.Result;

        var model = await proData.GetInvoiceSentAsync(proveedor.Entity!, id, cancellationToken);
        return model == null ? NotFound() : View(model);
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
    public async Task<IActionResult> DaySchedule(string? date, string? from, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var model = await jobWorkflow.GetDayScheduleAsync(proveedor.Entity!, date, cancellationToken);
        ViewBag.FromHome = string.Equals(from, "home", StringComparison.OrdinalIgnoreCase);
        ViewBag.FromJobs = string.Equals(from, "jobs", StringComparison.OrdinalIgnoreCase);
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
    public async Task<IActionResult> CreateJob(string? from, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        ClearCreateJobDraft();
        SaveCreateJobDraft(new ProviderProCreateJobDraft
        {
            NavOrigin = NormalizeCreateJobNavOrigin(from)
        });
        var model = await jobWorkflow.GetCreateJobCategoriesAsync(proveedor.Entity!, cancellationToken);
        ApplyCreateJobNavigation();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJob(ProviderProCreateJobStep1Input input, CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        if (string.IsNullOrWhiteSpace(input.ServiceCategoryId) || string.IsNullOrWhiteSpace(input.Title))
        {
            var model = await jobWorkflow.GetCreateJobCategoriesAsync(proveedor.Entity!, cancellationToken);
            model.SelectedCategoryId = input.ServiceCategoryId;
            model.JobTitle = input.Title;
            ApplyCreateJobNavigation();
            return View(model);
        }

        var detailsModel = await jobWorkflow.GetCreateJobDetailsAsync(
            proveedor.Entity!, input.ServiceCategoryId.Trim(), null, cancellationToken);
        if (detailsModel == null)
        {
            var model = await jobWorkflow.GetCreateJobCategoriesAsync(proveedor.Entity!, cancellationToken);
            model.SelectedCategoryId = input.ServiceCategoryId;
            model.JobTitle = input.Title;
            ApplyCreateJobNavigation();
            return View(model);
        }

        var draft = GetCreateJobDraft();
        draft.ServiceCategoryId = input.ServiceCategoryId.Trim();
        draft.ServiceCategoryLabel = detailsModel.ServiceCategoryLabel;
        draft.Title = UiDisplayLocalization.ToCatalogKey(input.Title.Trim());
        SaveCreateJobDraft(draft);

        return RedirectToAction(nameof(CreateJobDetails));
    }

    [HttpGet]
    public async Task<IActionResult> CreateJobDetails(string? categoryId, CancellationToken cancellationToken)
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

        if (string.IsNullOrWhiteSpace(draft.ServiceCategoryId) || string.IsNullOrWhiteSpace(draft.Title))
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
        ApplyCreateJobNavigation(draft);
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

        var draft = GetCreateJobDraft();
        if (string.IsNullOrWhiteSpace(input.Address)
            || (input.ClienteId == null && string.IsNullOrWhiteSpace(input.CustomerName)))
        {
            return RedirectToAction(nameof(CreateJobDetails));
        }

        draft.ServiceCategoryId = input.ServiceCategoryId;
        draft.Title = string.IsNullOrWhiteSpace(input.Title)
            ? draft.Title
            : UiDisplayLocalization.ToCatalogKey(input.Title.Trim());
        draft.ClienteId = input.ClienteId;
        draft.CustomerName = input.CustomerName.Trim();
        draft.Address = input.Address.Trim();
        draft.Description = string.IsNullOrWhiteSpace(input.Description)
            ? $"{draft.ServiceCategoryLabel} for {draft.CustomerName}"
            : input.Description.Trim();
        draft.Priority = string.IsNullOrWhiteSpace(input.Priority) ? "Medium" : input.Priority;
        draft.Notes = input.Notes.Trim();
        SaveCreateJobDraft(draft);

        return RedirectToAction(nameof(CreateJobQuote));
    }

    [HttpGet]
    public async Task<IActionResult> CreateJobQuote(CancellationToken cancellationToken)
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

        var model = await jobWorkflow.GetCreateJobQuoteAsync(proveedor.Entity!, draft, cancellationToken);
        if (model == null)
        {
            return RedirectToAction(nameof(CreateJob));
        }

        ApplyCreateJobNavigation(draft);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateJobQuote(ProviderProCreateJobQuoteInput input)
    {
        var draft = GetCreateJobDraft();
        if (string.IsNullOrWhiteSpace(draft.Title))
        {
            return RedirectToAction(nameof(CreateJob));
        }

        draft.SendQuote = input.SendQuote && !string.Equals(input.SubmitAction, "skip", StringComparison.OrdinalIgnoreCase);
        draft.QuoteRequestNotes = input.QuoteRequestNotes.Trim();
        draft.Description = draft.QuoteRequestNotes;
        draft.HasVoiceRecording = input.HasVoiceRecording;
        if (input.HasVoiceRecording)
        {
            draft.IncludeVoiceTranscript = true;
        }
        draft.AiDraftGenerated = false;
        SaveCreateJobDraft(draft);

        return draft.SendQuote
            ? RedirectToAction(nameof(CreateJobAiDraft))
            : RedirectToAction(nameof(CreateJobSend));
    }

    [HttpGet]
    public async Task<IActionResult> CreateJobAiDraft(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetCreateJobDraft();
        if (!draft.SendQuote)
        {
            return RedirectToAction(nameof(CreateJobSend));
        }

        var model = await jobWorkflow.GetCreateJobAiDraftAsync(proveedor.Entity!, draft);
        if (model == null)
        {
            return RedirectToAction(nameof(CreateJob));
        }

        SaveCreateJobDraft(draft);
        ApplyCreateJobNavigation(draft);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateJobAiDraft(ProviderProCreateJobAiDraftInput input)
    {
        var draft = GetCreateJobDraft();
        if (!draft.SendQuote)
        {
            return RedirectToAction(nameof(CreateJobSend));
        }

        if (string.Equals(input.SubmitAction, "regenerate", StringComparison.OrdinalIgnoreCase))
        {
            jobWorkflow.GenerateCreateJobAiDraft(draft, regenerate: true);
            SaveCreateJobDraft(draft);
            return RedirectToAction(nameof(CreateJobAiDraft));
        }

        SaveCreateJobDraft(draft);
        return RedirectToAction(nameof(CreateJobSend));
    }

    [HttpGet]
    public async Task<IActionResult> CreateJobSend(CancellationToken cancellationToken)
    {
        var proveedor = await ResolveProveedorAsync(cancellationToken);
        if (proveedor.Result != null)
        {
            return proveedor.Result;
        }

        var draft = GetCreateJobDraft();
        var model = await jobWorkflow.GetCreateJobSendAsync(proveedor.Entity!, draft);
        if (model == null)
        {
            return RedirectToAction(nameof(CreateJob));
        }

        draft.CustomerMessage = model.CustomerMessage;
        draft.DeliveryMethod = model.DeliveryMethod;
        SaveCreateJobDraft(draft);
        ApplyCreateJobNavigation(draft);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJobSend(ProviderProCreateJobSendInput input, CancellationToken cancellationToken)
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

        draft.DeliveryMethod = input.DeliveryMethod;
        draft.CustomerMessage = input.CustomerMessage.Trim();
        draft.IncludeAiSummary = input.IncludeAiSummary;
        draft.IncludeVoiceTranscript = input.IncludeVoiceTranscript;
        SaveCreateJobDraft(draft);

        var saveAsDraft = string.Equals(input.SubmitAction, "save_draft", StringComparison.OrdinalIgnoreCase);
        var sendQuote = draft.SendQuote
            && string.Equals(input.SubmitAction, "job_and_quote", StringComparison.OrdinalIgnoreCase);

        var jobInput = new ProviderProCreateJobInput
        {
            ServiceCategoryId = draft.ServiceCategoryId,
            ServiceCategory = draft.ServiceCategoryLabel,
            ClienteId = draft.ClienteId,
            CustomerName = draft.CustomerName,
            Address = draft.Address,
            Title = draft.Title,
            Description = draft.QuoteRequestNotes ?? draft.Description,
            Priority = draft.Priority,
            Notes = draft.Notes,
            VisitDate = draft.VisitDate,
            StartTimeLabel = draft.StartTimeLabel,
            EndTimeLabel = draft.EndTimeLabel,
            AssignedTechnician = draft.AssignedTechnician,
            Reminder = draft.Reminder,
            AddToCalendar = draft.AddToCalendar,
            SaveAsDraft = saveAsDraft,
            SendQuoteWithJob = sendQuote,
            EstimateAmount = sendQuote ? draft.EstimateTotal : null,
            EstimateScopeSummary = sendQuote ? draft.ScopeSummary : null,
            DeliveryMethod = draft.DeliveryMethod,
            CustomerMessage = draft.CustomerMessage
        };

        var navOrigin = draft.NavOrigin;
        var jobId = await jobWorkflow.CreateJobAsync(proveedor.Entity!.Id, jobInput, cancellationToken);
        ClearCreateJobDraft();
        return RedirectToAction(nameof(CreateJobSuccess), new { id = jobId, from = navOrigin });
    }

    [HttpGet]
    public async Task<IActionResult> CreateJobSuccess(int id, string? from, CancellationToken cancellationToken)
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

        var navOrigin = NormalizeCreateJobNavOrigin(from);
        ViewBag.NavActive = navOrigin;
        ViewBag.CreateJobFrom = navOrigin;
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

            await signInManager.SignInAsync(user, isPersistent: true);

        }



        var proveedor = await registration.GetProveedorForCurrentUserAsync(cancellationToken);

        if (proveedor == null)

        {

            return (RedirectToAction("Entry", "ProviderRegistration"), null);

        }



        if (proveedor.RegistrationStatus == ProviderRegistrationStatuses.Draft)

        {

            var action = registration.ResolveWizardResumeAction(proveedor);

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

    private static string NormalizeCreateJobNavOrigin(string? from) =>
        string.Equals(from, "home", StringComparison.OrdinalIgnoreCase) ? "home" : "jobs";

    private void ApplyCreateJobNavigation(ProviderProCreateJobDraft? draft = null)
    {
        draft ??= GetCreateJobDraft();
        if (string.IsNullOrWhiteSpace(draft.NavOrigin))
        {
            draft.NavOrigin = "jobs";
        }

        ViewBag.NavActive = draft.NavOrigin;
        ViewBag.CreateJobCancelAction = draft.NavOrigin == "home" ? "Dashboard" : "Jobs";
        ViewBag.CreateJobFrom = draft.NavOrigin;
        ViewBag.CreateJobCancelLabel = draft.NavOrigin == "home" ? localizer["Home"] : localizer["Jobs hub"];
        ViewBag.HideBottomNav = true;
        ViewBag.HideTopbarNotifications = true;
    }

    private void ApplyAddCustomerNavigation()
    {
        ViewBag.HideTopbarNotifications = true;
    }

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

        var draft = JsonSerializer.Deserialize<ProviderProUploadReportDraft>(json, CreateJobDraftJsonOptions)
            ?? new ProviderProUploadReportDraft();
        NormalizeUploadReportDraft(draft);
        return draft;
    }

    private static void NormalizeUploadReportDraft(ProviderProUploadReportDraft draft)
    {
        draft.Title ??= "";
        draft.Summary ??= "";
        draft.WorkCompleted ??= "";
        draft.MaterialsUsed ??= "";
        draft.WarrantyInfo ??= "";
        draft.Recommendations ??= "";
        draft.InternalNotes ??= "";
        draft.ReportType ??= ProviderReportTypes.Completion;
        draft.PhotoSlots ??=
        [
            new() { Slot = "Before" },
            new() { Slot = "During" },
            new() { Slot = "After" },
            new() { Slot = "Final Result" }
        ];
        draft.DocumentSlots ??=
        [
            new() { Slot = "Invoice", Required = true },
            new() { Slot = "Warranty Document", Required = false },
            new() { Slot = "Permit / Receipt", Required = false }
        ];
        draft.GeneralFiles ??= [];
    }

    private void SaveUploadReportDraft(ProviderProUploadReportDraft draft) =>
        HttpContext.Session.SetString(
            UploadReportDraftSessionKey,
            JsonSerializer.Serialize(draft, CreateJobDraftJsonOptions));

    private void ClearUploadReportDraft() =>
        HttpContext.Session.Remove(UploadReportDraftSessionKey);

    private const string UploadPhotosDraftSessionKey = "ProviderProUploadPhotosDraft";

    private ProviderProUploadPhotosDraft GetUploadPhotosDraft()
    {
        var json = HttpContext.Session.GetString(UploadPhotosDraftSessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ProviderProUploadPhotosDraft();
        }

        return JsonSerializer.Deserialize<ProviderProUploadPhotosDraft>(json, CreateJobDraftJsonOptions)
            ?? new ProviderProUploadPhotosDraft();
    }

    private void SaveUploadPhotosDraft(ProviderProUploadPhotosDraft draft) =>
        HttpContext.Session.SetString(
            UploadPhotosDraftSessionKey,
            JsonSerializer.Serialize(draft, CreateJobDraftJsonOptions));

    private void ClearUploadPhotosDraft() =>
        HttpContext.Session.Remove(UploadPhotosDraftSessionKey);

    private const string ExportReportDraftSessionKey = "ProviderProExportReportDraft";

    private ProviderProExportReportDraft GetExportReportDraft()
    {
        var json = HttpContext.Session.GetString(ExportReportDraftSessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ProviderProExportReportDraft();
        }

        return JsonSerializer.Deserialize<ProviderProExportReportDraft>(json, CreateJobDraftJsonOptions)
            ?? new ProviderProExportReportDraft();
    }

    private void SaveExportReportDraft(ProviderProExportReportDraft draft) =>
        HttpContext.Session.SetString(
            ExportReportDraftSessionKey,
            JsonSerializer.Serialize(draft, CreateJobDraftJsonOptions));

    private void ClearExportReportDraft() =>
        HttpContext.Session.Remove(ExportReportDraftSessionKey);

    private const string InsuranceDraftSessionKey = "ProviderProInsuranceQuoteDraft";

    private ProviderProInsuranceQuoteDraft GetInsuranceDraft()
    {
        var json = HttpContext.Session.GetString(InsuranceDraftSessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ProviderProInsuranceQuoteDraft();
        }

        return JsonSerializer.Deserialize<ProviderProInsuranceQuoteDraft>(json, CreateJobDraftJsonOptions)
            ?? new ProviderProInsuranceQuoteDraft();
    }

    private void SaveInsuranceDraft(ProviderProInsuranceQuoteDraft draft) =>
        HttpContext.Session.SetString(
            InsuranceDraftSessionKey,
            JsonSerializer.Serialize(draft, CreateJobDraftJsonOptions));

    private void ClearInsuranceDraft() =>
        HttpContext.Session.Remove(InsuranceDraftSessionKey);

    private static string TrimOrEmpty(string? value) => value?.Trim() ?? "";

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

    private static readonly string[] ProfilePhotoExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] ProfileDocumentExtensions = [".jpg", ".jpeg", ".png", ".webp", ".pdf"];
    private const long MaxProfilePhotoBytes = 10_000_000;
    private const long MaxProfileDocumentBytes = 10_000_000;

    private static readonly HashSet<string> AllowedProfileDocumentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ProviderDocumentTypes.License,
        ProviderDocumentTypes.Insurance,
        ProviderDocumentTypes.GovernmentId,
        ProviderDocumentTypes.W9,
        ProviderDocumentTypes.LiabilityInsurance,
        ProviderDocumentTypes.ContractorLicense,
        ProviderDocumentTypes.HvacLicense,
        ProviderDocumentTypes.PlumbingLicense
    };

    private async Task<(string? Error, string? PhotoUrl)> SaveProfilePhotoAsync(int proveedorId, IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) && file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            ext = ".jpg";
        }

        if (!ProfilePhotoExtensions.Contains(ext))
        {
            return (ProviderProDisplayLocalization.L("Photo must be JPG, PNG, or WEBP."), null);
        }

        if (file.Length > MaxProfilePhotoBytes)
        {
            return (ProviderProDisplayLocalization.L("Photo must be 10 MB or less."), null);
        }

        var uploadDir = Path.Combine(env.WebRootPath, "uploads", "provider", proveedorId.ToString());
        Directory.CreateDirectory(uploadDir);
        var storedName = $"{ProviderDocumentTypes.Logo}-{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadDir, storedName);
        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var relativeUrl = $"/uploads/provider/{proveedorId}/{storedName}";
        await registration.RegisterDocumentUploadAsync(ProviderDocumentTypes.Logo, relativeUrl, proveedorId);
        return (null, relativeUrl);
    }

    private IActionResult PhotoUploadResult(string? error, string? photoUrl = null)
    {
        if (IsAjaxPhotoUploadRequest())
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                return BadRequest(new { ok = false, message = error });
            }

            return Json(new { ok = true, message = localizer["Profile photo updated."].ToString(), photoUrl });
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            TempData["ProfilePhotoError"] = error;
        }
        else
        {
            TempData["ProfilePhotoOk"] = localizer["Profile photo updated."];
        }

        return RedirectToAction(nameof(EditProfile), new { section = "full" });
    }

    private static bool IsAjaxPhotoUploadRequest(Microsoft.AspNetCore.Http.HttpRequest request) =>
        string.Equals(request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

    private bool IsAjaxPhotoUploadRequest() => IsAjaxPhotoUploadRequest(Request);

    private async Task<string?> SaveProviderDocumentAsync(int proveedorId, string documentType, IFormFile file)
    {
        if (!AllowedProfileDocumentTypes.Contains(documentType))
        {
            return ProviderProDisplayLocalization.L("That document type is not supported.");
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) && file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            ext = ".jpg";
        }
        else if (string.IsNullOrEmpty(ext) && file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            ext = ".pdf";
        }

        if (!ProfileDocumentExtensions.Contains(ext))
        {
            return ProviderProDisplayLocalization.L("Document must be JPG, PNG, WEBP, or PDF.");
        }

        if (file.Length > MaxProfileDocumentBytes)
        {
            return ProviderProDisplayLocalization.L("Document must be 10 MB or less.");
        }

        var uploadDir = Path.Combine(env.WebRootPath, "uploads", "provider", proveedorId.ToString());
        Directory.CreateDirectory(uploadDir);
        var storedName = $"{documentType}-{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadDir, storedName);
        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var relativeUrl = $"/uploads/provider/{proveedorId}/{storedName}";
        await registration.RegisterDocumentUploadAsync(documentType, relativeUrl, proveedorId);
        await proData.ApplyVerificationDocumentFlagsAsync(proveedorId, documentType);

        return null;
    }
}

