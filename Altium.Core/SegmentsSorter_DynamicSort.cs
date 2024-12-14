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
        RowDtoSorting segmentRows = null;
        int segmentSize = 0;

        List<string> result = new();
        int segmentIndex = 0;

        if (!Directory.Exists(_folder))
            Directory.CreateDirectory(_folder);

        _logger.Information("Start reading segments");

        var flushTasks = new List<Task>();

        foreach (var t in rows)
        {
            segmentRows = new RowDtoSorting(t, segmentRows);
            segmentSize++;

            if (segmentSize > _maxSegmentSize)
            {
                _logger.Information("Segment {number} prepared", segmentIndex);

                var tSegmentRows = segmentRows;
                segmentRows = null;
                segmentSize = 0;
                var tSegmentIndex = segmentIndex++;

                flushTasks = await AwaitFreeFlushSlot(flushTasks);
                flushTasks.Add(FlushSegmentAsync(tSegmentRows, tSegmentIndex, result));
            }
        }

        if (segmentRows != null)
        {
            flushTasks = await AwaitFreeFlushSlot(flushTasks);
            flushTasks.Add(FlushSegmentAsync(segmentRows, segmentIndex, result));
        }

        await Task.WhenAll(flushTasks);

        return result;
    }

    private async Task FlushSegmentAsync(RowDtoSorting segmentRows, int segmentNumber, List<string> result)
    {
        //gives the calling thread a green light
        await Task.Yield();

        _logger.Information("Sorting segment {number}", segmentNumber);

        segmentRows = RowDtoSorting.Sort(segmentRows, _comparer);

        _logger.Information("Sorted segment {number}", segmentNumber);

        var segmentFile = WriteSegment(segmentRows, SegmentFileName(segmentNumber));

        _logger.Information("Wrote segment {number} to file", segmentNumber);

        lock (result)
            result.Add(segmentFile);
    }

    private string WriteSegment(RowDtoSorting segment, string segmentFileName)
    {
        using var writer = new FileWriter(segmentFileName);

        while (segment != null)
        {
            writer.WriteRow(segment.Data);
            segment = segment.Bigger;
        }

        return segmentFileName;
    }

    private string SegmentFileName(int segmentNumber)
    {
        var tFolder = _folder;

        //Protecting the file system from a huge amount of files in one directory
        var subfolderId = segmentNumber / 100;
        if (subfolderId > 0)
        {
            tFolder = Path.Combine(_folder, subfolderId.ToString());
            Directory.CreateDirectory(tFolder);
        }

        return Path.Combine(tFolder, segmentNumber.ToString() + ".txt");
    }

    private async Task<List<Task>> AwaitFreeFlushSlot(List<Task> flashTasks)
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
