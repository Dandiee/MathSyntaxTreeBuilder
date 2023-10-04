using System.Diagnostics;
using Trees;

namespace MathSyntaxTreeBuilder;

public abstract class Node : BaseNode<Node>
{
    public abstract string BuildString();
    public abstract double Eval(Dictionary<string, double>? variables = null);
    public readonly int ScopeDepth;
    public abstract string Name { get; }

    protected Node(int scopeDepth)
    {
        ScopeDepth = scopeDepth;
    }
}

public class NodeRoot : NodeOp
{
    public string LeftOverToken { get; set; } = string.Empty;
    public int CurrentDepth { get; set; }
    public NodeOp LastOperation { get; set; }
    public HashSet<string> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);
    public string VariablesText => string.Join(", ", Variables);
    public override string BuildString() => Children[0].BuildString();
    public override double Eval(Dictionary<string, double>? variables = null) => Children[0].Eval(variables);
    public override string Name => "Identity";

    public NodeRoot() : base(Op.Identity, -1)
    {
        LastOperation = this;
    }
}


[DebuggerDisplay("{Op.Name}")]
public class NodeOp : Node
{
    public readonly Op Op;

    public NodeOp(Op op, int scopeDepth) : base(scopeDepth) { Op = op; }

    public override string ToString() => Op.Name;
    public override string BuildString() => $"{Op.ToStringFunc(this)}";
    public override double Eval(Dictionary<string, double>? variables = null) => Op.EvalFunc(this, variables);
    public override string Name => Op.Name;

    public void AddArg(string value) => Children.Add(new NodeArg(value, ScopeDepth + 1));

    public NodeOp AddOp(NodeOp nodeToAdd, string? argument)
    {
        // it's a more important node
        // the new node is *, the last is +, so the new must be deeper
        // add node to the current children
        var nodeToAddIsMoreImportant = nodeToAdd.ScopeDepth == ScopeDepth
            ? nodeToAdd.Op.Precedent >= Op.Precedent
            : nodeToAdd.ScopeDepth >= ScopeDepth;


        if (nodeToAddIsMoreImportant)
        {
            // eg.: 1+2*4, the parent + will adopt the * as its child
            Children.Add(nodeToAdd);
            nodeToAdd.Parent = this;

            // the argument, if available belongs to the more important node
            // eg.: 1+2*4, the op * will takes the new argument 2
            if (!string.IsNullOrEmpty(argument))
            {
                nodeToAdd.AddArg(argument);
            }

            return nodeToAdd;
        }

        // it's a less important node
        // for eg the nodeToAdd is + and the current one is *
        // the new add must be the new parent
        // it needs to find a way in the tree where it fits

        // the argument goes with the more important node
        // so to where we tried to insert the node originally
        // eg.: 1*2+4, the argument '2' goes to the more important *
        if (argument != null)
        {
            AddArg(argument);
        }

        var newHead = this;
        // this is the recursive part, se the example (1+2)*(3-1)+1
        // when the last +1 arrives, the last node is the '-' but we need to move two steps up, not just one
        // not just the new head is interesting, maybe the nodeToAdd will be between some way higher level nodes
        var oldChild = this;
        var isSufficient = false;
        while (!isSufficient)
        {
            // here, the '<' is important for Precedent
            // with '<=' the order of the operations will be reserved
            // eg: 1-2+3, first we evaluate 1-2 then the result with +3
            // it matters if there's another operand sandwiched between them
            // 1-2*5+7 => (1-(2*5))+7 => (1-(10))+7 => (-9)+7 = -2      // first grouped together because of '<'
            // vs
            // 1-2*5+7 => 1-((2*5)+7) =>  1-((10)+7) => 1-(17) = -16    // second grouped together because of '<='

            // the '<' for the scopeDepth is just as good as '<='
            // because when the  depths are equal, the depths wont matter anymore
            isSufficient = newHead.ScopeDepth == nodeToAdd.ScopeDepth
                ? newHead.Op.Precedent < nodeToAdd.Op.Precedent
                : newHead.ScopeDepth < nodeToAdd.ScopeDepth;

            if (isSufficient)
            {
                break;
            }

            oldChild = newHead;
            newHead = newHead.Parent as NodeOp;

            if (newHead == null) throw new Exception("Syntax error");
        }

        // the found node must be re-parented
        // for eg.: 1*5+6, the new node '+' must be created as a new parent for the '*'
        newHead.Children.Remove(oldChild);        // identity op 
        newHead.Children.Add(nodeToAdd);

        nodeToAdd.Parent = newHead;
        nodeToAdd.Children.Add(oldChild);
        Parent = nodeToAdd;

        return nodeToAdd;
    }
}

[DebuggerDisplay("{Value}")]
public class NodeArg : Node
{
    public static readonly IReadOnlyDictionary<string, double> Constants = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
    {
        ["pi"] = Math.PI,
        ["e"] = Math.E,
        ["tau"] = Math.Tau,
    };

    public readonly string Value;
    public readonly double DoubleValue;
    public readonly string VariableName;
    public readonly bool IsNumerical;
    public double Delta;

    public NodeArg(string value, int depth)
        : base(depth)
    {
        Value = value;

        if (Constants.TryGetValue(value, out var constValue))
        {
            Value = value.ToLowerInvariant();
            DoubleValue = constValue;
            IsNumerical = true;
        }
        else if (double.TryParse(value, out var doubleValue))
        {
            IsNumerical = true;
            DoubleValue = doubleValue;
            Value = doubleValue.ToString("N2");
        }
        else
        {
            VariableName = value;
        }

    }

    public override string ToString() => Value;
    public override string BuildString() => Value;

    public override double Eval(Dictionary<string, double>? variables = null)
        => IsNumerical ? DoubleValue + Delta : variables[VariableName];

    public override string Name => Value;
}
