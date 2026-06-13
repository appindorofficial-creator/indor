using System.Text;
using UglyToad.PdfPig;

namespace IndorMvcApp.Services;

public static class InspectionReportTextExtractor
{
    private const int MaxCharsSingleRequest = 120_000;
    public const int PagesPerChunk = 10;

    public static (string Text, int PageCount) ExtractFromFile(string fullPath)
    {
        var pages = ExtractAllPages(fullPath);
        if (pages.Count == 0)
        {
            return (string.Empty, 0);
        }

        var builder = new StringBuilder();
        foreach (var (pageNumber, pageText) in pages)
        {
            builder.AppendLine($"--- Page {pageNumber} ---");
            builder.AppendLine(pageText);
            if (builder.Length >= MaxCharsSingleRequest)
            {
                break;
            }
        }

        var text = builder.ToString();
        if (text.Length > MaxCharsSingleRequest)
        {
            text = text[..MaxCharsSingleRequest];
        }

        return (text, pages.Count);
    }

    public static IReadOnlyList<(int PageNumber, string Text)> ExtractAllPages(string fullPath)
    {
        if (!File.Exists(fullPath))
        {
            return [];
        }

        var ext = Path.GetExtension(fullPath).ToLowerInvariant();
        if (ext != ".pdf")
        {
            return ext is ".jpg" or ".jpeg" or ".png" or ".webp"
                ? [(1, string.Empty)]
                : [];
        }

        try
        {
            using var document = PdfDocument.Open(fullPath);
            return document.GetPages()
                .Select(page => (page.Number, page.Text ?? string.Empty))
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    public static IReadOnlyList<string> BuildPageChunks(
        IReadOnlyList<(int PageNumber, string Text)> pages,
        int pagesPerChunk = PagesPerChunk)
    {
        if (pages.Count == 0)
        {
            return [];
        }

        var chunks = new List<string>();
        for (var i = 0; i < pages.Count; i += pagesPerChunk)
        {
            var builder = new StringBuilder();
            foreach (var (pageNumber, pageText) in pages.Skip(i).Take(pagesPerChunk))
            {
                builder.AppendLine($"--- Page {pageNumber} ---");
                builder.AppendLine(pageText);
            }

            var chunk = builder.ToString().Trim();
            if (chunk.Length > 0)
            {
                chunks.Add(chunk);
            }
        }

        return chunks;
    }

    public static int TotalCharCount(IReadOnlyList<(int PageNumber, string Text)> pages) =>
        pages.Sum(p => p.Text.Length);
}
