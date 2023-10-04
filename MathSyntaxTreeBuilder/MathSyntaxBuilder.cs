namespace MathSyntaxTreeBuilder;

public class MathSyntaxBuilder
{
    public static NodeRoot GetSyntaxTree(string input)
    {
        var token = string.Empty;
        var depth = 0;
        var root = new NodeRoot();
        var node = root as NodeOp;

        foreach (var c in input)
        {
            if (c == ' ') continue;
            if (c == ',')
            {
                if (token != string.Empty)
                {
                    node.AddArg(token);
                    token = string.Empty;
                    var namedOp = node;
                    while (!(namedOp.Op.IsMultiVariableFunction && namedOp.ScopeDepth == depth - 1))
                    {
                        namedOp = namedOp.Parent as NodeOp;
                        if (namedOp == null) throw new Exception("Syntax error");
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
                    else throw new Exception("Syntax error");

                    //else Debug.Assert(false, "Hidden multiplication is not supported yet.");
                    //TODO: it must be a hidden multiplication
                    // implicit multiplication is all over the place, examples:
                    // 2x, (1+2)(3+4), -cos(1), -5sin(1), even 1--1 can be a impl mul
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
            // character ops: '+', '-', '/', '*', '^'
            else if (Op.ByKeys.TryGetValue(c.ToString(), out var op))
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

        if (token.Length > 0)
        {
            node.AddArg(token);
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