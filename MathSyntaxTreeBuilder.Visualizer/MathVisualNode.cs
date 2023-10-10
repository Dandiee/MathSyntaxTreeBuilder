using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MathSyntaxTreeBuilder.Visualizer;

public class MathVisualNode
{
    private readonly ViewModel _owner;

    public Node Node { get; }
    public MathVisualNode? Parent { get; }
    public List<MathVisualNode> Children { get; } = new();

    public StackPanel Visual { get; private set; }

    public MathVisualNode(Node node, MathVisualNode? parent, ViewModel owner)
    {
        _owner = owner;

        Node = node;
        Parent = parent;
    }

    private static readonly HashSet<Op> SimpleBinaryOperators = new[]
    {
        Op.Add, Op.Subtract, Op.Mul
    }.ToHashSet(OpComparer.Instance);

    public StackPanel GetVisual()
    {
        Visual = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Orientation = Orientation.Horizontal,
        };

        if (Node.Parent != null && Node.ScopeDepth > Node.Parent?.ScopeDepth && Node.Parent is not NodeRoot)
        {
            Visual.Children.Add(new TextBlock { Text = "(" });
        }


        if (Node is NodeRoot root)
        {
            Visual.Children.Add(Children[0].GetVisual());
        }
        else if (Node is NodeOp op)
        {
            if (op.Op.Equals(Op.Mul))
            {
                Visual.Children.Add(Children[0].GetVisual());
                Visual.Children.Add(Children[1].GetVisual());
                //}
                //else
                //{
                //    Visual.Children.Add(Children[0].GetVisual());
                //    Visual.Children.Add(new TextBlock { Text = "(" });
                //    Visual.Children.Add(Children[1].GetVisual());
                //    Visual.Children.Add(new TextBlock { Text = ")" });
                //}
            }
            else if (SimpleBinaryOperators.Contains(op.Op))
            {
                Visual.Children.Add(Children[0].GetVisual());
                Visual.Children.Add(new TextBlock { Text = op.Name, Margin = new Thickness(3, 0, 3, 0) }.SetColumn(1));
                Visual.Children.Add(Children[1].GetVisual());
            }
            else if (op.Op.Equals(Op.Div))
            {
                Visual.Children.Add(new StackPanel
                {
                    Children =
                    {
                        Children[0].GetVisual(),
                        new Grid
                        {
                            Height = 1,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Background = Brushes.Black,
                            Margin = new Thickness(0, 4, 3, 0)
                        },
                        Children[1].GetVisual(),
                    }
                });
            }
            else if (op.Op.Equals(Op.Pow) || op.Op.Equals(Op.PowChar))
            {
                Visual.Children.Add(Children[0].GetVisual());
                Visual.Children.Add(Children[1].GetVisual().SetFontSize(15).SetMargin(0, -15, 0, 0));
            }
            else // named functions
            {
                Visual.Children.Add(new TextBlock { Text = $"{Node.Name}(" }.SetColumn(0));
                Visual.Children.Add(Children[0].GetVisual().SetColumn(1));
                Visual.Children.Add(new TextBlock { Text = ")" }.SetColumn(2));
            }
        }
        else if (Node is NodeArg arg)
        {
            Visual.Children.Add(new TextBlock { Text = arg.BuildExpression() });
        }

        if (Node.Parent != null && Node.ScopeDepth > Node.Parent?.ScopeDepth && Node.Parent is not NodeRoot)
        {
            Visual.Children.Add(new TextBlock { Text = ")" });
        }

        return Visual;
    }

}