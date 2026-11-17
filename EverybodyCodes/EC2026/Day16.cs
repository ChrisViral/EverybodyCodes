using System.Text;
using System.Text.RegularExpressions;
using Challenge.Solvers;
using Challenge.Utils.Extensions.Ranges;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 16
/// </summary>
[Solver(2026, 16)]
public sealed partial class Day16 : Solver<string>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        Span<char> shelf = stackalloc char[this.Data.Length];
        this.Data.CopyTo(shelf);

        int flips = 0;
        while (!shelf.IsEmpty)
        {
            (int groupFlips, _) = GetFlipsCountForStartingGroup(ref shelf);
            flips += groupFlips;
        }

        LogAnswer(flips);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const int REPEATS = 10;

        int length = this.Data.Length;
        Span<char> shelf = stackalloc char[length * REPEATS];
        foreach (int i in ..REPEATS)
        {
            this.Data.CopyTo(shelf.Slice(i * length, length));
        }

        int flipsLength = 0;
        while (!shelf.IsEmpty)
        {
            (_, int groupFlipsLength) = GetFlipsCountForStartingGroup(ref shelf);
            flipsLength += groupFlipsLength;
        }

        LogAnswer(flipsLength);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int REPEATS = 100;

        long flipsLength = GetFlipsLengthForRepeats(REPEATS);
        LogAnswer(flipsLength);
    }
    // ReSharper enable CognitiveComplexity

    private long GetFlipsLengthForRepeats(int repeats)
    {
        int dataLength = this.Data.Length;
        Span<char> shelf = new char[dataLength * repeats];
        foreach (int i in ..repeats)
        {
            this.Data.CopyTo(shelf.Slice(i * dataLength, dataLength));
        }

        long flipsLength = 0L;
        while (!shelf.IsEmpty)
        {
            flipsLength += GetFlipsLengthForStartingGroup(ref shelf);
        }

        return flipsLength;
    }

    private static long GetFlipsLengthForStartingGroup(ref Span<char> shelf)
    {
        char current = shelf[0];
        StringBuilder remainder = new(shelf.Length);

        int start = shelf.IndexOfAnyExcept(current);
        if (start is -1)
        {
            shelf = Span<char>.Empty;
            return 0;
        }

        int end = shelf.Length - 1;

        bool fromBack = true;
        long flipsLength = 0L;
        while (start < end)
        {
            if (fromBack)
            {
                while (shelf[end] != current)
                {
                    remainder.Append(shelf[end--]);
                }

                flipsLength += end - start + 1;

                while (shelf[end] == current)
                {
                    end--;
                }
            }
            else
            {
                while (shelf[start] != current)
                {
                    remainder.Append(shelf[start++]);
                }

                flipsLength += end - start + 1;

                while (shelf[start] == current)
                {
                    start++;
                }
            }

            fromBack = !fromBack;
        }

        shelf = shelf[..remainder.Length];
        remainder.CopyTo(0, shelf, shelf.Length);
        shelf.Reverse();
        return flipsLength;
    }

    private static (int Flips, int FlipsLength) GetFlipsCountForStartingGroup(ref Span<char> shelf)
    {
        int flips = 0;
        int flipsLength = 0;
        string currentGroupPattern = $"{shelf[0]}+";
        Regex currentGroupRegex = new(currentGroupPattern);

        while(true)
        {
            Regex.ValueMatchEnumerator enumerator = currentGroupRegex.EnumerateMatches(shelf);
            enumerator.MoveNext();

            ValueMatch firstGroup = enumerator.Current;

            int startIndex = firstGroup.Length;
            if (!enumerator.MoveNext())
            {
                shelf = shelf[startIndex..];
                return (flips, flipsLength);
            }

            ValueMatch lastGroup = enumerator.Current;
            foreach (ValueMatch match in enumerator)
            {
                lastGroup = match;
            }


            int endIndex = lastGroup.Index + lastGroup.Length;
            shelf[startIndex..endIndex].Reverse();
            shelf = shelf[startIndex..];

            flips++;
            flipsLength += endIndex - startIndex;
        }
    }

    /// <inheritdoc />
    protected override string Convert(string[] rawInput) => rawInput[0];
}
