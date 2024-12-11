using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Altium.Core;

public class SegmentsSorter
{
    private readonly RowDtoComparer _comparer = new();
    private readonly string _folder;
    private readonly int _maxSegmentSize;
    private readonly ILogger _logger;

    /// <summary>
    /// segmentSize is approximately segment size. Usually, we will have a bigger segment on one additional block.
    /// </summary>
    public SegmentsSorter(string folder, int maxSegmentSize, ILogger logger)
    {
        _folder = folder;
        _maxSegmentSize = maxSegmentSize;
        _logger = logger;
    }

    public async Task<List<string>> CreateSegmentsAsync(IEnumerable<RowDto> rows)
    {
        var currentSegmentSize = 0;
        List<RowDto> segmentRows = new();

        List<string> result = new();

        if (!Directory.Exists(_folder))
            Directory.CreateDirectory(_folder);

        _logger.Information("Start reading segments");

        foreach (var t in rows)
        {
            currentSegmentSize += sizeof(int) + t.StringValue.Length;
            segmentRows.Add(t);

            if (currentSegmentSize > _maxSegmentSize)
            {
                _logger.Information("Segment {number} prepared", result.Count);

                var segmentFile = await FlushSegment(segmentRows, result.Count);

                segmentRows.Clear();
                currentSegmentSize = 0;
                result.Add(segmentFile);
            }
        }

        result.Add(await FlushSegment(segmentRows, result.Count));

        return result;
    }

    private async Task<string> FlushSegment(List<RowDto> segmentRows, int segmentNumber)
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
        await writer.WriteRowsAsync(segmentRows);

        _logger.Information("Wrote segment {number} to file", segmentNumber);

        return segmentFileName;
    }
}