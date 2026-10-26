using System.Diagnostics;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Enumerables;
using Challenge.Utils.Extensions.Ranges;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 16
/// </summary>
[Solver(2025, 16)]
public sealed partial class Day16 : Solver<int[]>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const int LENGTH = 90;
        int blocks = this.Data.Sum(v => LENGTH / v);
        LogAnswer(blocks);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        List<int> spell = GetSpell();
        long spellValue = spell.Multiply(v => (long)v);
        LogAnswer(spellValue);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const long BLOCKS = 202520252025000L;
        List<int> spell = GetSpell();

        long minLength = 1L;
        long maxLength = BLOCKS;

        while (maxLength - minLength > 1)
        {
            long length = minLength + ((maxLength - minLength) / 2);
            long blocksUsed = spell.Sum(v => length / v);
            if (blocksUsed == BLOCKS)
            {
                minLength = maxLength = length;
            }
            else if (blocksUsed > BLOCKS)
            {
                maxLength = length - 1;
            }
            else
            {
                minLength = length;
            }
        }

        long finalLength = spell.Sum(v => maxLength / v) <= BLOCKS ? maxLength : minLength;
        LogAnswer(finalLength);
    }
    // ReSharper enable CognitiveComplexity

    private List<int> GetSpell()
    {
        List<int> spell = new(this.Data.Length);
        foreach (int i in ..this.Data.Length)
        {
            int blocks = this.Data[i];
            if (blocks is 0) continue;

            Debug.Assert(blocks is 1);
            int spellValue = i + 1;
            spell.Add(spellValue);

            for (int j = i + spellValue; j < this.Data.Length; j += spellValue)
            {
                this.Data[j]--;
            }
        }

        return spell;
    }

    /// <inheritdoc />
    protected override int[] Convert(string[] rawInput) => ParseUtils.ParseArray(rawInput[0], ',', s => int.Parse(s));
}
