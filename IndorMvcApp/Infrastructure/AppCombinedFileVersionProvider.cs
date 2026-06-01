using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IndorMvcApp.Services;

namespace IndorMvcApp.Infrastructure;

/// <summary>
/// Combines deploy version (App:Version) with per-file hash.
/// Example: ~/css/site.css?v=2.a1b2c3d4e5f6
/// </summary>
public sealed class AppCombinedFileVersionProvider : IFileVersionProvider
{
    private readonly IWebHostEnvironment _env;
    private readonly string _appVersion;

    public AppCombinedFileVersionProvider(IWebHostEnvironment env, IAppVersionService appVersion)
    {
        _env = env;
        _appVersion = appVersion.Version;
    }

    public string AddFileVersionToPath(PathString path, string? query)
    {
        var pathValue = path.Value ?? string.Empty;
        var fileHash = GetFileHash(pathValue);
        var versionToken = string.IsNullOrEmpty(fileHash) ? _appVersion : $"{_appVersion}.{fileHash}";

        if (string.IsNullOrEmpty(query))
        {
            return QueryHelpers.AddQueryString(pathValue, "v", versionToken);
        }

        return QueryHelpers.AddQueryString($"{pathValue}?{query}", "v", versionToken);
    }

    private string? GetFileHash(string pathValue)
    {
        var relativePath = pathValue.TrimStart('/');
        if (string.IsNullOrEmpty(relativePath)) return null;

        var file = _env.WebRootFileProvider.GetFileInfo(relativePath);
        if (!file.Exists) return null;

        using var stream = file.CreateReadStream();
        var hashBytes = SHA256.HashData(stream);
        return Convert.ToHexString(hashBytes)[..12].ToLowerInvariant();
    }

    public static void Register(IServiceCollection services)
    {
        services.RemoveAll<IFileVersionProvider>();
        services.AddSingleton<IFileVersionProvider, AppCombinedFileVersionProvider>();
    }
}
