using Altium.Core;
using FluentAssertions;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Altium.Tests;

[TestFixture]
public class ReadFileTests
{
    [Test]
    public async Task NewFile()
    {
        var folder = TempFolder.Create();

        var file = folder.SubPath("1.txt");

        var generator = new FileWriter(file);
        await generator.CreateRandomFileAsync(1);

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
    public async Task NewFile_SeveralLines()
    {
        var folder = TempFolder.Create();

        var file = folder.SubPath("1.txt");

        var generator = new FileWriter(file);
        await generator.CreateRandomFileAsync(2);

        File.Exists(file);

        using var reader = new StreamReader(file);
        reader.ReadLine().Should().NotBeNull();
        reader.ReadLine().Should().NotBeNull();
    }

    [Test]
    public async Task NewFile_Can_Be_Read()
    {
        var folder = TempFolder.Create();
        var file = folder.SubPath("1.txt");

        await new FileWriter(file).CreateRandomFileAsync(2);

        var rows = new FileReader(file, 0).Read().ToList();

        rows.Should().HaveCount(2);
    }
}