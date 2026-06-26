using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Altium.Core;
using Altium.Tests.Tools;
using Altium.Utilities.CheckResult;
using FluentAssertions;
using NUnit.Framework;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Altium.Tests;

[TestFixture]
public class SortedFileCheckerTests : IRowFileTest
{
    #region Init

    private static Logger CreateLogger(ILogEventSink sink)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Sink(sink)
            .CreateLogger();
    }

    private sealed class CollectingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = new();

        public void Emit(LogEvent logEvent)
        {
            Events.Add(logEvent);
        }
    }

    #endregion

    [Test]
    public async Task Check_SortedFile()
    {
        var folder = TempFolder.Create();
        var originalFile = folder.SubPath("original.txt");
        var resultFile = folder.SubPath("result.txt");
        var sink = new CollectingSink();

        await this.AppendLineToFile(originalFile, "1. abc");
        await this.AppendLineToFile(originalFile, "2. abc");
        await this.AppendLineToFile(originalFile, "1. def");

        await this.AppendLineToFile(resultFile, "1. abc");
        await this.AppendLineToFile(resultFile, "2. abc");
        await this.AppendLineToFile(resultFile, "1. def");

        using var logger = CreateLogger(sink);
        var checker = new SortedFileChecker(logger);

        var result = checker.Check(originalFile, resultFile);

        result.OrderCheck.Should().BeTrue();
        result.StatisticsCheck.Should().BeTrue();
    }

    [Test]
    public async Task Check_UnsortedFile_LogsLineNumberAndBothLines()
    {
        var folder = TempFolder.Create();
        var originalFile = folder.SubPath("original.txt");
        var resultFile = folder.SubPath("result.txt");
        var sink = new CollectingSink();

        await this.AppendLineToFile(originalFile, "2. abc");
        await this.AppendLineToFile(originalFile, "1. abc");
        await this.AppendLineToFile(originalFile, "3. abc");
        await this.AppendLineToFile(originalFile, "2. abc");

        await this.AppendLineToFile(resultFile, "2. abc");
        await this.AppendLineToFile(resultFile, "1. abc");
        await this.AppendLineToFile(resultFile, "3. abc");
        await this.AppendLineToFile(resultFile, "2. abc");

        using var logger = CreateLogger(sink);
        var checker = new SortedFileChecker(logger);

        var result = checker.Check(originalFile, resultFile);

        result.OrderCheck.Should().BeFalse();
        result.StatisticsCheck.Should().BeTrue();

        var warnings = sink.Events
            .Where(e => e.Level == LogEventLevel.Warning)
            .Select(e => e.RenderMessage())
            .ToList();

        warnings.Should().HaveCount(2);
        warnings[0].Should().Contain("Sorting violation at line 2.");
        warnings[0].Should().Contain("Previous line:");
        warnings[0].Should().Contain("2. abc");
        warnings[0].Should().Contain("Current line:");
        warnings[0].Should().Contain("1. abc");
        warnings[1].Should().Contain("Sorting violation at line 4.");
        warnings[1].Should().Contain("Previous line:");
        warnings[1].Should().Contain("3. abc");
        warnings[1].Should().Contain("Current line:");
        warnings[1].Should().Contain("2. abc");
    }

    [Test]
    public async Task Check_LogsOnlyFirst100Violations()
    {
        var folder = TempFolder.Create();
        var originalFile = folder.SubPath("original.txt");
        var resultFile = folder.SubPath("result.txt");
        var sink = new CollectingSink();

        for (int i = 150; i >= 1; i--)
        {
            await this.AppendLineToFile(originalFile, $"{i}. abc");
            await this.AppendLineToFile(resultFile, $"{i}. abc");
        }

        using var logger = CreateLogger(sink);
        var checker = new SortedFileChecker(logger);

        var result = checker.Check(originalFile, resultFile);

        result.OrderCheck.Should().BeFalse();
        result.StatisticsCheck.Should().BeTrue();

        var warnings = sink.Events
            .Where(e => e.Level == LogEventLevel.Warning)
            .Select(e => e.RenderMessage())
            .ToList();

        warnings.Should().HaveCount(100);
        warnings[0].Should().Contain("Sorting violation at line 2.");
        warnings[99].Should().Contain("Sorting violation at line 101.");
    }

    [Test]
    public async Task Check_DifferentNumberStatistics_LogsMismatchedCounts()
    {
        var folder = TempFolder.Create();
        var originalFile = folder.SubPath("original.txt");
        var resultFile = folder.SubPath("result.txt");
        var sink = new CollectingSink();

        await this.AppendLineToFile(originalFile, "1. abc");
        await this.AppendLineToFile(originalFile, "1. def");
        await this.AppendLineToFile(originalFile, "2. zzz");

        await this.AppendLineToFile(resultFile, "1. abc");
        await this.AppendLineToFile(resultFile, "2. zzz");
        await this.AppendLineToFile(resultFile, "3. zzz");

        using var logger = CreateLogger(sink);
        var checker = new SortedFileChecker(logger);

        var result = checker.Check(originalFile, resultFile);

        result.OrderCheck.Should().BeTrue();
        result.StatisticsCheck.Should().BeFalse();

        var warnings = sink.Events
            .Where(e => e.Level == LogEventLevel.Warning)
            .Select(e => e.RenderMessage())
            .ToList();

        warnings.Should().ContainSingle(x => x.Contains("Number statistics mismatch for number 1."));
        warnings.Should().ContainSingle(x => x.Contains("Number statistics mismatch for number 3."));
    }
}