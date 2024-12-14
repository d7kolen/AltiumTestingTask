using System.Collections.Generic;

namespace Altium.Core;

public class RowDtoSorting
{
    public RowDto Data { get; }
    public RowDtoSorting? Next { get; set; }

    public RowDtoSorting(RowDto data, RowDtoSorting? next = null)
    {
        Data = data;
        Next = next;
    }
}