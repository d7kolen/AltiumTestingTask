using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Altium.Core;

public class SegmentsSorter
{
    private readonly RowDtoComparer _comparer = new();
    private readonly string _folder;
    private readonly int _maxSegmentSize;

    /// <summary>
    /// segmentSize is approximately segment size. Usually, we will have a bigger segment on one additional block.
    /// </summary>
    public SegmentsSorter(string folder, int maxSegmentSize)
    {
        _folder = folder;
        _maxSegmentSize = maxSegmentSize;
    }

    public async Task<List<string>> CreateSegmentFiles(IEnumerable<RowDto> rows)
    {
        var currentSegmentSize = 0;
        List<RowDto> segmentRows = new();

        List<string> result = new();

        if (!Directory.Exists(_folder))
            Directory.CreateDirectory(_folder);

        foreach (var t in rows)
        {
            currentSegmentSize += sizeof(int) + t.StringValue.Length;
            segmentRows.Add(t);

            if (currentSegmentSize > _maxSegmentSize)
            {
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
        segmentRows.Sort(_comparer);

        var segmentFileName = Path.Combine(_folder, segmentNumber.ToString() + ".txt");
        using var writer = new FileWriter(segmentFileName);
        await writer.WriteRowsAsync(segmentRows);

        return segmentFileName;
    }
}