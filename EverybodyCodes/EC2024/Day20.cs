using System.Runtime.CompilerServices;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Strings;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 20
/// </summary>
[Solver(2024, 20)]
public sealed partial class Day20 : GridSolver<char>, IEverybodyCodesSolver
{
    private readonly record struct Glider(Vector2<int> Position, Direction Direction, int Checkpoints, int Altitude)
    {
        private readonly int hashCode = HashCode.Combine(Position, (int)Direction, Checkpoints);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Glider other) => this.Checkpoints == other.Checkpoints
                                         && this.Direction == other.Direction
                                         && this.Position == other.Position;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => this.hashCode;
    }

    private const char START = 'S';
    private const char ROCK  = '#';

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const int START_ALTITUDE   = 1000;
        const int FLIGHT_TIME = 100;
        Vector2<int> startPosition = this.Data.PositionOf(START);
        int maxAltitude = FindMaxAltitude(startPosition, Direction.DOWN, START_ALTITUDE, FLIGHT_TIME);
        LogAnswer(maxAltitude);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const int CHECKPOINTS = 3;
        const int START_ALTITUDE = 10000;
        Vector2<int> startPosition = this.Data.PositionOf(START);
        int minTime = FindMinPath(startPosition, Direction.DOWN, CHECKPOINTS, START_ALTITUDE);
        LogAnswer(minTime);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int START_ALTITUDE = 384400;
        int startColumn = this.Data.GetRow(0).IndexOf(START);

        // Find best colum to stick to
        int bestColumnIndex    = -1;
        int bestColumnDrop     = int.MaxValue;
        int bestColumnDistance = int.MaxValue;
        Span<char> columnBuffer = stackalloc char[this.Data.Height];
        foreach (int columnIndex in ..this.Data.Width)
        {
            // Make sure there are no rocks in the column
            this.Data.GetColumn(columnIndex, columnBuffer);
            if (columnBuffer.Contains(ROCK)) continue;

            // Find the column with the smallest drop, closest to the start
            int columnDrop = Math.Abs(columnBuffer.Sum(GetAltitudeChange));
            int columnDistance = Math.Abs(columnIndex - startColumn) - 1;
            if (bestColumnDrop > columnDrop
             || (bestColumnDrop == columnDrop && bestColumnDistance > columnDistance))
            {
                bestColumnIndex    = columnIndex;
                bestColumnDrop     = columnDrop;
                bestColumnDistance = columnDistance;
            }
        }

        // Remove time to move to column, and then calculate how many full cycles we can clear
        int altitude = START_ALTITUDE - bestColumnDistance;
        (int fullPasses, altitude) = Math.DivRem(altitude, bestColumnDrop);

        // Simulate the final pass to make sure we didn't drop below zero in the middle of it
        fullPasses--;
        altitude += bestColumnDrop;

        // Simulate the final column until we drop to 0
        int distance = (fullPasses * this.Data.Height) - 1;
        this.Data.GetColumn(bestColumnIndex, columnBuffer);
        for (int i = 0; altitude > 0; i++)
        {
            distance++;
            altitude += GetAltitudeChange(columnBuffer[i % columnBuffer.Length]);
        }

        LogAnswer(distance);
    }
    // ReSharper enable CognitiveComplexity

    private int FindMaxAltitude(Vector2<int> startPosition, Direction startDirection, int startAltitude, int flightTime)
    {
        // DP approach, keep max altitude on each step
        Dictionary<(Vector2<int> position, Direction direction, int remaining), int> cache = new(1000);
        return startAltitude + FindMaxAltitudeChange(startPosition, startDirection, flightTime) + 1;

        int FindMaxAltitudeChange(Vector2<int> position, Direction direction, int remaining)
        {
            (Vector2<int>, Direction, int) cacheKey = (position, direction, remaining);
            if (cache.TryGetValue(cacheKey, out int result)) return result;

            if (!this.Data.TryGetPosition(position, out char air) || air is ROCK)
            {
                cache[cacheKey] = int.MinValue;
                return int.MinValue;
            }

            int currentChange = GetAltitudeChange(air);
            if (remaining is 0) return currentChange;

            Direction invalidDirection = direction.Invert();
            int maxNextChange = Direction.CardinalDirections.Where(d => d != invalidDirection)
                                         .Max(d => FindMaxAltitudeChange(position + d, d, remaining - 1));

            result = maxNextChange is not int.MinValue ? currentChange + maxNextChange : int.MinValue;
            cache[cacheKey] = result;
            return result;
        }
    }

    private int FindMinPath(Vector2<int> startPosition, Direction startDirection, int checkpoints, int startAltitude)
    {
        // BFS approach, keep track of every step and find first to reach target
        int steps = 1;
        Queue<Glider> search     = new(200000);
        Queue<Glider> nextSearch = new(200000);
        Dictionary<Glider, int> visited = new(100000);

        Glider startGlider = new(startPosition, startDirection, 0, startAltitude);
        visited[startGlider] = startAltitude;
        search.Enqueue(startGlider);

        while (true)
        {
            while (search.TryDequeue(out Glider glider))
            {
                // Store best new state here
                Direction invalidDirection = glider.Direction.Invert();
                foreach (Direction newDirection in Direction.CardinalDirections.Where(d => d != invalidDirection))
                {
                    // Make sure we're moving within the grid
                    if (!this.Data.TryMoveWithinGrid(glider.Position, newDirection, out Vector2<int> newPosition)) continue;

                    // Make sure we're not moving into a rock
                    char air = this.Data[newPosition];
                    if (air is ROCK) continue;

                    // Get altitude and checkpoints
                    int newAltitude = glider.Altitude + GetAltitudeChange(air);
                    int newCheckpoints = glider.Checkpoints;
                    if (air.IsUpperChar && air.AsIndex == newCheckpoints)
                    {
                        newCheckpoints++;
                    }
                    // Check if we're in the final state
                    else if (newCheckpoints == checkpoints && newPosition == startPosition && newAltitude >= startAltitude)
                    {
                        return steps;
                    }

                    // Check if we've seen this state before at a greater altitude
                    Glider newGlider = new(newPosition, newDirection, newCheckpoints, newAltitude);
                    if (visited.TryGetValue(newGlider, out int previousAltitude) && previousAltitude >= newAltitude) continue;

                    // Enqueue for next tuirn
                    visited[newGlider] = newAltitude;
                    nextSearch.Enqueue(newGlider);
                }
            }

            steps++;
            SwapUtils.Swap(ref search, ref nextSearch);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetAltitudeChange(char air) => air switch
    {
        '.'   => -1,
        '-'   => -2,
        '+'   =>  1,
        ROCK  => throw new InvalidOperationException("Cannot navigate through a rock"),
        _     => -1,
    };

    /// <inheritdoc />
    protected override char[] LineConverter(string line) => line.ToCharArray();
}
