using System.Globalization;
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

    private string? _result;
    public string? Result
    {
        get => _result;
        set => SetProperty(ref _result, value);
    }

    private string? _outputExpression;
    public string? OutputExpression
    {
        get => _outputExpression;
        set => SetProperty(ref _outputExpression, value);
    }

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
                MathVisualExpression = value == null ? null : ToMathVisualNode(value);
                Result = value == null
                    ? null
                    : value.DependsOn.Count > 0
                        ? $"Depends on variable(s): {{{value.VariablesText}}}"
                        : $"{value.Eval()}";
                OutputExpression = value?.BuildExpression();
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

    private MathVisualNode? _mathVisualExpression;
    public MathVisualNode? MathVisualExpression
    {
        get => _mathVisualExpression;
        set
        {
            if (SetProperty(ref _mathVisualExpression, value))
            {
                try
                {
                    DrawMathExpression();
                }
                catch { }
                
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
    public ICommand ReduceCommand { get; }

    public ViewModel(MainWindow window)
    {
        _window = window;
        UserInput = DefaultInput;
        TreeCanvasSizeChanged = new DelegateCommand<SizeChangedEventArgs>(ResizeTree);
        FunctionCanvasSizeChanged = new DelegateCommand<SizeChangedEventArgs>(ResizeFunction);
        ReduceCommand = new DelegateCommand(Reduce);
    }

    private void Reduce()
    {
        if (Tree == null) return;

        var stack = new Stack<Node>(new [] { Tree });
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            
            var currentOp = current as NodeOp;
            var parentOp = current.Parent as NodeOp;

            if (currentOp != null && parentOp != null)
            {

                if (currentOp.Op.Equals(Op.Mul) && 
                    currentOp.Children.All(e => e is NodeOp cOp && 
                                                (cOp.Op.Equals(Op.Pow) || cOp.Op.Equals(Op.PowChar))))
                {
                    var lhsPow = currentOp.Children[0] as NodeOp;
                    var rhsPow = currentOp.Children[1] as NodeOp;

                    if (lhsPow.Children[0] is NodeVariable lhsVar &&
                        rhsPow.Children[0] is NodeVariable rhsVar &&
                        lhsVar.Name == rhsVar.Name)
                    {
                        if (lhsPow.Children[1] is NodeUserConstant lhsConst &&
                            rhsPow.Children[1] is NodeUserConstant rhsConst)
                        {

                        }
                    }

                }
                else if ((currentOp.Op.Equals(Op.Pow) || currentOp.Op.Equals(Op.PowChar)) &&
                    parentOp.Op.Equals(Op.Mul))
                {
                    var curLhs = currentOp.Children[0];
                    var curRhs = currentOp.Children[1];

                    if (curLhs is NodeVariable curLhsVar && curRhs is NodeUserConstant curRhsExp)
                    {
                        var parentOfParent = parentOp.Parent as NodeOp;
                        parentOfParent.Children.Remove(parentOp);
                        parentOp.Parent = null;

                        var newOperation = new NodeOp(Op.Pow, parentOp.ScopeDepth);
                        newOperation.AddArg(curLhs.Name);
                        newOperation.AddArg((curRhsExp.Value + 1).ToString(CultureInfo.InvariantCulture));

                        parentOfParent.Children.Add(newOperation);
                        newOperation.Parent = parentOfParent;


                        RefreshTree(); return;

                    }
                }
                else if (currentOp.Op.Equals(Op.Mul) && currentOp.Children.All(c => c is NodeVariable))
                {
                    var newOperation = new NodeOp(Op.Pow, currentOp.ScopeDepth)
                    {
                        Parent = parentOp
                    };
                    newOperation.AddArg(currentOp.Children[0].Name);
                    newOperation.AddArg("2");

                    currentOp.Parent = null;

                    parentOp.Children.Remove(currentOp);
                    parentOp.Children.Add(newOperation);

                    RefreshTree(); return;

                }
                else if (currentOp.Op.Equals(Op.Mul) && parentOp.Op.Equals(Op.Mul))
                {
                    var parVar = parentOp.Children.OfType<NodeVariable>().SingleOrDefault();
                    var curVar = currentOp.Children.OfType<NodeVariable>().SingleOrDefault();

                    if (parVar != null && curVar != null)
                    {
                        // drop the original var of the parent
                        // it will be replaced with a new operation
                        parentOp.Children.Remove(parVar);
                        parVar.Parent = null;

                        // create the new pow operation and fill it with arguments
                        var newOperation = new NodeOp(Op.Pow, parVar.ScopeDepth)
                        {
                            Parent = parentOp
                        };
                        newOperation.AddArg(parVar.Name);
                        newOperation.AddArg("2");

                        // sew the new operation to its parent
                        parentOp.Children.Add(newOperation);

                        // kill the child
                        parentOp.Children.Remove(currentOp);
                        currentOp.Parent = null;
                        current.Children.Remove(curVar);

                        // only one child left at this point
                        var transitiveChild = currentOp.Children.Single();
                        transitiveChild.Parent = parentOp;
                        parentOp.Children.Add(transitiveChild);

                        RefreshTree(); return;
                    }
                }
            }


            foreach (var child in current.Children)
            {
                stack.Push(child);
            }
        }

        return;
        var variables = Tree.VariableNodes;
        foreach (var variable in variables)
        {
            Node n = variable.Parent;
            while (n != null)
            {
                if (n is not NodeOp op) break;

                if (op.Op.Equals(Op.Mul))
                {
                    if (op.Children[0] is NodeVariable lhs && lhs.Name == variable.Name &&
                        op.Children[1] is NodeVariable rhs && rhs.Name == variable.Name)
                    {
                        var newOp =  op.ReplaceLeafOperation(new NodeOp(Op.PowChar, op.ScopeDepth));
                        newOp.AddArg(variable.Name);
                        newOp.AddArg("2");

                        goto here;
                    }
                }

                if (op.Op.Equals(Op.PowChar))
                {
                    if (op.Children[0] is NodeVariable lhs && lhs.Name == variable.Name)
                    {
                        if (op.Children[1] is NodeUserConstant userConst
                            || (op.Children[1] is NodeOp rhsOp && !rhsOp.DependsOn.Contains(variable.Name)))
                        {

                        }
                    }
                }

                n = n.Parent;
            }
        }

        here:
        var temp = Tree;
        //temp.VariableNodes.Clear();
        temp.CalculateVariables();
        temp.AssertRelationships();
        Tree = null;
        Tree = temp;

        
    }

    private void Render()
    {
        DrawTree();
        DrawFunction();
        AddTickChars();
    }

    private void RefreshTree()
    {
        var t = Tree;
        t.AssertRelationships();
        Tree = null;
        Tree = t;
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
        if (_tree.DependsOn.Count != 1) return;

        var variableName = _tree.DependsOn.First();

        var poly = new Polyline { Stroke = Brushes.Red, StrokeThickness = 2 };
        var totalRange = actualWidth / FunctionXFactor;

        var vars = new Dictionary<string, double>();
        try
        {
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

        }
        catch
        {
            return;
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

    private void DrawMathExpression()
    {
        if (MathVisualExpression == null) return;
        _window.MathExpressionGrid.Children.Clear();
        _window.MathExpressionGrid.Children.Add(MathVisualExpression.GetVisual());
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

    public MathVisualNode ToMathVisualNode(NodeRoot root)
    {
        var visualRoot = new MathVisualNode(root, null, this);
        var queue = new Queue<MathVisualNode>(new[] { visualRoot });

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var child in current.Node.Children)
            {
                var visualChild = new MathVisualNode(child, current, this);
                current.Children.Add(visualChild);
                queue.Enqueue(visualChild);
            }
        }

        return visualRoot;
    }
}