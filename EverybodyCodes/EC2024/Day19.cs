using System.Collections.Immutable;
using Challenge.Collections;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Enums;
using CommunityToolkit.HighPerformance;
using ZLinq;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 19
/// </summary>
[Solver(2024, 19)]
public sealed partial class Day19 : Solver<(ImmutableArray<Day19.Rotation> Rotations, Grid<char> Message)>, IEverybodyCodesSolver
{
    public enum Rotation
    {
        RIGHT = 'R',
        LEFT  = 'L'
    }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        PerformDecodePass(this.Data.Message);
        string message = ExtractMessage();
        LogAnswer(message);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const int PASSES = 100;
        PerformDecodePasses(PASSES);
        string message = ExtractMessage();
        LogAnswer(message);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int PASSES = 1048576000;
        PerformDecodePasses(PASSES);
        string message = ExtractMessage();
        LogAnswer(message);
    }
    // ReSharper enable CognitiveComplexity

    private void PerformDecodePass<T>(Grid<T> grid)
    {
        int rotationIndex = 0;
        foreach (Vector2<int> position in Vector2<int>.EnumerateOver(grid.Width - 2, grid.Height - 2))
        {
            Span2D<T> section = grid.AsSpan2D(position.X, 3, position.Y, 3);
            Rotation rotation = this.Data.Rotations[rotationIndex];
            rotationIndex = (rotationIndex + 1) % this.Data.Rotations.Length;
            switch (rotation)
            {
                case Rotation.RIGHT:
                    RotateRight(section);
                    break;

                case Rotation.LEFT:
                    RotateLeft(section);
                    break;

                default:
                    throw rotation.Invalid();
            }
        }
    }

    private void PerformDecodePasses(int passes)
    {
        // Create transition map
        Grid<Vector2<int>> transitions = new(this.Data.Message.Width, this.Data.Message.Height);
        foreach (Vector2<int> position in transitions.Dimensions.Enumerate())
        {
            transitions[position] = position;
        }
        PerformDecodePass(transitions);

        HashSet<Vector2<int>> inCycle = new(transitions.Size);
        List<List<Vector2<int>>> cycles = new(1000);
        foreach (Vector2<int> position in transitions.Dimensions.Enumerate())
        {
            if (!inCycle.Add(position)) continue;

            List<Vector2<int>> cycle = [position];
            Vector2<int> cyclePosition = transitions[position];
            while (inCycle.Add(cyclePosition))
            {
                cycle.Add(cyclePosition);
                cyclePosition = transitions[cyclePosition];
            }

            cycle.Reverse();
            cycles.Add(cycle);
        }

        Grid<char> temp = new(this.Data.Message.Width, this.Data.Message.Height);
        foreach (List<Vector2<int>> cycle in cycles)
        {
            foreach ((int i, Vector2<int> position) in cycle.Index())
            {
                Vector2<int> toPosition = cycle[(i + passes) % cycle.Count];
                temp[toPosition] = this.Data.Message[position];
            }
        }

        this.Data.Message.CopyFrom(temp);
    }

    private string ExtractMessage()
    {
        Span<char> result = stackalloc char[this.Data.Message.Width];
        int length = this.Data.Message
                         .SkipWhile(c => c is not '>')
                         .Skip(1)
                         .TakeWhile(c => c is not '<')
                         .CopyTo(result);
        return result[..length].ToString();
    }

    private static void RotateRight<T>(Span2D<T> section)
    {
        T temp = section[0, 0];
        SwapUtils.Swap(ref temp, ref section[0, 1]);
        SwapUtils.Swap(ref temp, ref section[0, 2]);
        SwapUtils.Swap(ref temp, ref section[1, 2]);
        SwapUtils.Swap(ref temp, ref section[2, 2]);
        SwapUtils.Swap(ref temp, ref section[2, 1]);
        SwapUtils.Swap(ref temp, ref section[2, 0]);
        SwapUtils.Swap(ref temp, ref section[1, 0]);
        section[0, 0] = temp;
    }

    private static void RotateLeft<T>(Span2D<T> section)
    {
        T temp = section[0, 0];
        SwapUtils.Swap(ref temp, ref section[1, 0]);
        SwapUtils.Swap(ref temp, ref section[2, 0]);
        SwapUtils.Swap(ref temp, ref section[2, 1]);
        SwapUtils.Swap(ref temp, ref section[2, 2]);
        SwapUtils.Swap(ref temp, ref section[1, 2]);
        SwapUtils.Swap(ref temp, ref section[0, 2]);
        SwapUtils.Swap(ref temp, ref section[0, 1]);
        section[0, 0] = temp;
    }

    /// <inheritdoc />
    protected override (ImmutableArray<Rotation>, Grid<char>) Convert(string[] rawInput)
    {
        ImmutableArray<Rotation> rotations = rawInput[0].Select(c => (Rotation)c).ToImmutableArray();
        int width  = rawInput[1].Length;
        int height = rawInput.Length - 1;
        Grid<char> message = new(width, height, rawInput[1..], l => l.ToCharArray());
        return (rotations, message);
    }
}
