using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Altium.Core;

public class SegmentsSorter
{
    private readonly RowDtoComparer _comparer = new();
    private readonly string _folder;
    private readonly int _maxSegmentSize;
    private readonly int _parallelSorting;
    private readonly ILogger _logger;

    /// <summary>
    /// segmentSize is approximately segment size. Usually, we will have a bigger segment on one additional block.
    /// </summary>
    public SegmentsSorter(string folder, int maxSegmentSize, int parallelSorting, ILogger logger)
    {
        _folder = folder;
        _maxSegmentSize = maxSegmentSize;
        _parallelSorting = parallelSorting <= 0 ? 1 : parallelSorting;
        _logger = logger;
    }

    public async Task<List<string>> CreateSegmentsAsync(IEnumerable<RowDto> rows)
    {
        List<RowDto> segmentRows = new(_maxSegmentSize);

        List<string> result = new();
        int segmentIndex = 0;

        if (!Directory.Exists(_folder))
            Directory.CreateDirectory(_folder);

        _logger.Information("Start reading segments");

        var flushTasks = new List<Task>();

        foreach (var t in rows)
        {
            segmentRows.Add(t);

            if (segmentRows.Count > _maxSegmentSize)
            {
                _logger.Information("Segment {number} prepared", segmentIndex);

                var tSegmentRows = segmentRows;
                segmentRows = new(_maxSegmentSize);
                var tSegmentIndex = segmentIndex++;

                flushTasks = await AwaitEmptyFlushSlot(flushTasks);
                flushTasks.Add(StartFlushSegmentTask(tSegmentRows, result, tSegmentIndex));
            }
        }

        flushTasks = await AwaitEmptyFlushSlot(flushTasks);
        flushTasks.Add(StartFlushSegmentTask(segmentRows, result, segmentIndex));

        await Task.WhenAll(flushTasks);

        return result;
    }

    private string FlushSegment(List<RowDto> segmentRows, int segmentNumber)
    {
        _logger.Information("Sorting segment {number}", segmentNumber);

        segmentRows.Sort(_comparer);

        _logger.Information("Sorted segment {number}", segmentNumber);

        var folder = _folder;

        //Protecting the file system from a huge amount of files in one directory
        var subfolderId = segmentNumber / 100;
        if (subfolderId > 0)
        {
            folder = Path.Combine(_folder, subfolderId.ToString());
            Directory.CreateDirectory(folder);
        }

        var segmentFileName = Path.Combine(folder, segmentNumber.ToString() + ".txt");
        using var writer = new FileWriter(segmentFileName);
        writer.WriteRows(segmentRows);

        _logger.Information("Wrote segment {number} to file", segmentNumber);

        return segmentFileName;
    }

    private async Task StartFlushSegmentTask(List<RowDto> segmentRows, List<string> result, int segmentIndex)
    {
        //gives the calling thread a green light
        await Task.Yield();

        var segmentFile = FlushSegment(segmentRows, segmentIndex);
        lock (result)
            result.Add(segmentFile);
    }

    private async Task<List<Task>> AwaitEmptyFlushSlot(List<Task> flashTasks)
    {
        flashTasks = flashTasks.Where(x => !x.IsCompleted).ToList();

        while (flashTasks.Count >= _parallelSorting)
        {
            await Task.WhenAny(flashTasks);
            flashTasks = flashTasks.Where(x => !x.IsCompleted).ToList();
        }

        return flashTasks;
    }
}
