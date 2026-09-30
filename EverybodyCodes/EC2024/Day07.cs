using System.Collections.Immutable;
using Challenge.Collections;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Spans;
using ZLinq;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 7
/// </summary>
[Solver(2024, 7)]
public sealed partial class Day07 : ArraySolver<Day07.Plan>, IEverybodyCodesSolver
{
    public enum Action : sbyte
    {
        NONE = sbyte.MinValue,
        DECREASE = -1,
        EQUAL    = 0,
        INCREASE = 1
    }

    public readonly record struct Plan(string Name, Action[] Actions)
    {
        private const int INITIAL_POWER = 10;

        public int GetTotalEssence()
        {
            int essence = 0;
            int power = 10;
            foreach (Action action in this.Actions)
            {
                power += (int)action;
                essence += power;
            }
            return essence;
        }

        public long GetEssenceWithTrack(ImmutableArray<Action> track, int loops)
        {
            long essence = 0L;
            int power = 10;
            int index = 0;
            foreach (int _ in ..loops)
            {
                foreach (Action trackAction in track)
                {
                    power += trackAction is Action.EQUAL
                                 ? (int)this.Actions[index]
                                 : (int)trackAction;
                    essence += power;
                    index = (index + 1) % this.Actions.Length;
                }
            }
            return essence;
        }
    }

    private const int PART2_LOOPS = 10;
    private const string PART2_TRACK = """
                                       S-=++=-==++=++=-=+=-=+=+=--=-=++=-==++=-+=-=+=-=+=+=++=-+==++=++=-=-=--
                                       -                                                                     -
                                       =                                                                     =
                                       +                                                                     +
                                       =                                                                     +
                                       +                                                                     =
                                       =                                                                     =
                                       -                                                                     -
                                       --==++++==+=+++-=+=-=+=-+-=+-=+-=+=-=+=--=+++=++=+++==++==--=+=++==+++-
                                       """;

