
using System.Collections.Generic;

namespace Altium.Core;

class RowDtoBTree_AsyncRead
{
    public IAsyncEnumerator<RowDto> Current { get; set; }

    public RowDtoBTree_AsyncRead? Left { get; set; }
    public RowDtoBTree_AsyncRead? Right { get; set; }

    public RowDtoBTree_AsyncRead Min()
    {
        var t = this;
        while (t.Left != null)
            t = t.Left;
        return t;
    }

    public static RowDtoBTree_AsyncRead Add(RowDtoBTree_AsyncRead? tree, IAsyncEnumerator<RowDto> item, IComparer<RowDto> comparer)
    {
        if (tree == null)
        {
            return new RowDtoBTree_AsyncRead()
            {
                Current = item
            };
        }

        var t = tree;

        while (true)
        {
            //the current is bigger
            if (comparer.Compare(t.Current.Current, item.Current) > 0)
            {
                if (t.Left == null)
                {
                    t.Left = new RowDtoBTree_AsyncRead() { Current = item };
                    return tree;
                }
                else
                    t = t.Left;
            }
            else
            {
                if (t.Right == null)
                {
                    t.Right = new RowDtoBTree_AsyncRead() { Current = item };
                    return tree;
                }
                else
                    t = t.Right;
            }
        }
    }

    public static RowDtoBTree_AsyncRead RemoveMin(RowDtoBTree_AsyncRead? tree)
    {
        if (tree == null)
            return null;

        if (tree.Left == null)
            return tree.Right;

        tree.Left = RemoveMin(tree.Left);
        return tree;
    }
}