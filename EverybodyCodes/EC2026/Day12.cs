using Challenge.Collections;
using Challenge.Collections.Pooling;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils.Extensions.Spans;
using Challenge.Utils.Extensions.ValueEnumerables;
using ZLinq;

namespace EverybodyCodes.EC2026;

/// <summary>
/// Solver for 2026 Day 12
/// </summary>
[Solver(2026, 12)]
public sealed partial class Day12 : GridSolver<bool>, IEverybodyCodesSolver
{
    private abstract record Block(int Width, int Height)
    {
        public virtual int GetSpellCount() => Math.Min(this.Width, this.Height);

        public static Block FindBlock(Grid<bool> grid, Vector2<int> start, HashSet<Vector2<int>> used)
        {
            using Pooled<HashSet<Vector2<int>>> points = HashSetObjectPool<Vector2<int>>.Shared.Get();
            using Pooled<Queue<Vector2<int>>> search = QueueObjectPool<Vector2<int>>.Shared.Get();

            points.Ref.Add(start);
            search.Ref.Enqueue(start);

            Vector2<int> min = start;
            Vector2<int> max = start;
            while (search.Ref.TryDequeue(out Vector2<int> position))
            {
                foreach (Vector2<int> adjacent in position.Adjacent())
                {
                    if (!used.Contains(adjacent) && grid.IsPositionEqual(adjacent, true) && points.Ref.Add(adjacent))
                    {
                        search.Ref.Enqueue(adjacent);
                        min = Vector2<int>.Min(min, adjacent);
                        max = Vector2<int>.Max(max, adjacent);
                    }
                }
            }

            used.UnionWith(points.Ref);
            int width  = max.X - min.X + 1;
            int height = max.Y - min.Y + 1;

            if (!points.Ref.Contains((min.X, min.Y))
             || !points.Ref.Contains((min.X, max.Y))
             || !points.Ref.Contains((max.X, min.Y))
             || !points.Ref.Contains((max.X, max.Y)))
            {
                int vertical = points.Ref.GroupBy(point => point.X)
                                     .Select(group => group.Count())
                                     .GroupBy(count => count)
                                     .OrderByDescending(group => group.Key)
                                     .First()
                                     .Count();

                int horizontal = points.Ref.GroupBy(point => point.Y)
                                       .Select(group => group.Count())
                                       .GroupBy(count => count)
                                       .OrderByDescending(group => group.Key)
                                       .First()
                                       .Count();

                // If any of the corners isn't a part of the block, we have an L shape
                return new LBlock(width, height, vertical, horizontal);
            }

            if (!grid.AsSpan2D(min.X, width, min.Y, height).Contains(false))
            {
                // If all of the points inside the block are contained, we have no holes
                return new FullBlock(width, height);
            }

            // Get the positions within the block to get the start and end point of the hole
            Vector2<int>[] withinBlock = Vector2<int>.EnumerateOver(width - 2, height - 2)
                                                     .Select(p => start + p + Vector2<int>.One)
                                                     .ToArray();
            Vector2<int> holeStart = withinBlock.First(p => !grid[p]);
            Vector2<int> holeEnd   = withinBlock.Last(p => !grid[p]);

            // Get the hole dimensions
            int holeWidth  = holeEnd.X - holeStart.X + 1;
            int holeHeight = holeEnd.Y - holeStart.Y + 1;

            // Remove extra positions we might have caught inside the block's hole
            Vector2<int>.EnumerateOver(holeWidth, holeHeight)
                        .Select(p => holeStart + p)
                        .ForEach(p => used.Remove(p));

            return new HoleBlock(width, height, holeWidth, holeHeight, holeStart - start);
        }
    }

    private sealed record FullBlock(int Width, int Height) : Block(Width, Height);

    private sealed record LBlock(int Width, int Height, int Vertical, int Horizontal) : Block(Width, Height)
    {
        /// <inheritdoc />
        public override int GetSpellCount() => Math.Min(base.GetSpellCount(), this.Vertical + this.Horizontal);
    }

    private sealed record HoleBlock(int Width, int Height, int HoleWidth, int HoleHeight, Vector2<int> HoleStart) : Block(Width, Height)
    {
        /// <inheritdoc />
        public override int GetSpellCount()
        {
            int horizontalSpellCount = this.Width - this.HoleWidth;
            horizontalSpellCount += Math.Min(this.HoleWidth, this.HoleStart.Y);
            horizontalSpellCount += Math.Min(this.HoleWidth, this.Height - this.HoleStart.Y - this.HoleHeight);

            int verticalSpellCount = this.Height - this.HoleHeight;
            verticalSpellCount += Math.Min(this.HoleHeight, this.HoleStart.X);
            verticalSpellCount += Math.Min(this.HoleHeight, this.Width - this.HoleStart.X - this.HoleWidth);

            return Math.Min(horizontalSpellCount, verticalSpellCount);
        }
    }

    private const char MAGMA = '#';

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        List<Block> blocks = GetBlocks();
        int spells = blocks.Sum(b => b.GetSpellCount());
        LogAnswer(spells);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2() => RunPart1();

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3() => RunPart1();
    // ReSharper enable CognitiveComplexity

    private List<Block> GetBlocks()
    {
        HashSet<Vector2<int>> used = new(this.Data.Size);
        List<Block> blocks = new(100);
        foreach (Vector2<int> position in this.Data.Dimensions.Enumerate())
        {
            if (!this.Data[position] || used.Contains(position)) continue;

            Block newBlock = Block.FindBlock(this.Data, position, used);
            blocks.Add(newBlock);
        }
        return blocks;
    }

    /// <inheritdoc />
    protected override bool[] LineConverter(string line) => line.Select(c => c is MAGMA).ToArray();
}
