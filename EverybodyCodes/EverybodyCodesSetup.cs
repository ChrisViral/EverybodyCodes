using System.Reflection;
using System.Text.Json;
using Challenge.CLI;
using Challenge.Solvers;
using Challenge.Utils.Extensions.Assemblies;
using EverybodyCodes.Resolver;
using EverybodyCodes.Resolver.Models;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Serilog;

namespace EverybodyCodes;

/// <summary>
/// Everybody Codes program setup
/// </summary>
public sealed class EverybodyCodesSetup() : Setup<EverybodyCodesResolverSettings>("Everybody Codes")
{
    /// <inheritdoc />
    public override Task CreateDefaultSettings(FileStream fileStream, CancellationToken token)
    {
        return JsonSerializer.SerializeAsync(fileStream,
                                             new EverybodyCodesResolverSettings(string.Empty, 0L, null),
                                             EverybodyCodesResolverSettingsJsonContext.Default.EverybodyCodesResolverSettings,
                                             token);
    }

    /// <inheritdoc />
    public override ValueTask<EverybodyCodesResolverSettings?> GetSettings(FileStream fileStream, CancellationToken token)
    {
        return JsonSerializer.DeserializeAsync(fileStream,
                                               EverybodyCodesResolverSettingsJsonContext.Default.EverybodyCodesResolverSettings,
                                               token);
    }

    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        // Add services
        services.AddSingleton<ISolverResolver, EverybodyCodesResolver>()
                .AddSingleton(this.settings)
                .AddLogging(builder => builder.AddSerilog(Log.Logger, true));

        // Create refit settings
        JsonSerializerOptions options = SystemTextJsonContentSerializer.GetDefaultJsonSerializerOptions();
        options.TypeInfoResolver = ModelsContext.Default;
        RefitSettings refitSettings = new(new SystemTextJsonContentSerializer(options));

        // Setup user agent value
        Version fileVersion = Assembly.GetExecutingAssembly().GetFileVersion;
        string userAgent = $"ChrisViral.{typeof(EverybodyCodesResolver).FullName}/{fileVersion.ToString(2)} (https://github.com/ChrisViral/EverybodyCodes)";

        // Add normal API client
        services.AddRefitClient<IEverybodyCodesAPI>(refitSettings)
                .ConfigureHttpClient(client =>
                 {
                     // Set address and headers
                     client.BaseAddress = new Uri("https://api.everybody.codes");
                     client.DefaultRequestHeaders.Add("cookie", $"everybody-codes={this.settings.Cookie}");
                     client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                 });

        // Add input API client
        services.AddRefitClient<IEverybodyCodesInputAPI>(refitSettings)
                .ConfigureHttpClient(client =>
                 {
                     // Set address and headers
                     client.BaseAddress = new Uri("https://everybody.codes");
                     client.DefaultRequestHeaders.Add("cookie", $"everybody-codes={this.settings.Cookie}");
                     client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                 });
    }
}
