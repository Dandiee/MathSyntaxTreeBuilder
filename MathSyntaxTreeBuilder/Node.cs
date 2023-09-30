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
public class OpNode : Node
{
    public readonly Op Op;
    public readonly int ScopeDepth;

    public void AddChild(Node child)
    {
        if (Children.Count == Op.OperandsCount)
        {
            OverwriteChild(child);
        }
        else
        {
            if (child.Parent != null)
            {
                var originalParent = child.Parent;
                originalParent.Children.Remove(child);
                originalParent.Children.Add(this);

                this.Parent = originalParent;
            }



            Children.Add(child);
            child.Parent = this;
        }
    }

    public void AddOperand(string value)
    {
        Children.Add(new OpArgNode(value));
    }

    // sin-ben vagyunk, egy gyereke van, a cos, most lenne kettő a szorzással
    public void OverwriteChild(Node child)
    {
        // remove the last children from the original
        var lastOriginalChild = Children[^1];
        Children.Remove(lastOriginalChild);

        // add the new children to the 
        this.Children.Add(child);
        child.Parent = this;


        child.Children.Add(lastOriginalChild);
        lastOriginalChild.Parent = child;

        // add original child to the new child
        

        
        //child.Parent?.Children.Remove(child);
        //this.Parent = child.Parent;
        //var temp = child.Parent;
        //child.Parent = this;

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
    public readonly string VariableName;

    public OpArgNode(string value)
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
    public override double Eval() => DoubleValue;
}
