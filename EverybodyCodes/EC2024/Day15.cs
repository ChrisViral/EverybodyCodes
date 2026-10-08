using System.Collections.Frozen;
using Challenge.Collections.Search;
using Challenge.Maths.Vectors;
using Challenge.Maths.Vectors.BitVectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Enumerables;
using Challenge.Utils.Extensions.Strings;
using ZLinq;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 15
/// </summary>
[Solver(2024, 15)]
public sealed partial class Day15 : GridSolver<char>, IEverybodyCodesSolver
{
    private readonly record struct State(Vector2<int> Position, BitVector32 Collected);

    private const char EMPTY = '.';
    private const char WALL  = '#';
    private const char LAKE  = '~';
    private const int START_OFFSET = 2;

    private Vector2<int> entryPosition;

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const char HERB = 'H';
        int minPathLength = this.Data.Dimensions.Enumerate()
                                .Where(p => this.Data[p] is HERB)
                                .Min(h => SearchUtils.GetPathLengthBFS(this.entryPosition, h, GetNeighbours)!.Value);
        LogAnswer((minPathLength * 2) + START_OFFSET);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        // Get herb types
        FrozenSet<char> herbTypes = [..this.Data.Distinct().Where(v => v is not WALL and not EMPTY and not LAKE)];

        // Get best path
        int minPathLength = FindBestPath(this.entryPosition, herbTypes, addStartToPath: true);
        LogAnswer(minPathLength + START_OFFSET);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        // Get middle graph transition points
        const char FIRST_MIDDLE_TRANSITION = 'K';
        const char SECOND_MIDDLE_TRANSITION = (char)(FIRST_MIDDLE_TRANSITION + 1);
        Vector2<int> leftTransition = this.Data.PositionOf(FIRST_MIDDLE_TRANSITION);
        this.Data[leftTransition] = SECOND_MIDDLE_TRANSITION;
        Vector2<int> rightTransition = this.Data.PositionOf(FIRST_MIDDLE_TRANSITION);

        // Get left graph starting point
        const char LEFT_TRANSITION = 'E';
        Vector2<int> leftStartPosition = this.Data.Dimensions.Enumerate()
                                             .Where(p => this.Data[p] is LEFT_TRANSITION)
                                             .MaxBy(p => p.X);

        // Get right graph starting point
        const char RIGHT_TRANSITION = 'R';
        Vector2<int> rightStartPosition = this.Data.Dimensions.Enumerate()
                                              .Where(p => this.Data[p] is RIGHT_TRANSITION)
                                              .MinBy(p => p.X);

        // Get all herb types
        char[] allHerbTypes = [..this.Data.Distinct().Where(v => v is not WALL and not EMPTY and not LAKE)];
        FrozenSet<char> leftHerbTypes   = [..allHerbTypes.Where(h => h <= LEFT_TRANSITION)];
        FrozenSet<char> middleHerbTypes = [..allHerbTypes.Where(h => h is > LEFT_TRANSITION and <= SECOND_MIDDLE_TRANSITION)];
        FrozenSet<char> rightHerbTypes  = [..allHerbTypes.Where(h => h > SECOND_MIDDLE_TRANSITION)];

        // Get transition distances
        int middleToLeftPathLength  = SearchUtils.GetPathLengthBFS(leftTransition, leftStartPosition, GetNeighbours)!.Value;
        int middleToRightPathLength = SearchUtils.GetPathLengthBFS(rightTransition, rightStartPosition, GetNeighbours)!.Value;

        // Calculate subgraph path lengths
        int leftPathLength   = FindBestPath(leftStartPosition, leftHerbTypes);
        int middlePathLength = FindBestPath(this.entryPosition, middleHerbTypes, addStartToPath: true);
        int rightPathLength  = FindBestPath(rightStartPosition, rightHerbTypes);

