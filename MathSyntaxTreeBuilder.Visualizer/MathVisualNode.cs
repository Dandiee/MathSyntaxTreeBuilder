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
            HorizontalAlignment = HorizontalAlignment.Left,
            Orientation = Orientation.Horizontal,
        };

        if (Node is NodeRoot root)
        {
            Visual.Children.Add(Children[0].GetVisual());
        }
        else if (Node is NodeOp op)
        {
            if (SimpleBinaryOperators.Contains(op.Op))
            {
                Visual.Children.Add(Children[0].GetVisual().SetColumn(0));
                Visual.Children.Add(new TextBlock { Text = op.Name, Margin = new Thickness(3,0,3,0)}.SetColumn(1));
                Visual.Children.Add(Children[1].GetVisual().SetColumn(2));
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
                            Background = Brushes.Yellow
                        },
                        Children[1].GetVisual(),
                    }
                });
            }
            else if (op.Op.Equals(Op.Pow) || op.Op.Equals(Op.PowChar))
            {
                Visual.Children.Add(Children[0].GetVisual());
                Visual.Children.Add(Children[1].GetVisual());
            }
            else
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

        return Visual;
    }

}