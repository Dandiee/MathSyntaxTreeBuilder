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
                    if (Op.ByKeys.TryGetValue(token, out var op))
                    {
                        node = node.AddOp(new NodeOp(op, depth), null);
                        token = string.Empty;
                    }
                    else
                    {
                        node = node.AddOp(new NodeOp(Op.Mul, depth), token);
                        token = string.Empty;
                    }
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
            else
            {
                if (token != string.Empty && double.TryParse(token, out _) && !char.IsDigit(c) && c != '.')
                {
                    node = node.AddOp(new NodeOp(Op.Mul, depth), token);
                    token = c.ToString();
                }
                else
                {
                    token += c.ToString();
                }
            }
        }

        if (token.Length > 0)
        {
            node.AddArg(token);
        }

        root.LeftOverToken = token;
        root.CurrentDepth = depth;
        root.LastOperation = node;

        root.CalculateVariables();
#if DEBUG
        root.AssertRelationships();
#endif
        return root;
    }

  

}