using System.Reflection.Metadata.Ecma335;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MathSyntaxTreeBuilder.Visualizer;

public class VisualNode
{
    private readonly Canvas _canvas;

    public double Width;
    public double Height;
    public double Prelim;
    public int Number;
    public double Change;
    public double Shift;
    public double Mod;
    public double X = 0;
    public double Y = 0;
    public VisualNode? Thread;
    public VisualNode? Ancestor;

    public void Clear()
    {

        Prelim = 0;
        Number = 0;
        Change = 0;
        Shift = 0;
        Mod = 0;
        Thread = null;
        Ancestor = Parent;

    }
    public VisualNode? GetLastChild() => Children.Count == 0 ? null : Children[^1];
    public VisualNode? GetFirstChild() => Children.Count == 0 ? null : Children[0];
    public VisualNode? GetPrevSibling()
    {
        if (Parent == null) return null;
        var ind = Parent.Children.IndexOf(this);
        if (ind == 0) return null;
        return Parent.Children[ind - 1];
    }

    public VisualNode? GetNextSibling()
    {
        if (Parent == null) return null;
        var ind = Parent.Children.IndexOf(this);
        if (ind == Parent.Children.Count - 1) return null;
        return Parent.Children[ind + 1];
    }


    public Node Node { get; }
    public VisualNode? Parent { get; }
    public List<VisualNode> Children { get; } = new();

    public string Text { get; }
    public Grid Grid { get; }
    public Line? Line { get; }

    public VisualNode(Node node, VisualNode? parent, Canvas canvas)
    {
        _canvas = canvas;
        Node = node;
        Parent = parent;
        Ancestor = parent;
        Width = 40 + (node.Name.Length - 1) * 10;
        Height = 40;

        Text = node.Name;

        if (parent != null)
        {
            Line = new Line
            {
                Stroke = Brushes.Yellow
            };

            _canvas.Children.Add(Line);
            Panel.SetZIndex(Line, -1);
        }


        Grid = new Grid();



        Grid.Children.Add(new Border
        {
            Width = Width,
            Height = Height,
            CornerRadius = new CornerRadius(20),
            Background = Node is NodeOp ?
                Node is NodeRoot
                    ? Brushes.DeepSkyBlue
                    : Brushes.White
                : Brushes.DarkGray,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0),
            BorderBrush = Brushes.Yellow,
            BorderThickness = new Thickness(2),
            Child = new TextBlock
            {
                Text = Text,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 18,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold
            }
        });

        Grid.Children.Add(new Border
        {
            Width = 16,
            Height = 16,
            CornerRadius = new CornerRadius(8),
            BorderBrush = Brushes.Black,
            Background = Brushes.Yellow,
            BorderThickness = new Thickness(1),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(-5, -5, 0, 0),
            Child = new TextBlock
            {
                Text = Node.ScopeDepth.ToString(),
                Foreground = Brushes.Black,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 9
            }
        });

        _canvas.Children.Add(Grid);
    }
}