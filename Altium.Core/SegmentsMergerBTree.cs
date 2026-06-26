using System;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using Altium.Core.IO;
using Altium.Core.Row;

namespace Altium.Core;

public class SegmentsMergerBTree
{
    private readonly RowDtoComparer _comparer = new();

    private readonly string _fileResult;
    private readonly int _readingBufferSize;
    private readonly ILogger _logger;

    /// <summary>
    /// readingBufferSize defines summarize the buffer size for all opened files
    /// </summary>
    public SegmentsMergerBTree(string fileResult, int readingBufferSize, ILogger logger)
    {
        _fileResult = fileResult;
        _readingBufferSize = readingBufferSize;
        _logger = logger;
    }

    public void MergeSegments(List<string> files)
    {
        _logger.Information("Start merging {count} files", files.Count);

        var fullInputList = new List<IEnumerator<RowDto>>();

        using var writer = new FileWriter(_fileResult);

        try
        {
            OpenInputStreams(files, fullInputList);

            var allActualInputs = CreateInputTree(fullInputList);
            while (allActualInputs != null)
            {
                var min = allActualInputs.Min();
                writer.WriteRow(min.Current.Current);

                MoveNext(ref allActualInputs);
            }
        }
        finally
        {
            foreach (var t in fullInputList)
                t.Dispose();
        }

        _logger.Information("Finish merging {count} files", files.Count);
    }

    private RowDtoBTree? CreateInputTree(List<IEnumerator<RowDto>> allInputs)
    {
        RowDtoBTree? allInputsTree = null;

        var allActualInputs = allInputs.Where(x => x.MoveNext()).ToList();
        foreach (var t in allActualInputs)
            allInputsTree = RowDtoBTree.Add(allInputsTree, t, _comparer);

        return allInputsTree;
    }

    /// <summary>
    /// Move next the minimal value and rebalance the tree
    /// </summary>
    void MoveNext(ref RowDtoBTree? list)
    {
        if (list == null)
            throw new ArgumentNullException($"{nameof(list)} is null");

        var minItem = list.Min().Current;
        list = RowDtoBTree.RemoveMin(list);

        if (minItem.MoveNext())
            list = RowDtoBTree.Add(list, minItem, _comparer);
    }

    void OpenInputStreams(List<string> files, List<IEnumerator<RowDto>> fullList)
    {
        // Using the fullList parameter instead of return helps as to dispose all opened streams,
        // despite we finished the function and stream opening or not

        var bufferSizeOfSingleFile = _readingBufferSize / files.Count;

        foreach (var t in files)
        {
            var tInput = new FileReader(t, bufferSizeOfSingleFile).Read().GetEnumerator();
            fullList.Add(tInput);
        }
    }
}