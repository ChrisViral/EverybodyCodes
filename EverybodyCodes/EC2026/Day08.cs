using System.Collections.Immutable;
using Challenge.Collections.Pooling;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Strings;
using ZLinq;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 8
/// </summary>
[Solver(2026, 8)]
public sealed partial class Day08 : GridSolver<int>, IEverybodyCodesSolver
{
    private readonly record struct Hexagon(List<Vector2<int>> Top,
                                           List<Vector2<int>> TopRight,
                                           List<Vector2<int>> BottomRight,
                                           List<Vector2<int>> Bottom,
                                           List<Vector2<int>> BottomLeft,
                                           List<Vector2<int>> TopLeft)
    {
        public List<Vector2<int>> this[int index] => index switch
        {
            0 => this.Top,
            1 => this.TopRight,
            2 => this.BottomRight,
            3 => this.Bottom,
            4 => this.BottomLeft,
            5 => this.TopLeft,
            _ => throw new IndexOutOfRangeException()
        };
    }

    private readonly record struct RoundData(int FirstSide, int SecondSide, Vector2<int> LightDirection);

    private static readonly ImmutableArray<RoundData> Rounds =
    [
        new(5, 0, ( 1,  1)),
        new(0, 1, (-1,  1)),
        new(1, 2, (-2,  0)),
        new(2, 3, (-1, -1)),
        new(3, 4, ( 1, -1)),
        new(4, 5, ( 2,  0)),
    ];

    private const char EMPTY = '.';

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        Hexagon sides = GetSides();
        using Pooled<List<Vector2<int>>> visible = GetVisibleForRound(sides, Rounds[0]);
        LogAnswer(visible.Ref.Count);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        Hexagon sides = GetSides();
        int energy = 0;
        foreach (RoundData round in Rounds)
        {
            using Pooled<List<Vector2<int>>> visible = GetVisibleForRound(sides, round);
            energy += visible.Ref.Count;
        }
        LogAnswer(energy);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int ARMY_SIZE = 100000;
        const int SEED_START = -3;
        const int ENT_START = 1;
        const int MAX_ENT_SIZE = 9;

        int round = 0;
        int energy = 0;
        int army = 0;
        Hexagon sides = GetSides();
        Queue<Vector2<int>> seeds = [];
        while (army < ARMY_SIZE)
        {
            // Get visible
            using Pooled<List<Vector2<int>>> visible = GetVisibleForRound(sides, Rounds[round++ % Rounds.Length]);
            energy += visible.Ref.Count;

            // Convert seeds that are done to ends of size 1
            while (seeds.TryPeek(out Vector2<int> position) && this.Data[position] is -1)
            {
                seeds.Dequeue();
                this.Data[position] = ENT_START;
            }

            // Grow remaining seeds
            foreach (Vector2<int> position in seeds)
            {
                this.Data[position]++;
            }

            // Grow visible ents
            foreach (Vector2<int> position in visible.Ref)
            {
                int height = this.Data[position];
                if (height >= MAX_ENT_SIZE)
                {
                    this.Data[position] = SEED_START;
                    seeds.Enqueue(position);
                    army++;
                }
                else
                {
                    this.Data[position] = height + 1;
                }
            }
        }

        LogAnswer(energy);
    }
    // ReSharper enable CognitiveComplexity

    private Hexagon GetSides()
    {
        Vector2<int> start = this.Data.FindPosition(e => e > 0);
        List<Vector2<int>> top         = GetSide(ref start, ( 2,  0));
        List<Vector2<int>> topRight    = GetSide(ref start, ( 1,  1));
        List<Vector2<int>> bottomRight = GetSide(ref start, (-1,  1));
        List<Vector2<int>> bottom      = GetSide(ref start, (-2,  0));
        List<Vector2<int>> bottomLeft  = GetSide(ref start, (-1, -1));
        List<Vector2<int>> topLeft     = GetSide(ref start, ( 1, -1));
        return new Hexagon(top, topRight, bottomRight, bottom, bottomLeft, topLeft);
    }

    private List<Vector2<int>> GetSide(ref Vector2<int> start, Vector2<int> movement)
    {
        List<Vector2<int>> side = [start];
        for (Vector2<int> position = start + movement;
             this.Data.TryGetPosition(position, out int height) && height is not 0;
             position += movement)
        {
            start = position;
            side.Add(position);
        }

        return side;
    }

    private Pooled<List<Vector2<int>>> GetVisibleForRound(in Hexagon sides, RoundData round)
    {
        Pooled<List<Vector2<int>>> visible = ListObjectPool<Vector2<int>>.Shared.Get();
        GetVisibleFrom(sides[round.FirstSide], sides[round.SecondSide], round.LightDirection, visible.Ref);
        return visible;
    }

    private void GetVisibleFrom(List<Vector2<int>> first, List<Vector2<int>> second, Vector2<int> lightDirection, List<Vector2<int>> visible)
    {
        foreach (int i in ..first.Count)
        {
            GetVisible(first[i], lightDirection, visible);
        }

        foreach (int i in 1..second.Count)
        {
            GetVisible(second[i], lightDirection, visible);
        }
    }

    private void GetVisible(Vector2<int> position, Vector2<int> lightDirection, List<Vector2<int>> visible)
    {
        int shadow = 0;
        while (this.Data.TryGetPosition(position, out int height) && height is not 0)
        {
            if (shadow >= height)
            {
                shadow = Math.Max(0, shadow - 1);
            }
            else
            {
                shadow = height - 1;
                visible.Add(position);
            }

            position += lightDirection;
        }
    }

    /// <inheritdoc />
    protected override int[] LineConverter(string line) => line.Select(c => c is not EMPTY ? c.AsIndex : 0).ToArray();

    /// <inheritdoc />
    protected override string StringConversion(int obj) => obj switch
    {
        0   => ".",
        < 0 => "X",
        _   => obj.ToString()
    };
}
