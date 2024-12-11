using System.Collections.Generic;

namespace Altium.Core;

class RowDtoBTree
{
    public IAsyncEnumerator<RowDto> Current { get; set; }

    public RowDtoBTree? Left { get; set; }
    public RowDtoBTree? Right { get; set; }

    public RowDtoBTree Min()
    {
        var t = this;
        while (t.Left != null)
            t = t.Left;
        return t;
    }

    public static RowDtoBTree Add(RowDtoBTree? tree, IAsyncEnumerator<RowDto> item, IComparer<RowDto> comparer)
    {
        if (tree == null)
        {
            return new RowDtoBTree()
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
                    t.Left = new RowDtoBTree() { Current = item };
                    return tree;
                }
                else
                    t = t.Left;
            }
            else
            {
                if (t.Right == null)
                {
                    t.Right = new RowDtoBTree() { Current = item };
                    return tree;
                }
                else
                    t = t.Right;
            }
        }
    }

    public static RowDtoBTree RemoveMin(RowDtoBTree? tree)
    {
        if (tree == null)
            return null;

        if (tree.Left == null)
            return tree.Right;

        tree.Left = RemoveMin(tree.Left);
        return tree;
    }
}