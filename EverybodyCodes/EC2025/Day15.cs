using System.Collections.Immutable;
using Challenge.Collections.Search;
using Challenge.Maths;
using Challenge.Maths.Directions;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Arrays;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Ranges;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 15
/// </summary>
[Solver(2025, 15)]
public sealed partial class Day15 : Solver<ImmutableArray<DirectionInstruction>>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        Vector2<int> position = Vector2<int>.Zero;
        Direction direction = Direction.UP;

        OrthogonalLine[] walls = new OrthogonalLine[this.Data.Length];
        Vector2<int>[] points = new Vector2<int>[this.Data.Length + 1];

        points[0] = Vector2<int>.Zero;
        walls[0]  = CreateWallFromInstruction(ref position, ref direction, this.Data[0], false);
        foreach (int i in 1..(this.Data.Length - 1))
        {
            DirectionInstruction instruction = this.Data[i];
            points[i] = GetNextCorner(position, direction, instruction);
            walls[i] = CreateWallFromInstruction(ref position, ref direction, instruction, false);
        }
        points[^2] = GetNextCorner(position, direction, this.Data[^1]);
        walls[^1]  = CreateWallFromInstruction(ref position, ref direction, this.Data[^1], true);
        points[^1] = position;

        Dictionary<UnorderedPair<Vector2<int>>, int> validMoves = new(this.Data.Length);
        foreach (UnorderedPair<Vector2<int>> pair in points.EnumeratePairs())
        {
            if (IsValidMove(pair.First, pair.Second, walls))
            {
                validMoves.Add(pair, Vector2<int>.ManhattanDistance(pair.First, pair.Second));
            }
        }

        int pathLength = SearchUtils.GetPathLength(Vector2<int>.Zero, position, null, p => GetNextMoves(p, points, validMoves), MinSearchComparer<int>.Comparer)!.Value;
        LogAnswer(pathLength);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2() => RunPart1();

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3() => RunPart1();
    // ReSharper enable CognitiveComplexity

    private static OrthogonalLine CreateWallFromInstruction(ref Vector2<int> position, ref Direction direction, DirectionInstruction instruction, bool isLastSegment)
    {
        direction =  direction.TurnBy(instruction.Direction);
        position  += direction;
        Vector2<int> start = position;

        position += direction.ToVector(instruction.Distance - 1);
        Vector2<int> end = isLastSegment ? position - direction : position;
        return new OrthogonalLine(start, end);
    }

    private static Vector2<int> GetNextCorner(Vector2<int> position, Direction direction, DirectionInstruction instruction)
    {
        return position + direction + direction.TurnBy(instruction.Direction.Invert());
    }

    private static bool DoesNotIntersectAnyWalls(OrthogonalLine line, OrthogonalLine[] walls)
    {
        return !walls.Exists(w => OrthogonalLine.Intersects(w, line));
    }

    private static bool IsValidMove(Vector2<int> from, Vector2<int> to, OrthogonalLine[] walls)
    {
        // If we're colinear, check only the relevant line
        if (from.X == to.X || from.Y == to.Y)
        {
            OrthogonalLine line = new(from, to);
            return DoesNotIntersectAnyWalls(line, walls);
        }

        Vector2<int> intermediate = new(from.X, to.Y);
        OrthogonalLine first = new(from, intermediate);
        if (DoesNotIntersectAnyWalls(first, walls))
        {
            OrthogonalLine second = new(intermediate, to);
            if (DoesNotIntersectAnyWalls(second, walls))
            {
                return true;
            }
        }

        intermediate = new Vector2<int>(to.X, from.Y);
        first        = new OrthogonalLine(from, intermediate);
        if (DoesNotIntersectAnyWalls(first, walls))
        {
            OrthogonalLine second = new(intermediate, to);
            return DoesNotIntersectAnyWalls(second, walls);
        }

        return false;
    }

    private static IEnumerable<MoveData<Vector2<int>, int>> GetNextMoves(Vector2<int> current, Vector2<int>[] points, Dictionary<UnorderedPair<Vector2<int>>, int> validMoves)
    {
        foreach (Vector2<int> target in points)
        {
            if (target == current) continue;

            UnorderedPair<Vector2<int>> move = new(current, target);
            if (validMoves.TryGetValue(move, out int distance))
            {
                yield return new MoveData<Vector2<int>, int>(target, distance);
            }
        }
    }

    /// <inheritdoc />
    protected override ImmutableArray<DirectionInstruction> Convert(string[] rawInput) => ParseUtils.ParseImmutableArray(rawInput[0], ',', DirectionInstruction.Parse);
}
