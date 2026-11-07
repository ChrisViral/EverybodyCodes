using System.Text;
using Challenge.Collections;
using Challenge.Maths.Directions;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Enumerables;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Strings;
using FastEnumUtility;
using ZLinq;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 10
/// </summary>
[Solver(2026, 10)]
public sealed partial class Day10 : GridSolver<Day10.Element>, IEverybodyCodesSolver
{
    public enum Element
    {
        EMPTY = '.',
        WALL  = '#',
        RIGHT = '>',
        LEFT  = '<',
        UP    = '^',
        DOWN  = 'v'
    }

    private sealed class Ghost(Vector2<int> position, Element data, long maxCollisions)
    {
        private Direction direction = data switch
        {
            Element.RIGHT => Direction.RIGHT,
            Element.LEFT  => Direction.LEFT,
            Element.UP    => Direction.UP,
            Element.DOWN  => Direction.DOWN,
            _             => throw new InvalidOperationException("Invalid ghost direction")
        };

        public Vector2<int> Position { get; private set; } = position;

        public long Collisions { get; set; }

        public bool IsActive => this.Collisions != maxCollisions;

        public string State => $"{this.Position}:{(int)this.direction}";

        public void Move(Grid<Element> map)
        {
            Vector2<int> newPosition = this.Position + this.direction;
            Element element = map[newPosition];
            if (element is Element.WALL)
            {
                this.direction = this.direction.TurnRight();
                this.Collisions++;
            }
            else
            {
                this.Position = newPosition;
            }
        }

        public void ProcessCollision()
        {
            this.Collisions++;
            this.direction = this.direction.Invert();
        }

        public override string ToString() => $"Position: {this.Position}, Direction: {this.direction.FastToString()}, Collisions: {this.Collisions}";
    }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const int ROUNDS = 10;
        List<Ghost> ghosts = GetGhosts();
        foreach (int _ in ..ROUNDS)
        {
            SimulateRound(ghosts);
        }

        long collisions = ghosts.Sum(g => g.Collisions);
        LogAnswer(collisions);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const int MAX_COLLISIONS = 10000;
        List<Ghost> ghosts = GetGhosts(MAX_COLLISIONS);
        long rounds = SimulateUntilAllDisappeared(ghosts);
        LogAnswer(rounds);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const long MAX_COLLISIONS = 1000000000000L;
        List<Ghost> ghosts = GetGhosts(MAX_COLLISIONS);

        long rounds = 0L;
        do
        {
            SimulateWithCycles(ref rounds, ghosts, MAX_COLLISIONS);
        }
        while (!ghosts.IsEmpty);

        LogAnswer(rounds);
    }

    private void SimulateWithCycles(ref long rounds, List<Ghost> ghosts, long disappearCollisions)
    {
        StringBuilder stateBuilder = new(1000);
        Dictionary<string, (long round, long[] collisions)> cache = new(1000);
        while (ghosts.TrueForAll(g => g.IsActive))
        {
            rounds++;
            SimulateRound(ghosts);

            stateBuilder.AppendJoin('\n', ghosts.AsEnumerable().Select(g => g.State));
            string state = stateBuilder.ToStringAndClear();
            if (cache.TryGetValue(state, out (long round, long[] collisions) cycle))
            {
                HandleCycle(ref rounds, ghosts, disappearCollisions, cycle);
                return;
            }

            long[] collisions = ghosts.Select(g => g.Collisions).ToArray();
            cache[state] = (rounds, collisions);
        }

        ghosts.RemoveAll(g => !g.IsActive);
    }

    private void HandleCycle(ref long rounds, List<Ghost> ghosts, long disappearCollisions, (long round, long[] collisions) cycle)
    {
        long cycles = long.MaxValue;
        long[] cycleCollisions = new long[ghosts.Count];
        foreach (int i in ..ghosts.Count)
        {
            Ghost ghost = ghosts[i];
            long ghostCycleCollisions = ghost.Collisions - cycle.collisions[i];
            cycleCollisions[i] = ghostCycleCollisions;

            long ghostCycle = (disappearCollisions - ghost.Collisions) / ghostCycleCollisions;
            cycles = Math.Min(cycles, ghostCycle);
        }

        cycles--;
        foreach (int i in ..ghosts.Count)
        {
            ghosts[i].Collisions += cycles * cycleCollisions[i];
        }

        long cycleLength = rounds - cycle.round;
        rounds += cycleLength * cycles;
        while (ghosts.TrueForAll(g => g.IsActive))
        {
            rounds++;
            SimulateRound(ghosts);
        }

        ghosts.RemoveAll(g => !g.IsActive);
    }

    private long SimulateUntilAllDisappeared(List<Ghost> ghosts)
    {
        long rounds;
        for (rounds = 0L; !ghosts.IsEmpty; rounds++)
        {
            SimulateRound(ghosts);
            ghosts.RemoveAll(g => !g.IsActive);
        }
        return rounds;
    }

    private void SimulateRound(List<Ghost> ghosts)
    {
        foreach (Ghost ghost in ghosts)
        {
            ghost.Move(this.Data);
        }

        foreach (IGrouping<Vector2<int>, Ghost> colliding in ghosts.Where(g => g.IsActive)
                                                                   .GroupBy(g => g.Position)
                                                                   .Where(g => g.Count() > 1))
        {
            colliding.ForEach(g => g.ProcessCollision());
        }
    }

    private List<Ghost> GetGhosts(long maxCollisions = long.MaxValue) =>
    [
        ..this.Data.EnumeratePositions()
              .Where(p => p.Element is Element.RIGHT or Element.LEFT or Element.UP or Element.DOWN)
              .Select(p => new Ghost(p.Position, p.Element, maxCollisions))
    ];

    // ReSharper enable CognitiveComplexity
    /// <inheritdoc />
    protected override Element[] LineConverter(string line) => line.Select(c => (Element)c).ToArray();
}
