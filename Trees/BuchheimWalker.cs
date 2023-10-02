
using System.Drawing;

namespace Trees;
public sealed class BuchheimWalker<TNode>
{
    private double[] _mDepths = new double[10];
    private int _mMaxDepth;
    public static double HorizontalMargin = 30;
    public static double VerticalMargin = 30;


    private double Spacing(BuchheimNode<TNode> l, BuchheimNode<TNode> r, bool siblings) 
        => 0.5 * (l.Width + r.Width) + HorizontalMargin;

    private void UpdateDepths(int depth, BuchheimNode<TNode> item)
    {
        // TODO: this 'd' doesn't do what you might think.
        var d = item.Height;
        if (_mDepths.Length <= depth)
        {
            // ArrayLib.resize(m_depths, 3 * depth / 2);
            _mDepths = new double[3 * depth / 2];
        }

        _mDepths[depth] = Math.Max(_mDepths[depth], d);
        _mMaxDepth = Math.Max(_mMaxDepth, depth);
    }

    private void DetermineDepths()
    {
        for (var i = 1; i < _mMaxDepth; ++i)
        {
            _mDepths[i] += _mDepths[i - 1];
        }
    }

    public void Run(BuchheimNode<TNode> root)
    {
        for (var i = 0; i < _mDepths.Length; i++)
        {
            _mDepths[i] = 0;
        }

        _mMaxDepth = 0;

        // do first pass - compute breadth information, collect depth info
        FirstWalk(root, 0, 1);

        // sum up the depth info
        DetermineDepths();

        // do second pass - assign layout positions
        SecondWalk(root, null, -root.Prelim, 0);
    }


    private void FirstWalk(BuchheimNode<TNode> n, int num, int depth)
    {
        n.Number = num;
        UpdateDepths(depth, n);

        if (n.Children.Count == 0) // is leaf
        {
            var l = n.GetPrevSibling();
            if (l == null)
            {
                n.Prelim = 0;
            }
            else
            {
                n.Prelim = l.Prelim + Spacing(l, n, true);
            }
        }
        else
        {
            var leftMost = n.GetFirstChild();
            var rightMost = n.GetLastChild();
            var defaultAncestor = leftMost;
            var c = leftMost;
            for (var i = 0; c != null; ++i, c = c.GetNextSibling())
            {
                FirstWalk(c, i, depth + 1);
                defaultAncestor = Apportion(c, defaultAncestor);
            }

            ExecuteShifts(n);

            var midpoint = 0.5 *
                           (leftMost.Prelim + rightMost.Prelim);

            var left = n.GetPrevSibling();
            if (left != null)
            {
                n.Prelim = left.Prelim + Spacing(left, n, true);
                n.Mod = n.Prelim - midpoint;
            }
            else
            {
                n.Prelim = midpoint;
            }
        }
    }

    private BuchheimNode<TNode> Apportion(BuchheimNode<TNode> v, BuchheimNode<TNode> a)
    {
        var w = v.GetPrevSibling();
        if (w != null)
        {
            BuchheimNode<TNode> vip, vim, vop, vom;
            double sip, sim, sop, som;

            vip = vop = v;
            vim = w;
            vom = vip.Parent!.Children[0];

            sip = vip.Mod;
            sop = vop.Mod;
            sim = vim.Mod;
            som = vom.Mod;

            var nr = NextRight(vim);
            var nl = NextLeft(vip);
            while (nr != null && nl != null)
            {
                vim = nr;
                vip = nl;
                vom = NextLeft(vom);
                vop = NextRight(vop);
                vop.Ancestor = v;
                var shift = vim.Prelim + sim -
                    (vip.Prelim + sip) + Spacing(vim, vip, false);
                if (shift > 0)
                {
                    MoveSubtree(Ancestor(vim, v, a), v, shift);
                    sip += shift;
                    sop += shift;
                }
                sim += vim.Mod;
                sip += vip.Mod;
                som += vom.Mod;
                sop += vop.Mod;

                nr = NextRight(vim);
                nl = NextLeft(vip);
            }
            if (nr != null && NextRight(vop) == null)
            {
                vop.Thread = nr;
                vop.Mod += sim - sop;
            }
            if (nl != null && NextLeft(vom) == null)
            {
                vom.Thread = nl;
                vom.Mod += sip - som;
                a = v;
            }
        }
        return a;
    }

    private BuchheimNode<TNode> NextLeft(BuchheimNode<TNode> n)
    {
        BuchheimNode<TNode> c = null;
        c = n.GetFirstChild();
        return c != null ? c : n.Thread;
    }

    private BuchheimNode<TNode> NextRight(BuchheimNode<TNode> n)
    {
        BuchheimNode<TNode> c = null;
        c = n.GetLastChild();
        return c != null ? c : n.Thread;
    }

    private void MoveSubtree(BuchheimNode<TNode> wm, BuchheimNode<TNode> wp, double shift)
    {
        double subtrees = wp.Number - wm.Number;
        wp.Change -= shift / subtrees;
        wp.Shift += shift;
        wm.Change += shift / subtrees;
        wp.Prelim += shift;
        wp.Mod += shift;
    }

    private void ExecuteShifts(BuchheimNode<TNode> n)
    {
        double shift = 0, change = 0;
        for (var c = n.GetLastChild();
              c != null; c = c.GetPrevSibling())
        {
            c.Prelim += shift;
            c.Mod += shift;
            change += c.Change;
            shift += c.Shift + change;
        }
    }

    private BuchheimNode<TNode> Ancestor(BuchheimNode<TNode> vim, BuchheimNode<TNode> v, BuchheimNode<TNode> a)
    {
        var p = v.Parent;
        if (vim.Ancestor.Parent == p)
        {
            return vim.Ancestor;
        }
        else
        {
            return a;
        }
    }

    private void SecondWalk(BuchheimNode<TNode> n, BuchheimNode<TNode> p, double m, int depth)
    {
        n.X  = n.Prelim + m;
        n.Y = _mDepths[depth] + depth * VerticalMargin;

        depth += 1;
        for (var c = n.GetFirstChild();
              c != null; c = c.GetNextSibling())
        {
            SecondWalk(c, n, m + n.Mod, depth);
        }
    }
}
