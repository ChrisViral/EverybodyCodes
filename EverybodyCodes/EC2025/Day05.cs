using System.Collections.Immutable;
using Challenge.Collections.Search;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils;
using Challenge.Utils.Extensions.Enumerables;
using Challenge.Utils.Extensions.Numbers;
using Challenge.Utils.Extensions.Ranges;
using CommunityToolkit.HighPerformance;
using ZLinq;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 5
/// </summary>
[Solver(2025, 5)]
public sealed partial class Day05 : ArraySolver<Day05.Sword>, IEverybodyCodesSolver
{
    public sealed class Sword(int id, ImmutableArray<int> values) : IComparable<Sword>
    {
        public int ID { get; } = id;

        public ImmutableArray<int> Values { get; } = values;

        public List<Bone> Fishbone { get; } = new(values.Length);

        private long? quality;
        public long Quality => this.quality ??= CalculateQuality();

        private long CalculateQuality()
        {
            this.Values.ForEach(AddToFishbone);
            long currentQuality = 0L;
            foreach (Bone bone in this.Fishbone)
            {
                currentQuality *= bone.Spine.DigitCount.Pow10;
                currentQuality += bone.Spine;
            }

            return currentQuality;
        }

        private void AddToFishbone(int value)
        {
            foreach (int i in ..this.Fishbone.Count)
            {
                ref Bone bone = ref this.Fishbone.AsSpan()[i];
                if (bone.AddSegment(value))
                {
                    return;
                }
            }

            this.Fishbone.Add(new Bone(value));
        }

        /// <inheritdoc />
        public int CompareTo(Sword? other)
        {
            if (other is null) return 1;

            int comparison = this.Quality.CompareTo(other.Quality);
            if (comparison is not 0) return comparison;

            int maxLevel = Math.Min(this.Fishbone.Count, other.Fishbone.Count);
            foreach (int level in ..maxLevel)
            {
                comparison = this.Fishbone[level].Score.CompareTo(other.Fishbone[level].Score);
                if (comparison is not 0) return comparison;
            }

            return this.ID.CompareTo(other.ID);
        }

        public override string ToString() => $"{this.ID}: {this.Quality}";
    }

    public struct Bone(int spine)
    {
        public int Spine { get; } = spine;

        private int? score;
        public int Score => this.score ??= GetScore();

        private int? left;
        private int? right;

        public bool AddSegment(int value)
        {
            if (value == this.Spine) return false;

            if (value > this.Spine)
            {
                if (this.right is not null) return false;

                this.right = value;
                return true;
            }

            if (this.left is not null) return false;

            this.left = value;
            return true;
        }

        private int GetScore()
        {
            int currentScore = 0;
            if (this.left is not null)
            {
                currentScore += this.left.Value;
            }

            currentScore *= this.Spine.DigitCount.Pow10;
            currentScore += this.Spine;

            if (this.right is not null)
            {
                currentScore *= this.right.Value.DigitCount.Pow10;
                currentScore += this.right.Value;
            }

            return currentScore;
        }
    }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        LogAnswer(this.Data[0].Quality);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        long minQuality = long.MaxValue;
        long maxQuality = long.MinValue;
        foreach (Sword sword in this.Data)
        {
            long quality = sword.Quality;
            minQuality = Math.Min(minQuality, quality);
            maxQuality = Math.Max(maxQuality, quality);
        }

        LogAnswer(maxQuality - minQuality);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        this.Data.Sort(DescendingComparer<Sword>.Comparer);
        int checksum = this.Data.Index().Sum(d => (d.Index + 1) * d.Item.ID);
        LogAnswer(checksum);
    }
    // ReSharper enable CognitiveComplexity

    /// <inheritdoc />
    protected override Sword ConvertLine(string line)
    {
        int separatorPosition = line.IndexOf(':');
        int id = int.Parse(line.AsSpan(0, separatorPosition));
        ImmutableArray<int> values = ParseUtils.ParseImmutableArray(line.AsSpan(separatorPosition + 1), ',', s => int.Parse(s));
        return new Sword(id, values);
    }
}
