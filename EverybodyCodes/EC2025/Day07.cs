using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text;
using Challenge.Solvers;
using Challenge.Utils;
using ZLinq;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 7
/// </summary>
[Solver(2025, 7)]
public sealed partial class Day07 : Solver<(ImmutableArray<string> Names, FrozenDictionary<char, FrozenSet<char>> Rules)>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        string name = this.Data.Names.First(IsValidName);
        LogAnswer(name);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        int valid = this.Data.Names.Index()
                        .Where(p => IsValidName(p.Item))
                        .Sum(p => p.Index + 1);
        LogAnswer(valid);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int MIN_LENGTH = 7;
        const int MAX_LENGTH = 11;

        int nameCount = this.Data.Names
                            .Where(IsValidName)
                            .SelectMany(name => EnumerateValidNames(new StringBuilder(name)))
                            .Distinct()
                            .Count();
        LogAnswer(nameCount);
        return;

        IEnumerable<string> EnumerateValidNames(StringBuilder nameBuilder)
        {
            if (nameBuilder.Length >= MIN_LENGTH) yield return nameBuilder.ToString();
            if (!this.Data.Rules.TryGetValue(nameBuilder[^1], out FrozenSet<char>? rule)) yield break;

            if (nameBuilder.Length is MAX_LENGTH - 1)
            {
                foreach (char next in rule)
                {
                    nameBuilder.Append(next);
                    yield return nameBuilder.ToString();
                    nameBuilder.Length--;
                }
                yield break;
            }

            foreach (char next in rule)
            {
                nameBuilder.Append(next);
                foreach (string name in EnumerateValidNames(nameBuilder))
                {
                    yield return name;
                }
                nameBuilder.Length--;
            }
        }
    }
    // ReSharper enable CognitiveComplexity

    private bool IsValidName(string name)
    {
        FrozenSet<char> rule = this.Data.Rules[name[0]];
        foreach (char c in name.AsSpan()[1..^1])
        {
            if (!rule.Contains(c)) return false;

            rule = this.Data.Rules[c];
        }

        return rule.Contains(name[^1]);
    }

    /// <inheritdoc />
    protected override (ImmutableArray<string>, FrozenDictionary<char, FrozenSet<char>>) Convert(string[] rawInput)
    {
        ImmutableArray<string> names = ParseUtils.ParseImmutableArray(rawInput[0], ',');
        FrozenDictionary<char, FrozenSet<char>> rules = rawInput.AsSpan(1)
                                                                .AsValueEnumerable()
                                                                .ToFrozenDictionary(s => s[0],
                                                                                    s => ParseUtils.ParseCollection<HashSet<char>, char>(s.AsSpan(4), ',', v => v[0])
                                                                                                   .ToFrozenSet());
        return (names, rules);
    }
}
