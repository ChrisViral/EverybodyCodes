using Challenge.Collections.Search;
using Challenge.Maths.Directions;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using ZLinq;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 17
/// </summary>
[Solver(2026, 17)]
public sealed partial class Day17 : GridSolver<bool>, IEverybodyCodesSolver
{
    private const char GROUND = '#';

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        Vector2<int> start = Vector2<int>.Zero;
        Vector2<int> end = this.Data.Dimensions - Vector2<int>.One;

        int steps = SearchUtils.GetPathLengthBFS(start, end, Neighbours)!.Value;
        LogAnswer(steps);
        return;

        IEnumerable<Vector2<int>> Neighbours(Vector2<int> position)
        {
            foreach (Vector2<int> adjacent in position.AsAdjacentEnumerable())
            {
                if (this.Data.IsPositionEqual(adjacent, true))
                {
                    yield return adjacent;
                }
            }
        }
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        int perimeter = GetPerimeter();
        LogAnswer(perimeter);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        int perimeter = GetPerimeter();

        Direction direction = Direction.LEFT;
        Vector2<int> position = Vector2<int>.Zero;

        int distance = 0;
        Vector2<int> target = this.Data.Dimensions - Vector2<int>.One;
        while (position != target || direction is not Direction.RIGHT)
        {
            Vector2<int> nextPosition = position + direction;
            if (this.Data.IsPositionEqual(nextPosition, true))
            {
                position  = nextPosition;
                direction = direction.TurnRight();
            }
            else
            {
                distance++;
                direction = direction.TurnLeft();
            }
        }

        int minDistance = Math.Min(distance, perimeter - distance);
        LogAnswer(minDistance);
    }
    // ReSharper enable CognitiveComplexity

    private int GetPerimeter()
    {
        int perimeter = 0;
        foreach (Vector2<int> position in this.Data.Dimensions.Enumerate())
        {
            if (!this.Data[position]) continue;

            foreach (Vector2<int> adjacent in position.Adjacent())
            {
                if (!this.Data.TryGetPosition(adjacent, out bool isGround) || !isGround)
                {
                    perimeter++;
                }
            }
        }

        return perimeter;
    }

    /// <inheritdoc />
    protected override bool[] LineConverter(string line) => line.Select(c => c is GROUND).ToArray();
}