    private const int PART3_LOOPS = 2024;
    private const string PART3_TRACK = """
                                       S+= +=-== +=++=     =+=+=--=    =-= ++=     +=-  =+=++=-+==+ =++=-=-=--
                                       - + +   + =   =     =      =   == = - -     - =  =         =-=        -
                                       = + + +-- =-= ==-==-= --++ +  == == = +     - =  =    ==++=    =++=-=++
                                       + + + =     +         =  + + == == ++ =     = =  ==   =   = =++=       
                                       = = + + +== +==     =++ == =+=  =  +  +==-=++ =   =++ --= + =          
                                       + ==- = + =   = =+= =   =       ++--          +     =   = = =--= ==++==
                                       =     ==- ==+-- = = = ++= +=--      ==+ ==--= +--+=-= ==- ==   =+=    =
                                       -               = = = =   +  +  ==+ = = +   =        ++    =          -
                                       -               = + + =   +  -  = + = = +   =        +     =          -
                                       --==++++==+=+++-= =-= =-+-=  =+-= =-= =--   +=++=+++==     -=+=++==+++-
                                       """;
    private static readonly ImmutableArray<Action> Part3Actions =
    [
        Action.INCREASE, Action.INCREASE, Action.INCREASE, Action.INCREASE, Action.INCREASE,
        Action.EQUAL,    Action.EQUAL,    Action.EQUAL,
        Action.DECREASE, Action.DECREASE, Action.DECREASE
    ];

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        string ranking = string.Concat(this.Data.AsEnumerable()
                                           .OrderByDescending(p => p.GetTotalEssence())
                                           .Select(p => p.Name));
        LogAnswer(ranking);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        ImmutableArray<Action> track = ParseCircularTrack(PART2_TRACK);
        string ranking = string.Concat(this.Data.AsEnumerable()
                                           .OrderByDescending(p => p.GetEssenceWithTrack(track, PART2_LOOPS))
                                           .Select(p => p.Name));
        LogAnswer(ranking);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        ImmutableArray<Action> track = ParseComplexTrack(PART3_TRACK);
        long opponentEssence = this.Data[0].GetEssenceWithTrack(track, PART3_LOOPS);
        int winning = Part3Actions.AsSpan()
                                  .UniquePermutationsInPlace()
                                  .AsValueEnumerable()
                                  .Select(actions => new Plan(string.Empty, actions))
                                  .Select(plan => plan.GetEssenceWithTrack(track, PART3_LOOPS))
                                  .Count(planEssence => planEssence > opponentEssence);
        LogAnswer(winning);
    }
    // ReSharper enable CognitiveComplexity

    private static ImmutableArray<Action> ParseCircularTrack(string track)
    {
        string[] lines = track.Split('\n', DEFAULT_OPTIONS);
        Grid<Action> trackGrid = new(lines[0].Length, lines.Length, lines, l => l.Select(ParseAction).ToArray());

        int trackLength = ((trackGrid.Width + trackGrid.Height) * 2) - 4;
        ImmutableArray<Action>.Builder builder = ImmutableArray.CreateBuilder<Action>(trackLength);
        Span<Action> columnSpan = stackalloc Action[trackGrid.Height];
        Span<Action> rowSpan    = stackalloc Action[trackGrid.Width];

        builder.AddRange(trackGrid.GetRow(0)[1..^1]);
        trackGrid.GetColumn(^1, columnSpan);
        builder.AddRange(columnSpan);
        trackGrid.GetRow(^1).CopyTo(rowSpan);
        rowSpan.Reverse();
        builder.AddRange(rowSpan[1..^1]);
        trackGrid.GetColumn(0, columnSpan);
        columnSpan.Reverse();
        builder.AddRange(columnSpan);

        return builder.ToImmutable();
    }

    private static ImmutableArray<Action> ParseComplexTrack(string track)
    {
        string[] lines = track.Split('\n').Select(l => l.Trim('\r')).ToArray();
        Grid<Action> trackGrid = new(lines[0].Length, lines.Length, lines, l => l.Select(ParseAction).ToArray());

        Vector2<int> position = Vector2<int>.Zero;
        Direction direction = Direction.RIGHT;
        List<Action> actions = new(100);
        do
        {
            if (trackGrid.TryMoveWithinGrid(position, direction, out Vector2<int> newPosition) && trackGrid[newPosition] is not Action.NONE)
            {
                Action newAction = trackGrid[newPosition];
                if (newAction is not Action.NONE)
                {
                    position = newPosition;
                    actions.Add(newAction);
                    continue;
                }
            }

            direction = direction.TurnLeft();
            if (trackGrid.TryMoveWithinGrid(position, direction, out newPosition))
            {
                Action newAction = trackGrid[newPosition];
                if (newAction is not Action.NONE)
                {
                    position = newPosition;
                    actions.Add(newAction);
                    continue;
                }
            }

            direction = direction.Invert();
            if (trackGrid.TryMoveWithinGrid(position, direction, out newPosition))
            {
                Action newAction = trackGrid[newPosition];
                if (newAction is not Action.NONE)
                {
                    position = newPosition;
                    actions.Add(newAction);
                    continue;
                }
            }

            throw new InvalidOperationException("Dead end located");
        }
        while (position != Vector2<int>.Zero);

        return [..actions];
    }

    private static Action ParseAction(char value) => value switch
    {
        '-' => Action.DECREASE,
        '=' => Action.EQUAL,
        '+' => Action.INCREASE,
        'S' => Action.EQUAL,
        ' ' => Action.NONE,
        _   => throw new InvalidOperationException($"Unknown action {value}")
    };

    /// <inheritdoc />
    protected override Plan ConvertLine(string line)
    {
        int colonPosition = line.IndexOf(':');
        string name = line[..colonPosition];

        int actionsStart = colonPosition + 1;
        int actionCount = line.Count(',') + 1;
        Span<Range> splits = stackalloc Range[actionCount];
        line.AsSpan(actionsStart).Split(splits, ',');
        Action[] actions = splits.Select(r => line.AsSpan(actionsStart)[r][0])
                                 .Select(ParseAction)
                                 .ToArray();
        return new Plan(name, actions);
    }
}
