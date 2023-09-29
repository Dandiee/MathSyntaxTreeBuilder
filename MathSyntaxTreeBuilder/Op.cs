namespace MathSyntaxTreeBuilder;

public struct Op
{
    public static readonly Op Identity = new("", 0, 1,
        node => node.Children[0].Eval(),
        node => $"{node.Children[0].BuildString()}");

    public static readonly Dictionary<string, Op> ByKeys = new();



    public static readonly Op Add = new("+", 1, 2,
        node => node.Children[0].Eval() + node.Children[1].Eval(),
        node => $"{node.Children[0].BuildString()}+{node.Children[1].BuildString()}");

    public static readonly Op Subtract = new("-", 1, 2,
        node => node.Children.Count == 2
            ? node.Children[0].Eval() - node.Children[1].Eval()
            : -node.Children[0].Eval(),
        node => node.Children.Count == 2
            ? $"{node.Children[0].BuildString()}-{node.Children[1].BuildString()}"
            : $"-{node.Children[0].BuildString()}");

    public static readonly Op Mul = new("*", 2, 2,
        node => node.Children[0].Eval() * node.Children[1].Eval(),
        node => $"{node.Children[0].BuildString()}*{node.Children[1].BuildString()}");

    public static readonly Op Div = new("/", 2, 2,
        node => node.Children[0].Eval() / node.Children[1].Eval(),
        node => $"{node.Children[0].BuildString()}/{node.Children[1].BuildString()}");

    public static readonly Op Exp = new("^", 3, 2,
        node => Math.Pow(node.Children[0].Eval(), node.Children[1].Eval()),
        node => $"{node.Children[0].BuildString()}^{node.Children[1].BuildString()}");

    public static readonly Op Max = new("max", 3, 2,
        node => Math.Max(node.Children[0].Eval(), node.Children[1].Eval()),
        node => $"max({node.Children[0].BuildString()},{node.Children[1].BuildString()})", true, true);

    public static readonly Op Min = new("min", 3, 2,
        node => Math.Min(node.Children[0].Eval(), node.Children[1].Eval()),
        node => $"min({node.Children[0].BuildString()},{node.Children[1].BuildString()})", true, true);

    public static readonly Op Sin = new("sin", 3, 1,
        node => Math.Sin(node.Children[0].Eval()),
        node => $"sin({node.Children[0].BuildString()})", true);

    public static readonly Op Cos = new("cos", 3, 1,
        node => Math.Cos(node.Children[0].Eval()),
        node => $"cos({node.Children[0].BuildString()})", true);

    public static readonly Op Tan = new("tan", 3, 1,
        node => Math.Tan(node.Children[0].Eval()),
        node => $"tan({node.Children[0].BuildString()})", true);

    public readonly int Precedent;
    public readonly string Name;
    public readonly int OperandsCount;
    public readonly Func<OpNode, string> ToStringFunc;
    public readonly bool IsNamedFunction;
    public readonly Func<OpNode, double> EvalFunc;
    public readonly bool IsMultiVariableFunction;

    public Op(string name, int precedent, int operandsCount,
        Func<OpNode, double> evalFunc,
        Func<OpNode, string> toStringFunc,
        bool isNamedFunction = false,
        bool isMultiVariableFunction = false)
    {
        IsMultiVariableFunction = isMultiVariableFunction;
        EvalFunc = evalFunc;
        Name = name;
        Precedent = precedent;
        OperandsCount = operandsCount;
        ToStringFunc = toStringFunc;
        IsNamedFunction = isNamedFunction;

        if (ByKeys != null)
        {
            ByKeys[name] = this;
        }
    }
}