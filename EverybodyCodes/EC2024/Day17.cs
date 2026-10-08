using System.Collections.Immutable;
using Challenge.Collections;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Enumerables;
using ZLinq;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 17
/// </summary>
[Solver(2024, 17)]
public sealed partial class Day17 : Solver<ImmutableArray<Vector2<int>>>, IEverybodyCodesSolver
{
    private sealed class Constellation
    {
        public int Links { get; set; }

        public List<Vector2<int>> Stars { get; } = [];

        public int Size => this.Links + this.Stars.Count;

        public override string ToString() => $"Links: {this.Links}, Stars: {this.Stars.Count}";
    }

    private readonly record struct DistancePair(Vector2<int> First, Vector2<int> Second) : IComparable<DistancePair>
    {
        public int Distance { get; } = Vector2<int>.ManhattanDistance(First, Second);

        /// <inheritdoc />
        public int CompareTo(DistancePair other) => this.Distance.CompareTo(other.Distance);
    }

    private const char STAR = '*';

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        DefaultDictionary<Vector2<int>, Constellation> constellations = GetConstellations();
        int size = constellations.Values.First().Size;
        LogAnswer(size);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2() => RunPart1();

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int MAX_LINK = 5;
        DefaultDictionary<Vector2<int>, Constellation> constellations = GetConstellations(MAX_LINK);
        long size = constellations.Values.Distinct()
                                  .OrderByDescending(c => c.Size)
                                  .Take(3)
                                  .Multiply(c => (long)c.Size);
        LogAnswer(size);
    }
    // ReSharper enable CognitiveComplexity

    private DefaultDictionary<Vector2<int>, Constellation> GetConstellations(int maxLinkDistance = int.MaxValue)
    {
        DefaultDictionary<Vector2<int>, Constellation> constellations = new(this.Data.Length, () => new Constellation());
        foreach (DistancePair pair in this.Data.EnumeratePairs()
                                          .Select(p => new DistancePair(p.First, p.Second))
                                          .Where(p => p.Distance <= maxLinkDistance)
                                          .Order())
        {
            (Vector2<int> first, Vector2<int> second) = pair;
            Constellation firstConstellation  = constellations[first];
            Constellation secondConstellation = constellations[second];
            if (firstConstellation == secondConstellation) continue;

            if (firstConstellation.Stars.Count < secondConstellation.Stars.Count)
            {
                SwapUtils.Swap(ref firstConstellation, ref secondConstellation);
                SwapUtils.Swap(ref first, ref second);
            }

            firstConstellation.Links += secondConstellation.Links + pair.Distance;
            if (firstConstellation.Stars.IsEmpty)
            {
                firstConstellation.Stars.Add(first);
            }

            if (secondConstellation.Stars.IsEmpty)
            {
                firstConstellation.Stars.Add(second);
                constellations[second] = firstConstellation;
            }
            else
            {
                firstConstellation.Stars.AddRange(secondConstellation.Stars);
                secondConstellation.Stars.ForEach(s => constellations[s] = firstConstellation);
            }

            constellations[first] = firstConstellation;
            if (firstConstellation.Stars.Count == this.Data.Length) break;
        }

        return constellations;
    }

    /// <inheritdoc />
    protected override ImmutableArray<Vector2<int>> Convert(string[] rawInput)
    {
        int width = rawInput[0].Length;
        int height = rawInput.Length;
        Grid<bool> sky = new(width, height, rawInput, l => l.Select(c => c is STAR).ToArray());
        return sky.Dimensions.Enumerate().Where(p => sky[p]).ToImmutableArray();
    }
}
