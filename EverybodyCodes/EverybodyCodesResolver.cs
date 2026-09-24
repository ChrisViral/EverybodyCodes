using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using Challenge.CLI;
using Challenge.Solvers;
using CSharpFunctionalExtensions;
using EverybodyCodes.Resolver.Models;
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
public sealed partial class EverybodyCodesResolver(ILogger<EverybodyCodesResolver> logger, EverybodyCodesResolverSettings settings, IEverybodyCodesAPI api, IEverybodyCodesInputAPI inputAPI)
    : SolverResolverBase<EverybodyCodesResolverSettings>(logger, settings)
{
    /// <inheritdoc />
    public override string ChallengeName => "Everybody Codes";

    /// <inheritdoc />
    protected override TimeSpan RateLimit => TimeSpan.Zero;

    /// <inheritdoc />
    protected override JsonTypeInfo<EverybodyCodesResolverSettings> SettingsTypeInfo => EverybodyCodesResolverSettingsJsonContext.Default.EverybodyCodesResolverSettings;

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
    /// <exception cref="InvalidOperationException">If the <paramref name="data"/> does not have a valid value for the <see cref="SolverData.Part"/></exception>
    protected override async Task<string> GetInputFromAPI(SolverData data, CancellationToken token)
    {
        if (!data.Part.HasValue) throw new InvalidOperationException("Part required to get input from API");

        // Get seed and inputs
        uint seed = await GetSeed(token).ConfigureAwait(false);
        Inputs inputs = await this.InputAPI.GetInputs(data.Year, data.Day, seed, token).ConfigureAwait(false);
        string input = inputs.GetInput(data.Part.Value);

        // Get quest data and decryption key
        Quest quest = await this.API.GetQuest(data.Year, data.Day, token).ConfigureAwait(false);
        string? key = quest.GetKey(data.Part.Value);

        // Decrypt part input
        return !string.IsNullOrEmpty(key)
                   ? DecryptInput(input, key)
                   : throw new InvalidOperationException("Input decryption key not obtained yet");
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
    /// <param name="encrypted">Encrypted input</param>
    /// <param name="key">AES decryption key</param>
    /// <returns>The decrypted input</returns>
    private static string DecryptInput(string encrypted, string key)
    {
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
