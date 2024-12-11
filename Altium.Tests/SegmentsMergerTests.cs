using Altium.Core;
using FluentAssertions;
using NUnit.Framework;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

namespace Altium.Tests;

[TestFixture]
public class SegmentsMergerTests : IRowFileTest
{
    #region Init

    private TempFolder _folder = null!;
    private ILogger _logger = null!;

    [SetUp]
    public void Init()
    {
        _folder = TempFolder.Create();
        _logger = new LoggerConfiguration().CreateLogger();
    }

    #endregion

    [Test]
    public async Task MergeSegments()
    {
        var file1 = _folder.SubPath("1.txt");
        var file2 = _folder.SubPath("2.txt");

        await this.AppendLineToFile(file1, "5. abc");
        await this.AppendLineToFile(file2, "6. abc");

        var fileResult = _folder.SubPath("res.txt");
        var segments = new BTreeSegmentsMerger(fileResult, 100, _logger);
        await segments.MergeSegmentsAsync(new() { file1, file2 });

        var resultRows = new FileReader(fileResult, 0).Read().ToList();

        resultRows.Should().HaveCount(2);
        resultRows[0].Number.Should().Be(5);
        resultRows[1].Number.Should().Be(6);
    }

    [Test]
    public async Task MergeSegments_1()
    {
        var file1 = _folder.SubPath("1.txt");
        var file2 = _folder.SubPath("2.txt");

        await this.AppendLineToFile(file1, "5. abc");
        await this.AppendLineToFile(file2, "6. abc");

        var fileResult = _folder.SubPath("res.txt");
        var segments = new BTreeSegmentsMerger(fileResult, 100, _logger);
        await segments.MergeSegmentsAsync(new() { file2, file1 }); //other file order

        var resultRows = new FileReader(fileResult, 0).Read().ToList();

        resultRows.Should().HaveCount(2);
        resultRows[0].Number.Should().Be(5);
        resultRows[1].Number.Should().Be(6);
    }

    [Test]
    public async Task MergeSegments_Three_Lists()
    {
        var file1 = _folder.SubPath("1.txt");
        var file2 = _folder.SubPath("2.txt");
        var file3 = _folder.SubPath("3.txt");

        await this.AppendLineToFile(file1, "5. abc");
        await this.AppendLineToFile(file2, "6. abc");
        await this.AppendLineToFile(file3, "7. abc");

        var fileResult = _folder.SubPath("res.txt");
        var segments = new BTreeSegmentsMerger(fileResult, 100, _logger);
        await segments.MergeSegmentsAsync(new() { file1, file2, file3 });

        var resultRows = new FileReader(fileResult, 0).Read().ToList();

        resultRows.Should().HaveCount(3);
        resultRows[0].Number.Should().Be(5);
        resultRows[1].Number.Should().Be(6);
        resultRows[2].Number.Should().Be(7);
    }

    [Test]
    public async Task MergeSegments_2()
    {
        var file1 = _folder.SubPath("1.txt");
        var file2 = _folder.SubPath("2.txt");

        await this.AppendLineToFile(file1, "4. abc");
        await this.AppendLineToFile(file1, "6. abc");

        await this.AppendLineToFile(file2, "5. abc");
        await this.AppendLineToFile(file2, "7. abc");

        var fileResult = _folder.SubPath("res.txt");
        var segments = new BTreeSegmentsMerger(fileResult, 100, _logger);
        await segments.MergeSegmentsAsync(new() { file1, file2 });

        var resultRows = new FileReader(fileResult, 0).Read().ToList();

        resultRows.Should().HaveCount(4);
        resultRows[0].Number.Should().Be(4);
        resultRows[1].Number.Should().Be(5);
        resultRows[2].Number.Should().Be(6);
        resultRows[3].Number.Should().Be(7);
    }

    [Test]
    public async Task MergeSegments_3()
    {
        var file1 = _folder.SubPath("1.txt");
        var file2 = _folder.SubPath("2.txt");

        await this.AppendLineToFile(file1, "6. abc");

        await this.AppendLineToFile(file2, "4. abc");
        await this.AppendLineToFile(file2, "5. abc");
        await this.AppendLineToFile(file2, "7. abc");

        var fileResult = _folder.SubPath("res.txt");
        var segments = new BTreeSegmentsMerger(fileResult, 100, _logger);
        await segments.MergeSegmentsAsync(new() { file1, file2 });

        var resultRows = new FileReader(fileResult, 0).Read().ToList();

        resultRows.Should().HaveCount(4);
        resultRows[0].Number.Should().Be(4);
        resultRows[1].Number.Should().Be(5);
        resultRows[2].Number.Should().Be(6);
        resultRows[3].Number.Should().Be(7);
    }

    [Test]
    public async Task MergeSegments_Duplicates()
    {
        var file1 = _folder.SubPath("1.txt");
        var file2 = _folder.SubPath("2.txt");

        await this.AppendLineToFile(file1, "6. abc");
        await this.AppendLineToFile(file2, "6. abc");

        var fileResult = _folder.SubPath("res.txt");
        var segments = new BTreeSegmentsMerger(fileResult, 100, _logger);
        await segments.MergeSegmentsAsync(new() { file1, file2 });

        var resultRows = new FileReader(fileResult, 0).Read().ToList();

        resultRows.Should().HaveCount(2);
        resultRows[0].Number.Should().Be(6);
        resultRows[1].Number.Should().Be(6);
    }
}