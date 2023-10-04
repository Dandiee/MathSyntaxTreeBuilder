namespace MathSyntaxTreeBuilder.Visualizer;

internal class BuchheimWalker
{
    private double[] depths = new double[10];
    private int maxDepth;
    public static double HorizontalMargin = 30;
    public static double VerticalMargin = 30;
    public double? MinX { get; private set; }
    public double? MaxX { get; private set; }
    public double TotalWidth { get; set; }

    public void Run(VisualNode root)
    {
        FirstWalk(root, 0, 1);
        DetermineDepths();
        SecondWalk(root, -root.Prelim, 0);

        TotalWidth = MaxX!.Value - MinX!.Value;
    }


    private double Spacing(VisualNode l, VisualNode r) => 0.5 * (l.Width + r.Width) + HorizontalMargin;

    private void UpdateDepths(int depth, VisualNode item)
    {
        var d = item.Height;
        if (depths.Length <= depth)
        {
            depths = new double[3 * depth / 2];
        }

        depths[depth] = Math.Max(depths[depth], d);
        maxDepth = Math.Max(maxDepth, depth);
    }

    private void DetermineDepths()
    {
        for (var i = 1; i < maxDepth; ++i)
        {
            depths[i] += depths[i - 1];
        }
    }

    private void FirstWalk(VisualNode n, int num, int depth)
    {
        n.Number = num;
        UpdateDepths(depth, n);

        if (n.Children.Count == 0) // is leaf
        {
            var l = n.GetPrevSibling();
            n.Prelim = l != null
                ? l.Prelim + Spacing(l, n)
                : 0;
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

            var midpoint = 0.5 * (leftMost.Prelim + rightMost.Prelim);

            var left = n.GetPrevSibling();
            if (left != null)
            {
                n.Prelim = left.Prelim + Spacing(left, n);
                n.Mod = n.Prelim - midpoint;
            }
            else
            {
                n.Prelim = midpoint;
            }
        }
    }

    private VisualNode Apportion(VisualNode v, VisualNode a)
    {
        var w = v.GetPrevSibling();
        if (w != null)
        {
            VisualNode? vop;

            var vip = vop = v;
            var vim = w;
            var vom = vip.Parent.GetFirstChild();

            var sip = vip.Mod;
            var sop = vop.Mod;
            var sim = vim.Mod;
            var som = vom.Mod;

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
                    (vip.Prelim + sip) + Spacing(vim, vip);
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

    private VisualNode? NextLeft(VisualNode n) => n.GetFirstChild() ?? n.Thread;
    private VisualNode? NextRight(VisualNode n) => n.GetLastChild() ?? n.Thread;

    private void MoveSubtree(VisualNode wm, VisualNode wp, double shift)
    {
        var change = shift / (wp.Number - wm.Number);
        wp.Change -= change;
        wp.Shift += shift;
        wm.Change += change;
        wp.Prelim += shift;
        wp.Mod += shift;
    }

    private void ExecuteShifts(VisualNode n)
    {
        double shift = 0, change = 0;
        for (var c = n.GetLastChild(); c != null; c = c.GetPrevSibling())
        {
            c.Prelim += shift;
            c.Mod += shift;
            change += c.Change;
            shift += c.Shift + change;
        }
    }

    private VisualNode Ancestor(VisualNode vim, VisualNode v, VisualNode a)
        => vim.Ancestor.Parent == v.Parent
            ? vim.Ancestor
            : a;

    private void SecondWalk(VisualNode n, double m, int depth)
    {
        n.X = n.Prelim + m;
        n.Y = depths[depth] + depth * VerticalMargin;

        if (MinX == null || MinX > n.X)
        {
            MinX = n.X;
        }

        if (MaxX == null || MaxX < n.X)
        {
            MaxX = n.X;
        }

        depth += 1;
        for (var c = n.GetFirstChild(); c != null; c = c.GetNextSibling())
        {
            SecondWalk(c, m + n.Mod, depth);
        }

        n.Clear();
    }
}
