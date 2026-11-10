using Challenge.Collections.Pooling;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Numbers;
using Challenge.Utils.Extensions.Ranges;
using ZLinq;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 11
/// </summary>
[Solver(2026, 11)]
public sealed partial class Day11 : Solver<(int RingSize, HashSet<UnorderedPair<int>> Connections)>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const int MAX_OFFSET = 1;

        int[] portals = [..(1..^this.Data.RingSize)];
        UnorderedPair<int>[] pairs = [..portals.EnumeratePairs()];
        int noConnectionStrain = pairs.Sum(p => GetShortestRoute(p.First, p.Second, MAX_OFFSET, true)) * 2;

        int connectionStrain = pairs.Sum(p => GetShortestRoute(p.First, p.Second, MAX_OFFSET, false)) * 2;
        LogAnswer(noConnectionStrain - connectionStrain);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const int MAX_OFFSET = 3;

        int[] portals = [..(1..^this.Data.RingSize)];
        UnorderedPair<int>[] pairs = [..portals.EnumeratePairs()];
        int noConnectionStrain = pairs.Sum(p => GetShortestRoute(p.First, p.Second, MAX_OFFSET, true)) * 2;

        int minConnectionStrain = int.MaxValue;
        foreach (UnorderedPair<int> extra in pairs)
        {
            if (!this.Data.Connections.Add(extra)) continue;

            minConnectionStrain = Math.Min(minConnectionStrain, pairs.Sum(p => GetShortestRoute(p.First, p.Second, MAX_OFFSET, false)) * 2);
            this.Data.Connections.Remove(extra);
        }

        LogAnswer(noConnectionStrain - minConnectionStrain);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int MAX_OFFSET = 3;
        const int CRYSTALS = 8;

        int[] portals = [..(1..^this.Data.RingSize)];
        UnorderedPair<int>[] pairs = [..portals.EnumeratePairs()];
        int noConnectionStrain = pairs.Sum(p => GetShortestRoute(p.First, p.Second, MAX_OFFSET, true)) * 2;

        int minConnectionStrain = int.MaxValue;
        foreach (IReadOnlyList<UnorderedPair<int>> extras in GetCrystalPairs(CRYSTALS))
        {
            this.Data.Connections.AddRange(extras);
            int connectionStrain = pairs.Sum(p => GetShortestRoute(p.First, p.Second, MAX_OFFSET, false)) * 2;
            if (minConnectionStrain > connectionStrain)
            {
                minConnectionStrain = connectionStrain;
            }
            this.Data.Connections.ExceptWith(extras);
        }

        LogAnswer(noConnectionStrain - minConnectionStrain);
    }
    // ReSharper enable CognitiveComplexity

    private int GetShortestRoute(int start, int target, int maxOffset, bool ignoreConnections)
    {
        using Pooled<HashSet<int>> pooledVisited = HashSetObjectPool<int>.Shared.Get();
        using Pooled<Queue<int>> pooledCurrent = QueueObjectPool<int>.Shared.Get();
        using Pooled<Queue<int>> pooledNext = QueueObjectPool<int>.Shared.Get();

        return FindShortestRoute(pooledVisited.Ref, pooledCurrent.Ref, pooledNext.Ref);

        int FindShortestRoute(HashSet<int> visited, Queue<int> current, Queue<int> next)
        {
            int distance = 0;
            current.Enqueue(start);
            visited.Add(start);
            do
            {
                distance++;
                while (current.TryDequeue(out int teleport))
                {
                    foreach (int connection in GetConnections(teleport, maxOffset, ignoreConnections))
                    {
                        if (connection == target) return distance;

                        if (visited.Add(connection))
                        {
                            next.Enqueue(connection);
                        }
                    }
                }

                SwapUtils.Swap(ref current, ref next);
            }
            while (!current.IsEmpty);

            return int.MaxValue;
        }
    }

    private int NormalizeTeleport(int teleport) => (teleport - 1).Mod(this.Data.RingSize) + 1;

    private IEnumerable<int> GetConnections(int teleport, int maxOffset, bool ignoreConnections)
    {
        for (int offset = 1; offset <= maxOffset; offset++)
        {
            yield return NormalizeTeleport(teleport + offset);
            yield return NormalizeTeleport(teleport - offset);
        }

        if (ignoreConnections) yield break;

        foreach (UnorderedPair<int> connection in this.Data.Connections)
        {
            if (connection.TryGetCounterpart(teleport, out int to))
            {
                yield return to;
            }
        }
    }

    private IEnumerable<IReadOnlyList<UnorderedPair<int>>> GetCrystalPairs(int crystalCount)
    {
        int[] crystals = new int[crystalCount];
        int offset = this.Data.RingSize / crystalCount;
        UnorderedPair<int>[] pairs = new UnorderedPair<int>[crystalCount / 2];
        for (int start = 1; start <= offset; start++)
        {
            for (int i = 0, value = start; i < crystalCount; i++, value += offset)
            {
                crystals[i] = value;
            }

            foreach (IReadOnlyList<UnorderedPair<int>> set in GenerateCrystalPairs())
            {
                yield return set;
            }
        }

        yield break;

        IEnumerable<IReadOnlyList<UnorderedPair<int>>> GenerateCrystalPairs(int pairIndex = 0)
        {
            if (pairIndex == pairs.Length)
            {
                yield return pairs;
                yield break;
            }

            int firstIndex = pairIndex * 2;
            int secondIndex = firstIndex + 1;
            int first = crystals[firstIndex];

            for (int i = firstIndex + 1; i < crystalCount; i++)
            {
                pairs[pairIndex] = (first, crystals[i]);

                SwapUtils.Swap(ref crystals[secondIndex], ref crystals[i]);

                foreach (IReadOnlyList<UnorderedPair<int>> result in GenerateCrystalPairs(pairIndex + 1))
                {
                    yield return result;
                }

                SwapUtils.Swap(ref crystals[secondIndex], ref crystals[i]);
            }
        }
    }


    /// <inheritdoc />
    protected override (int, HashSet<UnorderedPair<int>>) Convert(string[] rawInput)
    {
        int ringSize = int.Parse(rawInput[0].AsSpan(9));
        HashSet<UnorderedPair<int>> connections = rawInput.AsSpan(1).Select(l => UnorderedPair<int>.Parse(l)).ToHashSet();
        return (ringSize, connections);
    }
}
