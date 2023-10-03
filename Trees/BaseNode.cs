namespace Trees;

public interface IBaseNode<TNode>
    where TNode : class, IBaseNode<TNode>
{
    public TNode? Parent { get; set; }
 
    public List<TNode> Children { get; }
    int IndOf(IBaseNode<TNode> node);
}

public class BaseNode<TNode> : IBaseNode<TNode>
    where TNode :class, IBaseNode<TNode>
{
    public TNode? Parent { get; set; }
    public List<TNode> Children { get; } = new();

    public TNode? GetLastChild() => Children.Count > 0 ? Children[^1] : null;
    public TNode? GetFirstChild() => Children.Count > 0 ? Children[0] : null;
    public TNode? GetPrevSibling() =>
        Parent != null && Parent.Children[0] != this
            ? Parent.Children[Parent.IndOf(this) - 1] : null;
    public TNode? GetNextSibling() =>
        Parent != null && Parent.Children[^1] != this
            ? Parent.Children[Parent.IndOf(this) + 1] : null;

    public int IndOf(IBaseNode<TNode> node)
    {
        for (var i = 0; i < Children.Count; i++)
        {
            if (Children[i] == node)
            {
                return i;
            }
        }

        return -1;
    }

    public BaseNode(TNode? parent)
    {
        Parent = parent;
    }
}

public interface IPayloadNode<TNode, TPayload> : IBaseNode<TNode>
    where TNode : class, IBaseNode<TNode>
{
    public TPayload? Payload { get; set; }
    public IPayloadNode<TNode, TPayload> Owner { get; set; }
}

public abstract class PayloadNode<TNode, TPayload> : BaseNode<TNode>, IPayloadNode<TNode, TPayload>
    where TNode : class, IBaseNode<TNode>
{
    public TPayload? Payload { get; set; }
    public IPayloadNode<TNode, TPayload> Owner { get; set; }

    protected PayloadNode(TNode? parent, TPayload? payload) 
        : base(parent)
    {
    }
}

public interface IVisualNodeDescription
{
    public double X { get; set; }
    public double Y { get; set; }

    public double Width { get; set; }
    public double Height { get; set; }
}

public abstract class VisualNodeDescription : IVisualNodeDescription
{
    public double X { get; set; }
    public double Y { get; set; }

    public double Width { get; set; }
    public double Height { get; set; }
}

//public class VisualNode<TNode, TPayload> : BaseNode<TNode, TPayload>, IVisualNode<TNode>
//{
//    public double X { get; set; }
//    public double Y { get; set; }

//    public double Width { get; set; }
//    public double Height { get; set; }

//    protected VisualNode(TPayload payload, TNode? parent)
//        : base(payload, parent)
//    {
//        Value = payload;
//    }
//}


//public interface IBaseNode<TNode, TPayload> : IBaseNode<TNode>
//{
//    public TPayload Value { get; set; }
//}

//public class BaseNode<TNode, TPayload> : BaseNode<TNode>, IBaseNode<TNode, TPayload>
//{
//    public TPayload Value { get; set; }

//    public BaseNode(TPayload value, TNode? parent)
//        : base(parent)
//    {
//        Value = value;
//    }
//}
