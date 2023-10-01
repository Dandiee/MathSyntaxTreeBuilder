using System.Diagnostics;

namespace MathSyntaxTreeBuilder;

public class MathSyntaxBuilder
{
    public static Node GetSyntaxTree(string input, int? length = null)
    {
        var token = string.Empty;
        var tokenDepth = 0;
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
                if (token != string.Empty)
                {
                    var namedOp = Op.ByKeys[token];
                    var newNode = new NodeOp(namedOp, depth);
                    node = node.AddOp(newNode, null);
                    token = string.Empty;
                }

                depth++;
            }
            else if (Op.ByKeys.ContainsKey(c.ToString()) ||
                     (!string.IsNullOrEmpty(token) && Op.ByKeys.ContainsKey(token)))
            {
                var isOpToken = Op.ByKeys.TryGetValue(token, out var opToken);
                var op = isOpToken ? opToken : Op.ByKeys[c.ToString()];
                var newNode = new NodeOp(op, depth);
                if (isOpToken) token = string.Empty;
                node = node.AddOp(newNode, token);
                token = string.Empty;
            }

            else
            {
                token += c.ToString();
                tokenDepth = depth;
            }
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

        return root;
    }

}