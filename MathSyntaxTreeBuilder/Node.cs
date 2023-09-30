using System.Diagnostics;

namespace MathSyntaxTreeBuilder;

public abstract class Node
{
    public Node? Parent { get; set; }
    public abstract string BuildString();
    public abstract double Eval();
    public readonly List<Node> Children = new();
}


[DebuggerDisplay("{Op.Name}")]
public class NodeOp : Node
{
    public readonly Op Op;
    public readonly int Depth;

    public NodeOp(Op op, int depth)
    {
        Op = op;
        Depth = depth;
    }

    public override string ToString() => Op.Name;

    public override string BuildString() => $"({Op.ToStringFunc(this)})";
    public override double Eval() => Op.EvalFunc(this);

    public void AddArg(string value) => Children.Add(new NodeArg(value));

    public NodeOp AddOp(NodeOp nodeToAdd, string? argument)
    {
        // it's a more important node
        // the new node is *, the last is +, so the new must be deeper
        // add node to the current children

        var nodeToAddIsMoreImportant = nodeToAdd.Depth == Depth
            ? nodeToAdd.Op.Precedent >= Op.Precedent
            : nodeToAdd.Depth >= Depth;


        if (nodeToAddIsMoreImportant)
        {
            // eg.: 1+2*4, the parent + will adopt the * as its child
            Children.Add(nodeToAdd);
            nodeToAdd.Parent = this;

            // the argument, if available belongs to the more important node
            // eg.: 1+2*4, the op * will takes the new argument 2
            if (argument != null)
            {
                nodeToAdd.AddArg(argument!); Debug.Assert(argument != null);
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
            isSufficient = newHead.Depth == nodeToAdd.Depth
                ? newHead.Op.Precedent <= nodeToAdd.Op.Precedent
                : newHead.Depth <= nodeToAdd.Depth;
                //newHead.Depth <= nodeToAdd.Depth && newHead.Op.Precedent <= nodeToAdd.Op.Precedent;
            if (isSufficient)
            {
                break;
            }

            oldChild = newHead;
            newHead = newHead.Parent as NodeOp; Debug.Assert(newHead != null);
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
    public readonly string Value;
    public readonly double DoubleValue;
    public readonly string VariableName;

    public NodeArg(string value)
    {
        Value = value;
        if (double.TryParse(value, out var doubleValue))
        {
            DoubleValue = doubleValue;
        }
        else
        {
            VariableName = value;
        }

    }

    public override string ToString() => Value;
    public override string BuildString() => Value;
    public override double Eval() => DoubleValue!;
}
