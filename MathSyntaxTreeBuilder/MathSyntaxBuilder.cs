using System.Diagnostics;
using System.Xml;

namespace MathSyntaxTreeBuilder;

public class MathSyntaxBuilder
{
    public static Node GetSyntaxTree(string input)
    {
        var normalizedInput = input.Trim().Replace(" ", "").ToLowerInvariant();

        var token = string.Empty;
        var depth = 0;
        var root = new NodeOp(Op.Identity, -1);
        NodeOp node = root;

        foreach (var c in normalizedInput)
        {
            if (c == ',') { }
            else if (c == ')')
            {
                depth--;
            }
            else if (c == '(')
            {
                depth++;
            }
            else if (Op.ByKeys.ContainsKey(c.ToString()) || (!string.IsNullOrEmpty(token) && Op.ByKeys.ContainsKey(token)))
            {
                var isOpToken = Op.ByKeys.TryGetValue(token, out var opToken);
                var op = isOpToken ? opToken : Op.ByKeys[c.ToString()];

                var newNode = new NodeOp(op, depth);

                if (isOpToken) token = string.Empty;

                node = node.AddOp(newNode, token);
                token = string.Empty;
            }
            else token += c.ToString();
        }

        if (token.Length > 0)
        {
            node.AddArg(token);
        }
        
        
        return root!;
    }

}