using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Altium.Core;

public class FileReader_Async
{
    private readonly string _fileName;
    private readonly int _bufferSize;
    private readonly RowDtoAlphabet _alphabet = new();

    public FileReader_Async(string fileName, int bufferSize)
    {
        _fileName = fileName;
        _bufferSize = bufferSize;
    }

    public async IAsyncEnumerable<List<RowDto>> ReadAsync(int batch = 1000)
    {
        using var ioStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        await using var stream = new SwappingBufferReadingStream(ioStream, _bufferSize);
        using var reader = new StreamReader(stream, Encoding.UTF8, false, 1_000_000);

        string line = null;

        var result = new List<RowDto>(batch);

        while ((line = await reader.ReadLineAsync()) != null)
        {
            var parts = line.Split(". ");
            if (parts.Length != 2)
                break;

            result.Add(new RowDto(line, _alphabet));
            if (result.Count >= batch)
            {
                yield return result;
                result = new List<RowDto>();
            }
        }

        if (result.Any())
            yield return result;
    }
}