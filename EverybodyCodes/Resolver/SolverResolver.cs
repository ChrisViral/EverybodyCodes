using System.Security.Cryptography;
using System.Text;
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
    protected override async Task<string> GetInputFromAPI(SolverData data, CancellationToken token)
    {
        if (!data.Part.HasValue) throw new InvalidOperationException("Part required to get input from API");

        // Get seed and inputs
        int seed = await GetSeed(token);
        Inputs inputs = await this.API.GetInputs($"https://everybody.codes/assets/{data.Year}/{data.Day}/input/{seed}.json");
        string input = inputs.GetInput(data.Part.Value);

        // Get quest data and decryption key
        Quest quest = await this.API.GetQuest(data.Year, data.Day, token);
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
    private async ValueTask<int> GetSeed(CancellationToken token)
    {
        if (this.Settings.Seed.HasValue) return this.Settings.Seed.Value;

        // Get seed value and save to settings
        User user = await this.API.GetUser(token);
        this.Settings.Seed = user.Seed;
        await SaveSettings(token);
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

        // Get key and IV bytes
        int keyLength = Encoding.UTF8.GetByteCount(key);
        Span<byte> keyBytes = stackalloc byte[keyLength];
        Span<byte> ivBytes = keyBytes[..16];
        Encoding.UTF8.TryGetBytes(key, keyBytes, out _);

        // Decrypt
        using Aes aes = Aes.Create();
        aes.SetKey(keyBytes);
        aes.TryDecryptCbc(encryptedBytes, ivBytes, decryptedBytes, out int written);
        return Encoding.UTF8.GetString(decryptedBytes[..written]);
    }
}
