using Challenge.Maths;
using Challenge.Maths.BitVectors;
using Challenge.Maths.Directions;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Ranges;
using ZLinq;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 15
/// </summary>
[Solver(2026, 15)]
public sealed partial class Day15 : GridSolver<Day15.Element>, IEverybodyCodesSolver
{
    public enum Element
    {
        EMPTY   = '.',
        WALL    = '#',
        DEPOSIT = '@',
        START   = 'S',
        END     = 'E'
    }

    private readonly record struct Wagon(Vector2<int> Position, Direction Direction = Direction.NONE);

    private readonly record struct StateAll(Wagon Wagon, BitVector32 Visited = default);

    private readonly record struct StateCount(Wagon Wagon, long Visited = 0L);

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int pathLength = GetPathLengthAll();
        LogAnswer(pathLength);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const long SULPHUR_COUNT = 2026L;

        long pathLength = GetPathLengthCount(SULPHUR_COUNT);
        LogAnswer(pathLength);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int MAX_CYCLE_LENGTH = 150;
        const long SULPHUR_COUNT = 202620262026L;
        (Vector2<int> startPosition, Vector2<int> endPosition, Vector2<int>[] deposits) = GetMapData();

        Fraction<int> bestRatio = Fraction<int>.Zero;
        Wagon bestWagon = default;
        foreach (int x in ^startPosition.X..endPosition.X)
        {
            Vector2<int> wagonPosition = new(x, startPosition.Y);
            if (this.Data[wagonPosition + Direction.UP] is Element.WALL) continue;

            Wagon wagon = new(wagonPosition, Direction.RIGHT);
            Fraction<int> ratio = GetBestCollectRatio(wagon, wagon.Position, MAX_CYCLE_LENGTH, deposits);
            if (bestRatio < ratio)
            {
                bestRatio = ratio;
                bestWagon = wagon;
            }
        }

        long minPathLength = long.MaxValue;
        Wagon startWagon = new(startPosition, Direction.RIGHT);
        for (int maxLength = bestWagon.Position.X + 4; maxLength < MAX_CYCLE_LENGTH; maxLength++)
        {
            Fraction<int> toCycleRatio = GetBestCollectRatio(startWagon, bestWagon.Position, maxLength, deposits);

            long remainingSulphur = SULPHUR_COUNT - toCycleRatio.Numerator;
            long cycles = (remainingSulphur / bestRatio.Numerator) - 10;
            long collected = (cycles * bestRatio.Numerator) + toCycleRatio.Numerator;
            long pathLength = toCycleRatio.Denominator + (cycles * bestRatio.Denominator);
            pathLength += GetPathLengthCount(SULPHUR_COUNT, collected, bestWagon);
            minPathLength = Math.Min(minPathLength, pathLength);
        }

