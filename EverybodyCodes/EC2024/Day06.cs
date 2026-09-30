using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Challenge.Solvers;
using Challenge.Utils;
using Challenge.Utils.Extensions.Arrays;
using Challenge.Utils.Extensions.Enumerables;
using ZLinq;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 6
/// </summary>
[Solver(2024, 6)]
public sealed partial class Day06 : Solver<Day06.Branch>, IEverybodyCodesSolver
{
    private const string ROOT = "RR";
    private const string FRUIT = "@";

    [GeneratedRegex("([A-Z]+):([A-Z@]+(?:,[A-Z@]+)*)")]
    private static partial Regex Matcher { get; }

    public sealed class Branch
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string[] connections;

        public string Name { get; }

        public int Depth { get; private set; }

        public Branch? Parent { get; private set; }

        public ImmutableArray<Branch> Children { get; private set; } = [];

        public bool IsFruit => this.Name is FRUIT;

        private Branch(string name)
        {
            this.Name = name;
            this.connections = [];
        }

        public Branch(string name, string connections)
        {
            this.Name = name;
            this.connections = connections.Split(',');
        }

        public void SetupChildren(Dictionary<string, Branch> branches)
        {
            this.Children = this.connections.Select(c =>
            {
                if (c is FRUIT) return new Branch(FRUIT);
                if (!branches.TryGetValue(c, out Branch? branch))
                {
                    branch = new Branch(c);
                    branches.Add(c, branch);
                }
                return branch;
            }).ToImmutableArray();
        }

        public void SetupParent(Branch? parent)
        {
            if (this.Parent is not null) return;

            this.Parent = parent;
            this.Depth = parent?.Depth + 1 ?? 0;
            this.Children.ForEach(c => c.SetupParent(this));
        }

        public IEnumerable<Branch> EnumerateFruit(HashSet<string>? visited = null)
        {
            visited ??= [];
            if (!visited.Add(this.Name)) yield break;

            foreach (Branch child in this.Children)
            {
                if (child.IsFruit)
                {
                    yield return child;
                    continue;
                }

                foreach (Branch fruit in child.EnumerateFruit(visited))
                {
                    yield return fruit;
                }
            }

            visited.Remove(this.Name);
        }

        public override string ToString() => this.Name;
    }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        Branch bestFruit = this.Data.EnumerateFruit().GroupBy(f => f.Depth).First(g => g.Count() is 1).First();
        StringBuilder path = new(FRUIT);
        for (Branch? parent = bestFruit.Parent; parent is not null; parent = parent.Parent)
        {
            path.Insert(0, parent.Name);
        }
        LogAnswer(path.ToString());
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        Branch bestFruit = this.Data.EnumerateFruit().GroupBy(f => f.Depth).First(g => g.Count() is 1).First();
        StringBuilder path = new(FRUIT);
        for (Branch? parent = bestFruit.Parent; parent is not null; parent = parent.Parent)
        {
            path.Insert(0, parent.Name[0]);
        }
        LogAnswer(path.ToString());
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3() => RunPart2();
    // ReSharper enable CognitiveComplexity

    /// <inheritdoc />
    protected override Branch Convert(string[] rawInput)
    {
        Branch[] branches = RegexFactory<Branch>.ConstructObjects(Matcher, rawInput);
        Dictionary<string, Branch> branchMap = branches.ToDictionary(b => b.Name, b => b);
        branches.ForEach(b => b.SetupChildren(branchMap));
        Branch root = branchMap[ROOT];
        root.SetupParent(null);
        return root;
    }
}
