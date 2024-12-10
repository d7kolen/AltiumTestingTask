using System.Collections.Generic;

namespace Altium.Core;

class RowDtoComparer : IComparer<RowDto>
{
    public int Compare(RowDto x, RowDto y)
    {
        var result = x.StringValue.CompareTo(y.StringValue);
        if (result != 0)
            return result;

        return x.Number.CompareTo(y.Number);
    }
}