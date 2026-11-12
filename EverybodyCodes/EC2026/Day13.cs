using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Enumerables;
using Challenge.Utils.Extensions.Numbers;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Strings;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 13
/// </summary>
[Solver(2026, 13)]
public sealed partial class Day13 : Solver<ImmutableArray<ImmutableArray<Day13.Clock>>>, IEverybodyCodesSolver
{
    public enum ClockDirection
    {
        COUNTER_CLOCKWISE = -1,
        NONE = 0,
        CLOCKWISE = 1,
    }

    public sealed record Clock(int TotalPositions, int MoveDistance, ClockDirection MoveDirection, int Position, long TimeRemaining)
    {
        public int Position { get; private set; } = Position;

        public ClockDirection MoveDirection { get; private set; } = TimeRemaining is not 0 ? MoveDirection : ClockDirection.NONE;

        public long TimeRemaining
        {
            get;
            private set
            {
                field = value;
                if (value is 0)
                {
                    this.MoveDirection = ClockDirection.NONE;
                }
            }
        } = TimeRemaining;

        public bool IsRunning => this.TimeRemaining is not 0L;

        public void Move()
        {
            if (!this.IsRunning) return;

            this.Position = (this.Position + (this.MoveDistance * (int)this.MoveDirection)).Mod(this.TotalPositions);
            this.TimeRemaining--;
        }

        public void SkipTime(long timeToSkip) => this.TimeRemaining -= timeToSkip;

        public string State() => $"{this.Position}:{(int)this.MoveDirection}";

        public static bool CheckCrossing(Clock first, Clock second)
        {
            switch ((first.MoveDirection, second.MoveDirection))
            {
                case (ClockDirection.CLOCKWISE, ClockDirection.CLOCKWISE):
                {
                    if (first.MoveDistance == second.MoveDistance) return false;

                    (Clock faster, Clock slower) = first.MoveDistance > second.MoveDistance ? (first, second) : (second, first);
                    return faster.Position < slower.Position
                               ? faster.Position + faster.MoveDistance >= slower.Position + slower.MoveDistance
                               : faster.Position + faster.MoveDistance >= slower.Position + slower.TotalPositions + slower.MoveDistance;
                }

                case (ClockDirection.COUNTER_CLOCKWISE, ClockDirection.COUNTER_CLOCKWISE):
                {
                    if (first.MoveDistance == second.MoveDistance) return false;

                    (Clock faster, Clock slower) = first.MoveDistance > second.MoveDistance ? (first, second) : (second, first);
                    return faster.Position > slower.Position
                               ? faster.Position - faster.MoveDistance <= slower.Position - slower.MoveDistance
                               : faster.Position - faster.MoveDistance <= slower.Position - slower.TotalPositions - slower.MoveDistance;
                }


                case (ClockDirection.CLOCKWISE, ClockDirection.NONE):
                case (ClockDirection.NONE, ClockDirection.CLOCKWISE):
                {
                    (Clock moving, Clock stationary) = first.MoveDirection is not ClockDirection.NONE ? (first, second) : (second, first);
                    return moving.Position < stationary.Position
                               ? moving.Position + moving.MoveDistance >= stationary.Position
                               : moving.Position + moving.MoveDistance >= stationary.Position + stationary.TotalPositions;
                }

                case (ClockDirection.COUNTER_CLOCKWISE, ClockDirection.NONE):
                case (ClockDirection.NONE, ClockDirection.COUNTER_CLOCKWISE):
                {
                    (Clock moving, Clock stationary) = first.MoveDirection is not ClockDirection.NONE ? (first, second) : (second, first);
                    return moving.Position > stationary.Position
                               ? moving.Position - moving.MoveDistance <= stationary.Position
                               : moving.Position - moving.MoveDistance <= stationary.Position - stationary.TotalPositions;
                }

                case (ClockDirection.CLOCKWISE, ClockDirection.COUNTER_CLOCKWISE):
                case (ClockDirection.COUNTER_CLOCKWISE, ClockDirection.CLOCKWISE):
                    (Clock forwards, Clock backwards) = first.MoveDirection is ClockDirection.CLOCKWISE ? (first, second) : (second, first);
                    return forwards.Position < backwards.Position
                               ? forwards.Position + forwards.MoveDistance >= backwards.Position - backwards.MoveDistance
                               : forwards.Position + forwards.MoveDistance >= backwards.Position + backwards.TotalPositions - backwards.MoveDistance;

                case (ClockDirection.NONE, ClockDirection.NONE):
                    return false;

                default:
                    throw new UnreachableException("Invalid clock arrangement");
            }
        }
    }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        long calibration = GetPortalCalibrationValue(this.Data[0]);
        LogAnswer(calibration);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        long calibration = this.Data.Sum(GetPortalCalibrationValue);
        LogAnswer(calibration);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3() => RunPart2();
    // ReSharper enable CognitiveComplexity

