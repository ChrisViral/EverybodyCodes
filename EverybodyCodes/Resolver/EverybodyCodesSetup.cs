using System.Reflection;
using System.Text.Json;
using Challenge.CLI;
using Challenge.Utils.Extensions.Assemblies;
using EverybodyCodes.API;
using EverybodyCodes.API.Models;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace EverybodyCodes.Resolver;

/// <summary>
/// Everybody Codes program setup
/// </summary>
public sealed class EverybodyCodesSetup()
    : SolverSetup<EverybodyCodesSettings, EverybodyCodesResolver>(EverybodyCodesResolver.CHALLENGE_NAME)
{
    /// <inheritdoc />
    public override void ConfigureAPIClients(IServiceCollection services)
    {
        // Create refit settings
        JsonSerializerOptions options = SystemTextJsonContentSerializer.GetDefaultJsonSerializerOptions();
        options.TypeInfoResolver = ModelsContext.Default;
        RefitSettings refitSettings = new(new SystemTextJsonContentSerializer(options));

        // Setup user agent value
        Version fileVersion = Assembly.GetExecutingAssembly().GetFileVersion;
        string userAgent = $"ChrisViral.{typeof(EverybodyCodesResolver).FullName}/{fileVersion.ToString(2)} (https://github.com/ChrisViral/EverybodyCodes)";
        string cookie = $"everybody-codes={this.settings.Cookie}";

        // Add normal API client
        services.AddRefitClient<IEverybodyCodesAPI>(refitSettings)
                .ConfigureHttpClient(client =>
                 {
                     // Set address and headers
                     client.BaseAddress = new Uri("https://api.everybody.codes");
                     client.DefaultRequestHeaders.Add("cookie", cookie);
                     client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                 });

        // Add input API client
        services.AddRefitClient<IEverybodyCodesInputAPI>(refitSettings)
                .ConfigureHttpClient(client =>
                 {
                     // Set address and headers
                     client.BaseAddress = new Uri("https://everybody.codes");
                     client.DefaultRequestHeaders.Add("cookie", cookie);
                     client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                 });
    }
}
