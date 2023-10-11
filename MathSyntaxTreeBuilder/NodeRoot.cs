using System.Diagnostics;

namespace MathSyntaxTreeBuilder;

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
    public bool IsPolynomial { get; private set; }

    public NodeRoot() : base(Op.Identity, -1)
    {
        LastOperation = this;
    }

    private void CollectAllVariables()
    {
        VariableNodes.Clear();

        var queue = new Queue<Node>(new[] { this });
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current is NodeVariable variable)
            {
                VariableNodes.Add(variable);
            }
            else if (current is NodeOp op)
            {
                op.DependsOn.Clear();
            }

            foreach (var child in current.Children)
            {
                queue.Enqueue(child);
            }
        }
    }

    public void CalculateVariables()
    {
        IsPolynomial = true;

        CollectAllVariables();

        foreach (var variableNode in VariableNodes)
        {
            (variableNode.Parent as NodeOp).AddDependency(variableNode.Name);
        }

        foreach (var variableNode in VariableNodes)
        {
            IsPolynomial &= variableNode.UpdatePolynomial();
        }

        VariablesText = string.Join(", ", DependsOn);

        AssertRelationships();
    }

    public void AssertRelationships()
    {
        var queue = new Queue<Node>(new[] { this });
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var child in current.Children)
            {
                Debug.Assert(child.Parent == current);
                queue.Enqueue(child);
            }
        }
    }
}