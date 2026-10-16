using System.Collections.Frozen;
using Challenge.Collections;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Ranges;
using ZLinq;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 9
/// </summary>
[Solver(2025, 9)]
public sealed partial class Day09 : Solver<FrozenDictionary<int, string>>, IEverybodyCodesSolver
{
    private readonly record struct ParentalLink(int Child, UnorderedPair<int> Parents)
    {
        public bool IsValid(FrozenDictionary<int, string> data)
        {
            string childSequence = data[this.Child];
            string firstParentSequence  = data[this.Parents.First];
            string secondParentSequence = data[this.Parents.Second];
            foreach (int i in ..childSequence.Length)
            {
                char current = childSequence[i];
                if (firstParentSequence[i] != current && secondParentSequence[i] != current)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetSimilarity(FrozenDictionary<int, string> data)
        {
            string childSequence = data[this.Child];
            string firstParentSequence  = data[this.Parents.First];
            string secondParentSequence = data[this.Parents.Second];

            int firstSimilarity  = 0;
            int secondSimilarity = 0;
            foreach (int i in ..childSequence.Length)
            {
                char current = childSequence[i];
                if (firstParentSequence[i] == current)
                {
                    firstSimilarity++;
                }
                if (secondParentSequence[i] == current)
                {
                    secondSimilarity++;
                }
            }

            return firstSimilarity * secondSimilarity;
        }
    }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int[] keys = [..this.Data.Keys];
        ParentalLink parentalLink = keys.SelectMany(c => keys.EnumeratePairs()
                                                 .Where(p => !p.HasValue(c))
                                                 .Select(p => new ParentalLink(c, p)))
                            .First(l => l.IsValid(this.Data));
        int similarity = parentalLink.GetSimilarity(this.Data);
        LogAnswer(similarity);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        int[] keys = [..this.Data.Keys];
        int similarities = keys.SelectMany(c => keys.EnumeratePairs()
                                                    .Where(p => !p.HasValue(c))
                                                    .Select(p => new ParentalLink(c, p)))
                               .Where(l => l.IsValid(this.Data))
                               .Sum(l => l.GetSimilarity(this.Data));
        LogAnswer(similarities);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        int[] keys = [..this.Data.Keys];
        ParentalLink[] links = keys.SelectMany(c => keys.EnumeratePairs()
                                                        .Where(p => !p.HasValue(c))
                                                        .Select(p => new ParentalLink(c, p)))
                                   .Where(l => l.IsValid(this.Data))
                                   .ToArray();

        DefaultDictionary<int, HashSet<int>> families = new(k => [k]);
        foreach (ParentalLink link in links)
        {
            HashSet<int> childFamily = families[link.Child];
            HashSet<int> firstParentFamily  = families[link.Parents.First];
            HashSet<int> secondParentFamily = families[link.Parents.Second];
            MergeFamilies(ref childFamily, ref firstParentFamily);
            MergeFamilies(ref childFamily, ref secondParentFamily);
        }

        int scaleSum = families.Values.MaxBy(f => f.Count)!.Sum();
        LogAnswer(scaleSum);
        return;

        void MergeFamilies(ref HashSet<int> firstFamily, ref HashSet<int> secondFamily)
        {
            if (firstFamily == secondFamily) return;

            if (secondFamily.Count > firstFamily.Count)
            {
                SwapUtils.Swap(ref firstFamily, ref secondFamily);
            }

            firstFamily.UnionWith(secondFamily);
            foreach (int member in secondFamily)
            {
                families[member] = firstFamily;
            }
        }
    }
    // ReSharper enable CognitiveComplexity

    /// <inheritdoc />
    protected override FrozenDictionary<int, string> Convert(string[] rawInput)
    {
        KeyValuePair<int, string>[] data = new KeyValuePair<int, string>[rawInput.Length];
        foreach (int i in ..rawInput.Length)
        {
            string line = rawInput[i];
            int colonIndex = line.IndexOf(':');
            int key = int.Parse(line.AsSpan(0, colonIndex));
            string value = line[(colonIndex + 1)..];
            data[i] = new KeyValuePair<int, string>(key, value);
        }

        return FrozenDictionary.Create(data);
    }
}
