using Trees;

namespace MathSyntaxTreeBuilder;

public abstract class Node : BaseNode<Node>
{
    public abstract string BuildExpression();
    public abstract double Eval(Dictionary<string, double>? variables = null);
    public readonly int ScopeDepth;
    public abstract string Name { get; }

    protected Node(int scopeDepth)
    {
        ScopeDepth = scopeDepth;
    }
}