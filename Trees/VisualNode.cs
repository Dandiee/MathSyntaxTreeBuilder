namespace Trees;

public abstract class VisualNode<TNode, TPayload> : BaseNode<TNode>
{
    public TPayload Payload { get; set; }

    public double X { get; set; }
    public double Y { get; set; }

    public double Width { get; set; }
    public double Height { get; set; }
}