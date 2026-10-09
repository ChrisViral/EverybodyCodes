using Challenge.Collections;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Enumerables;
using JetBrains.Annotations;
using ZLinq;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 18
/// </summary>
[Solver(2024, 18)]
public sealed partial class Day18 : Solver<(Grid<Day18.Element> Map, HashSet<Vector2<int>> Palms)>, IEverybodyCodesSolver
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public enum Element
    {
        EMPTY = '.',
        WALL  = '#',
        PALM  = 'P',
        WATER = '~'
    }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        Vector2<int> start = this.Data.Map.Dimensions.Enumerate()
                                 .Where(p => this.Data.Map[p] is Element.EMPTY)
                                 .First(p => p is { X: 0 } or { Y: 0 }
                                          || p.X == this.Data.Map.Width - 1
                                          || p.Y == this.Data.Map.Height - 1);

        (int time, _) = GetFloodTime(this.Data.Map, this.Data.Palms, start);
        LogAnswer(time);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        Vector2<int>[] startPoints = this.Data.Map.Dimensions.Enumerate()
                                         .Where(p => this.Data.Map[p] is Element.EMPTY)
                                         .Where(p => p is { X: 0 } or { Y: 0 }
                                                  || p.X == this.Data.Map.Width - 1
                                                  || p.Y == this.Data.Map.Height - 1)
                                         .ToArray();

        (int time, _) = GetFloodTime(this.Data.Map, this.Data.Palms, startPoints);
        LogAnswer(time);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        // Flood filling in reverse from the trees would be faster, but this runs in ~3s and I can't really be assed to rewrite it
        int minTime = int.MaxValue;
        Grid<Element> map = new(this.Data.Map.Width, this.Data.Map.Height);
        HashSet<Vector2<int>> palms = [];
        foreach (Vector2<int> start in map.Dimensions.Enumerate().Where(p => this.Data.Map[p] is Element.EMPTY && !this.Data.Palms.Contains(p)).ToArray())
        {
            map.CopyFrom(this.Data.Map);
            palms.UnionWith(this.Data.Palms);
            (_, int time) = GetFloodTime(map, palms, start);
            minTime       = Math.Min(minTime, time);
        }
        LogAnswer(minTime);
    }
    // ReSharper enable CognitiveComplexity

    private static (int time, int timeSums) GetFloodTime(Grid<Element> map, HashSet<Vector2<int>> palms, params ReadOnlySpan<Vector2<int>> startPoints)
    {
        int time = 0, timeSums = 0;
        Queue<Vector2<int>> floodQueue = new();
        Queue<Vector2<int>> nextFloodQueue = new();

        foreach (Vector2<int> start in startPoints)
        {
            map[start] = Element.WATER;
            floodQueue.Enqueue(start);
        }

        do
        {
            time++;
            while (floodQueue.TryDequeue(out Vector2<int> position))
            {
                foreach (Vector2<int> adjacent in position.Adjacent().Where(a => map.IsPositionEqual(a, Element.EMPTY)))
                {
                    if (palms.Remove(adjacent))
                    {
                        timeSums += time;
                    }
                    nextFloodQueue.Enqueue(adjacent);
                    map[adjacent] = Element.WATER;
                }
            }

            SwapUtils.Swap(ref floodQueue, ref nextFloodQueue);
        }
        while (!palms.IsEmpty);

        return (time, timeSums);
    }

    /// <inheritdoc />
    protected override (Grid<Element>, HashSet<Vector2<int>>) Convert(string[] rawInput)
    {
        int width  = rawInput[0].Length;
        int height = rawInput.Length;
        Grid<Element> map = new(width, height, rawInput, l => l.Select(c => (Element)c).ToArray(), e => ((char)e).ToString());
        HashSet<Vector2<int>> palms = [..map.Dimensions.Enumerate().Where(p => map[p] is Element.PALM)];
        palms.ForEach(p => map[p] = Element.EMPTY);
        return (map, palms);
    }
}
