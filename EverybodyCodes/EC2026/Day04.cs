using System.Text.RegularExpressions;
using Challenge.Collections;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils.Extensions.Ranges;
using ZLinq;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 4
/// </summary>
[Solver(2026, 4)]
public sealed partial class Day04 : RegexSolver<Day04.Result>, IEverybodyCodesSolver
{
    public readonly record struct Result(int ID, int Quest, int Part, int Time) : IComparable<Result>
    {
        /// <inheritdoc />
        public int CompareTo(Result other)
        {
            return this.Time.CompareTo(other.Time);
        }
    }

    private const int PART1_SCORE = 50;
    private const int PART2_SCORE = 100;
    private const int PART3_SCORE = 150;

    /// <inheritdoc />
    [GeneratedRegex(@"id=(\d+) quest=(\d+) part=(\d+) time=(\d+)")]
    protected override partial Regex Matcher { get; }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int checksum = this.Data
                           .GroupBy(r => r.ID)
                           .Select(g => g.Sum(r => r.Time))
                           .Order()
                           .Index()
                           .Sum(p => (p.Index + 1) * p.Item);
        LogAnswer(checksum);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        int checksum = this.Data
                           .GroupBy(r => r.ID)
                           .Select(g => (time: g.Sum(r => r.Time),
                                         parts: g.Count()))
                           .OrderByDescending(d => d.parts)
                           .ThenBy(d => d.time)
                           .Index()
                           .Sum(p => (p.Index + 1) * p.Item.time);
        LogAnswer(checksum);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        DefaultDictionary<int, List<Result>[]> quests = new(_ => [[], [], []]);
        foreach (Result result in this.Data)
        {
            quests[result.Quest][result.Part - 1].Add(result);
        }

        DefaultDictionary<int, int> participants = new(0);
        foreach (List<Result>[] quest in quests.Values)
        {
            if (quest is not [{ } part1, { } part2, { } part3]) continue;

            AddPartTimes(part2, part1);
            AddPartTimes(part3, part2);

            CalculateScores(participants, part1, PART1_SCORE);
            CalculateScores(participants, part2, PART2_SCORE);
            CalculateScores(participants, part3, PART3_SCORE);
        }

        int checksum = 0;
        int previousPosition = 0;
        int previousScore    = 0;
        foreach ((int index, int score) in participants.Values.OrderDescending().Index())
        {
            int position = score != previousScore ? index + 1 : previousPosition;
            checksum        += position * score;
            previousPosition = position;
            previousScore    = score;
        }

        LogAnswer(checksum);
    }
    // ReSharper enable CognitiveComplexity

    private static void AddPartTimes(List<Result> currentPart, List<Result> previousPart)
    {
        foreach (int i in ..currentPart.Count)
        {
            Result result = currentPart[i];
            int totalTime = result.Time + previousPart.Find(r => r.ID == result.ID).Time;
            currentPart[i] = result with { Time = totalTime };
        }
    }

    private static void CalculateScores(DefaultDictionary<int, int> participants, List<Result> part, int maxScore)
    {
        part.Sort();

        int previousScore = 0;
        int previousTime  = 0;
        foreach (int i in ..part.Count)
        {
            Result result = part[i];
            int score = result.Time != previousTime ? maxScore - i : previousScore;
            if (score <= 0) return;

            participants[result.ID] += score;
            previousScore = score;
            previousTime  = result.Time;
        }
    }
}
