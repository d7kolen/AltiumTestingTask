using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Altium.Core;

public class SegmentsMerger_DirectMininum
{
    private readonly EnumeratorRowDtoComparer _comparer = new();

    private readonly string _fileResult;
    private readonly int _readingBufferSize;
    private readonly ILogger _logger;

    /// <summary>
    /// readingBufferSize defines a summary size of buffers for all reading files
    /// </summary>
    public SegmentsMerger_DirectMininum(string fileResult, int readingBufferSize, ILogger logger)
    {
        _fileResult = fileResult;
        _readingBufferSize = readingBufferSize;
        _logger = logger;
    }

    public async Task MergeSegmentsAsync(List<string> files)
    {
        _logger.Information("Start merging {count} files", files.Count);

        var bufferSize = _readingBufferSize / files.Count;

        var fullInputList = new List<IEnumerator<RowDto>>();

        using var writer = new FileWriter(_fileResult);

        try
        {
            CreateInputStreams(files, bufferSize, fullInputList);

            var acutualList = fullInputList.Where(x => x.MoveNext()).ToList();

            while (acutualList.Any())
            {
                var min = acutualList.Min(_comparer);
                writer.WriteRow(min.Current);

                MoveNext(acutualList, min);
            }
        }
        finally
        {
            foreach (var t in fullInputList)
                t.Dispose();
        }

        _logger.Information("Finish merging {count} files", files.Count);
    }

    void MoveNext(List<IEnumerator<RowDto>> list, IEnumerator<RowDto> minItem)
    {
        if (!minItem.MoveNext())
            list.Remove(minItem);
    }

    void CreateInputStreams(List<string> files, int bufferSize, List<IEnumerator<RowDto>> fullList)
    {
        foreach (var t in files)
        {
            var tInput = new FileReader(t, bufferSize).Read().GetEnumerator();
            fullList.Add(tInput);
        }
    }
}

class EnumeratorRowDtoComparer : IComparer<IEnumerator<RowDto>>
{
    private readonly IComparer<RowDto> _inner = new RowDtoComparer();

    public int Compare(IEnumerator<RowDto> x, IEnumerator<RowDto> y)
    {
        return _inner.Compare(x.Current, y.Current);
    }
}