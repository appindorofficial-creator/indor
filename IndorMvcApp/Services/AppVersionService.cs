namespace IndorMvcApp.Services;

public interface IAppVersionService
{
    string Version { get; }
}

public sealed class AppVersionService : IAppVersionService
{
    public AppVersionService(IConfiguration configuration)
    {
        Version = configuration["App:Version"]?.Trim() ?? "1";
    }

    public string Version { get; }
}
