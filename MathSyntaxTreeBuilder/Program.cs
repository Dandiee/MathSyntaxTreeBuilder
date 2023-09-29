using System.Diagnostics;

namespace MathSyntaxTreeBuilder;

public class Program
{
    // Brackets, Exponents, Division/Multiplication, Addition/Subtraction
    public static void Main(string[] args)
    {
        Test();
    }

    public static Node GetSyntaxTree(string input)
    {
        input = input.Replace(" ", "");

        var currentToken = "";
        var scopeDepth = 0;

        OpNode? lastOp = null;

        for (var i = 0; i < input.Length; i++)
        {
            var currentChar = input[i];

            if (currentChar == ' ') ;
            else if (currentChar == '(')
            {
                if (Op.ByKeys.TryGetValue(currentToken, out var op))
                {
                    var newNode = new OpNode(Op.ByKeys[currentToken], scopeDepth);
                    if (lastOp != null)
                    {
                        var isMoreImportant = lastOp.ScopeDepth != scopeDepth
                            ? lastOp.ScopeDepth > scopeDepth
                            : lastOp.Op.Precedent >= newNode.Op.Precedent;

                        if (isMoreImportant)
                        {
                            lastOp.AddChild(new OpArgNode(currentToken));
                            newNode.AddChild(lastOp);
                        }
                        else
                        {
                            newNode.AddChild(new OpArgNode(currentToken));
                            lastOp.AddChild(newNode);
                        }
                    }

                    currentToken = "";
                    lastOp = newNode;
                }
                scopeDepth++;

            }
            else if (currentChar == ')')
            {
                scopeDepth--;
            }
            else
            {
                here:

                if (Op.ByKeys.ContainsKey(currentChar.ToString()))
                {
                    Debug.Assert(int.TryParse(currentToken, out _));
                   
                    var newNode = new OpNode(Op.ByKeys[currentChar.ToString()], scopeDepth);
                    if (lastOp != null)
                    {
                        var isMoreImportant = lastOp.ScopeDepth != scopeDepth
                            ? lastOp.ScopeDepth > scopeDepth
                            : lastOp.Op.Precedent >= newNode.Op.Precedent;

                        if (isMoreImportant)
                        {
                            lastOp.AddChild(new OpArgNode(currentToken));
                            newNode.AddChild(lastOp);
                        }
                        else
                        {
                            newNode.AddChild(new OpArgNode(currentToken));
                            lastOp.AddChild(newNode);
                        }
                    }
                    else
                    {
                        newNode.AddChild(new OpArgNode(currentToken));
                    }



                    currentToken = "";
                    lastOp = newNode;
                }
                else if (char.IsDigit(currentChar))
                {
                    currentToken += currentChar;
                }
                else
                {
                    currentToken += currentChar;
                }
            }
        }

        lastOp.Children.Add(new OpArgNode(currentToken));

        Node root = lastOp;

        while (root.Parent != null)
        {
            root = root.Parent!;
        }

        return root;
    }

    public static void Test()
    {
        Test("sin(1 + 7)", "(sin((1+7)))", 0.98935824662);
        Test("sin(1)", "(sin(1))", 0.8414709848);
        Test("4 ^ (5 + 6)", "(4^(5+6))", 4194304);
        Test("4 ^ 5 + 6", "((4^5)+6)", 1030);
        Test("4 + (5 * 6)", "(4+(5*6))", 34);
        Test("(4 + 5) * 6", "((4+5)*6)", 54);
        Test("4 + 5 * 6", "(4+(5*6))", 34);
        Test("4 * 5 + 6", "((4*5)+6)", 26);
        Test("4 + 5 - 6", "((4+5)-6)", 3);
        Test("4 + 5 + 6", "((4+5)+6)", 15);
        Test("4 + 5", "(4+5)", 9);

    }

    public static void Test(string input, string expectedOutput, double? expectedEval)
    {
        var result = GetSyntaxTree(input);
        var output = result.BuildString();
        expectedOutput = expectedOutput.Replace(" ", "");
        if (expectedEval.HasValue)
        {
            var eval = result.Eval();
            Debug.Assert(Math.Abs(eval - expectedEval.Value) < 0.0001d);
        }
        

        Debug.Assert(output == expectedOutput);
    }




    public abstract class Node
    {
        public Node? Parent { get; set; }
        public abstract string BuildString();
        public abstract double Eval();
    }

    [DebuggerDisplay("{Op.Name}")]
    public class OpNode : Node
    {
        public readonly Op Op;
        public readonly int ScopeDepth;
        public readonly List<Node> Children = new();

        public void AddChild(Node child)
        {
            Children.Add(child);
            child.Parent = this;
        }

        public OpNode(Op op, int scopeDepth)
        {
            Op = op;
            ScopeDepth = scopeDepth;
        }

        public override string ToString() => Op.Name;

        public override string BuildString() => $"({Op.ToStringFunc(this)})";
        public override double Eval() => Op.EvalFunc(this);
    }

    [DebuggerDisplay("{Value}")]
    public class OpArgNode : Node
    {
        public readonly string Value;
        public readonly double DoubleValue;

        public OpArgNode(string value)
        {
            Value = value;
            DoubleValue = double.Parse(value);
        }

        public override string ToString() => Value;
        public override string BuildString() => Value;
        public override double Eval() => DoubleValue;
    }

    public struct Op
    {
        public static readonly Dictionary<string, Op> ByKeys = new();

        public static readonly Op Add = new("+", 1, 2,
            node => node.Children[0].Eval() + node.Children[1].Eval(),
            node => $"{node.Children[0].BuildString()}+{node.Children[1].BuildString()}");

        public static readonly Op Subtract = new("-", 1, 2,
            node => node.Children[0].Eval() - node.Children[1].Eval(),
            node => $"{node.Children[0].BuildString()}-{node.Children[1].BuildString()}");

        public static readonly Op Mul = new("*", 2, 2,
            node => node.Children[0].Eval() * node.Children[1].Eval(),
            node => $"{node.Children[0].BuildString()}*{node.Children[1].BuildString()}");

        public static readonly Op Div = new("/", 2, 2,
            node => node.Children[0].Eval() / node.Children[1].Eval(),
            node => $"{node.Children[0].BuildString()}/{node.Children[1].BuildString()}");

        public static readonly Op Exp = new("^", 3, 2,
            node => Math.Pow(node.Children[0].Eval(), node.Children[1].Eval()),
            node => $"{node.Children[0].BuildString()}^{node.Children[1].BuildString()}");

        public static readonly Op Sin = new("sin", 3, 2,
            node => Math.Sin(node.Children[0].Eval()),
            node => $"sin({node.Children[0].BuildString()})");

        public readonly int Precedent;
        public readonly string Name;
        public readonly int OperandsCount;
        public readonly Func<OpNode, string> ToStringFunc;
        public readonly Func<OpNode, double> EvalFunc;

        public Op(string name, int precedent, int operandsCount, 
            Func<OpNode, double> evalFunc,
            Func<OpNode, string> toStringFunc)
        {
            EvalFunc = evalFunc;
            Name = name;
            Precedent = precedent;
            OperandsCount = operandsCount;
            ToStringFunc = toStringFunc;

            ByKeys[name] = this;
        }
    }
}
