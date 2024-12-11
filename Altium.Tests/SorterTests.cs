using Altium.Core;
using FluentAssertions;
using NUnit.Framework;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

namespace Altium.Tests;

[TestFixture]
public class SorterTests : IRowFileTest
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
    public async Task SegmentSorting()
    {
        var inputFile = _folder.SubPath("1.txt");

        await this.AppendLineToFile(inputFile, "6. abc");
        await this.AppendLineToFile(inputFile, "8. abc");
        await this.AppendLineToFile(inputFile, "4. abc");
        await this.AppendLineToFile(inputFile, "5. abc");
        await this.AppendLineToFile(inputFile, "7. abc");
        await this.AppendLineToFile(inputFile, "9. abc");

        var sorter = new Sorter(_folder.SubPath("temp"), _logger);
        sorter.InitSegmentSize = 1;
        sorter.ReadingBufferSize = 1;

        var fileResult = _folder.SubPath("res.txt");
        await sorter.SortAsync(inputFile, fileResult);

        var resultRows = new FileReader(fileResult, 0).Read().ToList();

        resultRows.Should().HaveCount(6);
        resultRows[0].Number.Should().Be(4);
        resultRows[1].Number.Should().Be(5);
        resultRows[2].Number.Should().Be(6);
        resultRows[3].Number.Should().Be(7);
        resultRows[4].Number.Should().Be(8);
        resultRows[5].Number.Should().Be(9);
    }

    [Test]
    public async Task SegmentSorting_SingleSegment()
    {
        var inputFile = _folder.SubPath("1.txt");

        await this.AppendLineToFile(inputFile, "6. abc");
        await this.AppendLineToFile(inputFile, "8. abc");
        await this.AppendLineToFile(inputFile, "4. abc");

        var sorter = new Sorter(_folder.SubPath("temp"), _logger);
        sorter.InitSegmentSize = 10_000;
        sorter.ReadingBufferSize = 1;

        var fileResult = _folder.SubPath("res.txt");
        await sorter.SortAsync(inputFile, fileResult);

        var resultRows = new FileReader(fileResult, 0).Read().ToList();

        resultRows.Should().HaveCount(3);
        resultRows[0].Number.Should().Be(4);
        resultRows[1].Number.Should().Be(6);
        resultRows[2].Number.Should().Be(8);
    }
}