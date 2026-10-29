using System.Collections.Immutable;
using System.Diagnostics;
using Challenge.Collections.Search;
using Challenge.Maths.Directions;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils;
using Challenge.Utils.Extensions.Numbers;
using Challenge.Utils.Extensions.Ranges;
using ZLinq;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 20
/// </summary>
[Solver(2025, 20)]
public sealed partial class Day20 : GridSolver<Day20.Element>, IEverybodyCodesSolver
{
    public enum Element
    {
        EMPTY      = '.',
        TRAMPOLINE = 'T',
        START      = 'S',
        END        = 'E',
        VOID       = '#'
    }

    private readonly record struct State(Vector2<int> Position, int Rotation = 0);

    private static readonly ImmutableArray<Direction> EvenMoves = [Direction.NONE, Direction.LEFT, Direction.RIGHT, Direction.UP];
    private static readonly ImmutableArray<Direction> OddMoves  = [Direction.NONE, Direction.LEFT, Direction.RIGHT, Direction.DOWN];

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        HashSet<UnorderedPair<Vector2<int>>> transitions = GetValidTransitions();
        LogAnswer(transitions.Count);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        HashSet<UnorderedPair<Vector2<int>>> transitions = GetValidTransitions();
        Vector2<int> start = this.Data.PositionOf(Element.START);
        Vector2<int> end   = this.Data.PositionOf(Element.END);
        int jumps = SearchUtils.GetPathLengthBFS(start, end, p => GetValidTransitions(p, transitions))!.Value;
        LogAnswer(jumps);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        Vector2<int> start = this.Data.PositionOf(Element.START);
        Vector2<int> end   = this.Data.PositionOf(Element.END);

        int jumps = SearchUtils.GetPathLength(new State(start), new State(end), null,
                                              GetValidRotatedTransitions,
                                              MinSearchComparer<int>.Comparer,
                                              goalFound: GoalFound)!.Value;
        LogAnswer(jumps);
    }

    private bool GoalFound(State current, State goal) => current.Rotation switch
    {
        0 => current.Position == goal.Position,
        1 => current.Position == GetRotatedPosition(goal.Position),
        2 => GetRotatedPosition(current.Position) == goal.Position,
        _ => throw new UnreachableException("Invalid rotation state")
    };

    // ReSharper enable CognitiveComplexity

    private HashSet<UnorderedPair<Vector2<int>>> GetValidTransitions()
    {
        HashSet<UnorderedPair<Vector2<int>>> transitions = new(this.Data.Size);
        foreach (int y in ..(this.Data.Height - 1))
        {
            bool isRowEven = y.IsEven;
            int width = this.Data.Width - y;
            bool previousIsTrampoline = this.Data[y, y] is not Element.VOID;
            for (int x = y + 1; x < width; x += 1)
            {
                Vector2<int> position = new(x, y);
                if (this.Data[position] is Element.VOID)
                {
                    previousIsTrampoline = false;
                    continue;
                }

                if (previousIsTrampoline)
                {
                    transitions.Add((position + Direction.LEFT, position));
                }

                previousIsTrampoline = true;
                bool isColumnEven = x.IsEven;
                if (isRowEven == isColumnEven) continue;

                Vector2<int> positionBelow = position + Direction.DOWN;
                if (this.Data[positionBelow] is not Element.VOID)
                {
                    transitions.Add((position, positionBelow));
                }
            }
        }

        return transitions;
    }

    private static IEnumerable<Vector2<int>> GetValidTransitions(Vector2<int> position, HashSet<UnorderedPair<Vector2<int>>> transitions)
    {
        return position.AsAdjacentEnumerable().Where(a => transitions.Contains((position, a)));
    }

    private IEnumerable<MoveData<State, int>> GetValidRotatedTransitions(State state)
    {
        ImmutableArray<Direction> validDirections = state.Position.X.IsEven == state.Position.Y.IsEven ? EvenMoves : OddMoves;
        foreach (Direction direction in validDirections)
        {
            if (!this.Data.TryMoveWithinGrid(state.Position, direction, out Vector2<int> newPosition)
             || this.Data[newPosition] is Element.EMPTY) continue;

            Vector2<int> rotatedPosition = GetRotatedPosition(newPosition);
            if (this.Data[rotatedPosition] is not Element.VOID)
            {
                yield return new MoveData<State, int>(new State(rotatedPosition, (state.Rotation + 1) % 3));
            }
        }
    }

    private Vector2<int> GetRotatedPosition(Vector2<int> position)
    {
        int height = (this.Data.Width - 1) / 2;
        int x = height + (2 * position.Y) - ((position.X + position.Y) / 2);
        int y = height - ((position.X + position.Y + 1) / 2);
        return new Vector2<int>(x, y);
    }

    /// <inheritdoc />
    protected override Element[] LineConverter(string line) => line.Select(c => (Element)c).ToArray();

    /// <inheritdoc />
    protected override string StringConversion(Element obj) => ((char)obj).ToString();
}
