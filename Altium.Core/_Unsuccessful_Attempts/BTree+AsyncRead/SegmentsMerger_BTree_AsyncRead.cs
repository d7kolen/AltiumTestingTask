using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Altium.Core;

public class SegmentsMerger_BTree_AsyncRead
{
    private readonly RowDtoComparer _comparer = new();

    private readonly string _fileResult;
    private readonly int _readingBufferSize;
    private readonly int _readingBatchSize;
    private readonly ILogger _logger;

    /// <summary>
    /// readingBufferSize defines a summary size of buffers for all reading files
    /// </summary>
    public SegmentsMerger_BTree_AsyncRead(string fileResult, int readingBufferSize, int readingBatchSize, ILogger logger)
    {
        _fileResult = fileResult;
        _readingBufferSize = readingBufferSize;
        _readingBatchSize = readingBatchSize;
        _logger = logger;
    }

    public async Task MergeSegmentsAsync(List<string> files)
    {
        _logger.Information("Start merging {count} files", files.Count);

        var bufferSize = _readingBufferSize / files.Count;

        List<IAsyncEnumerator<RowDto>> fullInputList = new();

        using var writer = new FileWriter(_fileResult);

        try
        {
            CreateInputStreams(files, bufferSize, fullInputList);

            List<IAsyncEnumerator<RowDto>> acutualList = new();
            foreach (var t in fullInputList)
                if (await t.MoveNextAsync())
                    acutualList.Add(t);

            RowDtoBTree_AsyncRead actualTree = null;
            foreach (var t in acutualList)
                actualTree = RowDtoBTree_AsyncRead.Add(actualTree, t, _comparer);

            while (actualTree != null)
            {
                var min = actualTree.Min();
                writer.WriteRows(new() { min.Current.Current });

                actualTree = await MoveNext(actualTree);
            }
        }
        finally
        {
            foreach (var t in fullInputList)
                await t.DisposeAsync();
        }

        _logger.Information("Finish merging {count} files", files.Count);
    }

    async Task<RowDtoBTree_AsyncRead> MoveNext(RowDtoBTree_AsyncRead list)
    {
        var minItem = list.Min().Current;
        list = RowDtoBTree_AsyncRead.RemoveMin(list);

        if (await minItem.MoveNextAsync())
            list = RowDtoBTree_AsyncRead.Add(list, minItem, _comparer);

        return list;
    }

    void CreateInputStreams(List<string> files, int bufferSize, List<IAsyncEnumerator<RowDto>> fullList)
    {
        foreach (var t in files)
        {
            var tInput = Simplify(new FileReader_Async(t, bufferSize).ReadAsync(_readingBatchSize)).GetAsyncEnumerator();
            fullList.Add(tInput);
        }
    }

    async IAsyncEnumerable<RowDto> Simplify(IAsyncEnumerable<List<RowDto>> list)
    {
        await foreach (var tSet in list)
            foreach (var t in tSet)
                yield return t;
    }
}