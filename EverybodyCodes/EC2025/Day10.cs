using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Challenge.Collections;
using Challenge.Maths.Directions;
using Challenge.Maths.Vectors;
using Challenge.Solvers;
using Challenge.Solvers.Specialized;
using Challenge.Utils.Extensions.Ranges;
using ZLinq;

namespace EverybodyCodes.EC2025;

/// <summary>
/// Solver for 2025 Day 10
/// </summary>
[Solver(2025, 10)]
public sealed partial class Day10 : GridSolver<Day10.Cell>, IEverybodyCodesSolver
{
    public enum Piece
    {
        EMPTY  = '.',
        SHEEP  = 'S',
        DRAGON = 'D'
    }

    [InlineArray(7)]
    private struct Sheep : IEquatable<Sheep>
    {
        public const int DEAD  = -1;

        private int element;

        public Sheep()
        {
            this[0] = DEAD;
            this[1] = DEAD;
            this[2] = DEAD;
            this[3] = DEAD;
            this[4] = DEAD;
            this[5] = DEAD;
            this[6] = DEAD;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool AllDead() => this[0] is DEAD
                                       && this[1] is DEAD
                                       && this[2] is DEAD
                                       && this[3] is DEAD
                                       && this[4] is DEAD
                                       && this[5] is DEAD
                                       && this[6] is DEAD;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Sheep other) => this[0] == other[0]
                                                 && this[1] == other[1]
                                                 && this[2] == other[2]
                                                 && this[3] == other[3]
                                                 && this[4] == other[4]
                                                 && this[5] == other[5]
                                                 && this[6] == other[6];

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object? obj) => obj is Sheep other && Equals(other);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode() => HashCode.Combine(this[0], this[1], this[2], this[3], this[4], this[5], this[6]);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"{this[0]}, {this[1]}, {this[2]}, {this[3]}, {this[4]}, {this[5]}, {this[6]}";
    }

    public sealed class Cell
    {
        private const char PROTECTED = '#';

        public Piece Piece { get; set; }

        public bool IsProtected { get; }

        public bool IsSheep => this.Piece is Piece.SHEEP;

        public bool IsDragon => this.Piece is Piece.DRAGON;

        public Cell(char value)
        {
            this.IsProtected = value is PROTECTED;
            this.Piece       = this.IsProtected ? Piece.EMPTY : (Piece)value;
        }

        public Cell(Piece piece, bool isProtected)
        {
            this.Piece       = piece;
            this.IsProtected = isProtected;
        }

        public override string ToString() => this.IsProtected ? PROTECTED.ToString() : ((char)this.Piece).ToString();
    }

    private static readonly ImmutableArray<Vector2<int>> DragonMoves =
    [
        ( 1,  2),
        ( 2,  1),
        (-1,  2),
        (-2,  1),
        ( 1, -2),
        ( 2, -1),
        (-1, -2),
        (-2, -1),
    ];

    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        const int MOVES = 4;

        Vector2<int> startPosition = this.Data.FindPosition(c => c.IsDragon);
        DelayedGrid<bool> dragons = new(this.Data.Width, this.Data.Height) { [startPosition] = true };
        dragons.Apply(clear: true);

        int sheepCaptured = 0;
        foreach (int _ in ..MOVES)
        {
            sheepCaptured += SimulateDragonMoves(dragons);
        }

        LogAnswer(sheepCaptured);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const int MOVES = 20;

        Vector2<int> startPosition = this.Data.FindPosition(c => c.IsDragon);
        DelayedGrid<bool> dragons = new(this.Data.Width, this.Data.Height) { [startPosition] = true };
        dragons.Apply(clear: true);

        int sheepCaptured = 0;
        foreach (int _ in ..MOVES)
        {
            sheepCaptured += SimulateDragonMoves(dragons);
            sheepCaptured += SimulateSheepMoves(dragons);
        }

        LogAnswer(sheepCaptured);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        Vector2<int> startPosition = this.Data.FindPosition(c => c.IsDragon);

        Sheep startSheep = new();
        foreach (int i in ..this.Data.Width)
        {
            if (this.Data[i, 0].IsSheep)
            {
                startSheep[i] = 0;
            }
        }

        int[] lastUnprotected = new int[this.Data.Width];
        foreach (int x in ..this.Data.Width)
        {
            int y;
            for (y = this.Data.Height - 1; y >= 0 && this.Data[x, y].IsProtected; y--);
            lastUnprotected[x] = y;
        }

        Dictionary<(Vector2<int> DragonPosition, Sheep Sheep, bool DragonTurn), long> cache = new(1000);
        long possibleSequences = FindPossibleSequencesSheep(startPosition, startSheep);
        LogAnswer(possibleSequences);
        return;

        long FindPossibleSequencesSheep(Vector2<int> dragonPosition, in Sheep sheep)
        {
            (Vector2<int>, Sheep, bool) cacheKey = (dragonPosition, sheep, false);
            if (cache.TryGetValue(cacheKey, out long sequences)) return sequences;

            sequences = 0L;
            bool allBlocked = true;
            foreach (int x in ..this.Data.Width)
            {
                Sheep newSheep = sheep;
                ref int sheepYPosition = ref newSheep[x];
                if (sheepYPosition is Sheep.DEAD) continue;

                if (sheepYPosition == lastUnprotected[x])
                {
                    allBlocked = false;
                    continue;
                }

                sheepYPosition++;
                Vector2<int> sheepPosition = (x, sheepYPosition);
                if (dragonPosition == sheepPosition && !this.Data[sheepPosition].IsProtected) continue;

                allBlocked =  false;
                sequences += FindPossibleSequencesDragon(dragonPosition, newSheep);
            }

            if (allBlocked)
            {
                sequences = FindPossibleSequencesDragon(dragonPosition, sheep);
            }

            cache[cacheKey] = sequences;
            return sequences;
        }

        long FindPossibleSequencesDragon(Vector2<int> dragonPosition, in Sheep sheep)
        {
            (Vector2<int>, Sheep, bool) cacheKey = (dragonPosition, sheep, true);
            if (cache.TryGetValue(cacheKey, out long sequences)) return sequences;

            sequences = 0L;
            foreach (Vector2<int> move in DragonMoves)
            {
                if (!this.Data.TryMoveWithinGrid(dragonPosition, move, out Vector2<int> newDragonPosition)) continue;

                Sheep newSheep = sheep;
                ref int sheepPosition = ref newSheep[newDragonPosition.X];
                if (sheepPosition == newDragonPosition.Y && !this.Data[newDragonPosition].IsProtected)
                {
                    sheepPosition = Sheep.DEAD;
                    if (newSheep.AllDead())
                    {
                        sequences++;
                        continue;
                    }
                }

                sequences += FindPossibleSequencesSheep(newDragonPosition, newSheep);
            }

            cache[cacheKey] = sequences;
            return sequences;
        }
    }
    // ReSharper enable CognitiveComplexity

    private int SimulateDragonMoves(DelayedGrid<bool> dragons)
    {
        int sheepCaptures = 0;
        foreach ((Vector2<int> position, bool isDragon) in dragons.EnumeratePositions())
        {
            if (!isDragon) continue;

            foreach (Vector2<int> dragonMove in DragonMoves)
            {
                if (!this.Data.TryMoveWithinGrid(position, dragonMove, out Vector2<int> move)) continue;

                dragons[move] = true;
                Cell cell = this.Data[move];
                if (!cell.IsSheep || cell.IsProtected) continue;

                sheepCaptures++;
                cell.Piece = Piece.EMPTY;
            }
        }

        dragons.Apply(clear: true);
        return sheepCaptures;
    }

    private int SimulateSheepMoves(DelayedGrid<bool> dragons)
    {
        int sheepCaptures = 0;
        for (int y = this.Data.Height - 1; y >= 0; y--)
        {
            for (int x = this.Data.Width - 1; x >= 0; x--)
            {
                Vector2<int> position = new(x, y);
                Cell cell = this.Data[position];
                if (!cell.IsSheep) continue;

                cell.Piece = Piece.EMPTY;
                if (!this.Data.TryMoveWithinGrid(position, Direction.DOWN, out Vector2<int> move)) continue;

                Cell newCell = this.Data[move];
                if (!newCell.IsProtected && dragons[move])
                {
                    sheepCaptures++;
                }
                else
                {
                    newCell.Piece = Piece.SHEEP;
                }
            }
        }

        return sheepCaptures;
    }

    /// <inheritdoc />
    protected override Cell[] LineConverter(string line) => line.Select(c => new Cell(c)).ToArray();
}
