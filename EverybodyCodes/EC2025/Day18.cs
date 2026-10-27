using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Challenge.Collections;
using Challenge.Solvers;
using Challenge.Utils.Extensions.Enumerables;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Strings;
using Challenge.Utils.Extensions.ValueEnumerables;
using ZLinq;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 18
/// </summary>
[Solver(2025, 18)]
public sealed partial class Day18 : Solver<(ImmutableArray<Day18.Plant> Plants, Grid<bool> Tests)>, IEverybodyCodesSolver
{
    public sealed partial class Plant
    {
        [GeneratedRegex(@"Plant (\d+) with thickness (\d+):")]
        private static partial Regex Matcher { get; }

        private readonly int thickness;

        public int ID { get; }

        public List<Branch> Branches { get; } = [];

        private long? energy;
        public long Energy
        {
            get
            {
                if (this.energy is null)
                {
                    long receivedEnergy = this.Branches.Sum(b => b.Energy);
                    this.energy = receivedEnergy >= this.thickness ? receivedEnergy : 0;
                }

                return this.energy!.Value;
            }
        }

        public Plant(string data)
        {
            Match match    = Matcher.Match(data);
            this.ID        = int.Parse(match.Groups[1].ValueSpan);
            this.thickness = int.Parse(match.Groups[2].ValueSpan);
        }

        public void Reset()
        {
            this.energy = null;
            this.Branches.ForEach(b => b.Reset());
        }

        public override string ToString() => $"Plant {this.ID} with thickness {this.thickness}";
    }

    public sealed partial class Branch
    {
        [GeneratedRegex(@"- (?:free branch|branch to Plant (\d+)) with thickness (-?\d+)")]
        private static partial Regex Matcher { get; }

        public int Thickness { get; }

        public Plant? Connection { get; }

        public bool IsEnabled { get; set; } = true;

        private long? energy;
        public long Energy => this.energy ??= this.Connection?.Energy * this.Thickness
                                           ?? (this.IsEnabled ? this.Thickness : 0);

        public Branch(string data, IList<Plant> plants)
        {
            Match match = Matcher.Match(data);

            if (!match.Groups[1].ValueSpan.IsEmpty)
            {
                int connectionID = int.Parse(match.Groups[1].ValueSpan);
                this.Connection  = plants[connectionID - 1];
            }

            this.Thickness = int.Parse(match.Groups[2].ValueSpan);
        }

        public void Reset() => this.energy = null;

        public override string ToString() => this.Connection is not null
                                                 ? $"Branch to Plant {this.Connection.ID} with thickness {this.Thickness}"
                                                 : $"Free branch with thickness {this.Thickness}";
    }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        long totalEnergy = this.Data.Plants[^1].Energy;
        LogAnswer(totalEnergy);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        long totalEnergy = 0L;
        foreach (int row in ..this.Data.Tests.Height)
        {
            ReadOnlySpan<bool> test = this.Data.Tests.GetRow(row);
            totalEnergy += GetEnergyForState(test);
        }

        LogAnswer(totalEnergy);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        int leafCount = this.Data.Tests.Width;
        Span<bool> bestState = stackalloc bool[leafCount];
        ReadOnlySpan<Plant> body = this.Data.Plants.AsSpan(leafCount..);
        foreach (Plant plant in body)
        {
            foreach (Branch branch in plant.Branches)
            {
                if (branch.Connection!.ID <= leafCount)
                {
                    bestState[branch.Connection.ID - 1] = branch.Thickness > 0;
                }
            }
        }

        long optimalEnergy = GetEnergyForState(bestState);
        long energyDifference = 0L;
        foreach (int row in ..this.Data.Tests.Height)
        {
            ReadOnlySpan<bool> test = this.Data.Tests.GetRow(row);
            long stateEnergy = GetEnergyForState(test);
            if (stateEnergy is not 0L)
            {
                energyDifference += optimalEnergy - stateEnergy;
            }
        }

        LogAnswer(energyDifference);
    }
    // ReSharper enable CognitiveComplexity

    private long GetEnergyForState(ReadOnlySpan<bool> state)
    {
        state.Index().ForEach(p => this.Data.Plants[p.Index].Branches[0].IsEnabled = p.Item);
        long energy = this.Data.Plants[^1].Energy;
        this.Data.Plants.ForEach(p => p.Reset());
        return energy;
    }

    /// <inheritdoc />
    protected override (ImmutableArray<Plant>, Grid<bool>) Convert(string[] rawInput)
    {
        ImmutableArray<Plant>.Builder plants = ImmutableArray.CreateBuilder<Plant>(rawInput.Length);
        Plant currentPlant = new(rawInput[0]);
        plants.Add(currentPlant);

        int i;
        for (i = 1; i < rawInput.Length; i++)
        {
            string line = rawInput[i];
            if (line.StartsWith('-'))
            {
                Branch branch = new(line, plants);
                currentPlant.Branches.Add(branch);
            }
            else if (line.StartsWith('P'))
            {
                currentPlant = new Plant(line);
                plants.Add(currentPlant);
            }
            else
            {
                break;
            }
        }

        if (i == rawInput.Length) return (plants.ToImmutable(), Grid<bool>.Empty);

        int width = (rawInput[i].Length + 1) / 2;
        int height = rawInput.Length - i;
        Grid<bool> tests = new(width, height, rawInput[i..], l => l.Where(c => !c.IsWhiteSpaceChar).Select(c => c is '1').ToArray());
        return (plants.ToImmutable(), tests);
    }
}
