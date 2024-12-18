﻿using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Altium.Core;

public class Sorter
{
    private readonly string _tempFolder;
    private readonly ILogger _logger;

    public int InitSegmentSize { get; set; } = 10_000_000;
    public int ReadingBufferSize { get; set; } = 10_000_000;
    public int SegmentsToMerge { get; set; } = 2;
    public int SegmentsParallelize { get; set; } = 2;
    public int MergeParallelize { get; set; } = 2;

    public Sorter(string tempFolder, ILogger logger)
    {
        _tempFolder = tempFolder;
        _logger = logger;
    }

    public async Task SortAsync(string inputFileName, string resultFileName)
    {
        var inputRows = new FileReader(inputFileName, ReadingBufferSize).Read();

        var segmentsSorter = new SegmentsSorter_SimpleSort(Path.Combine(_tempFolder, "segments"), InitSegmentSize, SegmentsParallelize, _logger);
        var segments = await segmentsSorter.CreateSegmentsAsync(inputRows);

        if (segments.Count == 1)
            File.Copy(segments[0], resultFileName, true);
        else
            await MergeSegments(segments, resultFileName);

        Directory.Delete(_tempFolder, true);
    }

    private async Task MergeSegments(List<string> segments, string resultFileName)
    {
        var mergedFolder = Path.Combine(_tempFolder, "merged");
        Directory.CreateDirectory(mergedFolder);

        int mergeCounter = 0;

        while (segments.Count > 1)
        {
            var toMerge = segments.Take(SegmentsToMerge).ToList();
            
            var resultFile = Path.Combine(mergedFolder, $"{++mergeCounter}.txt");

            await
                new SegmentsMerger_BTree(resultFile, ReadingBufferSize, _logger)
                .MergeSegmentsAsync(toMerge);

            segments.RemoveRange(0, toMerge.Count);
            foreach (var t in toMerge)
                File.Delete(t);

            segments.Add(resultFile);
        }

        File.Move(segments[0], resultFileName);
    }
}