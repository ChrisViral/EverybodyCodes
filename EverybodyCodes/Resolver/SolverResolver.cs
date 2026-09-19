using Challenge.CLI;
using Challenge.Solvers;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace EverybodyCodes.Resolver;

/// <summary>
/// Solver resolver and input fetcher
/// </summary>
/// <param name="logger">Logger instance</param>
/// <param name="settings">Resolver settings</param>
public sealed class SolverResolver(ILogger<SolverResolver> logger, EverybodyCodesResolverSettings settings) : SolverResolverBase(logger, settings)
{
    /// <inheritdoc />
    public override string ChallengeName => "Everybody Codes";

    /// <inheritdoc />
    protected override TimeSpan RateLimit { get; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Resolver settings
    /// </summary>
    private new EverybodyCodesResolverSettings Settings { get; } = settings;

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
        return string.Empty;
    }
}
