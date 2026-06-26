using System;
using System.IO;
using Altium.Core;
using Altium.Core.Infrastructure;

var config = new ConfigLoader().Load();

using var logger = new LoggerFactory().CreateLogger(config["LogFolder"]!);

logger.Information("Start");

var tempFolder = config["TempFolder"]!;
if (Directory.Exists(tempFolder))
    Directory.Delete(tempFolder, true);

var resultFile = config["ResultFile"]!;
if (File.Exists(resultFile))
    File.Delete(resultFile);

var inputFile = config["InputFile"]!;

var sorter = new Sorter(tempFolder, logger);
sorter.Settings.MaxSegmentSize = 500_000;
sorter.Settings.ParallelSegmentSorting = 10;
sorter.Settings.ReadingBufferSize = 10_000_000;
sorter.Settings.SegmentsToMerge = 200;

try
{
    await sorter.SortAsync(inputFile, resultFile);
    logger.Information("Finish");
}
catch (Exception e)
{
    logger.Error(e, "Error");
}