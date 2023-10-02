namespace Trees;

public abstract class BaseNode<TNode>
{
    public BaseNode<TNode>? Parent { get; set; }
    public List<TNode> Children { get; } = new();
}

public class TorpNode : BaseNode<int>
{
    void Travelsal()
    {
        //this.Children[0].Parent
    }
}