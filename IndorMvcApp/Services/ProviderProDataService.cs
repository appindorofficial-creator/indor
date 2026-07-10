using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.Validation;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public partial class ProviderProDataService(
    AppDbContext db,
    IRealtorProviderBridgeService realtorBridge,
    IHttpContextAccessor httpContextAccessor,
    IAddressLookupService addressLookup,
    ILogger<ProviderProDataService> logger) : IProviderProDataService
{
    public async Task<ProviderProWorkspaceData> GetWorkspaceDataAsync(int proveedorId, bool includeLeads, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var prevMonthStart = monthStart.AddMonths(-1);

        var todaysJobRows = await db.IndorProveedorJobs
            .AsNoTracking()
            .Include(j => j.Cliente)
            .Where(j => j.ProveedorId == proveedorId && j.ScheduledAt >= today && j.ScheduledAt < tomorrow)
            .OrderBy(j => j.ScheduledAt)
            .ToListAsync(cancellationToken);

        var todaysJobs = todaysJobRows.Select(j =>
        {
            var (icon, tone) = DeriveHomeJobPresentation(j.Title, j.ServiceType, j.Status);
            return new ProviderProJobItemViewModel
            {
                Id = j.Id,
                TimeLabel = j.ScheduledAt.HasValue
                    ? (j.ScheduledAt.Value.Kind == DateTimeKind.Utc
                        ? j.ScheduledAt.Value.ToLocalTime()
                        : j.ScheduledAt.Value).ToString("h:mm tt")
                    : "TBD",
                Title = j.Title,
                Address = j.Address,
                CustomerName = j.Cliente?.Name ?? "",
                IconClass = icon,
                IconTone = tone,
                Status = MapJobStatusLabel(j.Status),
                StatusClass = MapJobStatusClass(j.Status)
            };
        }).ToList();

        var newLeadsCount = includeLeads
            ? await db.IndorProveedorLeads
                .AsNoTracking()
                .CountAsync(l => l.ProveedorId == proveedorId && l.Status == ProviderLeadStatuses.New, cancellationToken)
            : 0;

        var newLeads = includeLeads
            ? (await db.IndorProveedorLeads
                .AsNoTracking()
                .Where(l => l.ProveedorId == proveedorId && l.Status == ProviderLeadStatuses.New)
                .OrderByDescending(l => l.FechaCreacion)
                .Take(5)
                .ToListAsync(cancellationToken))
                .Select(l => new ProviderProLeadItemViewModel
                {
                    Id = l.Id,
                    Address = l.Address,
                    ServiceType = l.ServiceType,
                    Urgency = l.Urgency,
                    IsHighUrgency = l.Urgency.Contains("High", StringComparison.OrdinalIgnoreCase)
                })
                .ToList()
            : [];

        var pendingEstimatesQuery = db.IndorProveedorEstimates
            .AsNoTracking()
            .Where(e => e.ProveedorId == proveedorId
                && e.Status != ProviderEstimateStatuses.Approved
                && e.Status != ProviderEstimateStatuses.Declined);

        var pendingEstimatesCount = await pendingEstimatesQuery.CountAsync(cancellationToken);

        var pendingEstimates = await pendingEstimatesQuery
            .OrderByDescending(e => e.FechaCreacion)
            .Take(5)
            .Select(e => new ProviderProEstimateItemViewModel
            {
                Id = e.Id,
                EstimateId = e.EstimateCode.StartsWith("#") ? e.EstimateCode : $"#{e.EstimateCode}",
                Amount = e.Amount,
                Address = e.Address,
                ServiceType = e.ServiceType,
                Status = e.Status
            })
            .ToListAsync(cancellationToken);

        var pendingApprovals = await db.IndorProveedorApprovals
            .AsNoTracking()
            .Include(a => a.Job)
            .Where(a => a.ProveedorId == proveedorId && a.Status == "Pending")
            .OrderByDescending(a => a.FechaCreacion)
            .Take(5)
            .Select(a => new ProviderProApprovalItemViewModel
            {
                Id = a.Id,
                Address = a.Address,
                ServiceType = a.Job != null ? a.Job.ServiceType ?? a.Job.Title : null,
                ImageUrl = string.IsNullOrWhiteSpace(a.ImageUrl) ? "/welcome-house.png" : a.ImageUrl
            })
            .ToListAsync(cancellationToken);

        var invoices = await db.IndorProveedorInvoices
            .AsNoTracking()
            .Where(i => i.ProveedorId == proveedorId)
            .ToListAsync(cancellationToken);

        var payments = new ProviderProPaymentsSummaryViewModel
        {
            Paid = invoices.Where(i => i.Status == ProviderInvoiceStatuses.Paid
                && i.PaidDate.HasValue
                && i.PaidDate.Value >= monthStart).Sum(i => i.Amount),
            Pending = invoices.Where(i => i.Status == ProviderInvoiceStatuses.Pending).Sum(i => i.Amount),
            Overdue = invoices.Where(i => i.Status == ProviderInvoiceStatuses.Overdue).Sum(i => i.Amount)
        };

        var calendarDays = Enumerable.Range(0, 3).Select(offset =>
        {
            var day = today.AddDays(offset);
            return new { Day = day, Offset = offset };
        }).ToList();

        var calendarCounts = await db.IndorProveedorJobs
            .AsNoTracking()
            .Where(j => j.ProveedorId == proveedorId
                && j.ScheduledAt >= today
                && j.ScheduledAt < today.AddDays(3))
            .GroupBy(j => j.ScheduledAt!.Value.Date)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var upcomingCalendar = calendarDays.Select(d => new ProviderProCalendarDayViewModel
        {
            DayLabel = ProviderProDisplayLocalization.DayLabel(d.Offset, d.Day),
            DateIso = d.Day.ToString("yyyy-MM-dd"),
            JobCount = calendarCounts.FirstOrDefault(c => c.Day == d.Day)?.Count ?? 0
        }).ToList();

        var completedThisMonth = await db.IndorProveedorJobs
            .AsNoTracking()
            .CountAsync(j => j.ProveedorId == proveedorId
                && j.Status == ProviderJobStatuses.Completed
                && j.FechaActualizacion >= monthStart, cancellationToken);

        var completedPrevMonth = await db.IndorProveedorJobs
            .AsNoTracking()
            .CountAsync(j => j.ProveedorId == proveedorId
                && j.Status == ProviderJobStatuses.Completed
                && j.FechaActualizacion >= prevMonthStart
                && j.FechaActualizacion < monthStart, cancellationToken);

        var homesProtected = await db.IndorProveedorClientes
            .AsNoTracking()
            .CountAsync(c => c.ProveedorId == proveedorId && c.Activo, cancellationToken);

        return new ProviderProWorkspaceData
        {
            TodaysJobs = todaysJobs,
            NewLeadsCount = newLeadsCount,
            NewLeads = newLeads,
            PendingEstimatesCount = pendingEstimatesCount,
            PendingEstimates = pendingEstimates,
            PendingApprovals = pendingApprovals,
            Payments = payments,
            UpcomingCalendar = upcomingCalendar,
            HomeRecordsThisMonth = completedThisMonth,
            HomeRecordsDelta = Math.Max(0, completedThisMonth - completedPrevMonth),
            HomesProtected = homesProtected
        };
    }

    public async Task<ProviderProJobsPageViewModel> GetJobsPageAsync(
        IndorProveedor proveedor,
        string? tab = "active",
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var activeTab = NormalizeJobsTab(tab);

        var jobRows = await db.IndorProveedorJobs
            .AsNoTracking()
            .Include(j => j.Cliente)
            .Where(j => j.ProveedorId == proveedor.Id)
            .ToListAsync(cancellationToken);

        var leadRows = await db.IndorProveedorLeads
            .AsNoTracking()
            .Where(l => l.ProveedorId == proveedor.Id)
            .ToListAsync(cancellationToken);

        var estimateRows = await db.IndorProveedorEstimates
            .AsNoTracking()
            .Where(e => e.ProveedorId == proveedor.Id)
            .ToListAsync(cancellationToken);

        var invoices = await db.IndorProveedorInvoices
            .AsNoTracking()
            .Where(i => i.ProveedorId == proveedor.Id)
            .ToListAsync(cancellationToken);

        var reportRows = await db.IndorProveedorReports
            .AsNoTracking()
            .Where(r => r.ProveedorId == proveedor.Id)
            .ToListAsync(cancellationToken);

        var todayCount = jobRows.Count(j => j.ScheduledAt >= today && j.ScheduledAt < tomorrow);
        var activeCount = jobRows.Count(j => j.Status is ProviderJobStatuses.InProgress
            or ProviderJobStatuses.Scheduled
            or ProviderJobStatuses.Confirmed
            or ProviderJobStatuses.WaitingOnMaterials);
        var newLeadsCount = leadRows.Count(l => l.Status == ProviderLeadStatuses.New);
        var estimatesCount = estimateRows.Count(e => e.Status is ProviderEstimateStatuses.Sent
            or ProviderEstimateStatuses.Viewed
            or ProviderEstimateStatuses.Approved
            or ProviderEstimateStatuses.Draft);
        var completedCount = jobRows.Count(j => j.Status == ProviderJobStatuses.Completed);
        var needsReportCount = reportRows.Count(r =>
            r.Status is ProviderReportStatuses.Draft or ProviderReportStatuses.Approval);
        var paymentsDue = invoices
            .Where(i => i.Status is ProviderInvoiceStatuses.Overdue or ProviderInvoiceStatuses.Pending)
            .Sum(i => i.Amount);

        var allItems = BuildAllTabItems(jobRows, leadRows, estimateRows, today, tomorrow);

        var items = activeTab switch
        {
            "all" => allItems,
            "today" => jobRows
                .Where(j => j.ScheduledAt >= today && j.ScheduledAt < tomorrow)
                .OrderBy(j => j.ScheduledAt)
                .Select(MapJobWorkItem)
                .ToList(),
            "leads" => leadRows
                .Where(l => l.Status == ProviderLeadStatuses.New)
                .OrderByDescending(l => l.FechaCreacion)
                .Select(MapLeadWorkItem)
                .ToList(),
            "estimates" => estimateRows
                .Where(e => e.Status is ProviderEstimateStatuses.Sent
                    or ProviderEstimateStatuses.Viewed
                    or ProviderEstimateStatuses.Approved
                    or ProviderEstimateStatuses.Draft)
                .OrderByDescending(e => e.FechaCreacion)
                .Select(e => MapEstimateWorkItem(e, jobRows))
                .ToList(),
            "completed" => jobRows
                .Where(j => j.Status == ProviderJobStatuses.Completed)
                .OrderByDescending(j => j.FechaActualizacion ?? j.FechaCreacion)
                .Select(MapJobWorkItem)
                .ToList(),
            _ => BuildActiveTabItems(jobRows, leadRows, estimateRows)
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            items = items.Where(i =>
                i.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                || i.Address.Contains(q, StringComparison.OrdinalIgnoreCase)
                || i.CustomerName.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var allCount = allItems.Count;

        return new ProviderProJobsPageViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            ActiveTab = activeTab,
            SearchQuery = search,
            TodayCount = todayCount,
            ActiveCount = activeCount,
            AllCount = allCount,
            NewLeadsCount = newLeadsCount,
            EstimatesCount = estimatesCount,
            CompletedCount = completedCount,
            NeedsReportCount = needsReportCount,
            PaymentsDue = paymentsDue,
            Items = items,
            SmartSuggestions = BuildSmartSuggestions(
                reportRows.Count(r => r.Status == ProviderReportStatuses.Approval),
                reportRows.Count(r => r.Status == ProviderReportStatuses.Draft),
                invoices,
                newLeadsCount)
        };
    }

    private static List<ProviderProJobsWorkItemViewModel> BuildActiveTabItems(
        List<IndorProveedorJob> jobs,
        List<IndorProveedorLead> leads,
        List<IndorProveedorEstimate> estimates)
    {
        var activeJobs = jobs
            .Where(j => j.Status is ProviderJobStatuses.InProgress
                or ProviderJobStatuses.Scheduled
                or ProviderJobStatuses.Confirmed
                or ProviderJobStatuses.WaitingOnMaterials)
            .OrderByDescending(j => j.ScheduledAt ?? j.FechaCreacion)
            .Select(MapJobWorkItem);

        var activeLeads = leads
            .Where(l => l.Status == ProviderLeadStatuses.New)
            .OrderByDescending(l => l.FechaCreacion)
            .Take(3)
            .Select(MapLeadWorkItem);

        var openEstimates = estimates
            .Where(e => e.Status is ProviderEstimateStatuses.Sent or ProviderEstimateStatuses.Viewed or ProviderEstimateStatuses.Approved)
            .OrderByDescending(e => e.FechaCreacion)
            .Take(3)
            .Select(e => MapEstimateWorkItem(e, jobs));

        return activeJobs.Concat(activeLeads).Concat(openEstimates).ToList();
    }

    private static List<ProviderProJobsWorkItemViewModel> BuildAllTabItems(
        List<IndorProveedorJob> jobs,
        List<IndorProveedorLead> leads,
        List<IndorProveedorEstimate> estimates,
        DateTime today,
        DateTime tomorrow)
    {
        var tomorrowEnd = tomorrow;
        var items = new List<ProviderProJobsWorkItemViewModel>();

        items.AddRange(jobs
            .Where(j => j.ScheduledAt >= today && j.ScheduledAt < tomorrowEnd)
            .OrderBy(j => j.ScheduledAt)
            .Select(MapJobWorkItem));

        items.AddRange(leads
            .Where(l => l.Status == ProviderLeadStatuses.New)
            .OrderByDescending(l => l.FechaCreacion)
            .Select(MapLeadWorkItem));

        items.AddRange(jobs
            .Where(j => j.Status is ProviderJobStatuses.InProgress
                or ProviderJobStatuses.Scheduled
                or ProviderJobStatuses.Confirmed
                or ProviderJobStatuses.WaitingOnMaterials)
            .OrderByDescending(j => j.ScheduledAt ?? j.FechaCreacion)
            .Select(MapJobWorkItem));

        items.AddRange(estimates
            .Where(e => e.Status is ProviderEstimateStatuses.Sent
                or ProviderEstimateStatuses.Viewed
                or ProviderEstimateStatuses.Approved
                or ProviderEstimateStatuses.Draft)
            .OrderByDescending(e => e.FechaCreacion)
            .Select(e => MapEstimateWorkItem(e, jobs)));

        items.AddRange(jobs
            .Where(j => j.Status == ProviderJobStatuses.Completed)
            .OrderByDescending(j => j.FechaActualizacion ?? j.FechaCreacion)
            .Select(MapJobWorkItem));

        return items
            .GroupBy(i => $"{i.ItemKind}:{i.ItemId}")
            .Select(g => g.First())
            .ToList();
    }

    private static List<ProviderProSmartSuggestionViewModel> BuildSmartSuggestions(
        int approvalReportCount,
        int draftReportCount,
        List<IndorProveedorInvoice> invoices,
        int newLeadsCount)
    {
        var suggestions = new List<ProviderProSmartSuggestionViewModel>();
        var overdueCount = invoices.Count(i => i.Status == ProviderInvoiceStatuses.Overdue);

        if (approvalReportCount > 0)
        {
            suggestions.Add(new ProviderProSmartSuggestionViewModel
            {
                Text = $"{approvalReportCount} report{(approvalReportCount == 1 ? "" : "s")} need homeowner approval",
                IconClass = "fa-clipboard-check",
                Tone = "blue",
                Url = "/Proveedor/Reports?tab=approval"
            });
        }

        if (draftReportCount > 0)
        {
            suggestions.Add(new ProviderProSmartSuggestionViewModel
            {
                Text = $"{draftReportCount} report{(draftReportCount == 1 ? "" : "s")} ready to upload",
                IconClass = "fa-cloud-arrow-up",
                Tone = "teal",
                Url = "/Proveedor/UploadReport"
            });
        }

        if (overdueCount > 0)
        {
            suggestions.Add(new ProviderProSmartSuggestionViewModel
            {
                Text = $"{overdueCount} invoice{(overdueCount == 1 ? "" : "s")} {(overdueCount == 1 ? "is" : "are")} overdue",
                IconClass = "fa-file-invoice-dollar",
                Tone = "red",
                Url = "/Proveedor/Invoices?tab=overdue"
            });
        }

        if (newLeadsCount > 0)
        {
            suggestions.Add(new ProviderProSmartSuggestionViewModel
            {
                Text = $"{newLeadsCount} lead{(newLeadsCount == 1 ? "" : "s")} match your service area",
                IconClass = "fa-location-dot",
                Tone = "teal",
                Url = "/Proveedor/NewLeads"
            });
        }

        return suggestions;
    }

    private static ProviderProJobsWorkItemViewModel MapJobWorkItem(IndorProveedorJob job)
    {
        var statusLabel = MapJobStatusLabel(job.Status);
        var statusClass = MapJobStatusClass(job.Status);
        var (primary, primaryClass, secondary) = MapJobActions(job.Status);

        var meta = new List<ProviderProJobMetaLineViewModel>();
        if (job.EstimateAmount.HasValue && job.EstimateAmount > 0)
        {
            meta.Add(new ProviderProJobMetaLineViewModel
            {
                Text = $"Estimate ${job.EstimateAmount:N0}",
                IconClass = "fa-file-invoice-dollar",
                Tone = "neutral"
            });
        }

        if (!string.IsNullOrWhiteSpace(job.ChecklistStatus))
        {
            meta.Add(new ProviderProJobMetaLineViewModel
            {
                Text = job.ChecklistStatus!,
                IconClass = "fa-list-check",
                Tone = job.ChecklistStatus.Contains("pending", StringComparison.OrdinalIgnoreCase) ? "warning" : "success"
            });
        }

        if (job.PhotosCount > 0)
        {
            meta.Add(new ProviderProJobMetaLineViewModel
            {
                Text = $"{job.PhotosCount} photo{(job.PhotosCount == 1 ? "" : "s")} uploaded",
                IconClass = "fa-camera",
                Tone = "neutral"
            });
        }

        if (!string.IsNullOrWhiteSpace(job.HouseFactsStatus))
        {
            meta.Add(new ProviderProJobMetaLineViewModel
            {
                Text = job.HouseFactsStatus!,
                IconClass = job.HouseFactsStatus.Contains("Missing", StringComparison.OrdinalIgnoreCase) ? "fa-triangle-exclamation" : "fa-house",
                Tone = job.HouseFactsStatus.Contains("Eligible", StringComparison.OrdinalIgnoreCase) ? "success"
                    : job.HouseFactsStatus.Contains("Missing", StringComparison.OrdinalIgnoreCase) ? "danger" : "warning"
            });
        }

        if (job.ViewedByCustomer)
        {
            meta.Add(new ProviderProJobMetaLineViewModel
            {
                Text = "Viewed by customer",
                IconClass = "fa-eye",
                Tone = "neutral"
            });
        }

        var (homeIcon, homeTone) = DeriveHomeJobPresentation(job.Title, job.ServiceType, job.Status);

        return new ProviderProJobsWorkItemViewModel
        {
            ItemId = job.Id,
            ClienteId = job.ClienteId,
            ItemKind = "Job",
            Title = job.Title,
            Address = job.Address,
            CustomerName = job.Cliente?.Name ?? "",
            TimeLabel = FormatTimeLabel(job.ScheduledAt),
            ScheduleTimeShort = FormatScheduleTimeShort(job.ScheduledAt),
            StatusLabel = statusLabel,
            StatusClass = statusClass,
            IconClass = homeIcon,
            IconTone = homeTone,
            EstimateAmount = job.EstimateAmount,
            MetaLines = meta,
            ShowEstimateLink = true,
            ShowPhotosLink = true,
            ShowChecklistLink = true,
            ShowHouseFactsLink = true,
            PrimaryAction = primary,
            PrimaryActionClass = primaryClass,
            SecondaryAction = secondary
        };
    }

    private static ProviderProNewLeadCardViewModel MapNewLeadCard(IndorProveedorLead lead)
    {
        var (urgencyClass, urgencyIcon) = MapLeadUrgency(lead.Urgency);
        var isHigh = lead.Urgency.Contains("High", StringComparison.OrdinalIgnoreCase);
        var findingCount = ParseInspectionFindings(lead.FindingsJson).Count;

        return new ProviderProNewLeadCardViewModel
        {
            Id = lead.Id,
            Address = lead.Address,
            ServiceType = lead.ServiceType,
            Urgency = lead.Urgency,
            UrgencyClass = urgencyClass,
            UrgencyIcon = urgencyIcon,
            IsHighUrgency = isHigh,
            DistanceLabel = lead.DistanceMiles.HasValue ? $"{lead.DistanceMiles:0.#} miles away" : null,
            StatusLabel = lead.Status == ProviderLeadStatuses.Accepted ? ProviderProDisplayLocalization.L("Accepted") : ProviderProDisplayLocalization.L("New"),
            StatusClass = lead.Status == ProviderLeadStatuses.Accepted ? "accepted" : "new",
            ImageUrl = string.IsNullOrWhiteSpace(lead.ImageUrl) ? "/welcome-house.png" : lead.ImageUrl,
            CanAccept = lead.Status == ProviderLeadStatuses.New,
            CanDecline = lead.Status is ProviderLeadStatuses.New or ProviderLeadStatuses.Accepted,
            ReceivedLabel = FormatRelativeLeadTime(lead.FechaCreacion),
            FindingCount = findingCount,
            FindingSummary = findingCount > 0 ? $"{findingCount} inspection finding{(findingCount == 1 ? "" : "s")} found" : null,
            SourceBadge = string.Equals(lead.LeadSource, "RealtorInspection", StringComparison.OrdinalIgnoreCase)
                ? "Realtor inspection" : null
        };
    }

    private static (string Class, string Icon) MapLeadUrgency(string urgency)
    {
        if (urgency.Contains("High", StringComparison.OrdinalIgnoreCase))
        {
            return ("high", "fa-fire");
        }

        if (urgency.Contains("Same", StringComparison.OrdinalIgnoreCase))
        {
            return ("sameday", "fa-clock");
        }

        if (urgency.Contains("Medium", StringComparison.OrdinalIgnoreCase))
        {
            return ("medium", "fa-clock");
        }

        return ("standard", "fa-clock");
    }

    private static string NormalizeLeadsFilter(string? filter)
    {
        var normalized = (filter ?? "new").Trim().ToLowerInvariant();
        return normalized switch
        {
            "new" or "accepted" or "urgent" or "in_progress" or "responded" or "all" => normalized,
            _ => "new"
        };
    }

    private static ProviderProJobsWorkItemViewModel MapLeadWorkItem(IndorProveedorLead lead)
    {
        var isUrgent = lead.Urgency.Contains("High", StringComparison.OrdinalIgnoreCase);

        return new ProviderProJobsWorkItemViewModel
        {
            ItemId = lead.Id,
            ItemKind = "Lead",
            Title = $"{lead.ServiceType} Lead",
            Address = lead.Address,
            CustomerName = lead.CustomerName ?? "",
            TimeLabel = FormatTimeLabel(lead.FechaCreacion),
            StatusLabel = ProviderProDisplayLocalization.L("Lead"),
            StatusClass = "lead",
            SecondaryBadge = isUrgent ? lead.Urgency : null,
            SecondaryBadgeClass = "urgency",
            IconClass = MapServiceIcon(lead.ServiceType),
            ShowEstimateLink = true,
            ShowPhotosLink = false,
            ShowChecklistLink = false,
            ShowHouseFactsLink = true,
            PrimaryAction = "Accept",
            PrimaryActionClass = "primary",
            SecondaryAction = "Send Estimate",
            LeadId = lead.Id
        };
    }

    public async Task<ProviderProNewLeadsPageViewModel> GetNewLeadsPageAsync(
        IndorProveedor proveedor,
        string? filter = "all",
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var activeFilter = NormalizeLeadsFilter(filter);
        var leadRows = await db.IndorProveedorLeads
            .AsNoTracking()
            .Where(l => l.ProveedorId == proveedor.Id
                && l.Status != ProviderLeadStatuses.Declined)
            .OrderByDescending(l => l.FechaCreacion)
            .ToListAsync(cancellationToken);

        var respondedLeadIds = await db.IndorProveedorEstimates.AsNoTracking()
            .Where(e => e.ProveedorId == proveedor.Id
                && e.LeadId != null
                && (e.Status == ProviderEstimateStatuses.Sent
                    || e.Status == ProviderEstimateStatuses.Viewed
                    || e.Status == ProviderEstimateStatuses.Approved))
            .Select(e => e.LeadId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var newCount = leadRows.Count(l => l.Status == ProviderLeadStatuses.New);
        var acceptedCount = leadRows.Count(l => l.Status == ProviderLeadStatuses.Accepted);
        var inProgressCount = leadRows.Count(l =>
            l.Status == ProviderLeadStatuses.Accepted && !respondedLeadIds.Contains(l.Id));
        var respondedCount = leadRows.Count(l => respondedLeadIds.Contains(l.Id));
        var highUrgencyCount = leadRows.Count(l => l.Urgency.Contains("High", StringComparison.OrdinalIgnoreCase));

        var filtered = activeFilter switch
        {
            "new" => leadRows.Where(l => l.Status == ProviderLeadStatuses.New),
            "accepted" => leadRows.Where(l => l.Status == ProviderLeadStatuses.Accepted),
            "in_progress" => leadRows.Where(l =>
                l.Status == ProviderLeadStatuses.Accepted && !respondedLeadIds.Contains(l.Id)),
            "responded" => leadRows.Where(l => respondedLeadIds.Contains(l.Id)),
            "urgent" => leadRows.Where(l => l.Urgency.Contains("High", StringComparison.OrdinalIgnoreCase)),
            _ => leadRows.AsEnumerable()
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            filtered = filtered.Where(l =>
                l.Address.Contains(q, StringComparison.OrdinalIgnoreCase)
                || l.ServiceType.Contains(q, StringComparison.OrdinalIgnoreCase)
                || (l.CustomerName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        var leads = filtered.Select(MapNewLeadCard).ToList();

        return new ProviderProNewLeadsPageViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            ActiveFilter = activeFilter,
            SearchQuery = search,
            NewCount = newCount,
            InProgressCount = inProgressCount,
            RespondedCount = respondedCount,
            AcceptedCount = acceptedCount,
            HighUrgencyCount = highUrgencyCount,
            Leads = leads,
            FlowSteps =
            [
                new ProviderProFlowStepViewModel
                {
                    Label = "Home Dashboard",
                    IconClass = "fa-house",
                    IsLink = true,
                    Url = "/Proveedor/Dashboard"
                },
                new ProviderProFlowStepViewModel
                {
                    Label = "Pressed: New Leads / See leads",
                    IconClass = "fa-hand-pointer"
                },
                new ProviderProFlowStepViewModel
                {
                    Label = "Now: New Leads",
                    IconClass = "fa-user-group",
                    IsCurrent = true
                }
            ]
        };
    }

    public async Task<ProviderProLeadDetailsViewModel?> GetLeadDetailsAsync(
        IndorProveedor proveedor,
        int leadId,
        CancellationToken cancellationToken = default)
    {
        var lead = await db.IndorProveedorLeads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == leadId && l.ProveedorId == proveedor.Id, cancellationToken);

        if (lead == null)
        {
            return null;
        }

        var leadCode = !string.IsNullOrWhiteSpace(lead.LeadCode)
            ? lead.LeadCode.StartsWith('#') ? lead.LeadCode : $"#{lead.LeadCode}"
            : $"#L-{lead.Id}";

        return new ProviderProLeadDetailsViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            LeadId = lead.Id,
            LeadCode = leadCode,
            Address = lead.Address,
            ServiceType = lead.ServiceType,
            Urgency = lead.Urgency,
            IsHighUrgency = lead.Urgency.Contains("High", StringComparison.OrdinalIgnoreCase),
            DistanceLabel = lead.DistanceMiles.HasValue ? $"{lead.DistanceMiles:0.#} miles away" : null,
            TimelineNote = lead.TimelineNote,
            ImageUrl = string.IsNullOrWhiteSpace(lead.ImageUrl) ? "/welcome-house.png" : lead.ImageUrl,
            CustomerName = lead.CustomerName ?? ProviderProDisplayLocalization.L("Homeowner"),
            CustomerInitials = BuildInitials(lead.CustomerName ?? "Homeowner"),
            IsHomeownerVerified = lead.IsHomeownerVerified,
            CustomerPhone = lead.CustomerPhone,
            CustomerEmail = lead.CustomerEmail,
            ProblemDescription = lead.ProblemDescription,
            PhotoUrls = ParsePhotoUrls(lead.PhotosJson, lead.ImageUrl),
            HomeType = lead.HomeType,
            SquareFeetLabel = lead.SquareFeet.HasValue ? $"{lead.SquareFeet:N0}" : null,
            YearBuiltLabel = lead.YearBuilt?.ToString(),
            StoriesLabel = lead.Stories?.ToString(),
            AccessNotes = lead.AccessNotes,
            CanAccept = lead.Status == ProviderLeadStatuses.New,
            IsAccepted = lead.Status == ProviderLeadStatuses.Accepted,
            CanScheduleVisit = lead.Status is ProviderLeadStatuses.New or ProviderLeadStatuses.Accepted,
            CanCreateEstimate = lead.Status is ProviderLeadStatuses.New or ProviderLeadStatuses.Accepted,
            CanDecline = lead.Status is ProviderLeadStatuses.New or ProviderLeadStatuses.Accepted,
            InspectionReportUrl = lead.InspectionReportUrl,
            SourceBadge = string.Equals(lead.LeadSource, "RealtorInspection", StringComparison.OrdinalIgnoreCase)
                ? "From realtor inspection report" : null,
            AnalysisSummary = lead.AnalysisSummary,
            FindingCount = ParseInspectionFindings(lead.FindingsJson).Count,
            HasInspectionFindings = !string.IsNullOrWhiteSpace(lead.FindingsJson),
            InspectionFindings = ParseInspectionFindings(lead.FindingsJson),
            FlowSteps = LeadDetailsFlowSteps(lead.Id)
        };
    }

    public async Task<ProviderProInspectionFindingsViewModel?> GetInspectionFindingsAsync(
        IndorProveedor proveedor,
        int leadId,
        CancellationToken cancellationToken = default)
    {
        var lead = await LoadLeadForWorkflowAsync(proveedor.Id, leadId, cancellationToken);
        if (lead == null)
        {
            return null;
        }

        var session = httpContextAccessor.HttpContext?.Session;
        var selected = session != null ? ProviderLeadSelectionSession.Get(session, leadId) : [];
        var findings = ParseInspectionFindings(lead.FindingsJson);
        if (selected.Count == 0 && findings.Count > 0)
        {
            selected = findings.Select(f => f.Index).ToList();
        }

        foreach (var f in findings)
        {
            f.IsSelected = selected.Contains(f.Index);
        }

        return new ProviderProInspectionFindingsViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            LeadId = lead.Id,
            LeadCode = FormatLeadCode(lead),
            Address = lead.Address,
            ServiceType = lead.ServiceType,
            InspectionReportUrl = lead.InspectionReportUrl,
            AnalysisSummary = lead.AnalysisSummary ?? BuildDefaultAnalysisSummary(findings.Count, lead.ServiceType),
            Findings = findings,
            SelectedCount = findings.Count(f => f.IsSelected),
            FlowSteps = InspectionFindingsFlowSteps(lead.Id)
        };
    }

    public Task SaveLeadFindingSelectionAsync(int leadId, IReadOnlyList<int> selectedIndices)
    {
        var session = httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("Session is not available.");
        ProviderLeadSelectionSession.Set(session, leadId, selectedIndices);
        return Task.CompletedTask;
    }

    public async Task<ProviderProSelectRepairItemsViewModel?> GetSelectRepairItemsAsync(
        IndorProveedor proveedor,
        int leadId,
        CancellationToken cancellationToken = default)
    {
        var lead = await LoadLeadForWorkflowAsync(proveedor.Id, leadId, cancellationToken);
        if (lead == null)
        {
            return null;
        }

        var session = httpContextAccessor.HttpContext?.Session;
        var selected = session != null ? ProviderLeadSelectionSession.Get(session, leadId) : [];
        var findings = ParseInspectionFindings(lead.FindingsJson)
            .Where(f => selected.Count == 0 || selected.Contains(f.Index))
            .Select(f =>
            {
                f.IsSelected = true;
                return f;
            })
            .ToList();

        if (findings.Count == 0)
        {
            findings = ParseInspectionFindings(lead.FindingsJson);
        }

        return new ProviderProSelectRepairItemsViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            LeadId = lead.Id,
            Address = lead.Address,
            ServiceType = lead.ServiceType,
            SelectedItems = findings,
            FlowSteps = SelectRepairItemsFlowSteps(lead.Id)
        };
    }

    private static List<ProviderInspectionFindingItemViewModel> ParseInspectionFindings(string? findingsJson)
    {
        if (string.IsNullOrWhiteSpace(findingsJson))
        {
            return [];
        }

        try
        {
            using var doc = JsonDocument.Parse(findingsJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var items = new List<ProviderInspectionFindingItemViewModel>();
            var index = 0;
            foreach (var node in doc.RootElement.EnumerateArray())
            {
                var title = node.TryGetProperty("title", out var titleNode)
                    ? titleNode.GetString()?.Trim()
                    : node.TryGetProperty("Title", out var titleNodeAlt)
                        ? titleNodeAlt.GetString()?.Trim()
                        : null;
                if (string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                var section = ReadJsonString(node, "sourceSection", "SourceSection");
                var sectionNumber = ReadJsonString(node, "sourceSectionNumber", "SourceSectionNumber");
                int? page = null;
                if (node.TryGetProperty("sourcePage", out var pageNode) && pageNode.TryGetInt32(out var pageValue) && pageValue > 0)
                {
                    page = pageValue;
                }
                else if (node.TryGetProperty("SourcePage", out var pageNodeAlt) && pageNodeAlt.TryGetInt32(out var pageValueAlt) && pageValueAlt > 0)
                {
                    page = pageValueAlt;
                }

                var reportReference = ReadJsonString(node, "reportReference", "ReportReference")
                    ?? BuildReportReference(sectionNumber, section, page);

                items.Add(new ProviderInspectionFindingItemViewModel
                {
                    Index = index++,
                    Title = title,
                    Description = ReadJsonString(node, "description", "Description"),
                    Priority = ReadJsonString(node, "priority", "Priority") ?? "Moderate",
                    SourceSection = section,
                    SourceSectionNumber = sectionNumber,
                    SourcePage = page,
                    SourceExcerpt = ReadJsonString(node, "sourceExcerpt", "SourceExcerpt"),
                    ReportReference = reportReference
                });
            }

            return items;
        }
        catch
        {
            return [];
        }
    }

    private static string? ReadJsonString(JsonElement node, string camelName, string pascalName)
    {
        if (node.TryGetProperty(camelName, out var camel) && camel.ValueKind == JsonValueKind.String)
        {
            return camel.GetString()?.Trim();
        }

        if (node.TryGetProperty(pascalName, out var pascal) && pascal.ValueKind == JsonValueKind.String)
        {
            return pascal.GetString()?.Trim();
        }

        return null;
    }

    private static string? BuildReportReference(string? sectionNumber, string? section, int? page)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(sectionNumber))
        {
            parts.Add(sectionNumber.Trim());
        }

        if (!string.IsNullOrWhiteSpace(section))
        {
            parts.Add(section.Trim());
        }

        if (page is > 0)
        {
            parts.Add($"Page {page}");
        }

        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }

    public async Task<ProviderProScheduleVisitViewModel?> GetScheduleVisitAsync(
        IndorProveedor proveedor,
        int leadId,
        string? kind = null,
        CancellationToken cancellationToken = default)
    {
        var lead = await db.IndorProveedorLeads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == leadId && l.ProveedorId == proveedor.Id, cancellationToken);

        if (lead == null || lead.Status == ProviderLeadStatuses.Declined)
        {
            return null;
        }

        var isVerification = string.Equals(kind, "verification", StringComparison.OrdinalIgnoreCase);
        var visitDate = lead.DefaultVisitAt?.Date;
        var defaultNotes = lead.DefaultVisitNotes;

        var leadCode = !string.IsNullOrWhiteSpace(lead.LeadCode)
            ? lead.LeadCode.StartsWith('#') ? lead.LeadCode : $"#{lead.LeadCode}"
            : $"#L-{lead.Id}";

        return new ProviderProScheduleVisitViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            LeadId = lead.Id,
            LeadCode = leadCode,
            Address = lead.Address,
            ServiceType = lead.ServiceType,
            Urgency = lead.Urgency,
            IsHighUrgency = lead.Urgency.Contains("High", StringComparison.OrdinalIgnoreCase),
            DistanceLabel = lead.DistanceMiles.HasValue ? $"{lead.DistanceMiles:0.#} miles away" : null,
            TimelineNote = lead.TimelineNote,
            ImageUrl = string.IsNullOrWhiteSpace(lead.ImageUrl) ? "/welcome-house.png" : lead.ImageUrl,
            CustomerName = lead.CustomerName ?? ProviderProDisplayLocalization.L("Homeowner"),
            CustomerInitials = BuildInitials(lead.CustomerName ?? "Homeowner"),
            IsHomeownerVerified = lead.IsHomeownerVerified,
            CustomerPhone = lead.CustomerPhone,
            CustomerEmail = lead.CustomerEmail,
            PageTitle = isVerification ? "Schedule Verification Visit" : "Schedule Visit",
            VisitType = lead.DefaultVisitType ?? "",
            IsVerificationVisit = isVerification,
            InfoBanner = isVerification
                ? "Use a verification visit when photos are not enough to create an accurate estimate."
                : null,
            ScheduleDateLabel = visitDate?.ToString("dddd, MMM d") ?? "",
            VisitDate = visitDate?.ToString("yyyy-MM-dd") ?? "",
            TimeLabel = lead.DefaultVisitTimeLabel ?? "",
            AssignedTechnician = lead.DefaultAssignedTechnician ?? "",
            Priority = lead.Urgency.Contains("High", StringComparison.OrdinalIgnoreCase) ? "High" : "Medium",
            Notes = defaultNotes ?? "",
            FlowSteps = ScheduleVisitFlowSteps(lead.Id, isVerification)
        };
    }

    public async Task<int?> ConfirmScheduleVisitAsync(
        int proveedorId,
        ProviderProScheduleVisitInput input,
        CancellationToken cancellationToken = default)
    {
        var lead = await db.IndorProveedorLeads
            .FirstOrDefaultAsync(l => l.Id == input.LeadId && l.ProveedorId == proveedorId, cancellationToken);

        if (lead == null || lead.Status == ProviderLeadStatuses.Declined)
        {
            return null;
        }

        var scheduledAt = ParseVisitSchedule(input.VisitDate, input.TimeLabel);
        var customerName = lead.CustomerName ?? "Homeowner";

        var cliente = await db.IndorProveedorClientes
            .FirstOrDefaultAsync(c =>
                c.ProveedorId == proveedorId
                && (c.Name == customerName || c.Address == lead.Address), cancellationToken);

        if (cliente == null)
        {
            cliente = new IndorProveedorCliente
            {
                ProveedorId = proveedorId,
                Name = customerName,
                Address = lead.Address,
                Email = lead.CustomerEmail,
                Phone = lead.CustomerPhone,
                IsPropertyVerified = lead.IsHomeownerVerified
            };
            db.IndorProveedorClientes.Add(cliente);
            await db.SaveChangesAsync(cancellationToken);
        }

        var reminderNote = input.NotifyHomeowner
            ? $"Reminder: {input.Reminder}. Homeowner notified."
            : $"Reminder: {input.Reminder}.";

        var calendarNote = input.AddToCalendar ? " Added to provider calendar." : "";

        var linkedEstimate = await db.IndorProveedorEstimates
            .AsNoTracking()
            .Where(e => e.ProveedorId == proveedorId && e.LeadId == lead.Id)
            .OrderByDescending(e => e.FechaCreacion)
            .FirstOrDefaultAsync(cancellationToken);

        var job = new IndorProveedorJob
        {
            ProveedorId = proveedorId,
            ClienteId = cliente.Id,
            LeadId = lead.Id,
            JobCode = $"V-{DateTime.UtcNow:yyMMddHHmm}",
            Title = input.VisitType,
            Address = lead.Address,
            ServiceType = lead.ServiceType,
            Status = ProviderJobStatuses.Scheduled,
            IsDraft = input.SaveAsDraft,
            AssignedTechnician = input.AssignedTechnician,
            Priority = input.Priority,
            JobNotes = $"{input.Notes}. {reminderNote}{calendarNote}",
            ScopeOfWork = lead.ProblemDescription ?? input.Notes,
            AccessInstructions = lead.AccessNotes,
            ImageUrl = lead.ImageUrl,
            DistanceMiles = lead.DistanceMiles,
            ChecklistJson = lead.DefaultChecklistJson,
            PhotoUrlsJson = lead.PhotosJson,
            MaterialsUsedJson = lead.DefaultMaterialsUsedJson,
            LaborWarranty = lead.DefaultLaborWarranty,
            EstimateCode = linkedEstimate?.EstimateCode,
            EstimateAmount = linkedEstimate?.Amount,
            ScheduledAt = input.SaveAsDraft ? null : scheduledAt,
            FechaCreacion = DateTime.UtcNow
        };

        if (!input.SaveAsDraft && lead.Status == ProviderLeadStatuses.New)
        {
            lead.Status = ProviderLeadStatuses.Accepted;
        }

        db.IndorProveedorJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);
        return job.Id;
    }

    public async Task<bool> AcceptLeadAsync(int proveedorId, int leadId, CancellationToken cancellationToken = default)
    {
        var lead = await db.IndorProveedorLeads
            .FirstOrDefaultAsync(l => l.Id == leadId && l.ProveedorId == proveedorId, cancellationToken);

        if (lead == null || lead.Status != ProviderLeadStatuses.New)
        {
            return false;
        }

        lead.Status = ProviderLeadStatuses.Accepted;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeclineLeadAsync(int proveedorId, int leadId, CancellationToken cancellationToken = default)
    {
        var lead = await db.IndorProveedorLeads
            .FirstOrDefaultAsync(l => l.Id == leadId && l.ProveedorId == proveedorId, cancellationToken);

        if (lead == null || lead.Status == ProviderLeadStatuses.Declined)
        {
            return false;
        }

        lead.Status = ProviderLeadStatuses.Declined;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ApproveHomeownerRequestAsync(
        int proveedorId,
        int approvalId,
        CancellationToken cancellationToken = default)
    {
        var approval = await db.IndorProveedorApprovals
            .FirstOrDefaultAsync(a => a.Id == approvalId && a.ProveedorId == proveedorId, cancellationToken);

        if (approval == null || approval.Status != "Pending")
        {
            return false;
        }

        approval.Status = "Approved";
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ProviderProQuickEstimateViewModel?> GetQuickEstimateAsync(
        IndorProveedor proveedor,
        int leadId,
        CancellationToken cancellationToken = default)
    {
        var lead = await LoadLeadForWorkflowAsync(proveedor.Id, leadId, cancellationToken);
        if (lead == null)
        {
            return null;
        }

        var draft = await db.IndorProveedorEstimates
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                e.ProveedorId == proveedor.Id
                && e.LeadId == leadId
                && e.Status == "Draft", cancellationToken);

        var scopeItems = draft != null
            ? ParseScopeItems(draft.ScopeItemsJson)
            : ParseScopeItems(lead.SuggestedScopeItemsJson);

        if (scopeItems.Count == 0)
        {
            scopeItems = BuildScopeItemsFromSelectedFindings(lead);
        }

        var labor = draft?.LaborAmount ?? lead.SuggestedLaborAmount ?? 0m;
        var materials = draft?.MaterialsAmount ?? lead.SuggestedMaterialsAmount ?? 0m;
        var total = scopeItems.Sum(i => i.Amount);
        if (total <= 0 && labor + materials > 0)
        {
            total = labor + materials;
        }

        return BuildQuickEstimateViewModel(
            proveedor,
            lead,
            draft?.Id,
            scopeItems,
            labor,
            materials,
            total,
            draft?.Timeline ?? lead.SuggestedTimeline ?? "",
            draft?.Warranty ?? lead.SuggestedWarranty ?? "",
            draft?.HomeownerNotes ?? lead.SuggestedHomeownerNotes ?? "");
    }

    public async Task<int?> SaveQuickEstimateAsync(
        int proveedorId,
        ProviderProQuickEstimateInput input,
        CancellationToken cancellationToken = default)
    {
        IndorProveedorLead? lead = null;
        if (input.LeadId > 0)
        {
            lead = await db.IndorProveedorLeads
                .FirstOrDefaultAsync(l => l.Id == input.LeadId && l.ProveedorId == proveedorId, cancellationToken);

            if (lead == null || lead.Status == ProviderLeadStatuses.Declined)
            {
                return null;
            }
        }

        var scopeItems = BuildScopeItemsFromInput(input);
        var total = scopeItems.Sum(i => i.Amount);
        if (total <= 0)
        {
            total = input.LaborAmount + input.MaterialsAmount;
        }

        var estimate = input.EstimateId.HasValue
            ? await db.IndorProveedorEstimates
                .FirstOrDefaultAsync(e => e.Id == input.EstimateId && e.ProveedorId == proveedorId, cancellationToken)
            : lead != null
                ? await db.IndorProveedorEstimates
                    .FirstOrDefaultAsync(e =>
                        e.ProveedorId == proveedorId
                        && e.LeadId == input.LeadId
                        && e.Status == ProviderEstimateStatuses.Draft, cancellationToken)
                : null;

        if (estimate == null)
        {
            if (lead == null)
            {
                return null;
            }

            estimate = new IndorProveedorEstimate
            {
                ProveedorId = proveedorId,
                EstimateCode = $"E-{DateTime.UtcNow:yyMMddHHmm}"
            };
            db.IndorProveedorEstimates.Add(estimate);
        }

        estimate.LeadId = lead?.Id;
        estimate.Address = lead?.Address ?? estimate.Address;
        estimate.ServiceType = input.ServiceType;
        estimate.CustomerName = lead?.CustomerName ?? estimate.CustomerName;
        if (lead != null)
        {
            estimate.ImageUrl = lead.ImageUrl;
        }
        estimate.LaborAmount = input.LaborAmount;
        estimate.MaterialsAmount = input.MaterialsAmount;
        estimate.Amount = total;
        estimate.ScopeItemsJson = SerializeScopeItems(scopeItems);
        estimate.Timeline = input.Timeline;
        estimate.Warranty = !string.IsNullOrWhiteSpace(input.Warranty)
            ? input.Warranty
            : lead?.SuggestedWarranty ?? estimate.Warranty;
        estimate.HomeownerNotes = !string.IsNullOrWhiteSpace(input.HomeownerNotes)
            ? input.HomeownerNotes
            : lead?.SuggestedHomeownerNotes ?? estimate.HomeownerNotes;
        estimate.LaborWarranty = !string.IsNullOrWhiteSpace(input.LaborWarranty) ? input.LaborWarranty : estimate.Warranty;
        estimate.PartsWarranty = input.PartsWarranty;
        estimate.EstimatedDuration = !string.IsNullOrWhiteSpace(input.EstimatedDuration) ? input.EstimatedDuration : input.Timeline;
        if (!string.IsNullOrWhiteSpace(input.EstimatedStartDate) && DateTime.TryParse(input.EstimatedStartDate, out var startDate))
        {
            estimate.EstimatedStartDate = startDate.Date;
        }

        ApplyEstimatePricing(estimate, scopeItems, input);
        estimate.FechaActualizacion = DateTime.UtcNow;
        estimate.Status = input.GoToReview || input.GoToSend
            ? ProviderEstimateStatuses.Ready
            : ProviderEstimateStatuses.Draft;

        if (lead?.Status == ProviderLeadStatuses.New)
        {
            lead.Status = ProviderLeadStatuses.Accepted;
        }

        await db.SaveChangesAsync(cancellationToken);
        return estimate.Id;
    }

    public async Task<ProviderProReviewEstimateViewModel?> GetReviewEstimateAsync(
        IndorProveedor proveedor,
        int estimateId,
        CancellationToken cancellationToken = default)
    {
        var estimate = await db.IndorProveedorEstimates
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == estimateId && e.ProveedorId == proveedor.Id, cancellationToken);

        if (estimate == null)
        {
            return null;
        }

        IndorProveedorLead? lead = null;
        if (estimate.LeadId.HasValue)
        {
            lead = await LoadLeadForWorkflowAsync(proveedor.Id, estimate.LeadId.Value, cancellationToken);
        }

        return MapReviewEstimateViewModel(proveedor, estimate, lead);
    }

    public async Task<bool> SendEstimateAsync(
        int proveedorId,
        ProviderProSendEstimateInput input,
        CancellationToken cancellationToken = default)
    {
        var estimate = await db.IndorProveedorEstimates
            .FirstOrDefaultAsync(e => e.Id == input.EstimateId && e.ProveedorId == proveedorId, cancellationToken);

        if (estimate == null)
        {
            return false;
        }

        if (estimate.Status is not ProviderEstimateStatuses.Ready
            and not ProviderEstimateStatuses.Draft
            and not ProviderEstimateStatuses.Sent)
        {
            return false;
        }

        estimate.Status = ProviderEstimateStatuses.Sent;
        estimate.SentUtc = DateTime.UtcNow;
        estimate.FechaActualizacion = DateTime.UtcNow;
        estimate.NotifyHomeowner = input.NotifyHomeowner;
        estimate.SaveCopyToLeads = input.SaveCopyToLeads;
        if (!string.IsNullOrWhiteSpace(input.DeliveryMethod))
        {
            estimate.DeliveryMethod = NormalizeDeliveryMethod(input.DeliveryMethod);
        }

        if (!string.IsNullOrWhiteSpace(input.CustomerMessage))
        {
            estimate.HomeownerNotes = input.CustomerMessage.Trim();
        }

        if (input.SaveAsDraft)
        {
            estimate.Status = ProviderEstimateStatuses.Draft;
            estimate.SentUtc = null;
        }

        await db.SaveChangesAsync(cancellationToken);

        if (!input.SaveAsDraft)
        {
            await realtorBridge.SyncBidFromEstimateAsync(estimate, cancellationToken);
        }

        return true;
    }

    public async Task<ProviderProCreateEstimateSetupViewModel> GetCreateEstimateSetupAsync(
        IndorProveedor proveedor,
        ProviderProCreateEstimateDraft? draft,
        CancellationToken cancellationToken = default)
    {
        draft ??= new ProviderProCreateEstimateDraft();

        var customers = await db.IndorProveedorClientes
            .AsNoTracking()
            .Where(c => c.ProveedorId == proveedor.Id && c.Activo)
            .OrderBy(c => c.Name)
            .Select(c => new ProviderProCreateEstimateOptionViewModel
            {
                Id = c.Id,
                Label = c.Name,
                SubLabel = c.Address
            })
            .ToListAsync(cancellationToken);

        var leads = await db.IndorProveedorLeads
            .AsNoTracking()
            .Where(l => l.ProveedorId == proveedor.Id && l.Status != ProviderLeadStatuses.Declined)
            .OrderByDescending(l => l.FechaCreacion)
            .Select(l => new ProviderProCreateEstimateOptionViewModel
            {
                Id = l.Id,
                Label = $"{l.Address} · {l.ServiceType}",
                SubLabel = l.CustomerName
            })
            .ToListAsync(cancellationToken);

        var jobs = await db.IndorProveedorJobs
            .AsNoTracking()
            .Where(j => j.ProveedorId == proveedor.Id && !j.IsDraft && j.Status != ProviderJobStatuses.Completed)
            .OrderByDescending(j => j.FechaCreacion)
            .Select(j => new ProviderProCreateEstimateOptionViewModel
            {
                Id = j.Id,
                Label = $"{j.Title} · {j.Address}",
                SubLabel = j.JobCode
            })
            .ToListAsync(cancellationToken);

        var categories = await db.IndorProveedorCategoriasCatalogo
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.SortOrder)
            .Select(c => new ProviderProCreateJobCategoryOptionViewModel
            {
                Id = c.Id,
                Label = c.LabelEn,
                Description = c.DescriptionEn ?? "",
                IconClass = c.IconClass.StartsWith("fa-") ? c.IconClass : $"fa-{c.IconClass}"
            })
            .ToListAsync(cancellationToken);

        if (categories.Count == 0)
        {
            categories = OnboardingCatalog.ProviderCategories
                .Select(c => new ProviderProCreateJobCategoryOptionViewModel
                {
                    Id = c.Id,
                    Label = c.Label,
                    IconClass = c.IconClass
                })
                .ToList();
        }

        var addresses = customers
            .Where(c => !string.IsNullOrWhiteSpace(c.SubLabel))
            .Select(c => new ProviderProCreateEstimateOptionViewModel { Id = c.Id, Label = c.SubLabel! })
            .Concat(leads.Select(l => new ProviderProCreateEstimateOptionViewModel { Id = l.Id, Label = l.Label.Split('·')[0].Trim() }))
            .GroupBy(a => a.Label, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        return new ProviderProCreateEstimateSetupViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            EstimateType = draft.EstimateType,
            ClienteId = draft.ClienteId,
            CustomerName = draft.CustomerName,
            Address = draft.Address,
            ServiceCategoryId = draft.ServiceCategoryId,
            LeadId = draft.LeadId,
            JobId = draft.JobId,
            Customers = customers,
            Addresses = addresses,
            Categories = categories,
            Leads = leads,
            Jobs = jobs,
            FlowSteps = CreateEstimateSetupFlowSteps()
        };
    }

    public async Task ApplyCreateEstimateSourcePrefillAsync(
        int proveedorId,
        ProviderProCreateEstimateDraft draft,
        CancellationToken cancellationToken = default)
    {
        if (draft.EstimateType == "lead" && draft.LeadId.HasValue)
        {
            var lead = await db.IndorProveedorLeads
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == draft.LeadId && l.ProveedorId == proveedorId, cancellationToken);
            if (lead != null)
            {
                draft.Address = lead.Address;
                draft.CustomerName = lead.CustomerName ?? draft.CustomerName;
                draft.ServiceCategoryLabel = lead.ServiceType ?? draft.ServiceCategoryLabel;
                draft.Title = lead.ServiceType ?? draft.Title;
                draft.Description = lead.ProblemDescription ?? draft.Description;
            }
        }
        else if (draft.EstimateType == "job" && draft.JobId.HasValue)
        {
            var job = await db.IndorProveedorJobs
                .AsNoTracking()
                .Include(j => j.Cliente)
                .FirstOrDefaultAsync(j => j.Id == draft.JobId && j.ProveedorId == proveedorId, cancellationToken);
            if (job != null)
            {
                draft.Address = job.Address;
                draft.CustomerName = job.Cliente?.Name ?? draft.CustomerName;
                draft.ClienteId = job.ClienteId;
                draft.ServiceCategoryLabel = job.ServiceType ?? draft.ServiceCategoryLabel;
                draft.Title = job.Title;
                draft.Description = job.ScopeOfWork ?? draft.Description;
            }
        }
        else if (draft.ClienteId.HasValue)
        {
            var cliente = await db.IndorProveedorClientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == draft.ClienteId && c.ProveedorId == proveedorId, cancellationToken);
            if (cliente != null)
            {
                draft.CustomerName = cliente.Name;
                if (string.IsNullOrWhiteSpace(draft.Address))
                {
                    draft.Address = cliente.Address ?? draft.Address;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(draft.ServiceCategoryId))
        {
            var cat = await db.IndorProveedorCategoriasCatalogo
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == draft.ServiceCategoryId, cancellationToken);
            draft.ServiceCategoryLabel = cat?.LabelEn
                ?? OnboardingCatalog.ProviderCategories.FirstOrDefault(c => c.Id == draft.ServiceCategoryId)?.Label
                ?? draft.ServiceCategoryLabel;
        }
    }

    public Task<ProviderProCreateEstimateDetailsViewModel?> GetCreateEstimateDetailsAsync(
        IndorProveedor proveedor,
        ProviderProCreateEstimateDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Address) || string.IsNullOrWhiteSpace(draft.CustomerName))
        {
            return Task.FromResult<ProviderProCreateEstimateDetailsViewModel?>(null);
        }

        var start = string.IsNullOrWhiteSpace(draft.EstimatedStartDate)
            ? DateTime.Today.AddDays(7).ToString("yyyy-MM-dd")
            : draft.EstimatedStartDate;
        var end = string.IsNullOrWhiteSpace(draft.EstimatedEndDate)
            ? DateTime.Today.AddDays(13).ToString("yyyy-MM-dd")
            : draft.EstimatedEndDate;

        return Task.FromResult<ProviderProCreateEstimateDetailsViewModel?>(new ProviderProCreateEstimateDetailsViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            Title = string.IsNullOrWhiteSpace(draft.Title) ? draft.ServiceCategoryLabel : draft.Title,
            Description = draft.Description,
            CustomerName = draft.CustomerName,
            Address = draft.Address,
            Priority = draft.Priority,
            EstimatedStartDate = start,
            EstimatedEndDate = end,
            Warranty = string.IsNullOrWhiteSpace(draft.Warranty) ? "1 Year Parts & Labor" : draft.Warranty,
            Notes = draft.Notes,
            WarrantyOptions = BuildWarrantyOptions(),
            FlowSteps = CreateEstimateDetailsFlowSteps()
        });
    }

    public Task<ProviderProCreateEstimatePricingViewModel?> GetCreateEstimatePricingAsync(
        IndorProveedor proveedor,
        ProviderProCreateEstimateDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Title))
        {
            return Task.FromResult<ProviderProCreateEstimatePricingViewModel?>(null);
        }

        var lineItems = draft.LineItems.Count > 0 ? draft.LineItems : BuildDefaultEstimateLineItems();
        var (subtotal, tax, total) = CalculateEstimateTotals(lineItems, draft.TaxRate);

        return Task.FromResult<ProviderProCreateEstimatePricingViewModel?>(new ProviderProCreateEstimatePricingViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            TaxRate = draft.TaxRate,
            SubtotalAmount = subtotal,
            TaxAmount = tax,
            TotalAmount = total,
            LineItems = lineItems,
            FlowSteps = CreateEstimatePricingFlowSteps()
        });
    }

    public Task<ProviderProCreateEstimateReviewViewModel?> GetCreateEstimateReviewAsync(
        IndorProveedor proveedor,
        ProviderProCreateEstimateDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Title) || draft.LineItems.Count == 0)
        {
            return Task.FromResult<ProviderProCreateEstimateReviewViewModel?>(null);
        }

        var (_, _, total) = CalculateEstimateTotals(draft.LineItems, draft.TaxRate);

        return Task.FromResult<ProviderProCreateEstimateReviewViewModel?>(new ProviderProCreateEstimateReviewViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            EstimateId = draft.EstimateId,
            Title = draft.Title,
            CustomerName = draft.CustomerName,
            Address = draft.Address,
            Description = draft.Description,
            TotalAmount = total,
            Warranty = draft.Warranty,
            TimelineLabel = BuildEstimateTimelineLabel(draft),
            DeliveryMethod = draft.DeliveryMethod,
            FlowSteps = CreateEstimateReviewFlowSteps()
        });
    }

    public async Task<int?> SaveCreateEstimateFromDraftAsync(
        int proveedorId,
        ProviderProCreateEstimateDraft draft,
        bool readyForReview,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(draft.Address) || string.IsNullOrWhiteSpace(draft.CustomerName))
        {
            return null;
        }

        IndorProveedorEstimate? estimate = null;
        if (draft.EstimateId.HasValue)
        {
            estimate = await db.IndorProveedorEstimates
                .FirstOrDefaultAsync(e => e.Id == draft.EstimateId && e.ProveedorId == proveedorId, cancellationToken);
        }

        if (estimate == null)
        {
            estimate = new IndorProveedorEstimate
            {
                ProveedorId = proveedorId,
                EstimateCode = $"EST-{DateTime.UtcNow:yyyyMMddHHmm}",
                Address = draft.Address,
                Status = ProviderEstimateStatuses.Draft
            };
            db.IndorProveedorEstimates.Add(estimate);
        }

        estimate.EstimateType = draft.EstimateType;
        estimate.ClienteId = draft.ClienteId;
        estimate.CustomerName = draft.CustomerName;
        estimate.Address = draft.Address;
        estimate.ServiceCategoryId = draft.ServiceCategoryId;
        estimate.ServiceType = draft.ServiceCategoryLabel;
        estimate.LeadId = draft.LeadId;
        estimate.JobId = draft.JobId;
        estimate.Title = draft.Title;
        estimate.Description = draft.Description;
        estimate.Priority = draft.Priority;
        estimate.Warranty = draft.Warranty;
        estimate.HomeownerNotes = draft.Notes;
        estimate.DeliveryMethod = draft.DeliveryMethod;

        if (DateTime.TryParse(draft.EstimatedStartDate, out var startDate))
        {
            estimate.EstimatedStartDate = startDate.Date;
        }

        if (DateTime.TryParse(draft.EstimatedEndDate, out var endDate))
        {
            estimate.EstimatedEndDate = endDate.Date;
        }

        estimate.EstimatedDuration = draft.EstimatedDuration;
        estimate.Timeline = BuildEstimateTimelineLabel(draft);

        var lineItems = draft.LineItems.Count > 0 ? draft.LineItems : BuildDefaultEstimateLineItems();
        ApplyEstimatePricingFromLineItems(estimate, lineItems, draft.TaxRate);
        estimate.ScopeItemsJson = SerializePricingLineItems(lineItems);
        estimate.FechaActualizacion = DateTime.UtcNow;
        estimate.Status = readyForReview ? ProviderEstimateStatuses.Ready : ProviderEstimateStatuses.Draft;

        await db.SaveChangesAsync(cancellationToken);
        draft.EstimateId = estimate.Id;
        return estimate.Id;
    }

    public async Task<ProviderProPendingEstimatesPageViewModel> GetPendingEstimatesPageAsync(
        IndorProveedor proveedor,
        string? tab = "all",
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var activeTab = NormalizeEstimateTab(tab);
        var rows = await db.IndorProveedorEstimates
            .AsNoTracking()
            .Where(e => e.ProveedorId == proveedor.Id
                && e.Status != ProviderEstimateStatuses.Approved
                && e.Status != ProviderEstimateStatuses.Declined)
            .OrderByDescending(e => e.FechaActualizacion ?? e.FechaCreacion)
            .ToListAsync(cancellationToken);

        var draftCount = rows.Count(e => e.Status == ProviderEstimateStatuses.Draft);
        var readyCount = rows.Count(e => e.Status == ProviderEstimateStatuses.Ready);
        var sentCount = rows.Count(e => e.Status is ProviderEstimateStatuses.Sent or ProviderEstimateStatuses.Viewed);
        var aiDraftCount = rows.Count(e => MapEstimateHubStatus(e).FilterKey == "aidraft");
        var needsReviewCount = rows.Count(e => MapEstimateHubStatus(e).FilterKey == "needsreview");
        var pendingCount = draftCount + readyCount;

        var filtered = activeTab switch
        {
            "needsreview" => rows.Where(e => MapEstimateHubStatus(e).FilterKey == "needsreview"),
            "aidraft" => rows.Where(e => MapEstimateHubStatus(e).FilterKey == "aidraft"),
            "ready" => rows.Where(e => e.Status == ProviderEstimateStatuses.Ready),
            "sent" => rows.Where(e => e.Status is ProviderEstimateStatuses.Sent or ProviderEstimateStatuses.Viewed),
            "draft" => rows.Where(e => e.Status == ProviderEstimateStatuses.Draft),
            _ => rows.Where(e => e.Status is ProviderEstimateStatuses.Draft
                or ProviderEstimateStatuses.Ready
                or ProviderEstimateStatuses.Sent
                or ProviderEstimateStatuses.Viewed)
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            filtered = filtered.Where(e =>
                e.EstimateCode.Contains(q, StringComparison.OrdinalIgnoreCase)
                || e.Address.Contains(q, StringComparison.OrdinalIgnoreCase)
                || (e.ServiceType ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)
                || (e.CustomerName ?? "").Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var cards = filtered.Select(MapPendingEstimateCard).ToList();

        return new ProviderProPendingEstimatesPageViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            ActiveTab = activeTab,
            SearchQuery = search,
            DraftCount = draftCount,
            ReadyCount = readyCount,
            SentCount = sentCount,
            PendingCount = pendingCount,
            AiDraftCount = aiDraftCount,
            NeedsReviewCount = needsReviewCount,
            Estimates = cards,
            FlowSteps = PendingEstimatesFlowSteps(),
            WizardSteps = BuildEstimateWizardSteps(2)
        };
    }

    public async Task<ProviderProSendEstimatePageViewModel?> GetSendEstimatePageAsync(
        IndorProveedor proveedor,
        int estimateId,
        CancellationToken cancellationToken = default)
    {
        var estimate = await db.IndorProveedorEstimates
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == estimateId && e.ProveedorId == proveedor.Id, cancellationToken);

        if (estimate == null || estimate.Status is ProviderEstimateStatuses.Declined or ProviderEstimateStatuses.Approved)
        {
            return null;
        }

        IndorProveedorLead? lead = null;
        if (estimate.LeadId.HasValue)
        {
            lead = await LoadLeadForWorkflowAsync(proveedor.Id, estimate.LeadId.Value, cancellationToken);
        }

        var customerName = lead?.CustomerName ?? estimate.CustomerName ?? "Homeowner";
        var serviceType = estimate.ServiceType ?? lead?.ServiceType ?? "Service";
        var (icon, tone) = ResolveEstimateServiceVisuals(serviceType, estimate.ServiceCategoryId);
        var (statusLabel, statusClass, _) = MapEstimateHubStatus(estimate);
        var photos = ParsePhotoUrls(lead?.PhotosJson, estimate.ImageUrl ?? lead?.ImageUrl);
        var delivery = NormalizeDeliveryMethodKey(estimate.DeliveryMethod);
        var firstName = customerName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? customerName;
        var defaultMessage = string.IsNullOrWhiteSpace(estimate.HomeownerNotes)
            ? $"Hi {firstName}, thanks again for having us out! Attached is your estimate for the {serviceType.ToLowerInvariant()}. Let me know if you have any questions—I'm here to help!"
            : estimate.HomeownerNotes;

        return new ProviderProSendEstimatePageViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            EstimateId = estimate.Id,
            Title = ResolveEstimateTitle(estimate),
            CustomerName = customerName,
            Address = lead?.Address ?? estimate.Address,
            DateLabel = estimate.FechaCreacion.ToLocalTime().ToString("MMM d, yyyy"),
            ServiceType = serviceType,
            ServiceIcon = icon,
            ServiceTone = tone,
            StatusLabel = statusLabel,
            StatusClass = statusClass,
            TotalAmount = estimate.Amount,
            CustomerMessage = defaultMessage,
            DeliveryMethod = delivery,
            HasEstimatePdf = true,
            HasAiSummary = !string.IsNullOrWhiteSpace(estimate.Description) || !string.IsNullOrWhiteSpace(estimate.ScopeItemsJson),
            HasVoiceTranscript = !string.IsNullOrWhiteSpace(lead?.ProblemDescription),
            WizardSteps = BuildEstimateWizardSteps(5)
        };
    }

    public async Task<ProviderProQuickEstimateViewModel?> GetEditEstimateAsync(
        IndorProveedor proveedor,
        int estimateId,
        CancellationToken cancellationToken = default)
    {
        var estimate = await db.IndorProveedorEstimates
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == estimateId && e.ProveedorId == proveedor.Id, cancellationToken);

        if (estimate == null || estimate.Status is ProviderEstimateStatuses.Declined or ProviderEstimateStatuses.Approved)
        {
            return null;
        }

        IndorProveedorLead? lead = null;
        if (estimate.LeadId.HasValue)
        {
            lead = await LoadLeadForWorkflowAsync(proveedor.Id, estimate.LeadId.Value, cancellationToken);
        }

        var scopeItems = ParseScopeItems(estimate.ScopeItemsJson);
        if (scopeItems.Count == 0 && lead != null)
        {
            scopeItems = BuildScopeItemsFromFindings(lead.FindingsJson);
        }

        var code = FormatEstimateCode(estimate.EstimateCode);
        var (statusLabel, statusClass, _) = MapEstimateHubStatus(estimate);
        var (icon, tone) = ResolveEstimateServiceVisuals(estimate.ServiceType ?? lead?.ServiceType, estimate.ServiceCategoryId);
        var photos = ParsePhotoUrls(lead?.PhotosJson, estimate.ImageUrl ?? lead?.ImageUrl);

        return new ProviderProQuickEstimateViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            LeadId = lead?.Id ?? estimate.LeadId ?? 0,
            EstimateId = estimate.Id,
            PageTitle = "Edit Estimate",
            EstimateCode = code,
            StatusLabel = statusLabel,
            StatusClass = statusClass,
            Title = ResolveEstimateTitle(estimate),
            DateLabel = estimate.FechaCreacion.ToLocalTime().ToString("MMM d, yyyy"),
            ServiceIcon = icon,
            ServiceTone = tone,
            PhotoCount = photos.Count,
            VoiceTranscriptCount = string.IsNullOrWhiteSpace(lead?.ProblemDescription) ? 0 : 1,
            CreatedLabel = $"Created: {estimate.FechaCreacion.ToLocalTime():MMM d, yyyy}",
            PropertyMeta = BuildPropertyMeta(lead),
            Address = lead?.Address ?? estimate.Address,
            ServiceType = estimate.ServiceType ?? lead?.ServiceType ?? "",
            Urgency = lead?.Urgency ?? "",
            IsHighUrgency = lead?.Urgency.Contains("High", StringComparison.OrdinalIgnoreCase) ?? false,
            DistanceLabel = lead?.DistanceMiles.HasValue == true ? $"{lead.DistanceMiles:0.#} miles away" : null,
            ImageUrl = ResolveEstimateImage(estimate, lead),
            CustomerName = lead?.CustomerName ?? estimate.CustomerName ?? "Homeowner",
            CustomerInitials = BuildInitials(lead?.CustomerName ?? estimate.CustomerName ?? "Homeowner"),
            IsHomeownerVerified = lead?.IsHomeownerVerified ?? false,
            CustomerPhone = lead?.CustomerPhone,
            CustomerEmail = lead?.CustomerEmail,
            ScopeItems = scopeItems,
            LaborAmount = estimate.LaborAmount,
            MaterialsAmount = estimate.MaterialsAmount,
            SubtotalAmount = estimate.SubtotalAmount ?? scopeItems.Sum(i => i.Amount),
            TaxRate = estimate.TaxRate ?? 0.0825m,
            TaxAmount = estimate.TaxAmount ?? 0m,
            DiscountAmount = estimate.DiscountAmount ?? 0m,
            TotalAmount = estimate.Amount,
            Timeline = estimate.Timeline ?? "",
            EstimatedStartDate = estimate.EstimatedStartDate?.ToString("yyyy-MM-dd") ?? "",
            EstimatedDuration = estimate.EstimatedDuration ?? estimate.Timeline ?? "",
            Warranty = estimate.Warranty ?? "",
            LaborWarranty = estimate.LaborWarranty ?? estimate.Warranty ?? "",
            PartsWarranty = estimate.PartsWarranty ?? "",
            HomeownerNotes = estimate.HomeownerNotes ?? "",
            BackUrl = lead != null ? $"/Proveedor/LeadDetails/{lead.Id}" : "/Proveedor/PendingEstimates",
            FlowSteps = EditEstimateFlowSteps(estimate.Id),
            WizardSteps = BuildEstimateWizardSteps(4),
            LeadCode = lead != null
                ? (!string.IsNullOrWhiteSpace(lead.LeadCode)
                    ? lead.LeadCode.StartsWith('#') ? lead.LeadCode : $"#{lead.LeadCode}"
                    : $"#L-{lead.Id}")
                : "",
            ProblemDescription = lead?.ProblemDescription,
            InspectionReportUrl = lead?.InspectionReportUrl,
            SourceBadge = lead != null && string.Equals(lead.LeadSource, "RealtorInspection", StringComparison.OrdinalIgnoreCase)
                ? "From realtor inspection report" : null,
            InspectionFindings = lead != null ? ParseInspectionFindings(lead.FindingsJson) : [],
            PhotoUrls = photos
        };
    }

    public async Task<ProviderProEstimateSentViewModel?> GetEstimateSentAsync(
        IndorProveedor proveedor,
        int estimateId,
        CancellationToken cancellationToken = default)
    {
        var estimate = await db.IndorProveedorEstimates
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == estimateId && e.ProveedorId == proveedor.Id, cancellationToken);

        if (estimate == null || estimate.Status is not ProviderEstimateStatuses.Sent
            and not ProviderEstimateStatuses.Viewed
            and not ProviderEstimateStatuses.Approved)
        {
            return null;
        }

        IndorProveedorLead? lead = null;
        if (estimate.LeadId.HasValue)
        {
            lead = await LoadLeadForWorkflowAsync(proveedor.Id, estimate.LeadId.Value, cancellationToken);
        }

        var linkedJob = await db.IndorProveedorJobs
            .AsNoTracking()
            .Where(j => j.ProveedorId == proveedor.Id && j.EstimateCode == estimate.EstimateCode)
            .Select(j => (int?)j.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return new ProviderProEstimateSentViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            EstimateId = estimate.Id,
            LeadId = estimate.LeadId,
            EstimateCode = FormatEstimateCode(estimate.EstimateCode),
            Address = estimate.Address,
            ServiceType = estimate.ServiceType ?? "",
            TotalAmount = estimate.Amount,
            ImageUrl = ResolveEstimateImage(estimate, lead),
            StatusLabel = estimate.Status == ProviderEstimateStatuses.Approved
                ? "Approved"
                : "Waiting for approval",
            IsApproved = estimate.Status == ProviderEstimateStatuses.Approved,
            CanConvertToJob = estimate.Status == ProviderEstimateStatuses.Approved && linkedJob == null,
            ConvertedJobId = linkedJob,
            TrackingSteps = BuildEstimateTrackingSteps(estimate),
            FlowSteps = EstimateSentFlowSteps(estimate.Id)
        };
    }

    public async Task<int?> ConvertEstimateToJobAsync(
        int proveedorId,
        int estimateId,
        CancellationToken cancellationToken = default)
    {
        var estimate = await db.IndorProveedorEstimates
            .Include(e => e.Lead)
            .FirstOrDefaultAsync(e => e.Id == estimateId && e.ProveedorId == proveedorId, cancellationToken);

        if (estimate == null || estimate.Status != ProviderEstimateStatuses.Approved)
        {
            return null;
        }

        var existing = await db.IndorProveedorJobs
            .FirstOrDefaultAsync(j => j.ProveedorId == proveedorId && j.EstimateCode == estimate.EstimateCode, cancellationToken);

        if (existing != null)
        {
            return existing.Id;
        }

        var customerName = estimate.CustomerName ?? estimate.Lead?.CustomerName ?? "Homeowner";
        var cliente = await db.IndorProveedorClientes
            .FirstOrDefaultAsync(c =>
                c.ProveedorId == proveedorId
                && (c.Name == customerName || c.Address == estimate.Address), cancellationToken);

        if (cliente == null)
        {
            cliente = new IndorProveedorCliente
            {
                ProveedorId = proveedorId,
                Name = customerName,
                Address = estimate.Address,
                Email = estimate.Lead?.CustomerEmail,
                Phone = estimate.Lead?.CustomerPhone,
                IsPropertyVerified = estimate.Lead?.IsHomeownerVerified ?? false
            };
            db.IndorProveedorClientes.Add(cliente);
            await db.SaveChangesAsync(cancellationToken);
        }

        var job = new IndorProveedorJob
        {
            ProveedorId = proveedorId,
            ClienteId = cliente.Id,
            LeadId = estimate.LeadId,
            JobCode = $"J-{DateTime.UtcNow:yyMMddHHmm}",
            Title = estimate.ServiceType ?? "Approved Job",
            Address = estimate.Address,
            ServiceType = estimate.ServiceType,
            Status = ProviderJobStatuses.Scheduled,
            ScopeOfWork = estimate.HomeownerNotes ?? string.Join(", ", ParseScopeItems(estimate.ScopeItemsJson).Select(i => i.Label)),
            EstimateCode = estimate.EstimateCode,
            EstimateAmount = estimate.Amount,
            ImageUrl = estimate.ImageUrl ?? estimate.Lead?.ImageUrl,
            MaterialsUsedJson = estimate.Lead?.DefaultMaterialsUsedJson,
            ChecklistJson = estimate.Lead?.DefaultChecklistJson,
            LaborWarranty = estimate.LaborWarranty ?? estimate.Warranty,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorProveedorJobs.Add(job);
        estimate.JobId = job.Id;
        await db.SaveChangesAsync(cancellationToken);
        return job.Id;
    }

    private async Task<IndorProveedorLead?> LoadLeadForWorkflowAsync(
        int proveedorId,
        int leadId,
        CancellationToken cancellationToken) =>
        await db.IndorProveedorLeads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == leadId && l.ProveedorId == proveedorId && l.Status != ProviderLeadStatuses.Declined, cancellationToken);

    private ProviderProQuickEstimateViewModel BuildQuickEstimateViewModel(
        IndorProveedor proveedor,
        IndorProveedorLead lead,
        int? estimateId,
        List<ProviderProEstimateLineItemViewModel> scopeItems,
        decimal labor,
        decimal materials,
        decimal total,
        string timeline,
        string warranty,
        string homeownerNotes) =>
        new()
        {
            CompanyName = ResolveCompanyName(proveedor),
            LeadId = lead.Id,
            EstimateId = estimateId,
            PageTitle = "Quick Estimate",
            PropertyMeta = BuildPropertyMeta(lead),
            Address = lead.Address,
            ServiceType = lead.ServiceType,
            Urgency = lead.Urgency,
            IsHighUrgency = lead.Urgency.Contains("High", StringComparison.OrdinalIgnoreCase),
            DistanceLabel = lead.DistanceMiles.HasValue ? $"{lead.DistanceMiles:0.#} miles away" : null,
            ImageUrl = string.IsNullOrWhiteSpace(lead.ImageUrl) ? "/welcome-house.png" : lead.ImageUrl,
            CustomerName = lead.CustomerName ?? ProviderProDisplayLocalization.L("Homeowner"),
            CustomerInitials = BuildInitials(lead.CustomerName ?? "Homeowner"),
            IsHomeownerVerified = lead.IsHomeownerVerified,
            CustomerPhone = lead.CustomerPhone,
            CustomerEmail = lead.CustomerEmail,
            ScopeItems = scopeItems,
            LaborAmount = labor,
            MaterialsAmount = materials,
            SubtotalAmount = scopeItems.Sum(i => i.Amount) > 0 ? scopeItems.Sum(i => i.Amount) : labor + materials,
            TaxRate = 0.0825m,
            TotalAmount = total,
            Timeline = timeline,
            EstimatedDuration = timeline,
            Warranty = warranty,
            LaborWarranty = warranty,
            HomeownerNotes = homeownerNotes,
            BackUrl = $"/Proveedor/LeadDetails/{lead.Id}",
            FlowSteps = QuickEstimateFlowSteps(lead.Id),
            LeadCode = !string.IsNullOrWhiteSpace(lead.LeadCode)
                ? lead.LeadCode.StartsWith('#') ? lead.LeadCode : $"#{lead.LeadCode}"
                : $"#L-{lead.Id}",
            ProblemDescription = lead.ProblemDescription,
            InspectionReportUrl = lead.InspectionReportUrl,
            SourceBadge = string.Equals(lead.LeadSource, "RealtorInspection", StringComparison.OrdinalIgnoreCase)
                ? "From realtor inspection report" : null,
            InspectionFindings = ParseInspectionFindings(lead.FindingsJson),
            PhotoUrls = ParsePhotoUrls(lead.PhotosJson, lead.ImageUrl)
        };

    private static void ApplyEstimatePricing(
        IndorProveedorEstimate estimate,
        List<ProviderProEstimateLineItemViewModel> scopeItems,
        ProviderProQuickEstimateInput input)
    {
        var subtotal = scopeItems.Sum(i => i.Amount);
        if (subtotal <= 0)
        {
            subtotal = input.LaborAmount + input.MaterialsAmount;
        }

        var taxRate = input.TaxRate > 0 ? input.TaxRate : estimate.TaxRate ?? 0.0825m;
        var discount = input.DiscountAmount > 0 ? input.DiscountAmount : estimate.DiscountAmount ?? 0m;
        var taxable = Math.Max(0, subtotal - discount);
        var tax = Math.Round(taxable * taxRate, 2, MidpointRounding.AwayFromZero);

        estimate.SubtotalAmount = subtotal;
        estimate.TaxRate = taxRate;
        estimate.TaxAmount = tax;
        estimate.DiscountAmount = discount;
        estimate.Amount = taxable + tax;
        estimate.LaborAmount = input.LaborAmount;
        estimate.MaterialsAmount = input.MaterialsAmount;
    }

    private ProviderProReviewEstimateViewModel MapReviewEstimateViewModel(
        IndorProveedor proveedor,
        IndorProveedorEstimate estimate,
        IndorProveedorLead? lead)
    {
        var scopeItems = ParseScopeItems(estimate.ScopeItemsJson);
        var subtotal = estimate.SubtotalAmount ?? scopeItems.Sum(i => i.Amount);
        if (subtotal <= 0)
        {
            subtotal = estimate.LaborAmount + estimate.MaterialsAmount;
        }

        var serviceType = estimate.ServiceType ?? lead?.ServiceType ?? "";
        var (statusLabel, statusClass, _) = MapEstimateHubStatus(estimate);
        var (icon, tone) = ResolveEstimateServiceVisuals(serviceType, estimate.ServiceCategoryId);

        return new ProviderProReviewEstimateViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            LeadId = lead?.Id ?? estimate.LeadId ?? 0,
            EstimateId = estimate.Id,
            Title = ResolveEstimateTitle(estimate),
            StatusLabel = statusLabel,
            StatusClass = statusClass,
            DateLabel = estimate.FechaCreacion.ToLocalTime().ToString("MMM d, yyyy"),
            ServiceIcon = icon,
            ServiceTone = tone,
            ScopeSummaryLines = BuildScopeSummaryLines(estimate, scopeItems),
            AiRecommendations = BuildAiRecommendations(estimate, serviceType),
            EstimateCode = FormatEstimateCode(estimate.EstimateCode),
            CreatedLabel = $"Created: {estimate.FechaCreacion.ToLocalTime():MMM d, yyyy}",
            ValidForLabel = $"Valid for {estimate.ValidDays} days",
            PropertyMeta = BuildPropertyMeta(lead),
            Address = lead?.Address ?? estimate.Address,
            ServiceType = estimate.ServiceType ?? lead?.ServiceType ?? "",
            Urgency = lead?.Urgency ?? "",
            IsHighUrgency = lead?.Urgency.Contains("High", StringComparison.OrdinalIgnoreCase) ?? false,
            DistanceLabel = lead?.DistanceMiles.HasValue == true ? $"{lead.DistanceMiles:0.#} miles away" : null,
            ImageUrl = ResolveEstimateImage(estimate, lead),
            CustomerName = lead?.CustomerName ?? estimate.CustomerName ?? "Homeowner",
            CustomerInitials = BuildInitials(lead?.CustomerName ?? estimate.CustomerName ?? "Homeowner"),
            IsHomeownerVerified = lead?.IsHomeownerVerified ?? false,
            CustomerPhone = lead?.CustomerPhone,
            CustomerEmail = lead?.CustomerEmail,
            ScopeItems = scopeItems,
            LaborAmount = estimate.LaborAmount,
            MaterialsAmount = estimate.MaterialsAmount,
            SubtotalAmount = subtotal,
            TaxRate = estimate.TaxRate ?? 0.0825m,
            TaxAmount = estimate.TaxAmount ?? Math.Round(subtotal * (estimate.TaxRate ?? 0.0825m), 2),
            DiscountAmount = estimate.DiscountAmount ?? 0m,
            TotalAmount = estimate.Amount,
            Timeline = estimate.Timeline ?? estimate.EstimatedDuration ?? "",
            EstimatedStartLabel = estimate.EstimatedStartDate?.ToString("MMM d, yyyy"),
            EstimatedDuration = estimate.EstimatedDuration ?? estimate.Timeline,
            Warranty = estimate.Warranty ?? "",
            LaborWarranty = estimate.LaborWarranty ?? estimate.Warranty ?? "",
            PartsWarranty = estimate.PartsWarranty ?? "",
            HomeownerNotes = estimate.HomeownerNotes ?? "",
            NotifyHomeowner = estimate.NotifyHomeowner,
            SaveCopyToLeads = estimate.SaveCopyToLeads,
            CanSend = estimate.Status is ProviderEstimateStatuses.Draft or ProviderEstimateStatuses.Ready,
            FlowSteps = ReviewEstimateFlowSteps(estimate.Id),
            WizardSteps = BuildEstimateWizardSteps(3)
        };
    }

    private static ProviderProPendingEstimateCardViewModel MapPendingEstimateCard(IndorProveedorEstimate estimate)
    {
        var updated = estimate.FechaActualizacion ?? estimate.FechaCreacion;
        var isSent = estimate.Status is ProviderEstimateStatuses.Sent or ProviderEstimateStatuses.Viewed;
        var (statusLabel, statusClass, filterKey) = MapEstimateHubStatus(estimate);
        var (icon, tone) = ResolveEstimateServiceVisuals(estimate.ServiceType, estimate.ServiceCategoryId);

        return new ProviderProPendingEstimateCardViewModel
        {
            Id = estimate.Id,
            EstimateCode = FormatEstimateCode(estimate.EstimateCode),
            Title = ResolveEstimateTitle(estimate),
            CustomerName = estimate.CustomerName ?? "Homeowner",
            Address = estimate.Address,
            ServiceType = estimate.ServiceType,
            Status = statusLabel,
            StatusClass = statusClass,
            FilterKey = filterKey,
            ServiceIcon = icon,
            ServiceTone = tone,
            DateLabel = isSent && estimate.SentUtc.HasValue
                ? estimate.SentUtc.Value.ToLocalTime().ToString("MMM d, yyyy")
                : updated.ToLocalTime().ToString("MMM d, yyyy"),
            Amount = estimate.Amount,
            CanEdit = !isSent,
            CanReview = !isSent,
            CanSend = estimate.Status == ProviderEstimateStatuses.Ready,
            CanView = isSent
        };
    }

    private static List<ProviderProEstimateTrackingStepViewModel> BuildEstimateTrackingSteps(IndorProveedorEstimate estimate)
    {
        var steps = new List<ProviderProEstimateTrackingStepViewModel>
        {
            new()
            {
                Label = "Sent",
                Detail = estimate.SentUtc.HasValue
                    ? $"Sent {estimate.SentUtc.Value.ToLocalTime():MMM d, h:mm tt}"
                    : null,
                IconClass = "fa-paper-plane",
                StateClass = estimate.SentUtc.HasValue ? "done" : "pending"
            },
            new()
            {
                Label = "Viewed",
                Detail = estimate.ViewedUtc.HasValue
                    ? $"Viewed {estimate.ViewedUtc.Value.ToLocalTime():MMM d, h:mm tt}"
                    : "Pending",
                IconClass = "fa-eye",
                StateClass = estimate.ViewedUtc.HasValue ? "done" : "pending"
            },
            new()
            {
                Label = "Approved",
                Detail = estimate.ApprovedUtc.HasValue
                    ? $"Approved {estimate.ApprovedUtc.Value.ToLocalTime():MMM d, h:mm tt}"
                    : "Pending",
                IconClass = "fa-thumbs-up",
                StateClass = estimate.ApprovedUtc.HasValue ? "done" : "pending"
            }
        };

        return steps;
    }

    private static string FormatEstimateCode(string code) =>
        code.StartsWith('#') ? code : $"#{code}";

    private static string? BuildPropertyMeta(IndorProveedorLead? lead)
    {
        if (lead == null)
        {
            return null;
        }

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(lead.HomeType))
        {
            parts.Add(lead.HomeType);
        }

        if (lead.YearBuilt.HasValue && lead.SquareFeet.HasValue)
        {
            parts.Add($"Built in {lead.YearBuilt} · {lead.SquareFeet:N0} sq ft");
        }
        else if (lead.YearBuilt.HasValue)
        {
            parts.Add($"Built in {lead.YearBuilt}");
        }

        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }

    private static string ResolveEstimateImage(IndorProveedorEstimate estimate, IndorProveedorLead? lead)
    {
        if (!string.IsNullOrWhiteSpace(estimate.ImageUrl))
        {
            return estimate.ImageUrl;
        }

        if (!string.IsNullOrWhiteSpace(lead?.ImageUrl))
        {
            return lead.ImageUrl;
        }

        return "/welcome-house.png";
    }

    private static string NormalizeEstimateTab(string? tab) => (tab ?? "all").Trim().ToLowerInvariant() switch
    {
        "needsreview" or "needs_review" => "needsreview",
        "aidraft" or "ai_draft" => "aidraft",
        "draft" or "ready" or "sent" => tab!.Trim().ToLowerInvariant(),
        _ => "all"
    };

    private static List<ProviderProWizardStepViewModel> BuildEstimateWizardSteps(int currentStep) =>
    [
        new() { Number = 1, Label = "Home", IsComplete = currentStep > 1, IsCurrent = currentStep == 1 },
        new() { Number = 2, Label = "Pending Estimates", IsComplete = currentStep > 2, IsCurrent = currentStep == 2 },
        new() { Number = 3, Label = "Estimate Review", IsComplete = currentStep > 3, IsCurrent = currentStep == 3 },
        new() { Number = 4, Label = "Edit Estimate", IsComplete = currentStep > 4, IsCurrent = currentStep == 4 },
        new() { Number = 5, Label = "Send Estimate", IsComplete = currentStep > 5, IsCurrent = currentStep == 5 }
    ];

    private static (string Label, string Class, string FilterKey) MapEstimateHubStatus(IndorProveedorEstimate estimate)
    {
        var scopeItems = ParseScopeItems(estimate.ScopeItemsJson);
        if (estimate.Status == ProviderEstimateStatuses.Ready)
        {
            return ("Ready to Send", "ready", "ready");
        }

        if (estimate.Status is ProviderEstimateStatuses.Sent or ProviderEstimateStatuses.Viewed)
        {
            return ("Sent", "sent", "sent");
        }

        if (estimate.Status == ProviderEstimateStatuses.Draft)
        {
            var isAiDraft = scopeItems.Count == 0 && estimate.Amount <= 0
                || (estimate.Description?.Contains("AI", StringComparison.OrdinalIgnoreCase) ?? false);
            return isAiDraft
                ? ("AI Draft", "ai-draft", "aidraft")
                : ("Needs Review", "needs-review", "needsreview");
        }

        return (estimate.Status, "draft", "draft");
    }

    private static string ResolveEstimateTitle(IndorProveedorEstimate estimate) =>
        !string.IsNullOrWhiteSpace(estimate.Title)
            ? estimate.Title!
            : !string.IsNullOrWhiteSpace(estimate.ServiceType)
                ? estimate.ServiceType!
                : "Estimate";

    private static (string Icon, string Tone) ResolveEstimateServiceVisuals(string? serviceType, string? serviceCategoryId)
    {
        var st = (serviceType ?? serviceCategoryId ?? "").ToLowerInvariant();
        if (st.Contains("water") || st.Contains("damage") || st.Contains("inspection"))
        {
            return ("fa-droplet", "blue");
        }

        if (st.Contains("mold"))
        {
            return ("fa-seedling", "green");
        }

        if (st.Contains("hvac") || st.Contains("maintenance"))
        {
            return ("fa-fan", "purple");
        }

        if (st.Contains("smoke") || st.Contains("fire") || st.Contains("restoration"))
        {
            return ("fa-fire", "orange");
        }

        return ("fa-wrench", "blue");
    }

    private static List<string> BuildScopeSummaryLines(
        IndorProveedorEstimate estimate,
        List<ProviderProEstimateLineItemViewModel> scopeItems)
    {
        if (!string.IsNullOrWhiteSpace(estimate.Description))
        {
            return estimate.Description
                .Split(['\n', '\r', '•', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(line => line.Length > 2)
                .Take(5)
                .ToList();
        }

        return scopeItems
            .Select(i => i.Label)
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Take(5)
            .ToList();
    }

    private static List<string> BuildAiRecommendations(IndorProveedorEstimate estimate, string? serviceType)
    {
        var st = (serviceType ?? "").ToLowerInvariant();
        if (st.Contains("water") || st.Contains("damage"))
        {
            return
            [
                "Consider thermal imaging for hidden moisture detection.",
                "Include containment and drying if widespread moisture is found.",
                "Recommend air quality testing if mold is suspected."
            ];
        }

        if (st.Contains("mold"))
        {
            return
            [
                "Recommend professional remediation protocol if spore counts are elevated.",
                "Include containment barriers in scope if needed.",
                "Suggest follow-up clearance testing."
            ];
        }

        if (st.Contains("hvac"))
        {
            return
            [
                "Confirm filter sizes and system age before finalizing parts.",
                "Include coil cleaning if dust buildup is visible.",
                "Recommend seasonal maintenance plan."
            ];
        }

        if (!string.IsNullOrWhiteSpace(estimate.Warranty))
        {
            return
            [
                $"Highlight warranty coverage: {estimate.Warranty}.",
                "Confirm timeline with customer before sending.",
                "Include photos with the final estimate."
            ];
        }

        return
        [
            "Review scope with customer before sending.",
            "Confirm timeline and warranty terms.",
            "Include photos with the final estimate."
        ];
    }

    private static string NormalizeDeliveryMethod(string method) => method.Trim().ToLowerInvariant() switch
    {
        "indor" => "INDOR",
        "sms" or "text" => "SMS",
        "email" => "Email",
        _ => method
    };

    private static string NormalizeDeliveryMethodKey(string? method) => (method ?? "").Trim().ToLowerInvariant() switch
    {
        "indor" => "indor",
        "sms" or "text" => "sms",
        _ => "email"
    };

    private static List<ProviderProEstimateLineItemViewModel> BuildScopeItemsFromInput(ProviderProQuickEstimateInput input)
    {
        var items = new List<ProviderProEstimateLineItemViewModel>();
        for (var i = 0; i < input.ScopeLabels.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(input.ScopeLabels[i]))
            {
                continue;
            }

            var qty = i < input.ScopeQtys.Count && input.ScopeQtys[i] > 0 ? input.ScopeQtys[i] : 1m;
            var unitPrice = i < input.ScopeUnitPrices.Count ? input.ScopeUnitPrices[i] : 0m;
            var amount = i < input.ScopeAmounts.Count ? input.ScopeAmounts[i] : 0m;
            if (amount <= 0 && unitPrice > 0)
            {
                amount = Math.Round(qty * unitPrice, 2, MidpointRounding.AwayFromZero);
            }

            if (amount <= 0 && unitPrice <= 0)
            {
                continue;
            }

            var labor = i < input.ScopeLaborAmounts.Count ? input.ScopeLaborAmounts[i] : 0m;
            var material = i < input.ScopeMaterialAmounts.Count ? input.ScopeMaterialAmounts[i] : 0m;
            if (amount <= 0 && (labor > 0 || material > 0))
            {
                amount = labor + material;
            }

            items.Add(new ProviderProEstimateLineItemViewModel
            {
                Label = input.ScopeLabels[i],
                Qty = qty,
                UnitPrice = unitPrice > 0 ? unitPrice : qty > 0 ? amount / qty : amount,
                LaborAmount = labor,
                MaterialAmount = material,
                Amount = amount
            });
        }

        return items;
    }

    private static string SerializeScopeItems(List<ProviderProEstimateLineItemViewModel> items) =>
        JsonSerializer.Serialize(items);

    private static readonly JsonSerializerOptions ScopeJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static List<ProviderProEstimateLineItemViewModel> ParseScopeItems(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<ProviderProEstimateLineItemViewModel>>(json, ScopeJsonOptions) ?? [];
            if (items.Count > 0)
            {
                return items;
            }
        }
        catch
        {
            // fall through to legacy shape
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var legacy = new List<ProviderProEstimateLineItemViewModel>();
            foreach (var node in doc.RootElement.EnumerateArray())
            {
                var label = ReadJsonString(node, "label", "Label");
                if (string.IsNullOrWhiteSpace(label))
                {
                    continue;
                }

                decimal amount = 0m;
                if (node.TryGetProperty("amount", out var amountNode) && amountNode.TryGetDecimal(out var amountValue))
                {
                    amount = amountValue;
                }
                else if (node.TryGetProperty("Amount", out var amountNodeAlt) && amountNodeAlt.TryGetDecimal(out var amountValueAlt))
                {
                    amount = amountValueAlt;
                }

                legacy.Add(new ProviderProEstimateLineItemViewModel
                {
                    Label = label,
                    Amount = amount,
                    Description = ReadJsonString(node, "description", "Description") ?? "",
                    Qty = 1
                });
            }

            return legacy;
        }
        catch
        {
            return [];
        }
    }

    private static List<ProviderProEstimateLineItemViewModel> BuildScopeItemsFromFindings(string? findingsJson)
    {
        return ParseInspectionFindings(findingsJson)
            .Select(f => new ProviderProEstimateLineItemViewModel
            {
                Label = f.Title,
                Description = f.Description ?? "",
                Amount = 0,
                Qty = 1
            })
            .ToList();
    }

    private static List<string> ParsePhotoUrls(string? photosJson, string? fallback)
    {
        var urls = new List<string>();

        if (!string.IsNullOrWhiteSpace(photosJson))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<List<string>>(photosJson);
                if (parsed != null)
                {
                    urls.AddRange(parsed.Where(u => !string.IsNullOrWhiteSpace(u)));
                }
            }
            catch
            {
                // ignore malformed JSON
            }
        }

        if (urls.Count == 0 && !string.IsNullOrWhiteSpace(fallback))
        {
            urls.Add(fallback);
        }

        return urls;
    }

    private static string BuildInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return "H";
        }

        if (parts.Length == 1)
        {
            return parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant();
        }

        return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
    }

    private static List<ProviderProFlowStepViewModel> LeadDetailsFlowSteps(int leadId) =>
    [
        new()
        {
            Label = "From: New Leads",
            IconClass = "fa-user-group",
            IsLink = true,
            Url = "/Proveedor/NewLeads"
        },
        new() { Label = "Pressed: View", IconClass = "fa-eye" },
        new() { Label = "Now: Lead Details", IconClass = "fa-clipboard-list", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> ScheduleVisitFlowSteps(int leadId, bool verification = false) =>
    [
        new()
        {
            Label = "From: Lead Details",
            IconClass = "fa-clipboard-list",
            IsLink = true,
            Url = $"/Proveedor/LeadDetails/{leadId}"
        },
        new()
        {
            Label = verification ? "Pressed: Schedule Verification Visit" : "Pressed: Schedule Visit",
            IconClass = "fa-calendar"
        },
        new()
        {
            Label = verification ? "Now: Verification Visit" : "Now: Schedule Visit",
            IconClass = "fa-calendar-check",
            IsCurrent = true
        }
    ];

    private static List<ProviderProFlowStepViewModel> QuickEstimateFlowSteps(int leadId) =>
    [
        new()
        {
            Label = "From: Lead Details",
            IconClass = "fa-clipboard-list",
            IsLink = true,
            Url = $"/Proveedor/LeadDetails/{leadId}"
        },
        new() { Label = "Pressed: Create Quick Estimate", IconClass = "fa-file-invoice" },
        new() { Label = "Now: Quick Estimate", IconClass = "fa-pen", IsCurrent = true }
    ];

    private static List<ProviderProEstimateLineItemViewModel> BuildDefaultEstimateLineItems() =>
    [
        new() { Category = "labor", Label = "Labor", Description = "Installation & Setup", Qty = 12, Unit = "hrs", UnitPrice = 85, Amount = 1020, IsTaxable = true },
        new() { Category = "materials", Label = "Materials", Description = "Equipment & Supplies", Qty = 1, Unit = "ls", UnitPrice = 3250, Amount = 3250, IsTaxable = true },
        new() { Category = "permit", Label = "Permit / Disposal", Description = "Permit Fees & Dump Fees", Qty = 1, Unit = "ls", UnitPrice = 175, Amount = 175, IsTaxable = true },
        new() { Category = "addon", Label = "Optional Add-on", Description = "Premium Upgrade Package", Qty = 1, Unit = "ls", UnitPrice = 450, Amount = 450, IsTaxable = false }
    ];

    private static List<string> BuildWarrantyOptions() =>
    [
        "1 Year Parts & Labor",
        "2 Year Parts & Labor",
        "5 Year Parts & Labor",
        "10-Year Parts & Labor Warranty",
        "90 Day Labor Warranty"
    ];

    private static string BuildEstimateTimelineLabel(ProviderProCreateEstimateDraft draft)
    {
        if (DateTime.TryParse(draft.EstimatedStartDate, out var start)
            && DateTime.TryParse(draft.EstimatedEndDate, out var end))
        {
            draft.EstimatedDuration = $"{(end - start).Days + 1} day{((end - start).Days + 1 == 1 ? "" : "s")}";
            return $"Est. Start: {start:MMM d, yyyy} · Est. Duration: {draft.EstimatedDuration}";
        }

        if (!string.IsNullOrWhiteSpace(draft.EstimatedDuration))
        {
            return draft.EstimatedDuration;
        }

        return draft.EstimatedStartDate;
    }

    private static (decimal Subtotal, decimal Tax, decimal Total) CalculateEstimateTotals(
        List<ProviderProEstimateLineItemViewModel> lineItems,
        decimal taxRate)
    {
        var subtotal = lineItems.Sum(i => i.Amount);
        var taxable = lineItems.Where(i => i.IsTaxable).Sum(i => i.Amount);
        var tax = Math.Round(taxable * taxRate, 2, MidpointRounding.AwayFromZero);
        return (subtotal, tax, subtotal + tax);
    }

    private static void ApplyEstimatePricingFromLineItems(
        IndorProveedorEstimate estimate,
        List<ProviderProEstimateLineItemViewModel> lineItems,
        decimal taxRate)
    {
        var (subtotal, tax, total) = CalculateEstimateTotals(lineItems, taxRate);
        estimate.SubtotalAmount = subtotal;
        estimate.TaxRate = taxRate;
        estimate.TaxAmount = tax;
        estimate.Amount = total;
        estimate.LaborAmount = lineItems.Where(i => i.Category == "labor").Sum(i => i.Amount);
        estimate.MaterialsAmount = lineItems.Where(i => i.Category == "materials").Sum(i => i.Amount);
    }

    private static string SerializePricingLineItems(List<ProviderProEstimateLineItemViewModel> items) =>
        JsonSerializer.Serialize(items);

    private static string BuildCustomerFullName(ProviderProAddCustomerDraft draft) =>
        $"{draft.FirstName.Trim()} {draft.LastName.Trim()}".Trim();

    private static string BuildCustomerFullAddress(ProviderProAddCustomerDraft draft)
    {
        var line1 = draft.StreetAddress.Trim();
        if (!string.IsNullOrWhiteSpace(draft.AptUnit))
        {
            line1 = $"{line1}, {draft.AptUnit.Trim()}";
        }

        var line2 = $"{draft.City.Trim()}, {draft.State} {draft.ZipCode.Trim()}".Trim();
        return string.IsNullOrWhiteSpace(line2) ? line1 : $"{line1}, {line2}";
    }

    private static string BuildBedsBathsLabel(int? beds, decimal? baths)
    {
        if (!beds.HasValue && !baths.HasValue)
        {
            return "—";
        }

        var bedLabel = beds.HasValue ? beds.Value.ToString() : "—";
        var bathLabel = baths.HasValue ? baths.Value.ToString("0.#") : "—";
        return $"{bedLabel} / {bathLabel}";
    }

    private static List<string> BuildUsStateOptions() =>
    [
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
        "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
        "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
        "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
        "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY"
    ];

    private static List<ProviderProFlowStepViewModel> AddCustomerInfoFlowSteps() =>
    [
        new() { Label = "From: Home Dashboard", IconClass = "fa-house", IsLink = true, Url = "/Proveedor/Dashboard" },
        new() { Label = "Pressed: Add Customer", IconClass = "fa-user-plus" },
        new() { Label = "Now: Customer Info", IconClass = "fa-user", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> AddCustomerPropertyFlowSteps() =>
    [
        new() { Label = "From: Customer Information", IconClass = "fa-user", IsLink = true, Url = "/Proveedor/AddCustomer" },
        new() { Label = "Pressed: Next", IconClass = "fa-arrow-right" },
        new() { Label = "Now: Property Info", IconClass = "fa-house", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> AddCustomerPreferencesFlowSteps() =>
    [
        new() { Label = "From: Property Info", IconClass = "fa-house", IsLink = true, Url = "/Proveedor/AddCustomerProperty" },
        new() { Label = "Pressed: Next", IconClass = "fa-arrow-right" },
        new() { Label = "Now: Preferences & Notes", IconClass = "fa-sliders", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> AddCustomerReviewFlowSteps() =>
    [
        new() { Label = "From: Preferences & Notes", IconClass = "fa-sliders", IsLink = true, Url = "/Proveedor/AddCustomerPreferences" },
        new() { Label = "Pressed: Next", IconClass = "fa-arrow-right" },
        new() { Label = "Now: Review Customer", IconClass = "fa-clipboard-check", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> AddCustomerSuccessFlowSteps(string customerCode) =>
    [
        new() { Label = "From: Review Customer", IconClass = "fa-clipboard-check" },
        new() { Label = "Pressed: Save Customer", IconClass = "fa-circle-check" },
        new() { Label = "Now: Customer Created", IconClass = "fa-circle-check", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> UploadReportSelectJobFlowSteps() =>
    [
        new() { Label = "From: Home Dashboard", IconClass = "fa-house", IsLink = true, Url = "/Proveedor/Dashboard" },
        new() { Label = "Pressed: Upload Report", IconClass = "fa-cloud-arrow-up" },
        new() { Label = "Now: Select Job", IconClass = "fa-briefcase", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> UploadReportTypeFlowSteps() =>
    [
        new() { Label = "From: Select Job", IconClass = "fa-briefcase", IsLink = true, Url = "/Proveedor/UploadReport" },
        new() { Label = "Pressed: Next", IconClass = "fa-arrow-right" },
        new() { Label = "Now: Report Type", IconClass = "fa-file-lines", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> UploadReportFilesFlowSteps() =>
    [
        new() { Label = "From: Report Type", IconClass = "fa-file-lines", IsLink = true, Url = "/Proveedor/UploadReportType" },
        new() { Label = "Pressed: Next", IconClass = "fa-arrow-right" },
        new() { Label = "Now: Upload Files", IconClass = "fa-cloud-arrow-up", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> UploadReportDetailsFlowSteps() =>
    [
        new() { Label = "From: Upload Files", IconClass = "fa-cloud-arrow-up", IsLink = true, Url = "/Proveedor/UploadReportFiles" },
        new() { Label = "Pressed: Next", IconClass = "fa-arrow-right" },
        new() { Label = "Now: Report Details", IconClass = "fa-pen", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> UploadReportSuccessFlowSteps() =>
    [
        new() { Label = "From: Report Details", IconClass = "fa-pen" },
        new() { Label = "Pressed: Submit Report", IconClass = "fa-circle-check" },
        new() { Label = "Now: Report Uploaded", IconClass = "fa-circle-check", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> CreateEstimateSetupFlowSteps() =>
    [
        new() { Label = "From: Home Dashboard", IconClass = "fa-house", IsLink = true, Url = "/Proveedor/Dashboard" },
        new() { Label = "Pressed: Create Estimate", IconClass = "fa-file-invoice-dollar" },
        new() { Label = "Now: Create New Estimate", IconClass = "fa-file-circle-plus", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> CreateEstimateDetailsFlowSteps() =>
    [
        new() { Label = "From: Create New Estimate", IconClass = "fa-file-circle-plus", IsLink = true, Url = "/Proveedor/CreateEstimate" },
        new() { Label = "Pressed: Next", IconClass = "fa-arrow-right" },
        new() { Label = "Now: Estimate Details", IconClass = "fa-pen", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> CreateEstimatePricingFlowSteps() =>
    [
        new() { Label = "From: Estimate Details", IconClass = "fa-pen", IsLink = true, Url = "/Proveedor/CreateEstimateDetails" },
        new() { Label = "Pressed: Next", IconClass = "fa-arrow-right" },
        new() { Label = "Now: Pricing & Line Items", IconClass = "fa-dollar-sign", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> CreateEstimateReviewFlowSteps() =>
    [
        new() { Label = "From: Pricing & Line Items", IconClass = "fa-dollar-sign", IsLink = true, Url = "/Proveedor/CreateEstimatePricing" },
        new() { Label = "Pressed: Next", IconClass = "fa-arrow-right" },
        new() { Label = "Now: Review & Send", IconClass = "fa-paper-plane", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> PendingEstimatesFlowSteps() =>
    [
        new() { Label = "From: Home Dashboard", IconClass = "fa-house", IsLink = true, Url = "/Proveedor/Dashboard" },
        new() { Label = "Pressed: Pending Estimates / Review", IconClass = "fa-hand-pointer" },
        new() { Label = "Now: Pending Estimates", IconClass = "fa-file-invoice", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> EditEstimateFlowSteps(int estimateId) =>
    [
        new() { Label = "From: Pending Estimates", IconClass = "fa-file-invoice", IsLink = true, Url = "/Proveedor/PendingEstimates" },
        new() { Label = "Pressed: Edit", IconClass = "fa-pen" },
        new() { Label = "Now: Edit Estimate", IconClass = "fa-pen-to-square", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> ReviewEstimateFlowSteps(int estimateId) =>
    [
        new() { Label = "From: Pending Estimates", IconClass = "fa-file-invoice", IsLink = true, Url = "/Proveedor/PendingEstimates" },
        new() { Label = "Pressed: Review & Send", IconClass = "fa-paper-plane" },
        new() { Label = "Now: Review Estimate", IconClass = "fa-file-lines", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> EstimateSentFlowSteps(int estimateId) =>
    [
        new() { Label = "From: Review Estimate", IconClass = "fa-file-lines", IsLink = true, Url = $"/Proveedor/ReviewEstimate/{estimateId}" },
        new() { Label = "Pressed: Send Estimate", IconClass = "fa-paper-plane" },
        new() { Label = "Now: Estimate Sent", IconClass = "fa-circle-check", IsCurrent = true }
    ];

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

    private static ProviderProJobsWorkItemViewModel MapEstimateWorkItem(
        IndorProveedorEstimate estimate,
        IReadOnlyList<IndorProveedorJob> jobs)
    {
        var hasLinkedJob = estimate.JobId.HasValue
            || (estimate.LeadId.HasValue && jobs.Any(j => j.LeadId == estimate.LeadId));
        var canConvert = estimate.Status == ProviderEstimateStatuses.Approved && !hasLinkedJob;

        var meta = new List<ProviderProJobMetaLineViewModel>
        {
            new()
            {
                Text = $"Estimate ${estimate.Amount:N0}",
                IconClass = "fa-file-invoice-dollar",
                Tone = "neutral"
            }
        };

        if (estimate.ViewedUtc.HasValue)
        {
            meta.Add(new ProviderProJobMetaLineViewModel
            {
                Text = "Viewed by customer",
                IconClass = "fa-eye",
                Tone = "neutral"
            });
        }

        var isDraft = estimate.Status == ProviderEstimateStatuses.Draft;

        return new ProviderProJobsWorkItemViewModel
        {
            ItemId = estimate.Id,
            ItemKind = "Estimate",
            Title = estimate.ServiceType ?? estimate.Title ?? $"Estimate #{estimate.EstimateCode}",
            Address = estimate.Address,
            CustomerName = estimate.CustomerName ?? "",
            TimeLabel = estimate.SentUtc.HasValue ? FormatTimeLabel(estimate.SentUtc) : "Draft",
            StatusLabel = "Estimate",
            StatusClass = "estimate",
            IconClass = MapServiceIcon(estimate.ServiceType ?? estimate.EstimateCode),
            EstimateAmount = estimate.Amount,
            MetaLines = meta,
            ShowEstimateLink = true,
            ShowPhotosLink = false,
            ShowChecklistLink = false,
            ShowHouseFactsLink = true,
            PrimaryAction = isDraft ? "Edit Estimate" : "Follow Up",
            PrimaryActionClass = "primary",
            SecondaryAction = canConvert ? "Convert to Job" : "View Estimate",
            CanConvertToJob = canConvert,
            LeadId = estimate.LeadId
        };
    }

    private static string NormalizeJobsTab(string? tab) => (tab ?? "active").Trim().ToLowerInvariant() switch
    {
        "all" or "today" or "leads" or "estimates" or "completed" => tab!.Trim().ToLowerInvariant(),
        _ => "active"
    };

    private static string FormatTimeLabel(DateTime? when)
    {
        if (!when.HasValue)
        {
            return "Not scheduled";
        }

        var local = when.Value.Kind == DateTimeKind.Utc ? when.Value.ToLocalTime() : when.Value;
        var day = local.Date == DateTime.Today ? "Today"
            : local.Date == DateTime.Today.AddDays(1) ? "Tomorrow"
            : local.ToString("MMM d");

        return $"{day} • {local:h:mm tt}";
    }

    private static string FormatScheduleTimeShort(DateTime? when)
    {
        if (!when.HasValue)
        {
            return "TBD";
        }

        var local = when.Value.Kind == DateTimeKind.Utc ? when.Value.ToLocalTime() : when.Value;
        return local.ToString("h:mm tt");
    }

    private static string MapJobStatusLabel(string status) => ProviderProDisplayLocalization.MapJobStatus(status);

    private static (string Icon, string Tone) DeriveHomeJobPresentation(string title, string? serviceType, string status)
    {
        if (status == ProviderJobStatuses.InProgress)
            return ("fa-wrench", "blue");
        if (status is ProviderJobStatuses.Scheduled or ProviderJobStatuses.Confirmed)
            return ("fa-calendar-days", "orange");

        var combined = $"{title} {serviceType}".ToLowerInvariant();
        if (combined.Contains("mold"))
            return ("fa-virus", "purple");
        if (combined.Contains("water") || combined.Contains("damage"))
            return ("fa-droplet", "blue");
        return (MapServiceIcon(serviceType ?? title), "blue");
    }

    private static (string Primary, string PrimaryClass, string? Secondary) MapJobActions(string status) => status switch
    {
        ProviderJobStatuses.InProgress => ("Continue Job", "success", "Upload Photos"),
        ProviderJobStatuses.Completed => ("View Record", "ghost", null),
        ProviderJobStatuses.Scheduled or ProviderJobStatuses.Confirmed => ("Start Job", "primary", "View Details"),
        _ => ("View Details", "ghost", null)
    };

    private static string MapServiceIcon(string value)
    {
        var s = value.ToLowerInvariant();
        if (s.Contains("water") || s.Contains("heater")) return "fa-fire-flame-simple";
        if (s.Contains("hvac") || s.Contains("ac ") || s.Contains("air")) return "fa-snowflake";
        if (s.Contains("roof") || s.Contains("leak")) return "fa-house-chimney-crack";
        if (s.Contains("gutter")) return "fa-droplet";
        if (s.Contains("plumb")) return "fa-faucet-drip";
        if (s.Contains("electric")) return "fa-bolt";
        return "fa-wrench";
    }

    public ProviderProAddCustomerInfoViewModel GetAddCustomerInfoViewModel(
        IndorProveedor proveedor,
        ProviderProAddCustomerDraft? draft) =>
        new()
        {
            CompanyName = ResolveCompanyName(proveedor),
            CustomerType = draft?.CustomerType ?? "Homeowner",
            FirstName = draft?.FirstName ?? "",
            LastName = draft?.LastName ?? "",
            Phone = draft?.Phone ?? "",
            Email = draft?.Email ?? "",
            PreferredContactMethod = draft?.PreferredContactMethod ?? "SMS",
            CustomerCompanyName = draft?.CompanyName ?? "",
            FlowSteps = AddCustomerInfoFlowSteps()
        };

    public ProviderProAddCustomerPropertyViewModel GetAddCustomerPropertyViewModel(
        IndorProveedor proveedor,
        ProviderProAddCustomerDraft draft) =>
        new()
        {
            CompanyName = ResolveCompanyName(proveedor),
            StreetAddress = draft.StreetAddress,
            AptUnit = draft.AptUnit,
            City = draft.City,
            State = draft.State,
            ZipCode = draft.ZipCode,
            PropertyType = draft.PropertyType,
            Bedrooms = draft.Bedrooms,
            Bathrooms = draft.Bathrooms,
            IsBillingAddressSame = draft.IsBillingAddressSame,
            AccessNotes = draft.AccessNotes,
            StateOptions = BuildUsStateOptions(),
            BedroomOptions = [1, 2, 3, 4, 5, 6],
            BathroomOptions = [1m, 1.5m, 2m, 2.5m, 3m, 3.5m, 4m],
            FlowSteps = AddCustomerPropertyFlowSteps()
        };

    public ProviderProAddCustomerPreferencesViewModel GetAddCustomerPreferencesViewModel(
        IndorProveedor proveedor,
        ProviderProAddCustomerDraft draft) =>
        new()
        {
            CompanyName = ResolveCompanyName(proveedor),
            EstimateDeliveryPref = draft.EstimateDeliveryPref,
            InvoiceDeliveryPref = draft.InvoiceDeliveryPref,
            PreferredLanguage = draft.PreferredLanguage,
            CustomerSource = draft.CustomerSource,
            Tags = draft.Tags,
            InternalNotes = draft.InternalNotes,
            SendIndorInvite = draft.SendIndorInvite,
            AllowServiceUpdates = draft.AllowServiceUpdates,
            FlowSteps = AddCustomerPreferencesFlowSteps()
        };

    public ProviderProAddCustomerReviewViewModel? GetAddCustomerReviewViewModel(
        IndorProveedor proveedor,
        ProviderProAddCustomerDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.FirstName) || string.IsNullOrWhiteSpace(draft.StreetAddress))
        {
            return null;
        }

        return new ProviderProAddCustomerReviewViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            CustomerType = draft.CustomerType,
            FullName = BuildCustomerFullName(draft),
            Phone = draft.Phone,
            Email = draft.Email,
            PreferredContactMethod = draft.PreferredContactMethod,
            FullAddress = BuildCustomerFullAddress(draft),
            PropertyType = draft.PropertyType,
            BedsBathsLabel = BuildBedsBathsLabel(draft.Bedrooms, draft.Bathrooms),
            BillingLabel = draft.IsBillingAddressSame ? "Yes" : "No",
            EstimateDeliveryPref = draft.EstimateDeliveryPref,
            InvoiceDeliveryPref = draft.InvoiceDeliveryPref,
            PreferredLanguage = draft.PreferredLanguage,
            CustomerSource = draft.CustomerSource,
            Tags = draft.Tags,
            InternalNotes = draft.InternalNotes,
            FlowSteps = AddCustomerReviewFlowSteps()
        };
    }

    public async Task<int?> SaveAddCustomerFromDraftAsync(
        int proveedorId,
        ProviderProAddCustomerDraft draft,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(draft.FirstName) || string.IsNullOrWhiteSpace(draft.StreetAddress))
        {
            return null;
        }

        var fullName = BuildCustomerFullName(draft);
        var fullAddress = BuildCustomerFullAddress(draft);
        var cityState = string.IsNullOrWhiteSpace(draft.City) || string.IsNullOrWhiteSpace(draft.State)
            ? null
            : $"{draft.City}, {draft.State}";

        var cliente = new IndorProveedorCliente
        {
            ProveedorId = proveedorId,
            CustomerCode = $"CUST-{DateTime.UtcNow:yyyyMMddHHmm}",
            Name = fullName,
            CustomerType = draft.CustomerType,
            FirstName = draft.FirstName.Trim(),
            LastName = draft.LastName.Trim(),
            Phone = draft.Phone,
            Email = draft.Email,
            PreferredContactMethod = draft.PreferredContactMethod,
            CompanyName = string.IsNullOrWhiteSpace(draft.CompanyName) ? null : draft.CompanyName.Trim(),
            StreetAddress = draft.StreetAddress.Trim(),
            AptUnit = string.IsNullOrWhiteSpace(draft.AptUnit) ? null : draft.AptUnit.Trim(),
            City = draft.City.Trim(),
            State = draft.State,
            ZipCode = draft.ZipCode.Trim(),
            Address = fullAddress,
            CityState = cityState,
            PropertyType = draft.PropertyType,
            Bedrooms = draft.Bedrooms,
            Bathrooms = draft.Bathrooms,
            IsBillingAddressSame = draft.IsBillingAddressSame,
            AccessNotes = string.IsNullOrWhiteSpace(draft.AccessNotes) ? null : draft.AccessNotes.Trim(),
            EstimateDeliveryPref = draft.EstimateDeliveryPref,
            InvoiceDeliveryPref = draft.InvoiceDeliveryPref,
            PreferredLanguage = draft.PreferredLanguage,
            CustomerSource = draft.CustomerSource,
            TagsJson = draft.Tags.Count > 0 ? JsonSerializer.Serialize(draft.Tags) : null,
            InternalNotes = string.IsNullOrWhiteSpace(draft.InternalNotes) ? null : draft.InternalNotes.Trim(),
            SendIndorInvite = draft.SendIndorInvite,
            AllowServiceUpdates = draft.AllowServiceUpdates,
            ConnectionStatus = draft.SendIndorInvite
                ? ProviderCustomerConnectionStatuses.NeedsInvite
                : ProviderCustomerConnectionStatuses.Connected,
            IsAppConnected = false,
            PropertiesCount = 1,
            MemberSince = DateTime.UtcNow,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorProveedorClientes.Add(cliente);
        await db.SaveChangesAsync(cancellationToken);
        cliente.CustomerCode = $"CUST-{1000 + cliente.Id}";
        await db.SaveChangesAsync(cancellationToken);
        return cliente.Id;
    }

    public async Task<ProviderProAddCustomerSuccessViewModel?> GetAddCustomerSuccessAsync(
        IndorProveedor proveedor,
        int customerId,
        CancellationToken cancellationToken = default)
    {
        var cliente = await db.IndorProveedorClientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId && c.ProveedorId == proveedor.Id, cancellationToken);

        if (cliente == null)
        {
            return null;
        }

        return new ProviderProAddCustomerSuccessViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            CustomerId = cliente.Id,
            CustomerCode = cliente.CustomerCode ?? $"CUST-{cliente.Id}",
            FullName = cliente.Name,
            CustomerType = cliente.CustomerType ?? "Homeowner",
            Phone = cliente.Phone ?? "",
            Address = cliente.Address ?? "",
            StatusLabel = "Active",
            FlowSteps = AddCustomerSuccessFlowSteps(cliente.CustomerCode ?? $"CUST-{cliente.Id}")
        };
    }

    public async Task<ProviderProUploadReportSelectJobViewModel> GetUploadReportSelectJobAsync(
        IndorProveedor proveedor,
        ProviderProUploadReportDraft? draft,
        string? search = null,
        string? filter = "all",
        CancellationToken cancellationToken = default)
    {
        var activeFilter = NormalizeUploadReportJobFilter(filter);
        var recentCutoff = DateTime.UtcNow.AddDays(-30);

        var jobRows = await db.IndorProveedorJobs
            .AsNoTracking()
            .Include(j => j.Cliente)
            .Where(j => j.ProveedorId == proveedor.Id && !j.IsDraft)
            .OrderByDescending(j => j.ScheduledAt ?? j.FechaCreacion)
            .ToListAsync(cancellationToken);

        var filtered = activeFilter switch
        {
            "completed" => jobRows.Where(j => j.Status == ProviderJobStatuses.Completed),
            "inprogress" => jobRows.Where(j => j.Status is ProviderJobStatuses.InProgress
                or ProviderJobStatuses.Scheduled
                or ProviderJobStatuses.Confirmed
                or ProviderJobStatuses.WaitingOnMaterials),
            "recent" => jobRows.Where(j => (j.ScheduledAt ?? j.FechaCreacion) >= recentCutoff),
            _ => jobRows.AsEnumerable()
        };

        var filteredList = filtered.ToList();
        var totalAvailable = filteredList.Count;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            filteredList = filteredList.Where(j =>
                j.Address.Contains(q, StringComparison.OrdinalIgnoreCase)
                || j.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                || j.JobCode.Contains(q, StringComparison.OrdinalIgnoreCase)
                || (j.ServiceType ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)
                || (j.Cliente?.Name ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)
                || (j.Cliente?.StreetAddress ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)
                || (j.Cliente?.Address ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return new ProviderProUploadReportSelectJobViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            SearchQuery = search,
            ActiveFilter = activeFilter,
            TotalJobsAvailable = totalAvailable,
            HasSearchWithNoResults = !string.IsNullOrWhiteSpace(search) && filteredList.Count == 0,
            Jobs = filteredList.Select(MapUploadReportJobOption).ToList(),
            FlowSteps = UploadReportSelectJobFlowSteps()
        };
    }

    public async Task<ProviderProUploadReportTypeViewModel?> GetUploadReportTypeAsync(
        IndorProveedor proveedor,
        ProviderProUploadReportDraft draft,
        CancellationToken cancellationToken = default)
    {
        var job = await LoadUploadReportJobSummaryAsync(proveedor.Id, draft.JobId, cancellationToken);
        if (job == null)
        {
            return null;
        }

        return new ProviderProUploadReportTypeViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            ReportType = draft.ReportType,
            Job = job,
            FlowSteps = UploadReportTypeFlowSteps()
        };
    }

    public async Task<ProviderProUploadReportFilesViewModel?> GetUploadReportFilesAsync(
        IndorProveedor proveedor,
        ProviderProUploadReportDraft draft,
        CancellationToken cancellationToken = default)
    {
        var job = await LoadUploadReportJobSummaryAsync(proveedor.Id, draft.JobId, cancellationToken);
        if (job == null)
        {
            return null;
        }

        return new ProviderProUploadReportFilesViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            Job = job,
            AttachToHouseFacts = draft.AttachToHouseFacts,
            PhotoSlots = draft.PhotoSlots,
            DocumentSlots = draft.DocumentSlots,
            GeneralFiles = draft.GeneralFiles,
            FlowSteps = UploadReportFilesFlowSteps()
        };
    }

    public async Task<ProviderProUploadReportDetailsViewModel?> GetUploadReportDetailsAsync(
        IndorProveedor proveedor,
        ProviderProUploadReportDraft draft,
        CancellationToken cancellationToken = default)
    {
        var job = await LoadUploadReportJobSummaryAsync(proveedor.Id, draft.JobId, cancellationToken);
        if (job == null)
        {
            return null;
        }

        var defaultTitle = string.IsNullOrWhiteSpace(draft.Title)
            ? $"{job.ServiceType} Report"
            : draft.Title;

        return new ProviderProUploadReportDetailsViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            ReportType = draft.ReportType,
            Job = job,
            Title = defaultTitle,
            Summary = draft.Summary,
            WorkCompleted = string.IsNullOrWhiteSpace(draft.WorkCompleted) ? job.Title : draft.WorkCompleted,
            MaterialsUsed = draft.MaterialsUsed,
            WarrantyInfo = draft.WarrantyInfo,
            Recommendations = draft.Recommendations,
            InternalNotes = draft.InternalNotes,
            SendToHomeowner = draft.SendToHomeowner,
            RequestApproval = draft.RequestApproval,
            CreateHouseFactsRecord = draft.CreateHouseFactsRecord,
            FlowSteps = UploadReportDetailsFlowSteps()
        };
    }

    public async Task<int?> SaveUploadReportFromDraftAsync(
        int proveedorId,
        ProviderProUploadReportDraft draft,
        CancellationToken cancellationToken = default)
    {
        if (!draft.JobId.HasValue || string.IsNullOrWhiteSpace(draft.Title))
        {
            return null;
        }

        var job = await db.IndorProveedorJobs
            .Include(j => j.Cliente)
            .FirstOrDefaultAsync(j => j.Id == draft.JobId && j.ProveedorId == proveedorId, cancellationToken);

        if (job == null)
        {
            return null;
        }

        var photosCount = draft.PhotoSlots.Count(s => !string.IsNullOrWhiteSpace(s.Url))
            + draft.GeneralFiles.Count(f => !string.IsNullOrWhiteSpace(f.Url));
        var docsCount = draft.DocumentSlots.Count(s => !string.IsNullOrWhiteSpace(s.Url));
        var filesCount = photosCount + docsCount + draft.GeneralFiles.Count;

        var status = draft.RequestApproval
            ? ProviderReportStatuses.Approval
            : ProviderReportStatuses.Ready;

        var addedToHouseFacts = draft.AttachToHouseFacts && draft.CreateHouseFactsRecord;

        var report = new IndorProveedorReport
        {
            ProveedorId = proveedorId,
            JobId = job.Id,
            ClienteId = job.ClienteId,
            ReportCode = $"RPT-{DateTime.UtcNow:yyyyMMddHHmm}",
            Title = draft.Title.Trim(),
            Address = job.Address,
            CustomerName = job.Cliente?.Name,
            ServiceType = job.ServiceType ?? job.Title,
            ReportType = draft.ReportType,
            Summary = NullIfEmpty(draft.Summary),
            WorkCompleted = NullIfEmpty(draft.WorkCompleted),
            MaterialsUsed = NullIfEmpty(draft.MaterialsUsed),
            WarrantyInfo = NullIfEmpty(draft.WarrantyInfo),
            Recommendations = NullIfEmpty(draft.Recommendations),
            InternalNotes = NullIfEmpty(draft.InternalNotes),
            SendToHomeowner = draft.SendToHomeowner,
            RequestApproval = draft.RequestApproval,
            AttachToHouseFacts = draft.AttachToHouseFacts,
            AddedToHouseFacts = addedToHouseFacts,
            Status = addedToHouseFacts ? ProviderReportStatuses.HouseFacts : status,
            PhotosCount = photosCount,
            HasDocuments = docsCount > 0,
            HasWarranty = !string.IsNullOrWhiteSpace(draft.WarrantyInfo)
                || draft.DocumentSlots.Any(d => d.Slot.Contains("Warranty", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(d.Url)),
            HasChecklist = draft.ReportType == ProviderReportTypes.Assessment,
            PhotoUrlsJson = SerializeUploadReportFiles(draft.PhotoSlots, draft.GeneralFiles),
            DocumentsJson = SerializeUploadReportDocuments(draft.DocumentSlots),
            FilesCount = filesCount,
            CompletedUtc = DateTime.UtcNow,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorProveedorReports.Add(report);
        await db.SaveChangesAsync(cancellationToken);
        report.ReportCode = $"RPT-{1000 + report.Id}";
        await db.SaveChangesAsync(cancellationToken);
        return report.Id;
    }

    public async Task<ProviderProUploadReportSuccessViewModel?> GetUploadReportSuccessAsync(
        IndorProveedor proveedor,
        int reportId,
        CancellationToken cancellationToken = default)
    {
        var report = await db.IndorProveedorReports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reportId && r.ProveedorId == proveedor.Id, cancellationToken);

        if (report == null)
        {
            return null;
        }

        var statusLabel = report.Status switch
        {
            ProviderReportStatuses.Approval => "Awaiting approval",
            ProviderReportStatuses.HouseFacts => "Added to House Facts",
            _ => "Ready to send"
        };

        return new ProviderProUploadReportSuccessViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            ReportId = report.Id,
            JobId = report.JobId,
            Address = report.Address,
            ServiceType = report.ServiceType ?? report.Title,
            ReportType = report.ReportType ?? ProviderReportTypes.Completion,
            FilesCount = report.FilesCount > 0 ? report.FilesCount : report.PhotosCount,
            StatusLabel = statusLabel,
            FlowSteps = UploadReportSuccessFlowSteps()
        };
    }

    // ---------------------------------------------------------------
    // Upload Photos flow (select job → add photos → review → save)
    // ---------------------------------------------------------------

    public async Task<ProviderProUploadPhotosSelectJobViewModel> GetUploadPhotosSelectJobAsync(
        IndorProveedor proveedor,
        string? search = null,
        string? filter = "all",
        CancellationToken cancellationToken = default)
    {
        var activeFilter = NormalizeUploadReportJobFilter(filter);
        var recentCutoff = DateTime.UtcNow.AddDays(-30);

        var jobRows = await db.IndorProveedorJobs
            .AsNoTracking()
            .Include(j => j.Cliente)
            .Where(j => j.ProveedorId == proveedor.Id && !j.IsDraft)
            .OrderByDescending(j => j.ScheduledAt ?? j.FechaCreacion)
            .ToListAsync(cancellationToken);

        var filtered = activeFilter switch
        {
            "completed" => jobRows.Where(j => j.Status == ProviderJobStatuses.Completed),
            "inprogress" => jobRows.Where(j => j.Status is ProviderJobStatuses.InProgress
                or ProviderJobStatuses.Scheduled
                or ProviderJobStatuses.Confirmed
                or ProviderJobStatuses.WaitingOnMaterials),
            "recent" => jobRows.Where(j => (j.ScheduledAt ?? j.FechaCreacion) >= recentCutoff),
            _ => jobRows.AsEnumerable()
        };

        var filteredList = filtered.ToList();
        var totalAvailable = filteredList.Count;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            filteredList = filteredList.Where(j =>
                j.Address.Contains(q, StringComparison.OrdinalIgnoreCase)
                || j.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                || j.JobCode.Contains(q, StringComparison.OrdinalIgnoreCase)
                || (j.ServiceType ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)
                || (j.Cliente?.Name ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var jobIds = filteredList.Select(j => j.Id).ToList();
        var photoCounts = await db.IndorProveedorReports
            .AsNoTracking()
            .Where(r => r.ProveedorId == proveedor.Id && r.JobId != null && jobIds.Contains(r.JobId.Value))
            .GroupBy(r => r.JobId!.Value)
            .Select(g => new { JobId = g.Key, Count = g.Sum(r => r.PhotosCount) })
            .ToDictionaryAsync(x => x.JobId, x => x.Count, cancellationToken);

        return new ProviderProUploadPhotosSelectJobViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            SearchQuery = search,
            ActiveFilter = activeFilter,
            TotalJobsAvailable = totalAvailable,
            HasSearchWithNoResults = !string.IsNullOrWhiteSpace(search) && filteredList.Count == 0,
            Jobs = filteredList.Select(j => new ProviderProUploadPhotosJobOptionViewModel
            {
                JobId = j.Id,
                Title = j.Title,
                Address = j.Address,
                ServiceType = j.ServiceType ?? j.Title,
                StatusLabel = MapJobStatusLabel(j.Status),
                StatusClass = MapJobStatusClass(j.Status),
                IconClass = MapServiceIcon(j.ServiceType ?? j.Title),
                ImageUrl = j.ImageUrl,
                PhotosCount = photoCounts.TryGetValue(j.Id, out var c) ? c : 0
            }).ToList()
        };
    }

    private async Task<ProviderProUploadPhotosJobSummary?> BuildUploadPhotosJobSummaryAsync(
        IndorProveedor proveedor,
        int jobId,
        CancellationToken cancellationToken)
    {
        var job = await db.IndorProveedorJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobId && j.ProveedorId == proveedor.Id, cancellationToken);

        if (job == null)
        {
            return null;
        }

        var existingPhotos = await db.IndorProveedorReports
            .AsNoTracking()
            .Where(r => r.ProveedorId == proveedor.Id && r.JobId == jobId)
            .SumAsync(r => (int?)r.PhotosCount, cancellationToken) ?? 0;

        return new ProviderProUploadPhotosJobSummary
        {
            JobId = job.Id,
            Title = job.Title,
            Address = job.Address,
            StatusLabel = MapJobStatusLabel(job.Status),
            StatusClass = MapJobStatusClass(job.Status),
            IconClass = MapServiceIcon(job.ServiceType ?? job.Title),
            ImageUrl = job.ImageUrl,
            ExistingPhotosCount = existingPhotos
        };
    }

    private static List<ProviderProUploadPhotosItem> MapUploadPhotosItems(ProviderProUploadPhotosDraft draft) =>
        draft.Photos
            .Select((p, i) => new ProviderProUploadPhotosItem
            {
                Index = i,
                Url = p.Url ?? "",
                Category = string.IsNullOrWhiteSpace(p.Slot) ? "After" : p.Slot
            })
            .Where(p => !string.IsNullOrWhiteSpace(p.Url))
            .ToList();

    public async Task<ProviderProUploadPhotosAddViewModel?> GetUploadPhotosAddAsync(
        IndorProveedor proveedor,
        ProviderProUploadPhotosDraft draft,
        CancellationToken cancellationToken = default)
    {
        if (!draft.JobId.HasValue)
        {
            return null;
        }

        var summary = await BuildUploadPhotosJobSummaryAsync(proveedor, draft.JobId.Value, cancellationToken);
        if (summary == null)
        {
            return null;
        }

        return new ProviderProUploadPhotosAddViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            Job = summary,
            NewPhotos = MapUploadPhotosItems(draft)
        };
    }

    public async Task<ProviderProUploadPhotosReviewViewModel?> GetUploadPhotosReviewAsync(
        IndorProveedor proveedor,
        ProviderProUploadPhotosDraft draft,
        CancellationToken cancellationToken = default)
    {
        if (!draft.JobId.HasValue)
        {
            return null;
        }

        var summary = await BuildUploadPhotosJobSummaryAsync(proveedor, draft.JobId.Value, cancellationToken);
        if (summary == null)
        {
            return null;
        }

        var items = MapUploadPhotosItems(draft);

        return new ProviderProUploadPhotosReviewViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            Job = summary,
            Photos = items,
            Notes = draft.Notes,
            NewCount = items.Count,
            TotalCount = summary.ExistingPhotosCount + items.Count
        };
    }

    public async Task<int?> SaveUploadPhotosFromDraftAsync(
        int proveedorId,
        ProviderProUploadPhotosDraft draft,
        CancellationToken cancellationToken = default)
    {
        var validPhotos = draft.Photos.Where(p => !string.IsNullOrWhiteSpace(p.Url)).ToList();
        if (!draft.JobId.HasValue || validPhotos.Count == 0)
        {
            return null;
        }

        var job = await db.IndorProveedorJobs
            .Include(j => j.Cliente)
            .FirstOrDefaultAsync(j => j.Id == draft.JobId && j.ProveedorId == proveedorId, cancellationToken);

        if (job == null)
        {
            return null;
        }

        var title = $"{(job.ServiceType ?? job.Title)} — Photo Update";
        if (title.Length > 150)
        {
            title = title[..150];
        }

        var report = new IndorProveedorReport
        {
            ProveedorId = proveedorId,
            JobId = job.Id,
            ClienteId = job.ClienteId,
            ReportCode = $"RPT-{DateTime.UtcNow:yyyyMMddHHmm}",
            Title = title,
            Address = job.Address,
            CustomerName = job.Cliente?.Name,
            ServiceType = job.ServiceType ?? job.Title,
            ReportType = ProviderReportTypes.Photo,
            InternalNotes = NullIfEmpty(draft.Notes),
            SendToHomeowner = true,
            RequestApproval = false,
            AttachToHouseFacts = false,
            AddedToHouseFacts = false,
            Status = ProviderReportStatuses.Ready,
            PhotosCount = validPhotos.Count,
            HasDocuments = false,
            HasWarranty = false,
            HasChecklist = false,
            PhotoUrlsJson = SerializeUploadReportFiles([], validPhotos),
            FilesCount = validPhotos.Count,
            CompletedUtc = DateTime.UtcNow,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorProveedorReports.Add(report);
        await db.SaveChangesAsync(cancellationToken);
        report.ReportCode = $"RPT-{1000 + report.Id}";
        await db.SaveChangesAsync(cancellationToken);
        return report.Id;
    }

    // ---------------------------------------------------------------
    // Report Templates (DB-backed catalog)
    // ---------------------------------------------------------------

    public async Task<ProviderProTemplatesPageViewModel> GetReportTemplatesAsync(
        int proveedorId,
        CancellationToken cancellationToken = default)
    {
        var templates = await db.IndorProveedorReportTemplates
            .AsNoTracking()
            .Include(t => t.Sections)
            .Where(t => t.Activo && (t.ProveedorId == null || t.ProveedorId == proveedorId))
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);

        var views = templates.Select(MapTemplateView).ToList();

        return new ProviderProTemplatesPageViewModel
        {
            MostUsed = views.Where(v => !v.IsCustom).ToList(),
            MyTemplates = views.Where(v => v.IsCustom).ToList()
        };
    }

    public async Task<ReportTemplateView?> GetReportTemplateAsync(
        int proveedorId,
        string key,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        var template = await db.IndorProveedorReportTemplates
            .AsNoTracking()
            .Include(t => t.Sections)
            .Where(t => t.Activo && (t.ProveedorId == null || t.ProveedorId == proveedorId))
            .FirstOrDefaultAsync(t => t.TemplateKey == key, cancellationToken);

        return template == null ? null : MapTemplateView(template);
    }

    private static ReportTemplateView MapTemplateView(IndorProveedorReportTemplate t) => new()
    {
        Id = t.Id,
        Key = t.TemplateKey,
        Name = t.Name,
        Description = t.Description ?? "",
        Icon = t.Icon,
        Color = t.Color,
        Badge = t.Badge,
        Category = t.Category,
        IsCustom = t.IsCustom,
        Sections = t.Sections
            .OrderBy(s => s.SortOrder)
            .Select(s => new ReportTemplateSectionView { Label = s.Label, Icon = s.Icon })
            .ToList()
    };

    // ---------------------------------------------------------------
    // Export Report (DB-backed save)
    // ---------------------------------------------------------------

    public Task<ProviderProUploadPhotosJobSummary?> GetExportJobSummaryAsync(
        IndorProveedor proveedor,
        int jobId,
        CancellationToken cancellationToken = default)
        => BuildUploadPhotosJobSummaryAsync(proveedor, jobId, cancellationToken);

    public async Task<int?> SaveExportReportFromDraftAsync(
        int proveedorId,
        ProviderProExportReportDraft draft,
        bool send,
        CancellationToken cancellationToken = default)
    {
        if (!draft.JobId.HasValue)
        {
            return null;
        }

        var job = await db.IndorProveedorJobs
            .Include(j => j.Cliente)
            .FirstOrDefaultAsync(j => j.Id == draft.JobId && j.ProveedorId == proveedorId, cancellationToken);

        if (job == null)
        {
            return null;
        }

        var photos = draft.Photos.Where(p => !string.IsNullOrWhiteSpace(p.Url)).ToList();

        var title = NullIfEmpty(draft.ReportName) ?? $"{(job.ServiceType ?? job.Title)} — Export Report";
        if (title.Length > 150)
        {
            title = title[..150];
        }

        DateOnly? reportDate = null;
        if (DateTime.TryParse(draft.ReportDate, out var parsedDate))
        {
            reportDate = DateOnly.FromDateTime(parsedDate);
        }

        var report = new IndorProveedorReport
        {
            ProveedorId = proveedorId,
            JobId = job.Id,
            ClienteId = job.ClienteId,
            ReportCode = $"RPT-{DateTime.UtcNow:yyyyMMddHHmm}",
            Title = title,
            Address = job.Address,
            CustomerName = job.Cliente?.Name,
            ServiceType = job.ServiceType ?? job.Title,
            ReportType = "Export Report",
            Summary = NullIfEmpty(draft.Description),
            InternalNotes = NullIfEmpty(draft.Notes),
            PreparedBy = NullIfEmpty(draft.PreparedBy),
            ReportDate = reportDate,
            Category = NullIfEmpty(draft.Category),
            LocationDetail = NullIfEmpty(draft.Location),
            Priority = NullIfEmpty(draft.Priority),
            Weather = NullIfEmpty(draft.Weather),
            SendToHomeowner = send,
            RequestApproval = false,
            AttachToHouseFacts = false,
            AddedToHouseFacts = false,
            Status = send ? ProviderReportStatuses.Ready : ProviderReportStatuses.Draft,
            PhotosCount = photos.Count,
            HasDocuments = false,
            HasWarranty = false,
            HasChecklist = false,
            PhotoUrlsJson = SerializeUploadReportFiles([], photos),
            FilesCount = photos.Count,
            CompletedUtc = send ? DateTime.UtcNow : null,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorProveedorReports.Add(report);
        await db.SaveChangesAsync(cancellationToken);

        if (photos.Count > 0)
        {
            var order = 0;
            foreach (var p in photos)
            {
                db.IndorProveedorReportPhotos.Add(new IndorProveedorReportPhoto
                {
                    ReportId = report.Id,
                    ProveedorId = proveedorId,
                    JobId = job.Id,
                    Category = string.IsNullOrWhiteSpace(p.Slot) ? "After" : p.Slot,
                    FileUrl = p.Url!,
                    FileName = p.FileName,
                    SortOrder = order++
                });
            }
        }

        report.ReportCode = $"RPT-{1000 + report.Id}";
        await db.SaveChangesAsync(cancellationToken);
        return report.Id;
    }

    private static int? ParseEmployeeCount(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (value.Trim().Equals("Just me", StringComparison.OrdinalIgnoreCase)) return 1;
        var digits = new string(value.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var n) ? n : null;
    }

    public async Task<int> SaveInsuranceQuoteAsync(
        int proveedorId,
        ProviderProInsuranceQuoteDraft draft,
        CancellationToken cancellationToken = default)
    {
        DateTime? dob = null;
        if (DateTime.TryParse(draft.OwnerDateOfBirth, out var parsedDob))
        {
            dob = parsedDob.Date;
        }

        var now = DateTime.UtcNow;
        var (payToday, monthly) = IndorMvcApp.ViewModels.InsuranceCatalog.Pricing(draft.Plan);

        var quote = new IndorProviderInsuranceQuote
        {
            ProveedorId = proveedorId,
            QuoteCode = $"INS-{now:yyyyMMddHHmm}",
            Plan = NullIfEmpty(draft.Plan),
            Coverages = draft.Coverages.Count > 0 ? string.Join(", ", draft.Coverages) : "General Liability",
            Trade = NullIfEmpty(draft.Trade),
            BusinessName = NullIfEmpty(draft.BusinessName),
            BusinessAddress = NullIfEmpty(draft.StreetAddress),
            City = NullIfEmpty(draft.City),
            State = NullIfEmpty(draft.State),
            ZipCode = NullIfEmpty(draft.ZipCode),
            OwnerName = NullIfEmpty(draft.OwnerName),
            OwnerDateOfBirth = dob,
            OwnerPhone = NullIfEmpty(draft.OwnerPhone),
            OwnerEmail = NullIfEmpty(draft.OwnerEmail),
            NumberOfEmployees = ParseEmployeeCount(draft.NumberOfEmployees),
            EmployeePayroll = draft.EmployeePayroll,
            CompanyGrossRevenue = draft.CompanyGrossRevenue,
            YearsInBusiness = NullIfEmpty(draft.YearsInBusiness),
            WorksAtCustomerHomes = draft.WorksAtCustomerHomes,
            UsesSubcontractors = draft.UsesSubcontractors,
            NeedsCOI = draft.NeedsCOI,
            PayTodayAmount = payToday,
            MonthlyAmount = monthly,
            PaymentMethod = NullIfEmpty(draft.PaymentMethod) ?? "Card",
            CardLast4 = NullIfEmpty(draft.CardLast4),
            AutoPayMonthly = draft.AutoPayMonthly,
            FirstBillingDate = DateTime.Today.AddDays(30),
            // Payment gateway is not integrated yet — always approve for now.
            PaymentStatus = "Paid",
            PaymentAuthorized = true,
            PaidUtc = now,
            ConfirmedAccurate = true,
            Status = "Pending Carrier Approval",
            SubmittedUtc = now,
            FechaCreacion = now
        };

        db.IndorProviderInsuranceQuotes.Add(quote);
        await db.SaveChangesAsync(cancellationToken);

        quote.QuoteCode = $"INS-{10000 + quote.Id}";
        quote.ReceiptNumber = $"IND-GL-{now:yyyy}-{quote.Id:00000}";
        await db.SaveChangesAsync(cancellationToken);
        return quote.Id;
    }

    public async Task<ProviderProMessagesInboxViewModel> GetMessagesInboxAsync(
        IndorProveedor proveedor,
        string? tab = "all",
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var activeTab = NormalizeMessagesTab(tab);
        var rows = await db.IndorProveedorConversations
            .AsNoTracking()
            .Include(c => c.Cliente)
            .Include(c => c.Job)
            .Include(c => c.Lead)
            .Where(c => c.ProveedorId == proveedor.Id)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync(cancellationToken);

        IEnumerable<IndorProveedorConversation> filtered = rows;
        filtered = activeTab switch
        {
            "jobs" => filtered.Where(c => c.Category == ProviderConversationCategories.Job),
            "leads" => filtered.Where(c => c.Category == ProviderConversationCategories.Lead),
            "unread" => filtered.Where(c => c.UnreadCount > 0),
            _ => filtered
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            filtered = filtered.Where(c =>
                ResolveConversationCustomerName(c).Contains(q, StringComparison.OrdinalIgnoreCase)
                || ResolveConversationAddress(c).Contains(q, StringComparison.OrdinalIgnoreCase)
                || (c.LastMessagePreview ?? "").Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var unreadCount = rows.Sum(c => c.UnreadCount);

        return new ProviderProMessagesInboxViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            ActiveTab = activeTab,
            SearchQuery = search,
            UnreadCount = unreadCount,
            Threads = filtered.Select(MapMessageThread).ToList(),
            FlowSteps = MessagesInboxFlowSteps()
        };
    }

    public async Task<ProviderProConversationViewModel?> GetConversationAsync(
        IndorProveedor proveedor,
        int conversationId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await db.IndorProveedorConversations
            .AsNoTracking()
            .Include(c => c.Cliente)
            .Include(c => c.Job)
            .Include(c => c.Lead)
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.ProveedorId == proveedor.Id, cancellationToken);

        if (conversation == null)
        {
            return null;
        }

        await MarkConversationReadAsync(conversationId, cancellationToken);

        var messages = await db.IndorProveedorMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.SentAt)
            .ToListAsync(cancellationToken);

        IndorProveedorEstimate? estimate = null;
        if (conversation.JobId.HasValue)
        {
            estimate = await db.IndorProveedorEstimates
                .AsNoTracking()
                .Where(e => e.ProveedorId == proveedor.Id && e.JobId == conversation.JobId)
                .OrderByDescending(e => e.FechaCreacion)
                .FirstOrDefaultAsync(cancellationToken);
        }
        else if (conversation.LeadId.HasValue)
        {
            estimate = await db.IndorProveedorEstimates
                .AsNoTracking()
                .Where(e => e.ProveedorId == proveedor.Id && e.LeadId == conversation.LeadId)
                .OrderByDescending(e => e.FechaCreacion)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var customerName = ResolveConversationCustomerName(conversation);

        return new ProviderProConversationViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            ConversationId = conversation.Id,
            CustomerName = customerName,
            CustomerInitials = BuildCustomerInitials(customerName),
            IsCustomerOnline = conversation.IsCustomerOnline,
            CustomerPhone = conversation.Cliente?.Phone ?? conversation.Lead?.CustomerPhone,
            JobId = conversation.JobId,
            JobCode = conversation.Job?.JobCode,
            JobTitle = conversation.Job?.Title,
            PropertyAddress = ResolveConversationAddress(conversation),
            EstimateId = estimate?.Id,
            EstimateCode = estimate?.EstimateCode,
            EstimateAmountLabel = estimate != null ? $"${estimate.Amount:N0}" : null,
            Messages = messages.Select(m => new ProviderProChatMessageViewModel
            {
                SenderType = m.SenderType,
                Body = m.Body,
                TimeLabel = FormatChatTimeLabel(m.SentAt),
                IsRead = m.IsRead
            }).ToList(),
            FlowSteps = ConversationFlowSteps(conversationId)
        };
    }

    public async Task<bool> SendConversationMessageAsync(
        int proveedorId,
        ProviderProSendMessageInput input,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input.Body))
        {
            return false;
        }

        var conversation = await db.IndorProveedorConversations
            .FirstOrDefaultAsync(c => c.Id == input.ConversationId && c.ProveedorId == proveedorId, cancellationToken);

        if (conversation == null)
        {
            return false;
        }

        var body = input.Body.Trim();
        var now = DateTime.UtcNow;
        db.IndorProveedorMessages.Add(new IndorProveedorMessage
        {
            ConversationId = conversation.Id,
            SenderType = ProviderMessageSenderTypes.Provider,
            Body = body,
            SentAt = now,
            IsRead = true
        });

        conversation.LastMessagePreview = TruncatePreview(body);
        conversation.LastMessageAt = now;
        conversation.Status = ProviderConversationStatuses.InProgress;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ProviderProMessageQuickActionsViewModel?> GetMessageQuickActionsAsync(
        IndorProveedor proveedor,
        int conversationId,
        string? selectedAction = null,
        CancellationToken cancellationToken = default)
    {
        var conversation = await db.IndorProveedorConversations
            .AsNoTracking()
            .Include(c => c.Cliente)
            .Include(c => c.Job)
            .Include(c => c.Lead)
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.ProveedorId == proveedor.Id, cancellationToken);

        if (conversation == null)
        {
            return null;
        }

        return new ProviderProMessageQuickActionsViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            ConversationId = conversation.Id,
            CustomerName = ResolveConversationCustomerName(conversation),
            JobTitle = conversation.Job?.Title ?? conversation.Lead?.ServiceType ?? "General inquiry",
            Address = ResolveConversationAddress(conversation),
            SelectedAction = selectedAction ?? "",
            Actions = BuildMessageQuickActionOptions(),
            FlowSteps = MessageQuickActionsFlowSteps(conversationId)
        };
    }

    public async Task<bool> SendMessageQuickActionAsync(
        int proveedorId,
        ProviderProMessageActionDraft draft,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(draft.ActionType))
        {
            return false;
        }

        var conversation = await db.IndorProveedorConversations
            .Include(c => c.Job)
            .FirstOrDefaultAsync(c => c.Id == draft.ConversationId && c.ProveedorId == proveedorId, cancellationToken);

        if (conversation == null)
        {
            return false;
        }

        var (body, attachmentType, attachmentLabel) = BuildQuickActionMessage(draft.ActionType, conversation);
        draft.ActionLabel = attachmentLabel;

        var now = DateTime.UtcNow;
        db.IndorProveedorMessages.Add(new IndorProveedorMessage
        {
            ConversationId = conversation.Id,
            SenderType = ProviderMessageSenderTypes.Provider,
            Body = body,
            SentAt = now,
            IsRead = true,
            AttachmentType = attachmentType,
            AttachmentLabel = attachmentLabel
        });

        conversation.LastMessagePreview = TruncatePreview(body);
        conversation.LastMessageAt = now;
        conversation.Status = ProviderConversationStatuses.InProgress;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ProviderProMessageSentSuccessViewModel?> GetMessageSentSuccessAsync(
        IndorProveedor proveedor,
        int conversationId,
        string actionLabel,
        CancellationToken cancellationToken = default)
    {
        var conversation = await db.IndorProveedorConversations
            .AsNoTracking()
            .Include(c => c.Job)
            .Include(c => c.Lead)
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.ProveedorId == proveedor.Id, cancellationToken);

        if (conversation == null)
        {
            return null;
        }

        return new ProviderProMessageSentSuccessViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            ConversationId = conversation.Id,
            JobId = conversation.JobId,
            CustomerName = ResolveConversationCustomerName(conversation),
            JobTitle = conversation.Job?.Title ?? conversation.Lead?.ServiceType ?? "General inquiry",
            SentItemLabel = string.IsNullOrWhiteSpace(actionLabel) ? "Update" : actionLabel,
            FlowSteps = MessageSentSuccessFlowSteps()
        };
    }

    public async Task<int> GetUnreadMessageCountAsync(int proveedorId, CancellationToken cancellationToken = default) =>
        await db.IndorProveedorConversations
            .AsNoTracking()
            .Where(c => c.ProveedorId == proveedorId)
            .SumAsync(c => c.UnreadCount, cancellationToken);

    private async Task MarkConversationReadAsync(int conversationId, CancellationToken cancellationToken)
    {
        var unreadMessages = await db.IndorProveedorMessages
            .Where(m => m.ConversationId == conversationId
                && !m.IsRead
                && m.SenderType == ProviderMessageSenderTypes.Customer)
            .ToListAsync(cancellationToken);

        if (unreadMessages.Count == 0)
        {
            var conversationOnly = await db.IndorProveedorConversations
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.UnreadCount > 0, cancellationToken);
            if (conversationOnly == null)
            {
                return;
            }

            conversationOnly.UnreadCount = 0;
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        var conversation = await db.IndorProveedorConversations
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
        if (conversation != null)
        {
            conversation.UnreadCount = 0;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static ProviderProMessageThreadViewModel MapMessageThread(IndorProveedorConversation conversation) =>
        new()
        {
            ConversationId = conversation.Id,
            CustomerName = ResolveConversationCustomerName(conversation),
            Address = ResolveConversationAddress(conversation),
            Preview = conversation.LastMessagePreview ?? "No messages yet",
            TimeLabel = FormatInboxTimeLabel(conversation.LastMessageAt),
            StatusLabel = MapConversationStatusLabel(conversation.Status),
            StatusClass = MapConversationStatusClass(conversation.Status),
            Category = conversation.Category,
            UnreadCount = conversation.UnreadCount,
            AvatarUrl = conversation.Cliente?.PhotoUrl
        };

    private static string ResolveConversationCustomerName(IndorProveedorConversation conversation) =>
        conversation.Cliente?.Name
        ?? conversation.Lead?.CustomerName
        ?? "Customer";

    private static string ResolveConversationAddress(IndorProveedorConversation conversation) =>
        conversation.Job?.Address
        ?? conversation.Lead?.Address
        ?? conversation.Cliente?.Address
        ?? conversation.Cliente?.CityState
        ?? "";

    private static string BuildCustomerInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return "CU";
        }

        if (parts.Length == 1)
        {
            return parts[0].Length >= 2
                ? parts[0][..2].ToUpperInvariant()
                : parts[0].ToUpperInvariant();
        }

        return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
    }

    private static string TruncatePreview(string text) =>
        text.Length <= 250 ? text : text[..247] + "...";

    private static string FormatInboxTimeLabel(DateTime when)
    {
        var local = when.Kind == DateTimeKind.Utc ? when.ToLocalTime() : when;
        if (local.Date == DateTime.Today)
        {
            return local.ToString("h:mm tt");
        }

        if (local.Date == DateTime.Today.AddDays(-1))
        {
            return "Yesterday";
        }

        return local.ToString("MMM d");
    }

    private static string FormatChatTimeLabel(DateTime when)
    {
        var local = when.Kind == DateTimeKind.Utc ? when.ToLocalTime() : when;
        return local.ToString("h:mm tt");
    }

    private static string MapConversationStatusLabel(string status) => status switch
    {
        ProviderConversationStatuses.Pending => "Pending",
        ProviderConversationStatuses.InProgress => "In Progress",
        _ => "New"
    };

    private static string MapConversationStatusClass(string status) => status switch
    {
        ProviderConversationStatuses.Pending => "pending",
        ProviderConversationStatuses.InProgress => "progress",
        _ => "new"
    };

    private static string NormalizeMessagesTab(string? tab) => (tab ?? "all").Trim().ToLowerInvariant() switch
    {
        "jobs" or "leads" or "unread" => tab!.Trim().ToLowerInvariant(),
        _ => "all"
    };

    private static List<ProviderProMessageQuickActionOptionViewModel> BuildMessageQuickActionOptions() =>
    [
        new()
        {
            ActionType = ProviderMessageActionTypes.Estimate,
            Label = "Send estimate",
            Description = "Share a pending or sent estimate",
            IconClass = "fa-file-invoice-dollar",
            ToneClass = "green"
        },
        new()
        {
            ActionType = ProviderMessageActionTypes.Invoice,
            Label = "Send invoice",
            Description = "Send payment request to homeowner",
            IconClass = "fa-file-invoice",
            ToneClass = "green"
        },
        new()
        {
            ActionType = ProviderMessageActionTypes.Visit,
            Label = "Schedule visit",
            Description = "Confirm or propose a visit time",
            IconClass = "fa-calendar-days",
            ToneClass = "blue"
        },
        new()
        {
            ActionType = ProviderMessageActionTypes.Report,
            Label = "Share report",
            Description = "Send a completed job report",
            IconClass = "fa-chart-column",
            ToneClass = "blue"
        },
        new()
        {
            ActionType = ProviderMessageActionTypes.Approval,
            Label = "Request approval",
            Description = "Ask homeowner to approve work",
            IconClass = "fa-shield-check",
            ToneClass = "blue"
        }
    ];

    private static (string Body, string AttachmentType, string AttachmentLabel) BuildQuickActionMessage(
        string actionType,
        IndorProveedorConversation conversation)
    {
        var jobTitle = conversation.Job?.Title ?? "your project";
        return actionType switch
        {
            ProviderMessageActionTypes.Estimate => (
                $"I've shared the estimate for {jobTitle}. Please review it when you have a moment.",
                ProviderMessageActionTypes.Estimate,
                "Estimate"),
            ProviderMessageActionTypes.Invoice => (
                $"Your invoice for {jobTitle} is ready. You can review and pay at your convenience.",
                ProviderMessageActionTypes.Invoice,
                "Invoice"),
            ProviderMessageActionTypes.Visit => (
                $"I'd like to schedule a visit for {jobTitle}. Let me know what time works best for you.",
                ProviderMessageActionTypes.Visit,
                "Scheduled visit"),
            ProviderMessageActionTypes.Report => (
                $"I've shared the final report for {jobTitle}. Take a look when you're ready.",
                ProviderMessageActionTypes.Report,
                "Final report"),
            ProviderMessageActionTypes.Approval => (
                $"Could you please review and approve the work completed for {jobTitle}?",
                ProviderMessageActionTypes.Approval,
                "Approval request"),
            _ => (
                $"Here's an update regarding {jobTitle}.",
                "update",
                "Update")
        };
    }

    private async Task<ProviderProUploadReportJobSummaryViewModel?> LoadUploadReportJobSummaryAsync(
        int proveedorId,
        int? jobId,
        CancellationToken cancellationToken)
    {
        if (!jobId.HasValue)
        {
            return null;
        }

        var job = await db.IndorProveedorJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobId && j.ProveedorId == proveedorId, cancellationToken);

        return job == null ? null : MapUploadReportJobSummary(job);
    }

    private static ProviderProUploadReportJobSummaryViewModel MapUploadReportJobSummary(IndorProveedorJob job) =>
        new()
        {
            JobId = job.Id,
            Title = $"{job.Address} – {job.ServiceType ?? job.Title}",
            Address = job.Address,
            ServiceType = job.ServiceType ?? job.Title,
            StatusLabel = MapJobStatusLabel(job.Status),
            StatusClass = MapJobStatusClass(job.Status),
            DateLabel = FormatUploadReportDate(job.CompletedAt ?? job.ScheduledAt ?? job.FechaCreacion),
            ImageUrl = job.ImageUrl,
            JobCode = job.JobCode,
            ScheduledLabel = job.ScheduledAt.HasValue
                ? $"Scheduled: {job.ScheduledAt.Value:MMM d, yyyy}"
                : ""
        };

    private static ProviderProUploadReportJobOptionViewModel MapUploadReportJobOption(IndorProveedorJob job) =>
        new()
        {
            JobId = job.Id,
            Address = job.Address,
            ServiceType = job.ServiceType ?? job.Title,
            DateLabel = FormatUploadReportDate(job.CompletedAt ?? job.ScheduledAt ?? job.FechaCreacion),
            StatusLabel = MapJobStatusLabel(job.Status),
            StatusClass = MapJobStatusClass(job.Status),
            IconClass = MapServiceIcon(job.ServiceType ?? job.Title),
            ImageUrl = job.ImageUrl,
            JobCode = job.JobCode
        };

    private static string FormatUploadReportDate(DateTime date) =>
        date.ToString("MMM d, yyyy");

    private static string NormalizeUploadReportJobFilter(string? filter) =>
        (filter ?? "all").Trim().ToLowerInvariant() switch
        {
            "completed" or "inprogress" or "recent" => filter!.Trim().ToLowerInvariant(),
            _ => "all"
        };

    private static string? SerializeUploadReportFiles(
        List<ProviderProUploadReportFileSlot> photoSlots,
        List<ProviderProUploadReportFileSlot> generalFiles)
    {
        var items = photoSlots
            .Where(s => !string.IsNullOrWhiteSpace(s.Url))
            .Select(s => new { label = s.Slot, url = s.Url, fileName = s.FileName })
            .Concat(generalFiles
                .Where(f => !string.IsNullOrWhiteSpace(f.Url))
                .Select(f => new { label = f.Slot, url = f.Url, fileName = f.FileName }))
            .ToList();

        return items.Count == 0 ? null : JsonSerializer.Serialize(items);
    }

    private static string? SerializeUploadReportDocuments(List<ProviderProUploadReportFileSlot> documentSlots)
    {
        var items = documentSlots
            .Where(s => !string.IsNullOrWhiteSpace(s.Url))
            .Select(s => new { label = s.Slot, url = s.Url, fileName = s.FileName })
            .ToList();

        return items.Count == 0 ? null : JsonSerializer.Serialize(items);
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public async Task<ProviderProCustomersPageViewModel> GetCustomersPageAsync(
        IndorProveedor proveedor,
        string? tab = "connected",
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var activeTab = NormalizeCustomersTab(tab);

        var customerRows = await db.IndorProveedorClientes
            .AsNoTracking()
            .Where(c => c.ProveedorId == proveedor.Id && c.Activo)
            .OrderByDescending(c => c.MemberSince)
            .ToListAsync(cancellationToken);

        var jobs = await db.IndorProveedorJobs
            .AsNoTracking()
            .Where(j => j.ProveedorId == proveedor.Id)
            .ToListAsync(cancellationToken);

        var approvals = await db.IndorProveedorApprovals
            .AsNoTracking()
            .Where(a => a.ProveedorId == proveedor.Id && a.Status == "Pending")
            .ToListAsync(cancellationToken);

        var customerIdsWithPendingApproval = approvals
            .Select(a => jobs.FirstOrDefault(j => j.Id == a.JobId)?.ClienteId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        var activeJobsCount = jobs.Count(j => j.Status is ProviderJobStatuses.InProgress
            or ProviderJobStatuses.Scheduled
            or ProviderJobStatuses.Confirmed
            or ProviderJobStatuses.WaitingOnMaterials);

        var connectedCount = customerRows.Count(c =>
            c.ConnectionStatus == ProviderCustomerConnectionStatuses.Connected);
        var needsInviteCount = customerRows.Count(c =>
            c.ConnectionStatus == ProviderCustomerConnectionStatuses.NeedsInvite);
        var propertiesCount = customerRows.Sum(c => Math.Max(1, c.PropertiesCount));

        var cards = customerRows
            .Select(c => MapCustomerCard(c, jobs.Where(j => j.ClienteId == c.Id).ToList(), customerIdsWithPendingApproval.Contains(c.Id)))
            .ToList();

        var activeHomesCount = cards.Count(c =>
            c.ShowJobSection && c.JobStatusClass is "progress" or "scheduled" or "confirmed");

        cards = activeTab switch
        {
            "connected" => cards.Where(c => c.ConnectionClass == "connected").ToList(),
            "needsinvite" => cards.Where(c => c.ConnectionClass == "needsinvite").ToList(),
            "activehomes" => cards.Where(c => c.ShowJobSection && c.JobStatusClass is "progress" or "scheduled" or "confirmed").ToList(),
            "approvals" => cards.Where(c => c.JobMetaLines.Any(m => m.Text.Contains("approval", StringComparison.OrdinalIgnoreCase))).ToList(),
            _ => cards
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            cards = cards.Where(c =>
                c.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                || c.Address.Contains(q, StringComparison.OrdinalIgnoreCase)
                || c.Phone.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return new ProviderProCustomersPageViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            ActiveTab = activeTab,
            SearchQuery = search,
            TotalCustomers = customerRows.Count,
            ConnectedCount = connectedCount,
            NeedsInviteCount = needsInviteCount,
            ActiveHomesCount = activeHomesCount,
            ActiveJobsCount = activeJobsCount,
            PendingApprovalCount = approvals.Count,
            PropertiesCount = propertiesCount,
            Customers = cards,
            ConnectionInsights = BuildConnectionInsights(needsInviteCount, approvals.Count, customerRows, jobs)
        };
    }

    private static ProviderProCustomerCardViewModel MapCustomerCard(
        IndorProveedorCliente customer,
        List<IndorProveedorJob> customerJobs,
        bool hasPendingApproval)
    {
        var isConnected = customer.ConnectionStatus == ProviderCustomerConnectionStatuses.Connected;
        var connectionLabel = isConnected ? "Connected" : "Needs Invite";
        var connectionClass = isConnected ? "connected" : "needsinvite";

        var displayAddress = !string.IsNullOrWhiteSpace(customer.Address)
            ? customer.Address
            : customer.CityState ?? "";

        var activeJob = customerJobs
            .Where(j => j.Status is ProviderJobStatuses.InProgress
                or ProviderJobStatuses.Scheduled
                or ProviderJobStatuses.Confirmed
                or ProviderJobStatuses.WaitingOnMaterials)
            .OrderByDescending(j => j.ScheduledAt ?? j.FechaCreacion)
            .FirstOrDefault();

        var recentCompleted = customerJobs
            .Where(j => j.Status == ProviderJobStatuses.Completed)
            .OrderByDescending(j => j.FechaActualizacion ?? j.FechaCreacion)
            .FirstOrDefault();

        var card = new ProviderProCustomerCardViewModel
        {
            Name = customer.Name,
            Address = displayAddress,
            Phone = customer.Phone ?? "",
            ConnectionLabel = connectionLabel,
            ConnectionClass = connectionClass,
            IsPropertyVerified = customer.IsPropertyVerified,
            IsAppConnected = customer.IsAppConnected,
            PropertiesCount = Math.Max(1, customer.PropertiesCount),
            ActivityNote = customer.LastActivityNote,
            ShowPhotosLink = isConnected,
            PrimaryAction = isConnected ? "View Customer" : "Send Invite",
            PrimaryActionClass = isConnected ? "primary" : "primary",
            SecondaryAction = isConnected ? "Open Property" : "View Record"
        };

        if (activeJob != null)
        {
            card.ShowJobSection = true;
            card.JobTitle = activeJob.Title;
            card.JobStatusLabel = activeJob.Status == ProviderJobStatuses.InProgress ? "In Progress" : "Active Job";
            card.JobStatusClass = activeJob.Status == ProviderJobStatuses.InProgress ? "progress" : "scheduled";
            card.JobMetaLines = BuildCustomerJobMeta(activeJob, customer, hasPendingApproval);

            if (activeJob.Status == ProviderJobStatuses.InProgress)
            {
                card.PrimaryAction = "Continue Job";
                card.PrimaryActionClass = "success";
                card.SecondaryAction = "Message";
            }
        }
        else if (recentCompleted != null)
        {
            card.ShowJobSection = true;
            card.JobTitle = recentCompleted.Title;
            card.JobStatusLabel = "Recent Work";
            card.JobStatusClass = "completed";
            card.JobMetaLines =
            [
                new ProviderProJobMetaLineViewModel
                {
                    Text = "Completed",
                    IconClass = "fa-circle-check",
                    Tone = "success"
                }
            ];

            if (customer.HouseFactsCount > 0)
            {
                card.JobMetaLines.Add(new ProviderProJobMetaLineViewModel
                {
                    Text = $"House Facts: {customer.HouseFactsCount} records",
                    IconClass = "fa-house",
                    Tone = "neutral"
                });
            }

            card.PrimaryAction = "View Record";
            card.PrimaryActionClass = "ghost";
            card.SecondaryAction = "Message";
        }
        else if (!isConnected)
        {
            card.ShowJobSection = false;
            if (!string.IsNullOrWhiteSpace(customer.LastActivityNote))
            {
                card.ActivityNote = customer.LastActivityNote;
            }
        }

        return card;
    }

    private static List<ProviderProJobMetaLineViewModel> BuildCustomerJobMeta(
        IndorProveedorJob job,
        IndorProveedorCliente customer,
        bool hasPendingApproval)
    {
        var meta = new List<ProviderProJobMetaLineViewModel>();

        if (job.ScheduledAt.HasValue)
        {
            meta.Add(new ProviderProJobMetaLineViewModel
            {
                Text = $"Scheduled — {FormatTimeLabel(job.ScheduledAt)}",
                IconClass = "fa-calendar",
                Tone = "neutral"
            });
        }

        if (hasPendingApproval)
        {
            meta.Add(new ProviderProJobMetaLineViewModel
            {
                Text = "Pending homeowner approval",
                IconClass = "fa-clock",
                Tone = "warning"
            });
        }

        if (!string.IsNullOrWhiteSpace(job.ChecklistStatus))
        {
            meta.Add(new ProviderProJobMetaLineViewModel
            {
                Text = job.ChecklistStatus!,
                IconClass = "fa-list-check",
                Tone = job.ChecklistStatus.Contains("pending", StringComparison.OrdinalIgnoreCase) ? "warning" : "success"
            });
        }

        if (job.PhotosCount == 0 && job.Status == ProviderJobStatuses.InProgress)
        {
            meta.Add(new ProviderProJobMetaLineViewModel
            {
                Text = "Before photos missing",
                IconClass = "fa-triangle-exclamation",
                Tone = "danger"
            });
        }
        else if (job.PhotosCount > 0)
        {
            meta.Add(new ProviderProJobMetaLineViewModel
            {
                Text = $"{job.PhotosCount} photo{(job.PhotosCount == 1 ? "" : "s")} uploaded",
                IconClass = "fa-camera",
                Tone = "neutral"
            });
        }

        if (customer.HouseFactsCount > 0)
        {
            meta.Add(new ProviderProJobMetaLineViewModel
            {
                Text = $"House Facts: {customer.HouseFactsCount} records",
                IconClass = "fa-house",
                Tone = "neutral"
            });
        }

        return meta;
    }

    private static List<ProviderProSmartSuggestionViewModel> BuildConnectionInsights(
        int needsInviteCount,
        int pendingApprovals,
        List<IndorProveedorCliente> customers,
        List<IndorProveedorJob> jobs)
    {
        var insights = new List<ProviderProSmartSuggestionViewModel>();

        if (pendingApprovals > 0)
        {
            insights.Add(new ProviderProSmartSuggestionViewModel
            {
                Text = $"{pendingApprovals} customer record{(pendingApprovals == 1 ? "" : "s")} waiting for approval",
                IconClass = "fa-clock",
                Tone = "orange"
            });
        }

        if (needsInviteCount > 0)
        {
            insights.Add(new ProviderProSmartSuggestionViewModel
            {
                Text = $"{needsInviteCount} customer{(needsInviteCount == 1 ? "" : "s")} can be invited to claim their property",
                IconClass = "fa-user-plus",
                Tone = "blue"
            });
        }

        var maintenanceDue = customers.Count(c =>
            c.ConnectionStatus == ProviderCustomerConnectionStatuses.Connected
            && jobs.Any(j => j.ClienteId == c.Id && j.Status == ProviderJobStatuses.Completed));

        if (maintenanceDue > 0)
        {
            insights.Add(new ProviderProSmartSuggestionViewModel
            {
                Text = $"{maintenanceDue} home{(maintenanceDue == 1 ? "" : "s")} due for maintenance follow-up",
                IconClass = "fa-house-medical",
                Tone = "teal"
            });
        }

        return insights;
    }

    private static string NormalizeCustomersTab(string? tab) => (tab ?? "connected").Trim().ToLowerInvariant() switch
    {
        "all" or "needsinvite" or "activehomes" or "approvals" => tab!.Trim().ToLowerInvariant(),
        _ => "connected"
    };

    public async Task<ProviderProReportsPageViewModel> GetReportsPageAsync(
        IndorProveedor proveedor,
        string? tab = "all",
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var activeTab = NormalizeReportsTab(tab);

        var reportRows = await db.IndorProveedorReports
            .AsNoTracking()
            .Include(r => r.Cliente)
            .Include(r => r.Job)
            .Where(r => r.ProveedorId == proveedor.Id)
            .OrderByDescending(r => r.CompletedUtc ?? r.FechaCreacion)
            .ToListAsync(cancellationToken);

        var draftCount = reportRows.Count(r => r.Status == ProviderReportStatuses.Draft);
        var readyCount = reportRows.Count(r => r.Status == ProviderReportStatuses.Ready);
        var approvalCount = reportRows.Count(r => r.Status == ProviderReportStatuses.Approval);
        var approvedCount = reportRows.Count(r => r.Status == ProviderReportStatuses.Approved);
        var houseFactsCount = reportRows.Count(r =>
            r.Status == ProviderReportStatuses.HouseFacts || r.AddedToHouseFacts);

        var cards = reportRows.Select(MapReportCard).ToList();

        cards = activeTab switch
        {
            "draft" => cards.Where(c => c.StatusClass == "draft").ToList(),
            "ready" => cards.Where(c => c.StatusClass == "ready").ToList(),
            "approval" => cards.Where(c => c.StatusClass == "approval").ToList(),
            "approved" => cards.Where(c => c.StatusClass == "approved").ToList(),
            "housefacts" => cards.Where(c => c.StatusClass == "housefacts").ToList(),
            _ => cards
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            cards = cards.Where(c =>
                c.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                || c.Address.Contains(q, StringComparison.OrdinalIgnoreCase)
                || c.CustomerJobLabel.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return new ProviderProReportsPageViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            ActiveTab = activeTab,
            SearchQuery = search,
            DraftCount = draftCount,
            ReadyCount = readyCount,
            ApprovalCount = approvalCount,
            ApprovedCount = approvedCount,
            HouseFactsCount = houseFactsCount,
            Reports = cards,
            ReportInsights = BuildReportInsights(reportRows)
        };
    }

    private static ProviderProReportCardViewModel MapReportCard(IndorProveedorReport report)
    {
        var customerName = report.CustomerName ?? report.Cliente?.Name ?? "Customer";
        var (statusLabel, statusClass, actionLabel, actionClass) = MapReportStatus(report.Status, report.AddedToHouseFacts);

        return new ProviderProReportCardViewModel
        {
            Title = report.Title,
            Address = report.Address,
            CustomerJobLabel = $"{customerName} • Job {report.ReportCode}",
            CompletedLabel = report.CompletedUtc.HasValue
                ? $"Completed {report.CompletedUtc.Value.ToLocalTime():MMM d, yyyy}"
                : "In progress",
            StatusLabel = statusLabel,
            StatusClass = statusClass,
            IconClass = MapServiceIcon(report.ServiceType ?? report.Title),
            PhotosCount = report.PhotosCount,
            HasChecklist = report.HasChecklist,
            HasWarranty = report.HasWarranty,
            HasDocuments = report.HasDocuments,
            ActionLabel = actionLabel,
            ActionClass = actionClass
        };
    }

    private static (string Label, string Class, string Action, string ActionClass) MapReportStatus(string status, bool addedToHouseFacts)
    {
        if (addedToHouseFacts || status == ProviderReportStatuses.HouseFacts)
        {
            return ("Added to House Facts", "housefacts", "View in House Facts", "ghost");
        }

        return status switch
        {
            ProviderReportStatuses.Ready => ("Ready to Send", "ready", "Send to Homeowner", "primary"),
            ProviderReportStatuses.Approval => ("Awaiting Approval", "approval", "View Approval", "ghost"),
            ProviderReportStatuses.Approved => ("Approved", "approved", "View Record", "ghost"),
            _ => ("Draft", "draft", "Continue Report", "primary")
        };
    }

    private static List<ProviderProSmartSuggestionViewModel> BuildReportInsights(List<IndorProveedorReport> reports)
    {
        var insights = new List<ProviderProSmartSuggestionViewModel>();

        var awaitingApproval = reports.Count(r => r.Status == ProviderReportStatuses.Approval);
        if (awaitingApproval > 0)
        {
            insights.Add(new ProviderProSmartSuggestionViewModel
            {
                Text = $"{awaitingApproval} report{(awaitingApproval == 1 ? "" : "s")} need homeowner approval",
                IconClass = "fa-clock",
                Tone = "orange"
            });
        }

        var missingPhotos = reports.Count(r => r.Status == ProviderReportStatuses.Draft && r.PhotosCount == 0);
        if (missingPhotos > 0)
        {
            insights.Add(new ProviderProSmartSuggestionViewModel
            {
                Text = $"{missingPhotos} draft report{(missingPhotos == 1 ? "" : "s")} {(missingPhotos == 1 ? "is" : "are")} missing photos",
                IconClass = "fa-camera",
                Tone = "blue"
            });
        }

        var approvedCount = reports.Count(r => r.Status == ProviderReportStatuses.Approved);
        if (approvedCount > 0)
        {
            insights.Add(new ProviderProSmartSuggestionViewModel
            {
                Text = $"{approvedCount} approved report{(approvedCount == 1 ? "" : "s")} improved your Provider Score",
                IconClass = "fa-chart-line",
                Tone = "teal"
            });
        }

        return insights;
    }

    private static string NormalizeReportsTab(string? tab) => (tab ?? "all").Trim().ToLowerInvariant() switch
    {
        "draft" or "ready" or "approval" or "approved" or "housefacts" => tab!.Trim().ToLowerInvariant(),
        _ => "all"
    };

    public async Task<ProviderProProfilePageViewModel> GetProfilePageAsync(
        IndorProveedor proveedor,
        CancellationToken cancellationToken = default)
    {
        var isApproved = string.Equals(proveedor.RegistrationStatus, ProviderRegistrationStatuses.Approved, StringComparison.OrdinalIgnoreCase);
        var isProActive = isApproved
            || string.Equals(proveedor.RegistrationStatus, ProviderRegistrationStatuses.IndorProActive, StringComparison.OrdinalIgnoreCase);

        var categories = proveedor.Categorias
            .Where(c => c.Categoria != null)
            .Select(c => c.Categoria!.LabelEn)
            .OrderBy(l => l)
            .ToList();

        var services = proveedor.Ofertas
            .Where(o => o.Oferta != null)
            .Select(o => o.Oferta!.LabelEn)
            .OrderBy(l => l)
            .ToList();

        var providerScore = proveedor.ExamPassed == true && proveedor.ExamScorePercent.HasValue
            ? proveedor.ExamScorePercent.Value
            : isProActive ? 82 : 0;

        var yearStart = new DateTime(DateTime.Today.Year, 1, 1);

        var jobs = await db.IndorProveedorJobs
            .AsNoTracking()
            .Where(j => j.ProveedorId == proveedor.Id)
            .ToListAsync(cancellationToken);

        var reports = await db.IndorProveedorReports
            .AsNoTracking()
            .Where(r => r.ProveedorId == proveedor.Id)
            .ToListAsync(cancellationToken);

        var invoices = await db.IndorProveedorInvoices
            .AsNoTracking()
            .Where(i => i.ProveedorId == proveedor.Id)
            .ToListAsync(cancellationToken);

        var customerCount = await db.IndorProveedorClientes
            .AsNoTracking()
            .CountAsync(c => c.ProveedorId == proveedor.Id && c.Activo, cancellationToken);

        var completedJobs = jobs.Count(j => j.Status == ProviderJobStatuses.Completed);
        var jobsThisYear = jobs.Count(j =>
            j.Status == ProviderJobStatuses.Completed
            && (j.FechaActualizacion ?? j.FechaCreacion) >= yearStart);

        var houseFactsAdded = reports.Count(r =>
            r.AddedToHouseFacts || r.Status == ProviderReportStatuses.HouseFacts);

        var homeRecordsCreated = reports.Count(r =>
            r.Status is ProviderReportStatuses.Approved or ProviderReportStatuses.HouseFacts);

        var completionRate = jobs.Count > 0
            ? $"{(int)Math.Round(completedJobs * 100m / jobs.Count)}%"
            : "—";

        var nextPayout = invoices
            .Where(i => i.Status == ProviderInvoiceStatuses.Pending && i.DueDate.HasValue)
            .OrderBy(i => i.DueDate)
            .FirstOrDefault();

        var serviceAreas = BuildServiceAreas(proveedor);
        var specialties = categories.Count > 0
            ? string.Join(" • ", categories.Take(4))
            : services.Count > 0 ? string.Join(" • ", services.Take(4)) : "Services pending";

        var teamMembers = BuildTeamMembers(proveedor);

        var companyName = ResolveCompanyName(proveedor);
        var logoUrl = ResolveProviderLogoUrl(proveedor);

        return new ProviderProProfilePageViewModel
        {
            CompanyName = companyName,
            LogoUrl = logoUrl,
            CompanyInitial = BuildCompanyInitial(companyName),
            BusinessName = proveedor.BusinessName ?? "",
            DbaName = proveedor.DbaName ?? "",
            PrimaryContact = proveedor.PrimaryContact ?? "",
            Email = proveedor.Email ?? "",
            Phone = proveedor.Phone ?? "",
            BusinessAddress = proveedor.BusinessAddress ?? "",
            PrimaryCity = proveedor.PrimaryCity ?? "",
            RegistrationStatus = proveedor.RegistrationStatus,
            IsApproved = isApproved,
            IsVerified = isApproved || (proveedor.ExamPassed == true && proveedor.Documentos.Count > 0),
            IsTopRated = providerScore >= 85 && isApproved,
            SpecialtiesSummary = specialties,
            ServiceAreaLabel = BuildServiceAreaLabel(proveedor),
            ProviderScore = providerScore,
            Rating = 0,
            ReviewCount = 0,
            JobsCompletedThisYear = jobsThisYear,
            TotalJobsCompleted = completedJobs,
            HomeRecordsCreated = homeRecordsCreated,
            YearsActiveLabel = BuildYearsActiveLabel(proveedor.FechaCreacion),
            BusinessHours = string.IsNullOrWhiteSpace(proveedor.PreferredHours)
                ? "Hours not set"
                : proveedor.PreferredHours!,
            Website = "",
            TravelRadiusMiles = proveedor.TravelRadiusMiles,
            EmergencyService = proveedor.EmergencyService,
            SameDayJobs = proveedor.SameDayJobs,
            Categories = categories,
            Services = services,
            ServiceAreas = serviceAreas,
            VerificationItems = BuildVerificationItems(proveedor),
            PayoutConnected = invoices.Any(i => i.Status == ProviderInvoiceStatuses.Paid),
            PaymentProcessingActive = isProActive,
            NextPayoutAmount = nextPayout?.Amount ?? invoices.Where(i => i.Status == ProviderInvoiceStatuses.Pending).Sum(i => i.Amount),
            NextPayoutDateLabel = nextPayout?.DueDate?.ToLocalTime().ToString("MMM d, yyyy"),
            TeamMembers = teamMembers,
            AutoRemindersOn = proveedor.EmergencyService || proveedor.SameDayJobs,
            ReportTemplatesCount = reports.Count(r => r.Status != ProviderReportStatuses.Draft),
            FollowUpCampaignsActive = customerCount > 0,
            AiAssistantEnabled = isProActive,
            Performance = new ProviderProProfilePerformanceViewModel
            {
                AvgResponseTime = "—",
                CompletionRate = completionRate,
                HomeownerApproval = reports.Count(r => r.Status == ProviderReportStatuses.Approved) > 0
                    ? $"{reports.Count(r => r.Status == ProviderReportStatuses.Approved)} approved"
                    : "—",
                HouseFactsAdded = houseFactsAdded
            },
            Reviews = [],
            SubscriptionPlan = isProActive ? "Pro Plan" : "Activation Pending",
            DocumentsUploaded = proveedor.Documentos.Count(d => !string.IsNullOrWhiteSpace(d.FileUrl)),
            DocumentsRequired = proveedor.Documentos.Count
        };
    }

    public Task<ProviderProEditProfileViewModel> GetEditProfileAsync(
        IndorProveedor proveedor,
        ProviderProEditProfileInput? input = null,
        CancellationToken cancellationToken = default)
    {
        var companyName = input != null
            ? ResolveCompanyNameFromInput(input)
            : ResolveCompanyName(proveedor);

        return Task.FromResult(new ProviderProEditProfileViewModel
        {
            CompanyName = companyName,
            LogoUrl = ResolveProviderLogoUrl(proveedor),
            CompanyInitial = BuildCompanyInitial(companyName),
            BusinessName = input?.BusinessName?.Trim() ?? proveedor.BusinessName ?? "",
            DbaName = input?.DbaName?.Trim() ?? proveedor.DbaName ?? "",
            PrimaryContact = input?.PrimaryContact?.Trim() ?? proveedor.PrimaryContact ?? "",
            Phone = input?.Phone?.Trim() ?? proveedor.Phone ?? "",
            Email = input?.Email?.Trim() ?? proveedor.Email ?? "",
            BusinessAddress = input?.BusinessAddress?.Trim() ?? proveedor.BusinessAddress ?? "",
            PrimaryCity = input?.PrimaryCity?.Trim() ?? proveedor.PrimaryCity ?? "",
            ServiceZipCodes = input?.ServiceZipCodes?.Trim() ?? FormatZipNeighborhoods(proveedor.ZipNeighborhoodsJson),
            PreferredHours = input?.PreferredHours?.Trim() ?? proveedor.PreferredHours ?? "",
            ServiceDescription = input?.ServiceDescription?.Trim() ?? proveedor.ServiceDescription ?? "",
            TravelRadiusMiles = input?.TravelRadiusMiles > 0
                ? input.TravelRadiusMiles
                : proveedor.TravelRadiusMiles > 0 ? proveedor.TravelRadiusMiles : 25,
            EmergencyService = input?.EmergencyService ?? proveedor.EmergencyService,
            SameDayJobs = input?.SameDayJobs ?? proveedor.SameDayJobs
        });
    }

    public async Task<bool> SaveEditProfileAsync(
        int proveedorId,
        ProviderProEditProfileInput input,
        CancellationToken cancellationToken = default)
    {
        await ProviderDatabaseSchemaInitializer.EnsureEditProfileColumnsAsync(db, logger, cancellationToken);

        var entity = await db.IndorProveedores
            .FirstOrDefaultAsync(p => p.Id == proveedorId, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        entity.BusinessName = TrimOrEmpty(input.BusinessName);
        entity.DbaName = TrimOrEmpty(input.DbaName);
        entity.PrimaryContact = TrimOrEmpty(input.PrimaryContact);
        entity.Phone = UsPhoneOptionalAttribute.NormalizeToStorage(input.Phone) ?? "";
        entity.Email = TrimOrEmpty(input.Email);
        entity.BusinessAddress = TrimOrEmpty(input.BusinessAddress);
        entity.PrimaryCity = TrimOrEmpty(input.PrimaryCity);
        entity.ZipNeighborhoodsJson = ParseZipNeighborhoodsJson(input.ServiceZipCodes);
        entity.PreferredHours = TrimOrEmpty(input.PreferredHours);
        entity.ServiceDescription = TrimOrEmpty(input.ServiceDescription);
        entity.TravelRadiusMiles = input.TravelRadiusMiles > 0 ? input.TravelRadiusMiles : entity.TravelRadiusMiles;
        entity.EmergencyService = input.EmergencyService;
        entity.SameDayJobs = input.SameDayJobs;
        entity.FechaActualizacion = DateTime.UtcNow;

        try
        {
            await ProviderGeolocationHelper.ApplyGeocodeAsync(entity, addressLookup, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Geocoding skipped for provider {ProviderId} during profile save.", proveedorId);
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(
                ex,
                "Failed to save provider profile for provider {ProviderId}. {Detail}",
                proveedorId,
                ex.InnerException?.Message ?? ex.Message);
            return false;
        }
    }

    public async Task<ProviderProEditProfileServicesViewModel> GetEditProfileServicesAsync(
        IndorProveedor proveedor,
        CancellationToken cancellationToken = default)
    {
        var companyName = ResolveCompanyName(proveedor);
        var selectedCategories = proveedor.Categorias
            .Select(c => c.CategoriaId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var selectedServices = proveedor.Ofertas
            .Select(o => o.OfertaId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var categories = await db.IndorProveedorCategoriasCatalogo
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.SortOrder)
            .Select(c => new ProviderProEditProfileServiceOptionViewModel
            {
                Id = c.Id,
                Label = c.LabelEn,
                IconClass = c.IconClass.StartsWith("fa-") ? c.IconClass : $"fa-{c.IconClass}",
                IsCategory = true,
                IsSelected = selectedCategories.Contains(c.Id)
            })
            .ToListAsync(cancellationToken);

        var services = await db.IndorProveedorOfertasCatalogo
            .AsNoTracking()
            .Where(o => o.Activo)
            .OrderBy(o => o.SortOrder)
            .Select(o => new ProviderProEditProfileServiceOptionViewModel
            {
                Id = o.Id,
                Label = o.LabelEn,
                IconClass = o.IconClass.StartsWith("fa-") ? o.IconClass : $"fa-{o.IconClass}",
                IsCategory = false,
                IsSelected = selectedServices.Contains(o.Id)
            })
            .ToListAsync(cancellationToken);

        if (categories.Count == 0)
        {
            categories = OnboardingCatalog.ProviderCategories
                .Select(c => new ProviderProEditProfileServiceOptionViewModel
                {
                    Id = c.Id,
                    Label = c.Label,
                    IconClass = c.IconClass,
                    IsCategory = true,
                    IsSelected = selectedCategories.Contains(c.Id)
                })
                .ToList();
        }

        return new ProviderProEditProfileServicesViewModel
        {
            CompanyName = companyName,
            CompanyInitial = BuildCompanyInitial(companyName),
            Options = categories.Concat(services).ToList()
        };
    }

    public async Task<bool> SaveEditProfileServicesAsync(
        int proveedorId,
        IReadOnlyList<string> selectedIds,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.IndorProveedores
            .Include(p => p.Categorias)
            .Include(p => p.Ofertas)
            .FirstOrDefaultAsync(p => p.Id == proveedorId, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        var categoryIds = await db.IndorProveedorCategoriasCatalogo
            .AsNoTracking()
            .Where(c => c.Activo)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (categoryIds.Count == 0)
        {
            categoryIds = OnboardingCatalog.ProviderCategories.Select(c => c.Id).ToList();
        }

        var serviceIds = await db.IndorProveedorOfertasCatalogo
            .AsNoTracking()
            .Where(o => o.Activo)
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        var categorySet = categoryIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var serviceSet = serviceIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var desired = selectedIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        SyncCategoriesOnEntity(entity, desired.Where(id => categorySet.Contains(id)).ToList());
        SyncOfertasOnEntity(entity, desired.Where(id => serviceSet.Contains(id)).ToList());
        entity.FechaActualizacion = DateTime.UtcNow;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public Task<ProviderProEditProfileVerificationViewModel> GetEditProfileVerificationAsync(
        IndorProveedor proveedor,
        CancellationToken cancellationToken = default)
    {
        var companyName = ResolveCompanyName(proveedor);
        var docs = proveedor.Documentos;
        bool HasDoc(string type) =>
            docs.Any(d => string.Equals(d.DocumentType, type, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(d.FileUrl));

        string? GetDocUrl(string type) =>
            docs.FirstOrDefault(d => string.Equals(d.DocumentType, type, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(d.FileUrl))?.FileUrl;

        var slots = new[]
        {
            (ProviderDocumentTypes.License, "Trade / business license"),
            (ProviderDocumentTypes.Insurance, "Insurance certificate"),
            (ProviderDocumentTypes.GovernmentId, "Government-issued ID"),
            (ProviderDocumentTypes.W9, "W-9 tax form")
        }.Select(slot => new ProviderProEditProfileDocumentSlotViewModel
        {
            DocumentType = slot.Item1,
            Label = slot.Item2,
            FileUrl = GetDocUrl(slot.Item1),
            IsUploaded = HasDoc(slot.Item1)
        }).ToList();

        return Task.FromResult(new ProviderProEditProfileVerificationViewModel
        {
            CompanyName = companyName,
            CompanyInitial = BuildCompanyInitial(companyName),
            Items = BuildVerificationItems(proveedor),
            DocumentSlots = slots
        });
    }

    public async Task ApplyVerificationDocumentFlagsAsync(
        int proveedorId,
        string documentType,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.IndorProveedores
            .FirstOrDefaultAsync(p => p.Id == proveedorId, cancellationToken);

        if (entity == null)
        {
            return;
        }

        if (documentType.Equals(ProviderDocumentTypes.License, StringComparison.OrdinalIgnoreCase)
            || documentType.Equals(ProviderDocumentTypes.ContractorLicense, StringComparison.OrdinalIgnoreCase)
            || documentType.Equals(ProviderDocumentTypes.HvacLicense, StringComparison.OrdinalIgnoreCase)
            || documentType.Equals(ProviderDocumentTypes.PlumbingLicense, StringComparison.OrdinalIgnoreCase))
        {
            entity.IsLicensed = true;
        }

        if (documentType.Equals(ProviderDocumentTypes.Insurance, StringComparison.OrdinalIgnoreCase)
            || documentType.Equals(ProviderDocumentTypes.LiabilityInsurance, StringComparison.OrdinalIgnoreCase))
        {
            entity.IsInsured = true;
        }

        entity.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void SyncCategoriesOnEntity(IndorProveedor entity, List<string> ids)
    {
        var desired = ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var sel in entity.Categorias.ToList())
        {
            if (!desired.Contains(sel.CategoriaId))
            {
                entity.Categorias.Remove(sel);
            }
        }

        var existing = entity.Categorias
            .Select(c => c.CategoriaId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var id in desired.Where(id => !existing.Contains(id)))
        {
            entity.Categorias.Add(new IndorProveedorCategoriaSel { CategoriaId = id });
        }
    }

    private static void SyncOfertasOnEntity(IndorProveedor entity, List<string> ids)
    {
        var desired = ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var sel in entity.Ofertas.ToList())
        {
            if (!desired.Contains(sel.OfertaId))
            {
                entity.Ofertas.Remove(sel);
            }
        }

        var existing = entity.Ofertas
            .Select(o => o.OfertaId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var id in desired.Where(id => !existing.Contains(id)))
        {
            entity.Ofertas.Add(new IndorProveedorOfertaSel { OfertaId = id });
        }
    }

    private static string FormatZipNeighborhoods(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return "";
        }

        try
        {
            var zips = JsonSerializer.Deserialize<List<string>>(json);
            return zips == null ? "" : string.Join(", ", zips.Where(z => !string.IsNullOrWhiteSpace(z)));
        }
        catch
        {
            return "";
        }
    }

    private static string? ParseZipNeighborhoodsJson(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var zips = value
            .Split([',', ';', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(z => !string.IsNullOrWhiteSpace(z))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return zips.Count == 0 ? null : JsonSerializer.Serialize(zips);
    }

    public async Task<ProviderProNotificationsViewModel> GetNotificationsPageAsync(
        IndorProveedor proveedor,
        CancellationToken cancellationToken = default)
    {
        var meta = ReadOnboardingMeta(proveedor.OnboardingMetaJson);
        var companyName = ResolveCompanyName(proveedor);
        return new ProviderProNotificationsViewModel
        {
            CompanyName = companyName,
            CompanyInitial = BuildCompanyInitial(companyName),
            NotifyJobAlerts = meta.NotifyJobAlerts,
            NotifyLeadUpdates = meta.NotifyLeadUpdates,
            NotifyPaymentAlerts = meta.NotifyPaymentAlerts,
            NotifyReportReminders = meta.NotifyReportReminders
        };
    }

    public async Task<ProviderProTopbarViewModel> GetTopbarAsync(
        IndorProveedor proveedor,
        CancellationToken cancellationToken = default)
    {
        var companyName = ResolveCompanyName(proveedor);
        var recentNotifications = await BuildRecentNotificationsAsync(proveedor.Id, cancellationToken);
        var lastViewedUtc = LoadNotificationsLastViewedUtc(proveedor.Id);
        var hasUnreadNotifications = recentNotifications.Any(n => n.OccurredUtc > lastViewedUtc);

        return new ProviderProTopbarViewModel
        {
            CompanyName = companyName,
            CompanyInitial = BuildCompanyInitial(companyName),
            ShowNotifications = true,
            HasNotifications = hasUnreadNotifications,
            RecentNotifications = recentNotifications
        };
    }

    public void MarkNotificationsViewed(int proveedorId)
    {
        var session = httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            return;
        }

        session.SetString(
            NotificationsViewedSessionKey(proveedorId),
            DateTime.UtcNow.ToString("O"));
    }

    private async Task<List<ProviderProNotificationItemViewModel>> BuildRecentNotificationsAsync(
        int proveedorId,
        CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var candidates = new List<ProviderProNotificationItemViewModel>();

        var newLeads = await db.IndorProveedorLeads
            .AsNoTracking()
            .Where(l => l.ProveedorId == proveedorId
                && l.Status == ProviderLeadStatuses.New
                && l.FechaCreacion >= cutoff)
            .OrderByDescending(l => l.FechaCreacion)
            .Take(6)
            .ToListAsync(cancellationToken);

        foreach (var lead in newLeads)
        {
            candidates.Add(new ProviderProNotificationItemViewModel
            {
                Id = lead.Id,
                Description = $"New lead: {lead.ServiceType} — {ShortAddress(lead.Address)}",
                OccurredLabel = FormatRelativeLeadTime(lead.FechaCreacion),
                CategoryTag = "Leads",
                TagCssClass = "prv-pro-notify-tag--leads",
                IconClass = "fa-bolt",
                TargetUrl = $"/Proveedor/LeadDetails/{lead.Id}",
                OccurredUtc = lead.FechaCreacion
            });
        }

        var approvedEstimates = await db.IndorProveedorEstimates
            .AsNoTracking()
            .Where(e => e.ProveedorId == proveedorId
                && e.ApprovedUtc != null
                && e.ApprovedUtc >= cutoff)
            .OrderByDescending(e => e.ApprovedUtc)
            .Take(6)
            .ToListAsync(cancellationToken);

        foreach (var estimate in approvedEstimates)
        {
            var occurredUtc = estimate.ApprovedUtc ?? estimate.FechaCreacion;
            candidates.Add(new ProviderProNotificationItemViewModel
            {
                Id = estimate.Id,
                Description = $"Estimate approved for {ShortAddress(estimate.Address)}",
                OccurredLabel = FormatRelativeLeadTime(occurredUtc),
                CategoryTag = "Estimates",
                TagCssClass = "prv-pro-notify-tag--estimates",
                IconClass = "fa-circle-check",
                TargetUrl = $"/Proveedor/EstimateAccepted/{estimate.Id}",
                OccurredUtc = occurredUtc
            });
        }

        var viewedEstimates = await db.IndorProveedorEstimates
            .AsNoTracking()
            .Where(e => e.ProveedorId == proveedorId
                && e.ViewedUtc != null
                && e.ViewedUtc >= cutoff
                && e.ApprovedUtc == null)
            .OrderByDescending(e => e.ViewedUtc)
            .Take(4)
            .ToListAsync(cancellationToken);

        foreach (var estimate in viewedEstimates)
        {
            var occurredUtc = estimate.ViewedUtc ?? estimate.FechaCreacion;
            candidates.Add(new ProviderProNotificationItemViewModel
            {
                Id = estimate.Id,
                Description = $"Homeowner viewed your estimate for {ShortAddress(estimate.Address)}",
                OccurredLabel = FormatRelativeLeadTime(occurredUtc),
                CategoryTag = "Estimates",
                TagCssClass = "prv-pro-notify-tag--estimates",
                IconClass = "fa-eye",
                TargetUrl = $"/Proveedor/ReviewEstimate/{estimate.Id}",
                OccurredUtc = occurredUtc
            });
        }

        var unreadConversations = await db.IndorProveedorConversations
            .AsNoTracking()
            .Where(c => c.ProveedorId == proveedorId
                && c.UnreadCount > 0
                && c.LastMessageAt >= cutoff)
            .OrderByDescending(c => c.LastMessageAt)
            .Take(6)
            .ToListAsync(cancellationToken);

        foreach (var conversation in unreadConversations)
        {
            var preview = string.IsNullOrWhiteSpace(conversation.LastMessagePreview)
                ? "New message waiting"
                : conversation.LastMessagePreview!;
            candidates.Add(new ProviderProNotificationItemViewModel
            {
                Id = conversation.Id,
                Description = preview,
                OccurredLabel = FormatRelativeLeadTime(conversation.LastMessageAt),
                CategoryTag = "Messages",
                TagCssClass = "prv-pro-notify-tag--messages",
                IconClass = "fa-comment-dots",
                TargetUrl = $"/Proveedor/Conversation/{conversation.Id}",
                OccurredUtc = conversation.LastMessageAt
            });
        }

        var overdueInvoices = await db.IndorProveedorInvoices
            .AsNoTracking()
            .Where(i => i.ProveedorId == proveedorId
                && i.Status == ProviderInvoiceStatuses.Overdue)
            .OrderByDescending(i => i.DueDate ?? i.FechaCreacion)
            .Take(4)
            .ToListAsync(cancellationToken);

        foreach (var invoice in overdueInvoices)
        {
            var occurredUtc = invoice.DueDate?.ToUniversalTime() ?? invoice.FechaCreacion;
            candidates.Add(new ProviderProNotificationItemViewModel
            {
                Id = invoice.Id,
                Description = $"Overdue invoice {FormatInvoiceCode(invoice)} — {invoice.Amount:C0}",
                OccurredLabel = FormatRelativeLeadTime(occurredUtc),
                CategoryTag = "Payments",
                TagCssClass = "prv-pro-notify-tag--payments",
                IconClass = "fa-file-invoice-dollar",
                TargetUrl = $"/Proveedor/InvoiceDetails/{invoice.Id}",
                OccurredUtc = occurredUtc
            });
        }

        var paidInvoices = await db.IndorProveedorInvoices
            .AsNoTracking()
            .Where(i => i.ProveedorId == proveedorId
                && i.Status == ProviderInvoiceStatuses.Paid
                && i.PaidDate != null
                && i.PaidDate >= cutoff)
            .OrderByDescending(i => i.PaidDate)
            .Take(4)
            .ToListAsync(cancellationToken);

        foreach (var invoice in paidInvoices)
        {
            var occurredUtc = invoice.PaidDate?.ToUniversalTime() ?? invoice.FechaCreacion;
            candidates.Add(new ProviderProNotificationItemViewModel
            {
                Id = invoice.Id,
                Description = $"Payment received for {FormatInvoiceCode(invoice)} — {invoice.Amount:C0}",
                OccurredLabel = FormatRelativeLeadTime(occurredUtc),
                CategoryTag = "Payments",
                TagCssClass = "prv-pro-notify-tag--payments",
                IconClass = "fa-money-bill-wave",
                TargetUrl = $"/Proveedor/InvoiceDetails/{invoice.Id}",
                OccurredUtc = occurredUtc
            });
        }

        return candidates
            .OrderByDescending(c => c.OccurredUtc)
            .Take(8)
            .ToList();
    }

    private DateTime LoadNotificationsLastViewedUtc(int proveedorId)
    {
        var session = httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            return DateTime.MinValue;
        }

        var raw = session.GetString(NotificationsViewedSessionKey(proveedorId));
        return DateTime.TryParse(
            raw,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.RoundtripKind,
            out var parsed)
            ? parsed.ToUniversalTime()
            : DateTime.MinValue;
    }

    private static string NotificationsViewedSessionKey(int proveedorId) =>
        "provider-pro-notifications-viewed-" + proveedorId;

    private static string ShortAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return "your service area";
        }

        var trimmed = address.Trim();
        return trimmed.Length <= 42 ? trimmed : trimmed[..39] + "...";
    }

    private static string FormatInvoiceCode(IndorProveedorInvoice invoice) =>
        string.IsNullOrWhiteSpace(invoice.InvoiceCode)
            ? $"#{invoice.Id}"
            : invoice.InvoiceCode.StartsWith('#')
                ? invoice.InvoiceCode
                : $"#{invoice.InvoiceCode}";

    public async Task SaveNotificationPreferencesAsync(
        int proveedorId,
        ProviderProNotificationsInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.IndorProveedores
            .FirstOrDefaultAsync(p => p.Id == proveedorId, cancellationToken)
            ?? throw new InvalidOperationException("Provider not found.");

        UpdateOnboardingMeta(entity, meta =>
        {
            meta.NotifyJobAlerts = input.NotifyJobAlerts;
            meta.NotifyLeadUpdates = input.NotifyLeadUpdates;
            meta.NotifyPaymentAlerts = input.NotifyPaymentAlerts;
            meta.NotifyReportReminders = input.NotifyReportReminders;
        });

        entity.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static readonly JsonSerializerOptions OnboardingMetaJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static ProviderOnboardingMeta ReadOnboardingMeta(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ProviderOnboardingMeta();
        }

        try
        {
            return JsonSerializer.Deserialize<ProviderOnboardingMeta>(json, OnboardingMetaJsonOptions)
                ?? new ProviderOnboardingMeta();
        }
        catch (JsonException)
        {
            return new ProviderOnboardingMeta();
        }
    }

    private static void UpdateOnboardingMeta(IndorProveedor entity, Action<ProviderOnboardingMeta> update)
    {
        var meta = ReadOnboardingMeta(entity.OnboardingMetaJson);
        update(meta);
        entity.OnboardingMetaJson = JsonSerializer.Serialize(meta, OnboardingMetaJsonOptions);
    }

    private static string? ResolveProviderLogoUrl(IndorProveedor proveedor) =>
        proveedor.Documentos
            .FirstOrDefault(d => string.Equals(d.DocumentType, ProviderDocumentTypes.Logo, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(d.FileUrl))
            ?.FileUrl;

    private static string BuildCompanyInitial(string companyName) =>
        !string.IsNullOrWhiteSpace(companyName)
            ? companyName.Trim()[0].ToString().ToUpperInvariant()
            : "P";

    private static string TrimOrEmpty(string? value) => value?.Trim() ?? "";

    private static List<string> BuildServiceAreas(IndorProveedor proveedor)
    {
        var areas = new List<string>();

        if (!string.IsNullOrWhiteSpace(proveedor.PrimaryCity))
        {
            areas.Add(proveedor.PrimaryCity.Contains(',') ? proveedor.PrimaryCity : $"{proveedor.PrimaryCity}");
        }

        if (!string.IsNullOrWhiteSpace(proveedor.ZipNeighborhoodsJson))
        {
            try
            {
                var zips = JsonSerializer.Deserialize<List<string>>(proveedor.ZipNeighborhoodsJson);
                if (zips != null)
                {
                    foreach (var zip in zips.Take(6))
                    {
                        if (!string.IsNullOrWhiteSpace(zip))
                        {
                            areas.Add(zip);
                        }
                    }
                }
            }
            catch
            {
                // ignore malformed JSON
            }
        }

        if (areas.Count == 0 && proveedor.TravelRadiusMiles > 0)
        {
            areas.Add($"{proveedor.TravelRadiusMiles} mile service radius");
        }

        return areas.Distinct().ToList();
    }

    private static string BuildYearsActiveLabel(DateTime createdUtc)
    {
        var years = Math.Max(0, DateTime.UtcNow.Year - createdUtc.Year);
        if (years >= 3)
        {
            return "3+ Years";
        }

        return years switch
        {
            0 => "< 1 Year",
            1 => "1 Year",
            _ => $"{years} Years"
        };
    }

    private static string BuildServiceAreaLabel(IndorProveedor proveedor)
    {
        if (!string.IsNullOrWhiteSpace(proveedor.PrimaryCity))
        {
            return $"Serving {proveedor.PrimaryCity} and surrounding areas";
        }

        return proveedor.TravelRadiusMiles > 0
            ? $"Serving within {proveedor.TravelRadiusMiles} miles"
            : "Service area not set";
    }

    private static List<ProviderProProfileVerificationItemViewModel> BuildVerificationItems(IndorProveedor proveedor)
    {
        var docs = proveedor.Documentos;
        bool HasDoc(params string[] types) =>
            types.Any(t => docs.Any(d => string.Equals(d.DocumentType, t, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(d.FileUrl)));

        return
        [
            new() { Label = "License Verified", IsComplete = proveedor.IsLicensed && HasDoc(ProviderDocumentTypes.License, ProviderDocumentTypes.HvacLicense, ProviderDocumentTypes.PlumbingLicense, ProviderDocumentTypes.ContractorLicense) },
            new() { Label = "Insurance Active", IsComplete = proveedor.IsInsured && HasDoc(ProviderDocumentTypes.Insurance, ProviderDocumentTypes.LiabilityInsurance) },
            new() { Label = "Background Check Complete", IsComplete = proveedor.BackgroundCheckConsent },
            new() { Label = "W-9 on File", IsComplete = HasDoc(ProviderDocumentTypes.W9) }
        ];
    }

    private static List<ProviderProProfileTeamMemberViewModel> BuildTeamMembers(IndorProveedor proveedor)
    {
        var members = new List<ProviderProProfileTeamMemberViewModel>();

        if (!string.IsNullOrWhiteSpace(proveedor.PrimaryContact))
        {
            members.Add(new ProviderProProfileTeamMemberViewModel
            {
                Name = proveedor.PrimaryContact,
                Role = "Owner",
                RoleClass = "owner"
            });
        }

        if (!string.IsNullOrWhiteSpace(proveedor.TeamSize) && int.TryParse(proveedor.TeamSize, out var size) && size > 1)
        {
            members.Add(new ProviderProProfileTeamMemberViewModel
            {
                Name = "Team member",
                Role = "Technician",
                RoleClass = "technician"
            });
        }

        return members;
    }

    public async Task<ProviderProInvoicesPageViewModel> GetInvoicesPageAsync(
        IndorProveedor proveedor,
        string? tab = "all",
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var activeTab = NormalizeInvoicesTab(tab);
        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        var rows = await db.IndorProveedorInvoices
            .AsNoTracking()
            .Include(i => i.Job)
            .Where(i => i.ProveedorId == proveedor.Id)
            .OrderByDescending(i => i.DueDate ?? i.FechaCreacion)
            .ToListAsync(cancellationToken);

        var paidThisMonth = rows
            .Where(i => i.Status == ProviderInvoiceStatuses.Paid
                && i.PaidDate.HasValue
                && i.PaidDate.Value >= monthStart)
            .Sum(i => i.Amount);
        var pendingTotal = rows.Where(i => i.Status == ProviderInvoiceStatuses.Pending).Sum(i => i.Amount);
        var overdueRows = rows.Where(i => i.Status == ProviderInvoiceStatuses.Overdue).ToList();
        var overdueTotal = overdueRows.Sum(i => i.Amount);

        var filtered = activeTab switch
        {
            "paid" => rows.Where(i => i.Status == ProviderInvoiceStatuses.Paid),
            "pending" => rows.Where(i => i.Status == ProviderInvoiceStatuses.Pending),
            "overdue" => overdueRows,
            _ => rows
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            filtered = filtered.Where(i =>
                FormatInvoiceCode(i.InvoiceCode, i.Id).Contains(q, StringComparison.OrdinalIgnoreCase)
                || (i.Address ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)
                || (i.ServiceType ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)
                || (i.CustomerName ?? "").Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var pendingRows = rows.Where(i => i.Status == ProviderInvoiceStatuses.Pending).ToList();
        var cards = filtered.Select(MapInvoiceCard).ToList();
        var isOverdueView = activeTab == "overdue";
        var isPendingView = activeTab == "pending";

        return new ProviderProInvoicesPageViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            ActiveTab = activeTab,
            SearchQuery = search,
            PageTitle = isOverdueView ? "Overdue Invoices" : isPendingView ? "Pending Invoices" : "Payments & Invoices",
            SearchPlaceholder = isOverdueView ? "Search overdue invoices" : "Search invoices",
            PaidThisMonth = paidThisMonth,
            PendingTotal = pendingTotal,
            OverdueTotal = overdueTotal,
            OverdueCount = overdueRows.Count,
            PendingCount = pendingRows.Count,
            ShowOverdueSummary = isOverdueView,
            ShowPendingSummary = isPendingView,
            Invoices = cards,
            FlowSteps = isOverdueView
                ? OverdueInvoicesFlowSteps()
                : isPendingView
                    ? PendingInvoicesFlowSteps()
                    : PaymentsInvoicesFlowSteps()
        };
    }

    public async Task<ProviderProInvoiceDetailsViewModel?> GetInvoiceDetailsAsync(
        IndorProveedor proveedor,
        int invoiceId,
        string? fromTab = null,
        CancellationToken cancellationToken = default)
    {
        var invoice = await db.IndorProveedorInvoices
            .AsNoTracking()
            .Include(i => i.Job!)
                .ThenInclude(j => j.Cliente)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.ProveedorId == proveedor.Id, cancellationToken);

        if (invoice == null)
        {
            return null;
        }

        var statusClass = MapInvoiceStatusClass(invoice.Status);
        var lineItems = ParseInvoiceLineItems(invoice.LineItemsJson);
        if (lineItems.Count == 0 && invoice.Amount > 0)
        {
            lineItems.Add(new ProviderProInvoiceLineItemViewModel
            {
                Description = invoice.ServiceType ?? "Service",
                Category = "labor",
                IconClass = MapInvoiceLineIcon("labor"),
                Qty = 1,
                Rate = invoice.Amount,
                Amount = invoice.Amount
            });
        }

        var job = invoice.Job;
        var returnTab = NormalizeInvoicesTab(fromTab ?? invoice.Status switch
        {
            ProviderInvoiceStatuses.Overdue => "overdue",
            ProviderInvoiceStatuses.Pending => "pending",
            ProviderInvoiceStatuses.Paid => "paid",
            _ => "all"
        });
        var isPaid = invoice.Status == ProviderInvoiceStatuses.Paid;
        var paidAmount = invoice.PaidAmount ?? invoice.Amount;
        var paymentRecords = BuildPaymentRecords(invoice);
        var paymentHistory = BuildPaymentHistoryLabel(invoice);
        var paymentMethodLabel = isPaid
            ? FormatPaidPaymentMethod(invoice)
            : "Unpaid";

        return new ProviderProInvoiceDetailsViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            InvoiceId = invoice.Id,
            JobId = invoice.JobId,
            InvoiceCode = FormatInvoiceCode(invoice.InvoiceCode, invoice.Id),
            Address = invoice.Address ?? job?.Address ?? "",
            ServiceType = invoice.ServiceType ?? job?.Title ?? "",
            Amount = invoice.Amount,
            PaidAmount = paidAmount,
            Status = invoice.Status,
            StatusClass = statusClass,
            IsPaid = isPaid,
            PageTitle = isPaid ? "Paid Invoice Details" : "Invoice Details",
            AmountLabel = isPaid ? "Amount Paid" : "Amount Due",
            DueDateLabel = invoice.DueDate.HasValue
                ? invoice.DueDate.Value.ToLocalTime().ToString("MMM d, yyyy")
                : "—",
            InvoiceDateLabel = invoice.InvoiceDate.HasValue
                ? invoice.InvoiceDate.Value.ToString("MMM d, yyyy")
                : invoice.FechaCreacion.ToLocalTime().ToString("MMM d, yyyy"),
            PaymentDateLabel = invoice.PaidDate.HasValue
                ? invoice.PaidDate.Value.ToLocalTime().ToString("MMM d, yyyy")
                : "—",
            PaymentMethod = paymentMethodLabel,
            ReceiptAvailable = isPaid && invoice.PaidDate.HasValue,
            NotesToCustomer = invoice.NotesToCustomer
                ?? "Thank you for choosing our team. Payment is due by the date shown above. Please contact us with any questions.",
            CustomerNotes = invoice.CustomerNotes ?? "",
            PaymentHistoryLabel = paymentHistory,
            PaymentRecords = paymentRecords,
            JobTitle = job?.Title ?? invoice.ServiceType ?? "",
            JobCode = job?.JobCode ?? "",
            JobStatus = job?.Status ?? "",
            JobCompletedLabel = job?.CompletedAt.HasValue == true
                ? job.CompletedAt.Value.ToLocalTime().ToString("MMM d, yyyy")
                : invoice.PaidDate?.ToLocalTime().ToString("MMM d, yyyy") ?? "",
            TechnicianName = job?.AssignedTechnician ?? "",
            ServicePerformed = invoice.ServiceType ?? job?.Title ?? "",
            CustomerName = invoice.CustomerName ?? job?.Cliente?.Name ?? "Homeowner",
            PropertyAddress = invoice.Address ?? job?.Address ?? "",
            PropertyType = invoice.PropertyType ?? "Single Family Home",
            ReturnTab = returnTab,
            LineItemCount = lineItems.Count,
            LineItems = lineItems,
            ShowReminderAction = invoice.Status is ProviderInvoiceStatuses.Pending or ProviderInvoiceStatuses.Overdue,
            ShowMarkPaidAction = invoice.Status is ProviderInvoiceStatuses.Pending or ProviderInvoiceStatuses.Overdue,
            ShowSendReceiptAction = isPaid,
            ShowViewJobReport = isPaid && invoice.JobId.HasValue,
            FlowSteps = isPaid
                ? PaidInvoiceDetailsFlowSteps(invoice.Id)
                : returnTab == "pending"
                    ? PendingInvoiceDetailsFlowSteps(invoice.Id)
                    : InvoiceDetailsFlowSteps(invoice.Id, returnTab)
        };
    }

    public async Task<bool> SendInvoiceReceiptAsync(
        int proveedorId,
        int invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await db.IndorProveedorInvoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.ProveedorId == proveedorId, cancellationToken);

        return invoice != null && invoice.Status == ProviderInvoiceStatuses.Paid;
    }

    public async Task<ProviderProSendInvoiceReminderViewModel?> GetSendInvoiceReminderAsync(
        IndorProveedor proveedor,
        int invoiceId,
        string? fromTab = null,
        CancellationToken cancellationToken = default)
    {
        var invoice = await db.IndorProveedorInvoices
            .AsNoTracking()
            .Include(i => i.Job)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.ProveedorId == proveedor.Id, cancellationToken);

        if (invoice == null || invoice.Status == ProviderInvoiceStatuses.Paid)
        {
            return null;
        }

        var code = FormatInvoiceCode(invoice.InvoiceCode, invoice.Id);
        var customer = invoice.CustomerName ?? "there";
        var dueLabel = invoice.DueDate.HasValue
            ? invoice.DueDate.Value.ToLocalTime().ToString("MMM d, yyyy")
            : "the due date shown";
        var company = ResolveCompanyName(proveedor);
        var statusWord = invoice.Status == ProviderInvoiceStatuses.Pending ? "pending" : invoice.Status.ToLowerInvariant();
        var message =
            $"Hi {customer}, This is a friendly reminder that invoice {code} for {invoice.Amount:C0} is {statusWord}. " +
            $"The due date is {dueLabel}. Please let us know if you have any questions or if you need a copy of the invoice. Thank you! {company}";

        var returnTab = NormalizeInvoicesTab(fromTab ?? (invoice.Status == ProviderInvoiceStatuses.Pending ? "pending" : "all"));

        return new ProviderProSendInvoiceReminderViewModel
        {
            CompanyName = company,
            InvoiceId = invoice.Id,
            InvoiceCode = code,
            ServiceType = invoice.ServiceType ?? invoice.Job?.Title ?? "",
            Amount = invoice.Amount,
            DueDateLabel = dueLabel,
            Status = invoice.Status,
            StatusClass = MapInvoiceStatusClass(invoice.Status),
            CustomerName = invoice.CustomerName ?? "Homeowner",
            CustomerEmail = invoice.CustomerEmail ?? "",
            CustomerPhone = invoice.CustomerPhone ?? "",
            PropertyAddress = invoice.Address ?? invoice.Job?.Address ?? "",
            DefaultMessage = message,
            ReturnTab = returnTab,
            FlowSteps = returnTab == "pending"
                ? PendingSendReminderFlowSteps(invoice.Id)
                : SendInvoiceReminderFlowSteps(invoice.Id, returnTab)
        };
    }

    public async Task<ProviderProRecordPaymentViewModel?> GetRecordPaymentAsync(
        IndorProveedor proveedor,
        int invoiceId,
        string? fromTab = null,
        CancellationToken cancellationToken = default)
    {
        var invoice = await db.IndorProveedorInvoices
            .AsNoTracking()
            .Include(i => i.Job)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.ProveedorId == proveedor.Id, cancellationToken);

        if (invoice == null || invoice.Status == ProviderInvoiceStatuses.Paid)
        {
            return null;
        }

        var returnTab = NormalizeInvoicesTab(fromTab ?? "pending");

        return new ProviderProRecordPaymentViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            InvoiceId = invoice.Id,
            InvoiceCode = FormatInvoiceCode(invoice.InvoiceCode, invoice.Id),
            Address = invoice.Address ?? invoice.Job?.Address ?? "",
            ServiceType = invoice.ServiceType ?? invoice.Job?.Title ?? "",
            AmountDue = invoice.Amount,
            PaymentAmount = invoice.Amount,
            PaymentDate = DateTime.Today.ToString("yyyy-MM-dd"),
            PaymentMethod = "Cash",
            ReturnTab = returnTab,
            FlowSteps = RecordPaymentFlowSteps(invoice.Id, returnTab)
        };
    }

    public async Task<bool> RecordInvoicePaymentAsync(
        int proveedorId,
        ProviderProRecordPaymentInput input,
        CancellationToken cancellationToken = default)
    {
        var invoice = await db.IndorProveedorInvoices
            .FirstOrDefaultAsync(i => i.Id == input.InvoiceId && i.ProveedorId == proveedorId, cancellationToken);

        if (invoice == null || invoice.Status == ProviderInvoiceStatuses.Paid)
        {
            return false;
        }

        var paidDate = DateTime.TryParse(input.PaymentDate, out var parsedDate)
            ? parsedDate.Date
            : DateTime.Today;

        invoice.Status = ProviderInvoiceStatuses.Paid;
        invoice.PaidDate = paidDate;
        invoice.PaidAmount = input.PaymentAmount > 0 ? input.PaymentAmount : invoice.Amount;
        invoice.PaymentMethod = string.IsNullOrWhiteSpace(input.PaymentMethod) ? "Cash" : input.PaymentMethod.Trim();
        invoice.PaymentReference = input.PaymentReference?.Trim();
        invoice.InternalNotes = input.InternalNotes?.Trim();
        invoice.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SendInvoiceReminderAsync(
        int proveedorId,
        ProviderProSendInvoiceReminderInput input,
        CancellationToken cancellationToken = default)
    {
        var invoice = await db.IndorProveedorInvoices
            .FirstOrDefaultAsync(i => i.Id == input.InvoiceId && i.ProveedorId == proveedorId, cancellationToken);

        if (invoice == null || invoice.Status == ProviderInvoiceStatuses.Paid)
        {
            return false;
        }

        if (string.Equals(input.ReminderTiming, "Send now", StringComparison.OrdinalIgnoreCase))
        {
            invoice.LastReminderUtc = DateTime.UtcNow;
            invoice.LastReminderChannel = input.SendVia;
            invoice.LastReminderMessage = input.Message?.Trim();
            invoice.FechaActualizacion = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    private static List<ProviderProInvoicePaymentRecordViewModel> BuildPaymentRecords(IndorProveedorInvoice invoice)
    {
        if (invoice.Status != ProviderInvoiceStatuses.Paid || !invoice.PaidDate.HasValue)
        {
            return [];
        }

        return
        [
            new ProviderProInvoicePaymentRecordViewModel
            {
                TimestampLabel = invoice.PaidDate.Value.ToLocalTime().ToString("MMM d, yyyy h:mm tt"),
                MethodLabel = FormatPaidPaymentMethod(invoice),
                Amount = invoice.PaidAmount ?? invoice.Amount
            }
        ];
    }

    private static string FormatPaidPaymentMethod(IndorProveedorInvoice invoice)
    {
        var method = invoice.PaymentMethod ?? "Payment";
        if (string.Equals(method, "Card", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(invoice.PaymentReference))
        {
            return $"Visa **** {invoice.PaymentReference}";
        }

        if (!string.IsNullOrWhiteSpace(invoice.PaymentReference)
            && !method.Contains(invoice.PaymentReference, StringComparison.OrdinalIgnoreCase))
        {
            return $"{method} ({invoice.PaymentReference})";
        }

        return method;
    }

    private static string BuildPaymentHistoryLabel(IndorProveedorInvoice invoice)
    {
        if (invoice.Status == ProviderInvoiceStatuses.Paid && invoice.PaidDate.HasValue)
        {
            var amount = invoice.PaidAmount ?? invoice.Amount;
            var method = FormatPaidPaymentMethod(invoice);
            return $"Paid {amount:C0} via {method} on {invoice.PaidDate.Value.ToLocalTime():MMM d, yyyy}.";
        }

        if (invoice.LastReminderUtc.HasValue)
        {
            var channel = string.IsNullOrWhiteSpace(invoice.LastReminderChannel)
                ? "reminder"
                : invoice.LastReminderChannel;
            return $"Last {channel} reminder sent {invoice.LastReminderUtc.Value.ToLocalTime():MMM d, yyyy}. No payments recorded yet.";
        }

        return "No payments recorded yet.";
    }

    private static string MapInvoiceLineIcon(string? category) => (category ?? "").Trim().ToLowerInvariant() switch
    {
        "materials" or "material" => "fa-box",
        "permit" or "disposal" or "fee" => "fa-file-lines",
        _ => "fa-user-gear"
    };

    private static string InferLineCategory(string description)
    {
        var text = description.ToLowerInvariant();
        if (text.Contains("material") || text.Contains("shingle") || text.Contains("unit") || text.Contains("filter"))
        {
            return "materials";
        }

        if (text.Contains("permit") || text.Contains("disposal") || text.Contains("fee"))
        {
            return "permit";
        }

        return "labor";
    }

    private static string NormalizeInvoicesTab(string? tab) => (tab ?? "all").Trim().ToLowerInvariant() switch
    {
        "paid" or "pending" or "overdue" => tab!.Trim().ToLowerInvariant(),
        _ => "all"
    };

    private static ProviderProInvoiceCardViewModel MapInvoiceCard(IndorProveedorInvoice invoice)
    {
        var statusClass = MapInvoiceStatusClass(invoice.Status);
        var (icon, showReminder, showMarkPaid) = invoice.Status switch
        {
            ProviderInvoiceStatuses.Overdue => ("fa-circle-exclamation", true, false),
            ProviderInvoiceStatuses.Pending => ("fa-clock", true, true),
            _ => ("fa-circle-check", false, false)
        };

        string? daysLate = null;
        if (invoice.Status == ProviderInvoiceStatuses.Overdue && invoice.DueDate.HasValue)
        {
            var days = Math.Max(1, (DateTime.Today - invoice.DueDate.Value.Date).Days);
            daysLate = $"{days} day{(days == 1 ? "" : "s")} late";
        }

        return new ProviderProInvoiceCardViewModel
        {
            Id = invoice.Id,
            InvoiceCode = FormatInvoiceCode(invoice.InvoiceCode, invoice.Id),
            Address = invoice.Address ?? invoice.Job?.Address ?? "",
            ServiceType = invoice.ServiceType ?? invoice.Job?.Title ?? "",
            Amount = invoice.Amount,
            Status = invoice.Status,
            StatusClass = statusClass,
            StatusIcon = icon,
            DueDateLabel = invoice.DueDate.HasValue
                ? $"Due: {invoice.DueDate.Value.ToLocalTime():MMM d, yyyy}"
                : null,
            DaysLateLabel = daysLate,
            ShowReminderAction = showReminder,
            ShowMarkPaidAction = showMarkPaid
        };
    }

    private static string FormatInvoiceCode(string? code, int id) =>
        string.IsNullOrWhiteSpace(code) ? $"#{id}" : code.StartsWith('#') ? code : $"#{code}";

    private static string MapInvoiceStatusClass(string status) => status switch
    {
        ProviderInvoiceStatuses.Paid => "paid",
        ProviderInvoiceStatuses.Overdue => "overdue",
        _ => "pending"
    };

    private static List<ProviderProInvoiceLineItemViewModel> ParseInvoiceLineItems(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<ProviderProInvoiceLineItemViewModel>>(json) ?? [];
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.Category))
                {
                    item.Category = InferLineCategory(item.Description);
                }

                item.IconClass = MapInvoiceLineIcon(item.Category);
            }

            return items;
        }
        catch
        {
            return [];
        }
    }

    private static List<ProviderProFlowStepViewModel> PendingInvoicesFlowSteps() =>
    [
        new() { Label = "From: Payments & Invoices", IconClass = "fa-file-invoice-dollar", IsLink = true, Url = "/Proveedor/Invoices" },
        new() { Label = "Pressed: Pending", IconClass = "fa-clock" },
        new() { Label = "Now: Pending Invoices", IconClass = "fa-hourglass-half", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> PendingInvoiceDetailsFlowSteps(int invoiceId) =>
    [
        new() { Label = "From: Pending Invoices", IconClass = "fa-hourglass-half", IsLink = true, Url = "/Proveedor/Invoices?tab=pending" },
        new() { Label = "Pressed: View", IconClass = "fa-eye" },
        new() { Label = "Now: Invoice Details", IconClass = "fa-file-lines", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> PaidInvoiceDetailsFlowSteps(int invoiceId) =>
    [
        new() { Label = "From: Payments & Invoices", IconClass = "fa-file-invoice-dollar", IsLink = true, Url = "/Proveedor/Invoices?tab=paid" },
        new() { Label = "Pressed: Paid / View", IconClass = "fa-eye" },
        new() { Label = "Now: Paid Invoice Details", IconClass = "fa-circle-check", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> PendingSendReminderFlowSteps(int invoiceId) =>
    [
        new() { Label = "From: Pending Invoices", IconClass = "fa-hourglass-half", IsLink = true, Url = "/Proveedor/Invoices?tab=pending" },
        new() { Label = "Pressed: Reminder", IconClass = "fa-bell" },
        new() { Label = "Now: Send Reminder", IconClass = "fa-paper-plane", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> RecordPaymentFlowSteps(int invoiceId, string returnTab) =>
    [
        new()
        {
            Label = returnTab == "pending" ? "From: Pending Invoices" : "From: Invoice Details",
            IconClass = returnTab == "pending" ? "fa-hourglass-half" : "fa-file-lines",
            IsLink = true,
            Url = returnTab == "pending" ? "/Proveedor/Invoices?tab=pending" : $"/Proveedor/InvoiceDetails/{invoiceId}?from={returnTab}"
        },
        new() { Label = "Pressed: Mark as Paid", IconClass = "fa-circle-check" },
        new() { Label = "Now: Record Payment", IconClass = "fa-money-bill-wave", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> PaymentsInvoicesFlowSteps() =>
    [
        new() { Label = "From: Home Dashboard", IconClass = "fa-house", IsLink = true, Url = "/Proveedor/Dashboard" },
        new() { Label = "Pressed: Payments Due / View invoices", IconClass = "fa-hand-pointer" },
        new() { Label = "Now: Payments & Invoices", IconClass = "fa-file-invoice-dollar", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> OverdueInvoicesFlowSteps() =>
    [
        new() { Label = "From: Payments & Invoices", IconClass = "fa-file-invoice-dollar", IsLink = true, Url = "/Proveedor/Invoices" },
        new() { Label = "Pressed: Overdue", IconClass = "fa-circle-exclamation" },
        new() { Label = "Now: Overdue Invoices", IconClass = "fa-clock", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> InvoiceDetailsFlowSteps(int invoiceId, string returnTab = "all") =>
    [
        new()
        {
            Label = "From: Payments & Invoices",
            IconClass = "fa-file-invoice-dollar",
            IsLink = true,
            Url = $"/Proveedor/Invoices?tab={returnTab}"
        },
        new() { Label = "Pressed: View", IconClass = "fa-eye" },
        new() { Label = "Now: Invoice Details", IconClass = "fa-file-lines", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> SendInvoiceReminderFlowSteps(int invoiceId, string returnTab = "all") =>
    [
        new()
        {
            Label = "From: Invoice Details",
            IconClass = "fa-file-lines",
            IsLink = true,
            Url = $"/Proveedor/InvoiceDetails/{invoiceId}?from={returnTab}"
        },
        new() { Label = "Pressed: Reminder", IconClass = "fa-bell" },
        new() { Label = "Now: Send Reminder", IconClass = "fa-paper-plane", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> MessagesInboxFlowSteps() =>
    [
        new() { Label = "From: Home Dashboard", IconClass = "fa-house", IsLink = true, Url = "/Proveedor/Dashboard" },
        new() { Label = "Pressed: Messages", IconClass = "fa-message" },
        new() { Label = "Now: Messages Inbox", IconClass = "fa-inbox", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> ConversationFlowSteps(int conversationId) =>
    [
        new() { Label = "From: Messages Inbox", IconClass = "fa-inbox", IsLink = true, Url = "/Proveedor/Messages" },
        new() { Label = "Pressed: Open Conversation", IconClass = "fa-comments" },
        new() { Label = "Now: Conversation Detail", IconClass = "fa-comment-dots", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> MessageQuickActionsFlowSteps(int conversationId) =>
    [
        new() { Label = "From: Conversation Detail", IconClass = "fa-comment-dots", IsLink = true, Url = $"/Proveedor/Conversation/{conversationId}" },
        new() { Label = "Pressed: More Actions", IconClass = "fa-ellipsis" },
        new() { Label = "Now: Quick Actions", IconClass = "fa-bolt", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> MessageSentSuccessFlowSteps() =>
    [
        new() { Label = "From: Quick Actions", IconClass = "fa-bolt" },
        new() { Label = "Pressed: Send", IconClass = "fa-paper-plane" },
        new() { Label = "Now: Message Sent", IconClass = "fa-circle-check", IsCurrent = true }
    ];

    public async Task<ProviderProEstimateAcceptedViewModel?> GetEstimateAcceptedAsync(
        IndorProveedor proveedor,
        int estimateId,
        CancellationToken cancellationToken = default)
    {
        var estimate = await db.IndorProveedorEstimates.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == estimateId && e.ProveedorId == proveedor.Id, cancellationToken);
        if (estimate == null || estimate.Status != ProviderEstimateStatuses.Approved)
        {
            return null;
        }

        IndorProveedorLead? lead = null;
        if (estimate.LeadId is > 0)
        {
            lead = await LoadLeadForWorkflowAsync(proveedor.Id, estimate.LeadId.Value, cancellationToken);
        }

        var scopeItems = ParseScopeItems(estimate.ScopeItemsJson);
        return new ProviderProEstimateAcceptedViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            EstimateId = estimate.Id,
            LeadId = lead?.Id ?? estimate.LeadId ?? 0,
            Address = lead?.Address ?? estimate.Address,
            CustomerName = lead?.CustomerName ?? estimate.CustomerName ?? "Customer",
            TotalAmount = estimate.Amount,
            ApprovedItemCount = scopeItems.Count > 0 ? scopeItems.Count : 1,
            ApprovedLabel = estimate.ApprovedUtc?.ToLocalTime().ToString("MMM d, yyyy") ?? "Recently",
            ApprovedByLabel = lead?.CustomerName ?? "Customer",
            FlowSteps = EstimateAcceptedFlowSteps(estimate.Id, lead?.Id)
        };
    }

    public async Task<bool> ApproveEstimateAsync(int proveedorId, int estimateId, CancellationToken cancellationToken = default)
    {
        var estimate = await db.IndorProveedorEstimates
            .FirstOrDefaultAsync(e => e.Id == estimateId && e.ProveedorId == proveedorId, cancellationToken);
        if (estimate == null || estimate.Status is ProviderEstimateStatuses.Approved or ProviderEstimateStatuses.Declined)
        {
            return false;
        }

        estimate.Status = ProviderEstimateStatuses.Approved;
        estimate.ApprovedUtc = DateTime.UtcNow;
        estimate.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await realtorBridge.SyncBidFromEstimateAsync(estimate, cancellationToken);
        return true;
    }

    public async Task<ProviderProCreateInvoiceViewModel?> GetCreateInvoiceAsync(
        IndorProveedor proveedor,
        int estimateId,
        CancellationToken cancellationToken = default)
    {
        var estimate = await db.IndorProveedorEstimates.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == estimateId && e.ProveedorId == proveedor.Id, cancellationToken);
        if (estimate == null || estimate.Status != ProviderEstimateStatuses.Approved)
        {
            return null;
        }

        IndorProveedorLead? lead = null;
        if (estimate.LeadId is > 0)
        {
            lead = await LoadLeadForWorkflowAsync(proveedor.Id, estimate.LeadId.Value, cancellationToken);
        }

        var lineItems = ParseScopeItems(estimate.ScopeItemsJson);
        if (lineItems.Count == 0)
        {
            lineItems =
            [
                new ProviderProEstimateLineItemViewModel
                {
                    Label = estimate.ServiceType ?? "Approved work",
                    LaborAmount = estimate.LaborAmount,
                    MaterialAmount = estimate.MaterialsAmount,
                    Amount = estimate.Amount
                }
            ];
        }

        var subtotal = lineItems.Sum(i => i.Amount);
        var tax = estimate.TaxAmount ?? 0m;
        return new ProviderProCreateInvoiceViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            EstimateId = estimate.Id,
            LeadId = lead?.Id ?? estimate.LeadId ?? 0,
            Address = lead?.Address ?? estimate.Address,
            CustomerName = lead?.CustomerName ?? estimate.CustomerName ?? "Customer",
            ServiceType = estimate.ServiceType ?? lead?.ServiceType ?? "",
            LineItems = lineItems,
            SubtotalAmount = subtotal,
            TaxAmount = tax,
            TotalAmount = subtotal + tax,
            FlowSteps = CreateInvoiceFlowSteps(estimate.Id)
        };
    }

    public async Task<int?> SaveCreateInvoiceAsync(
        int proveedorId,
        ProviderProCreateInvoiceInput input,
        CancellationToken cancellationToken = default)
    {
        var estimate = await db.IndorProveedorEstimates.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == input.EstimateId && e.ProveedorId == proveedorId, cancellationToken);
        if (estimate == null || estimate.Status != ProviderEstimateStatuses.Approved)
        {
            return null;
        }

        var existing = await db.IndorProveedorInvoices
            .FirstOrDefaultAsync(i => i.EstimateId == estimate.Id && i.ProveedorId == proveedorId, cancellationToken);
        if (existing != null)
        {
            return existing.Id;
        }

        var lineItems = ParseScopeItems(estimate.ScopeItemsJson);
        if (input.IncludeServiceCall && input.ServiceCallAmount > 0)
        {
            lineItems.Add(new ProviderProEstimateLineItemViewModel
            {
                Label = "Service call / assessment",
                Amount = input.ServiceCallAmount,
                LaborAmount = input.ServiceCallAmount
            });
        }

        var amount = lineItems.Sum(i => i.Amount);
        if (amount <= 0)
        {
            amount = estimate.Amount;
        }

        var invoice = new IndorProveedorInvoice
        {
            ProveedorId = proveedorId,
            EstimateId = estimate.Id,
            LeadId = estimate.LeadId,
            JobId = estimate.JobId,
            InvoiceCode = $"INV-{DateTime.UtcNow:yyMMddHHmm}",
            Address = estimate.Address,
            ServiceType = estimate.ServiceType,
            CustomerName = estimate.CustomerName,
            Amount = amount,
            Status = "Draft",
            InvoiceDate = DateOnly.FromDateTime(DateTime.Today),
            DueDate = DateTime.Today.AddDays(14),
            LineItemsJson = SerializeScopeItems(lineItems),
            NotesToCustomer = input.PaymentTerms,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorProveedorInvoices.Add(invoice);
        await db.SaveChangesAsync(cancellationToken);
        return invoice.Id;
    }

    public async Task<ProviderProReviewInvoiceViewModel?> GetReviewInvoiceAsync(
        IndorProveedor proveedor,
        int invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await db.IndorProveedorInvoices.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.ProveedorId == proveedor.Id, cancellationToken);
        if (invoice == null)
        {
            return null;
        }

        var lineItems = ParseScopeItems(invoice.LineItemsJson);
        return new ProviderProReviewInvoiceViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            InvoiceId = invoice.Id,
            EstimateId = invoice.EstimateId ?? 0,
            InvoiceCode = invoice.InvoiceCode ?? $"INV-{invoice.Id}",
            Address = invoice.Address ?? "",
            CustomerName = invoice.CustomerName ?? "Customer",
            ServiceType = invoice.ServiceType ?? "",
            InvoiceDateLabel = invoice.InvoiceDate?.ToString("MMM d, yyyy") ?? DateTime.Today.ToString("MMM d, yyyy"),
            DueDateLabel = invoice.DueDate?.ToString("MMM d, yyyy") ?? "",
            LineItems = lineItems,
            TotalAmount = invoice.Amount,
            PaymentTerms = invoice.NotesToCustomer ?? "Due at completion",
            FlowSteps = ReviewInvoiceFlowSteps(invoice.Id)
        };
    }

    public async Task<bool> SendInvoiceAsync(int proveedorId, int invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await db.IndorProveedorInvoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.ProveedorId == proveedorId, cancellationToken);
        if (invoice == null)
        {
            return false;
        }

        invoice.Status = "Sent";
        invoice.SentUtc = DateTime.UtcNow;
        invoice.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ProviderProInvoiceSentViewModel?> GetInvoiceSentAsync(
        IndorProveedor proveedor,
        int invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await db.IndorProveedorInvoices.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.ProveedorId == proveedor.Id, cancellationToken);
        if (invoice == null)
        {
            return null;
        }

        return new ProviderProInvoiceSentViewModel
        {
            CompanyName = ResolveCompanyName(proveedor),
            InvoiceId = invoice.Id,
            InvoiceCode = invoice.InvoiceCode ?? $"INV-{invoice.Id}",
            Address = invoice.Address ?? "",
            TotalAmount = invoice.Amount,
            StatusLabel = invoice.Status,
            FlowSteps = InvoiceSentFlowSteps(invoice.Id)
        };
    }

    private List<ProviderProEstimateLineItemViewModel> BuildScopeItemsFromSelectedFindings(IndorProveedorLead lead)
    {
        var session = httpContextAccessor.HttpContext?.Session;
        var selected = session != null ? ProviderLeadSelectionSession.Get(session, lead.Id) : [];
        var findings = ParseInspectionFindings(lead.FindingsJson);
        var filtered = selected.Count > 0
            ? findings.Where(f => selected.Contains(f.Index)).ToList()
            : findings;

        return filtered.Select(f => new ProviderProEstimateLineItemViewModel
        {
            Label = f.Title,
            Description = f.Description ?? "",
            Amount = 0,
            LaborAmount = 0,
            MaterialAmount = 0,
            Qty = 1
        }).ToList();
    }

    private static string FormatLeadCode(IndorProveedorLead lead) =>
        !string.IsNullOrWhiteSpace(lead.LeadCode)
            ? lead.LeadCode.StartsWith('#') ? lead.LeadCode : $"#{lead.LeadCode}"
            : $"#L-{lead.Id}";

    private static string BuildDefaultAnalysisSummary(int findingCount, string serviceType) =>
        findingCount > 0
            ? $"INDOR found {findingCount} {serviceType.ToLowerInvariant()} repair item{(findingCount == 1 ? "" : "s")} from the uploaded inspection report."
            : "";

    private static string FormatRelativeLeadTime(DateTime utc)
    {
        var age = DateTime.UtcNow - utc;
        if (age.TotalMinutes < 60)
        {
            return "Now";
        }

        if (age.TotalHours < 24)
        {
            return $"{(int)age.TotalHours}h ago";
        }

        return utc.ToLocalTime().ToString("MMM d");
    }

    private static List<ProviderProFlowStepViewModel> InspectionFindingsFlowSteps(int leadId) =>
    [
        new() { Label = "From: Lead Details", IconClass = "fa-clipboard-list", IsLink = true, Url = $"/Proveedor/LeadDetails/{leadId}" },
        new() { Label = "Pressed: Review Findings", IconClass = "fa-list-check" },
        new() { Label = "Now: Inspection Findings", IconClass = "fa-magnifying-glass", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> SelectRepairItemsFlowSteps(int leadId) =>
    [
        new() { Label = "From: Findings", IconClass = "fa-list-check", IsLink = true, Url = $"/Proveedor/InspectionFindings/{leadId}" },
        new() { Label = "Pressed: Continue", IconClass = "fa-check-double" },
        new() { Label = "Now: Select Repair Items", IconClass = "fa-wrench", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> EstimateAcceptedFlowSteps(int estimateId, int? leadId) =>
    [
        new() { Label = "From: Estimate Sent", IconClass = "fa-paper-plane", IsLink = true, Url = $"/Proveedor/EstimateSent/{estimateId}" },
        new() { Label = "Approved", IconClass = "fa-circle-check" },
        new() { Label = "Now: Estimate Accepted", IconClass = "fa-thumbs-up", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> CreateInvoiceFlowSteps(int estimateId) =>
    [
        new() { Label = "From: Estimate Accepted", IconClass = "fa-circle-check", IsLink = true, Url = $"/Proveedor/EstimateAccepted/{estimateId}" },
        new() { Label = "Pressed: Create Invoice", IconClass = "fa-file-invoice" },
        new() { Label = "Now: Create Invoice", IconClass = "fa-file-invoice-dollar", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> ReviewInvoiceFlowSteps(int invoiceId) =>
    [
        new() { Label = "From: Create Invoice", IconClass = "fa-file-invoice-dollar" },
        new() { Label = "Pressed: Review", IconClass = "fa-eye" },
        new() { Label = "Now: Review Invoice", IconClass = "fa-file-lines", IsCurrent = true }
    ];

    private static List<ProviderProFlowStepViewModel> InvoiceSentFlowSteps(int invoiceId) =>
    [
        new() { Label = "From: Review Invoice", IconClass = "fa-file-lines", IsLink = true, Url = $"/Proveedor/ReviewInvoice/{invoiceId}" },
        new() { Label = "Pressed: Send", IconClass = "fa-paper-plane" },
        new() { Label = "Now: Invoice Sent", IconClass = "fa-circle-check", IsCurrent = true }
    ];

    private static string ResolveCompanyName(IndorProveedor proveedor) =>
        !string.IsNullOrWhiteSpace(proveedor.DbaName)
            ? proveedor.DbaName
            : proveedor.BusinessName ?? proveedor.PrimaryContact ?? "Your company";

    private static string ResolveCompanyNameFromInput(ProviderProEditProfileInput input) =>
        !string.IsNullOrWhiteSpace(input.DbaName)
            ? input.DbaName.Trim()
            : !string.IsNullOrWhiteSpace(input.BusinessName)
                ? input.BusinessName.Trim()
                : !string.IsNullOrWhiteSpace(input.PrimaryContact)
                    ? input.PrimaryContact.Trim()
                    : "Your company";

    private static string MapJobStatusClass(string status) => status switch
    {
        ProviderJobStatuses.InProgress => "progress",
        ProviderJobStatuses.Confirmed => "confirmed",
        ProviderJobStatuses.Completed => "completed",
        ProviderJobStatuses.WaitingOnMaterials => "waiting",
        _ => "scheduled"
    };
}
