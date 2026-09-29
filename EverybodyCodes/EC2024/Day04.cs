using Challenge.Solvers;
using Challenge.Solvers.Specialized;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 4
/// </summary>
[Solver(2024, 4)]
public sealed partial class Day04 : ArraySolver<int>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int min = this.Data.Min();
        int strikes = this.Data.Sum(n => n - min);
        LogAnswer(strikes);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        int min = this.Data.Min();
        int strikes = this.Data.Sum(n => n - min);
        LogAnswer(strikes);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        this.Data.Sort();
        int median = this.Data[this.Data.Length / 2];
        int strikes = this.Data.Sum(n => Math.Abs(n - median));
        LogAnswer(strikes);
    }
    // ReSharper enable CognitiveComplexity

    /// <inheritdoc />
    protected override int ConvertLine(string line) => int.Parse(line);
}
