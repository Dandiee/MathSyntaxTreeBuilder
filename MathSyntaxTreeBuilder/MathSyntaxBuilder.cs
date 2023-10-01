using System.Diagnostics;

namespace MathSyntaxTreeBuilder;

public class MathSyntaxBuilder
{
    public static NodeRoot GetSyntaxTree(string input, int? length = null)
    {
        var token = string.Empty;
        var depth = 0;
        var root = new NodeRoot();
        var node = root as NodeOp;

        for (var index = 0; index < (length ?? input.Length); index++)
        {
            var c = input[index];
            if (c == ' ') continue;
            if (c == ',')
            {
                if (token != string.Empty)
                {
                    node.AddArg(token);
                    token = string.Empty;
                    var namedOp = node;
                    while (!(namedOp.Op.IsMultiVariableFunction && namedOp.Depth == depth - 1))
                    {
                        namedOp = (NodeOp)namedOp.Parent;
                    }
                    node = namedOp;
                }
            }
            else if (c == ')') depth--;
            else if (c == '(')
            {
                if (token != string.Empty) // it's not just simple scoping
                {

                    // named ops: "sin", "cos", "min", "max", ...
                    if (Op.ByKeys.TryGetValue(token, out var op))
                    {
                        node = node.AddOp(new NodeOp(op, depth), null);
                        token = string.Empty;
                    }
                    //else Debug.Assert(false, "Hidden multiplication is not supported yet.");
                    //TODO: it must be a hidden multiplication
                    //{ 
                    //
                    //    var mulArg = token == "-"
                    //        ? "-1"
                    //        : token;
                    //
                    //    node = node.AddOp(new NodeOp(Op.Mul, depth), mulArg);
                    //    token = string.Empty;
                    //}
                }

                depth++;
            }
            else if (Op.ByKeys.TryGetValue(c.ToString(), out var op)) // character ops: '+', '-', '/', '*', '^'
            {
                // it's a negative token not an operation
                if (op.Equals(Op.Subtract) && token == string.Empty)
                {
                    token += "-";
                    continue;
                }

                node = node.AddOp(new NodeOp(op, depth), token);
                token = string.Empty;
            }
            else token += c.ToString();
        }

        if (!length.HasValue || length.Value == input.Length)
        {
            if (token.Length > 0)
            {
                node.AddArg(token);
            }
        }

        root.LeftOverToken = token;
        root.CurrentDepth = depth;
        root.LastOperation = node;

        FinalizeRoot(root);

        return root;
    }

    private static void FinalizeRoot(NodeRoot root)
    {
        var queue = new Queue<Node>(new Node[] { root });
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (node is NodeArg arg)
            {
                if (!arg.IsNumerical)
                {
                    root.Variables.Add(arg.Value);
                }
            }
            else
            {
                foreach (var child in node.Children)
                {
                    queue.Enqueue(child);
                }
            }
        }
    }

}