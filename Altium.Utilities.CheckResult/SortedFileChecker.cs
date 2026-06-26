using Altium.Core.IO;
using Altium.Core.Row;
using Serilog;

namespace Altium.Utilities.CheckResult;

public class SortedFileChecker
{
    private const int MaxLoggedViolations = 100;
    private const int MaxLoggedStatisticsDifferences = 100;

    private readonly RowDtoAlphabet _alphabet = new();
    private readonly RowDtoComparer _comparer = new();
    private readonly ILogger _logger;

    public SortedFileChecker(ILogger logger)
    {
        _logger = logger;
    }

    public SortedFileCheckResult Check(string originalFileName, string resultFileName)
    {
        return new SortedFileCheckResult(
            CheckOrdering(resultFileName),
            CompareNumberStatistics(originalFileName, resultFileName));
    }

    private bool CheckOrdering(string inputFileName)
    {
        using var rows = new FileReader(inputFileName, 1_000_000).Read().GetEnumerator();

        if (!rows.MoveNext())
            return true;

        int violationsCount = 0;
        var previous = rows.Current;
        var lineNumber = 1;

        while (rows.MoveNext())
        {
            lineNumber++;
            var current = rows.Current;

            if (_comparer.Compare(previous, current) > 0)
            {
                violationsCount++;

                if (violationsCount <= MaxLoggedViolations)
                {
                    _logger.Warning(
                        "Sorting violation at line {lineNumber}." +
                        "Previous line: \"{previousLine}\". Current line: \"{currentLine}\"",
                        lineNumber,
                        previous.OriginLine,
                        current.OriginLine);
                }
            }

            previous = current;
        }

        return violationsCount == 0;
    }

    private bool CompareNumberStatistics(string originalFileName, string resultFileName)
    {
        var originalStatistics = ReadNumberStatistics(originalFileName);
        var resultStatistics = ReadNumberStatistics(resultFileName);

        var differencesCount = 0;

        foreach (var number in originalStatistics.Keys.Union(resultStatistics.Keys).Order())
        {
            originalStatistics.TryGetValue(number, out var originalCount);
            resultStatistics.TryGetValue(number, out var resultCount);

            if (originalCount == resultCount)
                continue;

            differencesCount++;
            if (differencesCount <= MaxLoggedStatisticsDifferences)
            {
                _logger.Warning(
                    "Number statistics mismatch for number {number}. " +
                    "Original count: {originalCount}. Result count: {resultCount}",
                    number, originalCount, resultCount);
            }
        }

        return differencesCount == 0;
    }

    private Dictionary<int, long> ReadNumberStatistics(string inputFileName)
    {
        var statistics = new Dictionary<int, long>();

        int rowCount = 0;

        var file = new FileReader(inputFileName, 1_000_000);
        foreach (var row in file.Read())
        {
            row.Parse(_alphabet);
            statistics[row.Number] = statistics.TryGetValue(row.Number, out var numberCount) ? numberCount + 1 : 1;

            rowCount++;
            if (rowCount % 1_000_000 == 0)
            {
                _logger.Information("Processed {count} lines for number statistics from file {fileName}",
                    rowCount, inputFileName);
            }
        }

        return statistics;
    }
}