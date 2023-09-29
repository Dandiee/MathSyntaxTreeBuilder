namespace MathSyntaxTreeBuilder;

public class MathSyntaxBuilder
{
    public static Node GetSyntaxTree(string input)
    {
        var currentToken = "";
        var scopeDepth = 0;
        OpNode? lastOp = null;
        var variables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var namedFunctionDepth = 0;

        foreach (var currentChar in input)
        {
            if (currentChar == ' ') ;
            else if (currentChar == ',')
            {
                //Debug.Assert(lastOp.Op.OperandsCount == 2);
                lastOp.AddChild(new OpArgNode(currentToken));
                currentToken = string.Empty;
                //lastOp = (OpNode)lastOp.Parent;
                //scopeDepth--;
                //while (lastOp.Parent is OpNode op && lastOp.ScopeDepth != scopeDepth)

                while (true)
                {
                    if (lastOp.Op.IsMultiVariableFunction && lastOp.Children.Count < lastOp.Op.OperandsCount)
                    {
                        break;
                    }

                    lastOp = (OpNode)lastOp.Parent;
                }

                //if (lastOp.Children.Count == lastOp.Op.OperandsCount)
                //{
                //    do
                //    {
                //        lastOp = lastOp.Parent as OpNode;
                //    } while (lastOp != null && lastOp.Children.Count == lastOp.Op.OperandsCount);
                //}
            }
            else if (currentChar == ')')
            {
                scopeDepth--;
            }
            else if (currentChar == '(')
            {
                // it's a named function
                if (Op.ByKeys.TryGetValue(currentToken, out var op))
                {
                    var newNode = new OpNode(Op.ByKeys[currentToken], scopeDepth);
                    if (lastOp != null)
                    {
                        var isMoreImportant = lastOp.ScopeDepth != scopeDepth
                            ? lastOp.ScopeDepth > scopeDepth
                            : lastOp.Op.Precedent >= newNode.Op.Precedent;

                        if (isMoreImportant)
                        {
                            newNode.AddChild(lastOp);
                        }
                        else
                        {
                            lastOp.AddChild(newNode);
                        }
                    }

                    currentToken = "";
                    lastOp = newNode;
                }
                // it's a hidden multiplication with an an op arg
                // for eg.: 5(4+5)
                else if (currentToken != string.Empty)
                {
                    var newNode = new OpNode(Op.Mul, scopeDepth);
                    if (lastOp != null)
                    {
                        var isMoreImportant = lastOp.ScopeDepth != scopeDepth
                            ? lastOp.ScopeDepth > scopeDepth
                            : lastOp.Op.Precedent >= newNode.Op.Precedent;

                        if (isMoreImportant)
                        {
                            newNode.AddChild(lastOp);
                            lastOp.AddChild(new OpArgNode(currentToken));
                        }
                        else
                        {
                            lastOp.AddChild(newNode);
                            newNode.AddChild(new OpArgNode(currentToken));
                        }
                    }
                    else
                    {
                        newNode.AddChild(new OpArgNode(currentToken));
                    }

                    currentToken = "";
                    lastOp = newNode;
                }
                
                scopeDepth++;

            }
            
            else
            {
                if (Op.ByKeys.ContainsKey(currentChar.ToString()))
                {
                    var newNode = new OpNode(Op.ByKeys[currentChar.ToString()], scopeDepth);
                    if (lastOp != null)
                    {
                        var isMoreImportant = lastOp.ScopeDepth != scopeDepth
                            ? lastOp.ScopeDepth > scopeDepth
                            : lastOp.Op.Precedent > newNode.Op.Precedent;

                        if (isMoreImportant)
                        {
                            if (currentToken != string.Empty)
                            {
                                lastOp.AddChild(new OpArgNode(currentToken));
                            }

                            newNode.AddChild(lastOp);
                        }
                        else
                        {
                            if (currentToken != string.Empty)
                            {
                                newNode.AddChild(new OpArgNode(currentToken));
                            }

                            lastOp.AddChild(newNode);
                        }
                    }
                    else
                    {
                        if (currentToken != string.Empty)
                        {
                            newNode.AddChild(new OpArgNode(currentToken));
                        }
                    }

                    currentToken = "";
                    lastOp = newNode;
                }
                else if (char.IsDigit(currentChar))
                {
                    currentToken += currentChar;
                }
                else
                { 
                    currentToken += currentChar;
                }
            }
        }

        if (lastOp == null)
        {
            lastOp = new OpNode(Op.Identity, 0);
        }

        lastOp.AddChild(new OpArgNode(currentToken));

        Node root = lastOp;

        while (root.Parent != null)
        {
            root = root.Parent!;
        }

        return root;
    }
    
}