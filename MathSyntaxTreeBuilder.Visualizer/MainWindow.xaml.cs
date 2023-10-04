using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Prism.Mvvm;

namespace MathSyntaxTreeBuilder.Visualizer;

public class ViewModel : BindableBase
{
    public const string DefaultInput = "sin((5+4*cos(1))*cos(2))";

    private readonly MainWindow _window;

    private bool _isInputChange;
    private string _input = DefaultInput;
    public string Input
    {
        get => _input;
        set
        {
            if (SetProperty(ref _input, value))
            {
                _isInputChange = true;
                BuildTree();
                _isInputChange = false;
            }
        }
    }

    private int _lengthLimit = DefaultInput.Length - 1;
    public int LengthLimit
    {
        get => _lengthLimit;
        set
        {
            if (SetProperty(ref _lengthLimit, value) && !_isInputChange)
            {
                BuildTree();
            }
        }
    }

    private NodeRoot? _tree;
    public NodeRoot? Tree
    {
        get => _tree;
        set => SetProperty(ref _tree, value);
    }

    private VisualNode? _visualTree;
    public VisualNode? VisualTree
    {
        get => _visualTree;
        set => SetProperty(ref _visualTree, value);
    }

    public ViewModel(MainWindow window)
    {
        _window = window;
        BuildTree();
    }

    private void BuildTree()
    {
        Tree = MathSyntaxBuilder.GetSyntaxTree(Input, LengthLimit);
        VisualTree = ToVisualTree(Tree);
        Render();
    }

    private void Render()
    {
        _window.CharactersGrid.ColumnDefinitions.Clear();
        _window.CharactersGrid.Children.Clear();
        _window.TreeCanvas.Children.Clear();

        DrawTree();
        DrawFunction();
        AddTickChars();
    }

    private void DrawFunction()
    {
        if (_tree.Variables.Count != 1) return;

        var variableName = _tree.Variables.First();

        var c = _window.FunctionCanvas; 
        c.Children.Clear();
        var o = new Point(c.ActualWidth / 2, c.ActualHeight / 2);

        c.Children.Add(new Line { X1 = 0, X2 = c.ActualWidth, Y1 = o.Y, Y2 = o.Y, Stroke = Brushes.Yellow });
        c.Children.Add(new Line { X1 = o.X, X2 = o.X, Y1 = 0, Y2 = c.ActualHeight, Stroke = Brushes.Yellow });

        var poly = new Polyline { Stroke = Brushes.Red, StrokeThickness = 2 };
        var totalRange = c.ActualWidth / _window.XFactorSlider.Value;

        var vars = new Dictionary<string, double>();
        for (var x = totalRange / -2d; x <= totalRange; x += .01)
        {
            vars[variableName] = x;
            var y = _tree.Eval(vars);
            if (!double.IsNaN(y))
            {
                poly.Points.Add(new Point(x * _window.XFactorSlider.Value + o.X, y * -_window.YFactorSlider.Value + o.Y));
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
    private void DrawTree()
    {
        var queue = new Queue<VisualNode>(new[] { VisualTree });
        var mid = _window.TreeCanvas.ActualWidth / 2;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            Canvas.SetLeft(current.Grid, current.X + mid - (current.Width / 2));
            Canvas.SetTop(current.Grid, current.Y + BuchheimWalker.VerticalMargin);

            _window.TreeCanvas.Children.Add(current.Grid);

            foreach (var child in current.Children)
            {
                queue.Enqueue(child);

                var line = new Line
                {
                    X1 = current.X + current.Width * 0.5 + mid - (current.Width / 2),
                    X2 = child.X + child.Width * 0.5 + mid - (child.Width / 2),
                    Y1 = current.Y + current.Height * 0.5 + BuchheimWalker.VerticalMargin,
                    Y2 = child.Y + child.Height * 0.5 + BuchheimWalker.VerticalMargin,
                    Stroke = Brushes.Yellow,

                };
                _window.TreeCanvas.Children.Add(line);
                Panel.SetZIndex(line, -1);
            }
        }


        AddTickChars();

        _window.TokenTextBox.Text = Tree.LeftOverToken;
        _window.DepthTextBox.Text = Tree.CurrentDepth.ToString();
        _window.LastOpTextBox.Text = Tree.LastOperation.Op.Name;
        _window.SubstringTextBox.Text = Input.Substring(0, LengthLimit);
        _window.VariablesTextBox.Text = string.Join(", ", _tree.Variables);
        //Eval.Text = string.Empty;
        _window.Result.Text = string.Empty;
    }
    private void AddTickChars()
    {
        var text = Input.ToArray();
        _window.CharactersGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

        var s = new TextBlock { Text = " " };
        _window.CharactersGrid.Children.Add(s);
        Grid.SetColumn(s, 0);

        for (var index = 0; index < text.Length; index++)
        {
            var c = text[index];
            _window.CharactersGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            var tb = new TextBlock
            {
                Text = c.ToString()
            };

            _window.CharactersGrid.Children.Add(tb);
            Grid.SetColumn(tb, index + 1);
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
        new BuchheimWalker().Run(visualRoot);

        return visualRoot;
    }
}

public partial class MainWindow
{
    private bool _isTextChange = true;
    private bool _isInitialized = false;

    public ViewModel ViewModel { get; }

    private NodeRoot? _tree = null;
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new ViewModel(this);
        DataContext = ViewModel;
    }

    

  
    private void InputChanged(object sender, TextChangedEventArgs e)
    {
        //_isTextChange = true;
        //BuildTree();
        //LengthLimitSlider.Value = InputTextBox.Text.Length;
        //Render();
        //_isTextChange = false;
    }
    

   
  

    private void LengthLimitSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        //if (!_isTextChange)
        //{
        //    BuildTree();
        //}
    }


    private void FactorSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        //Render();
    }


    private void TreeCanvas_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        //var halfDelta = (e.NewSize.Width - e.PreviousSize.Width) / 2;
        //foreach (var child in TreeCanvas.Children.OfType<UIElement>())
        //{
        //    if (child is Line line)
        //    {
        //        line.X2 += halfDelta;
        //        line.X1 += halfDelta;
        //    }
        //    else
        //    {
        //        var prevLeft = Canvas.GetLeft(child);
        //        Canvas.SetLeft(child, prevLeft + halfDelta);
        //    }
        //}
    }

    private void TestSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        //BuchheimWalker.HorizontalMargin = TestSlider.Value;
        //Render();
    }
}