using Challenge.Maths;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils.Extensions.Numbers;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 3
/// </summary>
[Solver(2026, 3)]
public sealed partial class Day03 : ArraySolver<Day03.Standard>, IEverybodyCodesSolver
{
    public readonly record struct Standard
    {
        private readonly long standard;
        private readonly long number;

        public Standard(ReadOnlySpan<char> data)
        {
            int dashIndex  = data.IndexOf('-');
            int spaceIndex = data.IndexOf(' ');
            this.standard  = long.Parse(data[(dashIndex + 1)..spaceIndex]);
            this.number    = long.Parse(data[(spaceIndex + 1)..]);
        }

        public long GetISOValue() => GetISOValue(this.number);

        private long GetISOValue(long value)
        {
            int standardLength = this.standard.DigitCount;
            int numberLength   = value.DigitCount;
            long result = value.GetDigit(0);
            for (int i = 1; i < numberLength; i++)
            {
                long numberDigit   = value.GetDigit(i);
                long standardDigit = this.standard.GetDigit(i % standardLength);
                result += numberDigit * MathUtils.IntegerPow(standardDigit, i);
            }

            return result;
        }

        public bool TryGetISO2Value(out long value)
        {
            value = GetISOValue(this.number);
            for (long test = this.number - 1; test > 0; test--)
            {
                long testValue = GetISOValue(test);
                if (testValue == value)
                {
                    value = 0L;
                    return false;
                }
            }

            return true;
        }
    }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        long score = this.Data.Sum(i => i.GetISOValue());
        LogAnswer(score);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2() => RunPart1();

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        long score = 0L;
        foreach (Standard standard in this.Data)
        {
            if (standard.TryGetISO2Value(out long value))
            {
                score += value;
            }
        }

        LogAnswer(score);
    }
    // ReSharper enable CognitiveComplexity

    /// <inheritdoc />
    protected override Standard ConvertLine(string line) => new(line);
}
