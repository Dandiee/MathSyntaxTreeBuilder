namespace Trees;

public abstract class BaseNode<TNode> 
    where TNode : class
{
    public TNode? Parent { get; set; }
    public List<TNode> Children { get; } = new();
}