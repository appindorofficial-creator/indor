using System.Text.RegularExpressions;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public partial class NeighborhoodFeedService(AppDbContext db, IIndorLocalizer localizer)
{
    private const string FilterAll = "All";

    private static readonly IReadOnlyList<(string Value, string LabelEn, string LabelEs, string Icon)> CategoryDefs =
    [
        (FilterAll, "All", "Todos", "fa-border-all"),
        ("Construction", "Construction", "Construcción", "fa-helmet-safety"),
        ("Home", "Home", "Hogar", "fa-house"),
        ("HVAC", "HVAC", "HVAC", "fa-wind"),
        ("Plumbing", "Plumbing", "Plomería", "fa-faucet-drip"),
        ("Services", "Services", "Servicios", "fa-screwdriver-wrench"),
        ("Recommendation", "Recommendation", "Recomendación", "fa-thumbs-up"),
        ("Question", "Question", "Pregunta", "fa-circle-question"),
        ("Other", "Other", "Otro", "fa-ellipsis")
    ];

    public static string? NormalizeZip(string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
        {
            return null;
        }

        var match = ZipRegex().Match(postalCode);
        return match.Success ? match.Value : postalCode.Trim();
    }

    public async Task<NeighborhoodFeedViewModel> BuildFeedAsync(
        string? userId,
        Propiedad? propiedad,
        PropertyInfoViewModel? propertyInfo,
        int notificationCount,
        string? tab,
        string? filter,
        string? search,
        string? zipOverride,
        IUrlHelper url,
        CancellationToken ct = default)
    {
        var homeZip = NormalizeZip(propertyInfo?.PostalCode);
        var browseZip = NormalizeZip(zipOverride);
        var zip = browseZip ?? homeZip;
        var isBrowsing = browseZip != null && !string.Equals(browseZip, homeZip, StringComparison.OrdinalIgnoreCase);

        var activeFilter = NormalizeCategory(filter);
        var activeTab = NormalizeTab(tab);

        if (propiedad == null || string.IsNullOrWhiteSpace(zip))
        {
            return new NeighborhoodFeedViewModel
            {
                HasProperty = propiedad != null,
                PropiedadId = propiedad?.Id,
                ZipCode = zip ?? string.Empty,
                HomeZip = homeZip ?? string.Empty,
                City = propertyInfo?.City,
                ActiveFilter = activeFilter,
                ActiveTab = activeTab,
                SearchQuery = search,
                NotificationCount = notificationCount,
                Tabs = BuildTabs(activeTab, activeFilter, search, zip, homeZip, url),
                Filters = BuildCategoryChips(activeFilter, activeTab, search, zip, homeZip, url),
                CreatePostUrl = url.Action("Create", "Neighborhood") ?? "#",
                SavedUrl = url.Action("Saved", "Neighborhood") ?? "#"
            };
        }

        var query = db.IndorNeighborhoodPosts
            .AsNoTracking()
            .Where(p => p.IsActive && p.ZipCode == zip);

        if (string.Equals(activeTab, "questions", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => p.PostType == "Question");
        }

        if (!string.Equals(activeFilter, FilterAll, StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => p.CategoryCode == activeFilter);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                (p.Title != null && EF.Functions.Like(p.Title, $"%{term}%")) ||
                EF.Functions.Like(p.Body, $"%{term}%") ||
                EF.Functions.Like(p.AuthorName, $"%{term}%"));
        }

        var posts = await query
            .OrderByDescending(p => p.CreatedUtc)
            .Take(60)
            .ToListAsync(ct);

        var postIds = posts.Select(p => p.Id).ToList();
        var savedIds = await LoadSavedPostIdsAsync(userId, postIds, ct);
        var media = await LoadMediaAsync(postIds, ct);

        var cards = posts
            .Select(p => BuildCard(p, userId, savedIds.Contains(p.Id), media.GetValueOrDefault(p.Id), url))
            .ToList();

        return new NeighborhoodFeedViewModel
        {
            HasProperty = true,
            PropiedadId = propiedad.Id,
            ZipCode = zip,
            HomeZip = homeZip ?? string.Empty,
            City = propertyInfo?.City,
            ActiveFilter = activeFilter,
            ActiveTab = activeTab,
            SearchQuery = search,
            NotificationCount = notificationCount,
            Tabs = BuildTabs(activeTab, activeFilter, search, zip, homeZip, url),
            Filters = BuildCategoryChips(activeFilter, activeTab, search, zip, homeZip, url),
            Posts = cards,
            CreatePostUrl = url.Action("Create", "Neighborhood") ?? "#",
            SavedUrl = url.Action("Saved", "Neighborhood") ?? "#",
            IsBrowsingOtherZip = isBrowsing
        };
    }

    private async Task<HashSet<int>> LoadSavedPostIdsAsync(string? userId, List<int> postIds, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId) || postIds.Count == 0)
        {
            return [];
        }

        return (await db.IndorNeighborhoodPostSaves.AsNoTracking()
            .Where(s => s.UserId == userId && postIds.Contains(s.PostId))
            .Select(s => s.PostId)
            .ToListAsync(ct)).ToHashSet();
    }

    private async Task<Dictionary<int, List<NeighborhoodMediaViewModel>>> LoadMediaAsync(
        List<int> postIds, CancellationToken ct)
    {
        if (postIds.Count == 0)
        {
            return [];
        }

        var rows = await db.IndorNeighborhoodPostMedia.AsNoTracking()
            .Where(m => postIds.Contains(m.PostId))
            .OrderBy(m => m.PostId).ThenBy(m => m.SortOrder)
            .ToListAsync(ct);

        return rows
            .GroupBy(m => m.PostId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(m => new NeighborhoodMediaViewModel
                {
                    Path = m.FilePath,
                    IsVideo = string.Equals(m.MediaType, "video", StringComparison.OrdinalIgnoreCase)
                }).ToList());
    }

    public async Task<NeighborhoodPostDetailViewModel?> BuildDetailAsync(
        string? userId,
        int postId,
        IUrlHelper url,
        CancellationToken ct = default)
    {
        var post = await db.IndorNeighborhoodPosts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postId && p.IsActive, ct);

        if (post == null)
        {
            return null;
        }

        var saved = !string.IsNullOrWhiteSpace(userId)
            && await db.IndorNeighborhoodPostSaves.AsNoTracking()
                .AnyAsync(s => s.PostId == postId && s.UserId == userId, ct);

        var media = await LoadMediaAsync([postId], ct);

        var allComments = await db.IndorNeighborhoodComments
            .AsNoTracking()
            .Where(c => c.PostId == postId && c.IsActive)
            .ToListAsync(ct);

        var savedCommentIds = new HashSet<int>();
        if (!string.IsNullOrWhiteSpace(userId) && allComments.Count > 0)
        {
            var ids = allComments.Select(c => c.Id).ToList();
            savedCommentIds = (await db.IndorNeighborhoodCommentSaves.AsNoTracking()
                .Where(s => s.UserId == userId && ids.Contains(s.CommentId))
                .Select(s => s.CommentId)
                .ToListAsync(ct)).ToHashSet();
        }

        var repliesByParent = allComments
            .Where(c => c.ParentCommentId.HasValue)
            .GroupBy(c => c.ParentCommentId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.CreatedUtc).ToList());

        // Top-level comments newest first; replies chronological.
        var topLevel = allComments
            .Where(c => !c.ParentCommentId.HasValue)
            .OrderByDescending(c => c.CreatedUtc)
            .Select(c => MapComment(c, userId, savedCommentIds, repliesByParent, url))
            .ToList();

        return new NeighborhoodPostDetailViewModel
        {
            Post = BuildCard(post, userId, saved, media.GetValueOrDefault(postId), url),
            Comments = topLevel,
            TotalCommentCount = allComments.Count,
            BackUrl = url.Action("Index", "Neighborhood") ?? "#",
            CanComment = !string.IsNullOrWhiteSpace(userId)
        };
    }

    private NeighborhoodCommentViewModel MapComment(
        IndorNeighborhoodComment c,
        string? userId,
        HashSet<int> savedCommentIds,
        Dictionary<int, List<IndorNeighborhoodComment>> repliesByParent,
        IUrlHelper url)
    {
        var replies = repliesByParent.TryGetValue(c.Id, out var list)
            ? list.Select(r => new NeighborhoodCommentViewModel
            {
                Id = r.Id,
                AuthorName = r.AuthorName,
                AuthorPhotoUrl = r.AuthorPhotoUrl,
                AuthorInitials = Initials(r.AuthorName),
                ZipCode = r.ZipCode ?? string.Empty,
                TimeAgo = TimeAgo(r.CreatedUtc),
                Body = r.Body,
                IsMine = !string.IsNullOrWhiteSpace(userId) && r.UserId == userId,
                SavedByMe = savedCommentIds.Contains(r.Id)
            }).ToList()
            : [];

        return new NeighborhoodCommentViewModel
        {
            Id = c.Id,
            AuthorName = c.AuthorName,
            AuthorPhotoUrl = c.AuthorPhotoUrl,
            AuthorInitials = Initials(c.AuthorName),
            ZipCode = c.ZipCode ?? string.Empty,
            TimeAgo = TimeAgo(c.CreatedUtc),
            Body = c.Body,
            IsMine = !string.IsNullOrWhiteSpace(userId) && c.UserId == userId,
            SavedByMe = savedCommentIds.Contains(c.Id),
            Replies = replies
        };
    }

    public async Task<NeighborhoodSavedViewModel> BuildSavedAsync(
        string userId,
        string? tab,
        string? category,
        IUrlHelper url,
        CancellationToken ct = default)
    {
        var activeTab = string.Equals(tab, "comments", StringComparison.OrdinalIgnoreCase) ? "comments" : "posts";
        var activeCategory = NormalizeCategory(category);

        var savedPosts = new List<NeighborhoodPostCardViewModel>();
        var savedComments = new List<NeighborhoodCommentViewModel>();

        if (activeTab == "posts")
        {
            var savedRows = await db.IndorNeighborhoodPostSaves.AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedUtc)
                .Select(s => s.PostId)
                .ToListAsync(ct);

            if (savedRows.Count > 0)
            {
                var posts = await db.IndorNeighborhoodPosts.AsNoTracking()
                    .Where(p => savedRows.Contains(p.Id) && p.IsActive)
                    .ToListAsync(ct);

                if (!string.Equals(activeCategory, FilterAll, StringComparison.OrdinalIgnoreCase))
                {
                    posts = posts.Where(p => string.Equals(
                        NeighborhoodPostCategories.Resolve(p.CategoryCode).Code, activeCategory,
                        StringComparison.OrdinalIgnoreCase)).ToList();
                }

                var media = await LoadMediaAsync(posts.Select(p => p.Id).ToList(), ct);
                savedPosts = savedRows
                    .Select(id => posts.FirstOrDefault(p => p.Id == id))
                    .Where(p => p != null)
                    .Select(p => BuildCard(p!, userId, true, media.GetValueOrDefault(p!.Id), url))
                    .ToList();
            }
        }
        else
        {
            var commentIds = await db.IndorNeighborhoodCommentSaves.AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedUtc)
                .Select(s => s.CommentId)
                .ToListAsync(ct);

            if (commentIds.Count > 0)
            {
                var comments = await db.IndorNeighborhoodComments.AsNoTracking()
                    .Where(c => commentIds.Contains(c.Id) && c.IsActive)
                    .ToListAsync(ct);
                var relatedPostIds = comments.Select(c => c.PostId).Distinct().ToList();
                var posts = await db.IndorNeighborhoodPosts.AsNoTracking()
                    .Where(p => relatedPostIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.Title, p.Body })
                    .ToListAsync(ct);

                savedComments = commentIds
                    .Select(id => comments.FirstOrDefault(c => c.Id == id))
                    .Where(c => c != null)
                    .Select(c =>
                    {
                        var parent = posts.FirstOrDefault(p => p.Id == c!.PostId);
                        var context = parent?.Title;
                        if (string.IsNullOrWhiteSpace(context) && parent != null)
                        {
                            context = parent.Body.Length > 60 ? parent.Body[..60] + "…" : parent.Body;
                        }
                        return new NeighborhoodCommentViewModel
                        {
                            Id = c!.Id,
                            AuthorName = c.AuthorName,
                            AuthorPhotoUrl = c.AuthorPhotoUrl,
                            AuthorInitials = Initials(c.AuthorName),
                            TimeAgo = TimeAgo(c.CreatedUtc),
                            Body = c.Body,
                            SavedByMe = true,
                            PostTitleContext = context,
                            PostDetailUrl = url.Action("Detail", "Neighborhood", new { id = c.PostId }) ?? "#"
                        };
                    }).ToList();
            }
        }

        return new NeighborhoodSavedViewModel
        {
            ActiveTab = activeTab,
            ActiveCategory = activeCategory,
            CategoryFilters = CategoryDefs.Select(f => new NeighborhoodFilterChipViewModel
            {
                Label = localizer.IsSpanish ? f.LabelEs : f.LabelEn,
                Value = f.Value,
                Icon = f.Icon,
                Url = url.Action("Saved", "Neighborhood", new { tab = "posts", category = f.Value == FilterAll ? null : f.Value }) ?? "#",
                IsActive = string.Equals(f.Value, activeCategory, StringComparison.OrdinalIgnoreCase)
            }).ToList(),
            Posts = savedPosts,
            Comments = savedComments,
            PostsTabUrl = url.Action("Saved", "Neighborhood", new { tab = "posts" }) ?? "#",
            CommentsTabUrl = url.Action("Saved", "Neighborhood", new { tab = "comments" }) ?? "#",
            BackUrl = url.Action("Index", "Neighborhood") ?? "#"
        };
    }

    public NeighborhoodCreatePostViewModel BuildCreate(
        Propiedad? propiedad,
        PropertyInfoViewModel? propertyInfo,
        IUrlHelper url)
    {
        var zip = NormalizeZip(propertyInfo?.PostalCode) ?? string.Empty;
        return new NeighborhoodCreatePostViewModel
        {
            PropiedadId = propiedad?.Id,
            ZipCode = zip,
            CategoryCode = "Construction",
            PostType = "WorkDone",
            Audience = "Public",
            Categories = NeighborhoodPostCategories.All.Select(c => new NeighborhoodCategoryOptionViewModel
            {
                Code = c.Code,
                Label = localizer.IsSpanish ? c.LabelEs : c.LabelEn,
                Css = c.Css
            }).ToList(),
            Types = NeighborhoodPostTypes.All.Select(t => new NeighborhoodCategoryOptionViewModel
            {
                Code = t.Code,
                Label = localizer.IsSpanish ? t.LabelEs : t.LabelEn,
                Css = t.Css,
                Icon = t.Icon
            }).ToList(),
            Audiences = NeighborhoodAudiences.All.Select(a => new NeighborhoodCategoryOptionViewModel
            {
                Code = a.Code,
                Label = localizer.IsSpanish ? a.LabelEs : a.LabelEn,
                Icon = a.Icon
            }).ToList(),
            BackUrl = url.Action("Index", "Neighborhood") ?? "#"
        };
    }

    public NeighborhoodPublishedViewModel BuildPublished(int postId, IUrlHelper url) => new()
    {
        PostId = postId,
        ViewPostUrl = url.Action("Detail", "Neighborhood", new { id = postId }) ?? "#",
        FeedUrl = url.Action("Index", "Neighborhood") ?? "#",
        CreateAnotherUrl = url.Action("Create", "Neighborhood") ?? "#"
    };

    public async Task<int?> CreatePostAsync(
        string userId,
        ApplicationUser user,
        Propiedad propiedad,
        string zip,
        NeighborhoodCreatePostViewModel model,
        IReadOnlyList<(string Path, bool IsVideo)>? media,
        CancellationToken ct = default)
    {
        var body = model.Body?.Trim();
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        var category = NeighborhoodPostCategories.Resolve(model.CategoryCode);
        var type = NeighborhoodPostTypes.Resolve(model.PostType);
        var audience = NeighborhoodAudiences.Resolve(model.Audience);

        var post = new IndorNeighborhoodPost
        {
            UserId = userId,
            PropiedadId = propiedad.Id,
            ZipCode = zip,
            AuthorName = BuildDisplayName(user),
            AuthorPhotoUrl = user.FotoUrl,
            CategoryCode = category.Code,
            PostType = type?.Code,
            Audience = audience.Code,
            Title = string.IsNullOrWhiteSpace(model.Title) ? null : model.Title.Trim(),
            Body = body.Length > 500 ? body[..500] : body,
            ImagePath = media is { Count: > 0 } ? media[0].Path : null,
            LocationLabel = string.IsNullOrWhiteSpace(model.LocationLabel) ? null : model.LocationLabel.Trim(),
            CreatedUtc = DateTime.UtcNow
        };

        if (media is { Count: > 0 })
        {
            var order = 0;
            foreach (var item in media)
            {
                post.Media.Add(new IndorNeighborhoodPostMedia
                {
                    FilePath = item.Path,
                    MediaType = item.IsVideo ? "video" : "image",
                    SortOrder = order++,
                    CreatedUtc = DateTime.UtcNow
                });
            }
        }

        db.IndorNeighborhoodPosts.Add(post);
        await db.SaveChangesAsync(ct);
        return post.Id;
    }

    public async Task<bool> AddCommentAsync(
        string userId,
        ApplicationUser user,
        int postId,
        string body,
        int? parentCommentId,
        string? zip,
        CancellationToken ct = default)
    {
        var text = body?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var post = await db.IndorNeighborhoodPosts.FirstOrDefaultAsync(p => p.Id == postId && p.IsActive, ct);
        if (post == null)
        {
            return false;
        }

        // Only allow replying to a comment that belongs to this post.
        if (parentCommentId.HasValue)
        {
            var parentOk = await db.IndorNeighborhoodComments
                .AnyAsync(c => c.Id == parentCommentId.Value && c.PostId == postId && c.IsActive, ct);
            if (!parentOk)
            {
                parentCommentId = null;
            }
        }

        db.IndorNeighborhoodComments.Add(new IndorNeighborhoodComment
        {
            PostId = postId,
            ParentCommentId = parentCommentId,
            UserId = userId,
            AuthorName = BuildDisplayName(user),
            AuthorPhotoUrl = user.FotoUrl,
            ZipCode = NormalizeZip(zip),
            Body = text.Length > 1000 ? text[..1000] : text,
            CreatedUtc = DateTime.UtcNow
        });
        post.CommentCount += 1;
        post.UpdatedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeletePostAsync(string userId, int postId, CancellationToken ct = default)
    {
        var post = await db.IndorNeighborhoodPosts.FirstOrDefaultAsync(p => p.Id == postId, ct);
        if (post == null || post.UserId != userId)
        {
            return false;
        }

        post.IsActive = false;
        post.UpdatedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ToggleCommentSaveAsync(string userId, int commentId, CancellationToken ct = default)
    {
        var comment = await db.IndorNeighborhoodComments.FirstOrDefaultAsync(c => c.Id == commentId && c.IsActive, ct);
        if (comment == null)
        {
            return false;
        }

        var existing = await db.IndorNeighborhoodCommentSaves
            .FirstOrDefaultAsync(s => s.CommentId == commentId && s.UserId == userId, ct);

        bool saved;
        if (existing != null)
        {
            db.IndorNeighborhoodCommentSaves.Remove(existing);
            saved = false;
        }
        else
        {
            db.IndorNeighborhoodCommentSaves.Add(new IndorNeighborhoodCommentSave
            {
                CommentId = commentId,
                UserId = userId,
                CreatedUtc = DateTime.UtcNow
            });
            saved = true;
        }

        await db.SaveChangesAsync(ct);
        return saved;
    }

    public async Task<bool> ToggleSaveAsync(string userId, int postId, CancellationToken ct = default)
    {
        var post = await db.IndorNeighborhoodPosts.FirstOrDefaultAsync(p => p.Id == postId && p.IsActive, ct);
        if (post == null)
        {
            return false;
        }

        var existing = await db.IndorNeighborhoodPostSaves
            .FirstOrDefaultAsync(s => s.PostId == postId && s.UserId == userId, ct);

        bool saved;
        if (existing != null)
        {
            db.IndorNeighborhoodPostSaves.Remove(existing);
            saved = false;
        }
        else
        {
            db.IndorNeighborhoodPostSaves.Add(new IndorNeighborhoodPostSave
            {
                PostId = postId,
                UserId = userId,
                CreatedUtc = DateTime.UtcNow
            });
            saved = true;
        }

        await db.SaveChangesAsync(ct);
        return saved;
    }

    private NeighborhoodPostCardViewModel BuildCard(
        IndorNeighborhoodPost post,
        string? userId,
        bool savedByMe,
        List<NeighborhoodMediaViewModel>? media,
        IUrlHelper url)
    {
        var category = NeighborhoodPostCategories.Resolve(post.CategoryCode);
        var type = NeighborhoodPostTypes.Resolve(post.PostType);
        var mediaList = media ?? [];
        if (mediaList.Count == 0 && !string.IsNullOrWhiteSpace(post.ImagePath))
        {
            mediaList = [new NeighborhoodMediaViewModel { Path = post.ImagePath!, IsVideo = false }];
        }

        var detailUrl = url.Action("Detail", "Neighborhood", new { id = post.Id }) ?? "#";

        return new NeighborhoodPostCardViewModel
        {
            Id = post.Id,
            AuthorName = post.AuthorName,
            AuthorPhotoUrl = post.AuthorPhotoUrl,
            AuthorInitials = Initials(post.AuthorName),
            ZipCode = post.ZipCode,
            TimeAgo = TimeAgo(post.CreatedUtc),
            CategoryLabel = localizer.IsSpanish ? category.LabelEs : category.LabelEn,
            CategoryCss = category.Css,
            TypeLabel = type is null ? null : (localizer.IsSpanish ? type.LabelEs : type.LabelEn),
            TypeCss = type?.Css ?? "workdone",
            Title = post.Title,
            Body = post.Body,
            Media = mediaList,
            LocationLabel = post.LocationLabel,
            HasProvider = post.ProveedorId.HasValue,
            ProviderUrl = post.ProveedorId.HasValue
                ? (url.Action("Index", "Home") ?? "/") + "#section-services"
                : null,
            CommentCount = post.CommentCount,
            SavedByMe = savedByMe,
            IsMine = !string.IsNullOrWhiteSpace(userId) && post.UserId == userId,
            DetailUrl = detailUrl,
            ShareUrl = detailUrl
        };
    }

    private IReadOnlyList<NeighborhoodFilterChipViewModel> BuildTabs(
        string activeTab, string activeFilter, string? search, string? zip, string? homeZip, IUrlHelper url)
    {
        object? ZipArg() => (zip != null && !string.Equals(zip, homeZip, StringComparison.OrdinalIgnoreCase)) ? zip : null;

        return
        [
            new NeighborhoodFilterChipViewModel
            {
                Label = localizer.IsSpanish ? "Todos" : "All",
                Value = "all",
                Icon = "fa-layer-group",
                Url = url.Action("Index", "Neighborhood", new { tab = (string?)null, q = search, zip = ZipArg() }) ?? "#",
                IsActive = activeTab == "all"
            },
            new NeighborhoodFilterChipViewModel
            {
                Label = localizer.IsSpanish ? "Más recientes" : "Most recent",
                Value = "recent",
                Icon = "fa-clock",
                Url = url.Action("Index", "Neighborhood", new { tab = "recent", q = search, zip = ZipArg() }) ?? "#",
                IsActive = activeTab == "recent"
            },
            new NeighborhoodFilterChipViewModel
            {
                Label = localizer.IsSpanish ? "Guardados" : "Saved",
                Value = "saved",
                Icon = "fa-bookmark",
                Url = url.Action("Saved", "Neighborhood") ?? "#",
                IsActive = false
            },
            new NeighborhoodFilterChipViewModel
            {
                Label = localizer.IsSpanish ? "Preguntas" : "Questions",
                Value = "questions",
                Icon = "fa-circle-question",
                Url = url.Action("Index", "Neighborhood", new { tab = "questions", q = search, zip = ZipArg() }) ?? "#",
                IsActive = activeTab == "questions"
            }
        ];
    }

    private IReadOnlyList<NeighborhoodFilterChipViewModel> BuildCategoryChips(
        string activeFilter, string activeTab, string? search, string? zip, string? homeZip, IUrlHelper url)
    {
        object? ZipArg() => (zip != null && !string.Equals(zip, homeZip, StringComparison.OrdinalIgnoreCase)) ? zip : null;

        return CategoryDefs.Select(f => new NeighborhoodFilterChipViewModel
        {
            Label = localizer.IsSpanish ? f.LabelEs : f.LabelEn,
            Value = f.Value,
            Icon = f.Icon,
            Url = url.Action("Index", "Neighborhood", new
            {
                filter = f.Value == FilterAll ? null : f.Value,
                tab = activeTab == "all" ? null : activeTab,
                q = search,
                zip = ZipArg()
            }) ?? "#",
            IsActive = string.Equals(f.Value, activeFilter, StringComparison.OrdinalIgnoreCase)
        }).ToList();
    }

    private static string NormalizeCategory(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return FilterAll;
        }

        foreach (var f in CategoryDefs)
        {
            if (string.Equals(f.Value, filter, StringComparison.OrdinalIgnoreCase))
            {
                return f.Value;
            }
        }

        return FilterAll;
    }

    private static string NormalizeTab(string? tab) => tab?.ToLowerInvariant() switch
    {
        "recent" => "recent",
        "questions" => "questions",
        _ => "all"
    };

    private static string BuildDisplayName(ApplicationUser user)
    {
        var first = (user.Nombre ?? string.Empty).Trim();
        var last = (user.Apellidos ?? string.Empty).Trim();
        var lastInitial = last.Length > 0 ? $" {last[0]}." : string.Empty;
        var name = $"{first}{lastInitial}".Trim();
        return string.IsNullOrWhiteSpace(name) ? "INDOR" : name;
    }

    private static string Initials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return "?";
        }

        var first = char.ToUpperInvariant(parts[0][0]);
        if (parts.Length == 1)
        {
            return first.ToString();
        }

        var second = char.ToUpperInvariant(parts[^1][0]);
        return $"{first}{second}";
    }

    private string TimeAgo(DateTime utc)
    {
        var span = DateTime.UtcNow - utc;
        var es = localizer.IsSpanish;

        if (span.TotalMinutes < 1)
        {
            return es ? "ahora" : "just now";
        }
        if (span.TotalMinutes < 60)
        {
            var m = (int)span.TotalMinutes;
            return es ? $"hace {m} min" : $"{m}m ago";
        }
        if (span.TotalHours < 24)
        {
            var h = (int)span.TotalHours;
            return es ? $"hace {h} h" : $"{h}h ago";
        }
        if (span.TotalDays < 7)
        {
            var d = (int)span.TotalDays;
            return es ? $"hace {d} d" : $"{d}d ago";
        }

        var w = (int)(span.TotalDays / 7);
        return es ? $"hace {w} sem" : $"{w}w ago";
    }

    [GeneratedRegex(@"\d{5}")]
    private static partial Regex ZipRegex();
}
