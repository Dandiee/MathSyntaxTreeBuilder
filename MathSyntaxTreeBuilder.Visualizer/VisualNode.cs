using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MathSyntaxTreeBuilder.Visualizer;

public class VisualNode
{
    // new shit
    public double Width;
    public double Height;
    public double Prelim = 0; // ???
    public int Number = 0; // ???
    public double Change = 0; // ???
    public double Shift = 0; // ???
    public double Mod = 0; // ???
    public double X = 0; // ???
    public double Y = 0; // ???
    //public VisualNode? Thread = null; // ???
    //public VisualNode? Ancestor = null; // ???

    public void Clear() { }
    public VisualNode? GetLastChild() => Children.Count == 0 ? null : Children[^1];
    public VisualNode? GetFirstChild() => Children.Count == 0 ? null : Children[0];
    public VisualNode? PrevSibling
    {
        get
        {
            if (Parent == null) return null;
            var ind = Parent.Children.IndexOf(this);
            if (ind == 0) return null;
            return Parent.Children[ind - 1];
        }
    }

    public VisualNode? NextSibling
    {
        get
        {
            if (Parent == null) return null;
            var ind = Parent.Children.IndexOf(this);
            if (ind == Parent.Children.Count - 1) return null;
            return Parent.Children[ind + 1];
        }
    }




    public Node Node { get; }
    public VisualNode? Parent { get; }
    public List<VisualNode> Children { get; } = new();
    public int IndexInRow { get; }
    public int Depth { get; }

    public string Text { get; }
    public Grid Grid { get; private set; }
    private Border Border;
    public double HorizontalOffset;
    public double VerticalOffset;

    public VisualNode(Node node, VisualNode? parent)
    {
        Node = node;
        Parent = parent;
        //Ancestor = parent;
        Width = 40 + (node.Name.Length - 1) * 10;
        Height = 40;

        Text = node.Name;

        Border = new Border
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
                FontWeight = FontWeights.Bold
            }
        };

        Grid = new Grid();
        Grid.Children.Add(Border);
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
                FontFamily = new FontFamily("Consolas"),
                FontSize = 9
            }
        });
    }
}