using JetBrains.Annotations;

namespace EverybodyCodes.Resolver.Models;

/// <summary>
/// Everybody Codes Answer API request
/// </summary>
[PublicAPI]
public sealed class AnswerRequest
{
    /// <summary>
    /// Answer value
    /// </summary>
    public required string Answer { get; init; }
}
