using System.Collections.Immutable;
using Challenge.Collections;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Strings;
using ZLinq;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 12
/// </summary>
[Solver(2024, 12)]
public sealed partial class Day12 : Solver<Day12.FieldData>, IEverybodyCodesSolver
{
    public readonly record struct Launcher(Vector2<int> Position, int Number);
    public readonly record struct Target(Vector2<int> Position, int RequiredHits);
    public readonly record struct FieldData(ImmutableArray<Launcher> Launchers, ImmutableArray<Target> Targets);

    private const char TARGET = 'T';
    private const char RUIN   = 'H';
    private static readonly Vector2<int> UpRight   = Vector2<int>.Down + Vector2<int>.Right;
    private static readonly Vector2<int> Right     = Vector2<int>.Right;
    private static readonly Vector2<int> DownRight = Vector2<int>.Up + Vector2<int>.Right;
    private static readonly Vector2<int> DownLeft  = Vector2<int>.Up + Vector2<int>.Left;

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        Dictionary<Vector2<int>, int> remainingTargets = this.Data.Targets.ToDictionary(t => t.Position, t => t.RequiredHits);
        int score = Enumerable.InfiniteSequence(1, 1)
                              .TakeWhile(_ => !remainingTargets.IsEmpty)
                              .Sum(p => this.Data.Launchers.Sum(l => SimulateLaunch(l.Position, p, remainingTargets) * l.Number * p));
        LogAnswer(score);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2() => RunPart1();

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        // Honestly cba to find the algebraic solution
        int score = this.Data.Targets.Sum(meteor => FindMeteorShot(meteor.Position));
        LogAnswer(score);
    }
    // ReSharper enable CognitiveComplexity

    private static int SimulateLaunch(Vector2<int> position, int power, Dictionary<Vector2<int>, int> remainingTargets)
    {
        int totalHits = 0;
        foreach (int _ in ..power)
        {
            position += UpRight;
            if (remainingTargets.Remove(position, out int hits))
            {
                totalHits += hits;
            }
        }

        foreach (int _ in ..power)
        {
            position += Right;
            if (remainingTargets.Remove(position, out int hits))
            {
                totalHits += hits;
            }
        }

        do
        {
            position += DownRight;
            if (remainingTargets.Remove(position, out int hits))
            {
                totalHits += hits;
            }
        }
        while (position.Y > 1);
        return totalHits;
    }

    private int FindMeteorShot(Vector2<int> meteor)
    {
        int minScore = int.MaxValue;
        do
        {
            foreach (Launcher launcher in this.Data.Launchers)
            {
                if (CanHitFromLauncher(launcher.Position, meteor, out int power))
                {
                    minScore = Math.Min(minScore, launcher.Number * power);
                }
            }

            meteor += DownLeft;
        }
        while (minScore is int.MaxValue);

        return minScore;
    }

    private static bool CanHitFromLauncher(Vector2<int> launcher, Vector2<int> meteor, out int power)
    {
        bool canHit = true;
        for (power = 1; canHit; power++)
        {
            if (HitsMeteor(launcher, meteor, power, out canHit)) return true;
        }

        power = 0;
        return false;
    }

    private static bool HitsMeteor(Vector2<int> position, Vector2<int> meteor, int power, out bool canHit)
    {
        canHit = true;
        foreach (int _ in ..power)
        {
            position += UpRight;
            meteor   += DownLeft;
            if (position == meteor) return true;

            if (position.X >= meteor.X)
            {
                canHit = false;
                return false;
            }
        }

        foreach (int _ in ..power)
        {
            position += Right;
            meteor   += DownLeft;
            if (position == meteor) return true;

            if (position.X >= meteor.X)
            {
                canHit = position.Y < meteor.Y;
                return false;
            }
        }

        do
        {
            position += DownRight;
            meteor   += DownLeft;
            if (position == meteor) return true;

            if (position.X >= meteor.X)
            {
                canHit = position.Y < meteor.Y;
                return false;
            }
        }
        while (position.Y > 1);

        return false;
    }

    /// <inheritdoc />
    protected override FieldData Convert(string[] rawInput)
    {
        if (rawInput[0][0] is '.')
        {
            int width = rawInput[0].Length;
            int height = rawInput.Length;
            int offset = height - 1;
            Grid<char> field = new(width, height, rawInput, l => l.ToCharArray());

            ImmutableArray<Launcher>.Builder launchers = ImmutableArray.CreateBuilder<Launcher>(100);
            ImmutableArray<Target>.Builder targets = ImmutableArray.CreateBuilder<Target>(100);
            foreach (Vector2<int> position in field.Dimensions.Enumerate())
            {
                char value = field[position];
                switch (value)
                {
                    case TARGET:
                        targets.Add(new Target(new Vector2<int>(position.X, offset - position.Y), 1));
                        break;

                    case RUIN:
                        targets.Add(new Target(new Vector2<int>(position.X, offset - position.Y), 2));
                        break;

                    case { IsUpperChar: true }:
                        launchers.Add(new Launcher(new Vector2<int>(position.X, offset - position.Y), value.AsIndex + 1));
                        break;
                }
            }

            return new FieldData(launchers.ToImmutable(), targets.ToImmutable());
        }
        else
        {
            ImmutableArray<Launcher> launchers =
            [
                new(new Vector2<int>(0, 0), 1),
                new(new Vector2<int>(0, 1), 2),
                new(new Vector2<int>(0, 2), 3)
            ];
            ImmutableArray<Target> targets = rawInput.Select(l => new Target(Vector2<int>.Parse(l, " "), 0))
                                                     .ToImmutableArray();
            return new FieldData(launchers, targets);
        }
    }
}