        LogAnswer(minPathLength);
    }
    // ReSharper enable CognitiveComplexity

    private (Vector2<int> Start, Vector2<int> End, Vector2<int>[] Deposits) GetMapData()
    {
        Vector2<int> start = this.Data.PositionOf(Element.START);
        Vector2<int> end   = this.Data.PositionOf(Element.END);
        Vector2<int>[] deposits =
        [
            ..this.Data.EnumeratePositions()
                  .Where(p => p.Element is Element.DEPOSIT)
                  .Select(p => p.Position)
        ];

        return (start, end, deposits);
    }

    private int GetPathLengthAll()
    {
        (Vector2<int> startPosition, Vector2<int> endPosition, Vector2<int>[] deposits) = GetMapData();

        BitVector32 allVisited = new();
        foreach (int i in ..deposits.Length)
        {
            allVisited[i] = true;
        }

        Wagon startWagon = new(startPosition, Direction.RIGHT);
        StateAll startState = new(startWagon);

        int pathLength = 1;
        Queue<StateAll> currentStates = new(100);
        Queue<StateAll> nextStates = new(100);
        HashSet<StateAll> visitedStates = new(100);

        currentStates.Enqueue(startState);
        visitedStates.Add(startState);

        while (!currentStates.IsEmpty)
        {
            while (currentStates.TryDequeue(out StateAll current))
            {
                foreach (Wagon nextWagon in GetNextWagons(current.Wagon))
                {
                    if (nextWagon.Position == endPosition)
                    {
                        if (current.Visited == allVisited) return pathLength;
                        continue;
                    }

                    BitVector32 nextVisited = current.Visited;
                    int depositIndex = deposits.IndexOf(nextWagon.Position);
                    if (depositIndex is not -1)
                    {
                        nextVisited[depositIndex] = true;
                    }

                    StateAll next = new(nextWagon, nextVisited);
                    if (!visitedStates.Add(next)) continue;

                    nextStates.Enqueue(next);
                }
            }

            pathLength++;
            SwapUtils.Swap(ref currentStates, ref nextStates);
        }

        return -1;
    }

    private long GetPathLengthCount(long required, long startCount = 0L, Wagon? startWagon = null)
    {
        (Vector2<int> startPosition, Vector2<int> endPosition, Vector2<int>[] deposits) = GetMapData();

        startWagon ??= new Wagon(startPosition, Direction.RIGHT);
        StateCount startState = new(startWagon.Value, startCount);

        long pathLength = 1L;
        Queue<StateCount> currentStates = new(100);
        Queue<StateCount> nextStates = new(100);
        HashSet<StateCount> visitedStates = new(100);

        currentStates.Enqueue(startState);
        visitedStates.Add(startState);

        while (!currentStates.IsEmpty)
        {
            while (currentStates.TryDequeue(out StateCount current))
            {
                foreach (Wagon nextWagon in GetNextWagons(current.Wagon))
                {
                    if (nextWagon.Position == endPosition)
                    {
                        if (current.Visited >= required) return pathLength;
                        continue;
                    }

                    long nextVisited = current.Visited;
                    int depositIndex = deposits.IndexOf(nextWagon.Position);
                    if (depositIndex is not -1)
                    {
                        nextVisited++;
                    }

                    StateCount next = new(nextWagon, nextVisited);
                    if (!visitedStates.Add(next)) continue;

                    nextStates.Enqueue(next);
                }
            }

            pathLength++;
            SwapUtils.Swap(ref currentStates, ref nextStates);
        }

        return -1L;
    }

    private Fraction<int> GetBestCollectRatio(Wagon startWagon, Vector2<int> endPosition, int maxLength, Vector2<int>[] deposits)
    {
        StateCount startState = new(startWagon);

        int pathLength = 1;
        Fraction<int> bestRatio = Fraction<int>.Zero;

        Queue<StateCount> currentStates   = new(100);
        Queue<StateCount> nextStates      = new(100);
        HashSet<StateCount> visitedStates = new(100);

        currentStates.Enqueue(startState);
        visitedStates.Add(startState);

        while (!currentStates.IsEmpty && pathLength < maxLength)
        {
            while (currentStates.TryDequeue(out StateCount current))
            {
                foreach (Wagon nextWagon in GetNextWagons(current.Wagon))
                {
                    if (nextWagon.Position == endPosition)
                    {
                        Fraction<int> ratio = new((int)current.Visited, pathLength);
                        if (bestRatio < ratio)
                        {
                            bestRatio = ratio;
                        }
                        continue;
                    }

                    long nextVisited = current.Visited;
                    int depositIndex = deposits.IndexOf(nextWagon.Position);
                    if (depositIndex is not -1)
                    {
                        nextVisited++;
                    }

                    StateCount next = new(nextWagon, nextVisited);
                    if (!visitedStates.Add(next)) continue;

                    nextStates.Enqueue(next);
                }
            }

            pathLength++;
            SwapUtils.Swap(ref currentStates, ref nextStates);
        }

        return bestRatio;
    }

    private IEnumerable<Wagon> GetNextWagons(Wagon current)
    {
        Direction direction = current.Direction;
        Vector2<int> position = current.Position + direction;
        if (this.Data.TryGetPosition(position, out Element element) && element is not Element.WALL)
        {
            yield return new Wagon(position, direction);
        }
        direction = current.Direction.TurnLeft();
        position = current.Position + direction;
        if (this.Data.TryGetPosition(position, out element) && element is not Element.WALL)
        {
            yield return new Wagon(position, direction);
        }

        direction = current.Direction.TurnRight();
        position  = current.Position + direction;
        if (this.Data.TryGetPosition(position, out element) && element is not Element.WALL)
        {
            yield return new Wagon(position, direction);
        }
    }

    /// <inheritdoc />
    protected override Element[] LineConverter(string line) => line.Select(c => (Element)c).ToArray();
}
