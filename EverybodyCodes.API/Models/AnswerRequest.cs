using JetBrains.Annotations;

namespace EverybodyCodes.API.Models;

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
