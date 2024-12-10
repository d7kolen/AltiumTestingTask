using Altium.Core;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Altium.Tests;

[TestFixture]
public class SegmentsSorterTests
{
    [Test]
    public async Task SegmentSorting()
    {
        var folder = TempFolder.Create();

        var rows = new List<RowDto>
        {
            new RowDto(7, "abc"),
            new RowDto(6, "abc"),
            new RowDto(5, "abc")
        };

        var segments = new SegmentsSorter(folder.SubPath("segments"), 8);
        var fileList = await segments.CreateSegments(rows);

        fileList.Should().HaveCount(2);

        var rows0 = new FileReader(fileList[0], 0).Read().ToList();

        rows0.Should().HaveCount(2);
        rows0[0].Number.Should().Be(6);
        rows0[1].Number.Should().Be(7);

        var rows1 = new FileReader(fileList[1], 0).Read().ToList();

        rows1.Should().HaveCount(1);
        rows1[0].Number.Should().Be(5);
    }

    [Test]
    public async Task SegmentSorting_BigSegmentSize()
    {
        var folder = TempFolder.Create();

        var rows = new List<RowDto>
        {
            new RowDto(7, "abc"),
            new RowDto(6, "abc"),
            new RowDto(5, "abc")
        };

        var segments = new SegmentsSorter(folder.SubPath("segments"), 100);
        var fileList = await segments.CreateSegments(rows);

        fileList.Should().HaveCount(1);

        var resultRows = new FileReader(fileList[0], 0).Read().ToList();

        resultRows.Should().HaveCount(3);
        resultRows[0].Number.Should().Be(5);
        resultRows[1].Number.Should().Be(6);
        resultRows[2].Number.Should().Be(7);
    }

    [Test]
    public async Task SegmentSorting_SortingCriterias()
    {
        var folder = TempFolder.Create();

        var rows = new List<RowDto>
        {
            new RowDto(5, "def"),
            new RowDto(5, "abc")
        };

        var segments = new SegmentsSorter(folder.SubPath("segments"), 100);
        var fileList = await segments.CreateSegments(rows);

        fileList.Should().HaveCount(1);

        var resultRows = new FileReader(fileList[0], 0).Read().ToList();

        resultRows.Should().HaveCount(2);
        resultRows[0].StringValue.Should().Be("abc");
        resultRows[1].StringValue.Should().Be("def");
    }

    [Test]
    public async Task SegmentSorting_SortingCriterias_1()
    {
        var folder = TempFolder.Create();

        var rows = new List<RowDto>
        {
            new RowDto(5, "def"),
            new RowDto(6, "abc") //StringValue has sorting priority
        };

        var segments = new SegmentsSorter(folder.SubPath("segments"), 100);
        var fileList = await segments.CreateSegments(rows);

        fileList.Should().HaveCount(1);

        var resultRows = new FileReader(fileList[0], 0).Read().ToList();

        resultRows.Should().HaveCount(2);
        resultRows[0].StringValue.Should().Be("abc");
        resultRows[1].StringValue.Should().Be("def");
    }
}