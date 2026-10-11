#define EC2025_DAY2_PRINT_ENGRAVING

using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Utils.Extensions.Ranges;
using ZLinq;

#if EC2025_DAY2_PRINT_ENGRAVING
using Challenge.Collections;
#endif

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 2
/// </summary>
[Solver(2025, 2)]
public sealed partial class Day02 : Solver<Day02.Complex>, IEverybodyCodesSolver
{
    public readonly record struct Complex(long X, long Y)
    {
        public static Complex Zero { get; } = new(0L, 0L);

        public override string ToString() => $"[{this.X},{this.Y}]";

        public static Complex operator +(Complex left, Complex right) => new(left.X + right.X, left.Y + right.Y);

        public static Complex operator *(Complex left, Complex right) => new((left.X * right.X) - (left.Y * right.Y), (left.X * right.Y) + (left.Y * right.X));

        public static Complex operator /(Complex left, Complex right) => new(left.X / right.X, left.Y / right.Y);

        public static implicit operator Complex(Vector2<int> vector) => new(vector.X, vector.Y);

        public static implicit operator Vector2<int>(Complex value) => new((int)value.X, (int)value.Y);

        public static implicit operator Complex((long x, long y) tuple) => new(tuple.x, tuple.y);
    }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const int CYCLES = 3;
        Complex result = Complex.Zero;
        Complex divisor = (10L, 10L);
        Complex a = this.Data;
        foreach (int _ in ..CYCLES)
        {
            result *= result;
            result /= divisor;
            result += a;
        }
        LogAnswer(result);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const int STEP = 10;

        int engraved = GetEngravingCount(STEP);
        LogAnswer(engraved);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int STEP = 1;

        int engraved = GetEngravingCount(STEP);
        LogAnswer(engraved);
    }
    // ReSharper enable CognitiveComplexity

    private int GetEngravingCount(int step)
    {
        const int GRID_SIZE = 1000;

        int gridSize = (GRID_SIZE / step) + 1;
        Vector2<int> start   = this.Data;
        int engraved = 0;

#if EC2025_DAY2_PRINT_ENGRAVING
        Grid<bool> engraving = new(gridSize, gridSize, b => b ? "x" : ".");
#endif

        foreach (Vector2<int> offset in Vector2<int>.EnumerateOver(gridSize, gridSize))
        {
            Vector2<int> position = start + (offset * step);
            if (IsEngraved(position))
            {
                engraved++;
#if EC2025_DAY2_PRINT_ENGRAVING
                engraving[offset] = true;
#endif
            }
        }

#if EC2025_DAY2_PRINT_ENGRAVING
        Log("\n" + engraving);
#endif

        return engraved;
    }

    private static bool IsEngraved(Complex position)
    {
        const int CYCLES = 100;
        const int MAX    = 1000000;

        Complex result = Complex.Zero;
        Complex divisor = (100000 ,100000);
        foreach (int _ in ..CYCLES)
        {
            result *= result;
            result /= divisor;
            result += position;
            if (result.X is > MAX or < -MAX || result.Y is > MAX or < -MAX)
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    protected override Complex Convert(string[] rawInput) => Vector2<int>.Parse(rawInput[0].AsSpan()[3..^1]);
}
