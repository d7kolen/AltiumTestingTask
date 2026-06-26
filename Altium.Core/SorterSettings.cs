namespace Altium.Core;

public class SorterSettings
{
    public int MaxSegmentSize { get; set; } = 10_000_000;
    public int ParallelSegmentSorting { get; set; } = 2;
    public int ReadingBufferSize { get; set; } = 10_000_000;
    public int SegmentsToMerge { get; set; } = 2;
}