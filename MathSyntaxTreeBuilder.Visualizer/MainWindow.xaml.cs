using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Prism.Commands;
using Prism.Mvvm;

namespace MathSyntaxTreeBuilder.Visualizer;

public class ViewModel : BindableBase
{
    public const string DefaultInput = "sin((5+4*cos(1))*cos(2))";

    private readonly MainWindow _window;

    private string _userInput;
    public string UserInput
    {
        get => _userInput;
        set
        {
            if (SetProperty(ref _userInput, value))
            {
                ProcessedInput = value;
                LengthLimit = value.Length;
            }
        }
    }

    private string _processedInput;
    public string ProcessedInput
    {
        get => _processedInput;
        set
        {
            if (SetProperty(ref _processedInput, value))
            {
                Tree = MathSyntaxBuilder.GetSyntaxTree(value);
            }
        }
    }

    private int _lengthLimit;
    public int LengthLimit
    {
        get => _lengthLimit;
        set
        {
            if (SetProperty(ref _lengthLimit, value))
            {
                ProcessedInput = UserInput.Substring(0, value);
            }
        }
    }

    private NodeRoot? _tree;
    public NodeRoot? Tree
    {
        get => _tree;
        set
        {
            if (SetProperty(ref _tree, value))
            {
                VisualTree = value == null ? null : ToVisualTree(value);
            }
        }
    }

    private VisualNode? _visualTree;
    public VisualNode? VisualTree
    {
        get => _visualTree;
        set
        {
            if (SetProperty(ref _visualTree, value))
            {
                Render();
            }
        }
    }

    private double _functionXFactor = 100;
    public double FunctionXFactor
    {
        get => _functionXFactor;
        set
        {
            if (SetProperty(ref _functionXFactor, value))
            {
                DrawFunction();
            }
        }
    }

    private double _functionYFactor = 100;
    public double FunctionYFactor
    {
        get => _functionYFactor;
        set
        {
            if (SetProperty(ref _functionYFactor, value))
            {
                DrawFunction();
            }
        }
    }

    private double _treeVerticalSpacing = 30;
    public double TreeVerticalSpacing
    {
        get => _treeVerticalSpacing;
        set
        {
            if (SetProperty(ref _treeVerticalSpacing, value))
            {
                DrawTree();
            }
        }
    }

    private double _treeHorizontalSpacing = 30;
    public double TreeHorizontalSpacing
    {
        get => _treeHorizontalSpacing;
        set
        {
            if (SetProperty(ref _treeHorizontalSpacing, value))
            {
                DrawTree();
            }
        }
    }

    public ICommand TreeCanvasSizeChanged { get; }
    public ICommand FunctionCanvasSizeChanged { get; }

    public ViewModel(MainWindow window)
    {
        _window = window;
        UserInput = DefaultInput;
        TreeCanvasSizeChanged = new DelegateCommand<SizeChangedEventArgs>(ResizeTree);
        FunctionCanvasSizeChanged = new DelegateCommand<SizeChangedEventArgs>(ResizeFunction);
    }

    private void Render()
    {
        DrawTree();
        DrawFunction();
        AddTickChars();
    }

    public void DrawFunction()
    {
        _window.FunctionCanvas.Children.Clear();
        var c = _window.FunctionCanvas;
        var actualWidth = c.ActualWidth == 0 ? 500 : c.ActualWidth;
        var actualHeight = c.ActualHeight == 0 ? 500 : c.ActualHeight;
        var o = new Point(actualWidth / 2, actualHeight / 2);
        c.Children.Add(new Line { X1 = 0, X2 = actualWidth, Y1 = o.Y, Y2 = o.Y, Stroke = Brushes.Yellow });
        c.Children.Add(new Line { X1 = o.X, X2 = o.X, Y1 = 0, Y2 = actualHeight, Stroke = Brushes.Yellow });

        if (_tree == null) return;
        if (_tree.Variables.Count != 1) return;

        var variableName = _tree.Variables.First();

        var poly = new Polyline { Stroke = Brushes.Red, StrokeThickness = 2 };
        var totalRange = actualWidth / FunctionXFactor;

        var vars = new Dictionary<string, double>();
        for (var x = totalRange / -2d; x <= totalRange; x += .01)
        {
            vars[variableName] = x;
            var y = _tree.Eval(vars);
            if (!double.IsNaN(y))
            {
                poly.Points.Add(new Point(x * FunctionXFactor + o.X, y * -FunctionYFactor + o.Y));
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

    private void ResizeFunction(SizeChangedEventArgs e)
    {
        var halfXDelta = (e.NewSize.Width - e.PreviousSize.Width) / 2;
        var halfYDelta = (e.NewSize.Height - e.PreviousSize.Height) / 2;

        DrawFunction();
    }

    private void DrawTree()
    {
        if (VisualTree == null) return;

        BuchheimWalker.VerticalMargin = TreeVerticalSpacing;
        BuchheimWalker.HorizontalMargin = TreeHorizontalSpacing;
        var walker = new BuchheimWalker();
        walker.Run(VisualTree);

        var queue = new Queue<VisualNode>(new[] { VisualTree });
        var actualWidth = _window.TreeCanvas.ActualWidth == 0 ? 500 : _window.TreeCanvas.ActualWidth;
        var mid = (actualWidth / 2);

        var graphWidthOffset = -(walker.MinX.Value + walker.TotalWidth / 2);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            Canvas.SetLeft(current.Grid, current.X + graphWidthOffset + mid - (current.Width / 2));
            Canvas.SetTop(current.Grid, current.Y + BuchheimWalker.VerticalMargin);

            foreach (var child in current.Children)
            {
                queue.Enqueue(child);

                child.Line!.X1 = current.X + graphWidthOffset + current.Width * 0.5 + mid - (current.Width / 2);
                child.Line!.X2 = child.X + graphWidthOffset + child.Width * 0.5 + mid - (child.Width / 2);
                child.Line!.Y1 = current.Y + current.Height * 0.5 + BuchheimWalker.VerticalMargin;
                child.Line!.Y2 = child.Y + child.Height * 0.5 + BuchheimWalker.VerticalMargin;
            }
        }
    }

    private void ResizeTree(SizeChangedEventArgs e)
    {
        var halfDelta = (e.NewSize.Width - e.PreviousSize.Width) / 2;
        foreach (var child in _window.TreeCanvas.Children.OfType<UIElement>())
        {
            if (child is Line line)
            {
                line.X2 += halfDelta;
                line.X1 += halfDelta;
            }
            else Canvas.SetLeft(child, Canvas.GetLeft(child) + halfDelta);
        }
    }

    private void AddTickChars()
    {
        _window.CharactersGrid.ColumnDefinitions.Clear();
        _window.CharactersGrid.Children.Clear();

        _window.CharactersGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var s = new TextBlock { Text = " " };
        _window.CharactersGrid.Children.Add(s);
        Grid.SetColumn(s, 0);

        for (var index = 0; index < UserInput.Length; index++)
        {
            var c = UserInput[index];
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
        _window.TreeCanvas.Children.Clear();

        var visualRoot = new VisualNode(root, null, _window.TreeCanvas, this);
        var queue = new Queue<VisualNode>(new[] { visualRoot });

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var child in current.Node.Children)
            {
                var visualChild = new VisualNode(child, current, _window.TreeCanvas, this);
                current.Children.Add(visualChild);
                queue.Enqueue(visualChild);
            }
        }

        return visualRoot;
    }
}

public partial class MainWindow
{
    public ViewModel ViewModel { get; }

    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new ViewModel(this);
        DataContext = ViewModel;
    }
}