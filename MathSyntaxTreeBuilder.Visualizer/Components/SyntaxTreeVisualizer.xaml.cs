using Prism.Mvvm;

namespace MathSyntaxTreeBuilder.Visualizer.Components
{
    public partial class SyntaxTreeVisualizer
    {
        public SyntaxTreeVisualizer()
        {
            InitializeComponent();
        }
    }

    public sealed class SyntaxTreeVisualizerViewModel : BindableBase
    {
        public void SetInput(NodeRoot input)
        {
            Input = input;
        }

        private NodeRoot? _input;
        public NodeRoot? Input
        {
            get => _input;
            private set => SetProperty(ref _input, value);
        }
    }
}
