using System.Text;
using Challenge.Collections;
using Challenge.Solvers;
using Challenge.Utils.Extensions.Arrays;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Strings;
using ZLinq;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 6
/// </summary>
[Solver(2025, 6)]
public sealed partial class Day06 : Solver<string>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const char TYPE = 'A';

        DefaultDictionary<char, int[]> populationMap = GetPopulationMap(this.Data);
        int[] novices = CalculateNovices(populationMap[TYPE]);
        int count = this.Data.Index().Where(p => p.Item is TYPE).Sum(p => novices[p.Index]);
        LogAnswer(count);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        DefaultDictionary<char, int[]> populationMap = GetPopulationMap(this.Data);
        Dictionary<char, int[]> novicesMap = populationMap.ToDictionary(p => p.Key, p => CalculateNovices(p.Value));
        int count = this.Data.Index().Where(p => p.Item.IsUpperChar).Sum(p => novicesMap[p.Item][p.Index]);
        LogAnswer(count);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int REPEATS      = 1000;
        const int MAX_DISTANCE = 1000;

        StringBuilder dataBuilder = new(this.Data.Length * REPEATS);
        dataBuilder.AppendJoin(string.Empty, Enumerable.Repeat(this.Data, REPEATS));
        string data = dataBuilder.ToString();

        DefaultDictionary<char, int[]> populationMap = GetPopulationMap(data);
        Dictionary<char, int[]> frontNovices = populationMap.ToDictionary(p => p.Key, p => CalculateNovices(p.Value, maxDistance: MAX_DISTANCE, reverse: false));
        Dictionary<char, int[]> backNovices  = populationMap.ToDictionary(p => p.Key, p => CalculateNovices(p.Value, maxDistance: MAX_DISTANCE, reverse: true));
        int count = data.Index().Where(p => p.Item.IsUpperChar).Sum(p => frontNovices[p.Item][p.Index] + backNovices[p.Item][p.Index]);
        LogAnswer(count);
    }
    // ReSharper enable CognitiveComplexity

    private static DefaultDictionary<char, int[]> GetPopulationMap(string data)
    {
        DefaultDictionary<char, int[]> frequenciesMap = new(() => new int[data.Length]);
        foreach (int i in ..data.Length)
        {
            char value = data[i];
            if (!value.IsLowerChar) continue;

            int[] frequencies = frequenciesMap[value.ToUpperChar];
            frequencies[i] = 1;
        }

        return frequenciesMap;
    }

    private static int[] CalculateNovices(int[] population, int? maxDistance = null, bool reverse = false)
    {
        int currentFrequency = 0;
        int length = population.Length;
        int[] novices = new int[length];
        int end = maxDistance.HasValue ? Math.Min(length, maxDistance.Value + 1) : length;

        if (reverse)
        {
            population.ReverseInPlace();
        }

        foreach (int i in 1..^end)
        {
            currentFrequency += population[^i];
            novices[^i]     = currentFrequency;
        }

        if (end != length)
        {
            foreach (int i in (end + 1)..^length)
            {
                currentFrequency += population[^i] - population[^(i - end)];
                novices[^i]    =  currentFrequency;
            }
        }

        if (reverse)
        {
            population.ReverseInPlace();
            novices.ReverseInPlace();
        }

        return novices;
    }

    /// <inheritdoc />
    protected override string Convert(string[] rawInput) => rawInput[0];
}
