using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MathSyntaxTreeBuilder.Visualizer;

public partial class MainWindow
{
    private bool _isTextChange = true;
    private bool _isInitialized = false;

    private NodeRoot? _tree = null;
    public MainWindow()
    {
        InitializeComponent();
        _isInitialized = true;
        InputTextBox.Text = "sin((5+4*cos(1))*cos(2))";
        
    }

    private void BuildTree()
    {
        _tree = MathSyntaxBuilder.GetSyntaxTree(InputTextBox.Text, (int)LengthLimitSlider.Value);
        Render();
    }

    private void Render()
    {
        if (!_isInitialized) return;

        CharactersGrid.ColumnDefinitions.Clear();
        CharactersGrid.Children.Clear();
        TreeCanvas.Children.Clear();

        DrawTree();
        DrawFunction();
    }

    private void InputChanged(object sender, TextChangedEventArgs e)
    {
        _isTextChange = true;
        BuildTree();
        LengthLimitSlider.Value = InputTextBox.Text.Length;
        Render();
        _isTextChange = false;
    }
    private void AddTickChars()
    {
        var text = InputTextBox.Text.ToArray();
        CharactersGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

        var s = new TextBlock { Text = " " };
        CharactersGrid.Children.Add(s);
        Grid.SetColumn(s, 0);

        for (var index = 0; index < text.Length; index++)
        {
            var c = text[index];
            CharactersGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });


            var tb = new TextBlock
            {
                Text = c.ToString()
            };

            CharactersGrid.Children.Add(tb);
            Grid.SetColumn(tb, index + 1);
        }
    }

    private void DrawFunction()
    {
        if (_tree.Variables.Count != 1) return;

        var variableName = _tree.Variables.First();

        var c = FunctionCanvas; c.Children.Clear();
        var o = new Point(c.ActualWidth / 2, c.ActualHeight / 2);

        c.Children.Add(new Line { X1 = 0, X2 = c.ActualWidth, Y1 = o.Y, Y2 = o.Y, Stroke = Brushes.Yellow });
        c.Children.Add(new Line { X1 = o.X, X2 = o.X, Y1 = 0, Y2 = c.ActualHeight, Stroke = Brushes.Yellow });

        var poly = new Polyline { Stroke = Brushes.Red, StrokeThickness = 2 };
        var totalRange = c.ActualWidth / XFactorSlider.Value;

        var vars = new Dictionary<string, double>();
        for (var x = totalRange / -2d; x <= totalRange; x += .01)
        {
            vars[variableName] = x;
            var y = _tree.Eval(vars);
            if (!double.IsNaN(y))
            {
                poly.Points.Add(new Point(x * XFactorSlider.Value + o.X, y * -YFactorSlider.Value + o.Y));
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

    public VisualNode ToVisualTree(NodeRoot root)
    {
        var visualRoot = new VisualNode(root, null);
        var queue = new Queue<VisualNode>(new[] { visualRoot });

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var child in current.Node.Children)
            {
                var visualChild = new VisualNode(child, current);
                current.Children.Add(visualChild);
                queue.Enqueue(visualChild);
            }
        }
        return visualRoot;
    }

    private void DrawTree()
    {
        var b = new BuchheimWalker();
        var visualTree = ToVisualTree(_tree);
        b.Run(visualTree);

        var q = new Queue<VisualNode>(new[] { visualTree });
        var mid2 = TreeCanvas.ActualWidth / 2;

        while (q.Count > 0)
        {
            var current = q.Dequeue();

            Canvas.SetLeft(current.Grid, current.X + mid2 - (current.Width / 2));
            Canvas.SetTop(current.Grid, current.Y + BuchheimWalker.VerticalMargin);

            TreeCanvas.Children.Add(current.Grid);

            foreach (var child in current.Children)
            {
                q.Enqueue(child);

                var line = new Line
                {
                    X1 = current.X + current.Width * 0.5 + mid2 - (current.Width / 2),
                    X2 = child.X + child.Width * 0.5 + mid2 - (child.Width / 2),
                    Y1 = current.Y + current.Height * 0.5 + BuchheimWalker.VerticalMargin,
                    Y2 = child.Y + child.Height * 0.5 + BuchheimWalker.VerticalMargin,
                    Stroke = Brushes.Yellow,

                };
                TreeCanvas.Children.Add(line);
                Panel.SetZIndex(line, -1);
            }
        }


        AddTickChars();

        TokenTextBox.Text = _tree.LeftOverToken;
        DepthTextBox.Text = _tree.CurrentDepth.ToString();
        LastOpTextBox.Text = _tree.LastOperation.Op.Name;
        SubstringTextBox.Text = InputTextBox.Text.Substring(0, (int)LengthLimitSlider.Value);
        VariablesTextBox.Text = string.Join(", ", _tree.Variables);
        //Eval.Text = string.Empty;
        Result.Text = string.Empty;
    }

    private void LengthLimitSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_isTextChange)
        {
            BuildTree();
        }
    }
        

    private void FactorSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
        Render();


    private void TreeCanvas_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var halfDelta = (e.NewSize.Width - e.PreviousSize.Width) / 2;
        foreach (var child in TreeCanvas.Children.OfType<UIElement>())
        {
            if (child is Line line)
            {
                line.X2 += halfDelta;
                line.X1 += halfDelta;
            }
            else
            {
                var prevLeft = Canvas.GetLeft(child);
                Canvas.SetLeft(child, prevLeft + halfDelta);
            }
        }
    }

    private void TestSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        BuchheimWalker.HorizontalMargin = TestSlider.Value;
        Render();
    }
}