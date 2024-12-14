using Altium.Core;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Altium.Tests;

[TestFixture]
public class FileReaderTests : IRowFileTest
{
    [Test]
    public async Task ReadRow()
    {
        var folder = TempFolder.Create();
        var file = folder.SubPath("1.txt");

        await this.AppendLineToFile(file, "5. abc");

        var rows = new FileReader(file, 0).Read().ToList();

        rows.Should().HaveCount(1);
        rows[0].Number.Should().Be(5);
        rows[0].StringValueAsString().Should().Be("abc");
    }

    [Test]
    public async Task ReadRow_SeveralLines()
    {
        var folder = TempFolder.Create();
        var file = folder.SubPath("1.txt");

        await this.AppendLineToFile(file, "5. abc");
        await this.AppendLineToFile(file, "7. def");

        var rows = new FileReader(file, 0).Read().ToList();

        rows.Should().HaveCount(2);

        rows[0].Number.Should().Be(5);
        rows[0].StringValueAsString().Should().Be("abc");

        rows[1].Number.Should().Be(7);
        rows[1].StringValueAsString().Should().Be("def");
    }

    [Test]
    public async Task ReadRow_By_TwoReaders()
    {
        var folder = TempFolder.Create();
        var file = folder.SubPath("1.txt");

        await this.AppendLineToFile(file, "5. abc");
        await this.AppendLineToFile(file, "7. def");

        using var rows0 = new FileReader(file, 0).Read().GetEnumerator();
        using var rows1 = new FileReader(file, 0).Read().GetEnumerator();

        rows0.MoveNext();
        rows1.MoveNext();

        rows0.Current.Number.Should().Be(rows1.Current.Number);
        rows0.Current.StringValueAsString().Should().Be(rows1.Current.StringValueAsString());

        rows0.MoveNext();
        rows1.MoveNext();

        rows0.Current.Number.Should().Be(rows1.Current.Number);
        rows0.Current.StringValueAsString().Should().Be(rows1.Current.StringValueAsString());
    }
}