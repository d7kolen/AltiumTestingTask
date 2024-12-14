using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Altium.Core;

public class FileReader
{
    private readonly string _fileName;
    private readonly int _bufferSize;
    private readonly RowDtoAlphabet _alphabet = new();

    public FileReader(string fileName, int bufferSize)
    {
        _fileName = fileName;
        _bufferSize = bufferSize;
    }

    public async IAsyncEnumerable<RowDto> ReadAsync(int prereadingBuffer)
    {
        var channel = Channel.CreateBounded<RowDto>(prereadingBuffer);

        var writer = Task.Run(async () =>
        {
            foreach (var t in Read())
                await channel.Writer.WriteAsync(t);

            channel.Writer.Complete();
        });

        await foreach (var t in channel.Reader.ReadAllAsync())
            yield return t;
    }

    public IEnumerable<RowDto> Read()
    {
        using var stream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read, _bufferSize);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string line = null;
        while ((line = reader.ReadLine()) != null)
            yield return new RowDto(line, _alphabet);
    }
}