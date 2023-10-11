namespace MathSyntaxTreeBuilder;

public abstract class NodeArg : Node
{
    protected NodeArg(NodeOp parent, int depth)
        : base(depth)
    {
        Parent = parent;
    }
}