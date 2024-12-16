using Serilog;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Altium.Core;

public class SegmentsSorter_SimpleSort
{
    private readonly RowDtoComparer _comparer = new();
    private readonly string _folder;
    private readonly int _maxSegmentSize;
    private readonly int _parallelSorting;
    private readonly ILogger _logger;

    /// <summary>
    /// segmentSize is approximately segment size. Usually, we will have a bigger segment on one additional block.
    /// </summary>
    public SegmentsSorter_SimpleSort(string folder, int maxSegmentSize, int parallelSorting, ILogger logger)
    {
        _folder = folder;
        _maxSegmentSize = maxSegmentSize;
        _parallelSorting = parallelSorting <= 0 ? 1 : parallelSorting;
        _logger = logger;
    }

    public async Task<List<string>> CreateSegmentsAsync(IEnumerable<RowDto> rows)
    {
        List<RowDto> segmentRows = new(_maxSegmentSize);

        ConcurrentBag<string> result = new();
        int segmentNumber = 0;

        if (!Directory.Exists(_folder))
            Directory.CreateDirectory(_folder);

        _logger.Information("Start reading segments");

        await using (var flushTasks = new TaskSet(_parallelSorting, CancellationToken.None))
        {
            foreach (var t in rows)
            {
                segmentRows.Add(t);

                if (segmentRows.Count > _maxSegmentSize)
                {
                    _logger.Information("Segment {number} prepared", segmentNumber);

                    var tSegmentRows = segmentRows;
                    segmentRows = new(_maxSegmentSize);
                    var tSegmentNumber = segmentNumber++;

                    await flushTasks.WaitAndAdd(() => FlushSegmentAsync(tSegmentRows, tSegmentNumber, result));
                }
            }

            if (segmentRows.Any())
                await flushTasks.WaitAndAdd(() => FlushSegmentAsync(segmentRows, segmentNumber, result));
        }

        return result.ToList();
    }

    private async Task<string> FlushSegmentAsync(List<RowDto> segmentRows, int segmentNumber, ConcurrentBag<string> result)
    {
        //gives the calling thread a green light
        await Task.Yield();

        _logger.Information("Sorting segment {number}", segmentNumber);

        segmentRows.Sort(_comparer);

        _logger.Information("Sorted segment {number}", segmentNumber);

        string segmentFileName = SegmentFileName(segmentNumber);
        using var writer = new FileWriter(segmentFileName);
        foreach (var t in segmentRows)
            writer.WriteRow(t);

        _logger.Information("Wrote segment {number} to file", segmentNumber);

        result.Add(segmentFileName);

        return segmentFileName;
    }

    private string SegmentFileName(int segmentNumber)
    {
        var folder = _folder;

        //Protecting the file system from a huge amount of files in one directory
        var subfolderId = segmentNumber / 100;
        if (subfolderId > 0)
        {
            folder = Path.Combine(_folder, subfolderId.ToString());
            Directory.CreateDirectory(folder);
        }

        return Path.Combine(folder, segmentNumber.ToString() + ".txt");
    }
}