using System;
using System.Collections.Generic;

namespace Altium.Core.Row;

public class RowDtoComparer : IComparer<RowDto>
{
    RowDtoAlphabet _alphabet = new();

    public int Compare(RowDto? x, RowDto? y)
    {
        x!.Parse(_alphabet);
        y!.Parse(_alphabet);

        if (x.StringValueWeight != y.StringValueWeight)
        {
            return x.StringValueWeight < y.StringValueWeight ? -1 : 1;
        }

        var valueCompare = x.StringValue.Span.CompareTo(y.StringValue.Span, StringComparison.Ordinal);
        if (valueCompare != 0)
            return valueCompare;

        return x.Number.CompareTo(y.Number);
    }
}