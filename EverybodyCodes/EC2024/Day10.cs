using System.Collections.Immutable;
using System.Text;
using Challenge.Collections;
using Challenge.Collections.Pooling;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Utils.Extensions.Enumerables;
using Challenge.Utils.Extensions.Ranges;
using Challenge.Utils.Extensions.Strings;
using ZLinq;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 10
/// </summary>
[Solver(2024, 10)]
public sealed partial class Day10 : Solver<Grid<Day10.RunicGrid>>, IEverybodyCodesSolver
{
    public sealed class RunicGrid
    {
        private const char UNKNOWN = '?';
        private static readonly Grid<char> Field = new(FIELD_SIZE, FIELD_SIZE);
        private static readonly char[] Buffer = new char[GRID_SIZE];

        private ImmutableArray<char[]> Rows { get; }

        private ImmutableArray<char[]> Columns { get; }

        private Vector2<int> Position { get; }

        private Grid<char> Runes { get; } = new(GRID_SIZE, GRID_SIZE);

        public bool IsSolved => !this.Runes.Contains(UNKNOWN);

        public RunicGrid(ReadOnlySpan<string> lines, Vector2<int> position, int horizontalOffset)
        {
            const int EMPTY_START = GRID_SIZE / 2;
            const int EMPTY_END = EMPTY_START + GRID_SIZE;

            this.Runes.Fill(UNKNOWN);
            this.Position = position;
            int start = position.X * horizontalOffset;
            foreach (int row in ..FIELD_SIZE)
            {
                lines[row].AsSpan(start, FIELD_SIZE).CopyTo(Field.GetRow(row));
            }

            ImmutableArray<char[]>.Builder builder = ImmutableArray.CreateBuilder<char[]>(GRID_SIZE);
            Span<char> lineData = stackalloc char[GRID_SIZE];
            for (int y = 0; y < GRID_SIZE; y++)
            {
                int row = y + EMPTY_START;
                Span<char> rowContents = Field.GetRow(row);
                rowContents[..EMPTY_START].CopyTo(lineData[..EMPTY_START]);
                rowContents[EMPTY_END..].CopyTo(lineData[EMPTY_START..]);
                builder.Add([..lineData]);
            }
            this.Rows = builder.ToImmutable();

            builder = ImmutableArray.CreateBuilder<char[]>(GRID_SIZE);
            Span<char> columnContent = stackalloc char[FIELD_SIZE];
            for (int x = 0; x < GRID_SIZE; x++)
            {
                int column = x + EMPTY_START;
                Field.GetColumn(column).CopyTo(columnContent);
                columnContent[..EMPTY_START].CopyTo(lineData[..EMPTY_START]);
                columnContent[EMPTY_END..].CopyTo(lineData[EMPTY_START..]);
                builder.Add([..lineData]);
            }
            this.Columns = builder.ToImmutable();
            Field.Clear();
        }

        public void SolveRunicWord()
        {
            foreach (Vector2<int> position in Vector2<int>.EnumerateOver(GRID_SIZE, GRID_SIZE))
            {
                char[] column = this.Columns[position.X];
                char[] row    = this.Rows[position.Y];
                this.Runes[position] = column.FirstOrDefault(row.Contains, UNKNOWN);
            }
        }

        public string GetRunicWord()
        {
            Span<char> buffer = stackalloc char[GRID_SIZE * GRID_SIZE];
            for (int row = 0, offset = 0; row < GRID_SIZE; row++, offset += GRID_SIZE)
            {
                this.Runes.GetRow(row).CopyTo(buffer[offset..]);
            }
            return buffer.ToString();
        }

        public int GetRunicPower() => this.Runes.Select((c, i) => (c.AsIndex + 1) * (i + 1)).Sum();

        // ReSharper disable once CognitiveComplexity
        public bool TrySolveUnknowns(Grid<RunicGrid> grids)
        {
            if (this.IsSolved) return false;

            // Look through all positions in runes
            bool updates = false;
            foreach (Vector2<int> position in this.Runes.Dimensions.Enumerate())
            {
                // Ignore solved positions
                if (this.Runes[position] is not UNKNOWN) continue;

                // Get current column/row
                char[] column = this.Columns[position.X];
                char[] row    = this.Rows[position.Y];

                // Try to solve directly
                char nonUnknownValue = column.Where(c => c is not UNKNOWN).FirstOrDefault(row.Contains, UNKNOWN);
                if (nonUnknownValue is not UNKNOWN)
                {
                    this.Runes[position] = nonUnknownValue;
                    updates = true;
                    continue;
                }

                // Get index of unknown in row and column
                int rowUnknownIndex    = row.IndexOf(UNKNOWN);
                int columnUnknownIndex = column.IndexOf(UNKNOWN);

                // Unknown in row
                if (rowUnknownIndex is not -1)
                {
                    // If we have unknowns in both row and column, we cannot solve this
                    if (columnUnknownIndex is not -1) continue;

                    // Get column buffer, if more than one unknown, we cannot solve yet
                    this.Runes.GetColumn(position.X).CopyTo(Buffer);
                    if (Buffer.Count(UNKNOWN) > 1) continue;

                    // Get first value not in current column already, if none found, we cannot solve yet
                    char unknownValue = column.FirstOrDefault(c => !Buffer.Contains(c), UNKNOWN);
                    if (unknownValue is UNKNOWN) continue;

                    // Solve current value
                    row[rowUnknownIndex] = unknownValue;
                    this.Runes[position] = unknownValue;
                    updates = true;
                }
                // Unknown in column
                else if (columnUnknownIndex is not -1)
                {
                    // Get row buffer, if more than one unknown, we cannot solve yet
                    this.Runes.GetRow(position.Y).CopyTo(Buffer);
                    if (Buffer.Count(UNKNOWN) > 1) continue;

                    // Get first value not in current row already, if none found, we cannot solve yet
                    char unknownValue = row.FirstOrDefault(c => !Buffer.Contains(c), UNKNOWN);
                    if (unknownValue is UNKNOWN) continue;

                    // Solve current value
                    column[columnUnknownIndex] = unknownValue;
                    this.Runes[position] = unknownValue;
                    updates = true;
                }
            }

            // Copy shared rows/columns only when solved
            if (updates && this.IsSolved)
            {
                CopyToAdjacent(grids);
            }

            return updates;
        }

