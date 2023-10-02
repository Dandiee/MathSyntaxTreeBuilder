using MathSyntaxTreeBuilder.Visualizer;

namespace MathSyntaxTreeBuilder
{
    internal class BuchheimWalker
    {
        private double[] m_depths = new double[10];
        private int m_maxDepth = 0;
        public double HorizontalMargin = 20;
        public double VerticalMargin = 20;


        private double spacing(VisualNode l, VisualNode r, bool siblings)
        {
            return 0.5 *
                (l.Width + r.Width) + HorizontalMargin;
        }

        private void updateDepths(int depth, VisualNode item)
        {
            double d = (item.Height);
            if (m_depths.Length <= depth)
            {
                // ArrayLib.resize(m_depths, 3 * depth / 2);
                m_depths = new double[3 * depth / 2]; 
            }

            m_depths[depth] = Math.Max(m_depths[depth], d);
            m_maxDepth = Math.Max(m_maxDepth, depth);
        }

        private void determineDepths()
        {
            for (int i = 1; i < m_maxDepth; ++i)
                m_depths[i] += m_depths[i - 1];
        }

        // ------------------------------------------------------------------------

        /**
         * @see prefuse.action.Action#run(double)
         */
        public void run(VisualNode root)
        {
            // Arrays.fill(m_depths, 0);
            for (var i = 0; i < m_depths.Length; i++)
            {
                m_depths[i] = 0;
            }

            m_maxDepth = 0;

            // do first pass - compute breadth information, collect depth info
            firstWalk(root, 0, 1);

            // sum up the depth info
            determineDepths();

            // do second pass - assign layout positions
            secondWalk(root, null, -root.Prelim, 0);
        }


        private void firstWalk(VisualNode n, int num, int depth)
        {
            n.Number = num;
            updateDepths(depth, n);

            if (n.Children.Count == 0) // is leaf
            {
                VisualNode l = (VisualNode)n.PrevSibling;
                if (l == null)
                {
                    n.Prelim = 0;
                }
                else
                {
                    n.Prelim = l.Prelim + spacing(l, n, true);
                }
            }
            else
            {
                VisualNode leftMost = n.GetFirstChild();
                VisualNode rightMost = n.GetLastChild();
                VisualNode defaultAncestor = leftMost;
                VisualNode c = leftMost;
                for (int i = 0; c != null; ++i, c = (VisualNode)c.NextSibling)
                {
                    firstWalk(c, i, depth + 1);
                    defaultAncestor = apportion(c, defaultAncestor);
                }

                executeShifts(n);

                double midpoint = 0.5 *
                    (leftMost.Prelim + rightMost.Prelim);

                VisualNode left = (VisualNode)n.PrevSibling;
                if (left != null)
                {
                    n.Prelim = left.Prelim + spacing(left, n, true);
                    n.Mod = n.Prelim - midpoint;
                }
                else
                {
                    n.Prelim = midpoint;
                }
            }
        }

        private VisualNode apportion(VisualNode v, VisualNode a)
        {
            VisualNode w = (VisualNode)v.PrevSibling;
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

                VisualNode nr = nextRight(vim);
                VisualNode nl = nextLeft(vip);
                while (nr != null && nl != null)
                {
                    vim = nr;
                    vip = nl;
                    vom = nextLeft(vom);
                    vop = nextRight(vop);
                    vop.Ancestor = v;
                    double shift = (vim.Prelim + sim) -
                        (vip.Prelim + sip) + spacing(vim, vip, false);
                    if (shift > 0)
                    {
                        moveSubtree(ancestor(vim, v, a), v, shift);
                        sip += shift;
                        sop += shift;
                    }
                    sim += vim.Mod;
                    sip += vip.Mod;
                    som += vom.Mod;
                    sop += vop.Mod;

                    nr = nextRight(vim);
                    nl = nextLeft(vip);
                }
                if (nr != null && nextRight(vop) == null)
                {
                    vop.Thread = nr;
                    vop.Mod += sim - sop;
                }
                if (nl != null && nextLeft(vom) == null)
                {
                    vom.Thread = nl;
                    vom.Mod += sip - som;
                    a = v;
                }
            }
            return a;
        }

        private VisualNode nextLeft(VisualNode n)
        {
            VisualNode c = null;
            c = n.GetFirstChild();
            return (c != null ? c : n.Thread);
        }

        private VisualNode nextRight(VisualNode n)
        {
            VisualNode c = null;
            c = n.GetLastChild();
            return (c != null ? c : n.Thread);
        }

        private void moveSubtree(VisualNode wm, VisualNode wp, double shift)
        {
            double subtrees = wp.Number - wm.Number;
            wp.Change -= shift / subtrees;
            wp.Shift += shift;
            wm.Change += shift / subtrees;
            wp.Prelim += shift;
            wp.Mod += shift;
        }

        private void executeShifts(VisualNode n)
        {
            double shift = 0, change = 0;
            for (VisualNode c = n.GetLastChild();
                  c != null; c = (VisualNode)c.PrevSibling)
            {
                c.Prelim += shift;
                c.Mod += shift;
                change += c.Change;
                shift += c.Shift + change;
            }
        }

        private VisualNode ancestor(VisualNode vim, VisualNode v, VisualNode a)
        {
            VisualNode p = (VisualNode)v.Parent;
            if (vim.Ancestor.Parent == p)
            {
                return vim.Ancestor;
            }
            else
            {
                return a;
            }
        }

        private void secondWalk(VisualNode n, VisualNode p, double m, int depth)
        {
            n.X = n.Prelim + m;
            n.Y = m_depths[depth] + depth * VerticalMargin;

            depth += 1;
            for (VisualNode c = n.GetFirstChild();
                  c != null; c = (VisualNode)c.NextSibling)
            {
                secondWalk(c, n, m + n.Mod, depth);
            }

            n.Clear();
        }
    }
}
