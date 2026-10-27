using Challenge.Collections;
using Challenge.Collections.Search;
using Challenge.Maths;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils.Extensions.Strings;
using ZLinq;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 17
/// </summary>
[Solver(2025, 17)]
public sealed partial class Day17 : GridSolver<byte>, IEverybodyCodesSolver
{
    private readonly record struct State(Vector2<int> Position, int QuadrantsVisited, int LowestVisited = int.MaxValue, int TimeElapsed = 0)
    {
        /// <inheritdoc />
        public bool Equals(State other) => this.Position == other.Position
                                        && this.QuadrantsVisited == other.QuadrantsVisited
                                        && this.LowestVisited == other.LowestVisited;

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(this.Position, this.QuadrantsVisited, this.LowestVisited);
    }

    private const char SOURCE = '@';
    private const char START  = 'S';

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const int RADIUS = 10;
        const int SQUARED_RADIUS = RADIUS * RADIUS;

        Vector2<int> source = this.Data.PositionOf(byte.MinValue);
        int destroyed = this.Data.EnumeratePositions()
                            .Where(p => (p.Position - source).SquaredLength <= SQUARED_RADIUS)
                            .Sum(p => p.Element);
        LogAnswer(destroyed);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        Vector2<int> source = this.Data.PositionOf(byte.MinValue);

        int largestSquaredLength = 0;
        DefaultDictionary<int, int> destructions = new(0);
        foreach ((Vector2<int> position, byte value) in this.Data.EnumeratePositions())
        {
            int squaredLength = (position - source).SquaredLength;
            destructions[squaredLength] += value;
            largestSquaredLength = Math.Max(largestSquaredLength, squaredLength);
        }

        int maxDestroyed = 0;
        int maxDestruction = 0;
        for (int currentSquaredLength = 1, maxLength = 1, maxSquaredLength = 1;
             currentSquaredLength < largestSquaredLength;
             maxLength++, maxSquaredLength = maxLength * maxLength)
        {
            int destroyed = 0;
            do
            {
                destroyed += destructions[currentSquaredLength];
            }
            while (++currentSquaredLength <= maxSquaredLength && currentSquaredLength <= largestSquaredLength);

            if (maxDestroyed < destroyed)
            {
                maxDestroyed   = destroyed;
                maxDestruction = destroyed * maxLength;
            }
        }

        LogAnswer(maxDestruction);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int FLOW_TIME = 30;

        Vector2<int> source = this.Data.PositionOf(byte.MinValue);
        Vector2<int> start  = this.Data.PositionOf(byte.MaxValue);
        this.Data[start] = byte.MinValue;

        Grid<int> timeGrid = new(this.Data.Width, this.Data.Height);
        foreach (Vector2<int> position in this.Data.Dimensions.Enumerate())
        {
            int squareLength = (position - source).SquaredLength;
            int length = MathUtils.IntegerSqrt(squareLength);
            if (squareLength != length * length)
            {
                length++;
            }

            timeGrid[position] = length * FLOW_TIME;
        }

        State startState = new(start, 1);
        State endState   = new(start, 4);
        int loopTime = SearchUtils.GetPathLength(startState, endState, null,
                                                 state => GetNextStates(state, source, timeGrid),
                                                 MinSearchComparer<int>.Comparer,
                                                 goalFound: IsEndState)!.Value;
        int flowDistance = loopTime / FLOW_TIME;
        LogAnswer(loopTime * flowDistance);
    }
    // ReSharper enable CognitiveComplexity

    private IEnumerable<MoveData<State, int>> GetNextStates(State state, Vector2<int> source, Grid<int> timeGrid)
    {
        foreach (Vector2<int> newPosition in state.Position.AsAdjacentEnumerable())
        {
            if (!this.Data.TryGetPosition(newPosition, out byte travelTime)) continue;

            int newLowestVisited = Math.Min(state.LowestVisited, timeGrid[newPosition]);
            int newTimeElapsed = state.TimeElapsed + travelTime;
            if (newTimeElapsed >= newLowestVisited) continue;

            int quadrant = GetPositionQuadrant(newPosition, source);
            int newQuadrantsVisited = quadrant == state.QuadrantsVisited ? state.QuadrantsVisited + 1 : state.QuadrantsVisited;
            yield return new MoveData<State, int>(new State(newPosition, newQuadrantsVisited, newLowestVisited, newTimeElapsed), travelTime);
        }
    }

    private static int GetPositionQuadrant(Vector2<int> position, Vector2<int> source)
    {
        Vector2<int> relative = position - source;
        return (Math.Sign(relative.X), Math.Sign(relative.Y)) switch
        {
            (-1, 0) => 1,
            ( 0, 1) => 2,
            ( 1, 0) => 3,
            _       => 0
        };
    }

    private static bool IsEndState(State current, State goal) => current.Position == goal.Position
                                                              && current.QuadrantsVisited == goal.QuadrantsVisited;

    /// <inheritdoc />
    protected override byte[] LineConverter(string line) => line.Select(c => c switch
    {
        SOURCE => byte.MinValue,
        START  => byte.MaxValue,
        _      => (byte)c.AsIndex
    }).ToArray();
}
