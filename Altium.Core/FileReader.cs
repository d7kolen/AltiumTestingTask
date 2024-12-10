using System.Collections.Generic;

namespace Altium.Core;

public class FileReader
{
    private readonly string _fileName;

    public FileReader(string fileName)
    {
        _fileName = fileName;
    }

    public IEnumerable<RowDto> Read()
    {
        yield break;
    }
}