namespace Altium.Core;

public class RowDto
{
    public int Number { get; set; }

    public string StringValue { get; set; }
    public long? StringValueWeight { get; set; }

    public RowDto() { }

    public RowDto(int number, string stringValue)
    {
        Number = number;
        StringValue = stringValue;
    }
}
