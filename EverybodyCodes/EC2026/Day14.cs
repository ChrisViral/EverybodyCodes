using System.Collections.Frozen;
using Challenge.Collections;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils;
using Challenge.Utils.Extensions.Collections;
using ZLinq;

namespace EverybodyCodes.EC2026;

using Regions = HashSet<char>;
using Binding = FrozenSet<UnorderedPair<Vector2<int>>>;
using BindingMap = FrozenDictionary<UnorderedPair<char>, FrozenSet<UnorderedPair<Vector2<int>>>>;

/// <summary>
/// Solver for 2026 Day 14
/// </summary>
[Solver(2026, 14)]
public sealed partial class Day14 : GridSolver<char>, IEverybodyCodesSolver
{
    private const char OUTSIDE = '\0';

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        BindingMap bindings = GetBindings();
        int stitch = bindings.Sum(b => b.Value.Count);
        LogAnswer(stitch);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        BindingMap bindings = GetBindings();
        int stitch = GetMinimalStitch(bindings);
        LogAnswer(stitch);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        int minStitch = int.MaxValue;
        char[] allRegions = [..this.Data.Distinct()];
        BindingMap bindings = GetBindings();
        Regions firstLinkedRegions  = new(100);
        Regions secondLinkedRegions = new(100);

        foreach (UnorderedPair<char> selected in allRegions.EnumeratePairs())
        {
            MergeRegions(selected.First, bindings, firstLinkedRegions);
            MergeRegions(selected.Second, bindings, secondLinkedRegions);

            int stitch;
            if (firstLinkedRegions.Overlaps(secondLinkedRegions))
            {
                firstLinkedRegions.UnionWith(secondLinkedRegions);
                stitch = GetMinimalStitch(bindings, firstLinkedRegions);
            }
            else
            {
                stitch = GetMinimalStitch(bindings, firstLinkedRegions, secondLinkedRegions);
            }

            minStitch = Math.Min(minStitch, stitch);
            firstLinkedRegions.Clear();
            secondLinkedRegions.Clear();
        }

        LogAnswer(minStitch);
    }
    // ReSharper enable CognitiveComplexity

    private BindingMap GetBindings()
    {
        DefaultDictionary<UnorderedPair<char>, HashSet<UnorderedPair<Vector2<int>>>> bindings = new(this.Data.Size, _ => []);
        foreach (Vector2<int> position in this.Data.Dimensions.Enumerate())
        {
            char current = this.Data[position];
            foreach (Vector2<int> adjacent in position.Adjacent())
            {
                if (!this.Data.TryGetPosition(adjacent, out char next))
                {
                    next = OUTSIDE;
                }

                if (current != next)
                {
                    bindings[(current, next)].Add((position, adjacent));
                }
            }
        }

        return bindings.ToFrozenDictionary(p => p.Key, p => p.Value.ToFrozenSet());
    }

    private int GetMinimalStitch(BindingMap bindings, params ReadOnlySpan<Regions> linkedRegions)
    {
        Regions unusedRegions = [..this.Data];
        Regions usedRegions   = [OUTSIDE];

        foreach (Regions linkedRegion in linkedRegions)
        {
            if (linkedRegion.Contains(OUTSIDE))
            {
                usedRegions.UnionWith(linkedRegion);
                unusedRegions.ExceptWith(linkedRegion);
            }
        }

        int stitch = 0;
        while (!unusedRegions.IsEmpty)
        {
            stitch += GetNextOptimalBinding(unusedRegions, usedRegions, bindings, linkedRegions);
        }

        return stitch;
    }

    private static int GetNextOptimalBinding(Regions unusedRegions, Regions usedRegions, BindingMap bindings, ReadOnlySpan<Regions> linkedRegions)
    {
        int bestBindingCount = int.MaxValue;
        char bestBindingRegion = OUTSIDE;
        Binding? bestBinging = null;
        foreach (char unused in unusedRegions)
        {
            foreach (char used in usedRegions)
            {
                if (!linkedRegions.Any(l => l.Contains(unused) && l.Contains(used))
                 && bindings.TryGetValue((unused, used), out Binding? binding)
                 && bestBindingCount > binding.Count)
                {
                    bestBinging       = binding;
                    bestBindingRegion = unused;
                    bestBindingCount  = binding.Count;
                }
            }
        }

        if (bestBinging is null) throw new InvalidOperationException("Could not find binding");

        Regions? linkedRegion = linkedRegions.FirstOrDefault(l => l.Contains(bestBindingRegion));
        if (linkedRegion is not null)
        {
            unusedRegions.ExceptWith(linkedRegion);
            usedRegions.UnionWith(linkedRegion);
        }
        else
        {
            unusedRegions.Remove(bestBindingRegion);
            usedRegions.Add(bestBindingRegion);
        }

        return bestBinging.Count;
    }

    private static void MergeRegions(char region, BindingMap bindings, Regions regions)
    {
        regions.Add(region);
        foreach (UnorderedPair<char> linked in bindings.Keys)
        {
            if (linked.TryGetCounterpart(region, out char other))
            {
                regions.Add(other);
            }
        }
    }

    /// <inheritdoc />
    protected override char[] LineConverter(string line) => line.ToCharArray();
}
