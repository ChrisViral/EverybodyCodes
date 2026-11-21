using System.Collections.Frozen;
using System.Collections.Immutable;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils.Extensions.Enums;
using Challenge.Utils.Extensions.Numbers;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Strings;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 20
/// </summary>
[Solver(2026, 20)]
public sealed partial class Day20 : ArraySolver<char[]>, IEverybodyCodesSolver
{
    private enum Operation
    {
        ADD      = '+',
        SUBTRACT = '-',
        EQUAL    = '='
    }

    private static readonly FrozenDictionary<char, int> SegmentCounts = new Dictionary<char, int>
    {
        [' '] = 0,
        ['0'] = 6,
        ['1'] = 2,
        ['2'] = 5,
        ['3'] = 5,
        ['4'] = 4,
        ['5'] = 5,
        ['6'] = 6,
        ['7'] = 3,
        ['8'] = 7,
        ['9'] = 6,
        ['A'] = 6,
        ['+'] = 2,
        ['-'] = 1,
        ['='] = 2,
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<char, ImmutableArray<char>> SelfSwaps = new Dictionary<char, ImmutableArray<char>>
    {
        ['0'] = ['6', '9', 'A'],
        ['2'] = ['3'],
        ['3'] = ['2', '5'],
        ['5'] = ['3'],
        ['6'] = ['0', '9', 'A'],
        ['9'] = ['0', '6', 'A'],
        ['A'] = ['6', '9', '0'],
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<char, ImmutableArray<char>> SegmentDonors = new Dictionary<char, ImmutableArray<char>>
    {
        ['6'] = ['5'],
        ['7'] = ['1'],
        ['8'] = ['0', '6', '9', 'A'],
        ['9'] = ['3', '5'],
        ['+'] = ['-'],
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<char, ImmutableArray<char>> SegmentReceivers = new Dictionary<char, ImmutableArray<char>>
    {
        ['0'] = ['8'],
        ['1'] = ['7'],
        ['3'] = ['9'],
        ['5'] = ['6', '9'],
        ['6'] = ['8'],
        ['9'] = ['8'],
        ['A'] = ['8'],
        ['-'] = ['+'],
    }.ToFrozenDictionary();

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int segments = CountSegmentsTotal(s => int.Parse(s));
        LogAnswer(segments);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        int segments = CountSegmentsTotal(ParseBase11);
        LogAnswer(ToBase11(segments));
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        int segments = 0;
        foreach (int i in ..this.Data.Length)
        {
            foreach (char[] equation in EnumerateVariations(this.Data[i]))
            {
                if (CheckEquationTrue(equation, ParseBase11))
                {
                    segments += CountEquationSegments(this.OriginalData[i]);
                    break;
                }
            }
        }

        LogAnswer(ToBase11(segments));
    }
    // ReSharper enable CognitiveComplexity

    private int CountSegmentsTotal(Converter<ReadOnlySpan<char>, int> parser)
    {
        int segments = 0;
        foreach (int i in ..this.Data.Length)
        {
            if (CheckEquationTrue(this.Data[i], parser))
            {
                segments += CountEquationSegments(this.Data[i]);
            }
        }

        return segments;
    }

    private static int CountEquationSegments(ReadOnlySpan<char> equation) => equation.Sum(c => SegmentCounts[c]);

    private static bool CheckEquationTrue(ReadOnlySpan<char> equation, Converter<ReadOnlySpan<char>, int> parser)
    {
        int partsCount = equation.Count(' ') + 1;
        Span<Range> splits = stackalloc Range[partsCount];
        partsCount = equation.Split(splits, ' ', DEFAULT_OPTIONS);

        int first = 0;
        int index = 0;
        Operation operation = Operation.ADD;
        do
        {
            int operand = parser(equation[splits[index++]]);
            first       = ExecuteOperation(first, operand, operation);
            operation   = (Operation)equation[splits[index++]][0];
        }
        while (operation is not Operation.EQUAL);

        int second = parser(equation[splits[index++]]);
        while (index < partsCount)
        {
            operation   = (Operation)equation[splits[index++]][0];
            int operand = parser(equation[splits[index++]]);
            second      = ExecuteOperation(second, operand, operation);
        }

        return first == second;
    }

    private static int ExecuteOperation(int left, int right, Operation operation) => operation switch
    {
        Operation.ADD      => left + right,
        Operation.SUBTRACT => left - right,
        Operation.EQUAL    => left == right ? 1 : 0,
        _                  => throw operation.Invalid()
    };

    private static int ParseBase11(ReadOnlySpan<char> value)
    {
        int result = 0;
        int multiplier = 1;
        foreach (int i in 1..^value.Length)
        {
            char c      = value[^i];
            int digit   = c is 'A' ? 10 : c.AsIndex;
            result     += digit * multiplier;
            multiplier *= 11;
        }

        return result;
    }

    private static string ToBase11(int value)
    {
        int length = 0;
        Span<char> result = stackalloc char[value.DigitCount];
        while (value > 0)
        {
            (value, int remainder) = Math.DivRem(value, 11);
            result[^++length] = remainder == 10 ? 'A' : remainder.AsAsciiDigit;
        }

        return result[..length].ToString();
    }

    private static IEnumerable<char[]> EnumerateVariations(char[] equation)
    {
        yield return equation;

        foreach (char[] swapped in EnumerateSelfSwaps(equation))
        {
            yield return swapped;
        }

        foreach ((char[] oneRemoved, int donorIndex) in EnumerateRemovedSegments(equation))
        {
            foreach (char[] swapped in EnumerateAddedSegments(oneRemoved, donorIndex))
            {
                yield return swapped;
            }
        }
    }

    private static IEnumerable<char[]> EnumerateSelfSwaps(char[] equation)
    {
        for (int i = 0; i < equation.Length; i++)
        {
            char element = equation[i];
            if (!SelfSwaps.TryGetValue(element, out ImmutableArray<char> results)) continue;

            foreach (char result in results)
            {
                equation[i] = result;
                yield return equation;
            }

            equation[i] = element;
        }
    }

    private static IEnumerable<(char[] result, int donorIndex)> EnumerateRemovedSegments(char[] equation)
    {
        for (int i = 0; i < equation.Length; i++)
        {
            char donor = equation[i];
            if (!SegmentDonors.TryGetValue(donor, out ImmutableArray<char> results)) continue;

            foreach (char result in results)
            {
                equation[i] = result;
                yield return (equation, i);
            }

            equation[i] = donor;
        }
    }

    private static IEnumerable<char[]> EnumerateAddedSegments(char[] equation, int donorIndex)
    {
        for (int i = 0; i < equation.Length; i++)
        {
            if (i == donorIndex) continue;

            char receiver = equation[i];
            if (!SegmentReceivers.TryGetValue(receiver, out ImmutableArray<char> results)) continue;

            foreach (char result in results)
            {
                equation[i] = result;
                yield return equation;
            }

            equation[i] = receiver;
        }
    }

    /// <inheritdoc />
    protected override char[] ConvertLine(string line) => line.ToCharArray();
}
