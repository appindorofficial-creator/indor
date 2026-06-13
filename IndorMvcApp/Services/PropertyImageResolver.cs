using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class PropertyImageResolver
{
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    public static string Resolve(
        IEnumerable<PropiedadDocumento>? documents,
        string? hvacLabelPath = null,
        string? waterHeaterLabelPath = null,
        string fallback = "/welcome-house.png")
    {
        var photo = FindUserPhoto(documents);
        if (!string.IsNullOrWhiteSpace(photo))
        {
            return photo;
        }

        if (!string.IsNullOrWhiteSpace(hvacLabelPath) && IsImagePath(hvacLabelPath))
        {
            return hvacLabelPath;
        }

        if (!string.IsNullOrWhiteSpace(waterHeaterLabelPath) && IsImagePath(waterHeaterLabelPath))
        {
            return waterHeaterLabelPath;
        }

        return fallback;
    }

    private static string? FindUserPhoto(IEnumerable<PropiedadDocumento>? documents)
    {
        if (documents == null)
        {
            return null;
        }

        return documents
            .Where(IsPropertyPhotoDocument)
            .OrderByDescending(d => d.FechaCreacion)
            .Select(d => NormalizePath(d.StoragePath))
            .FirstOrDefault(path => !string.IsNullOrWhiteSpace(path));
    }

    private static bool IsPropertyPhotoDocument(PropiedadDocumento doc) =>
        string.Equals(doc.Category, "Photo", StringComparison.OrdinalIgnoreCase)
        || string.Equals(doc.Category, "Photos", StringComparison.OrdinalIgnoreCase);

    private static string? NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return path.StartsWith('/') ? path : $"/{path}";
    }

    private static bool IsImagePath(string path) =>
        ImageExtensions.Contains(Path.GetExtension(path).ToLowerInvariant());
}
