using System;

namespace Altium.Core;

public class RowDto
{
    private readonly RowDtoAlphabet _alphabet;
    private bool _parsed = false;

    public string OriginLine { get; }

    private int _number;
    public int Number => Parse()._number;

    private ReadOnlyMemory<char> _stringValue;
    public ReadOnlyMemory<char> StringValue => Parse()._stringValue;

    long? _stringValueWeight;
    public long? StringValueWeight => Parse()._stringValueWeight;

    public RowDto(string originLine, RowDtoAlphabet alphabet)
    {
        OriginLine = originLine;
        _alphabet = alphabet;
    }

    private RowDto Parse()
    {
        if (_parsed)
            return this;

        var dotIndex = OriginLine.IndexOf('.');
        if (dotIndex < 0)
            throw new NotSupportedException();

        _number = int.Parse(OriginLine.AsSpan(0, dotIndex));
        _stringValue = OriginLine.AsMemory(dotIndex + 2); //". "
        _stringValueWeight = _alphabet.StringValueWeight(_stringValue);

        _parsed = true;

        return this;
    }
}
