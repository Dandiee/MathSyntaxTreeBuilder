using System.Xml.Linq;
namespace Trees;

public class BuchheimVisualNodeDescription<TNode> : VisualNodeDescription
    where TNode : PayloadNode<TNode, BuchheimVisualNodeDescription<TNode>>
{
    public double Prelim { get; set; }
    public int Number { get; set; }
    public double Mod { get; set; }
    public double Change { get; set; }
    public double Shift { get; set; }

    public TNode? Ancestor { get; set; }
    public TNode? Thread { get; set; }

    public BuchheimVisualNodeDescription()
    {
        
    }
    


    //public BuchheimNode<TPayload>? GetLastChild() => Children.Count > 0 ? Children[^1] : null;
    //public BuchheimNode<TPayload>? GetFirstChild() => Children.Count > 0 ? Children[0] : null;
    //public BuchheimNode<TPayload>? GetPrevSibling() =>
    //    Parent != null && Parent.Children[0] != this
    //        ? Parent.Children[Parent.Children.IndexOf(this) - 1] : null;
    //public BuchheimNode<TPayload>? GetNextSibling() =>
    //    Parent != null && Parent.Children[^1] != this
    //        ? Parent.Children[Parent.Children.IndexOf(this) + 1] : null;

    //public BuchheimNode(TPayload payload, BuchheimNode<TPayload>? parent) 
    //    : base(payload, parent)
    //{
        
    //}
}
