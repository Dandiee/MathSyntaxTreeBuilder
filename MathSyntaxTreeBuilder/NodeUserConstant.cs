namespace MathSyntaxTreeBuilder;

public class NodeUserConstant : NodeArg
{
    public double Value;
    public double Delta { get; set; }

    public NodeUserConstant(NodeOp parent, int scopeDepth, double value)
        : base(parent, scopeDepth)
    {
        Value = value;
        Name = Value.ToString("N2");
    }

    public override string BuildExpression() => double.IsInteger(Value + Delta)
        ? (Value + Delta).ToString("N0")
        : (Value + Delta).ToString("N2");

    public override double Eval(Dictionary<string, double>? variables = null) => Value + Delta;

    public override string Name { get; }
}