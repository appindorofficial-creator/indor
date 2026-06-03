using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class ProviderRegistrationService(
    AppDbContext db,
    IHttpContextAccessor httpContextAccessor,
    UserManager<ApplicationUser> userManager) : IProviderRegistrationService
{
    private const string ProveedorIdSessionKey = "ProveedorRegistroId";
    private const int DefaultPassingPercent = 80;
    private const int QuestionsPerPage = 4;
    private const string ElectricalTrade = "electrical";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<ProviderRegistrationState> GetAsync(CancellationToken cancellationToken = default)
    {
        var proveedorId = await ResolveProveedorIdAsync(cancellationToken);
        if (proveedorId is not > 0)
        {
            return new ProviderRegistrationState();
        }

        var entity = await db.IndorProveedores
            .AsNoTracking()
            .Include(p => p.Categorias)
            .Include(p => p.Ofertas)
            .Include(p => p.ExamRespuestas)
            .FirstOrDefaultAsync(p => p.Id == proveedorId, cancellationToken);

        return entity == null ? new ProviderRegistrationState() : MapFromEntity(entity);
    }

    public async Task SaveAsync(ProviderRegistrationState state, int currentStep, CancellationToken cancellationToken = default)
    {
        var proveedorId = await EnsureDraftAsync(cancellationToken);
        var entity = await db.IndorProveedores
            .Include(p => p.Categorias)
            .Include(p => p.Ofertas)
            .FirstAsync(p => p.Id == proveedorId, cancellationToken);

        ApplyToEntity(entity, state, currentStep);
        SyncCategoriesOnEntity(entity, state.SelectedCategoryIds);
        SyncOfertasOnEntity(entity, state.SelectedServiceIds);
        entity.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task LinkCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        var proveedorId = await EnsureDraftAsync(cancellationToken);
        var entity = await db.IndorProveedores.FirstAsync(p => p.Id == proveedorId, cancellationToken);
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return;
        }

        entity.UserId = userId;
        entity.Email ??= user.Email;
        entity.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OnboardingOption>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await db.IndorProveedorCategoriasCatalogo
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);

        return rows.Count == 0
            ? OnboardingCatalog.ProviderCategories
            : rows.Select(c => new OnboardingOption(c.Id, c.LabelEn, c.IconClass)).ToList();
    }

    public async Task<IReadOnlyList<OnboardingOption>> GetServiceOfferingsAsync(CancellationToken cancellationToken = default)
    {
        var rows = await db.IndorProveedorOfertasCatalogo
            .AsNoTracking()
            .Where(o => o.Activo)
            .OrderBy(o => o.SortOrder)
            .ToListAsync(cancellationToken);

        return rows.Count == 0
            ? OnboardingCatalog.ServiceOfferings
            : rows.Select(o => new OnboardingOption(o.Id, o.LabelEn, o.IconClass)).ToList();
    }

    public Task<int> GetExamPassingPercentAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(DefaultPassingPercent);

    public async Task<int> GetExamQuestionCountAsync(string tradeCode = ElectricalTrade, CancellationToken cancellationToken = default)
    {
        var count = await db.IndorProveedorExamPreguntas
            .AsNoTracking()
            .CountAsync(q => q.TradeCode == tradeCode && q.Activo, cancellationToken);
        return count > 0 ? count : ElectricalExamCatalog.Questions.Count;
    }

    public async Task<int> GetExamTotalPagesAsync(string tradeCode = ElectricalTrade, CancellationToken cancellationToken = default)
    {
        var count = await GetExamQuestionCountAsync(tradeCode, cancellationToken);
        return Math.Max(1, (int)Math.Ceiling(count / (double)QuestionsPerPage));
    }

    public async Task<IReadOnlyList<ExamQuestion>> GetExamPageQuestionsAsync(int page, string tradeCode = ElectricalTrade, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var dbQuestions = await db.IndorProveedorExamPreguntas
            .AsNoTracking()
            .Where(q => q.TradeCode == tradeCode && q.Activo)
            .OrderBy(q => q.QuestionNumber)
            .ToListAsync(cancellationToken);

        if (dbQuestions.Count == 0)
        {
            return ElectricalExamCatalog.PageQuestions(page);
        }

        return dbQuestions
            .Skip((page - 1) * QuestionsPerPage)
            .Take(QuestionsPerPage)
            .Select(q =>
            {
                var options = JsonSerializer.Deserialize<List<string>>(q.OptionsJson) ?? [];
                return new ExamQuestion(q.QuestionNumber, q.TextEn, options, q.CorrectIndex);
            })
            .ToList();
    }

    public async Task SaveExamPageAnswersAsync(
        int page,
        IReadOnlyDictionary<int, string> pageAnswers,
        string tradeCode = ElectricalTrade,
        CancellationToken cancellationToken = default)
    {
        var proveedorId = await EnsureDraftAsync(cancellationToken);
        var questions = await db.IndorProveedorExamPreguntas
            .AsNoTracking()
            .Where(q => q.TradeCode == tradeCode && q.Activo)
            .ToListAsync(cancellationToken);

        if (questions.Count == 0)
        {
            questions = ElectricalExamCatalog.Questions
                .Select(q => new IndorProveedorExamPregunta
                {
                    TradeCode = tradeCode,
                    QuestionNumber = q.Number,
                    TextEn = q.Text,
                    OptionsJson = JsonSerializer.Serialize(q.Options.ToList()),
                    CorrectIndex = q.CorrectIndex,
                })
                .ToList();
        }

        var pageNums = questions
            .OrderBy(q => q.QuestionNumber)
            .Skip((page - 1) * QuestionsPerPage)
            .Take(QuestionsPerPage)
            .Select(q => q.QuestionNumber)
            .ToHashSet();

        foreach (var q in questions.Where(q => pageNums.Contains(q.QuestionNumber)))
        {
            if (!pageAnswers.TryGetValue(q.QuestionNumber, out var raw) || !int.TryParse(raw, out var selected))
            {
                continue;
            }

            var existing = await db.IndorProveedorExamRespuestas
                .FirstOrDefaultAsync(r =>
                    r.ProveedorId == proveedorId &&
                    r.TradeCode == tradeCode &&
                    r.QuestionNumber == q.QuestionNumber, cancellationToken);

            var isCorrect = selected == q.CorrectIndex;
            if (existing == null)
            {
                db.IndorProveedorExamRespuestas.Add(new IndorProveedorExamRespuesta
                {
                    ProveedorId = proveedorId,
                    TradeCode = tradeCode,
                    QuestionNumber = q.QuestionNumber,
                    SelectedIndex = selected,
                    IsCorrect = isCorrect,
                });
            }
            else
            {
                existing.SelectedIndex = selected;
                existing.IsCorrect = isCorrect;
                existing.AnsweredUtc = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(bool Passed, int ScorePercent)> FinalizeExamAsync(string tradeCode = ElectricalTrade, CancellationToken cancellationToken = default)
    {
        var proveedorId = await EnsureDraftAsync(cancellationToken);
        var total = await GetExamQuestionCountAsync(tradeCode, cancellationToken);
        if (total == 0)
        {
            return (false, 0);
        }

        var correct = await db.IndorProveedorExamRespuestas
            .CountAsync(r => r.ProveedorId == proveedorId && r.TradeCode == tradeCode && r.IsCorrect, cancellationToken);

        var score = (int)Math.Round(100.0 * correct / total);
        var passed = score >= DefaultPassingPercent;

        var entity = await db.IndorProveedores.FirstAsync(p => p.Id == proveedorId, cancellationToken);
        entity.ExamScorePercent = score;
        entity.ExamPassed = passed;
        entity.ExamSubmittedUtc = DateTime.UtcNow;
        entity.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return (passed, score);
    }

    public async Task<IReadOnlyList<string>> GetScopeAllowedAsync(string tradeCode = ElectricalTrade, CancellationToken cancellationToken = default)
    {
        var rows = await db.IndorProveedorAlcanceReglas
            .AsNoTracking()
            .Where(r => r.TradeCode == tradeCode && r.Activo && r.IsAllowed)
            .OrderBy(r => r.SortOrder)
            .Select(r => r.LabelEn)
            .ToListAsync(cancellationToken);
        return rows.Count > 0 ? rows : OnboardingCatalog.AllowedElectricalServices;
    }

    public async Task<IReadOnlyList<string>> GetScopeDisallowedAsync(string tradeCode = ElectricalTrade, CancellationToken cancellationToken = default)
    {
        var rows = await db.IndorProveedorAlcanceReglas
            .AsNoTracking()
            .Where(r => r.TradeCode == tradeCode && r.Activo && !r.IsAllowed)
            .OrderBy(r => r.SortOrder)
            .Select(r => r.LabelEn)
            .ToListAsync(cancellationToken);
        return rows.Count > 0 ? rows : OnboardingCatalog.DisallowedForElectrical;
    }

    public async Task CompleteRegistrationAsync(ProviderRegistrationState state, CancellationToken cancellationToken = default)
    {
        var proveedorId = await EnsureDraftAsync(cancellationToken);
        var entity = await db.IndorProveedores.FirstAsync(p => p.Id == proveedorId, cancellationToken);
        ApplyToEntity(entity, state, ProviderRegistrationState.TotalSteps);
        entity.RegistrationStatus = ProviderRegistrationStatuses.Submitted;
        entity.ProfileSubmittedUtc = DateTime.UtcNow;
        entity.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> EnsureDraftAsync(CancellationToken cancellationToken)
    {
        var session = httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("Session is not available.");

        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);

        var id = session.GetInt32(ProveedorIdSessionKey);
        if (id is > 0)
        {
            var exists = await db.IndorProveedores.AnyAsync(p => p.Id == id, cancellationToken);
            if (exists)
            {
                return id.Value;
            }
        }

        if (!string.IsNullOrEmpty(userId))
        {
            var byUser = await db.IndorProveedores.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
            if (byUser != null)
            {
                session.SetInt32(ProveedorIdSessionKey, byUser.Id);
                return byUser.Id;
            }
        }

        var proveedor = new IndorProveedor
        {
            UserId = userId,
            RegistrationStatus = ProviderRegistrationStatuses.Draft,
            CurrentStep = 1,
            Email = (await userManager.GetUserAsync(httpContextAccessor.HttpContext!.User))?.Email,
            PrimaryCity = "Charlotte, NC",
            LanguagesJson = JsonSerializer.Serialize(new[] { "English" }, JsonOptions),
            AvailableDaysJson = JsonSerializer.Serialize(new[] { "Mon", "Tue", "Wed", "Thu", "Fri" }, JsonOptions),
            JobSizesJson = JsonSerializer.Serialize(new[] { "small", "standard", "large" }, JsonOptions),
            ZipNeighborhoodsJson = JsonSerializer.Serialize(new[] { "28202", "28203", "28205" }, JsonOptions),
        };
        db.IndorProveedores.Add(proveedor);
        await db.SaveChangesAsync(cancellationToken);
        session.SetInt32(ProveedorIdSessionKey, proveedor.Id);
        return proveedor.Id;
    }

    private async Task<int?> ResolveProveedorIdAsync(CancellationToken cancellationToken)
    {
        var session = httpContextAccessor.HttpContext?.Session;
        var id = session?.GetInt32(ProveedorIdSessionKey);
        if (id is > 0)
        {
            return id;
        }

        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var proveedor = await db.IndorProveedores.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (proveedor != null)
        {
            session?.SetInt32(ProveedorIdSessionKey, proveedor.Id);
            return proveedor.Id;
        }

        return null;
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

        foreach (var id in desired)
        {
            if (!existing.Contains(id))
            {
                entity.Categorias.Add(new IndorProveedorCategoriaSel { CategoriaId = id });
            }
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

        foreach (var id in desired)
        {
            if (!existing.Contains(id))
            {
                entity.Ofertas.Add(new IndorProveedorOfertaSel { OfertaId = id });
            }
        }
    }

    private static void ApplyToEntity(IndorProveedor entity, ProviderRegistrationState state, int currentStep)
    {
        entity.CurrentStep = currentStep;
        entity.ProviderType = state.ProviderType;
        entity.BusinessName = state.BusinessName;
        entity.DbaName = state.DbaName;
        entity.PrimaryContact = state.PrimaryContact;
        entity.Phone = state.Phone;
        entity.Email = state.Email;
        entity.YearsExperience = state.YearsExperience;
        entity.LanguagesJson = JsonSerializer.Serialize(state.Languages, JsonOptions);
        entity.LicenseNumber = state.LicenseNumber;
        entity.PrimaryCity = state.PrimaryCity;
        entity.TravelRadiusMiles = state.TravelRadiusMiles;
        entity.ZipNeighborhoodsJson = JsonSerializer.Serialize(state.ZipOrNeighborhoods, JsonOptions);
        entity.EmergencyService = state.EmergencyService;
        entity.SameDayJobs = state.SameDayJobs;
        entity.AvailableDaysJson = JsonSerializer.Serialize(state.AvailableDays, JsonOptions);
        entity.PreferredHours = state.PreferredHours;
        entity.JobSizesJson = JsonSerializer.Serialize(state.JobSizes, JsonOptions);
        entity.LogoUploaded = state.LogoUploaded;
        entity.ScopeTradeUnderstood = state.ScopeTradeUnderstood;
        entity.ScopeStandardsAgreed = state.ScopeStandardsAgreed;
        entity.ExamScorePercent = state.ExamScorePercent;
        entity.ExamPassed = state.ExamPassed;
    }

    private static ProviderRegistrationState MapFromEntity(IndorProveedor entity)
    {
        var state = new ProviderRegistrationState
        {
            ProviderType = entity.ProviderType,
            BusinessName = entity.BusinessName ?? "",
            DbaName = entity.DbaName ?? "",
            PrimaryContact = entity.PrimaryContact ?? "",
            Phone = entity.Phone ?? "",
            Email = entity.Email ?? "",
            YearsExperience = entity.YearsExperience ?? "",
            LicenseNumber = entity.LicenseNumber,
            PrimaryCity = entity.PrimaryCity ?? "Charlotte, NC",
            TravelRadiusMiles = entity.TravelRadiusMiles,
            EmergencyService = entity.EmergencyService,
            SameDayJobs = entity.SameDayJobs,
            PreferredHours = entity.PreferredHours ?? "8:00 AM – 6:00 PM",
            LogoUploaded = entity.LogoUploaded,
            ScopeTradeUnderstood = entity.ScopeTradeUnderstood,
            ScopeStandardsAgreed = entity.ScopeStandardsAgreed,
            ExamScorePercent = entity.ExamScorePercent ?? 0,
            ExamPassed = entity.ExamPassed,
            ProfileSubmitted = entity.ProfileSubmittedUtc.HasValue,
            SelectedCategoryIds = entity.Categorias.Select(c => c.CategoriaId).ToList(),
            SelectedServiceIds = entity.Ofertas.Select(o => o.OfertaId).ToList(),
        };

        state.Languages = DeserializeList(entity.LanguagesJson, ["English"]);
        state.ZipOrNeighborhoods = DeserializeList(entity.ZipNeighborhoodsJson, ["28202", "28203", "28205"]);
        state.AvailableDays = DeserializeList(entity.AvailableDaysJson, ["Mon", "Tue", "Wed", "Thu", "Fri"]);
        state.JobSizes = DeserializeList(entity.JobSizesJson, ["small", "standard", "large"]);

        foreach (var answer in entity.ExamRespuestas)
        {
            state.ExamAnswers[answer.QuestionNumber] = answer.SelectedIndex.ToString();
        }

        return state;
    }

    private static List<string> DeserializeList(string? json, List<string> fallback)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return fallback;
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }
}
