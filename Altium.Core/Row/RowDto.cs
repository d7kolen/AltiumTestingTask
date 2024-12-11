namespace Altium.Core;

public class RowDto
{
    public string OriginLine { get; set; }
    public int Number { get; set; }

    public string StringValue { get; set; }
    public long? StringValueWeight { get; set; }

    public RowDto() { }

    public RowDto(int number, string stringValue)
    {
        Number = number;
        StringValue = stringValue;
        OriginLine = $"{number}. {stringValue}";
    }
}
