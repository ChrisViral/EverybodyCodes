using JetBrains.Annotations;

namespace EverybodyCodes.Resolver.Models;

/// <summary>
/// Error message response
/// </summary>
[PublicAPI]
public sealed class Error
{
    /// <summary>
    /// Error message
    /// </summary>
    public required string Message { get; init; }
}
