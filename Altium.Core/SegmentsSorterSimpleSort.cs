using Serilog;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altium.Core.Async;
using Altium.Core.IO;
using Altium.Core.Row;

namespace Altium.Core;

public class SegmentsSorterSimpleSort
{
    private readonly RowDtoComparer _comparer;
    private readonly string _folder;
    private readonly SorterSettings _sorterSettings;
    private readonly ILogger _logger;

    /// <summary>
    /// segmentSize is approximately segment size. Usually, we will have a bigger segment on one additional block.
    /// </summary>
    public SegmentsSorterSimpleSort(string folder, SorterSettings sorterSettings, RowDtoComparer comparer,
        ILogger logger)
    {
        _folder = folder;
        _sorterSettings = sorterSettings;
        _comparer = comparer;
        _logger = logger;
    }

    public async Task<List<string>> CreateSegmentsAsync(IEnumerable<RowDto> rows)
    {
        List<RowDto> segmentRows = new(_sorterSettings.MaxSegmentSize);

        ConcurrentBag<string> result = new();
        int segmentNumber = 0;

        if (!Directory.Exists(_folder))
            Directory.CreateDirectory(_folder);

        _logger.Information("Start reading segments");

        await using (var flushTasks = new TaskSet(_sorterSettings.ParallelSegmentSorting, CancellationToken.None))
        {
            foreach (var t in rows)
            {
                segmentRows.Add(t);

                if (segmentRows.Count > _sorterSettings.MaxSegmentSize)
                {
                    _logger.Information("Segment {number} was prepared", segmentNumber);

                    var tSegmentRows = segmentRows;
                    segmentRows = new(_sorterSettings.MaxSegmentSize);
                    var tSegmentNumber = segmentNumber++;

                    await flushTasks.WaitAndAdd(() => FlushSegmentAsync(tSegmentRows, tSegmentNumber, result));
                }
            }

            if (segmentRows.Any())
                await flushTasks.WaitAndAdd(() => FlushSegmentAsync(segmentRows, segmentNumber, result));
        }

        return result.ToList();
    }

    private async Task<string> FlushSegmentAsync(List<RowDto> segmentRows, int segmentNumber,
        ConcurrentBag<string> result)
    {
        // The method returns immediately, while the remaining work is performed asynchronously.
        // This allows us to use the async/await syntax to create new asynchronous tasks.
        await Task.Yield();

        _logger.Information("Sorting segment {number}", segmentNumber);

        segmentRows.Sort(_comparer);

        _logger.Information("Sorted segment {number}", segmentNumber);

        var segmentFileName = SaveSegment(segmentRows, segmentNumber);

        _logger.Information("Wrote segment {number} to file", segmentNumber);

        result.Add(segmentFileName);

        return segmentFileName;
    }

    private string SaveSegment(List<RowDto> segmentRows, int segmentNumber)
    {
        string fileName = SegmentFileName(segmentNumber);
        using var writer = new FileWriter(fileName);

        foreach (var t in segmentRows)
            writer.WriteRow(t);

        return fileName;
    }

    private string SegmentFileName(int segmentNumber)
    {
        var folder = _folder;

        //Protecting the file system from a huge count of files in one directory
        var subfolderId = segmentNumber / 100;
        if (subfolderId > 0)
        {
            folder = Path.Combine(_folder, subfolderId.ToString());
            Directory.CreateDirectory(folder);
        }

        return Path.Combine(folder, segmentNumber.ToString() + ".txt");
    }
}