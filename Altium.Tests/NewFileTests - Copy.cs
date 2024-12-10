using Altium.Core;
using FluentAssertions;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altium.Tests;

[TestFixture]
public class ReadRowsTests
{
    [Test]
    public async Task ReadRow()
    {
        var folder = TempFolder.Create();
        var file = folder.SubFile("1.txt");

        await using var writer = new StreamWriter(file, false, Encoding.UTF8);
        writer.WriteLine("5. abc");

        var rows = new FileReader(file).Read().ToList();
        
        rows.Should().HaveCount(1);
        rows[0].Number.Should().Be(5);
        rows[0].StringValue.Should().Be("abc");
    }
}