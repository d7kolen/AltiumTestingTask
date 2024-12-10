using Altium.Core;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Altium.Tests;

[TestFixture]
public class ReadFileTests
{
    [Test]
    public async Task CreateRandomFile()
    {
        var folder = TempFolder.Create();

        var file = folder.SubPath("1.txt");

        using (var generator = new FileWriter(file))
            await generator.WriteRandomRowsAsync(1);

        File.Exists(file);

        using var reader = new StreamReader(file);
        var line = reader.ReadLine()!;

        var parts = line.Split(". ");
        parts.Should().HaveCount(2);
        int.TryParse(parts[0], out var _).Should().BeTrue();

        int.TryParse(parts[1], out var _).Should().BeFalse();
        parts[1].Length.Should().BeGreaterThan(0);

        reader.ReadLine().Should().BeNull();
    }

    [Test]
    public async Task CreateRandomFile_SeveralLines()
    {
        var folder = TempFolder.Create();

        var file = folder.SubPath("1.txt");

        using (var writer = new FileWriter(file))
            await writer.WriteRandomRowsAsync(2);

        File.Exists(file);

        using var reader = new StreamReader(file);
        reader.ReadLine().Should().NotBeNull();
        reader.ReadLine().Should().NotBeNull();
    }

    [Test]
    public async Task CreateRandomFile_Can_Be_Read()
    {
        var folder = TempFolder.Create();
        var file = folder.SubPath("1.txt");

        using (var writer = new FileWriter(file))
            await writer.WriteRandomRowsAsync(2);

        var rows = new FileReader(file, 0).Read().ToList();

        rows.Should().HaveCount(2);
    }
}