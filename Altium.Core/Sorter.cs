using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Altium.Core.IO;
using Altium.Core.Row;

namespace Altium.Core;

public class Sorter
{
    private readonly RowDtoComparer _comparer = new();
    private readonly string _tempFolder;
    private readonly ILogger _logger;

    public SorterSettings Settings { get; } = new();

    public Sorter(string tempFolder, ILogger logger)
    {
        _tempFolder = tempFolder;
        _logger = logger;
    }

    public async Task SortAsync(string inputFileName, string resultFileName)
    {
        var inputRows = new FileReader(inputFileName, Settings.ReadingBufferSize).Read();

        var segmentsSorter = new SegmentsSorterSimpleSort(
            Path.Combine(_tempFolder, "segments"), Settings, _comparer, _logger);

        var segments = await segmentsSorter.CreateSegmentsAsync(inputRows);

        if (segments.Count == 1)
            File.Copy(segments[0], resultFileName, true);
        else
            MergeSegments(segments, resultFileName);

        Directory.Delete(_tempFolder, true);
    }

    private void MergeSegments(List<string> segments, string resultFileName)
    {
        var mergedFolder = Path.Combine(_tempFolder, "merged");
        Directory.CreateDirectory(mergedFolder);

        int mergeCounter = 0;

        while (segments.Count > 1)
        {
            var toMerge = segments.Take(Settings.SegmentsToMerge).ToList();

            var resultFile = Path.Combine(mergedFolder, $"{++mergeCounter}.txt");
            new SegmentsMergerBTree(resultFile, Settings.ReadingBufferSize, _comparer, _logger).MergeSegments(toMerge);

            segments.RemoveRange(0, toMerge.Count);
            foreach (var t in toMerge)
                File.Delete(t);

            segments.Add(resultFile);
        }

        File.Move(segments[0], resultFileName);
    }
}