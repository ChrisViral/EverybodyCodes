using System.Text.RegularExpressions;
using Challenge.Collections;
using Challenge.Collections.Pooling;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Spans;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 2
/// </summary>
/// ReSharper disable CognitiveComplexity
[Solver(2024, 2)]
public sealed partial class Day02 : Solver<(string[] words, string[] inscriptions)>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int count = this.Data.words.AsSpan().Sum(w => this.Data.inscriptions[0].AsSpan().Count(w));
        LogAnswer(count);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        int count = 0;
        HashSet<int> symbols = new(100);
        foreach (ReadOnlySpan<char> inscription in this.Data.inscriptions)
        {
            inscription.Reversed(out ReadOnlySpan<char> reversedInscription);
            foreach (string word in this.Data.words)
            {
                foreach (ValueMatch match in Regex.EnumerateMatches(inscription, word))
                {
                    for (int i = 0; i < match.Length; i++)
                    {
                        symbols.Add(match.Index + i);
                    }
                }

                foreach (ValueMatch match in Regex.EnumerateMatches(reversedInscription, word))
                {
                    int offset = reversedInscription.Length - match.Index - 1;
                    for (int i = 0; i < match.Length; i++)
                    {
                        symbols.Add(offset - i);
                    }
                }
            }

            count += symbols.Count;
            symbols.Clear();
        }
        LogAnswer(count);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        int width = this.Data.inscriptions[0].Length;
        int height = this.Data.inscriptions.Length;
        Grid<char> armour = new(width, height, this.Data.inscriptions, s => s.ToCharArray());

        HashSet<Vector2<int>> scales = new(armour.Size);
        foreach (int row in ..height)
        {
            MarkSymbolsFromPosition((0, row), Direction.RIGHT, armour, scales);
        }

        foreach (int column in ..width)
        {
            MarkSymbolsFromPosition((column, 0), Direction.DOWN, armour, scales);
        }
        LogAnswer(scales.Count);
    }
    // ReSharper enable CognitiveComplexity

    private void MarkSymbolsFromPosition(Vector2<int> position, Direction direction, Grid<char> armour, HashSet<Vector2<int>> scales)
    {
        using Pooled<List<Vector2<int>>> matches = ListObjectPool<Vector2<int>>.Shared.Get();
        do
        {
            foreach (string word in this.Data.words)
            {
                if (TryMatchWord(position, word, armour, direction, matches.Ref))
                {
                    scales.AddRange(matches.Ref);
                    matches.Ref.Clear();
                }
                if (TryMatchWord(position, word, armour, direction.Invert(), matches.Ref))
                {
                    scales.AddRange(matches.Ref);
                    matches.Ref.Clear();
                }
            }
        }
        while (armour.TryMoveWithinGrid(position, direction, out position));
    }

    private static bool TryMatchWord(Vector2<int> fromPosition, ReadOnlySpan<char> word, Grid<char> armour, Direction direction, List<Vector2<int>> matches)
    {
        Vector2<int>? position = fromPosition;
        foreach (char c in word)
        {
            if (!position.HasValue || armour[position.Value] != c)
            {
                matches.Clear();
                return false;
            }

            matches.Add(position.Value);
            position = armour.MoveWithinGrid(position.Value, direction, Wrap.HORIZONTAL);
        }

        return true;
    }

    /// <inheritdoc />
    protected override (string[], string[]) Convert(string[] rawInput)
    {
        string[] words = rawInput[0]["WORDS:".Length..].Split(',', DEFAULT_OPTIONS);
        return (words, rawInput[1..]);
    }
}
