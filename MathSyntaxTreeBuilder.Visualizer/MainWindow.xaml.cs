using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace MathSyntaxTreeBuilder.Visualizer;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

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
                Eval.Text = tree.Eval().ToString();
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

    private void Draw(Node root)
    {
        Clear();
        AddTickChars();
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

            Canvas.SetLeft(current.Border, current.HorizontalOffset);
            Canvas.SetTop(current.Border, current.VerticalOffset);

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

            TreeCanvas.Children.Add(current.Border);

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
    public Border Border { get; }
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
            Background = new SolidColorBrush(Colors.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0),
            BorderBrush = new SolidColorBrush(Colors.Yellow),
            BorderThickness = new Thickness(2),
            Child = new TextBlock
            {
                Text = Text,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 18
            }
        };
    }
}