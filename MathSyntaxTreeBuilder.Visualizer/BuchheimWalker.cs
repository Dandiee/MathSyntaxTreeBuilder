using MathSyntaxTreeBuilder.Visualizer;

namespace MathSyntaxTreeBuilder
{
    public class Meta
    {
        //public VisualNode? Thread = null; // ???
        //public VisualNode? Ancestor = null; // ???
    }

    internal class BuchheimWalker
    {
        private double[] _mDepths = new double[10];
        private int _mMaxDepth;
        public static double HorizontalMargin = 30;
        public static double VerticalMargin = 30;
        private Dictionary<VisualNode, Meta> _metas;

        public void Run(VisualNode root)
        {
            _metas = new Dictionary<VisualNode, Meta>();
            var queue = new Queue<VisualNode>(new[] { root });
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                _metas[current] = new Meta
                {
                    //Ancestor = current.Parent
                };
                foreach (var child in current.Children)
                {
                    queue.Enqueue(child);
                }
            }



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


        private double Spacing(VisualNode l, VisualNode r, bool siblings)
        {
            return 0.5*(l.Width + r.Width) + HorizontalMargin;
        }

        private void UpdateDepths(int depth, VisualNode item)
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

        

        private void FirstWalk(VisualNode n, int num, int depth)
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

        private VisualNode Apportion(VisualNode v, VisualNode a)
        {
            var w = v.GetPrevSibling();
            if (w != null)
            {
                VisualNode vip, vim, vop, vom;
                double sip, sim, sop, som;

                vip = vop = v;
                vim = w;
                vom = vip.Parent.GetFirstChild();

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

        private VisualNode NextLeft(VisualNode n)
        {
            VisualNode c = null;
            c = n.GetFirstChild();
            return c != null ? c : n.Thread;
        }

        private VisualNode NextRight(VisualNode n)
        {
            VisualNode c = null;
            c = n.GetLastChild();
            return c != null ? c : n.Thread;
        }

        private void MoveSubtree(VisualNode wm, VisualNode wp, double shift)
        {
            double subtrees = wp.Number - wm.Number;
            wp.Change -= shift / subtrees;
            wp.Shift += shift;
            wm.Change += shift / subtrees;
            wp.Prelim += shift;
            wp.Mod += shift;
        }

        private void ExecuteShifts(VisualNode n)
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

        private VisualNode Ancestor(VisualNode vim, VisualNode v, VisualNode a)
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

        private void SecondWalk(VisualNode n, VisualNode p, double m, int depth)
        {
            n.X = n.Prelim + m;
            n.Y = _mDepths[depth] + depth * VerticalMargin;

            depth += 1;
            for (var c = n.GetFirstChild();
                  c != null; c = c.GetNextSibling())
            {
                SecondWalk(c, n, m + n.Mod, depth);
            }

            n.Clear();
        }
    }
}
