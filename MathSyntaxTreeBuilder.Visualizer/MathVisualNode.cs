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

    public Grid Visual { get; private set; }

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

    public Grid GetVisual()
    {
        Visual = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Left
        };

        if (Node is NodeRoot root)
        {
            Visual.Children.Add(Children[0].GetVisual());
        }
        else if (Node is NodeOp op)
        {
            if (SimpleBinaryOperators.Contains(op.Op))
            {
                Visual.ColumnDefinitions.Add(new ColumnDefinition());
                Visual.ColumnDefinitions.Add(new ColumnDefinition());
                Visual.ColumnDefinitions.Add(new ColumnDefinition());
                
                Visual.Children.Add(new TextBlock { Text = op.Name, Margin = new Thickness(3,0,3,0)}.SetColumn(1));
                Visual.Children.Add(Children[0].GetVisual().SetColumn(0));
                Visual.Children.Add(Children[1].GetVisual().SetColumn(2));
            }
            else if (op.Op.Equals(Op.Div))
            {
                Visual.RowDefinitions.Add(new RowDefinition());
                Visual.RowDefinitions.Add(new RowDefinition());
                Visual.RowDefinitions.Add(new RowDefinition());

                Visual.Children.Add(new Grid
                {
                    Height = 1,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Background = Brushes.Yellow
                }.SetRow(1));
                Visual.Children.Add(Children[0].GetVisual().SetRow(0));
                Visual.Children.Add(Children[1].GetVisual().SetRow(2));
            }
            else if (op.Op.Equals(Op.Pow) || op.Op.Equals(Op.PowChar))
            {
                Visual.ColumnDefinitions.Add(new ColumnDefinition());
                Visual.ColumnDefinitions.Add(new ColumnDefinition());

                Visual.Children.Add(Children[0].GetVisual().SetColumn(0));
                Visual.Children.Add(new Grid
                {
                    Margin = new Thickness(2, -10, 0, 0),
                    Children =
                    {
                        Children[1].GetVisual()
                    }

                }.SetColumn(1));
            }
            else
            {
                Visual.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                Visual.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                Visual.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

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