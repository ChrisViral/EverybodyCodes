using System.Collections.Frozen;
using System.Collections.Immutable;
using Challenge.Collections.Search;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Spans;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 14
/// </summary>
[Solver(2024, 14)]
public sealed partial class Day14 : ArraySolver<ImmutableArray<Vector3<int>>>, IEverybodyCodesSolver
{
    private FrozenSet<Vector3<int>> tree = [];

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int maxHeight = 0;
        Vector3<int> position = Vector3<int>.Zero;
        foreach (Vector3<int> direction in this.Data[0])
        {
            position += direction;
            maxHeight = Math.Max(maxHeight, position.Y);
        }
        LogAnswer(maxHeight);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        GenerateTree(out _);
        LogAnswer(this.tree.Count);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        GenerateTree(out ImmutableArray<Vector3<int>> leaves);
        List<Vector3<int>> trunk = [..this.tree.Where(p => p is { X: 0, Z: 0})];

        int murkiness = trunk.Min(p => CalculateMurkiness(p, leaves));
        LogAnswer(murkiness);
    }
    // ReSharper enable CognitiveComplexity

    private void GenerateTree(out ImmutableArray<Vector3<int>> leaves)
    {
        ImmutableArray<Vector3<int>>.Builder leavesBuilder = ImmutableArray.CreateBuilder<Vector3<int>>(this.Data.Length);
        HashSet<Vector3<int>> treeSegments = new(1000);
        foreach (ImmutableArray<Vector3<int>> branch in this.Data)
        {
            Vector3<int> position = Vector3<int>.Zero;
            foreach (Vector3<int> direction in branch)
            {
                int length = direction.ManhattanLength;
                Vector3<int> movement = direction / length;
                foreach (int _ in ..length)
                {
                    position += movement;
                    treeSegments.Add(position);
                }
            }
            leavesBuilder.Add(position);
        }

        this.tree = treeSegments.ToFrozenSet();
        leaves    = leavesBuilder.ToImmutable();
    }

    private int CalculateMurkiness(Vector3<int> tapPosition, ImmutableArray<Vector3<int>> leaves)
    {
        return leaves.Sum(leave => SearchUtils.GetPathLengthBFS(tapPosition, leave, Neighbours)!.Value);
    }

    private IEnumerable<Vector3<int>> Neighbours(Vector3<int> node)
    {
        return node.AsAdjacentEnumerable().Where(adjacent => this.tree.Contains(adjacent));
    }

    /// <inheritdoc />
    protected override ImmutableArray<Vector3<int>> ConvertLine(string line)
    {
        int count = line.Count(',') + 1;
        Span<Range> splits = stackalloc Range[count];
        line.Split(splits, ',');
        ImmutableArray<Vector3<int>>.Builder vectors = ImmutableArray.CreateBuilder<Vector3<int>>(count);
        splits.ForEach(s => vectors.Add(Vector3<int>.ParseFromDirection(line[s])));
        return vectors.ToImmutable();
    }
}
