using Challenge.Solvers;
using Microsoft.Extensions.Logging;
using ZLinq;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 1
/// </summary>
public abstract class Day01 : Solver<string>
{
    /// <summary>
    /// Part 1 solver
    /// </summary>
    [Solver(2024, 1, Part = 1)]
    public sealed class Part1 : Day01
    {
        /// <inheritdoc />
        public Part1(string input, ILogger logger) : base(input, logger) { }

        /// <inheritdoc />
        public override void Run()
        {
            int potions = this.Data.AsValueEnumerable().Sum(PotionsRequired);
            LogAnswer(potions);
        }
    }

    /// <summary>
    /// Part 2 solver
    /// </summary>
    [Solver(2024, 1, Part = 2)]
    public sealed class Part2 : Day01
    {
        /// <inheritdoc />
        public Part2(string input, ILogger logger) : base(input, logger) { }

        /// <inheritdoc />
        public override void Run()
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
    }

    /// <summary>
    /// Part 3 solver
    /// </summary>
    [Solver(2024, 1, Part = 3)]
    public sealed class Part3 : Day01
    {
        /// <inheritdoc />
        public Part3(string input, ILogger logger) : base(input, logger) { }

        /// <inheritdoc />
        public override void Run()
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
    }

    /// <summary>
    /// Creates a new <see cref="Day01"/> Solver with the input data properly parsed
    /// </summary>
    /// <param name="input">Puzzle input</param>
    /// <param name="logger">Logger instance</param>
    /// <exception cref="InvalidOperationException">Thrown if the conversion to the data type fails</exception>
    protected Day01(string input, ILogger logger) : base(input, logger) { }

    protected static int PotionsRequired(char enemy) => enemy switch
    {
        'A' => 0,
        'B' => 1,
        'C' => 3,
        'D' => 5,
        _   => 0
    };

    /// <inheritdoc />
    protected sealed override string Convert(string[] rawInput) => rawInput[0];
}
