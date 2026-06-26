using System.Collections.Generic;
using System.IO;
using System.Text;
using Altium.Core.Row;

namespace Altium.Core.IO;

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

    public IEnumerable<RowDto> Read()
    {
        using var stream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read, _bufferSize);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string? line;
        while ((line = reader.ReadLine()) != null)
            yield return new RowDto(line);
    }
}