        // ReSharper disable once CognitiveComplexity
        private void CopyToAdjacent(Grid<RunicGrid> grids)
        {
            // Copy to grid to the left
            if (grids.TryGetPosition(this.Position + Direction.LEFT, out RunicGrid? otherGrid))
            {
                foreach (int row in ..GRID_SIZE)
                {
                    otherGrid.Rows[row][2] = this.Rows[row][0];
                    otherGrid.Rows[row][3] = this.Rows[row][1];
                }
            }
            // Copy to grid to the right
            if (grids.TryGetPosition(this.Position + Direction.RIGHT, out otherGrid))
            {
                foreach (int row in ..GRID_SIZE)
                {
                    otherGrid.Rows[row][0] = this.Rows[row][2];
                    otherGrid.Rows[row][1] = this.Rows[row][3];
                }
            }
            // Copy to grid above
            if (grids.TryGetPosition(this.Position + Direction.UP, out otherGrid))
            {
                foreach (int column in ..GRID_SIZE)
                {
                    otherGrid.Columns[column][2] = this.Columns[column][0];
                    otherGrid.Columns[column][3] = this.Columns[column][1];
                }
            }
            // Copy to grid below
            if (grids.TryGetPosition(this.Position + Direction.DOWN, out otherGrid))
            {
                foreach (int column in ..GRID_SIZE)
                {
                    otherGrid.Columns[column][0] = this.Columns[column][2];
                    otherGrid.Columns[column][1] = this.Columns[column][3];
                }
            }
        }

        public override string ToString()
        {
            using Pooled<StringBuilder> pooledBuilder = StringBuilderObjectPool.Shared.Get();
            StringBuilder builder = pooledBuilder.Ref;
            builder.Append("**").Append(string.Concat(this.Columns.Select(c => c[0]))).AppendLine("**");
            builder.Append("**").Append(string.Concat(this.Columns.Select(c => c[1]))).AppendLine("**");
            builder.Append(this.Rows[0].AsSpan(0, 2)).Append(this.Runes.GetRow(0)).Append(this.Rows[0].AsSpan(2)).AppendLine();
            builder.Append(this.Rows[1].AsSpan(0, 2)).Append(this.Runes.GetRow(1)).Append(this.Rows[1].AsSpan(2)).AppendLine();
            builder.Append(this.Rows[2].AsSpan(0, 2)).Append(this.Runes.GetRow(2)).Append(this.Rows[2].AsSpan(2)).AppendLine();
            builder.Append(this.Rows[3].AsSpan(0, 2)).Append(this.Runes.GetRow(3)).Append(this.Rows[3].AsSpan(2)).AppendLine();
            builder.Append("**").Append(string.Concat(this.Columns.Select(c => c[2]))).AppendLine("**");
            builder.Append("**").Append(string.Concat(this.Columns.Select(c => c[3]))).AppendLine("**");
            return builder.ToStringAndClear();
        }
    }

    private const int GRID_SIZE = 4;
    private const int FIELD_SIZE = GRID_SIZE * 2;

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        RunicGrid grid = this.Data[0, 0];
        grid.SolveRunicWord();
        LogAnswer(grid.GetRunicWord());
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        this.Data.ForEach(g => g.SolveRunicWord());
        int totalPower = this.Data.Sum(g => g.GetRunicPower());
        LogAnswer(totalPower);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        this.Data.ForEach(g => g.SolveRunicWord());
        while (this.Data.Any(g => g.TrySolveUnknowns(this.Data)));
        int totalPower = this.Data.Where(g => g.IsSolved)
                             .Sum(g => g.GetRunicPower());
        LogAnswer(totalPower);
    }
    // ReSharper enable CognitiveComplexity

    /// <inheritdoc />
    protected override Grid<RunicGrid> Convert(string[] rawInput)
    {
        bool isPacked = rawInput.Length > FIELD_SIZE && !rawInput[0].Contains(' ');
        int verticalJump = isPacked ? FIELD_SIZE - 2 : FIELD_SIZE;
        int horizontalJump = isPacked ? FIELD_SIZE - 2 : FIELD_SIZE + 1;

        int width = (rawInput[0].Length + 1) / horizontalJump;
        int height = rawInput.Length / verticalJump;
        Grid<RunicGrid> grid = new(width, height);
        for (int lineStart = 0, y = 0; lineStart <= rawInput.Length - FIELD_SIZE; lineStart += verticalJump, y++)
        {
            ReadOnlySpan<string> lines = rawInput.AsSpan(lineStart, FIELD_SIZE);
            foreach (int x in ..width)
            {
                Vector2<int> position = (x, y);
                grid[position] = new RunicGrid(lines, position, horizontalJump);
            }
        }
        return grid;
    }
}
