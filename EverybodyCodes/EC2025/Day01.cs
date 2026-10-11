using System.Collections.Immutable;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Enumerables;
using Challenge.Utils.Extensions.Numbers;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 1
/// </summary>
[Solver(2025, 1)]
public sealed partial class Day01 : Solver<(string[] Names, ImmutableArray<int> Moves)>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int last  = this.Data.Names.Length - 1;
        int index = this.Data.Moves.Aggregate(0, (c, m) => Math.Clamp(c + m, 0, last));
        LogAnswer(this.Data.Names[index]);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        int index = this.Data.Moves.Aggregate(0, (c, m) => (c + m).Mod(this.Data.Names.Length));
        LogAnswer(this.Data.Names[index]);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        this.Data.Moves.ForEach(m => SwapUtils.Swap(ref this.Data.Names[0], ref this.Data.Names[m.Mod(this.Data.Names.Length)]));
        LogAnswer(this.Data.Names[0]);
    }
    // ReSharper enable CognitiveComplexity

    /// <inheritdoc />
    protected override (string[], ImmutableArray<int>) Convert(string[] rawInput)
    {
        string[] names = ParseUtils.ParseArray(rawInput[0], ',');
        ImmutableArray<int> moves = ParseUtils.ParseImmutableArray(rawInput[1], ',', s =>
        {
            int value = int.Parse(s[1..]);
            return s[0] is 'L' ? -value : value;
        });
        return (names, moves);
    }
}
