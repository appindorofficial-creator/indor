using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Helpers;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

[Authorize]
public class MyHomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;
    private readonly IPropertyEnrichmentService _propertyEnrichmentService;

    public MyHomeController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env,
        IPropertyEnrichmentService propertyEnrichmentService)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
        _propertyEnrichmentService = propertyEnrichmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? id)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return RedirectToAction("AddProperty", "Propietario");

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        return View(MyHomeDisplayService.BuildSummary(propiedad, info));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, string? tab)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();

        var model = MyHomeDisplayService.BuildDetails(propiedad, MyHomeDisplayService.DeserializeProperty(propiedad));
        model.ActiveTab = tab?.ToLowerInvariant() switch
        {
            "public" => "public",
            "attom" => "attom",
            _ => "information"
        };

        if (model.ActiveTab == "attom")
        {
            HttpContext.Session.SetString(
                HouseFactPreviewContext.ReturnUrlSessionKey,
                Url.Action(nameof(Details), new { id, tab = "attom" })!);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncAttom(int id)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();

        var info = MyHomeDisplayService.DeserializeProperty(propiedad) ?? new PropertyInfoViewModel
        {
            FormattedAddress = propiedad.Direccion ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(info.FormattedAddress))
        {
            info.FormattedAddress = propiedad.Direccion ?? string.Empty;
        }

        var result = await _propertyEnrichmentService.EnrichPropertyAsync(info);
        if (!string.IsNullOrWhiteSpace(result.RawJson))
        {
            propiedad.AttomRawJson = result.RawJson;
            propiedad.AttomLastSyncUtc = DateTime.UtcNow;
        }

        if (result.Success)
        {
            propiedad.AttomPropertyId = result.ExternalPropertyId ?? info.AttomPropertyId;
            propiedad.AttomSyncStatus = "Success";
            propiedad.AttomSyncError = null;
            info.DataSource = result.DataSource;
            info.AttomPropertyId = propiedad.AttomPropertyId;
        }
        else
        {
            propiedad.AttomSyncStatus = string.IsNullOrWhiteSpace(result.RawJson) ? "Failed" : "Partial";
            propiedad.AttomSyncError = result.ErrorMessage;
        }

        propiedad.Direccion = info.FormattedAddress;
        propiedad.DatosJson = SerializePropertyInfo(info);
        await _db.SaveChangesAsync();

        TempData["AttomSyncMessage"] = result.Success
            ? $"Property data refreshed ({result.DataSource})."
            : (result.ErrorMessage ?? "Property refresh did not return data.");

        return RedirectToAction(nameof(Details), new { id = propiedad.Id, tab = result.Success ? "attom" : "information" });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        var details = info?.PropertyDetails ?? new PropertyDetailsInfo();
        return View(new MyHomeEditViewModel
        {
            PropiedadId = propiedad.Id,
            Address = propiedad.Direccion ?? info?.FormattedAddress ?? string.Empty,
            PropertyType = details.PropertyType,
            YearBuilt = details.YearBuilt,
            LivingArea = details.LivingArea,
            Bedrooms = details.Bedrooms,
            Bathrooms = details.Bathrooms,
            LotSizeAcres = details.LotSize,
            EstimatedValue = details.EstimatedValue,
            AnnualTaxAmount = details.AnnualTaxAmount,
            TaxYear = details.TaxYear,
            ParcelId = details.ParcelNumber,
            Zoning = details.Zoning,
            AssignedSchool = details.AssignedSchool,
            LastSalePrice = details.LastSalePrice,
            LastSaleDate = details.LastSaleDate
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MyHomeEditViewModel model)
    {
        var propiedad = await LoadPropertyAsync(model.PropiedadId);
        if (propiedad == null) return NotFound();

        if (!ModelState.IsValid) return View(model);

        var info = MyHomeDisplayService.DeserializeProperty(propiedad) ?? new PropertyInfoViewModel();
        info.FormattedAddress = model.Address.Trim();
        info.PropertyDetails ??= new PropertyDetailsInfo();
        info.PropertyDetails.PropertyType = model.PropertyType ?? info.PropertyDetails.PropertyType;
        info.PropertyDetails.YearBuilt = model.YearBuilt;
        info.PropertyDetails.LivingArea = model.LivingArea;
        info.PropertyDetails.Bedrooms = model.Bedrooms;
        info.PropertyDetails.Bathrooms = model.Bathrooms;
        info.PropertyDetails.LotSize = model.LotSizeAcres;
        info.PropertyDetails.EstimatedValue = model.EstimatedValue;
        info.PropertyDetails.AnnualTaxAmount = model.AnnualTaxAmount;
        info.PropertyDetails.TaxYear = model.TaxYear;
        info.PropertyDetails.ParcelNumber = model.ParcelId;
        info.PropertyDetails.Zoning = model.Zoning;
        info.PropertyDetails.AssignedSchool = model.AssignedSchool;
        info.PropertyDetails.LastSalePrice = model.LastSalePrice;
        info.PropertyDetails.LastSaleDate = model.LastSaleDate;
        info.DataSource = string.IsNullOrWhiteSpace(info.DataSource) ? "UserEdited" : $"{info.DataSource}+Edited";

        propiedad.Direccion = model.Address.Trim();
        propiedad.DatosJson = SerializePropertyInfo(info);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { id = propiedad.Id });
    }

    [HttpGet]
    public async Task<IActionResult> History(int id, string? filter)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();

        filter = string.IsNullOrWhiteSpace(filter) ? "All" : filter;
        var query = _db.PropiedadHistorial.Where(h => h.PropiedadId == propiedad.Id);
        if (!string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(h => h.RecordType == filter);
        }

        var items = await query
            .OrderByDescending(h => h.CompletionDate ?? h.FechaCreacion)
            .Select(h => new MyHomeHistoryItemViewModel
            {
                Id = h.Id,
                RecordType = h.RecordType,
                Title = h.Title,
                ProviderName = h.ProviderName,
                TotalCost = h.TotalCost,
                CompletionDate = h.CompletionDate,
                MonthLabel = (h.CompletionDate ?? h.FechaCreacion).ToString("MMMM yyyy", CultureInfo.InvariantCulture)
            })
            .ToListAsync();

        return View(new MyHomeHistoryListViewModel
        {
            PropiedadId = propiedad.Id,
            Address = propiedad.Direccion ?? string.Empty,
            Filter = filter,
            Items = items
        });
    }

    [HttpGet]
    public async Task<IActionResult> HistoryCreate(int id)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();
        return View(new MyHomeHistoryFormViewModel { PropiedadId = propiedad.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HistoryCreate(MyHomeHistoryFormViewModel model)
    {
        if (!await UserOwnsPropertyAsync(model.PropiedadId)) return NotFound();
        if (!ModelState.IsValid) return View(model);

        _db.PropiedadHistorial.Add(new PropiedadHistorial
        {
            PropiedadId = model.PropiedadId,
            RecordType = model.RecordType,
            Title = model.Title.Trim(),
            ProviderName = model.ProviderName?.Trim(),
            CompletionDate = model.CompletionDate,
            TotalCost = model.TotalCost,
            Description = model.Description?.Trim(),
            WarrantyStatus = model.WarrantyStatus,
            Source = "User"
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(History), new { id = model.PropiedadId });
    }

    [HttpGet]
    public async Task<IActionResult> HistoryDetail(int id)
    {
        var record = await LoadHistoryAsync(id);
        if (record == null) return NotFound();

        return View(new MyHomeHistoryDetailViewModel
        {
            Id = record.Id,
            PropiedadId = record.PropiedadId,
            RecordType = MyHomeDisplayService.RecordTypeLabel(record.RecordType),
            Title = record.Title,
            ProviderName = record.ProviderName,
            ProviderId = record.PropiedadProveedorId,
            CompletionDate = record.CompletionDate,
            TotalCost = record.TotalCost,
            Description = record.Description,
            WarrantyStatus = record.WarrantyStatus ?? "Not specified",
            RelatedDocuments = await _db.PropiedadDocumentos
                .Where(d => d.PropiedadId == record.PropiedadId
                    && (d.Category == "Invoices" || d.Category == "Permits" || d.Category == "Warranties"))
                .OrderByDescending(d => d.FechaCreacion)
                .Take(5)
                .Select(d => new MyHomeDocumentItemViewModel
                {
                    Id = d.Id,
                    Category = d.Category,
                    Title = d.Title,
                    FileName = d.FileName,
                    StoragePath = d.StoragePath,
                    ContentType = d.ContentType,
                    SizeBytes = d.SizeBytes
                })
                .ToListAsync()
        });
    }

    [HttpGet]
    public async Task<IActionResult> HistoryEdit(int id)
    {
        var record = await LoadHistoryAsync(id);
        if (record == null) return NotFound();

        return View(new MyHomeHistoryFormViewModel
        {
            Id = record.Id,
            PropiedadId = record.PropiedadId,
            RecordType = record.RecordType,
            Title = record.Title,
            ProviderName = record.ProviderName,
            CompletionDate = record.CompletionDate,
            TotalCost = record.TotalCost,
            Description = record.Description,
            WarrantyStatus = record.WarrantyStatus
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HistoryEdit(MyHomeHistoryFormViewModel model)
    {
        if (!model.Id.HasValue) return NotFound();
        var record = await LoadHistoryAsync(model.Id.Value);
        if (record == null) return NotFound();
        if (!ModelState.IsValid) return View(model);

        record.RecordType = model.RecordType;
        record.Title = model.Title.Trim();
        record.ProviderName = model.ProviderName?.Trim();
        record.CompletionDate = model.CompletionDate;
        record.TotalCost = model.TotalCost;
        record.Description = model.Description?.Trim();
        record.WarrantyStatus = model.WarrantyStatus;
        record.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(HistoryDetail), new { id = record.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Providers(int id, string? search)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();

        var query = _db.PropiedadProveedores.Where(p => p.PropiedadId == propiedad.Id && p.Activo);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Name.Contains(search) || p.ServiceCategory.Contains(search));
        }

        var items = await query
            .OrderBy(p => p.Name)
            .Select(p => new MyHomeProviderItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                ServiceCategory = p.ServiceCategory,
                Phone = p.Phone
            })
            .ToListAsync();

        return View(new MyHomeProvidersViewModel
        {
            PropiedadId = propiedad.Id,
            Address = propiedad.Direccion ?? string.Empty,
            Search = search,
            Items = items
        });
    }

    [HttpGet]
    public async Task<IActionResult> ProviderCreate(int id)
    {
        if (!await UserOwnsPropertyAsync(id)) return NotFound();
        return View(new MyHomeProviderFormViewModel { PropiedadId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProviderCreate(MyHomeProviderFormViewModel model)
    {
        if (!await UserOwnsPropertyAsync(model.PropiedadId)) return NotFound();
        if (!ModelState.IsValid) return View(model);

        _db.PropiedadProveedores.Add(new PropiedadProveedor
        {
            PropiedadId = model.PropiedadId,
            Name = model.Name.Trim(),
            ServiceCategory = model.ServiceCategory.Trim(),
            Phone = model.Phone?.Trim(),
            Website = model.Website?.Trim(),
            Notes = model.Notes?.Trim(),
            Source = "User"
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Providers), new { id = model.PropiedadId });
    }

    [HttpGet]
    public async Task<IActionResult> ProviderEdit(int id)
    {
        var provider = await LoadProviderAsync(id);
        if (provider == null) return NotFound();

        return View(new MyHomeProviderFormViewModel
        {
            Id = provider.Id,
            PropiedadId = provider.PropiedadId,
            Name = provider.Name,
            ServiceCategory = provider.ServiceCategory,
            Phone = provider.Phone,
            Website = provider.Website,
            Notes = provider.Notes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProviderEdit(MyHomeProviderFormViewModel model)
    {
        if (!model.Id.HasValue) return NotFound();
        var provider = await LoadProviderAsync(model.Id.Value);
        if (provider == null) return NotFound();
        if (!ModelState.IsValid) return View(model);

        provider.Name = model.Name.Trim();
        provider.ServiceCategory = model.ServiceCategory.Trim();
        provider.Phone = model.Phone?.Trim();
        provider.Website = model.Website?.Trim();
        provider.Notes = model.Notes?.Trim();
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Providers), new { id = provider.PropiedadId });
    }

    [HttpGet]
    public async Task<IActionResult> Maintenance(int id, string? filter)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();

        filter = string.IsNullOrWhiteSpace(filter) ? "Upcoming" : filter;
        var query = _db.PropiedadMantenimiento.Where(m => m.PropiedadId == propiedad.Id);
        query = filter switch
        {
            "Completed" => query.Where(m => m.Status == "Completed"),
            "All" => query,
            _ => query.Where(m => m.Status != "Completed")
        };

        var items = await query
            .OrderBy(m => m.DueDate ?? DateTime.MaxValue)
            .Select(m => new MyHomeMaintenanceItemViewModel
            {
                Id = m.Id,
                Title = m.Title,
                DueDate = m.DueDate,
                Status = m.Status,
                MonthLabel = (m.DueDate ?? m.FechaCreacion).ToString("MMMM yyyy", CultureInfo.InvariantCulture)
            })
            .ToListAsync();

        return View(new MyHomeMaintenanceViewModel
        {
            PropiedadId = propiedad.Id,
            Address = propiedad.Direccion ?? string.Empty,
            Filter = filter,
            Items = items
        });
    }

    [HttpGet]
    public async Task<IActionResult> MaintenanceCreate(int id)
    {
        if (!await UserOwnsPropertyAsync(id)) return NotFound();
        return View(new MyHomeMaintenanceFormViewModel { PropiedadId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MaintenanceCreate(MyHomeMaintenanceFormViewModel model)
    {
        if (!await UserOwnsPropertyAsync(model.PropiedadId)) return NotFound();
        if (!ModelState.IsValid) return View(model);

        _db.PropiedadMantenimiento.Add(new PropiedadMantenimiento
        {
            PropiedadId = model.PropiedadId,
            Title = model.Title.Trim(),
            DueDate = model.DueDate,
            Status = model.Status,
            Notes = model.Notes?.Trim(),
            PropiedadProveedorId = model.PropiedadProveedorId
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Maintenance), new { id = model.PropiedadId });
    }

    [HttpGet]
    public async Task<IActionResult> MaintenanceEdit(int id)
    {
        var item = await LoadMaintenanceAsync(id);
        if (item == null) return NotFound();

        return View(new MyHomeMaintenanceFormViewModel
        {
            Id = item.Id,
            PropiedadId = item.PropiedadId,
            Title = item.Title,
            DueDate = item.DueDate,
            Status = item.Status,
            Notes = item.Notes,
            PropiedadProveedorId = item.PropiedadProveedorId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MaintenanceEdit(MyHomeMaintenanceFormViewModel model)
    {
        if (!model.Id.HasValue) return NotFound();
        var item = await LoadMaintenanceAsync(model.Id.Value);
        if (item == null) return NotFound();
        if (!ModelState.IsValid) return View(model);

        item.Title = model.Title.Trim();
        item.DueDate = model.DueDate;
        item.Status = model.Status;
        item.Notes = model.Notes?.Trim();
        item.PropiedadProveedorId = model.PropiedadProveedorId;
        item.CompletedDate = model.Status == "Completed" ? DateTime.UtcNow : null;
        item.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(MaintenanceDetail), new { id = item.Id });
    }

    [HttpGet]
    public async Task<IActionResult> MaintenanceDetail(int id)
    {
        var item = await LoadMaintenanceAsync(id);
        if (item == null) return NotFound();

        return View(new MyHomeMaintenanceDetailViewModel
        {
            Id = item.Id,
            PropiedadId = item.PropiedadId,
            Title = item.Title,
            DueDate = item.DueDate,
            CompletedDate = item.CompletedDate,
            Status = item.Status,
            Notes = item.Notes,
            ProviderName = item.Proveedor?.Name
        });
    }

    [HttpGet]
    public async Task<IActionResult> DocumentList(int id, string category)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();

        category = string.IsNullOrWhiteSpace(category) ? "Other" : category;
        var items = await _db.PropiedadDocumentos
            .Where(d => d.PropiedadId == propiedad.Id && d.Category == category)
            .OrderByDescending(d => d.FechaCreacion)
            .Select(d => new MyHomeDocumentItemViewModel
            {
                Id = d.Id,
                Category = d.Category,
                Title = d.Title,
                FileName = d.FileName,
                StoragePath = d.StoragePath,
                ContentType = d.ContentType,
                SizeBytes = d.SizeBytes
            })
            .ToListAsync();

        return View(new MyHomeDocumentListViewModel
        {
            PropiedadId = propiedad.Id,
            Category = category,
            Items = items
        });
    }

    [HttpGet]
    public async Task<IActionResult> Documents(int id, string? search)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();

        var docs = await _db.PropiedadDocumentos.Where(d => d.PropiedadId == propiedad.Id).ToListAsync();
        if (!string.IsNullOrWhiteSpace(search))
        {
            docs = docs.Where(d =>
                d.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                || d.Category.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var categories = MyHomeDisplayService.DocumentCategories
            .Select(cat => new MyHomeDocumentCategoryViewModel
            {
                Category = cat,
                Count = docs.Count(d => string.Equals(d.Category, cat, StringComparison.OrdinalIgnoreCase))
            })
            .ToList();

        return View(new MyHomeDocumentsViewModel
        {
            PropiedadId = propiedad.Id,
            Address = propiedad.Direccion ?? string.Empty,
            Search = search,
            Categories = categories
        });
    }

    [HttpGet]
    public async Task<IActionResult> DocumentCreate(int id, string? category)
    {
        if (!await UserOwnsPropertyAsync(id)) return NotFound();
        return View(new MyHomeDocumentFormViewModel
        {
            PropiedadId = id,
            Category = string.IsNullOrWhiteSpace(category) ? "Other" : category
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DocumentCreate(MyHomeDocumentFormViewModel model, IFormFile? file)
    {
        if (!await UserOwnsPropertyAsync(model.PropiedadId)) return NotFound();
        if (!ModelState.IsValid) return View(model);

        string? storagePath = null;
        string? fileName = null;
        long? sizeBytes = null;
        string? contentType = null;

        if (file != null && file.Length > 0)
        {
            var userId = _userManager.GetUserId(User)!;
            var folder = Path.Combine(_env.WebRootPath, "uploads", "my-home", userId, model.PropiedadId.ToString());
            Directory.CreateDirectory(folder);
            fileName = Path.GetFileName(file.FileName);
            var stored = $"{Guid.NewGuid():N}_{fileName}";
            var physical = Path.Combine(folder, stored);
            await using (var stream = System.IO.File.Create(physical))
            {
                await file.CopyToAsync(stream);
            }

            storagePath = $"/uploads/my-home/{userId}/{model.PropiedadId}/{stored}";
            sizeBytes = file.Length;
            contentType = file.ContentType;
        }

        _db.PropiedadDocumentos.Add(new PropiedadDocumento
        {
            PropiedadId = model.PropiedadId,
            Category = model.Category,
            Title = model.Title.Trim(),
            FileName = fileName,
            StoragePath = storagePath,
            ContentType = contentType,
            SizeBytes = sizeBytes
        });
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Documents), new { id = model.PropiedadId });
    }

    private async Task<Propiedad?> LoadPropertyAsync(int? id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return null;

        if (id.HasValue)
        {
            return await _db.Propiedades
                .FirstOrDefaultAsync(p => p.Id == id.Value && p.UserId == userId && p.Activo);
        }

        return await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<bool> UserOwnsPropertyAsync(int propiedadId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return false;
        return await _db.Propiedades.AnyAsync(p => p.Id == propiedadId && p.UserId == userId && p.Activo);
    }

    private async Task<PropiedadHistorial?> LoadHistoryAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return null;

        return await _db.PropiedadHistorial
            .Include(h => h.Propiedad)
            .FirstOrDefaultAsync(h => h.Id == id && h.Propiedad!.UserId == userId);
    }

    private async Task<PropiedadProveedor?> LoadProviderAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return null;

        return await _db.PropiedadProveedores
            .Include(p => p.Propiedad)
            .FirstOrDefaultAsync(p => p.Id == id && p.Propiedad!.UserId == userId);
    }

    private async Task<PropiedadMantenimiento?> LoadMaintenanceAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return null;

        return await _db.PropiedadMantenimiento
            .Include(m => m.Propiedad)
            .Include(m => m.Proveedor)
            .FirstOrDefaultAsync(m => m.Id == id && m.Propiedad!.UserId == userId);
    }

    private static string SerializePropertyInfo(PropertyInfoViewModel info)
    {
        info.AttomRawJson = null;
        return JsonSerializer.Serialize(info, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
    }
}
