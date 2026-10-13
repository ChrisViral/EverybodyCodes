using System.Text.RegularExpressions;
using Challenge.Maths;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils.Extensions.Enumerables;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 4
/// </summary>
[Solver(2025, 4)]
public sealed partial class Day04 : RegexSolver<Day04.Gear>, IEverybodyCodesSolver
{
    public readonly record struct Gear(int Inner, int Outer)
    {
        public Gear(int teeth) : this(teeth, teeth) { }
    }

    /// <inheritdoc />
    [GeneratedRegex(@"(\d+)(?:\|(\d+))?")]
    protected override partial Regex Matcher { get; }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const int TURNS = 2025;
        Fraction<long> ratio = GetFullRatio();
        ratio = ratio.Invert * TURNS;
        LogAnswer(ratio.Whole);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const long TURNS = 10000000000000L;
        Fraction<long> ratio = GetFullRatio();
        ratio *= TURNS;
        long fullTurns = ratio.Whole;
        if (ratio.Denominator is not 1L)
        {
            fullTurns++;
        }
        LogAnswer(fullTurns);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int TURNS = 100;
        Fraction<long> ratio = GetFullRatio();
        ratio = ratio.Invert * TURNS;
        LogAnswer(ratio.Whole);
    }
    // ReSharper enable CognitiveComplexity

    public Fraction<long> GetFullRatio() => this.Data.Pairwise()
                                                .Select(p => new Fraction<long>(p.Second.Inner, p.First.Outer))
                                                .Aggregate(Fraction<long>.MultiplyReduced);
}
