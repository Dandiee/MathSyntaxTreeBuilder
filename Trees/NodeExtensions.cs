namespace Trees
{
    public static class NodeExtensions
    {
        //public static IEnumerable<TNode> Traversal<TNode>(this TNode root)
        //    where TNode : IBaseNode<TNode>
        //{
        //    var queue = new Queue<IBaseNode<TNode>>(new IBaseNode<TNode>[] { root });
        //    while (queue.Count > 0)
        //    {
        //        var current = queue.Dequeue();
        //        yield return (TNode)current;

        //        foreach (var child in current.Children)
        //        {
        //            yield return child;
        //            queue.Enqueue(current);
        //        }
        //    }
        //}

        public static IPayloadNode<TNode, IVisualNodeDescription> ToVisualNode<TNode, TPayload>(this TNode root, Func<TNode, TPayload> factory)
            where TNode : class, IBaseNode<TNode>
        {

            var newRoot = new BuchheimVisualNodeDescription<TNode>(null, factory(root));
            var queue = new Queue<BuchheimNode<TNode>>(new[] { newRoot });
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var child in current.Value.Children)
                {
                    var newChild = new BuchheimNode<TNode>(child, current);
                    current.Children.Add(newChild);
                    queue.Enqueue(current);
                }
            }

            return newRoot!;
        }
    }

}
