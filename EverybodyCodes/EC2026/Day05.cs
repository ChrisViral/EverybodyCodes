using Challenge.Collections;
using Challenge.Collections.Pooling;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils.Extensions.Enumerables;
using Challenge.Utils.Extensions.ValueEnumerables;
using ZLinq;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 5
/// </summary>
[Solver(2026, 5)]
public sealed partial class Day05 : GridSolver<Day05.Cell>, IEverybodyCodesSolver
{
    public enum Cell
    {
        LIGHT = '0',
        DARK  = '1',
        WATER = '.',
        LAND  = '#',
        SAND  = '=',
        TREE  = '@'
    }

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int land = ProcessLand();
        LogAnswer(land);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        ProcessLand();
        int sand = ProcessBeaches();
        LogAnswer(sand);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        ProcessLand();
        ProcessBeaches();
        int trees = ProcessTrees();
        LogAnswer(trees);
    }
    // ReSharper enable CognitiveComplexity

    private int ProcessLand()
    {
        Vector2<int> centre = this.Data.Dimensions / 2;
        this.Data[centre] = Cell.LAND;
        return ProcessMap(Cell.LAND, IsValidLandCell);
    }

    private int ProcessBeaches()
    {
        this.Data.Dimensions
            .Enumerate()
            .Where(p => this.Data[p] is not Cell.LAND)
            .ForEach(p => this.Data[p] = Cell.WATER);

        this.Data.Dimensions
            .Enumerate()
            .Where(p => this.Data[p] == Cell.LAND
                     && p.Adjacent(options: AdjacentOptions.WITH_DIAGONALS)
                         .Any(a => this.Data.TryGetPosition(a, out Cell value) && value is Cell.WATER))
            .ForEach(p => this.Data[p] = Cell.SAND);

        return ProcessMap(Cell.SAND, IsValidBeachCell);
    }

    private int ProcessTrees()
    {
        const int TREE_LAYERS = 2;
        const char TREE_CHECK = '+';
        const int TREE_PATTERN_SIZE = 5;
        const string TREE_PATTERN =
        """
        .+++.
        +++++
        ++X++
        +++++
        .+++.
        """;

        Vector2<int> treeOffset = new(TREE_PATTERN_SIZE / 2, TREE_PATTERN_SIZE / 2);
        Grid<bool> treePattern = new(TREE_PATTERN_SIZE, TREE_PATTERN_SIZE,
                                     TREE_PATTERN.Split("\n", DEFAULT_OPTIONS),
                                     line => line.Select(c => c is TREE_CHECK).ToArray());

        foreach (Vector2<int> start in Vector2<int>.EnumerateOver(this.Data.Width - TREE_PATTERN_SIZE, this.Data.Height - TREE_PATTERN_SIZE))
        {
            Vector2<int> treePosition = start + treeOffset;
            if (this.Data[treePosition] is not Cell.LAND) continue;

            if (Vector2<int>.EnumerateOver(TREE_PATTERN_SIZE, TREE_PATTERN_SIZE)
                            .Where(p => treePattern[p])
                            .All(p => this.Data[start + p] is Cell.LAND or Cell.TREE))
            {
                this.Data[treePosition] = Cell.TREE;
            }
        }

        return ProcessMap(Cell.TREE, IsValidTreeCell, TREE_LAYERS);
    }

    private int ProcessMap(Cell type, Func<Vector2<int>, bool> isValid, int maxSteps = int.MaxValue)
    {
        int cells = this.Data.Count(type);
        for (int step = 0, changed = 1; step < maxSteps && changed > 0; step++, cells += changed)
        {
            changed =  ProcessMapLayer(type, isValid);
        }
        return cells;
    }

    private int ProcessMapLayer(Cell type, Func<Vector2<int>, bool> isValid)
    {
        using Pooled<List<Vector2<int>>> toChange = ListObjectPool<Vector2<int>>.Shared.Get();
        using Pooled<HashSet<Vector2<int>>> positions = GetAllAdjacent(type);
        foreach (Vector2<int> position in positions.Ref)
        {
            if (isValid(position))
            {
                toChange.Ref.Add(position);
            }
        }

        toChange.Ref.ForEach(p => this.Data[p] = type);
        return toChange.Ref.Count;
    }

    private Pooled<HashSet<Vector2<int>>> GetAllAdjacent(Cell type)
    {
        Pooled<HashSet<Vector2<int>>> adjacentSet = HashSetObjectPool<Vector2<int>>.Shared.Get();
        foreach (Vector2<int> adjacent in this.Data.Dimensions
                                              .Enumerate()
                                              .Where(p => this.Data[p] == type)
                                              .SelectMany(p => p.Adjacent(options: AdjacentOptions.WITH_DIAGONALS))
                                              .Where(p => !this.Data.TryGetPosition(p, out Cell value) || value != type))
        {
            adjacentSet.Ref.Add(adjacent);
        }

        return adjacentSet;
    }

    private bool IsValidLandCell(Vector2<int> position) => position.Adjacent(options: AdjacentOptions.WITH_DIAGONALS)
                                                                        .Count(p => this.Data.TryGetPosition(p, out Cell value)
                                                                                 && value is not Cell.LIGHT) >= 5;

    private bool IsValidBeachCell(Vector2<int> position) => position.Adjacent(options: AdjacentOptions.WITH_DIAGONALS)
                                                                         .Count(p => this.Data.TryGetPosition(p, out Cell value)
                                                                                  && value is Cell.SAND) >= 5;

    private bool IsValidTreeCell(Vector2<int> position)
    {
        int trees = 0;
        foreach (Vector2<int> adjacent in position.Adjacent(options: AdjacentOptions.WITH_DIAGONALS))
        {
            if (!this.Data.TryGetPosition(adjacent, out Cell value)) continue;

            if (value is Cell.SAND) return false;

            if (value is Cell.TREE)
            {
                trees++;
            }
        }

        return trees >= 4;
    }

    /// <inheritdoc />
    protected override Cell[] LineConverter(string line) => line.Select(c => (Cell)c).ToArray();

    /// <inheritdoc />
    protected override string StringConversion(Cell obj) => ((char)obj).ToString();
}
