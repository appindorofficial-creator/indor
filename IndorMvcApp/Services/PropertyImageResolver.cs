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
            .Where(IsPhotoDocument)
            .OrderByDescending(d => d.FechaCreacion)
            .Select(d => NormalizePath(d.StoragePath))
            .FirstOrDefault(path => !string.IsNullOrWhiteSpace(path));
    }

    private static bool IsPhotoDocument(PropiedadDocumento doc)
    {
        if (string.Equals(doc.Category, "Photo", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(doc.ContentType)
            && doc.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var name = doc.FileName ?? doc.StoragePath ?? string.Empty;
        var extension = Path.GetExtension(name).ToLowerInvariant();
        return ImageExtensions.Contains(extension);
    }

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