        // Add total path length
        int totalPathLength = leftPathLength + rightPathLength + middlePathLength
                            + (2 * middleToLeftPathLength) + (2 * middleToRightPathLength);
        LogAnswer(totalPathLength + START_OFFSET);
    }
    // ReSharper enable CognitiveComplexity

    private IEnumerable<Vector2<int>> GetNeighbours(Vector2<int> p)
    {
        return p.AsAdjacentEnumerable().Where(a => this.Data[a] is not WALL and not LAKE);
    }

    private int FindBestPath(Vector2<int> startPosition, FrozenSet<char> herbTypes, bool addStartToPath = false)
    {
        // Get position map for each herb
        FrozenDictionary<char, Vector2<int>[]> herbPositions = this.Data.Dimensions.Enumerate()
                                                                   .Where(p => herbTypes.Contains(this.Data[p]))
                                                                   .GroupBy(p => this.Data[p])
                                                                   .ToFrozenDictionary(g => g.Key, g => g.ToArray());

        // Get list of all points of interest to path for
        List<Vector2<int>> pointsOfInterst = [..herbPositions.Values.SelectMany(p => p)];
        if (addStartToPath)
        {
            pointsOfInterst.Add(startPosition);
        }

        // Calculate pathes between all points of interest
        Dictionary<Vector2<int>, Dictionary<Vector2<int>, int>> distanceCaches = new(100);
        Dictionary<UnorderedPair<Vector2<int>>, int> distances = new(1000);
        foreach (UnorderedPair<Vector2<int>> pair in pointsOfInterst.EnumeratePairs().Where(p => this.Data[p.First] != this.Data[p.Second]))
        {
            if (!distanceCaches.TryGetValue(pair.Second, out Dictionary<Vector2<int>, int>? cache))
            {
                cache = new Dictionary<Vector2<int>, int>(1000);
                distanceCaches[pair.Second] = cache;
            }

            int distance = SearchUtils.GetPathLength(pair.First, pair.Second,
                                                     c => Vector2<int>.ManhattanDistance(c, pair.Second),
                                                     n => n.AsAdjacentEnumerable()
                                                           .Where(a => this.Data[a] is not WALL and not LAKE)
                                                           .Select(p => new MoveData<Vector2<int>, int>(p)),
                                                     MinSearchComparer<int>.Comparer, cache)!.Value;
            distances[pair] = distance;
        }

        // Create start state
        BitVector32 startState = new();
        char startValue = this.Data[startPosition];
        if (herbTypes.Contains(startValue))
        {
            startState[startValue.AsIndex] = true;
        }
        State start = new(startPosition, startState);

        // Create final state
        BitVector32 finalState = new();
        herbTypes.ForEach(h => finalState[h.AsIndex] = true);
        State end = new(startPosition, finalState);

        // Calculate final path distance
        SearchUtils.Search(start, end, null, FindHerbs, MinSearchComparer<int>.Comparer, out int totalDistance);
        return totalDistance;

        IEnumerable<MoveData<State, int>> FindHerbs(State state)
        {
            // If we're in the final state, go back to the start
            if (state.Collected.Data == finalState)
            {
                yield return new MoveData<State, int>(state with { Position = startPosition }, distances[(state.Position, startPosition)]);
                yield break;
            }

            // Else, go through every herb type
            foreach (char herbType in herbTypes)
            {
                // Check we haven't collected that herb type yet
                int herbIndex = herbType.AsIndex;
                if (state.Collected[herbIndex]) continue;

                // Mark it as collected down the line
                BitVector32 collected = state.Collected;
                collected[herbIndex] = true;

                // Try going to every potential herb of the type
                foreach (Vector2<int> herb in herbPositions[herbType])
                {
                    yield return new MoveData<State, int>(new State(herb, collected), distances[(state.Position, herb)]);
                }
            }
        }
    }

    /// <inheritdoc />
    public override void ParseInput(string input)
    {
        base.ParseInput(input);

        // Get starting position
        int startX = this.Data.GetRow(0).IndexOf(EMPTY);
        this.Data[startX, 0] = WALL;
        this.entryPosition = new Vector2<int>(startX, 1);
    }

    /// <inheritdoc />
    protected override char[] LineConverter(string line) => line.ToCharArray();
}
