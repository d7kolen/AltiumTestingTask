using System.Collections.Generic;

namespace Altium.Core;

class RowDtoComparer : IComparer<RowDto>
{
    public int Compare(RowDto x, RowDto y)
    {
        if (x.StringValueWeight != null && y.StringValueWeight != null &&
            x.StringValueWeight != y.StringValueWeight)
        {
            return x.StringValueWeight < y.StringValueWeight ? -1 : 1;
        }

        var result = x.StringValue.CompareTo(y.StringValue);
        if (result != 0)
            return result;

        return x.Number.CompareTo(y.Number);
    }
}

class EnumeratorRowDtoComparer : IComparer<IEnumerator<RowDto>>
{
    private readonly IComparer<RowDto> _inner = new RowDtoComparer();

    public int Compare(IEnumerator<RowDto> x, IEnumerator<RowDto> y)
    {
        return _inner.Compare(x.Current, y.Current);
    }
}