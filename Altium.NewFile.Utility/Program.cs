using Altium.Core;

//await new FileWriter(@"c:\Temp\Altium\input.txt").WriteRandomRowsAsync(100_000_000);

var sorter = new Sorter(@"c:\Temp\Altium\result.txt", @"c:\Temp\Altium\temp");
sorter.ReadingBufferSize = 100_000_000;
sorter.InitSegmentSize = 100_000_000;
sorter.SegmentsToMerge = 5;

await sorter.SortAsync(@"c:\Temp\Altium\input.txt");