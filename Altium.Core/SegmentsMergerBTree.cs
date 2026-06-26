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
    /// readingBufferSize defines a summary size of buffers for all reading files
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

        var bufferSize = _readingBufferSize;

        var fullInputList = new List<IEnumerator<RowDto>>();

        using var writer = new FileWriter(_fileResult);

        try
        {
            CreateInputStreams(files, bufferSize, fullInputList);

            var acutualList = fullInputList.Where(x => x.MoveNext()).ToList();

            RowDtoBTree? actualTree = null;
            foreach (var t in acutualList)
                actualTree = RowDtoBTree.Add(actualTree, t, _comparer);

            while (actualTree != null)
            {
                var min = actualTree.Min();
                writer.WriteRow(min.Current.Current);

                MoveNext(ref actualTree);
            }
        }
        finally
        {
            foreach (var t in fullInputList)
                t.Dispose();
        }

        _logger.Information("Finish merging {count} files", files.Count);
    }

    void MoveNext(ref RowDtoBTree? list)
    {
        if (list == null)
            throw new ArgumentNullException($"{nameof(list)} is null");

        var minItem = list.Min().Current;
        list = RowDtoBTree.RemoveMin(list);

        if (minItem.MoveNext())
            list = RowDtoBTree.Add(list, minItem, _comparer);
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