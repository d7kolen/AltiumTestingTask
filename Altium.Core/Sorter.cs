using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Altium.Core;

public class Sorter
{
    private readonly string _resultFileName;
    private readonly string _tempFolder;

    public int InitSegmentSize { get; set; } = 10_000_000;
    public int ReadingBufferSize { get; set; } = 10_000_000;
    public int SegmentsToMerge { get; set; } = 2;

    public Sorter(string resultFileName, string tempFolder)
    {
        _resultFileName = resultFileName;
        _tempFolder = tempFolder;
    }

    public async Task SortAsync(string inputFileName)
    {
        var inputRows = new FileReader(inputFileName, ReadingBufferSize).Read();

        var segmentsFolder = Path.Combine(_tempFolder, "segments");
        var segments = await new SegmentsSorter(segmentsFolder, InitSegmentSize).CreateSegmentsAsync(inputRows);

        if (segments.Count == 1)
            File.Copy(segments[0], _resultFileName, true);
        else
            await MergeSegments(segments);

        Directory.Delete(_tempFolder, true);
    }

    private async Task MergeSegments(List<string> segments)
    {
        var mergedFolder = Path.Combine(_tempFolder, "merged");
        Directory.CreateDirectory(mergedFolder);

        int mergeCounter = 0;

        while (segments.Count > 1)
        {
            var toMerge = segments.Take(SegmentsToMerge).ToList();

            var resultFile = _resultFileName;
            if (segments.Count != toMerge.Count)
                resultFile = Path.Combine(mergedFolder, $"{++mergeCounter}.txt");

            await new SegmentsMerger(resultFile, ReadingBufferSize).MergeSegmentsAsync(toMerge);

            segments.RemoveRange(0, toMerge.Count);
            foreach (var t in toMerge)
                File.Delete(t);

            segments.Add(resultFile);
        }
    }
}