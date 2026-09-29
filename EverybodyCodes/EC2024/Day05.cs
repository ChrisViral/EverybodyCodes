using System.Collections.Immutable;
using System.Text;
using Challenge.Collections;
using Challenge.Solvers;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Numbers;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Strings;
using ZLinq;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 5
/// </summary>
[Solver(2024, 5)]
public sealed partial class Day05 : Solver<ImmutableArray<LinkedList<int>>>, IEverybodyCodesSolver
{
    private const int PART1_ROUNDS = 10;
    private static readonly StringBuilder Builder = new(1000);

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int columnIndex = 0;
        foreach (int _ in ..PART1_ROUNDS)
        {
            ExecuteRound(ref columnIndex);
        }

        LogAnswer(GetRoundValue());
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        Counter<long> values = new();
        int columnIndex = 0;
        int rounds = 0;
        long lastRoundValue;
        do
        {
            rounds++;
            ExecuteRound(ref columnIndex);
            lastRoundValue = GetRoundValue();
        }
        while (values.Add(lastRoundValue) is not 2024);

        LogAnswer(rounds * lastRoundValue);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        HashSet<string> states = new(1000);
        HashSet<long> values = new(1000);

        int columnIndex = 0;
        do
        {
            ExecuteRound(ref columnIndex);
            values.Add(GetRoundValue());
        }
        while (states.Add(GetRoundState()));
        LogAnswer(values.Max());
    }
    // ReSharper enable CognitiveComplexity

    private void ExecuteRound(ref int columnIndex)
    {
        LinkedList<int> currentColumn = this.Data[columnIndex];
        columnIndex = (columnIndex + 1) % this.Data.Length;
        LinkedList<int> nextColumn = this.Data[columnIndex];

        LinkedListNode<int> clapper = currentColumn.First!;
        currentColumn.RemoveFirst();

        (int cycles, int position) = Math.DivRem(clapper.Value - 1, this.Data[columnIndex].Count);
        if (cycles.IsEven)
        {
            LinkedListNode<int> target = nextColumn.GetElementAt(position);
            nextColumn.AddBefore(target, clapper);
        }
        else
        {
            LinkedListNode<int> target = nextColumn.GetElementAt(^(position + 1));
            nextColumn.AddAfter(target, clapper);
        }
    }

    private long GetRoundValue()
    {
        foreach (LinkedList<int> column in this.Data)
        {
            Builder.Append(column.First!.Value);
        }

        return long.Parse(Builder.ToStringAndClear());
    }

    private string GetRoundState()
    {
        Builder.AppendJoin('.', this.Data[0]);
        foreach (int i in 1..this.Data.Length)
        {
            Builder.Append('-');
            Builder.AppendJoin('.', this.Data[i]);
        }
        return Builder.ToStringAndClear();
    }

    /// <inheritdoc />
    protected override ImmutableArray<LinkedList<int>> Convert(string[] rawInput)
    {
        int columnCount = rawInput[0].Count(' ') + 1;
        ImmutableArray<LinkedList<int>> columns = [..(..columnCount).Select(_ => new LinkedList<int>())];
        foreach (ReadOnlySpan<char> line in rawInput)
        {
            int column = 0;
            foreach (Range split in line.Split(' '))
            {
                columns[column++].AddLast(int.Parse(line[split]));
            }
        }
        return columns;
    }
}
