using System.Runtime.CompilerServices;
using Challenge.Solvers;
using Challenge.Utils.Extensions.Numbers;

namespace EverybodyCodes.EC2024;

/// <summary>
/// Solver for 2024 Day 8
/// </summary>
[Solver(2024, 8)]
public sealed partial class Day08 : Solver<int>, IEverybodyCodesSolver
{
    // ReSharper disable CognitiveComplexity
    /// <inheritdoc />
    [Part(1)]
    public void RunPart1()
    {
        int layers = 1;
        int shrineSize;
        do
        {
            layers++;
            shrineSize = layers.Triangular + layers.PreviousTriangular;
        }
        while (shrineSize < this.Data);

        int missing = shrineSize - this.Data;
        int width = (layers * 2) - 1;
        LogAnswer(missing * width);
    }

    /// <inheritdoc />
    [Part(2)]
    public void RunPart2()
    {
        const int ACOLYTES = 1111;
        const int TOTAL_BLOCKS = 20240000;

        int thickness = 1;
        int width = 1;
        int blocks = 1;
        do
        {
            width += 2;
            thickness = (thickness * this.Data) % ACOLYTES;
            blocks += width * thickness;
        }
        while (blocks < TOTAL_BLOCKS);

        int missing = blocks - TOTAL_BLOCKS;
        LogAnswer(missing * width);
    }

    /// <inheritdoc />
    [Part(3)]
    public void RunPart3()
    {
        const int ACOLYTES = 10;
        const int TOTAL_BLOCKS = 202400000;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int GetColumnBlocks(int priests, int width, int columnHeight) => columnHeight - (int)(((long)priests * width * columnHeight) % ACOLYTES);

        int blocks;
        int layers = 1;
        int width = 1;
        List<int> layerHeights = new(4096) { 1 };
        do
        {
            layerHeights.Add(((layerHeights[^1] * this.Data) % ACOLYTES) + ACOLYTES);

            layers++;
            width += 2;

            // Setup last layer, no removals
            int columnHeight = layerHeights[^1];
            blocks = columnHeight * 2;

            // Skip the last layer (structural), skip the first layer (not doubled)
            for (int layer = layers - 2; layer > 0; layer--)
            {
                columnHeight += layerHeights[layer];
                blocks += GetColumnBlocks(this.Data, width, columnHeight) * 2;
            }

            // First layer, not doubled
            columnHeight += layerHeights[0];
            blocks += GetColumnBlocks(this.Data, width, columnHeight);
        }
        while (blocks < TOTAL_BLOCKS);

        long missing = blocks - TOTAL_BLOCKS;
        LogAnswer(missing);
    }
    // ReSharper enable CognitiveComplexity

    /// <inheritdoc />
    protected override int Convert(string[] rawInput) => int.Parse(rawInput[0]);
}
