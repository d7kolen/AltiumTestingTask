using Serilog;
using Serilog.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Altium.Core;

public class SegmentsMerger
{
    private readonly RowDtoComparer _comparer = new();

    private readonly string _fileResult;
    private readonly int _readingBufferSize;
    private readonly ILogger _logger;

    /// <summary>
    /// readingBufferSize defines a summary size of buffers for all reading files
    /// </summary>
    public SegmentsMerger(string fileResult, int readingBufferSize, ILogger logger)
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
        var actualList = new List<IEnumerator<RowDto>>();

        using var writer = new FileWriter(_fileResult);

        try
        {
            CreateInputStreams(files, bufferSize, fullInputList);

            actualList = fullInputList.Where(x => x.MoveNext()).ToList();            

            while (actualList.Any())
            {
                var tItem = actualList.Select(x => x.Current).Min(_comparer);
                await writer.WriteRowsAsync(new() { tItem });

                MoveNextItemList(actualList, tItem);
            }
        }
        finally
        {
            foreach (var t in fullInputList)
                t.Dispose();
        }

        _logger.Information("Finish merging {count} files", files.Count);
    }

    void MoveNextItemList(List<IEnumerator<RowDto>> actualList, RowDto tItem)
    {
        var tList = actualList.First(x => x.Current == tItem);
        if (!tList.MoveNext())
            actualList.Remove(tList);
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