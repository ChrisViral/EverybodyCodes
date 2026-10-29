using System.Collections.Immutable;
using Challenge.Maths;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Numbers;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 1
/// </summary>
[Solver(2026, 1)]
public sealed partial class Day01 : Solver<ImmutableArray<int>>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int collect = this.Data.Sum(v => GetNextLargestSquare(v) - v);
        LogAnswer(collect);
        return;

        static int GetNextLargestSquare(int value)
        {
            int sqrt = MathUtils.IntegerSqrt(value);
            int square = sqrt * sqrt;
            return square != value ? square + (sqrt * 2) + 1 : square;
        }
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        int collect = this.Data.Sum(v => (4 * Math.Max((v + 3) / 4, 2)) - v);
        LogAnswer(collect);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        int collect = this.Data.Sum(v => GetNextLargestSquare(v) - v);
        LogAnswer(collect);
        return;

        static int GetNextLargestSquare(int value)
        {
            int sqrt = Math.Max(MathUtils.IntegerSqrt(value), 3);
            if (sqrt.IsEven)
            {
                sqrt++;
            }

            int square = (sqrt * sqrt) - 1;
            return square < value ? square + ((sqrt + 1) * 4) : square;
        }
    }
    // ReSharper enable CognitiveComplexity


    /// <inheritdoc />
    protected override ImmutableArray<int> Convert(string[] rawInput) => ParseUtils.ParseImmutableArray(rawInput[0], ',', s => int.Parse(s));
}
