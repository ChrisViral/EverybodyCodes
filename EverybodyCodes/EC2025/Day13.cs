using Challenge.Collections;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils.Extensions.Ranges;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 13
/// </summary>
[Solver(2025, 13)]
public sealed partial class Day13 : ArraySolver<Day13.DialValue>, IEverybodyCodesSolver
{
    public readonly struct DialValue
    {
        public int Start { get; }

        public int? End { get; }

        public int Size => this.End is not null ? this.End.Value - this.Start + 1 : 1;

        public DialValue(ReadOnlySpan<char> data)
        {
            int dashIndex = data.IndexOf('-');
            if (dashIndex != -1)
            {
                this.Start = int.Parse(data[..dashIndex]);
                this.End   = int.Parse(data[(dashIndex + 1)..]);
            }
            else
            {
                this.Start = int.Parse(data);
            }
        }

        public void AddToDial(RingBuffer<int> dial, bool back)
        {
            if (this.End is null)
            {
                if (back)
                {
                    dial.Push(this.Start);
                }
                else
                {
                    dial.PushFront(this.Start);
                }
            }
            else
            {
                Func<int, int> addFunction = back ? dial.Push : dial.PushFront;
                foreach (int value in this.Start..^this.End.Value)
                {
                    addFunction(value);
                }
            }
        }
    }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const int TURNS = 2025;
        int finalValue = GetFinalDialValue(TURNS);
        LogAnswer(finalValue);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const int TURNS = 20252025;
        int finalValue = GetFinalDialValue(TURNS);
        LogAnswer(finalValue);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const long TURNS = 202520252025L;
        int finalValue = GetFinalDialValue(TURNS);
        LogAnswer(finalValue);
    }
    // ReSharper enable CognitiveComplexity

    private int GetFinalDialValue(long turns)
    {
        RingBuffer<int> dial = new(this.Data.Sum(v => v.Size) + 1);
        dial.Push(1);

        bool back = true;
        foreach (DialValue value in this.Data)
        {
            value.AddToDial(dial, back);
            back = !back;
        }

        int pointer = dial.IndexOf(1);
        pointer = (int)((pointer + turns) % dial.Count);
        return dial[pointer];
    }

    /// <inheritdoc />
    protected override DialValue ConvertLine(string line) => new(line);
}
