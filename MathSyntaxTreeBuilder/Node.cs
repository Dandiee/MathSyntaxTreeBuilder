using System.Diagnostics;
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

public class NodeRoot : NodeOp
{
    public string LeftOverToken { get; set; } = string.Empty;
    public int CurrentDepth { get; set; }
    public NodeOp LastOperation { get; set; }
    public List<NodeVariable> VariableNodes { get; } = new();
    public string VariablesText { get; private set; }
    public override string BuildExpression() => Children[0].BuildExpression();
    public override double Eval(Dictionary<string, double>? variables = null) => Children[0].Eval(variables);
    public override string Name => "Identity";

    public NodeRoot() : base(Op.Identity, -1)
    {
        LastOperation = this;
    }

    public void CalculateVariables()
    {
        foreach (var variableNode in VariableNodes)
        {
            (variableNode.Parent as NodeOp).AddDependency(variableNode.Name);
        }

        VariablesText = string.Join(", ", DependsOn);
    }
}


[DebuggerDisplay("{Op.Name}")]
public class NodeOp : Node
{
    public HashSet<string> DependsOn { get; } = new(StringComparer.OrdinalIgnoreCase);

    public readonly Op Op;

    public NodeOp(Op op, int scopeDepth) : base(scopeDepth) { Op = op; }

    public override string ToString() => Op.Name;

    public override string BuildExpression()
    {
        if (Parent != null && Parent.ScopeDepth > -1 && Parent.ScopeDepth < ScopeDepth && Parent is NodeOp op && !op.Op.IsNamedFunction)
        {
            return $"({Op.ToStringFunc(this)})";
        }

        return $"{Op.ToStringFunc(this)}";

    }
    public override double Eval(Dictionary<string, double>? variables = null) => Op.EvalFunc(this, variables);
    public override string Name => Op.Name;

    public void AddArg(string argument)
    {
        if (NodeNamedConstant.Constants.ContainsKey(argument))
        {
            Children.Add(new NodeNamedConstant(this, argument, ScopeDepth));
        }
        else if (double.TryParse(argument, out var numericalValue))
        {
            Children.Add(new NodeUserConstant(this, ScopeDepth, numericalValue));
        }
        else
        {
            Children.Add(new NodeVariable(this, ScopeDepth, argument));
        }
    }

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
        if (!string.IsNullOrEmpty(argument))
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
        newHead.Children.Remove(oldChild);        
        newHead.Children.Add(nodeToAdd);

        nodeToAdd.Parent = newHead;
        nodeToAdd.Children.Add(oldChild);
        oldChild.Parent = nodeToAdd;
        this.Parent = oldChild;

        return nodeToAdd;
    }

    public void AddDependency(string variableName)
    {
        if (DependsOn.Add(variableName) && this.Parent is NodeOp op)
        {
            op.AddDependency(variableName);
        }
    }
}


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

public class NodeUserConstant : NodeArg
{
    public readonly double Value;
    public double Delta { get; set; }

    public NodeUserConstant(NodeOp parent, int scopeDepth, double value) 
        : base(parent, scopeDepth)
    {
        Value = value;
        Name = Value.ToString("N2");
    }

    public override string BuildExpression() => Value.ToString("N2");

    public override double Eval(Dictionary<string, double>? variables = null) => Value + Delta;

    public override string Name { get; }
}

public class NodeVariable : NodeArg
{
    public NodeVariable(NodeOp parent, int scopeDepth, string name) 
        : base(parent, scopeDepth)
    {
        Name = name;

        Node root = this;
        while (root is not NodeRoot)
        {
            root = root.Parent;
        }

        ((NodeRoot)root).VariableNodes.Add(this);
    }

    public override string BuildExpression() => Name;

    public override double Eval(Dictionary<string, double>? variables = null) => variables![Name];

    public override string Name { get; }
}

public abstract class NodeArg : Node
{
    protected NodeArg(NodeOp parent, int depth)
        : base(depth)
    {
        Parent = parent;
    }
}
