using System.Diagnostics;

namespace MathSyntaxTreeBuilder;

[DebuggerDisplay("{Op.Name}")]
public class NodeOp : Node
{
    public HashSet<string> DependsOn { get; } = new(StringComparer.OrdinalIgnoreCase);

    // TODO: create a replace method
    // should be readonly i was just too lazy to write a proper "replace" function for reduction
    public readonly Op Op;

    public NodeOp(Op op, int scopeDepth) : base(scopeDepth) { Op = op; }

    public NodeOp? ParentOp => Parent is NodeOp parentOp ? parentOp : null;

    public override string ToString() => Op.Name;

    public void Kill()
    {
        ParentOp.RemoveChild(this);
    }

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

    public NodeOp ReplaceLeafOperation(NodeOp newOp, params NodeArg[] args)
    {
        Debug.Assert(Children.All(c => c is NodeArg));
        Debug.Assert(Parent is NodeOp);

        var parentOp = (NodeOp)Parent;
        parentOp.RemoveChild(this);
        parentOp.AddChild(newOp);

        foreach (var arg in args)
        {
            newOp.AddChild(arg);
        }

        return newOp;
    }

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
        var nodeToAddIsMoreImportant =
            /*nodeToAdd.Op.Equals(Op.Pow) || nodeToAdd.Op.Equals(Op.PowChar)*/ false ||
                                                                               (nodeToAdd.ScopeDepth == ScopeDepth
                                                                                   ? nodeToAdd.Op.Equals(Op.PowChar) && Op.Equals(Op.PowChar)
                                                                                       ? nodeToAdd.Op.Precedent >= Op.Precedent // right-to-left associative operators ('^')
                                                                                       : nodeToAdd.Op.Precedent > Op.Precedent
                                                                                   : nodeToAdd.ScopeDepth >= ScopeDepth);


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
                ? newHead.Op.Equals(Op.PowChar) && nodeToAdd.Op.Equals(Op.PowChar) // right-to-left associative operators
                    // pow (the char '^' only) is more complicated, it acts to the right, not to the left
                    // so the < becomes <=
                    ? newHead.Op.Precedent <= nodeToAdd.Op.Precedent // right-to-left associative operators
                    : newHead.Op.Precedent < nodeToAdd.Op.Precedent // left-to-right associative operators
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

        newHead.RemoveChild(oldChild);
        nodeToAdd.AddChild(oldChild);
        newHead.AddChild(nodeToAdd);
        return nodeToAdd;
    }

    public void AddChild(Node child, params string[]? args)
    {
        Children.Add(child);
        child.Parent = this;

        if (args != null)
        {
            foreach (var arg in args)
            {
                AddArg(arg);
            }
        }
    }

    private void RemoveChild(Node child)
    {
        child.Parent = null;
        Children.Remove(child);
    }

    public void AddDependency(string variableName)
    {
        if (DependsOn.Add(variableName) && this.Parent is NodeOp op)
        {
            op.AddDependency(variableName);
        }
    }
}