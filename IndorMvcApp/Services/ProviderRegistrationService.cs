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

    public async Task<IReadOnlyList<OnboardingOption>> GetServiceOfferingsAsync(CancellationToken cancellationToken = default) =>
        await GetServiceOfferingsForTradeAsync(cancellationToken);

    public async Task<IReadOnlyList<OnboardingOption>> GetServiceOfferingsForTradeAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetAsync(cancellationToken);
        var trade = state.PrimaryTradeId;
        var plumbingIds = OnboardingCatalog.PlumbingServiceOfferings.Select(o => o.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var hvacIds = OnboardingCatalog.HvacServiceOfferings.Select(o => o.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var handymanIds = OnboardingCatalog.HandymanServiceOfferings.Select(o => o.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var constructionIds = OnboardingCatalog.ConstructionServiceOfferings.Select(o => o.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var bathroomIds = OnboardingCatalog.BathroomServiceOfferings.Select(o => o.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var kitchenIds = OnboardingCatalog.KitchenServiceOfferings.Select(o => o.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var roofingIds = OnboardingCatalog.RoofingServiceOfferings.Select(o => o.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var paintingIds = OnboardingCatalog.PaintingServiceOfferings.Select(o => o.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var flooringIds = OnboardingCatalog.FlooringServiceOfferings.Select(o => o.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var cleaningIds = OnboardingCatalog.CleaningServiceOfferings.Select(o => o.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var landscapingIds = OnboardingCatalog.LandscapingServiceOfferings.Select(o => o.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var pestIds = OnboardingCatalog.PestControlServiceOfferings.Select(o => o.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var applianceIds = OnboardingCatalog.ApplianceRepairServiceOfferings.Select(o => o.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rows = await db.IndorProveedorOfertasCatalogo
            .AsNoTracking()
            .Where(o => o.Activo)
            .OrderBy(o => o.SortOrder)
            .ToListAsync(cancellationToken);

        if (state.IsPlumbingOnly)
        {
            var plumbingRows = rows.Where(o => plumbingIds.Contains(o.Id)).ToList();
            return plumbingRows.Count > 0
                ? plumbingRows.Select(o => new OnboardingOption(o.Id, o.LabelEn, o.IconClass)).ToList()
                : OnboardingCatalog.PlumbingServiceOfferings;
        }

        if (state.IsHvacOnly)
        {
            var hvacRows = rows.Where(o => hvacIds.Contains(o.Id)).ToList();
            return hvacRows.Count > 0
                ? hvacRows.Select(o => new OnboardingOption(o.Id, o.LabelEn, o.IconClass)).ToList()
                : OnboardingCatalog.HvacServiceOfferings;
        }

        if (state.IsHandymanOnly)
        {
            var handymanRows = rows.Where(o => handymanIds.Contains(o.Id)).ToList();
            return handymanRows.Count > 0
                ? handymanRows.Select(o => new OnboardingOption(o.Id, o.LabelEn, o.IconClass)).ToList()
                : OnboardingCatalog.HandymanServiceOfferings;
        }

        if (state.IsConstructionOnly)
        {
            var constructionRows = rows.Where(o => constructionIds.Contains(o.Id)).ToList();
            return constructionRows.Count > 0
                ? constructionRows.Select(o => new OnboardingOption(o.Id, o.LabelEn, o.IconClass)).ToList()
                : OnboardingCatalog.ConstructionServiceOfferings;
        }

        if (state.IsBathroomOnly)
        {
            var bathroomRows = rows.Where(o => bathroomIds.Contains(o.Id)).ToList();
            return bathroomRows.Count > 0
                ? bathroomRows.Select(o => new OnboardingOption(o.Id, o.LabelEn, o.IconClass)).ToList()
                : OnboardingCatalog.BathroomServiceOfferings;
        }

        if (state.IsKitchenOnly)
        {
            var kitchenRows = rows.Where(o => kitchenIds.Contains(o.Id)).ToList();
            return kitchenRows.Count > 0
                ? kitchenRows.Select(o => new OnboardingOption(o.Id, o.LabelEn, o.IconClass)).ToList()
                : OnboardingCatalog.KitchenServiceOfferings;
        }

        if (state.IsRoofingOnly)
        {
            var roofingRows = rows.Where(o => roofingIds.Contains(o.Id)).ToList();
            return roofingRows.Count > 0
                ? roofingRows.Select(o => new OnboardingOption(o.Id, o.LabelEn, o.IconClass)).ToList()
                : OnboardingCatalog.RoofingServiceOfferings;
        }

        if (state.IsPaintingOnly)
        {
            var paintingRows = rows.Where(o => paintingIds.Contains(o.Id)).ToList();
            return paintingRows.Count > 0
                ? paintingRows.Select(o => new OnboardingOption(o.Id, o.LabelEn, o.IconClass)).ToList()
                : OnboardingCatalog.PaintingServiceOfferings;
        }

        if (state.IsFlooringOnly)
        {
            var flooringRows = rows.Where(o => flooringIds.Contains(o.Id)).ToList();
            return flooringRows.Count > 0
                ? flooringRows.Select(o => new OnboardingOption(o.Id, o.LabelEn, o.IconClass)).ToList()
                : OnboardingCatalog.FlooringServiceOfferings;
        }

        if (state.IsCleaningOnly)
        {
            var cleaningRows = rows.Where(o => cleaningIds.Contains(o.Id)).ToList();
            return cleaningRows.Count > 0
                ? cleaningRows.Select(o => new OnboardingOption(o.Id, o.LabelEn, o.IconClass)).ToList()
                : OnboardingCatalog.CleaningServiceOfferings;
        }

        if (state.IsLandscapingOnly)
        {
            var landscapingRows = rows.Where(o => landscapingIds.Contains(o.Id)).ToList();
            return landscapingRows.Count > 0
                ? landscapingRows.Select(o => new OnboardingOption(o.Id, o.LabelEn, o.IconClass)).ToList()
                : OnboardingCatalog.LandscapingServiceOfferings;
        }

        if (state.IsPestOnly)
        {
            var pestRows = rows.Where(o => pestIds.Contains(o.Id)).ToList();
            return pestRows.Count > 0
                ? pestRows.Select(o => new OnboardingOption(o.Id, o.LabelEn, o.IconClass)).ToList()
                : OnboardingCatalog.PestControlServiceOfferings;
        }

        if (state.IsApplianceOnly)
        {
            var applianceRows = rows.Where(o => applianceIds.Contains(o.Id)).ToList();
            return applianceRows.Count > 0
                ? applianceRows.Select(o => new OnboardingOption(o.Id, o.LabelEn, o.IconClass)).ToList()
                : OnboardingCatalog.ApplianceRepairServiceOfferings;
        }

        return rows.Count == 0
            ? OnboardingCatalog.ServiceOfferings
            : rows.Select(o => new OnboardingOption(o.Id, o.LabelEn, o.IconClass)).ToList();
    }

    public async Task<string?> GetPrimaryTradeLabelAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetAsync(cancellationToken);
        if (string.IsNullOrEmpty(state.PrimaryTradeId))
        {
            return null;
        }

        var cat = await db.IndorProveedorCategoriasCatalogo.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == state.PrimaryTradeId, cancellationToken);
        return cat?.LabelEn ?? OnboardingCatalog.ProviderCategories
            .FirstOrDefault(c => c.Id.Equals(state.PrimaryTradeId, StringComparison.OrdinalIgnoreCase))?.Label;
    }

    public async Task<string> ResolveTradeCodeAsync(ProviderRegistrationState? state = null, CancellationToken cancellationToken = default)
    {
        state ??= await GetAsync(cancellationToken);
        if (state.IsPlumbingOnly)
        {
            return PlumbingExamCatalog.TradeCode;
        }

        if (state.IsHvacOnly)
        {
            return HvacExamCatalog.TradeCode;
        }

        if (state.IsHandymanOnly)
        {
            return HandymanExamCatalog.TradeCode;
        }

        if (state.IsConstructionOnly)
        {
            return ConstructionExamCatalog.TradeCode;
        }

        if (state.IsBathroomOnly)
        {
            return BathroomExamCatalog.TradeCode;
        }

        if (state.IsKitchenOnly)
        {
            return KitchenExamCatalog.TradeCode;
        }

        if (state.IsRoofingOnly)
        {
            return RoofingExamCatalog.TradeCode;
        }

        if (state.IsPaintingOnly)
        {
            return PaintingExamCatalog.TradeCode;
        }

        if (state.IsFlooringOnly)
        {
            return FlooringExamCatalog.TradeCode;
        }

        if (state.IsCleaningOnly)
        {
            return CleaningExamCatalog.TradeCode;
        }

        if (state.IsLandscapingOnly)
        {
            return LandscapingExamCatalog.TradeCode;
        }

        if (state.IsPestOnly)
        {
            return PestControlExamCatalog.TradeCode;
        }

        if (state.IsApplianceOnly)
        {
            return ApplianceRepairExamCatalog.TradeCode;
        }

        if (state.IsElectricianOnly || state.SelectedIncludesElectrical)
        {
            return ElectricalExamCatalog.Questions.FirstOrDefault() != null
                ? "electrical"
                : ProviderRegistrationState.ElectricalCategoryId;
        }

        return state.PrimaryTradeId ?? ProviderRegistrationState.ElectricalCategoryId;
    }

    public Task<int> GetExamPassingPercentAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(DefaultPassingPercent);

    public async Task<int> GetExamQuestionCountAsync(string tradeCode = ElectricalTrade, CancellationToken cancellationToken = default)
    {
        var count = await db.IndorProveedorExamPreguntas
            .AsNoTracking()
            .CountAsync(q => q.TradeCode == tradeCode && q.Activo, cancellationToken);
        if (count > 0)
        {
            return count;
        }

        if (tradeCode.Equals(PlumbingExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
        {
            return PlumbingExamCatalog.Questions.Count;
        }

        if (tradeCode.Equals(HvacExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
        {
            return HvacExamCatalog.Questions.Count;
        }

        if (tradeCode.Equals(HandymanExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
        {
            return HandymanExamCatalog.Questions.Count;
        }

        if (tradeCode.Equals(ConstructionExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
        {
            return ConstructionExamCatalog.Questions.Count;
        }

        if (tradeCode.Equals(BathroomExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
        {
            return BathroomExamCatalog.Questions.Count;
        }

        if (tradeCode.Equals(KitchenExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
        {
            return KitchenExamCatalog.Questions.Count;
        }

        if (tradeCode.Equals(RoofingExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
        {
            return RoofingExamCatalog.Questions.Count;
        }

        if (tradeCode.Equals(PaintingExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
        {
            return PaintingExamCatalog.Questions.Count;
        }

        if (tradeCode.Equals(FlooringExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
        {
            return FlooringExamCatalog.Questions.Count;
        }

        if (tradeCode.Equals(CleaningExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
        {
            return CleaningExamCatalog.Questions.Count;
        }

        if (tradeCode.Equals(LandscapingExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
        {
            return LandscapingExamCatalog.Questions.Count;
        }

        if (tradeCode.Equals(PestControlExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
        {
            return PestControlExamCatalog.Questions.Count;
        }

        if (tradeCode.Equals(ApplianceRepairExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
        {
            return ApplianceRepairExamCatalog.Questions.Count;
        }

        return ElectricalExamCatalog.Questions.Count;
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
            if (tradeCode.Equals(PlumbingExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
            {
                return PlumbingExamCatalog.PageQuestions(page);
            }

            if (tradeCode.Equals(HvacExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
            {
                return HvacExamCatalog.PageQuestions(page);
            }

            if (tradeCode.Equals(HandymanExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
            {
                return HandymanExamCatalog.PageQuestions(page);
            }

            if (tradeCode.Equals(ConstructionExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
            {
                return ConstructionExamCatalog.PageQuestions(page);
            }

            if (tradeCode.Equals(BathroomExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
            {
                return BathroomExamCatalog.PageQuestions(page);
            }

            if (tradeCode.Equals(KitchenExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
            {
                return KitchenExamCatalog.PageQuestions(page);
            }

            if (tradeCode.Equals(RoofingExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
            {
                return RoofingExamCatalog.PageQuestions(page);
            }

            if (tradeCode.Equals(PaintingExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
            {
                return PaintingExamCatalog.PageQuestions(page);
            }

            if (tradeCode.Equals(FlooringExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
            {
                return FlooringExamCatalog.PageQuestions(page);
            }

            if (tradeCode.Equals(CleaningExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
            {
                return CleaningExamCatalog.PageQuestions(page);
            }

            if (tradeCode.Equals(LandscapingExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
            {
                return LandscapingExamCatalog.PageQuestions(page);
            }

            if (tradeCode.Equals(PestControlExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
            {
                return PestControlExamCatalog.PageQuestions(page);
            }

            if (tradeCode.Equals(ApplianceRepairExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase))
            {
                return ApplianceRepairExamCatalog.PageQuestions(page);
            }

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
            IReadOnlyList<ExamQuestion> fallback = tradeCode switch
            {
                _ when tradeCode.Equals(PlumbingExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase) => PlumbingExamCatalog.Questions,
                _ when tradeCode.Equals(HvacExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase) => HvacExamCatalog.Questions,
                _ when tradeCode.Equals(HandymanExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase) => HandymanExamCatalog.Questions,
                _ when tradeCode.Equals(ConstructionExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase) => ConstructionExamCatalog.Questions,
                _ when tradeCode.Equals(BathroomExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase) => BathroomExamCatalog.Questions,
                _ when tradeCode.Equals(KitchenExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase) => KitchenExamCatalog.Questions,
                _ when tradeCode.Equals(RoofingExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase) => RoofingExamCatalog.Questions,
                _ when tradeCode.Equals(PaintingExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase) => PaintingExamCatalog.Questions,
                _ when tradeCode.Equals(FlooringExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase) => FlooringExamCatalog.Questions,
                _ when tradeCode.Equals(CleaningExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase) => CleaningExamCatalog.Questions,
                _ when tradeCode.Equals(LandscapingExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase) => LandscapingExamCatalog.Questions,
                _ when tradeCode.Equals(PestControlExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase) => PestControlExamCatalog.Questions,
                _ when tradeCode.Equals(ApplianceRepairExamCatalog.TradeCode, StringComparison.OrdinalIgnoreCase) => ApplianceRepairExamCatalog.Questions,
                _ => ElectricalExamCatalog.Questions,
            };
            questions = fallback
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
        var entity = await db.IndorProveedores
            .Include(p => p.Categorias)
            .Include(p => p.Ofertas)
            .FirstAsync(p => p.Id == proveedorId, cancellationToken);

        ApplyToEntity(entity, state, ProviderRegistrationState.TotalSteps);
        SyncCategoriesOnEntity(entity, state.SelectedCategoryIds);
        SyncOfertasOnEntity(entity, state.SelectedServiceIds);
        entity.RegistrationStatus = ProviderRegistrationStatuses.PendingReview;
        entity.ProfileSubmittedUtc = DateTime.UtcNow;
        entity.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SubmitApplicationAsync(ProviderRegistrationState state, CancellationToken cancellationToken = default)
    {
        state.ProfileSubmitted = true;
        state.SubmitConfirmed = true;
        await SaveAsync(state, ProviderRegistrationState.TotalSteps, cancellationToken);
        await CompleteRegistrationAsync(state, cancellationToken);
    }

    public async Task<bool> RequiresTradeExamAsync(
        ProviderRegistrationState? state = null,
        CancellationToken cancellationToken = default)
    {
        state ??= await GetAsync(cancellationToken);
        var tradeId = state.PrimaryTradeId;
        if (string.IsNullOrEmpty(tradeId))
        {
            return false;
        }

        var catalog = await db.IndorProveedorCategoriasCatalogo
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == tradeId, cancellationToken);

        if (catalog?.RequiresTradeExam == true)
        {
            return true;
        }

        // Catalog rows may exist with RequiresTradeExam = 0 until seed scripts run; honor known exam trades.
        return tradeId.Equals(ProviderRegistrationState.ElectricalCategoryId, StringComparison.OrdinalIgnoreCase)
               || tradeId.Equals(ProviderRegistrationState.PlumbingCategoryId, StringComparison.OrdinalIgnoreCase)
               || tradeId.Equals(ProviderRegistrationState.HvacCategoryId, StringComparison.OrdinalIgnoreCase)
               || tradeId.Equals(ProviderRegistrationState.HandymanCategoryId, StringComparison.OrdinalIgnoreCase)
               || tradeId.Equals(ProviderRegistrationState.ConstructionCategoryId, StringComparison.OrdinalIgnoreCase)
               || tradeId.Equals(ProviderRegistrationState.BathroomCategoryId, StringComparison.OrdinalIgnoreCase)
               || tradeId.Equals(ProviderRegistrationState.KitchenCategoryId, StringComparison.OrdinalIgnoreCase)
               || tradeId.Equals(ProviderRegistrationState.RoofingCategoryId, StringComparison.OrdinalIgnoreCase)
               || tradeId.Equals(ProviderRegistrationState.PaintingCategoryId, StringComparison.OrdinalIgnoreCase)
               || tradeId.Equals(ProviderRegistrationState.FlooringCategoryId, StringComparison.OrdinalIgnoreCase)
               || tradeId.Equals(ProviderRegistrationState.CleaningCategoryId, StringComparison.OrdinalIgnoreCase)
               || tradeId.Equals(ProviderRegistrationState.LandscapingCategoryId, StringComparison.OrdinalIgnoreCase)
               || tradeId.Equals(ProviderRegistrationState.PestCategoryId, StringComparison.OrdinalIgnoreCase)
               || tradeId.Equals(ProviderRegistrationState.ApplianceCategoryId, StringComparison.OrdinalIgnoreCase);
    }

    public async Task EnsureDocumentSlotsAsync(CancellationToken cancellationToken = default)
    {
        var proveedorId = await EnsureDraftAsync(cancellationToken);
        var existing = await db.IndorProveedorDocumentos
            .Where(d => d.ProveedorId == proveedorId)
            .Select(d => d.DocumentType)
            .ToListAsync(cancellationToken);

        var existingSet = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var trade = (await GetAsync(cancellationToken)).PrimaryTradeId;
        foreach (var (type, _, required) in ProviderDocumentTypes.GetSlotsForTrade(trade))
        {
            if (existingSet.Contains(type))
            {
                continue;
            }

            db.IndorProveedorDocumentos.Add(new IndorProveedorDocumento
            {
                ProveedorId = proveedorId,
                DocumentType = type,
                Status = required ? "Required" : "Optional",
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProviderDocumentSlot>> GetDocumentSlotsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDocumentSlotsAsync(cancellationToken);
        var proveedorId = await ResolveProveedorIdAsync(cancellationToken);
        if (proveedorId is not > 0)
        {
            return [];
        }

        var rows = await db.IndorProveedorDocumentos
            .AsNoTracking()
            .Where(d => d.ProveedorId == proveedorId)
            .ToListAsync(cancellationToken);

        var trade = (await GetAsync(cancellationToken)).PrimaryTradeId;
        return ProviderDocumentTypes.GetSlotsForTrade(trade).Select(slot =>
        {
            var row = rows.FirstOrDefault(r =>
                r.DocumentType.Equals(slot.Type, StringComparison.OrdinalIgnoreCase));
            var uploaded = !string.IsNullOrWhiteSpace(row?.FileUrl);
            var status = uploaded ? "Uploaded" : row?.Status ?? (slot.Required ? "Required" : "Optional");
            var fileName = uploaded && row?.FileUrl != null
                ? Path.GetFileName(row.FileUrl.TrimStart('/'))
                : null;
            return new ProviderDocumentSlot(
                slot.Type,
                slot.Label,
                slot.Required,
                status,
                row?.FileUrl,
                fileName);
        }).ToList();
    }

    public async Task RegisterDocumentUploadAsync(
        string documentType,
        string relativeUrl,
        CancellationToken cancellationToken = default)
    {
        var proveedorId = await EnsureDraftAsync(cancellationToken);
        await EnsureDocumentSlotsAsync(cancellationToken);

        var doc = await db.IndorProveedorDocumentos.FirstOrDefaultAsync(
            d => d.ProveedorId == proveedorId &&
                 d.DocumentType == documentType,
            cancellationToken);

        if (doc == null)
        {
            doc = new IndorProveedorDocumento
            {
                ProveedorId = proveedorId,
                DocumentType = documentType,
            };
            db.IndorProveedorDocumentos.Add(doc);
        }

        doc.FileUrl = relativeUrl;
        doc.Status = "Uploaded";
        doc.UploadedUtc = DateTime.UtcNow;

        if (documentType.Equals(ProviderDocumentTypes.Logo, StringComparison.OrdinalIgnoreCase))
        {
            var entity = await db.IndorProveedores.FirstAsync(p => p.Id == proveedorId, cancellationToken);
            entity.LogoUploaded = true;
            entity.FechaActualizacion = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasRequiredDocumentsAsync(CancellationToken cancellationToken = default)
    {
        var slots = await GetDocumentSlotsAsync(cancellationToken);
        return slots.Where(s => s.Required).All(s =>
            string.Equals(s.Status, "Uploaded", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(s.FileUrl));
    }

    public async Task<IndorProveedor?> GetProveedorForCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return await db.IndorProveedores
            .AsNoTracking()
            .Include(p => p.Categorias)
            .ThenInclude(c => c.Categoria)
            .Include(p => p.Ofertas)
            .ThenInclude(o => o.Oferta)
            .Include(p => p.Documentos)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
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
        entity.EpaCertificationNumber = state.EpaCertificationNumber;
        entity.BackgroundCheckConsent = state.BackgroundCheckConsent;
        entity.ServiceDescription = state.ServiceDescription;
        entity.IsInsured = state.IsInsured;
        entity.IsLicensed = state.IsLicensed;
        entity.TeamSize = state.TeamSize;
        entity.BusinessAddress = state.BusinessAddress;
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
        entity.OnboardingMetaJson = SerializeMeta(state);
    }

    public async Task ActivateIndorProAsync(ProviderRegistrationState state, CancellationToken cancellationToken = default)
    {
        state.IndorProActive = true;
        state.UsesNewWizard = true;
        var proveedorId = await EnsureDraftAsync(cancellationToken);
        var entity = await db.IndorProveedores
            .Include(p => p.Categorias)
            .Include(p => p.Ofertas)
            .FirstAsync(p => p.Id == proveedorId, cancellationToken);

        ApplyToEntity(entity, state, ProviderRegistrationState.TotalSteps);
        SyncCategoriesOnEntity(entity, state.SelectedCategoryIds);
        SyncOfertasOnEntity(entity, state.SelectedServiceIds);
        entity.RegistrationStatus = ProviderRegistrationStatuses.IndorProActive;
        entity.ProfileSubmittedUtc ??= DateTime.UtcNow;
        entity.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public string ResolveWizardResumeAction(int currentStep) => currentStep switch
    {
        <= 1 => "Entry",
        2 => "CompanyInfo",
        3 => "Verification",
        4 => "CategoriesAssessment",
        _ => "ActivationCall"
    };

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
            EpaCertificationNumber = entity.EpaCertificationNumber,
            BackgroundCheckConsent = entity.BackgroundCheckConsent,
            ServiceDescription = entity.ServiceDescription ?? "",
            IsInsured = entity.IsInsured,
            IsLicensed = entity.IsLicensed,
            TeamSize = entity.TeamSize ?? "",
            BusinessAddress = entity.BusinessAddress ?? "",
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
        state.ServiceZipCodes = state.ServiceZipCodesDisplay;
        state.AvailableDays = DeserializeList(entity.AvailableDaysJson, ["Mon", "Tue", "Wed", "Thu", "Fri"]);
        state.JobSizes = DeserializeList(entity.JobSizesJson, ["small", "standard", "large"]);

        foreach (var answer in entity.ExamRespuestas)
        {
            state.ExamAnswers[answer.QuestionNumber] = answer.SelectedIndex.ToString();
        }

        ApplyMetaFromJson(state, entity.OnboardingMetaJson);
        return state;
    }

    private static string SerializeMeta(ProviderRegistrationState state)
    {
        var meta = new ProviderOnboardingMeta
        {
            OnboardingPath = state.OnboardingPath,
            AssessmentSkipped = state.AssessmentSkipped,
            AssessmentStarted = state.AssessmentStarted,
            TermsAccepted = state.TermsAccepted,
            Website = state.Website,
            EinNumber = state.EinNumber,
            ActivationCallSlot = state.ActivationCallSlot,
            ActivationCallScheduled = state.ActivationCallScheduled,
            IndorProActive = state.IndorProActive,
            UsesNewWizard = state.UsesNewWizard
        };

        return JsonSerializer.Serialize(meta, JsonOptions);
    }

    private static void ApplyMetaFromJson(ProviderRegistrationState state, string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        try
        {
            var meta = JsonSerializer.Deserialize<ProviderOnboardingMeta>(json, JsonOptions);
            if (meta == null)
            {
                return;
            }

            state.OnboardingPath = meta.OnboardingPath;
            state.AssessmentSkipped = meta.AssessmentSkipped;
            state.AssessmentStarted = meta.AssessmentStarted;
            state.TermsAccepted = meta.TermsAccepted;
            state.Website = meta.Website;
            state.EinNumber = meta.EinNumber;
            state.ActivationCallSlot = meta.ActivationCallSlot;
            state.ActivationCallScheduled = meta.ActivationCallScheduled;
            state.IndorProActive = meta.IndorProActive;
            state.UsesNewWizard = meta.UsesNewWizard;
        }
        catch (JsonException)
        {
            // Ignore invalid legacy payloads.
        }
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
