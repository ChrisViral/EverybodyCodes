using JetBrains.Annotations;

namespace EverybodyCodes.Resolver.Models;

/// <summary>
/// Everybody Codes User API object
/// </summary>
[PublicAPI]
public class User
{
    /// <summary>
    /// Challenges seed
    /// </summary>
    public int Seed { get; init; }
}
