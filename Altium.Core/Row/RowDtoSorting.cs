using System.Collections.Generic;

namespace Altium.Core;

public class RowDtoSorting
{
    public RowDto Data { get; }
    public RowDtoSorting? Next { get; set; }
    public RowDtoSorting? Bigger { get; set; }

    public RowDtoSorting(RowDto data, RowDtoSorting? next = null)
    {
        Data = data;
        Next = next;
    }

    public static RowDtoSorting Sort(RowDtoSorting root, IComparer<RowDto> comparer)
    {
        var rootAnchor = new RowDtoSorting(null, root);
        var mergeAnchor = new RowDtoSorting(null);

        while (rootAnchor.Next.Next != null)
        {
            var tRootAncher = rootAnchor;
            while (tRootAncher.Next != null && tRootAncher.Next.Next != null)
            {
                var first = tRootAncher.Next;
                var second = tRootAncher.Next.Next;
                var tNext = tRootAncher.Next.Next.Next;

                first.Next = null;
                second.Next = null;

                var tMergeAnchor = mergeAnchor;

                while (first != null && second != null)
                {
                    var compareResult = comparer.Compare(first.Data, second.Data);
                    if (compareResult <= 0)
                    {
                        tMergeAnchor.Bigger = first;
                        first = first.Bigger;
                    }
                    else
                    {
                        tMergeAnchor.Bigger = second;
                        second = second.Bigger;
                    }

                    tMergeAnchor = tMergeAnchor.Bigger;
                }

                tMergeAnchor.Bigger = first ?? second;

                mergeAnchor.Bigger.Next = tNext;
                tRootAncher.Next = mergeAnchor.Bigger;
                tRootAncher = tRootAncher.Next;
            }
        }

        return rootAnchor.Next;
    }

    public override string ToString()
    {
        if (Data == null)
            return "empty";
        return Data.OriginLine;
    }
}