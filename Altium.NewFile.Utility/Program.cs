using Altium.Core;
using Serilog;

using var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(
        $@"c:\Temp\Altium\logs\log-{DateTime.UtcNow:yyyy-MM-dd_hh-mm-ss}.txt",
        rollingInterval: RollingInterval.Infinite)
     .WriteTo.Console()
    .CreateLogger();

logger.Information("Start");



//using var writer = new FileWriter(@"c:\Temp\Altium\input.txt");
//await writer.WriteRandomRowsAsync(50_000_000, logger); //1GB
//await writer.WriteRandomRowsAsync(500_000_000, logger); //1GB


var sorter = new Sorter(@"c:\Temp\Altium\temp", logger);
sorter.InitSegmentSize = 10_000_000;
sorter.ReadingBufferSize = 100_000_000;
sorter.SegmentsToMerge = 200;
sorter.SegmentsParallelize = 30;

await sorter.SortAsync(@"c:\Temp\Altium\input-1gb.txt", @"c:\Temp\Altium\result.txt");



logger.Information("Finish");