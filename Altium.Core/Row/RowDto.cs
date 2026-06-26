using System;

namespace Altium.Core.Row;

public class RowDto
{
    private bool _parsed = false;

    /// <summary>
    /// Using the OriginalLine in a kind of performance optimization.
    /// I tried several approaches here:
    /// 1. Split + concatenation initially.
    /// 2. Keep the OriginLine to exclude concatenation for the writing after that
    /// But the way with the memory sharing between the OriginLine and the StringValue is the most efficient.
    /// This is because memory allocation for small objects appears to be the bottleneck here.
    ///
    /// Answer on the comment:
    /// OriginLine appears unnecessary since the writer can generate
    /// the Row directly from the number and text parts. Simplifying this will streamline the code.
    /// </summary>
    public string OriginLine { get; }

    public int Number { get; private set; }
    public ReadOnlyMemory<char> StringValue { get; private set; }
    public long StringValueWeight { get; private set; }

    public RowDto(string originLine)
    {
        OriginLine = originLine;
    }

    public RowDto Parse(RowDtoAlphabet alphabet)
    {
        if (_parsed)
            return this;

        var dotIndex = OriginLine.IndexOf('.');
        if (dotIndex < 0)
            throw new NotSupportedException();

        Number = int.Parse(OriginLine.AsSpan(0, dotIndex));
        StringValue = OriginLine.AsMemory(dotIndex + 2); //". "
        StringValueWeight = alphabet.StringValueWeight(StringValue);

        _parsed = true;

        return this;
    }
}