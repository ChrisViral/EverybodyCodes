using System.Text.RegularExpressions;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using ZLinq;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 18
/// </summary>
[Solver(2026, 18)]
public sealed partial class Day18 : RegexSolver<Day18.Stack>, IEverybodyCodesSolver
{
    public readonly record struct Stack(long Position, long Height, long Width) : IComparable<Stack>
    {
        // ReSharper disable once IntroduceOptionalParameters.Global
        public Stack(long Position, long Height) : this(Position, Height, 1L) { }

        /// <inheritdoc />
        public int CompareTo(Stack other) => this.Position.CompareTo(other.Position);
    }

    /// <inheritdoc />
    [GeneratedRegex(@"x=(\d+) y=(\d+)(?: width=(\d+))?")]
    protected override partial Regex Matcher { get; }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        long material = GetRequiredMaterial();
        LogAnswer(material);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        long material = GetRequiredMaterial();
        LogAnswer(material);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        long material = GetRequiredMaterial();
        LogAnswer(material);
    }
    // ReSharper enable CognitiveComplexity

    public long GetRequiredMaterial()
    {
        (int tallestIndex, (_, long tallestHeight, long tallestWidth)) = this.Data.Index().MaxBy(s => s.Item.Height);

        long material = tallestHeight * tallestWidth;

        long left = 0L;
        Stack previous = this.Data[0];
        Stack current;
        for (int i = 1; i <= tallestIndex; i++)
        {
            current   = this.Data[i];
            left      = Math.Max(left, previous.Height);
            material += left * (current.Position - previous.Position);
            previous  = current;
        }

        long right = 0L;
        current = this.Data[^1];
        for (int i = this.Data.Length - 2; i >= tallestIndex; i--)
        {
            Stack next = this.Data[i];
            right      = Math.Max(right, current.Height);
            material  += right * ((current.Position + current.Width) - (next.Position + next.Width));
            current    = next;
        }

        return material;
    }

    /// <inheritdoc />
    public override void ParseInput(string input)
    {
        base.ParseInput(input);
        this.Data.Sort();
    }
}
