using System.Text.Json.Serialization.Metadata;
using Challenge.CLI;
using Challenge.Solvers;
using CSharpFunctionalExtensions;
using EverybodyCodes.Resolver.Models;
using Microsoft.Extensions.Logging;

namespace EverybodyCodes.Resolver;

/// <summary>
/// Solver resolver and input fetcher
/// </summary>
/// <param name="logger">Logger instance</param>
/// <param name="settings">Resolver settings</param>
public sealed class SolverResolver(ILogger<SolverResolver> logger, EverybodyCodesResolverSettings settings, IEverybodyCodesAPI api)
    : SolverResolverBase<EverybodyCodesResolverSettings>(logger, settings)
{
    /// <inheritdoc />
    public override string ChallengeName => "Everybody Codes";

    /// <inheritdoc />
    protected override TimeSpan RateLimit { get; } = TimeSpan.FromSeconds(60L);

    /// <inheritdoc />
    protected override JsonTypeInfo<EverybodyCodesResolverSettings> SettingsTypeInfo => EverybodyCodesResolverSettingsJsonContext.Default.EverybodyCodesResolverSettings;

    /// <summary>
    /// Everybody Codes API
    /// </summary>
    private IEverybodyCodesAPI API { get; } = api;

    /// <inheritdoc />
    public override async Task<Result> SubmitAnswer(string answer, CancellationToken token = default)
    {
        return default;
    }

    /// <inheritdoc />
    protected override string GetInputFileName(in SolverData data)
    {
        string day = data.Part.HasValue ? $"day{data.Day:D2}_{data.Part:D2}.txt" : $"day{data.Day:D2}.txt";
        return string.IsNullOrEmpty(data.Module)
                   ? Path.Combine(INPUT_FOLDER, data.Year.ToString(), day)
                   : Path.Combine(INPUT_FOLDER, data.Module, data.Year.ToString(), day);
    }

    /// <inheritdoc />
    protected override async Task<string> GetInputFromWebsite(SolverData data, CancellationToken token)
    {
        if (this.Settings.Seed is null)
        {
            User user = await this.API.GetUser(token);
            this.Settings.Seed = user.Seed;
            await SaveSettings(token);
        }
        return string.Empty;
    }
}
