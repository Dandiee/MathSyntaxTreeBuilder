namespace MathSyntaxTreeBuilder;

public class OpComparer : IEqualityComparer<Op>
{
    public static readonly OpComparer Instance = new();

    public bool Equals(Op x, Op y)
    {
        return x.Precedent == y.Precedent && x.Name == y.Name && x.OperandsCount == y.OperandsCount && x.ToStringFunc.Equals(y.ToStringFunc) && x.IsNamedFunction == y.IsNamedFunction && x.EvalFunc.Equals(y.EvalFunc) && x.IsMultiVariableFunction == y.IsMultiVariableFunction;
    }

    public int GetHashCode(Op obj)
    {
        return HashCode.Combine(obj.Precedent, obj.Name, obj.OperandsCount, obj.ToStringFunc, obj.IsNamedFunction, obj.EvalFunc, obj.IsMultiVariableFunction);
    }
}

public struct Op
{
    public static Random Rnd = new(564654);

    public static readonly Op Identity = new("", 0, 1,
        (node, vars) => node.Children[0].Eval(vars),
        node => $"{node.Children[0].BuildExpression()}");

    public static readonly Dictionary<string, Op> ByKeys = new();

    public static readonly Op Add = new("+", 1, 2,
        (node, vars) => node.Children[0].Eval(vars) + node.Children[1].Eval(vars),
        node => $"{node.Children[0].BuildExpression()}+{node.Children[1].BuildExpression()}");

    public static readonly Op Subtract = new("-", 1, 2,
        (node, vars) => node.Children.Count == 2
            ? node.Children[0].Eval(vars) - node.Children[1].Eval(vars)
            : -node.Children[0].Eval(vars),
        node => node.Children.Count == 2
            ? $"{node.Children[0].BuildExpression()}-{node.Children[1].BuildExpression()}"
            : $"-{node.Children[0].BuildExpression()}");

    public static readonly Op Mul = new("*", 2, 2,
        (node, vars) => node.Children[0].Eval(vars) * node.Children[1].Eval(vars),
        node => $"{node.Children[0].BuildExpression()}*{node.Children[1].BuildExpression()}");

    public static readonly Op Div = new("/", 2, 2,
        (node, vars) => node.Children[0].Eval(vars) / node.Children[1].Eval(vars),
        node => $"{node.Children[0].BuildExpression()}/{node.Children[1].BuildExpression()}");

    public static readonly Op PowChar = new("^", 3, 2,
        (node, vars) => Math.Pow(node.Children[0].Eval(vars), node.Children[1].Eval(vars)),
        node => $"{node.Children[0].BuildExpression()}^{node.Children[1].BuildExpression()}", true, true);

    public static readonly Op Pow = new("pow", 3, 2,
        (node, vars) => Math.Pow(node.Children[0].Eval(vars), node.Children[1].Eval(vars)),
        node => $"pow({node.Children[0].BuildExpression()},{node.Children[1].BuildExpression()})", true, true);

    public static readonly Op Log = new("log", 3, 2,
        (node, vars) => Math.Log(node.Children[0].Eval(vars), node.Children[1].Eval(vars)),
        node => $"log({node.Children[0].BuildExpression()},{node.Children[1].BuildExpression()})", true, true);

    public static readonly Op Exp = new("exp", 3, 2,
        (node, vars) => Math.Exp(node.Children[0].Eval(vars)),
        node => $"exp({node.Children[0].BuildExpression()})", true);

    public static readonly Op Max = new("max", 3, 2,
        (node, vars) => Math.Max(node.Children[0].Eval(vars), node.Children[1].Eval(vars)),
        node => $"max({node.Children[0].BuildExpression()},{node.Children[1].BuildExpression()})", true, true);

    public static readonly Op Min = new("min", 3, 2,
        (node, vars) => Math.Min(node.Children[0].Eval(vars), node.Children[1].Eval(vars)),
        node => $"min({node.Children[0].BuildExpression()},{node.Children[1].BuildExpression()})", true, true);

    public static readonly Op Sin = new("sin", 3, 1,
        (node, vars) => Math.Sin(node.Children[0].Eval(vars)),
        node => $"sin({node.Children[0].BuildExpression()})", true);

    public static readonly Op Sinh = new("sinh", 3, 1,
        (node, vars) => Math.Sinh(node.Children[0].Eval(vars)),
        node => $"sinh({node.Children[0].BuildExpression()})", true);

    public static readonly Op Cos = new("cos", 3, 1,
        (node, vars) => Math.Cos(node.Children[0].Eval(vars)),
        node => $"cos({node.Children[0].BuildExpression()})", true);

    public static readonly Op Cosh = new("cosh", 3, 1,
        (node, vars) => Math.Cosh(node.Children[0].Eval(vars)),
        node => $"cosh({node.Children[0].BuildExpression()})", true);

    public static readonly Op Atan = new("atan", 3, 1,
        (node, vars) => Math.Atan(node.Children[0].Eval(vars)),
        node => $"atan({node.Children[0].BuildExpression()})", true);

    public static readonly Op Atanh = new("atanh", 3, 1,
        (node, vars) => Math.Atanh(node.Children[0].Eval(vars)),
        node => $"atanh({node.Children[0].BuildExpression()})", true);

    public static readonly Op Tan = new("tan", 3, 1,
        (node, vars) => Math.Tan(node.Children[0].Eval(vars)),
        node => $"tan({node.Children[0].BuildExpression()})", true);

    public static readonly Op Abs = new("abs", 3, 1,
        (node, vars) => Math.Abs(node.Children[0].Eval(vars)),
        node => $"abs({node.Children[0].BuildExpression()})", true);

    public static readonly Op Sign = new("sign", 3, 1,
        (node, vars) => Math.Sign(node.Children[0].Eval(vars)),
        node => $"sign({node.Children[0].BuildExpression()})", true);

    public static readonly Op Tanh = new("tanh", 3, 1,
        (node, vars) => Math.Tanh(node.Children[0].Eval(vars)),
        node => $"tanh({node.Children[0].BuildExpression()})", true);

    public static readonly Op Sqrt = new("sqrt", 3, 1,
        (node, vars) => Math.Sqrt(node.Children[0].Eval(vars)),
        node => $"sqrt({node.Children[0].BuildExpression()})", true);

    public static readonly Op Rand = new("rand", 3, 1,
        (node, vars) => Rnd.NextDouble(),
        node => $"rand()", true);

    public static readonly Op Clamp = new("clamp", 3, 1,
        (node, vars) => Math.Clamp(node.Children[0].Eval(vars), node.Children[1].Eval(vars), node.Children[2].Eval(vars)),
        node => $"clamp({node.Children[0].BuildExpression()})", true, true);


    public readonly int Precedent;
    public readonly string Name;
    public readonly int OperandsCount;
    public readonly Func<NodeOp, string> ToStringFunc;
    public readonly bool IsNamedFunction;
    public readonly Func<NodeOp, Dictionary<string, double>?, double> EvalFunc;
    public readonly bool IsMultiVariableFunction;

    public Op(string name, int precedent, int operandsCount,
        Func<NodeOp, Dictionary<string, double>?, double> evalFunc,
        Func<NodeOp, string> toStringFunc,
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