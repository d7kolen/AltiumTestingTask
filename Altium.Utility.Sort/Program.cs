﻿using Altium.Core;
using System.Runtime;

GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

var config = new ConfigLoader().Load();

using var logger = new LoggerFactory().CreateLogger(config["LogFolder"]);

logger.Information("Start");

var tempFolder = config["TempFolder"];
if (Directory.Exists(tempFolder))
    Directory.Delete(tempFolder, true);

var resultFile = config["ResultFile"];
if (File.Exists(resultFile))
    File.Delete(resultFile);

var inputFile = config["InputFile"];

var sorter = new Sorter(tempFolder, logger);
sorter.InitSegmentSize = 20_000_000;
sorter.ReadingBufferSize = 100_000_000;
sorter.SegmentsToMerge = 200;
sorter.SegmentsParallelize = 5;

await sorter.SortAsync(inputFile, resultFile);

logger.Information("Finish");