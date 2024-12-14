using System;
using System.Collections.Generic;

namespace Altium.Core;

public class RowDtoComparer : IComparer<RowDto>
{
    public int Compare(RowDto x, RowDto y)
    {
        if (x.StringValueWeight != null && y.StringValueWeight != null &&
            x.StringValueWeight != y.StringValueWeight)
        {
            return x.StringValueWeight < y.StringValueWeight ? -1 : 1;
        }

        var valueCompare = x.StringValue.Span.CompareTo(y.StringValue.Span, StringComparison.Ordinal);
        if (valueCompare != 0)
            return valueCompare;

        return x.Number.CompareTo(y.Number);
    }
}