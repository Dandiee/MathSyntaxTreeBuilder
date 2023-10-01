using System.Xml.Linq;

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
                if (token != string.Empty)
                {
                    var namedOp = Op.ByKeys[token];
                    var newNode = new NodeOp(namedOp, depth);
                    node = node.AddOp(newNode, null);
                    token = string.Empty;
                }

                depth++;
            }
            else if (Op.ByKeys.TryGetValue(c.ToString(), out var op))
            {
                if (op.Equals(Op.Subtract) && token == string.Empty)
                {
                    token += "-";
                    continue;
                }

                var newNode = new NodeOp(op, depth);
                node = node.AddOp(newNode, null);
            }
            else
            {
                token += c.ToString();
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