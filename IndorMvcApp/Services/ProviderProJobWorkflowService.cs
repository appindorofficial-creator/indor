using System.Text.Json;

using IndorMvcApp.Data;

using IndorMvcApp.Models;

using IndorMvcApp.ViewModels;

using Microsoft.EntityFrameworkCore;



namespace IndorMvcApp.Services;



public class ProviderProJobWorkflowService(AppDbContext db) : IProviderProJobWorkflowService

{

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };



    public async Task<ProviderProJobsScheduleViewModel> GetJobsScheduleAsync(

        IndorProveedor proveedor,

        string? view = "today",

        CancellationToken cancellationToken = default)

    {

        var activeView = NormalizeScheduleView(view);

        var today = DateTime.Today;

        var jobs = await db.IndorProveedorJobs

            .AsNoTracking()

            .Where(j => j.ProveedorId == proveedor.Id && !j.IsDraft)

            .ToListAsync(cancellationToken);



        var filtered = activeView switch

        {

            "week" => jobs.Where(j => j.ScheduledAt >= today && j.ScheduledAt < today.AddDays(7)),

            "month" => jobs.Where(j => j.ScheduledAt >= today && j.ScheduledAt < today.AddMonths(1)),

            _ => jobs.Where(j => j.ScheduledAt >= today && j.ScheduledAt < today.AddDays(1))

        };



        var items = filtered

            .OrderBy(j => j.ScheduledAt ?? j.FechaCreacion)

            .Select(j => new ProviderProScheduleJobItemViewModel

            {

                Id = j.Id,

                TimeLabel = j.ScheduledAt.HasValue ? j.ScheduledAt.Value.ToLocalTime().ToString("h:mm tt") : "TBD",

                Title = j.Title,

                Address = j.Address,

                StatusLabel = MapJobStatusLabel(j.Status),

                StatusClass = MapJobStatusClass(j.Status)

            })

            .ToList();



        return new ProviderProJobsScheduleViewModel

        {

            CompanyName = ResolveCompanyName(proveedor),

            ActiveView = activeView,

            DateLabel = today.ToString("dddd, MMM d"),

            JobsTodayCount = jobs.Count(j => j.ScheduledAt >= today && j.ScheduledAt < today.AddDays(1)),

            Jobs = items,

            FlowSteps = ScheduleFlowSteps()

        };

    }

    public async Task<ProviderProCalendarOverviewViewModel> GetCalendarOverviewAsync(
        IndorProveedor proveedor,
        string? view = "week",
        string? filter = "all",
        string? weekStart = null,
        CancellationToken cancellationToken = default)
    {
        var activeView = NormalizeCalendarView(view);
        var activeFilter = NormalizeCalendarFilter(filter);
        var weekStartDate = ParseWeekStart(weekStart);
        var weekEndDate = weekStartDate.AddDays(7);
        var today = DateTime.Today;

        var jobs = await db.IndorProveedorJobs
            .AsNoTracking()
            .Where(j => j.ProveedorId == proveedor.Id && !j.IsDraft && j.ScheduledAt.HasValue)
            .ToListAsync(cancellationToken);

        var weekJobs = jobs
            .Where(j => j.ScheduledAt!.Value >= weekStartDate && j.ScheduledAt < weekEndDate)
            .Where(j => MatchesCalendarFilter(j, activeFilter))
            .ToList();

        var dayHeaders = Enumerable.Range(0, 7).Select(offset =>
        {
            var day = weekStartDate.AddDays(offset);
            return new ProviderProCalendarDayHeaderViewModel
            {
                DayName = day.ToString("ddd"),
                DayNumber = day.Day,
                DateIso = day.ToString("yyyy-MM-dd"),
                IsToday = day == today,
                IsUnavailable = day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
            };
        }).ToList();

        var hourSlots = Enumerable.Range(8, 10).ToList();
        var gridEvents = weekJobs.Select(j =>
        {
            var local = j.ScheduledAt!.Value.ToLocalTime();
            var dayIndex = (int)(local.Date - weekStartDate).TotalDays;
            if (dayIndex < 0 || dayIndex > 6)
            {
                return null;
            }

            var startHour = Math.Clamp(local.Hour, 8, 16);
            var span = 1;
            if (j.ScheduledEndAt.HasValue)
            {
                var endLocal = j.ScheduledEndAt.Value.ToLocalTime();
                span = Math.Max(1, Math.Min(3, (int)Math.Ceiling((endLocal - local).TotalHours)));
            }

            return new ProviderProCalendarGridEventViewModel
            {
                JobId = j.Id,
                Title = j.Title,
                Address = j.Address,
                TimeLabel = local.ToString("h:mm tt"),
                DayIndex = dayIndex,
                StartHour = startHour,
                SpanHours = span,
                ToneClass = MapCalendarEventTone(j.Status, j.ServiceType ?? j.Title)
            };
        }).Where(e => e != null).Cast<ProviderProCalendarGridEventViewModel>().ToList();

        var estimatedMinutes = weekJobs.Sum(EstimateJobMinutes);
        var workHours = 9 * 5;
        var utilization = workHours > 0
            ? Math.Min(100, (int)Math.Round(estimatedMinutes / (workHours * 60.0) * 100))
            : 0;

        return new ProviderProCalendarOverviewViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            ActiveView = activeView,
            ActiveFilter = activeFilter,
            WeekRangeLabel = $"{weekStartDate:MMM d} – {weekEndDate.AddDays(-1):MMM d, yyyy}",
            WeekStartIso = weekStartDate.ToString("yyyy-MM-dd"),
            WeekEventCount = weekJobs.Count,
            EstimatedWorkLabel = FormatWorkDuration(estimatedMinutes),
            AvailableTimeLabel = FormatWorkDuration(Math.Max(0, workHours * 60 - estimatedMinutes)),
            UtilizationPercent = utilization,
            DayHeaders = dayHeaders,
            HourSlots = hourSlots,
            GridEvents = gridEvents,
            FlowSteps = CalendarOverviewFlowSteps()
        };
    }

    public async Task<ProviderProDayScheduleViewModel> GetDayScheduleAsync(
        IndorProveedor proveedor,
        string? date = null,
        CancellationToken cancellationToken = default)
    {
        var day = ParseScheduleDate(date);
        var nextDay = day.AddDays(1);
        var yesterday = day.AddDays(-1);
        var today = DateTime.Today;

        var jobs = await db.IndorProveedorJobs
            .AsNoTracking()
            .Include(j => j.Cliente)
            .Where(j => j.ProveedorId == proveedor.Id && !j.IsDraft && j.ScheduledAt >= day && j.ScheduledAt < nextDay)
            .OrderBy(j => j.ScheduledAt)
            .ToListAsync(cancellationToken);

        var yesterdayCount = await db.IndorProveedorJobs
            .AsNoTracking()
            .CountAsync(j => j.ProveedorId == proveedor.Id && !j.IsDraft
                && j.ScheduledAt >= yesterday && j.ScheduledAt < day, cancellationToken);

        var inProgress = jobs.Count(j => j.Status == ProviderJobStatuses.InProgress);
        var nextJob = jobs.FirstOrDefault(j => j.Status != ProviderJobStatuses.Completed && j.ScheduledAt >= DateTime.Now);

        return new ProviderProDayScheduleViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            DateIso = day.ToString("yyyy-MM-dd"),
            DateLabel = day == today
                ? $"Today, {day:dddd, MMM d, yyyy}"
                : day.ToString("dddd, MMM d, yyyy"),
            EventsTodayCount = jobs.Count,
            InProgressCount = inProgress,
            NextJobTime = nextJob?.ScheduledAt?.ToLocalTime().ToString("h:mm tt"),
            NextJobTitle = nextJob?.Title,
            EventsDeltaLabel = jobs.Count >= yesterdayCount
                ? $"+{jobs.Count - yesterdayCount} vs yesterday"
                : $"{jobs.Count - yesterdayCount} vs yesterday",
            Items = jobs.Select(j => new ProviderProDayScheduleItemViewModel
            {
                Id = j.Id,
                TimeLabel = j.ScheduledAt!.Value.ToLocalTime().ToString("h:mm tt"),
                Title = j.Title,
                Address = j.Address,
                CustomerName = j.Cliente?.Name ?? "Customer",
                StatusLabel = MapJobStatusLabel(j.Status),
                StatusClass = MapJobStatusClass(j.Status),
                IconClass = MapCalendarServiceIcon(j.ServiceType ?? j.Title),
                CanStart = j.Status is ProviderJobStatuses.Scheduled or ProviderJobStatuses.Confirmed
            }).ToList(),
            FlowSteps = DayScheduleFlowSteps(day.ToString("yyyy-MM-dd"))
        };
    }

    public async Task<ProviderProRescheduleJobViewModel?> GetRescheduleJobAsync(
        IndorProveedor proveedor,
        int jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await LoadJobAsync(proveedor.Id, jobId, cancellationToken);
        if (job == null)
        {
            return null;
        }

        var localStart = job.ScheduledAt?.ToLocalTime();
        var localEnd = job.ScheduledEndAt?.ToLocalTime();
        var technicians = await db.IndorProveedorJobs
            .AsNoTracking()
            .Where(j => j.ProveedorId == proveedor.Id && !string.IsNullOrWhiteSpace(j.AssignedTechnician))
            .Select(j => j.AssignedTechnician!)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (technicians.Count == 0)
        {
            technicians.Add(job.AssignedTechnician ?? proveedor.PrimaryContact ?? "Technician");
        }

        return new ProviderProRescheduleJobViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            JobId = job.Id,
            Title = job.Title,
            Address = job.Address,
            VisitDate = localStart?.ToString("yyyy-MM-dd") ?? DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
            StartTimeLabel = localStart?.ToString("h:mm tt") ?? "9:00 AM",
            EndTimeLabel = localEnd?.ToString("h:mm tt") ?? "11:00 AM",
            AssignedTechnician = job.AssignedTechnician ?? technicians[0],
            DurationLabel = EstimateDurationLabel(job.ScheduledAt, job.ScheduledEndAt),
            Notes = job.JobNotes ?? job.ScopeOfWork ?? "",
            TimeOptions = BuildTimeOptions(),
            DurationOptions = ["1 hour", "1.5 hours", "2 hours", "3 hours", "4 hours"],
            TechnicianOptions = technicians,
            FlowSteps = RescheduleJobFlowSteps(job.Title)
        };
    }

    public async Task<bool> RescheduleJobAsync(
        int proveedorId,
        ProviderProRescheduleJobInput input,
        CancellationToken cancellationToken = default)
    {
        var job = await db.IndorProveedorJobs
            .FirstOrDefaultAsync(j => j.Id == input.JobId && j.ProveedorId == proveedorId, cancellationToken);

        if (job == null)
        {
            return false;
        }

        var scheduledAt = ParseVisitSchedule(input.VisitDate, input.StartTimeLabel);
        var scheduledEndAt = ParseVisitSchedule(input.VisitDate, input.EndTimeLabel);
        if (!scheduledEndAt.HasValue && scheduledAt.HasValue)
        {
            scheduledEndAt = scheduledAt.Value.Add(ParseDurationLabel(input.DurationLabel));
        }

        job.ScheduledAt = scheduledAt;
        job.ScheduledEndAt = scheduledEndAt;
        job.AssignedTechnician = input.AssignedTechnician;
        if (!string.IsNullOrWhiteSpace(input.Notes))
        {
            job.JobNotes = input.Notes.Trim();
        }

        if (job.Status is ProviderJobStatuses.Scheduled or ProviderJobStatuses.Confirmed)
        {
            job.Status = ProviderJobStatuses.Confirmed;
        }

        job.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ProviderProCalendarUpdatedViewModel?> GetCalendarUpdatedAsync(
        IndorProveedor proveedor,
        int jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await LoadJobAsync(proveedor.Id, jobId, cancellationToken);
        if (job == null)
        {
            return null;
        }

        var localStart = job.ScheduledAt?.ToLocalTime();
        var localEnd = job.ScheduledEndAt?.ToLocalTime();
        var timeLabel = localStart.HasValue
            ? localEnd.HasValue
                ? $"{localStart:h:mm tt} – {localEnd:h:mm tt}"
                : localStart.Value.ToString("h:mm tt")
            : "—";

        return new ProviderProCalendarUpdatedViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            JobId = job.Id,
            Title = job.Title,
            Address = job.Address,
            UpdatedDateLabel = localStart?.ToString("MMM d, yyyy (ddd)") ?? "—",
            UpdatedTimeLabel = timeLabel,
            AssignedTechnician = job.AssignedTechnician ?? "—",
            StatusLabel = "Rescheduled",
            FlowSteps = CalendarUpdatedFlowSteps()
        };
    }



    private static readonly (string Id, string Label, string Icon, string Tone, string SuggestedName)[] CreateJobWizardTypes =
    [
        ("inspection", "Inspection", "fa-droplet", "blue", "Water Damage Inspection"),
        ("repair", "Repair", "fa-wrench", "green", "Repair Service"),
        ("maintenance", "Maintenance", "fa-gear", "purple", "Maintenance Visit"),
        ("estimate", "Estimate", "fa-calculator", "teal", "Estimate Request"),
        ("installation", "Installation", "fa-screwdriver-wrench", "orange", "Installation Job"),
        ("emergency", "Emergency", "fa-bell", "red", "Emergency Service"),
        ("cleaning", "Cleaning", "fa-broom", "blue", "Cleaning Service"),
        ("custom", "Custom Type", "fa-ellipsis", "gray", "Custom Job"),
    ];

    public Task<ProviderProCreateJobCategoriesViewModel> GetCreateJobCategoriesAsync(
        IndorProveedor proveedor,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderProCreateJobCategoriesViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            Categories = CreateJobWizardTypes.Select(c => new ProviderProCreateJobCategoryOptionViewModel
            {
                Id = c.Id,
                Label = c.Label,
                Description = "",
                IconClass = c.Icon,
                ToneClass = c.Tone,
                SuggestedJobName = c.SuggestedName
            }).ToList(),
            WizardSteps = BuildCreateJobWizardSteps(1)
        });
    }

    public async Task<ProviderProCreateJobDetailsViewModel?> GetCreateJobDetailsAsync(
        IndorProveedor proveedor,
        string categoryId,
        ProviderProCreateJobDraft? draft,
        CancellationToken cancellationToken = default)
    {
        var category = await ResolveJobCategoryAsync(categoryId, cancellationToken);
        if (category == null)
        {
            return null;
        }

        var customerRows = await db.IndorProveedorClientes
            .AsNoTracking()
            .Where(c => c.ProveedorId == proveedor.Id && c.Activo)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var customers = customerRows.Select(c => new ProviderProCreateJobCustomerOptionViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Initials = DeriveCustomerInitials(c.Name),
            ToneClass = DeriveAvatarTone(c.Id),
            Address = string.IsNullOrWhiteSpace(c.Address) ? c.StreetAddress : c.Address,
            PropertyLabel = DerivePropertyLabel(c.PropertyType),
            IsConnected = c.IsAppConnected || c.ConnectionStatus == ProviderCustomerConnectionStatuses.Connected,
            PropertiesCount = Math.Max(1, c.PropertiesCount)
        }).ToList();

        var selectedCustomer = draft?.ClienteId.HasValue == true
            ? customers.FirstOrDefault(c => c.Id == draft.ClienteId)
            : null;

        return new ProviderProCreateJobDetailsViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            ServiceCategoryId = category.Id,
            ServiceCategoryLabel = category.LabelEn,
            Title = draft?.Title ?? "",
            ClienteId = draft?.ClienteId ?? selectedCustomer?.Id,
            CustomerName = draft?.CustomerName ?? selectedCustomer?.Name ?? "",
            Address = draft?.Address ?? selectedCustomer?.Address ?? "",
            Description = draft?.Description ?? "",
            Priority = draft?.Priority ?? "Medium",
            Notes = draft?.Notes ?? "",
            Customers = customers,
            WizardSteps = BuildCreateJobWizardSteps(2)
        };
    }

    public Task<ProviderProCreateJobQuoteViewModel?> GetCreateJobQuoteAsync(
        IndorProveedor proveedor,
        ProviderProCreateJobDraft draft,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(draft.ServiceCategoryId) || string.IsNullOrWhiteSpace(draft.Title)
            || string.IsNullOrWhiteSpace(draft.CustomerName))
        {
            return Task.FromResult<ProviderProCreateJobQuoteViewModel?>(null);
        }

        var (icon, tone) = ResolveWizardTypeVisuals(draft.ServiceCategoryId);
        var notes = string.IsNullOrWhiteSpace(draft.QuoteRequestNotes)
            ? BuildDefaultQuoteRequestNotes(draft)
            : draft.QuoteRequestNotes;

        if (draft.Attachments.Count == 0)
        {
            draft.Attachments = BuildDefaultQuoteAttachments();
        }

        return Task.FromResult<ProviderProCreateJobQuoteViewModel?>(new ProviderProCreateJobQuoteViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            JobTitle = draft.Title,
            ServiceCategoryLabel = draft.ServiceCategoryLabel,
            ServiceCategoryIcon = icon,
            ServiceCategoryTone = tone,
            CustomerName = draft.CustomerName,
            CustomerInitials = DeriveCustomerInitials(draft.CustomerName),
            CustomerTone = DeriveAvatarTone(draft.ClienteId ?? draft.CustomerName.GetHashCode()),
            SendQuote = draft.SendQuote,
            QuoteRequestNotes = notes,
            Attachments = draft.Attachments,
            StepSubtitle = "Create quote request",
            WizardSteps = BuildCreateJobWizardSteps(3)
        });
    }

    public Task<ProviderProCreateJobAiDraftViewModel?> GetCreateJobAiDraftAsync(
        IndorProveedor proveedor,
        ProviderProCreateJobDraft draft)
    {
        if (!draft.SendQuote || string.IsNullOrWhiteSpace(draft.Title) || string.IsNullOrWhiteSpace(draft.Address))
        {
            return Task.FromResult<ProviderProCreateJobAiDraftViewModel?>(null);
        }

        if (!draft.AiDraftGenerated)
        {
            GenerateCreateJobAiDraft(draft);
        }

        var (icon, tone) = ResolveWizardTypeVisuals(draft.ServiceCategoryId);

        return Task.FromResult<ProviderProCreateJobAiDraftViewModel?>(new ProviderProCreateJobAiDraftViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            JobTitle = draft.Title,
            ServiceCategoryLabel = draft.ServiceCategoryLabel,
            ServiceCategoryIcon = icon,
            ServiceCategoryTone = tone,
            CustomerName = draft.CustomerName,
            CustomerInitials = DeriveCustomerInitials(draft.CustomerName),
            CustomerTone = DeriveAvatarTone(draft.ClienteId ?? draft.CustomerName.GetHashCode()),
            AiCustomerNeeds = draft.AiCustomerNeeds,
            AiRecommendedScope = draft.AiRecommendedScope,
            EstimateLines = draft.EstimateLines,
            EstimateTotalLabel = FormatCurrency(draft.EstimateTotal),
            StepSubtitle = "AI estimate assistant",
            WizardSteps = BuildCreateJobWizardSteps(4)
        });
    }

    public Task<ProviderProCreateJobSendViewModel?> GetCreateJobSendAsync(
        IndorProveedor proveedor,
        ProviderProCreateJobDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Title) || string.IsNullOrWhiteSpace(draft.Address))
        {
            return Task.FromResult<ProviderProCreateJobSendViewModel?>(null);
        }

        if (draft.SendQuote)
        {
            if (!draft.AiDraftGenerated)
            {
                GenerateCreateJobAiDraft(draft);
            }

            RefineEstimateForSend(draft);
        }

        var (icon, tone) = ResolveWizardTypeVisuals(draft.ServiceCategoryId);
        var message = string.IsNullOrWhiteSpace(draft.CustomerMessage)
            ? BuildDefaultCustomerMessage(draft)
            : draft.CustomerMessage;

        return Task.FromResult<ProviderProCreateJobSendViewModel?>(new ProviderProCreateJobSendViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            JobTitle = draft.Title,
            ServiceCategoryLabel = draft.ServiceCategoryLabel,
            ServiceCategoryIcon = icon,
            ServiceCategoryTone = tone,
            CustomerName = draft.CustomerName,
            Address = draft.Address,
            EstimateLines = draft.EstimateLines,
            EstimateTotalLabel = draft.SendQuote ? FormatCurrency(draft.EstimateTotal) : "",
            ScopeSummary = draft.ScopeSummary,
            DeliveryMethod = draft.DeliveryMethod,
            CustomerMessage = message,
            IncludeAiSummary = draft.IncludeAiSummary,
            IncludeVoiceTranscript = draft.HasVoiceRecording && draft.IncludeVoiceTranscript,
            SendQuote = draft.SendQuote,
            StepSubtitle = "Finalize the job and quote",
            WizardSteps = BuildCreateJobWizardSteps(5)
        });
    }

    public void GenerateCreateJobAiDraft(ProviderProCreateJobDraft draft, bool regenerate = false)
    {
        if (draft.AiDraftGenerated && !regenerate)
        {
            return;
        }

        var notes = string.IsNullOrWhiteSpace(draft.QuoteRequestNotes)
            ? draft.Description
            : draft.QuoteRequestNotes;

        draft.AiCustomerNeeds = BuildAiCustomerNeeds(draft, notes);
        draft.AiRecommendedScope = BuildAiRecommendedScope(draft);
        draft.EstimateLines = BuildAiEstimateLines(draft);
        draft.EstimateTotal = draft.EstimateLines.Sum(l => l.Amount);
        draft.ScopeSummary = BuildScopeSummary(draft);
        draft.AiDraftGenerated = true;
    }

    public async Task<ProviderProCreateJobSuccessViewModel?> GetCreateJobSuccessAsync(
        IndorProveedor proveedor,
        int jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await db.IndorProveedorJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobId && j.ProveedorId == proveedor.Id, cancellationToken);

        if (job == null)
        {
            return null;
        }

        return new ProviderProCreateJobSuccessViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            JobId = job.Id,
            JobCode = job.JobCode,
            Title = job.Title,
            Address = job.Address,
            ScheduleLabel = FormatJobScheduleLabel(job.ScheduledAt, job.ScheduledEndAt),
            StatusLabel = MapJobStatusLabel(job.Status),
            StatusClass = MapJobStatusClass(job.Status),
            FlowSteps = CreateJobSuccessFlowSteps(job.JobCode)
        };
    }

    public async Task<int> CreateJobAsync(int proveedorId, ProviderProCreateJobInput input, CancellationToken cancellationToken = default)
    {
        var startTime = string.IsNullOrWhiteSpace(input.StartTimeLabel) ? input.TimeLabel : input.StartTimeLabel;
        var scheduledAt = input.SaveAsDraft
            ? null
            : ParseVisitSchedule(input.VisitDate, startTime);
        var scheduledEndAt = input.SaveAsDraft
            ? null
            : ParseVisitSchedule(input.VisitDate, input.EndTimeLabel);

        var serviceType = !string.IsNullOrWhiteSpace(input.ServiceCategory)
            ? input.ServiceCategory
            : input.ServiceCategoryId;

        var job = new IndorProveedorJob
        {
            ProveedorId = proveedorId,
            JobCode = $"JOB-{DateTime.UtcNow:yyyyMMddHHmm}",
            Title = input.Title,
            Address = input.Address,
            ServiceType = serviceType,
            Status = ProviderJobStatuses.Scheduled,
            IsDraft = input.SaveAsDraft,
            AssignedTechnician = input.AssignedTechnician,
            Priority = input.Priority,
            JobNotes = input.Notes,
            ScopeOfWork = string.IsNullOrWhiteSpace(input.Description) ? input.Notes : input.Description,
            ScheduledAt = scheduledAt,
            ScheduledEndAt = scheduledEndAt,
            ReminderSetting = input.Reminder,
            AddToCalendar = input.AddToCalendar,
            FechaCreacion = DateTime.UtcNow
        };

        job.ClienteId = await ResolveClienteIdAsync(proveedorId, input.ClienteId, input.CustomerName, input.Address, cancellationToken);

        if (input.EstimateAmount.HasValue && input.EstimateAmount.Value > 0)
        {
            job.EstimateAmount = input.EstimateAmount;
            job.EstimateCode = $"EST-{DateTime.UtcNow:yyyyMMddHHmm}";
        }

        if (!string.IsNullOrWhiteSpace(input.EstimateScopeSummary))
        {
            job.ScopeOfWork = input.EstimateScopeSummary;
        }

        if (!string.IsNullOrWhiteSpace(input.CustomerMessage))
        {
            job.JobNotes = string.IsNullOrWhiteSpace(job.JobNotes)
                ? input.CustomerMessage
                : $"{job.JobNotes}\n\n{input.CustomerMessage}";
        }

        db.IndorProveedorJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);
        return job.Id;
    }



    public async Task<ProviderProJobDetailsViewModel?> GetJobDetailsAsync(

        IndorProveedor proveedor,

        int jobId,

        bool fromCalendar = false,

        string? calendarDate = null,

        CancellationToken cancellationToken = default)

    {

        var job = await LoadJobAsync(proveedor.Id, jobId, cancellationToken);

        if (job == null)

        {

            return null;

        }



        var checklist = ParseChecklist(job.ChecklistJson);

        var customer = job.Cliente;



        return new ProviderProJobDetailsViewModel

        {

            CompanyName = ResolveCompanyName(proveedor),

            JobId = job.Id,

            JobCode = job.JobCode,

            EstimateCode = job.EstimateCode,

            Title = job.Title,

            Address = job.Address,

            ServiceType = job.ServiceType ?? job.Title,

            StatusLabel = MapJobStatusLabel(job.Status),

            StatusClass = MapJobStatusClass(job.Status),

            DistanceLabel = job.DistanceMiles.HasValue ? $"{job.DistanceMiles:0.#} miles away" : null,

            ImageUrl = string.IsNullOrWhiteSpace(job.ImageUrl) ? "/welcome-house.png" : job.ImageUrl,

            AppointmentLabel = FormatAppointment(job.ScheduledAt),

            InvoiceStatus = job.InvoiceStatus ?? "Pending",

            PaymentLabel = job.PaymentAmount.HasValue ? job.PaymentAmount.Value.ToString("C0") : null,

            CustomerName = customer?.Name ?? "Customer",

            CustomerInitials = BuildInitials(customer?.Name ?? "Customer"),

            IsHomeownerVerified = customer?.IsPropertyVerified ?? false,

            CustomerPhone = customer?.Phone,

            CustomerEmail = customer?.Email,

            ScopeOfWork = job.ScopeOfWork,

            MaterialsNeeded = job.MaterialsNeeded,

            AccessInstructions = job.AccessInstructions,

            JobNotes = job.JobNotes,

            Checklist = checklist.Take(3).ToList(),

            ChecklistCompleted = checklist.Count(c => c.IsCompleted),

            ChecklistTotal = checklist.Count,

            CanStart = job.Status is ProviderJobStatuses.Scheduled or ProviderJobStatuses.Confirmed,

            FlowSteps = JobDetailsFlowSteps(job.Title, fromCalendar, calendarDate)

        };

    }



    public async Task<bool> StartJobAsync(int proveedorId, int jobId, CancellationToken cancellationToken = default)

    {

        var job = await db.IndorProveedorJobs

            .FirstOrDefaultAsync(j => j.Id == jobId && j.ProveedorId == proveedorId, cancellationToken);



        if (job == null || job.Status == ProviderJobStatuses.Completed)

        {

            return false;

        }



        job.Status = ProviderJobStatuses.InProgress;

        job.StartedAt = DateTime.UtcNow;

        job.FechaActualizacion = DateTime.UtcNow;



        await db.SaveChangesAsync(cancellationToken);

        return true;

    }



    public async Task<ProviderProActiveJobViewModel?> GetActiveJobAsync(

        IndorProveedor proveedor,

        int jobId,

        CancellationToken cancellationToken = default)

    {

        var job = await LoadJobAsync(proveedor.Id, jobId, cancellationToken);

        if (job == null || job.Status != ProviderJobStatuses.InProgress)

        {

            return null;

        }



        var elapsed = job.StartedAt.HasValue

            ? DateTime.UtcNow - job.StartedAt.Value

            : TimeSpan.Zero;



        return new ProviderProActiveJobViewModel

        {

            CompanyName = ResolveCompanyName(proveedor),

            JobId = job.Id,

            Title = job.Title,

            Address = job.Address,

            ServiceType = job.ServiceType ?? job.Title,

            DistanceLabel = job.DistanceMiles.HasValue ? $"{job.DistanceMiles:0.#} miles away" : null,

            ImageUrl = string.IsNullOrWhiteSpace(job.ImageUrl) ? "/welcome-house.png" : job.ImageUrl,

            StartedLabel = job.StartedAt.HasValue ? job.StartedAt.Value.ToLocalTime().ToString("h:mm tt") : "",

            ElapsedLabel = $"{(int)elapsed.TotalMinutes:00}:{elapsed.Seconds:00}",

            Checklist = ParseChecklist(job.ChecklistJson),

            PhotoLabels = ParsePhotoLabels(job.PhotoUrlsJson),

            Materials = ParseMaterials(job.MaterialsUsedJson),

            JobNotes = job.JobNotes,

            HomeownerSignature = job.HomeownerSignature,

            SignatureLabel = job.HomeownerSignedAt.HasValue

                ? $"{job.HomeownerSignature} • {job.HomeownerSignedAt.Value.ToLocalTime():MMM d, h:mm tt}"

                : null,

            HasSignature = !string.IsNullOrWhiteSpace(job.HomeownerSignature),

            FlowSteps = ActiveJobFlowSteps(job.Title)

        };

    }



    public async Task<bool> CompleteJobAsync(int proveedorId, int jobId, CancellationToken cancellationToken = default)

    {

        var job = await db.IndorProveedorJobs

            .Include(j => j.Cliente)

            .FirstOrDefaultAsync(j => j.Id == jobId && j.ProveedorId == proveedorId, cancellationToken);



        if (job == null || job.Status != ProviderJobStatuses.InProgress)

        {

            return false;

        }



        job.Status = ProviderJobStatuses.Completed;

        job.CompletedAt = DateTime.UtcNow;

        job.FechaActualizacion = DateTime.UtcNow;

        job.ReportCode ??= $"R-{job.Id}";



        var existingReport = await db.IndorProveedorReports

            .FirstOrDefaultAsync(r => r.JobId == job.Id, cancellationToken);



        if (existingReport == null)

        {

            db.IndorProveedorReports.Add(new IndorProveedorReport

            {

                ProveedorId = proveedorId,

                JobId = job.Id,

                ClienteId = job.ClienteId,

                ReportCode = job.ReportCode,

                Title = job.Title,

                Address = job.Address,

                CustomerName = job.Cliente?.Name,

                ServiceType = job.ServiceType,

                Status = ProviderReportStatuses.Approval,

                PhotosCount = job.PhotosCount,

                HasChecklist = !string.IsNullOrWhiteSpace(job.ChecklistJson),

                HasWarranty = !string.IsNullOrWhiteSpace(job.LaborWarranty),

                HasDocuments = true,

                CompletedUtc = job.CompletedAt

            });

        }



        await db.SaveChangesAsync(cancellationToken);

        return true;

    }



    public async Task<ProviderProJobCompletionReportViewModel?> GetJobReportAsync(

        IndorProveedor proveedor,

        int jobId,

        CancellationToken cancellationToken = default)

    {

        var job = await LoadJobAsync(proveedor.Id, jobId, cancellationToken);

        if (job == null || job.Status != ProviderJobStatuses.Completed)

        {

            return null;

        }



        return new ProviderProJobCompletionReportViewModel

        {

            CompanyName = ResolveCompanyName(proveedor),

            JobId = job.Id,

            ReportCode = job.ReportCode ?? $"R-{job.Id}",

            Title = job.Title,

            Address = job.Address,

            CompletedLabel = job.CompletedAt.HasValue

                ? $"Completed: {job.CompletedAt.Value.ToLocalTime():dddd, MMM d 'at' h:mm tt}"

                : "Completed",

            ImageUrl = string.IsNullOrWhiteSpace(job.ImageUrl) ? "/welcome-house.png" : job.ImageUrl,

            Photos = ParseReportPhotos(job.PhotoUrlsJson),

            WorkPerformed = job.WorkPerformed ?? job.ScopeOfWork,

            Materials = ParseMaterials(job.MaterialsUsedJson),

            LaborWarranty = job.LaborWarranty,

            FinalNotes = job.FinalNotes ?? job.JobNotes,

            HomeownerSignature = job.HomeownerSignature,

            SignedLabel = job.HomeownerSignedAt.HasValue

                ? $"Signed: {job.HomeownerSignedAt.Value.ToLocalTime():MMM d, yyyy h:mm tt}"

                : null,

            FlowSteps = ReportFlowSteps(job.Title)

        };

    }



    private async Task<IndorProveedorJob?> LoadJobAsync(int proveedorId, int jobId, CancellationToken cancellationToken) =>

        await db.IndorProveedorJobs

            .AsNoTracking()

            .Include(j => j.Cliente)

            .FirstOrDefaultAsync(j => j.Id == jobId && j.ProveedorId == proveedorId, cancellationToken);



    private static DateTime? ParseVisitSchedule(string? visitDate, string? timeLabel)

    {

        if (string.IsNullOrWhiteSpace(visitDate) || !DateTime.TryParse(visitDate, out var date))

        {

            return null;

        }



        date = date.Date;



        if (!string.IsNullOrWhiteSpace(timeLabel) && DateTime.TryParse(timeLabel, out var parsedTime))

        {

            return date.Add(parsedTime.TimeOfDay);

        }



        return date;

    }



    private static List<ProviderProFlowStepViewModel> CalendarOverviewFlowSteps() =>
    [
        new() { Label = "From: Home Dashboard", IconClass = "fa-house", IsLink = true, Url = "/Proveedor/Dashboard" },
        new() { Label = "Pressed: Calendar", IconClass = "fa-calendar" },
        new() { Label = "Now: Calendar Overview", IconClass = "fa-calendar-week", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> DayScheduleFlowSteps(string dateIso) =>
    [
        new() { Label = "From: Calendar Overview", IconClass = "fa-calendar-week", IsLink = true, Url = "/Proveedor/Calendar" },
        new() { Label = "Pressed: View Day", IconClass = "fa-calendar-day" },
        new() { Label = "Now: Day Schedule", IconClass = "fa-list", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> RescheduleJobFlowSteps(string title) =>
    [
        new() { Label = "From: Job Details", IconClass = "fa-clipboard-list" },
        new() { Label = "Pressed: Edit Schedule", IconClass = "fa-pen" },
        new() { Label = "Now: Reschedule Job", IconClass = "fa-calendar", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> CalendarUpdatedFlowSteps() =>
    [
        new() { Label = "From: Reschedule Job", IconClass = "fa-calendar" },
        new() { Label = "Pressed: Save Changes", IconClass = "fa-circle-check" },
        new() { Label = "Now: Calendar Updated", IconClass = "fa-circle-check", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> ScheduleFlowSteps() =>

    [

        new() { Label = "Home Dashboard", IconClass = "fa-house", IsLink = true, Url = "/Proveedor/Dashboard" },

        new() { Label = "Pressed: Jobs Today / View Schedule", IconClass = "fa-hand-pointer" },

        new() { Label = "Now: Jobs Schedule", IconClass = "fa-calendar-day", IsCurrent = true }

    ];



    private async Task<IndorProveedorCategoriaCatalogo?> ResolveJobCategoryAsync(
        string categoryId,
        CancellationToken cancellationToken)
    {
        var category = await db.IndorProveedorCategoriasCatalogo
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.Activo, cancellationToken);

        if (category != null)
        {
            return category;
        }

        var wizardType = CreateJobWizardTypes
            .FirstOrDefault(c => c.Id.Equals(categoryId, StringComparison.OrdinalIgnoreCase));
        if (wizardType != default)
        {
            return new IndorProveedorCategoriaCatalogo
            {
                Id = wizardType.Id,
                LabelEn = wizardType.Label,
                IconClass = wizardType.Icon,
                Activo = true
            };
        }

        var fallback = OnboardingCatalog.ProviderCategories
            .FirstOrDefault(c => c.Id.Equals(categoryId, StringComparison.OrdinalIgnoreCase));

        return fallback == null
            ? null
            : new IndorProveedorCategoriaCatalogo
            {
                Id = fallback.Id,
                LabelEn = fallback.Label,
                IconClass = fallback.IconClass,
                Activo = true
            };
    }

    private static List<ProviderProWizardStepViewModel> BuildCreateJobWizardSteps(int currentStep)
    {
        var labels = new[] { "Type", "Customer", "Quote", "AI Draft", "Send" };
        var allComplete = currentStep >= 5;
        return labels.Select((label, index) =>
        {
            var stepNumber = index + 1;
            return new ProviderProWizardStepViewModel
            {
                Number = stepNumber,
                Label = label,
                IsComplete = allComplete || stepNumber < currentStep,
                IsCurrent = !allComplete && stepNumber == currentStep
            };
        }).ToList();
    }

    private static string DeriveCustomerInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return "?";
        }

        if (parts.Length == 1)
        {
            return parts[0].Length <= 2
                ? parts[0].ToUpperInvariant()
                : parts[0][..2].ToUpperInvariant();
        }

        return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
    }

    private static string DeriveAvatarTone(int id) => (id % 5) switch
    {
        0 => "blue",
        1 => "green",
        2 => "purple",
        3 => "orange",
        _ => "teal"
    };

    private static string DerivePropertyLabel(string? propertyType) =>
        string.IsNullOrWhiteSpace(propertyType) ? "Primary Home" : propertyType;

    private static (string Icon, string Tone) ResolveWizardTypeVisuals(string categoryId)
    {
        var match = CreateJobWizardTypes.FirstOrDefault(c => c.Id.Equals(categoryId, StringComparison.OrdinalIgnoreCase));
        return match == default ? ("fa-wrench", "blue") : (match.Icon, match.Tone);
    }

    private static string BuildDefaultQuoteRequestNotes(ProviderProCreateJobDraft draft)
    {
        if (draft.ServiceCategoryId.Equals("inspection", StringComparison.OrdinalIgnoreCase)
            || draft.Title.Contains("inspection", StringComparison.OrdinalIgnoreCase))
        {
            return "Please inspect the water damage in the kitchen ceiling and adjacent living room. Identify the source, document affected areas, and provide a repair estimate. Include minor drywall repair and repaint as needed.";
        }

        return $"Please review the requested {draft.ServiceCategoryLabel.ToLowerInvariant()} work at {draft.Address}. Document findings and provide a repair estimate with recommended next steps.";
    }

    private static List<ProviderProCreateJobAttachmentViewModel> BuildDefaultQuoteAttachments() =>
    [
        new() { Id = "ceiling", Name = "Ceiling Damage.jpg", SizeLabel = "2.4 MB", Kind = "image", ThumbnailUrl = "/welcome-house.png" },
        new() { Id = "kitchen", Name = "Kitchen Area.jpg", SizeLabel = "1.8 MB", Kind = "image", ThumbnailUrl = "/welcome-house.png" },
        new() { Id = "notes", Name = "Notes.docx", SizeLabel = "412 KB", Kind = "document" }
    ];

    private static string BuildAiCustomerNeeds(ProviderProCreateJobDraft draft, string notes)
    {
        if (!string.IsNullOrWhiteSpace(notes) && notes.Length > 40)
        {
            return notes;
        }

        return "Customer reports water coming through the ceiling after heavy rain. They'd like an inspection to find the cause and get a repair recommendation and estimate.";
    }

    private static List<string> BuildAiRecommendedScope(ProviderProCreateJobDraft draft)
    {
        if (draft.ServiceCategoryId.Equals("inspection", StringComparison.OrdinalIgnoreCase)
            || draft.Title.Contains("inspection", StringComparison.OrdinalIgnoreCase))
        {
            return
            [
                "Inspect water-damaged area",
                "Document moisture readings and affected materials",
                "Identify likely source of water intrusion",
                "Recommend minor drywall repair (if needed)",
                "Provide homeowner summary and next steps"
            ];
        }

        return
        [
            $"Assess the requested {draft.ServiceCategoryLabel.ToLowerInvariant()} work",
            "Document findings and affected areas",
            "Identify materials and labor needed",
            "Provide homeowner summary and next steps"
        ];
    }

    private static List<ProviderProCreateJobEstimateLineViewModel> BuildAiEstimateLines(ProviderProCreateJobDraft draft)
    {
        if (draft.ServiceCategoryId.Equals("inspection", StringComparison.OrdinalIgnoreCase)
            || draft.Title.Contains("inspection", StringComparison.OrdinalIgnoreCase))
        {
            return
            [
                Line("Service call", 95m),
                Line("Inspection & moisture assessment", 275m),
                Line("Minor drywall repair allowance", 350m),
                Line("Materials allowance", 125m)
            ];
        }

        return
        [
            Line("Service call", 95m),
            Line("On-site assessment", 175m),
            Line("Labor allowance", 250m),
            Line("Materials allowance", 100m)
        ];
    }

    private static void RefineEstimateForSend(ProviderProCreateJobDraft draft)
    {
        if (draft.ServiceCategoryId.Equals("inspection", StringComparison.OrdinalIgnoreCase)
            || draft.Title.Contains("inspection", StringComparison.OrdinalIgnoreCase))
        {
            draft.EstimateLines =
            [
                Line("On-site inspection & assessment", 225m),
                Line("Moisture mapping & documentation", 150m),
                Line("Photos & AI analysis", 125m),
                Line("Report & recommendations", 125m)
            ];
            draft.EstimateTotal = draft.EstimateLines.Sum(l => l.Amount);
            draft.ScopeSummary = "Inspect affected areas, identify moisture sources, document damage, and provide a detailed report with recommendations.";
        }
    }

    private static string BuildScopeSummary(ProviderProCreateJobDraft draft) =>
        draft.AiRecommendedScope.Count > 0
            ? string.Join(" ", draft.AiRecommendedScope.Take(3)) + "."
            : $"Complete the requested {draft.ServiceCategoryLabel.ToLowerInvariant()} work and provide a detailed summary.";

    private static string BuildDefaultCustomerMessage(ProviderProCreateJobDraft draft)
    {
        var firstName = draft.CustomerName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? draft.CustomerName;
        return $"Hi {firstName}, here's your quote for the {draft.Title.ToLowerInvariant()}. Let us know if you have any questions!";
    }

    private static ProviderProCreateJobEstimateLineViewModel Line(string label, decimal amount) =>
        new() { Label = label, Amount = amount, AmountLabel = FormatCurrency(amount) };

    private static string FormatCurrency(decimal amount) =>
        amount.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("en-US"));

    private async Task<int?> ResolveClienteIdAsync(
        int proveedorId,
        int? clienteId,
        string customerName,
        string address,
        CancellationToken cancellationToken)
    {
        if (clienteId.HasValue)
        {
            var existing = await db.IndorProveedorClientes
                .FirstOrDefaultAsync(c => c.Id == clienteId.Value && c.ProveedorId == proveedorId, cancellationToken);
            if (existing != null)
            {
                return existing.Id;
            }
        }

        if (string.IsNullOrWhiteSpace(customerName))
        {
            return null;
        }

        var cliente = await db.IndorProveedorClientes
            .FirstOrDefaultAsync(c => c.ProveedorId == proveedorId && c.Name == customerName, cancellationToken);

        if (cliente == null)
        {
            cliente = new IndorProveedorCliente
            {
                ProveedorId = proveedorId,
                Name = customerName,
                Address = address
            };
            db.IndorProveedorClientes.Add(cliente);
            await db.SaveChangesAsync(cancellationToken);
        }

        return cliente.Id;
    }

    private async Task<List<string>> GetTechnicianOptionsAsync(int proveedorId, CancellationToken cancellationToken)
    {
        var fromJobs = await db.IndorProveedorJobs
            .AsNoTracking()
            .Where(j => j.ProveedorId == proveedorId && j.AssignedTechnician != null && j.AssignedTechnician != "")
            .Select(j => j.AssignedTechnician!)
            .Distinct()
            .ToListAsync(cancellationToken);

        var fromLeads = await db.IndorProveedorLeads
            .AsNoTracking()
            .Where(l => l.ProveedorId == proveedorId && l.DefaultAssignedTechnician != null && l.DefaultAssignedTechnician != "")
            .Select(l => l.DefaultAssignedTechnician!)
            .Distinct()
            .ToListAsync(cancellationToken);

        return fromJobs
            .Concat(fromLeads)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n)
            .ToList();
    }

    private static List<string> BuildTimeOptions()
    {
        var options = new List<string>();
        for (var hour = 6; hour <= 20; hour++)
        {
            options.Add(DateTime.Today.AddHours(hour).ToString("h:mm tt"));
            options.Add(DateTime.Today.AddHours(hour).AddMinutes(30).ToString("h:mm tt"));
        }

        return options;
    }

    private static List<string> BuildReminderOptions() =>
    [
        "15 minutes before",
        "30 minutes before",
        "1 hour before",
        "2 hours before",
        "Morning of"
    ];

    private static string MapPriorityClass(string priority) => priority.ToLowerInvariant() switch
    {
        "low" => "low",
        "high" => "high",
        _ => "medium"
    };

    private static string FormatJobScheduleLabel(DateTime? start, DateTime? end)
    {
        if (!start.HasValue)
        {
            return "Not scheduled";
        }

        var localStart = start.Value.ToLocalTime();
        var dateLabel = localStart.ToString("MMM d, yyyy");
        var timeLabel = localStart.ToString("h:mm tt");

        if (end.HasValue)
        {
            var localEnd = end.Value.ToLocalTime();
            timeLabel = $"{timeLabel} – {localEnd:h:mm tt}";
        }

        return $"{dateLabel}, {timeLabel}";
    }

    private static List<ProviderProFlowStepViewModel> CreateJobCategoryFlowSteps() =>
    [
        new() { Label = "From: Home Dashboard", IconClass = "fa-house", IsLink = true, Url = "/Proveedor/Dashboard" },
        new() { Label = "Pressed: Create Job", IconClass = "fa-plus" },
        new() { Label = "Now: Create New Job", IconClass = "fa-clipboard-list", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> CreateJobDetailsFlowSteps(string categoryLabel) =>
    [
        new() { Label = "From: Create New Job", IconClass = "fa-clipboard-list", IsLink = true, Url = "/Proveedor/CreateJob" },
        new() { Label = $"Pressed: {categoryLabel}", IconClass = "fa-hand-pointer" },
        new() { Label = "Now: Job Details", IconClass = "fa-pen", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> CreateJobScheduleFlowSteps() =>
    [
        new() { Label = "From: Job Details", IconClass = "fa-pen", IsLink = true, Url = "/Proveedor/CreateJobDetails" },
        new() { Label = "Pressed: Next", IconClass = "fa-arrow-right" },
        new() { Label = "Now: Schedule", IconClass = "fa-calendar", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> CreateJobReviewFlowSteps() =>
    [
        new() { Label = "From: Schedule", IconClass = "fa-calendar", IsLink = true, Url = "/Proveedor/CreateJobSchedule" },
        new() { Label = "Pressed: Next", IconClass = "fa-arrow-right" },
        new() { Label = "Now: Review Job", IconClass = "fa-clipboard-check", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> CreateJobSuccessFlowSteps(string jobCode) =>
    [
        new() { Label = "From: Review Job", IconClass = "fa-clipboard-check" },
        new() { Label = "Pressed: Create Job", IconClass = "fa-circle-check" },
        new() { Label = "Now: Job Created", IconClass = "fa-circle-check", IsCurrent = true }
    ];



    private static List<ProviderProFlowStepViewModel> JobDetailsFlowSteps(string title, bool fromCalendar = false, string? dateIso = null) =>
        fromCalendar
            ?
            [
                new() { Label = "From: Day Schedule", IconClass = "fa-list", IsLink = true, Url = $"/Proveedor/DaySchedule?date={dateIso ?? ""}" },
                new() { Label = $"Pressed: {title}", IconClass = "fa-hand-pointer" },
                new() { Label = "Now: Job Details", IconClass = "fa-clipboard-list", IsCurrent = true }
            ]
            :
            [
                new() { Label = "Jobs Schedule", IconClass = "fa-calendar", IsLink = true, Url = "/Proveedor/Calendar" },
                new() { Label = $"Pressed: {title}", IconClass = "fa-hand-pointer" },
                new() { Label = "Now: Job Details", IconClass = "fa-clipboard-list", IsCurrent = true }
            ];



    private static List<ProviderProFlowStepViewModel> ActiveJobFlowSteps(string title) =>

    [

        new() { Label = "Job Details", IconClass = "fa-clipboard-list" },

        new() { Label = "Pressed: Start Job", IconClass = "fa-play" },

        new() { Label = "Now: Active Job", IconClass = "fa-briefcase", IsCurrent = true }

    ];



    private static List<ProviderProFlowStepViewModel> ReportFlowSteps(string title) =>

    [

        new() { Label = "Active Job", IconClass = "fa-briefcase" },

        new() { Label = "Pressed: Complete Job", IconClass = "fa-circle-check" },

        new() { Label = "Now: Job Completion Report", IconClass = "fa-file-lines", IsCurrent = true }

    ];



    private static List<ProviderProJobChecklistItemViewModel> ParseChecklist(string? json)

    {

        if (string.IsNullOrWhiteSpace(json))

        {

            return [];

        }



        try

        {

            var items = JsonSerializer.Deserialize<List<JobChecklistEntity>>(json, JsonOptions) ?? [];

            return items.Select(i => new ProviderProJobChecklistItemViewModel

            {

                Label = i.Label,

                IsCompleted = i.Completed,

                IsInProgress = i.InProgress,

                CompletedLabel = i.CompletedAt.HasValue ? i.CompletedAt.Value.ToLocalTime().ToString("h:mm tt") : null

            }).ToList();

        }

        catch

        {

            return [];

        }

    }



    private static List<ProviderProJobMaterialViewModel> ParseMaterials(string? json)

    {

        if (string.IsNullOrWhiteSpace(json))

        {

            return [];

        }



        try

        {

            return JsonSerializer.Deserialize<List<ProviderProJobMaterialViewModel>>(json, JsonOptions) ?? [];

        }

        catch

        {

            return [];

        }

    }



    private static List<string> ParsePhotoLabels(string? json) =>

        ParseReportPhotos(json).Select(p => p.Label).ToList();



    private static List<ProviderProJobPhotoLabelViewModel> ParseReportPhotos(string? json)

    {

        if (string.IsNullOrWhiteSpace(json))

        {

            return [];

        }



        try

        {

            return JsonSerializer.Deserialize<List<ProviderProJobPhotoLabelViewModel>>(json, JsonOptions) ?? [];

        }

        catch

        {

            return [];

        }

    }



    private static string FormatAppointment(DateTime? when)

    {

        if (!when.HasValue)

        {

            return "Not scheduled";

        }



        var local = when.Value.Kind == DateTimeKind.Utc ? when.Value.ToLocalTime() : when.Value;

        var day = local.Date == DateTime.Today ? "Today" : local.Date == DateTime.Today.AddDays(1) ? "Tomorrow" : local.ToString("MMM d");

        return $"{day}, {local:h:mm tt}";

    }



    private static string NormalizeScheduleView(string? view) => (view ?? "today").ToLowerInvariant() switch

    {

        "week" or "month" => view!.ToLowerInvariant(),

        _ => "today"

    };



    private static string ResolveCompanyName(IndorProveedor proveedor) =>

        !string.IsNullOrWhiteSpace(proveedor.DbaName)

            ? proveedor.DbaName

            : proveedor.BusinessName ?? proveedor.PrimaryContact ?? "Your company";



    private static string MapJobStatusLabel(string status) => status switch

    {

        ProviderJobStatuses.InProgress => "In Progress",

        ProviderJobStatuses.Completed => "Completed",

        ProviderJobStatuses.Confirmed => "Confirmed",

        ProviderJobStatuses.WaitingOnMaterials => "Pending",

        _ => "Scheduled"

    };



    private static string MapJobStatusClass(string status) => status switch

    {

        ProviderJobStatuses.InProgress => "progress",

        ProviderJobStatuses.Completed => "completed",

        ProviderJobStatuses.Confirmed => "confirmed",

        ProviderJobStatuses.WaitingOnMaterials => "approval",

        _ => "scheduled"

    };



    private static string BuildInitials(string name)

    {

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length >= 2 ? $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant() : name[..Math.Min(2, name.Length)].ToUpperInvariant();

    }



    private static DateTime ParseWeekStart(string? weekStart)
    {
        if (!string.IsNullOrWhiteSpace(weekStart) && DateTime.TryParse(weekStart, out var parsed))
        {
            return GetWeekStart(parsed.Date);
        }

        return GetWeekStart(DateTime.Today);
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }

    private static DateTime ParseScheduleDate(string? date) =>
        !string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var parsed)
            ? parsed.Date
            : DateTime.Today;

    private static string NormalizeCalendarView(string? view) => (view ?? "week").ToLowerInvariant() switch
    {
        "day" or "month" => view!.ToLowerInvariant(),
        _ => "week"
    };

    private static string NormalizeCalendarFilter(string? filter) => (filter ?? "all").ToLowerInvariant() switch
    {
        "jobs" or "visits" or "followups" or "blocked" => filter!.ToLowerInvariant(),
        _ => "all"
    };

    private static bool MatchesCalendarFilter(IndorProveedorJob job, string filter) => filter switch
    {
        "jobs" => true,
        "visits" => (job.ServiceType ?? job.Title).Contains("visit", StringComparison.OrdinalIgnoreCase)
            || (job.ServiceType ?? job.Title).Contains("estimate", StringComparison.OrdinalIgnoreCase),
        "followups" => (job.ServiceType ?? job.Title).Contains("follow", StringComparison.OrdinalIgnoreCase),
        "blocked" => false,
        _ => true
    };

    private static string MapCalendarEventTone(string status, string service) =>
        status switch
        {
            ProviderJobStatuses.InProgress => "progress",
            ProviderJobStatuses.Completed => "completed",
            ProviderJobStatuses.Confirmed => "confirmed",
            _ when service.Contains("follow", StringComparison.OrdinalIgnoreCase) => "followup",
            _ when service.Contains("estimate", StringComparison.OrdinalIgnoreCase) => "visit",
            _ => "job"
        };

    private static string MapCalendarServiceIcon(string value)
    {
        var s = value.ToLowerInvariant();
        if (s.Contains("water") || s.Contains("heater")) return "fa-fire-flame-simple";
        if (s.Contains("hvac") || s.Contains("ac ")) return "fa-snowflake";
        if (s.Contains("roof")) return "fa-house-chimney-crack";
        if (s.Contains("plumb")) return "fa-faucet-drip";
        if (s.Contains("follow")) return "fa-phone";
        if (s.Contains("estimate") || s.Contains("visit")) return "fa-file-invoice";
        return "fa-wrench";
    }

    private static int EstimateJobMinutes(IndorProveedorJob job)
    {
        if (job.ScheduledAt.HasValue && job.ScheduledEndAt.HasValue)
        {
            return (int)Math.Max(30, (job.ScheduledEndAt.Value - job.ScheduledAt.Value).TotalMinutes);
        }

        return 90;
    }

    private static string FormatWorkDuration(int minutes)
    {
        var hours = minutes / 60;
        var mins = minutes % 60;
        return mins == 0 ? $"{hours}h" : $"{hours}h {mins}m";
    }

    private static string EstimateDurationLabel(DateTime? start, DateTime? end)
    {
        if (!start.HasValue || !end.HasValue)
        {
            return "2 hours";
        }

        var hours = (end.Value - start.Value).TotalHours;
        return hours switch
        {
            <= 1 => "1 hour",
            <= 1.5 => "1.5 hours",
            <= 2 => "2 hours",
            <= 3 => "3 hours",
            _ => "4 hours"
        };
    }

    private static TimeSpan ParseDurationLabel(string? label) => (label ?? "2 hours").ToLowerInvariant() switch
    {
        "1 hour" => TimeSpan.FromHours(1),
        "1.5 hours" => TimeSpan.FromHours(1.5),
        "3 hours" => TimeSpan.FromHours(3),
        "4 hours" => TimeSpan.FromHours(4),
        _ => TimeSpan.FromHours(2)
    };

    private sealed record JobChecklistEntity(string Label, bool Completed, DateTime? CompletedAt, bool InProgress = false);

}