    private static long GetPortalCalibrationValue(ImmutableArray<Clock> portal)
    {
        long calibration = 0L;
        Dictionary<string, (long seen, long crossings)> cache = new(1000);

        StringBuilder stateBuilder = new();
        stateBuilder.AppendJoin("\n", portal.AsEnumerable().Select(c => c.State()));
        string state = stateBuilder.ToStringAndClear();

        long round = 0L;
        while (portal.Any(c => c.IsRunning))
        {
            if (cache.TryGetValue(state, out (long seen, long crossings) lastState))
            {
                long cycleLength = round - lastState.seen;
                long cycleCrossings = calibration - lastState.crossings;

                long smallestRemaining = portal.Where(c => c.IsRunning).Min(c => c.TimeRemaining);
                long cycles = smallestRemaining / cycleLength;

                cache.Clear();
                if (cycles is not 0L)
                {
                    long skipTime = cycles * cycleLength;
                    long skipCrossings = cycles * cycleCrossings;

                    portal.Where(c => c.IsRunning).ForEach(c => c.SkipTime(skipTime));
                    round       += skipTime;
                    calibration += skipCrossings;
                    continue;
                }
            }

            cache[state] = (round++, calibration);
            calibration += portal.EnumeratePairs().LongCount(pair => Clock.CheckCrossing(pair.First, pair.Second));

            portal.ForEach(c => c.Move());

            stateBuilder.AppendJoin("\n", portal.AsEnumerable().Select(c => c.State()));
            state = stateBuilder.ToStringAndClear();
        }

        return calibration;
    }

    /// <inheritdoc />
    protected override ImmutableArray<ImmutableArray<Clock>> Convert(string[] rawInput)
    {
        int portalCount = rawInput.Length / 4;
        ImmutableArray<ImmutableArray<Clock>>.Builder portals = ImmutableArray.CreateBuilder<ImmutableArray<Clock>>(portalCount);

        for (int i = 0; i < rawInput.Length; /* i += 4 */)
        {
            int[] hands = ParseUtils.ParseArray(rawInput[i++].AsSpan(6), ':', v => int.Parse(v));
            ClockDirection[] direction = ParseUtils.ParseArray(rawInput[i++].AsSpan(10), ':', v => v is "CW" ? ClockDirection.CLOCKWISE : ClockDirection.COUNTER_CLOCKWISE);
            int[] current = ParseUtils.ParseArray(rawInput[i++].AsSpan(8), ':', v => int.Parse(v));
            long[] move = ParseUtils.ParseArray(rawInput[i++].AsSpan(5), ':', v => long.Parse(v));

            int totalPositions = int.LCM(hands);
            ImmutableArray<Clock>.Builder clocks = ImmutableArray.CreateBuilder<Clock>(hands.Length);
            foreach (int j in ..hands.Length)
            {
                int moves = totalPositions / hands[j];
                Clock clock = new(totalPositions, moves, direction[j], current[j] * moves, move[j]);
                clocks.Add(clock);
            }

            portals.Add(clocks.ToImmutable());
        }

        return portals.ToImmutable();
    }
}
