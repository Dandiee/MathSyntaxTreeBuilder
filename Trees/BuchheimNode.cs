using System.Drawing;

namespace Trees;

public sealed class BuchheimNode<TPayload> : VisualNode<BuchheimNode<TPayload>, TPayload>
    //where TNode : BaseNode<TNode>
{
    public double Prelim { get; set; }
    public int Number { get; set; }
    public double Mod { get; set; }
    public double Change { get; set; }
    public double Shift { get; set; }

    public BuchheimNode<TPayload>? Ancestor { get; set; }
    public BuchheimNode<TPayload>? Thread { get; set; }
    

    public BuchheimNode<TPayload>? GetLastChild() => Children.Count > 0 ? Children[^1] : null;
    public BuchheimNode<TPayload>? GetFirstChild() => Children.Count > 0 ? Children[0] : null;
    public BuchheimNode<TPayload>? GetPrevSibling() =>
        Parent != null && Parent.Children[0] != this
            ? Parent.Children[Parent.Children.IndexOf(this) - 1] : null;
    public BuchheimNode<TPayload>? GetNextSibling() =>
        Parent != null && Parent.Children[^1] != this
            ? Parent.Children[Parent.Children.IndexOf(this) + 1] : null;
}
