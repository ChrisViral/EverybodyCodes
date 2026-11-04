using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Challenge.Collections;
using Challenge.Collections.Pooling;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Collections;
using Challenge.Utils.Extensions.Enumerables;
using Challenge.Utils.Extensions.Ranges;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 7
/// </summary>
[Solver(2026, 7)]
public sealed partial class Day07 : Solver<ImmutableArray<Day07.Cloud>>, IEverybodyCodesSolver
{
    public sealed class Cloud : IEquatable<Cloud>
    {
        private const int MAX_ENERGY = 1000000;
        private const int EXPLODE_IMPULSE = 10;

        public int ID { get; }

        public int Energy { get; private set; }

        public Cloud[] Connections { get; }

        private bool isExploded;
        private readonly ImmutableArray<int> connectionIDs;

        public Cloud(int id, int energy, string connections)
        {
            this.ID            = id;
            this.Energy        = energy;
            this.connectionIDs = ParseUtils.ParseImmutableArray(connections, ',', v => int.Parse(v));
            this.Connections   = new Cloud[this.connectionIDs.Length];
        }

        public Cloud(Cloud other)
        {
            this.ID            = other.ID;
            this.Energy        = other.Energy;
            this.Connections   = new Cloud[other.Connections.Length];
            this.connectionIDs = other.connectionIDs;
            this.isExploded    = other.isExploded;
        }

        public void Impulse(int strength, int count, DefaultDictionary<Cloud, Counter<int>> chainedImpulses)
        {
            if (this.isExploded) return;

            this.Energy += strength * count;
            if (strength > 1)
            {
                strength--;
                this.Connections.ForEach(c => chainedImpulses[c][strength] += count);
            }
        }

        public void HandleExplosion(DefaultDictionary<Cloud, Counter<int>> chainedImpulses)
        {
            if (this.isExploded || this.Energy < MAX_ENERGY) return;

            this.Energy     = 0;
            this.isExploded = true;
            this.Connections.ForEach(c => chainedImpulses[c][EXPLODE_IMPULSE]++);
        }

        public void SetupConnections(Dictionary<int, Cloud> clouds)
        {
            foreach (int i in ..this.Connections.Length)
            {
                this.Connections[i] = clouds[this.connectionIDs[i]];
            }
        }

        public override string ToString() => $"ID: {this.ID}, Energy: {this.Energy}";

        /// <inheritdoc />
        public bool Equals(Cloud? other) => this.ID == other?.ID;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Cloud other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => this.ID;
    }

    [GeneratedRegex(@"id=(\d+), energy=(\d+), connections=([\d,]+)")]
    private static partial Regex Matcher { get; }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const int IMPULSE = 5;
        int totalEnergy = SendImpulse(this.Data, IMPULSE);
        LogAnswer(totalEnergy);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const int IMPULSE = 10;
        int totalEnergy = SendImpulse(this.Data, IMPULSE);
        LogAnswer(totalEnergy);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int IMPULSE = 10;
        int bestTotalEnergy = 0;
        Cloud[] clouds = new Cloud[this.Data.Length];
        foreach (int i in ..this.Data.Length)
        {
            SetupCloudsCopy(clouds);
            Cloud start = clouds[i];
            bestTotalEnergy = Math.Max(bestTotalEnergy, GetImpulseTotalEnergy(clouds, start, IMPULSE));
        }

        LogAnswer(bestTotalEnergy);
    }
    // ReSharper enable CognitiveComplexity

    private static int SendImpulse(IReadOnlyList<Cloud> clouds, int startStrength)
    {
        Cloud start = clouds.MaxBy(c => c.Connections.Length)!;
        return GetImpulseTotalEnergy(clouds, start, startStrength, false);
    }

    private static int GetImpulseTotalEnergy(IReadOnlyList<Cloud> clouds, Cloud start, int startStrength, bool withExplosions = true)
    {
        DefaultDictionary<Cloud, Counter<int>> impulsesPerCloud = new(_ => []);
        DefaultDictionary<Cloud, Counter<int>> nextImpulsesPerCloud = new(_ => []);

        impulsesPerCloud[start][startStrength] = 1;
        do
        {
            foreach ((Cloud cloud, Counter<int> impulses) in impulsesPerCloud)
            {
                foreach ((int strength, int count) in impulses)
                {
                    cloud.Impulse(strength, count, nextImpulsesPerCloud);
                }
            }

            if (withExplosions)
            {
                foreach (Cloud cloud in clouds)
                {
                    cloud.HandleExplosion(nextImpulsesPerCloud);
                }
            }

            SwapUtils.Swap(ref impulsesPerCloud, ref nextImpulsesPerCloud);
            nextImpulsesPerCloud.Clear();
        }
        while (!impulsesPerCloud.IsEmpty);

        return clouds.Sum(c => c.Energy);
    }

    private void SetupCloudsCopy(Cloud[] clouds)
    {
        using Pooled<Dictionary<int, Cloud>> pooledCloudMap = DictionaryObjectPool<int, Cloud>.Shared.Get();
        Dictionary<int, Cloud> cloudMap = pooledCloudMap.Ref;
        foreach (int i in ..this.Data.Length)
        {
            Cloud cloud = new(this.Data[i]);
            cloudMap[cloud.ID] = cloud;
            clouds[i] = cloud;
        }

        clouds.ForEach(c => c.SetupConnections(cloudMap));
    }

    /// <inheritdoc />
    protected override ImmutableArray<Cloud> Convert(string[] rawInput)
    {
        using Pooled<Dictionary<int, Cloud>> pooledCloudMap = DictionaryObjectPool<int, Cloud>.Shared.Get();
        Dictionary<int, Cloud> cloudMap = pooledCloudMap.Ref;
        RegexFactory<Cloud> factory = new(Matcher);
        foreach (string line in rawInput)
        {
            Cloud cloud = factory.ConstructObject(line);
            cloudMap[cloud.ID] = cloud;
        }

        cloudMap.Values.ForEach(c => c.SetupConnections(cloudMap));
        return [..cloudMap.Values.OrderBy(c => c.ID)];
    }
}
