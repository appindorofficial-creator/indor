namespace IndorMvcApp.ViewModels;

public sealed class NeighborhoodFeedViewModel
{
    public bool HasProperty { get; init; }
    public int? PropiedadId { get; init; }
    public string ZipCode { get; init; } = string.Empty;
    public string? City { get; init; }
    public string ActiveFilter { get; init; } = "All";
    public string ActiveTab { get; init; } = "all";
    public string? SearchQuery { get; init; }
    public int NotificationCount { get; init; }
    public IReadOnlyList<NeighborhoodFilterChipViewModel> Tabs { get; init; } = [];
    public IReadOnlyList<NeighborhoodFilterChipViewModel> Filters { get; init; } = [];
    public IReadOnlyList<NeighborhoodPostCardViewModel> Posts { get; init; } = [];
    public string CreatePostUrl { get; init; } = "#";
    public string SavedUrl { get; init; } = "#";
    public bool IsBrowsingOtherZip { get; init; }
    public string HomeZip { get; init; } = string.Empty;
}

public sealed class NeighborhoodMediaViewModel
{
    public string Path { get; init; } = string.Empty;
    public bool IsVideo { get; init; }
}

public sealed class NeighborhoodFilterChipViewModel
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string Url { get; init; } = "#";
    public bool IsActive { get; init; }
}

public sealed class NeighborhoodPostCardViewModel
{
    public int Id { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string? AuthorPhotoUrl { get; init; }
    public string AuthorInitials { get; init; } = "?";
    public string ZipCode { get; init; } = string.Empty;
    public string TimeAgo { get; init; } = string.Empty;
    public string? CategoryLabel { get; init; }
    public string CategoryCss { get; init; } = "general";
    public string? TypeLabel { get; init; }
    public string TypeCss { get; init; } = "workdone";
    public string? Title { get; init; }
    public string Body { get; init; } = string.Empty;
    public IReadOnlyList<NeighborhoodMediaViewModel> Media { get; init; } = [];
    public string? LocationLabel { get; init; }
    public bool HasProvider { get; init; }
    public string? ProviderUrl { get; init; }
    public int CommentCount { get; init; }
    public bool SavedByMe { get; init; }
    public bool IsMine { get; init; }
    public string DetailUrl { get; init; } = "#";
    public string ShareUrl { get; init; } = "#";
}

public sealed class NeighborhoodPostDetailViewModel
{
    public NeighborhoodPostCardViewModel Post { get; init; } = new();
    public IReadOnlyList<NeighborhoodCommentViewModel> Comments { get; init; } = [];
    public int TotalCommentCount { get; init; }
    public string BackUrl { get; init; } = "#";
    public bool CanComment { get; init; } = true;
}

public sealed class NeighborhoodCommentViewModel
{
    public int Id { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string? AuthorPhotoUrl { get; init; }
    public string AuthorInitials { get; init; } = "?";
    public string ZipCode { get; init; } = string.Empty;
    public string TimeAgo { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public bool IsMine { get; init; }
    public bool SavedByMe { get; init; }
    public string? PostTitleContext { get; init; }
    public string? PostDetailUrl { get; init; }
    public IReadOnlyList<NeighborhoodCommentViewModel> Replies { get; init; } = [];
}

public sealed class NeighborhoodSavedViewModel
{
    public string ActiveTab { get; init; } = "posts";
    public string ActiveCategory { get; init; } = "All";
    public IReadOnlyList<NeighborhoodFilterChipViewModel> CategoryFilters { get; init; } = [];
    public IReadOnlyList<NeighborhoodPostCardViewModel> Posts { get; init; } = [];
    public IReadOnlyList<NeighborhoodCommentViewModel> Comments { get; init; } = [];
    public string PostsTabUrl { get; init; } = "#";
    public string CommentsTabUrl { get; init; } = "#";
    public string BackUrl { get; init; } = "#";
}

public sealed class NeighborhoodCreatePostViewModel
{
    public int? PropiedadId { get; init; }
    public string ZipCode { get; init; } = string.Empty;
    public string? Title { get; set; }
    public string Body { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = "Construction";
    public string PostType { get; set; } = "WorkDone";
    public string Audience { get; set; } = "Public";
    public string? LocationLabel { get; set; }
    public IReadOnlyList<NeighborhoodCategoryOptionViewModel> Categories { get; init; } = [];
    public IReadOnlyList<NeighborhoodCategoryOptionViewModel> Types { get; init; } = [];
    public IReadOnlyList<NeighborhoodCategoryOptionViewModel> Audiences { get; init; } = [];
    public string BackUrl { get; init; } = "#";
}

public sealed class NeighborhoodCategoryOptionViewModel
{
    public string Code { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Css { get; init; } = "general";
    public string Icon { get; init; } = string.Empty;
}

public sealed class NeighborhoodPublishedViewModel
{
    public int PostId { get; init; }
    public string ViewPostUrl { get; init; } = "#";
    public string FeedUrl { get; init; } = "#";
    public string CreateAnotherUrl { get; init; } = "#";
}
