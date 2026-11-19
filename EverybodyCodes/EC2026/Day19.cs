using Challenge.Collections;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Utils.Extensions.Arrays;
using Challenge.Utils.Extensions.Enumerables;
using Challenge.Utils.Extensions.Strings;
using Challenge.Utils.Extensions.ValueEnumerables;
using ZLinq;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 19
/// </summary>
[Solver(2026, 19)]
public sealed partial class Day19() : Solver<(Grid<Day19.Crystal> BaseLayer, Grid<char>? TopLayer)>(options: StringSplitOptions.TrimEntries), IEverybodyCodesSolver
{
    [Flags]
    public enum Crystal
    {
        CYAN    = 0b001,
        MAGENTA = 0b010,
        YELLOW  = 0b100,

        BLUE  = CYAN | MAGENTA,
        GREEN = CYAN | YELLOW,
        RED   = MAGENTA | YELLOW,

        BLACK = CYAN | MAGENTA | YELLOW
    }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        long required = GetRequiredWhiteCrystals(this.Data.BaseLayer, this.Data.TopLayer!);
        LogAnswer(required);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        char[] baseColours = ['C', 'Y', 'M', 'K'];
        Grid<Crystal> baseLayer = new(this.Data.BaseLayer.Width, this.Data.BaseLayer.Height);
        Grid<char> topLayer     = new(baseLayer.Width, baseLayer.Height);

        long maxRequired = 0L;
        foreach (char[] colourMap in baseColours.PermutationsInPlace())
        {
            baseLayer.CopyFrom(this.Data.BaseLayer);
            topLayer.CopyFrom(this.Data.TopLayer!);
            foreach (Vector2<int> position in topLayer.Dimensions.Enumerate())
            {
                topLayer[position] = colourMap[topLayer[position].AsIndex - 1];
            }

            long required = GetRequiredWhiteCrystals(baseLayer, topLayer);
            maxRequired = Math.Max(maxRequired, required);
        }

        LogAnswer(maxRequired);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        int cyan = this.Data.BaseLayer.Count(Crystal.CYAN);
        int magenta = this.Data.BaseLayer.Count(Crystal.MAGENTA);
        int yellow = this.Data.BaseLayer.Count(Crystal.YELLOW);
        int total = cyan + magenta + yellow;

        int first = total / 3;
        total -= first;
        int second = total / 2;
        int third  = total - second;

        long maxRequired = (long)first * second * third;
        LogAnswer(maxRequired);
    }
    // ReSharper enable CognitiveComplexity

    private static long GetRequiredWhiteCrystals(Grid<Crystal> baseLayer, Grid<char> topLayer)
    {
        foreach (Vector2<int> position in baseLayer.Dimensions.Enumerate())
        {
            baseLayer[position] |= ToCrystal(topLayer[position]);
        }

        return baseLayer.GroupBy(c => c)
                        .Select(g => (value: g.Key, count: g.Count()))
                        .OrderByDescending(p => p.value)
                        .Take(3)
                        .Multiply(p => (long)p.count);
    }

    /// <inheritdoc />
    protected override (Grid<Crystal>, Grid<char>?) Convert(string[] rawInput)
    {
        int emptyIndex = rawInput.IndexOf(string.Empty);
        if (emptyIndex is not -1)
        {
            int width  = rawInput[0].Length;
            int height = (rawInput.Length - 1) / 2;
            Grid<Crystal> baseLayer = new(width, height, rawInput[..emptyIndex], l => l.Select(ToCrystal).ToArray());
            Grid<char> topLayer = new(width, height, rawInput[(emptyIndex + 1)..], l => l.ToCharArray());
            return (baseLayer, topLayer);
        }
        else
        {
            int width  = rawInput[0].Length;
            int height = rawInput.Length;
            Grid<Crystal> first = new(width, height, rawInput, l => l.Select(ToCrystal).ToArray());
            return (first, null);
        }


    }

    private static Crystal ToCrystal(char c) => c switch
    {
        'C' => Crystal.CYAN,
        'M' => Crystal.MAGENTA,
        'Y' => Crystal.YELLOW,
        'K' => Crystal.BLACK,
        _   => throw new InvalidOperationException($"Unknown colour character {c}")
    };
}
