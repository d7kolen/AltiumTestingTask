using Altium.Core;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Altium.Tests;

[TestFixture]
public class FileReaderAsyncTests : IRowFileTest
{
    [Test]
    public async Task ReadRow()
    {
        var folder = TempFolder.Create();
        var file = folder.SubPath("1.txt");

        await this.AppendLineToFile(file, "5. abc");

        List<RowDto> rows = new();
        await foreach (var tSet in new FileReader_Async(file, 0).ReadAsync(1))
            foreach (var t in tSet)
                rows.Add(t);

        rows.Should().HaveCount(1);
        rows[0].Number.Should().Be(5);
        rows[0].StringValue.Should().Be("abc");
    }

    [Test]
    public async Task ReadRow_SeveralLines()
    {
        var folder = TempFolder.Create();
        var file = folder.SubPath("1.txt");

        await this.AppendLineToFile(file, "5. abc");
        await this.AppendLineToFile(file, "7. def");

        List<RowDto> rows = new();
        await foreach (var tSet in new FileReader_Async(file, 0).ReadAsync(1))
            foreach (var t in tSet)
                rows.Add(t);

        rows.Should().HaveCount(2);

        rows[0].Number.Should().Be(5);
        rows[0].StringValue.Should().Be("abc");

        rows[1].Number.Should().Be(7);
        rows[1].StringValue.Should().Be("def");
    }

    [Test]
    public async Task ReadRow_By_TwoReaders()
    {
        var folder = TempFolder.Create();
        var file = folder.SubPath("1.txt");

        await this.AppendLineToFile(file, "5. abc");
        await this.AppendLineToFile(file, "7. def");

        await using var rows0 = new FileReader_Async(file, 0).ReadAsync(1).GetAsyncEnumerator();
        await using var rows1 = new FileReader_Async(file, 0).ReadAsync(1).GetAsyncEnumerator();

        await rows0.MoveNextAsync();
        await rows1.MoveNextAsync();

        rows0.Current[0].Number.Should().Be(rows1.Current[0].Number);
        rows0.Current[0].StringValue.Should().Be(rows1.Current[0].StringValue);

        await rows0.MoveNextAsync();
        await rows1.MoveNextAsync();

        rows0.Current[0].Number.Should().Be(rows1.Current[0].Number);
        rows0.Current[0].StringValue.Should().Be(rows1.Current[0].StringValue);
    }
}