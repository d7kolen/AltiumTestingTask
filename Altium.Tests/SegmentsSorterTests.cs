using Altium.Core;
using FluentAssertions;
using NUnit.Framework;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Altium.Core.IO;
using Altium.Core.Row;
using Altium.Tests.Tools;

namespace Altium.Tests;

[TestFixture]
public class SegmentsSorterDynamicSortTests
{
    #region Init

    private readonly RowDtoComparer _comparer = new();
    private TempFolder _folder = null!;
    private ILogger _logger = null!;
    private RowDtoAlphabet _alphabet = new();

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
        var rows = new List<RowDto>
        {
            new RowDto("7. abc"),
            new RowDto("6. abc"),
            new RowDto("5. abc"),
        };

        var settings = new SorterSettings { MaxSegmentSize = 1, ParallelSegmentSorting = 1 };
        var segments = new SegmentsSorterSimpleSort(_folder.SubPath("segments"), settings, _comparer, _logger);
        var fileList = await segments.CreateSegmentsAsync(rows);
        fileList.Sort();

        fileList.Should().HaveCount(2);

        var rows0 = new FileReader(fileList[0], 0).Read().ToList().ParseAll();

        rows0.Should().HaveCount(2);
        rows0[0].Number.Should().Be(6);
        rows0[1].Number.Should().Be(7);

        var rows1 = new FileReader(fileList[1], 0).Read().ToList().ParseAll();

        rows1.Should().HaveCount(1);
        rows1[0].Number.Should().Be(5);
    }

    [Test]
    public async Task SegmentSorting_BigSegmentSize()
    {
        var rows = new List<RowDto>
        {
            new RowDto("7. abc"),
            new RowDto("6. abc"),
            new RowDto("5. abc"),
        };

        var settings = new SorterSettings { MaxSegmentSize = 100, ParallelSegmentSorting = 1 };
        var segments = new SegmentsSorterSimpleSort(_folder.SubPath("segments"), settings, _comparer, _logger);
        var fileList = await segments.CreateSegmentsAsync(rows);
        fileList.Sort();

        fileList.Should().HaveCount(1);

        var resultRows = new FileReader(fileList[0], 0).Read().ToList().ParseAll();

        resultRows.Should().HaveCount(3);
        resultRows[0].Number.Should().Be(5);
        resultRows[1].Number.Should().Be(6);
        resultRows[2].Number.Should().Be(7);
    }

    [Test]
    public async Task SegmentSorting_SortingCriterias()
    {
        var rows = new List<RowDto>
        {
            new RowDto("5. def"),
            new RowDto("5. abc"),
        };

        var settings = new SorterSettings { MaxSegmentSize = 100, ParallelSegmentSorting = 1 };
        var segments = new SegmentsSorterSimpleSort(_folder.SubPath("segments"), settings, _comparer, _logger);
        var fileList = await segments.CreateSegmentsAsync(rows);
        fileList.Sort();

        fileList.Should().HaveCount(1);

        var resultRows = new FileReader(fileList[0], 0).Read().ToList().ParseAll();

        resultRows.Should().HaveCount(2);
        resultRows[0].StringValueAsString().Should().Be("abc");
        resultRows[1].StringValueAsString().Should().Be("def");
    }

    [Test]
    public async Task SegmentSorting_SortingCriterias_1()
    {
        var rows = new List<RowDto>
        {
            new RowDto("5. def"),
            new RowDto("6. abc"), //StringValue has sorting priority
        };

        var settings = new SorterSettings { MaxSegmentSize = 100, ParallelSegmentSorting = 1 };
        var segments = new SegmentsSorterSimpleSort(_folder.SubPath("segments"), settings, _comparer, _logger);
        var fileList = await segments.CreateSegmentsAsync(rows);
        fileList.Sort();

        fileList.Should().HaveCount(1);

        var resultRows = new FileReader(fileList[0], 0).Read().ToList().ParseAll();

        resultRows.Should().HaveCount(2);
        resultRows[0].StringValueAsString().Should().Be("abc");
        resultRows[1].StringValueAsString().Should().Be("def");
    }

    [Test]
    public async Task SegmentSorting_SortingCriterias_2()
    {
        var rows = new List<RowDto>
        {
            new RowDto("5. abcd"),
            new RowDto("5. abc"), //StringValue has sorting priority
        };

        var settings = new SorterSettings { MaxSegmentSize = 100, ParallelSegmentSorting = 1 };
        var segments = new SegmentsSorterSimpleSort(_folder.SubPath("segments"), settings, _comparer, _logger);
        var fileList = await segments.CreateSegmentsAsync(rows);
        fileList.Sort();

        fileList.Should().HaveCount(1);

        var resultRows = new FileReader(fileList[0], 0).Read().ToList().ParseAll();

        resultRows.Should().HaveCount(2);
        resultRows[0].StringValueAsString().Should().Be("abc");
        resultRows[1].StringValueAsString().Should().Be("abcd");
    }

    [Test]
    public async Task SegmentSorting_SortingCriterias_3()
    {
        var rows = new List<RowDto>
        {
            new RowDto("5. abc"),
            new RowDto("5. 123"), //StringValue has sorting priority
        };

        var settings = new SorterSettings { MaxSegmentSize = 100, ParallelSegmentSorting = 1 };
        var segments = new SegmentsSorterSimpleSort(_folder.SubPath("segments"), settings, _comparer, _logger);
        var fileList = await segments.CreateSegmentsAsync(rows);
        fileList.Sort();

        fileList.Should().HaveCount(1);

        var resultRows = new FileReader(fileList[0], 0).Read().ToList().ParseAll();

        resultRows.Should().HaveCount(2);
        resultRows[0].StringValueAsString().Should().Be("123");
        resultRows[1].StringValueAsString().Should().Be("abc");
    }

    [Test]
    public async Task SegmentSorting_SortingCriterias_4()
    {
        var rows = new List<RowDto>
        {
            new RowDto("5. 123"),
            new RowDto("5. 23"), //StringValue has sorting priority
        };

        var settings = new SorterSettings { MaxSegmentSize = 100, ParallelSegmentSorting = 1 };
        var segments = new SegmentsSorterSimpleSort(_folder.SubPath("segments"), settings, _comparer, _logger);
        var fileList = await segments.CreateSegmentsAsync(rows);
        fileList.Sort();

        fileList.Should().HaveCount(1);

        var resultRows = new FileReader(fileList[0], 0).Read().ToList().ParseAll();

        resultRows.Should().HaveCount(2);
        resultRows[0].StringValueAsString().Should().Be("123");
        resultRows[1].StringValueAsString().Should().Be("23");
    }
}