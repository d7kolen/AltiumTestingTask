using Altium.Core;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Altium.Tests;

[TestFixture]
public class SwappingBufferStreamTests : IRowFileTest
{
    [Test]
    public async Task ReadRow()
    {
        var folder = TempFolder.Create();
        var file = folder.SubPath("1.txt");

        await this.AppendLineToFile(file, "5. abc");

        using var stream = new FileStream(file, FileMode.Open);
        using var swappingStream = new SwappingBufferReadingStream(stream, 1);

        var buffer = new byte[100];
        var count = await swappingStream.ReadAsync(buffer, 0, 100, CancellationToken.None);

        count.Should().Be(11);

        var str = Encoding.UTF8.GetString(buffer, 0, count);
        str.Trim('\r', '\n', (char)65279).Should().Be("5. abc");
    }

    [Test]
    public async Task ReadRow_1()
    {
        var folder = TempFolder.Create();
        var file = folder.SubPath("1.txt");

        await this.AppendLineToFile(file, "5. abc");

        using var stream = new FileStream(file, FileMode.Open);
        using var swappingStream = new SwappingBufferReadingStream(stream, 1);

        int count = 1;
        int countSum = 0;
        while (count > 0)
        {
            var buffer = new byte[1];
            count = await swappingStream.ReadAsync(buffer, 0, 1, CancellationToken.None);
            countSum += count;
        }

        countSum.Should().Be(11);
    }

    [Test]
    public async Task ReadRow_2()
    {
        var folder = TempFolder.Create();
        var file = folder.SubPath("1.txt");

        await this.AppendLineToFile(file, "5. abc");

        using var stream = new FileStream(file, FileMode.Open);
        using var swappingStream = new SwappingBufferReadingStream(stream, 2);

        int count = 1;
        int countSum = 0;
        while (count > 0)
        {
            var buffer = new byte[1];
            count = await swappingStream.ReadAsync(buffer, 0, 1, CancellationToken.None);
            countSum += count;
        }

        countSum.Should().Be(11);
    }

    [Test]
    public async Task ReadRow_DisposeCheck()
    {
        var folder = TempFolder.Create();
        var file = folder.SubPath("1.txt");

        await this.AppendLineToFile(file, "5. abc");

        using var stream = new FileStream(file, FileMode.Open);
        await using (var swappingStream = new SwappingBufferReadingStream(stream, 1))
            await Task.Delay(TimeSpan.FromSeconds(1));
    }
}