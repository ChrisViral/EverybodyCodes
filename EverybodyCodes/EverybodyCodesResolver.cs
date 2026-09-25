using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using Challenge.CLI;
using Challenge.Solvers;
using CSharpFunctionalExtensions;
using EverybodyCodes.API;
using EverybodyCodes.API.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Refit;

namespace EverybodyCodes;

/// <summary>
/// Solver resolver and input fetcher
/// </summary>
/// <param name="logger">Logger instance</param>
/// <param name="settings">Resolver settings</param>
/// <param name="api">Everybody Codes API</param>
/// <param name="inputAPI">REverybody Codes Input API</param>
[PublicAPI, SolverTable]
public sealed partial class EverybodyCodesResolver(ILogger<EverybodyCodesResolver> logger, EverybodyCodesSettings settings, IEverybodyCodesAPI api, IEverybodyCodesInputAPI inputAPI)
    : SolverResolverBase<EverybodyCodesSettings>(logger, settings)
{
    private readonly Dictionary<(uint year, uint day), (Inputs inputs, Quest quest)> inputsCache = new();

    /// <inheritdoc />
    public override string ChallengeName => "Everybody Codes";

    /// <inheritdoc />
    protected override TimeSpan RateLimit => TimeSpan.FromSeconds(120L);

    /// <inheritdoc />
    protected override JsonTypeInfo<EverybodyCodesSettings> SettingsTypeInfo => EverybodyCodesSettingsJsonContext.Default.EverybodyCodesSettings;

    /// <summary>
    /// Everybody Codes API
    /// </summary>
    private IEverybodyCodesAPI API { get; } = api;

    /// <summary>
    /// Everybody Codes input API
    /// </summary>
    private IEverybodyCodesInputAPI InputAPI { get; } = inputAPI;

    /// <inheritdoc />
    public override async Task<Result> SubmitAnswer(string answer, SolverData data, CancellationToken token = default)
    {
        if (!data.Part.HasValue) return Result.Failure($"[{nameof(InvalidOperationException)}]: Part required to post answer");

        try
        {
            // Post answer
            AnswerRequest request = new() { Answer = answer };
            AnswerResponse response = await this.API.PostAnswer(request, data.Year, data.Day, data.Part.Value, token).ConfigureAwait(false);
            return response.Correct
                       ? Result.Success()
                       : Result.Failure($"""
                                         
                                         First character correct: {(response.FirstCorrect ? "yes" : "no")}
                                         Length correct: {(response.LengthCorrect ? "yes" : "no")}
                                         Cannot answer again for {response.PenaltyLeft.TotalSeconds:F0} seconds
                                         """);
        }
        catch (ApiException e)
        {
            Error? error = await e.GetContentAsAsync<Error>().ConfigureAwait(false);
            if (error is null) return Result.Failure($"[{nameof(ApiException)}]: {e.Message}");
            return error.Message is "already solved" ? Result.Success() : Result.Failure(error.Message);
        }
        catch (Exception e)
        {
            return Result.Failure($"[{e.GetType().Name}]: {e.Message}");
        }
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">If the <paramref name="data"/> does not have a valid value for the <see cref="SolverData.Part"/></exception>
    protected override string GetInputFileName(in SolverData data)
    {
        if (!data.Part.HasValue) throw new InvalidOperationException("Part required to post answer");

        return string.IsNullOrEmpty(data.Module)
                   ? Path.Combine(INPUT_FOLDER, data.Year.ToString(), $"day{data.Day:D2}_{data.Part:D2}.txt")
                   : Path.Combine(INPUT_FOLDER, data.Module, data.Year.ToString(), $"day{data.Day:D2}_{data.Part:D2}.txt");
    }

    /// <inheritdoc />
    protected override async Task<Result<string>> GetInputFromAPI(SolverData data, CancellationToken token)
    {
        if (!data.Part.HasValue) return Result.Failure<string>("Part required to get input from API");

        // Get seed
        uint seed = await GetSeed(token).ConfigureAwait(false);

        // Get input and quest from API
        Inputs inputs = await this.InputAPI.GetInputs(data.Year, data.Day, seed, token).ConfigureAwait(false);
        Quest quest   = await this.API.GetQuest(data.Year, data.Day, token).ConfigureAwait(false);
        this.inputsCache[(data.Year, data.Day)] = (inputs, quest);

        // Decrypt data and return
        return DecryptInput(data, inputs, quest);
    }

    /// <inheritdoc />
    protected override Result<string> GetCachedInput(SolverData data)
    {
        return this.inputsCache.TryGetValue((data.Year, data.Day), out (Inputs inputs, Quest quest) cachedData)
                   ? DecryptInput(data, cachedData.inputs, cachedData.quest)
                   : Result.Failure<string>("Inputs or quest not found in cache");
    }

    /// <summary>
    /// Gets the seed for the current user
    /// </summary>
    /// <param name="token">Cancellation token</param>
    /// <returns>The current seed for the user</returns>
    private async ValueTask<uint> GetSeed(CancellationToken token)
    {
        if (this.Settings.Seed.HasValue) return this.Settings.Seed.Value;

        // Get seed value and save to settings
        User user = await this.API.GetUser(token).ConfigureAwait(false);
        this.Settings.Seed = user.Seed;
        await SaveSettings(token).ConfigureAwait(false);
        return user.Seed;
    }

    /// <summary>
    /// Decrypts the input
    /// </summary>
    /// <param name="data">Solver data</param>
    /// <param name="inputs">Solver inputs</param>
    /// <param name="quest">Solver quest</param>
    /// <returns>The decrypted input, if successful</returns>
    private static Result<string> DecryptInput(SolverData data, Inputs inputs, Quest quest)
    {
        // Get encrypted data and key
        string encrypted = inputs.GetInput(data.Part!.Value);
        string? key = quest.GetKey(data.Part.Value);
        if (string.IsNullOrEmpty(key)) return Result.Failure<string>("Input decryption key not obtained yet");

        // Get encrypted bytes
        int encryptedLength = encrypted.Length / 2;
        Span<byte> encryptedBytes = stackalloc byte[encryptedLength];
        Span<byte> decryptedBytes = stackalloc byte[encryptedLength];
        Convert.FromHexString(encrypted, encryptedBytes, out _, out _);

        // Get key bytes
        int keyLength = Encoding.UTF8.GetByteCount(key);
        Span<byte> keyBytes = stackalloc byte[keyLength];
        Encoding.UTF8.TryGetBytes(key, keyBytes, out _);

        // Decrypt
        using Aes aes = Aes.Create();
        aes.SetKey(keyBytes);
        aes.TryDecryptCbc(encryptedBytes, keyBytes[..16], decryptedBytes, out int written);
        return Encoding.UTF8.GetString(decryptedBytes[..written]);
    }
}
