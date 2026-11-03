using System.Collections.Frozen;
using Challenge.Collections;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Collections;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 6
/// </summary>
[Solver(2026, 6)]
public sealed partial class Day06 : Solver<FrozenDictionary<string, FrozenSet<string>>>, IEverybodyCodesSolver
{
    private const string NULL = "NULL";

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        List<string> required = [NULL];
        for (int i = 0; i < required.Count; i++)
        {
            string current = required[i];
            if (!this.Data.TryGetValue(current, out FrozenSet<string>? recipe)) continue;

            foreach (string ingredient in recipe)
            {
                if (!required.Contains(ingredient))
                {
                    required.Add(ingredient);
                }
            }
        }

        LogAnswer(required.Count - 1);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        List<string> madeWithNull = GetMadeWithNull(NULL);
        LogAnswer(madeWithNull.Count);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        string[] nullTypes =
        [
            ..this.Data.Values
                  .SelectMany(i => i)
                  .Distinct()
                  .Where(n => n.StartsWith(NULL))
        ];

        Counter<string> nullCounts = [];
        nullCounts.AddRange(nullTypes.AsEnumerable().SelectMany(GetMadeWithNull));
        int badIngredients = nullCounts.Counts.Count(c => c >= 2);
        LogAnswer(badIngredients);
    }
    // ReSharper enable CognitiveComplexity

    private List<string> GetMadeWithNull(string nullType)
    {
        List<string> madeWithNull = new(1000) { nullType };
        for (int i = 0; i < madeWithNull.Count; i++)
        {
            string current = madeWithNull[i];
            foreach ((string ingredient, _) in this.Data.Where(p => p.Value.Contains(current)))
            {
                if (!madeWithNull.Contains(ingredient))
                {
                    madeWithNull.Add(ingredient);
                }
            }
        }

        madeWithNull.RemoveSwap(0);
        return madeWithNull;
    }

    /// <inheritdoc />
    protected override FrozenDictionary<string, FrozenSet<string>> Convert(string[] rawInput)
    {
        Dictionary<string, FrozenSet<string>> ingredients = new(rawInput.Length);
        foreach (ReadOnlySpan<char> line in rawInput)
        {
            string name = line[..4].ToString();
            string[] recipe = ParseUtils.ParseArray(line[5..], ',');
            ingredients.Add(name, recipe.ToFrozenSet());
        }

        return ingredients.ToFrozenDictionary();
    }
}
