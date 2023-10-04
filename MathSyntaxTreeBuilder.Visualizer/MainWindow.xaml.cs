namespace MathSyntaxTreeBuilder.Visualizer;

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