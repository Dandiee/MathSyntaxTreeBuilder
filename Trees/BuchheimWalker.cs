namespace Trees;

public sealed class BuchheimWalker<TNode>
    where TNode : PayloadNode<TNode, BuchheimVisualNodeDescription<TNode>>
{
    private double[] _mDepths = new double[10];
    private int _mMaxDepth;
    public static double HorizontalMargin = 30;
    public static double VerticalMargin = 30;


    private double Spacing(TNode l, TNode r, bool siblings)
    {
        return 0.5 * (l.Payload.Width + r.Payload.Width) + HorizontalMargin;
    }

    private void UpdateDepths(int depth, TNode item)
    {
        // TODO: this 'd' doesn't do what you might think.
        var d = item.Payload.Height;
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

    public void Run(TNode root)
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
        SecondWalk(root, null, -root.Payload.Prelim, 0);
    }


    private void FirstWalk(TNode n, int num, int depth)
    {
        n.Payload.Number = num;
        UpdateDepths(depth, n);

        if (n.Children.Count == 0) // is leaf
        {
            var l = n.GetPrevSibling();
            if (l == null)
            {
                n.Payload.Prelim = 0;
            }
            else
            {
                n.Payload.Prelim = l.Payload.Prelim + Spacing(l, n, true);
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
                           (leftMost.Payload.Prelim + rightMost.Payload.Prelim);

            var left = n.GetPrevSibling();
            if (left != null)
            {
                n.Payload.Prelim = left.Payload.Prelim + Spacing(left, n, true);
                n.Payload.Mod = n.Payload.Prelim - midpoint;
            }
            else
            {
                n.Payload.Prelim = midpoint;
            }
        }
    }

    private TNode Apportion(TNode v, TNode a)
    {
        var w = v.GetPrevSibling();
        if (w != null)
        {
            TNode vip, vim, vop, vom;
            double sip, sim, sop, som;

            vip = vop = v;
            vim = w;
            vom = vip.Parent.GetFirstChild();

            sip = vip.Payload.Mod;
            sop = vop.Payload.Mod;
            sim = vim.Payload.Mod;
            som = vom.Payload.Mod;

            var nr = NextRight(vim);
            var nl = NextLeft(vip);
            while (nr != null && nl != null)
            {
                vim = nr;
                vip = nl;
                vom = NextLeft(vom);
                vop = NextRight(vop);
                vop.Payload.Ancestor = v;
                var shift = vim.Payload.Prelim + sim -
                    (vip.Payload.Prelim + sip) + Spacing(vim, vip, false);
                if (shift > 0)
                {
                    MoveSubtree(Ancestor(vim, v, a), v, shift);
                    sip += shift;
                    sop += shift;
                }
                sim += vim.Payload.Mod;
                sip += vip.Payload.Mod;
                som += vom.Payload.Mod;
                sop += vop.Payload.Mod;

                nr = NextRight(vim);
                nl = NextLeft(vip);
            }
            if (nr != null && NextRight(vop) == null)
            {
                vop.Payload.Thread = nr;
                vop.Payload.Mod += sim - sop;
            }
            if (nl != null && NextLeft(vom) == null)
            {
                vom.Payload.Thread = nl;
                vom.Payload.Mod += sip - som;
                a = v;
            }
        }
        return a;
    }

    private TNode NextLeft(TNode n) => n.GetFirstChild() ?? n.Payload.Thread;

    private TNode NextRight(TNode n) => n.GetLastChild() ?? n.Payload.Thread;

    private void MoveSubtree(TNode wm, TNode wp, double shift)
    {
        double subtrees = wp.Payload.Number - wm.Payload.Number;
        wp.Payload.Change -= shift / subtrees;
        wp.Payload.Shift += shift;
        wm.Payload.Change += shift / subtrees;
        wp.Payload.Prelim += shift;
        wp.Payload.Mod += shift;
    }

    private void ExecuteShifts(TNode n)
    {
        double shift = 0, change = 0;
        for (var c = n.GetLastChild();
              c != null; c = c.GetPrevSibling())
        {
            c.Payload.Prelim += shift;
            c.Payload.Mod += shift;
            change += c.Payload.Change;
            shift += c.Payload.Shift + change;
        }
    }

    private TNode Ancestor(TNode vim, TNode v, TNode a)
        => vim.Payload.Ancestor.Parent == v.Parent
            ? vim.Payload.Ancestor
            : a;

    private void SecondWalk(TNode n, TNode p, double m, int depth)
    {
        n.Payload.X = n.Payload.Prelim + m;
        n.Payload.Y = _mDepths[depth] + depth * VerticalMargin;

        depth += 1;
        for (var c = n.GetFirstChild();
              c != null; c = c.GetNextSibling())
        {
            SecondWalk(c, n, m + n.Payload.Mod, depth);
        }

        //n.Clear();
    }
}
