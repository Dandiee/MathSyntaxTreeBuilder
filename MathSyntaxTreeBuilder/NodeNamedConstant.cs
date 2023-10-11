namespace MathSyntaxTreeBuilder;

public class NodeNamedConstant : NodeArg
{
    public override string Name { get; }

    public static readonly IReadOnlyDictionary<string, double> Constants = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
    {
        ["pi"] = Math.PI,
        ["e"] = Math.E,
        ["tau"] = Math.Tau,
    };

    public NodeNamedConstant(NodeOp parent, string constName, int scopeDepth)
        : base(parent, scopeDepth)
    {
        Name = constName;
    }

    public override string BuildExpression() => Name;

    public override double Eval(Dictionary<string, double>? variables = null) => Constants[Name];

}