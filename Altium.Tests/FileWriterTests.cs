using Altium.Core;
using FluentAssertions;
using NUnit.Framework;
using Serilog;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Altium.Tests;

[TestFixture]
public class ReadFileTests
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
    public async Task CreateRandomFile()
    {
        await Task.Yield();

        var file = _folder.SubPath("1.txt");

        using (var generator = new FileWriter(file))
            generator.WriteRandomRows(1, _logger);

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
        await Task.Yield();

        var file = _folder.SubPath("1.txt");

        using (var writer = new FileWriter(file))
            writer.WriteRandomRows(2, _logger);

        File.Exists(file);

        using var reader = new StreamReader(file);
        reader.ReadLine().Should().NotBeNull();
        reader.ReadLine().Should().NotBeNull();
    }

    [Test]
    public async Task CreateRandomFile_Can_Be_Read()
    {
        await Task.Yield();

        var file = _folder.SubPath("1.txt");

        using (var writer = new FileWriter(file))
            writer.WriteRandomRows(2, _logger);

        var rows = new FileReader(file, 0).Read().ToList();

        rows.Should().HaveCount(2);
    }
}