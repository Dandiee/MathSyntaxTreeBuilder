namespace MathSyntaxTreeBuilder;

public class NodeVariable : NodeArg
{
    public bool IsPolynomialTerm { get; private set; }

    public NodeVariable(NodeOp parent, int scopeDepth, string name)
        : base(parent, scopeDepth)
    {
        Name = name;
    }

    public static readonly HashSet<Op> PolynomialOperations = new[]
    {
        Op.Mul, Op.Abs, Op.Subtract, Op.Identity, Op.PowChar, Op.Add, Op.Pow
    }.ToHashSet(OpComparer.Instance);

    public bool UpdatePolynomial()
    {
        var op = Parent as NodeOp;
        while (op != null)
        {
            if (op.Op.Equals(Op.PowChar) || op.Op.Equals(Op.Pow))
            {
                var lhs = op.Children[0];
                var rhs = op.Children[1];

                if (rhs == this)
                {
                    IsPolynomialTerm = false;
                    return false;
                }

                if (rhs is NodeVariable)
                {
                    IsPolynomialTerm = false;
                    return false;
                }

                if (rhs is NodeOp rhsOp && rhsOp.DependsOn.Count > 0)
                {
                    IsPolynomialTerm = false;
                    return false;
                }


                var value = rhs.Eval();
                if (value < 0 || !double.IsInteger(value))
                {
                    IsPolynomialTerm = false;
                    return false;
                }
            }

            if (!PolynomialOperations.Contains(op.Op))
            {
                IsPolynomialTerm = false;
                return false;
            }

            op = op.Parent as NodeOp;
        }

        IsPolynomialTerm = true;
        return true;
    }

    public override string BuildExpression() => Name;

    public override double Eval(Dictionary<string, double>? variables = null) => variables![Name];

    public override string Name { get; }
}