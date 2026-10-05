using Challenge.Collections.Search;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using ZLinq;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 13
/// </summary>
[Solver(2024, 13)]
public sealed partial class Day13 : GridSolver<char>, IEverybodyCodesSolver
{
    private const char WALL  = '#';
    private const char START = 'S';
    private const char END   = 'E';

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        Vector2<int> startPosition = this.Data.PositionOf(START);
        Vector2<int> endPosition   = this.Data.PositionOf(END);
        SearchUtils.Search(startPosition, endPosition, p => Vector2<int>.ManhattanDistance(p, endPosition),
                           FindNeighbours, MinSearchComparer<int>.Comparer, out int shortestPath);
        LogAnswer(shortestPath);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2() => RunPart1();

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        Vector2<int> endPosition = this.Data.PositionOf(END);
        this.Data[endPosition]   = '0';

        int shortestPath = int.MaxValue;
        List<Vector2<int>> startPositions = [..this.Data.Dimensions.Enumerate().Where(p => this.Data[p] is START)];
        foreach (Vector2<int> startPosition in startPositions)
        {
            Vector2<int>[]? path = SearchUtils.Search(startPosition, endPosition, p => Vector2<int>.ManhattanDistance(p, endPosition),
                                                      FindNeighbours, MinSearchComparer<int>.Comparer, out int pathLength);

            if (path is not null)
            {
                shortestPath = Math.Min(shortestPath, pathLength);
            }
        }
        LogAnswer(shortestPath);
    }
    // ReSharper enable CognitiveComplexity

    private IEnumerable<MoveData<Vector2<int>, int>> FindNeighbours(Vector2<int> currentPosition)
    {
        char currentCell = this.Data[currentPosition];
        if (currentCell is START)
        {
            currentCell = '0';
        }

        foreach (Vector2<int> target in currentPosition.AsAdjacentEnumerable())
        {
            if (!this.Data.TryGetPosition(target, out char cell) || cell is WALL or START) continue;

            int absoluteDistance = Math.Abs(currentCell - cell);
            int travelDistance = Math.Min(absoluteDistance, 10 - absoluteDistance) + 1;
            yield return new MoveData<Vector2<int>, int>(target, travelDistance);
        }
    }

    /// <inheritdoc />
    protected override char[] LineConverter(string line) => line.ToCharArray();
}
