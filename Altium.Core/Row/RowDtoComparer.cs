using System.Collections.Generic;

namespace Altium.Core;

class RowDtoComparer : IComparer<RowDto>
{
    public int Compare(RowDto x, RowDto y)
    {
        if (x.PrimaryWeight != null && y.PrimaryWeight != null &&
            x.PrimaryWeight != y.PrimaryWeight)
        {
            return x.PrimaryWeight < y.PrimaryWeight ? -1 : 1;
        }

        var result = x.StringValue.CompareTo(y.StringValue);
        if (result != 0)
            return result;

        return x.Number.CompareTo(y.Number);
    }
}