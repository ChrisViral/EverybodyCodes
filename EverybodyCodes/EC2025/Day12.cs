using Challenge.Collections.Pooling;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Strings;
using ZLinq;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 12
/// </summary>
[Solver(2025, 12)]
public sealed partial class Day12 : GridSolver<byte>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        using Pooled<HashSet<Vector2<int>>> exploded = GetExploded(Vector2<int>.Zero);
        LogAnswer(exploded.Ref.Count);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        Vector2<int> bottomRight = this.Data.Dimensions - Vector2<int>.One;
        using Pooled<HashSet<Vector2<int>>> exploded = GetExploded(Vector2<int>.Zero, bottomRight);
        LogAnswer(exploded.Ref.Count);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int COUNT = 3;

        Pooled<HashSet<Vector2<int>>> allExploded = GetBestExploded([]);
        foreach (int _ in 1..COUNT)
        {
            Pooled<HashSet<Vector2<int>>> newExploded = GetBestExploded(allExploded.Ref);
            allExploded.Return();
            allExploded = newExploded;
        }

        LogAnswer(allExploded.Ref.Count);
        allExploded.Return();
    }
    // ReSharper enable CognitiveComplexity

    private Pooled<HashSet<Vector2<int>>> GetBestExploded(HashSet<Vector2<int>> alreadyExploded)
    {
        Vector2<int> first = this.Data.Dimensions.Enumerate().First(p => !alreadyExploded.Contains(p));
        Pooled<HashSet<Vector2<int>>> bestExploded = GetExploded(alreadyExploded, first);

        foreach (Vector2<int> start in this.Data.Dimensions.Enumerate()
                                           .Where(p => !alreadyExploded.Contains(p))
                                           .Skip(1))
        {
            Pooled<HashSet<Vector2<int>>> exploded = GetExploded(alreadyExploded, start);
            if (bestExploded.Ref.Count < exploded.Ref.Count)
            {
                bestExploded.Return();
                bestExploded = exploded;
            }
            else
            {
                exploded.Return();
            }
        }

        return bestExploded;
    }

    private Pooled<HashSet<Vector2<int>>> GetExploded(params ReadOnlySpan<Vector2<int>> startPositions) => GetExploded(null, startPositions);

    private Pooled<HashSet<Vector2<int>>> GetExploded(HashSet<Vector2<int>>? alreadyExploded = null, params ReadOnlySpan<Vector2<int>> startPositions)
    {
        Pooled<HashSet<Vector2<int>>> exploded = HashSetObjectPool<Vector2<int>>.Shared.Get();
        exploded.Ref.AddRange(startPositions);
        if (alreadyExploded is not null)
        {
            exploded.Ref.UnionWith(alreadyExploded);
        }

        using Pooled<Queue<Vector2<int>>> toExplode = QueueObjectPool<Vector2<int>>.Shared.Get();
        toExplode.Ref.EnqueueRange(startPositions);

        while (toExplode.Ref.TryDequeue(out Vector2<int> barrel))
        {
            byte current = this.Data[barrel];
            foreach (Vector2<int> adjacent in barrel.Adjacent())
            {
                if (this.Data.TryGetPosition(adjacent, out byte adjacentValue)
                 && adjacentValue <= current
                 && exploded.Ref.Add(adjacent))
                {
                    toExplode.Ref.Enqueue(adjacent);
                }
            }
        }

        return exploded;
    }

    /// <inheritdoc />
    protected override byte[] LineConverter(string line) => line.Select(c => (byte)c.AsIndex).ToArray();
}
