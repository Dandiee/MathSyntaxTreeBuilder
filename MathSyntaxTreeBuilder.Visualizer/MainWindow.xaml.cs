using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MathSyntaxTreeBuilder.Visualizer;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        InputTextBox.Text = "sin((5+4*cos(1))*cos(2))";
    }

    private void InputChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            LengthLimitSlider.Value = InputTextBox.Text.Length;
            Calculate();
        }
    }

    private void Calculate()
    {
        if (string.IsNullOrEmpty(InputTextBox.Text))
        {
            Clear();
        }
        else
        {
            try
            {
                
                var tree = MathSyntaxBuilder.GetSyntaxTree(InputTextBox.Text, (int)LengthLimitSlider.Value);
                Draw(tree);

                if (tree.Variables.Count > 0)
                {
                    DrawFunction(tree);
                }
                else
                {
                    Eval.Text = tree.Eval().ToString();
                }
                
                Result.Text = tree.BuildString();
                InputTextBox.Foreground = new SolidColorBrush(Colors.Black);
            }
            catch
            {
                InputTextBox.Foreground = new SolidColorBrush(Colors.Red);
            }
        }
    }

    private void AddTickChars()
    {
        var text = InputTextBox.Text.ToArray();
        CharactersGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto});
        
        var s = new TextBlock { Text = " " };
        CharactersGrid.Children.Add(s);
        Grid.SetColumn(s, 0);

        for (var index = 0; index < text.Length; index++)
        {
            var c = text[index];
            CharactersGrid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)});


            var tb = new TextBlock
            {
                Text = c.ToString()
            };

            CharactersGrid.Children.Add(tb);
            Grid.SetColumn(tb, index + 1);
        }
    }

    private void DrawFunction(NodeRoot root)
    {
        if (root.Variables.Count != 1) return;

        var variableName = root.Variables.First();

        var c = FunctionCanvas; c.Children.Clear();
        var o = new Point(c.ActualWidth / 2, c.ActualHeight / 2);

        c.Children.Add(new Line { X1 = 0, X2 = c.ActualWidth, Y1 = o.Y, Y2 = o.Y, Stroke = Brushes.Yellow });
        c.Children.Add(new Line { X1 = o.X, X2 = o.X, Y1 = 0, Y2 = c.ActualHeight, Stroke = Brushes.Yellow });

        var poly = new Polyline { Stroke = Brushes.Red, StrokeThickness = 2};

        var pixelPerUnit = 50;
        var totalRange = c.ActualWidth / pixelPerUnit;

        var vars = new Dictionary<string, double>();
        Point? lastY = null;
        for (var x = totalRange / -2d; x <= totalRange; x += .01)
        {
            vars[variableName] = x;
            
            var y = root.Eval(vars);
            if (!double.IsNaN(y))
            {
                poly.Points.Add(new Point(x * pixelPerUnit + o.X, y * -pixelPerUnit + o.Y));
            }
            else
            {
                if (poly.Points.Count > 2)
                {
                    c.Children.Add(poly);
                    poly = new Polyline() { Stroke = Brushes.Red, StrokeThickness = 2 };
                }
            }
        }

        if (poly.Points.Count > 2)
        { 
            c.Children.Add(poly);
        }
    }

    private void Draw(NodeRoot root)
    {
        Clear();
        AddTickChars();

        TokenTextBox.Text = root.LeftOverToken;
        DepthTextBox.Text = root.CurrentDepth.ToString();
        LastOpTextBox.Text = root.LastOperation.Op.Name;
        SubstringTextBox.Text = InputTextBox.Text.Substring(0, (int)LengthLimitSlider.Value);
        VariablesTextBox.Text = string.Join(", ", root.Variables);
        Eval.Text = string.Empty;
        Result.Text = string.Empty;

        var q1 = new Queue<(Node, int)>(new (Node, int)[] { new(root, 0) });
        var maxDepth = 0;
        while (q1.Count > 0)
        {
            var current = q1.Dequeue();
            if (maxDepth < current.Item2) maxDepth = current.Item2;

            foreach (var chil in current.Item1.Children)
            {
                q1.Enqueue(new(chil, current.Item2 + 1));
            }
        }


        Title = maxDepth.ToString();


        var visualRoot = new VisualNode(root, TreeCanvas, maxDepth, 40);
        var queue = new Queue<VisualNode>(new[] { visualRoot });
        var width = 40d;

        var mid = TreeCanvas.ActualWidth / 2;
        var maximumWidth = Math.Pow(2, maxDepth - 1) * width;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.Parent == null)
            {
                current.HorizontalOffset = mid;
                current.VerticalOffset = 20;
            }
            else
            {
                if (current.Parent.Children.Count == 1)
                {
                    current.HorizontalOffset = current.Parent.HorizontalOffset;
                }
                else
                {
                    var isLeft = current.Parent.Children[0].Node == current.Node;
                    var step = maximumWidth / Math.Pow(2, current.Depth);

                    current.HorizontalOffset = isLeft
                        ? current.Parent.HorizontalOffset - step
                        : current.Parent.HorizontalOffset + step;
                }

                current.VerticalOffset = current.Parent.VerticalOffset + 80;
            }

            Canvas.SetLeft(current.Grid, current.HorizontalOffset);
            Canvas.SetTop(current.Grid, current.VerticalOffset);

            if (current.Parent != null)
            {
                var line = new Line
                {
                    X1 = current.Parent.HorizontalOffset + width / 2,
                    X2 = current.HorizontalOffset + width / 2,
                    Y1 = current.Parent.VerticalOffset + width / 2,
                    Y2 = current.VerticalOffset + width / 2,
                    Stroke = new SolidColorBrush(Colors.Yellow),

                };
                TreeCanvas.Children.Add(line);
                Panel.SetZIndex(line, -1);
            }

            TreeCanvas.Children.Add(current.Grid);

            foreach (var child in current.Children)
            {
                queue.Enqueue(child);
            }
        }
    }

    private void Clear()
    {
        CharactersGrid.ColumnDefinitions.Clear();
        CharactersGrid.Children.Clear();
        TreeCanvas.Children.Clear();
    }

    private void LengthLimitSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        Calculate();

    }
}

public class VisualNode
{
    public Node Node { get; }
    public VisualNode? Parent { get; }
    public List<VisualNode> Children { get; } = new();
    public int IndexInRow { get; }
    public int Depth { get; }
    
    public string Text { get; }
    public Grid Grid { get; }
    private Border Border { get; }
    public double HorizontalOffset;
    public double VerticalOffset;

    public VisualNode(Node node, Canvas canvas, int totalDepth, double width, VisualNode? parent = null, int indexInRow = 0, int depth = 0)
    {
        Node = node;
        IndexInRow = indexInRow;
        Depth = depth;
        Parent = parent;

        Text = node is NodeOp op
            ? op.Op.Name
            : (node as NodeArg).Value;

        for (var i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            var index = i == 0 ? -1 : 1;
            Children.Add(new VisualNode(child, canvas, totalDepth, width, this, index, depth + 1));
        }


        Border = new Border
        {
            Width = 40,
            Height = 40,
            CornerRadius = new CornerRadius(20),
            Background =  node is NodeOp ? 
                node is NodeRoot 
                    ? Brushes.Black
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
                Text = node.Depth.ToString(),
                Foreground = Brushes.Black,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 9
            }
        });


    }
}