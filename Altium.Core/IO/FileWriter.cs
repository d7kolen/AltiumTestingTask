using System;
using System.IO;
using System.Text;
using Altium.Core.Row;
using Serilog;

namespace Altium.Core.IO;

public class FileWriter : IDisposable
{
    private const int FileBufferSize = 1_000_000;
    private const int MaxNumber = 9_999;

    private StreamWriter _writer;
    private RowDtoAlphabet _alphabet = new();

    public FileWriter(string fileName)
    {
        var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write);

        //writer will close the 'stream' implicitly
        _writer = new StreamWriter(stream, Encoding.UTF8, FileBufferSize);
    }

    public void WriteRandomRows(int count, ILogger logger)
    {
        var random = new Random(new Guid().GetHashCode());

        for (int i = 0; i < count; i++)
        {
            WriteRow(
                random.Next(MaxNumber),
                _alphabet.RandomString(random));

            if (i % 1000000 == 0)
                logger.Information("Wrote {count} random lines", i);
        }
    }

    public void WriteRow(RowDto row)
    {
        _writer.WriteLine(row.OriginLine);
    }

    private void WriteRow(int number, string stringValue)
    {
        _writer.Write(number.ToString());
        _writer.Write(". ");
        _writer.Write(stringValue);
        _writer.WriteLine();
    }

    public void Dispose()
    {
        // if repeat call
        if (_writer == null!)
            return;

        _writer.Dispose();
        _writer = null!;
    }
}