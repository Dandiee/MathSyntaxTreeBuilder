using System.Reflection.Metadata.Ecma335;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MathSyntaxTreeBuilder.Visualizer;

public class VisualNode
{
    private readonly Canvas _canvas;
    private readonly ViewModel _owner;
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
    public Brush Background { get; }
    public Border MainBorder { get; }

    public bool IsDragged { get; private set; }
    private Point? _dragStartPos;
    public TextBlock MainTextBlock { get; }

    public VisualNode(Node node, VisualNode? parent, Canvas canvas, ViewModel owner)
    {
        _canvas = canvas;
        _owner = owner;

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

        Background = Node is NodeOp
            ? Node is NodeRoot
                ? Brushes.DeepSkyBlue
                : Brushes.White
            : Brushes.DarkGray;

        Grid = new Grid();

        MainTextBlock = new TextBlock
        {
            Text = Text,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 18,
            Foreground = Brushes.Black,
            FontWeight = FontWeights.Bold
        };

        MainBorder = new Border
        {
            Width = Width,
            Height = Height,
            CornerRadius = new CornerRadius(20),
            Background = Background,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0),
            BorderBrush = Brushes.Yellow,
            BorderThickness = new Thickness(2),
            Child = MainTextBlock
        };

        Grid.Children.Add(MainBorder);

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

        MainBorder.MouseDown += (sender, args) =>
        {
            IsDragged = true;
            _dragStartPos = args.GetPosition(_canvas);
            MainBorder.Background = Brushes.YellowGreen;
            
        };
        _canvas.MouseMove += (sender, args) =>
        {
            if (IsDragged)
            {
                MainBorder.Background = Brushes.Red;
                if (Node is NodeArg arg)
                {
                    var newPos = args.GetPosition(_canvas);
                    var delta = (_dragStartPos!.Value.Y - newPos.Y) / 20;
                    arg.Delta = delta;
                    MainTextBlock.Text = (arg.DoubleValue + arg.Delta).ToString("N2");
                    _owner.DrawFunction();
                }
            }
        };
        _canvas.MouseLeave += (sender, args) =>
        {
            if (IsDragged)
            {
                IsDragged = false;
                MainBorder.Background = Background;
                _dragStartPos = null;
            }
        };
        _canvas.MouseUp += (sender, args) =>
        {
            if (IsDragged)
            {
                IsDragged = false;
                MainBorder.Background = Background;
                _dragStartPos = null;
            }
        };
    }
}