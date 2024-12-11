using System.Collections.Generic;
using System.IO;
using System.Text;

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

    public IEnumerable<RowDto> Read()
    {
        using var stream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read, _bufferSize);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string line = null;
        while ((line = reader.ReadLine()) != null)
        {
            var parts = line.Split(". ");
            if (parts.Length != 2)
                yield break;

            yield return new RowDto()
            {
                OriginLine = line,
                Number = int.Parse(parts[0]),
                StringValue = parts[1],
                StringValueWeight = _alphabet.StringValueWeight(parts[1])
            };
        }
    }
}