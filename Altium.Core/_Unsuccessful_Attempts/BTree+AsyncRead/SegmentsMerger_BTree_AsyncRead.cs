using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Altium.Core;

public class SegmentsMerger_BTree_AsyncRead
{
    private readonly RowDtoComparer _comparer = new();

    private readonly string _fileResult;
    private readonly int _readingBufferSize;
    private readonly int _preloadReadBugger;
    private readonly ILogger _logger;

    /// <summary>
    /// readingBufferSize defines a summary size of buffers for all reading files
    /// </summary>
    public SegmentsMerger_BTree_AsyncRead(string fileResult, int readingBufferSize, int preloadReadBugger, ILogger logger)
    {
        _fileResult = fileResult;
        _readingBufferSize = readingBufferSize;
        _preloadReadBugger = preloadReadBugger;
        _logger = logger;
    }

    public async Task MergeSegmentsAsync(List<string> files)
    {
        _logger.Information("Start merging {count} files", files.Count);

        var bufferSize = _readingBufferSize / files.Count;

        var fullInputList = new List<IAsyncEnumerator<RowDto>>();

        using var writer = new FileWriter(_fileResult);

        try
        {
            CreateInputStreams(files, bufferSize, fullInputList);

            RowDtoBTree_AsyncRead actualTree = null;
            foreach (var t in fullInputList)
                if (await t.MoveNextAsync())
                    actualTree = RowDtoBTree_AsyncRead.Add(actualTree, t, _comparer);

            while (actualTree != null)
            {
                var min = actualTree.Min();
                await writer.WriteRowsAsync(new() { min.Current.Current });

                actualTree = await MoveNextAsync(actualTree);
            }
        }
        finally
        {
            foreach (var t in fullInputList)
                await t.DisposeAsync();
        }

        _logger.Information("Finish merging {count} files", files.Count);
    }

    async Task<RowDtoBTree_AsyncRead> MoveNextAsync(RowDtoBTree_AsyncRead tree)
    {
        var minItem = tree.Min().Current;
        tree = RowDtoBTree_AsyncRead.RemoveMin(tree);

        if (await minItem.MoveNextAsync())
            return RowDtoBTree_AsyncRead.Add(tree, minItem, _comparer);

        return tree;
    }

    void CreateInputStreams(List<string> files, int bufferSize, List<IAsyncEnumerator<RowDto>> fullList)
    {
        foreach (var t in files)
        {
            var tInput = new FileReader(t, bufferSize).ReadAsync(_preloadReadBugger).GetAsyncEnumerator();
            fullList.Add(tInput);
        }
    }
}