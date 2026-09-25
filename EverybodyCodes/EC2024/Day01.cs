using Challenge.Solvers;
using ZLinq;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 1
/// </summary>
[Solver(2024, 1)]
public sealed partial class Day01 : Solver<string>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int potions = this.Data.AsValueEnumerable().Sum(PotionsRequired);
        LogAnswer(potions);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        int potions = 0;
        for (int i = 0; i < this.Data.Length; i += 2)
        {
            ReadOnlySpan<char> pair = this.Data.AsSpan(i, 2);
            int extra = Math.Max(1 - pair.Count('x'), 0);
            potions += pair.Where(e => e is not 'x').Sum(e => PotionsRequired(e) + extra);
        }
        LogAnswer(potions);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        int potions = 0;
        for (int i = 0; i < this.Data.Length; i += 3)
        {
            ReadOnlySpan<char> triple = this.Data.AsSpan(i, 3);
            int extra = Math.Max(2 - triple.Count('x'), 0);
            potions += triple.Where(e => e is not 'x').Sum(e => PotionsRequired(e) + extra);
        }
        LogAnswer(potions);
    }
    // ReSharper enable CognitiveComplexity

    private static int PotionsRequired(char enemy) => enemy switch
    {
        'A' => 0,
        'B' => 1,
        'C' => 3,
        'D' => 5,
        _   => 0
    };

    /// <inheritdoc />
    protected override string Convert(string[] rawInput) => rawInput[0];
}
