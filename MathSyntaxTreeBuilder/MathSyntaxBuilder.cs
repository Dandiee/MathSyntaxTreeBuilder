using System.Diagnostics;
using System.Xml;

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

        var namedOpStack = new Stack<(OpNode? Named, int Dept)>();

        foreach (var currentChar in input)
        {
            if (currentChar == ' ') ;
            else if (currentChar == ',')
            {
                if (currentToken != string.Empty)
                {
                    lastOp.AddOperand(currentToken);
                    currentToken = string.Empty;
                    //lastOp = (OpNode)lastOp.Parent;
                    //while (true)
                    //{
                    //    if (lastOp.Op.IsMultiVariableFunction && lastOp.Children.Count < lastOp.Op.OperandsCount)
                    //    {
                    //        break;
                    //    }
                    //
                    //    lastOp = (OpNode)lastOp.Parent;
                    //}
                }

                while (true)
                {
                    if (lastOp.Op.IsMultiVariableFunction && lastOp.Children.Count < lastOp.Op.OperandsCount)
                    {
                        break;
                    }

                    lastOp = (OpNode)lastOp.Parent;
                }
            }
            else if (currentChar == ')')
            {
                var poppedScope = namedOpStack.Pop();
                scopeDepth--;
                if (poppedScope.Named != null)
                {
                    if (currentToken != string.Empty)
                    {
                        lastOp.AddOperand(currentToken);
                        currentToken = string.Empty;
                    }
                    lastOp = (OpNode)poppedScope.Named;
                }

                //
                //if ((OpNode)lastOp.Parent != null)
                //{
                //    lastOp = (OpNode)lastOp.Parent;
                //}

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
                            : lastOp.Op.Precedent > newNode.Op.Precedent;

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
                

                namedOpStack.Push(new(lastOp, scopeDepth));


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

                        if (!isMoreImportant)
                        {
                            if (lastOp.ScopeDepth == scopeDepth &&
                                lastOp.Op.Precedent == newNode.Op.Precedent)
                            {
                                isMoreImportant = true;
                            }
                        }

                        if (isMoreImportant)
                        {
                            if (currentToken != string.Empty)
                            {
                                lastOp.AddOperand(currentToken);
                            }

                            newNode.AddChild(lastOp);
                        }
                        else
                        {
                            if (currentToken != string.Empty)
                            {
                                newNode.AddOperand(currentToken);
                            }

                            lastOp.AddChild(newNode);
                        }
                    }
                    else
                    {
                        if (currentToken != string.Empty)
                        {
                            newNode.AddOperand(currentToken);
                        }
                    }

                    currentToken = "";
                    lastOp = newNode;
                }
                else if (char.IsDigit(currentChar))
                {
                    if (currentChar == '2')
                    {

                    }

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

        if (currentToken != string.Empty)
        {
            lastOp.AddOperand(currentToken);
        }


        Node root = lastOp;

        while (root.Parent != null)
        {
            root = root.Parent!;
        }

        return root;
    }

}