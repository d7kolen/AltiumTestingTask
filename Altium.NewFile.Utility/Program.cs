using Altium.Core;
using Serilog;

//await new FileWriter(@"c:\Temp\Altium\input.txt").WriteRandomRowsAsync(100_000_000);

using var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(
        $@"c:\Temp\Altium\logs\log-{DateTime.UtcNow:yyyy-MM-dd_hh-mm-ss}.txt",
        rollingInterval: RollingInterval.Infinite,
        retainedFileCountLimit: 5)
     .WriteTo.Console()
    .CreateLogger();

logger.Information("Start");

var sorter = new Sorter(@"c:\Temp\Altium\temp", logger);
//sorter.ReadingBufferSize = 100_000_000;
sorter.InitSegmentSize = 10_000_000;
sorter.SegmentsToMerge = 5;

await sorter.SortAsync(@"c:\Temp\Altium\input.txt", @"c:\Temp\Altium\result.txt");

logger.Information("Finish");