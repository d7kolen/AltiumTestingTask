using System;

namespace Altium.Core;

public class RowDto
{
    private readonly RowDtoAlphabet _alphabet;
    private bool _parsed = false;

    public string OriginLine { get; }

    private int _number;
    public int Number => Parse()._number;

    private string _stringValue;
    public string StringValue => Parse()._stringValue;

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

        var parts = OriginLine.Split(". ");
        if (parts.Length != 2)
            throw new NotSupportedException();

        _number = int.Parse(parts[0]);
        _stringValue = parts[1];
        _stringValueWeight = _alphabet.StringValueWeight(parts[1]);

        _parsed = true;

        return this;
    }
}
