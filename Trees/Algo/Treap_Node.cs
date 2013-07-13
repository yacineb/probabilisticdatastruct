using System;
using System.Runtime.CompilerServices;

namespace DataStructExtensions.Algo
{
    public partial class Treap<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        private sealed class TreapNode
        {
            /// <summary>
            /// Random Priority to balance the tree
            /// </summary>
            public int Priority { get; private set; }

            /// <summary>
            /// Data Associated with Key
            /// </summary>
            public StrongBox<TValue> Data { get; set; }

            /// <summary>
            /// Key
            /// </summary>
            public TKey Key { get; private set; }

            /// <summary>
            /// Left Node
            /// </summary>
            public TreapNode Left { get; set; }

            /// <summary>
            /// Right Node
            /// </summary>
            public TreapNode Right { get; set; }


            public TreapNode(TKey key, int priority)
            {
                Key = key;
                Priority = priority;
            }
            /// <summary>
            /// Balances the tree by rotating nodes to the left
            /// </summary>
            /// <returns></returns>
            public TreapNode RotateLeft()
            {
                TreapNode temp = Right;
                Right = Right.Left;
                temp.Left = this;
                return temp;
            }

            /// <summary>
            /// Balances the tree by rotating nodes to the right
            /// </summary>
            /// <returns></returns>
            public TreapNode RotateRight()
            {
                TreapNode temp = Left;
                Left = Left.Right;
                temp.Right = this;
                return temp;
            }

            ///<summary>
            /// DeleteRoot
            /// If one of the children is an empty subtree, remove the root and put the other
            /// child in its place. If both children are nonempty, rotate the treapTree at
            /// the root so that the child with the smallest priority number comes to the
            /// top, then delete the root from the other subtee.
            ///
            /// NOTE: This method is recursive
            ///</summary>
            public TreapNode DeleteRoot()
            {
                TreapNode temp;

                if (Left == null)
                    return Right;

                if (Right == null)
                    return Left;

                if (Left.Priority < Right.Priority)
                {
                    temp = RotateRight();
                    temp.Right = DeleteRoot();
                }
                else
                {
                    temp = RotateLeft();
                    temp.Left = DeleteRoot();
                }

                return temp;
            }

            public TreapNode DeepCopy()
            {
                TreapNode newNode = new TreapNode(Key, Priority);
                newNode.Data = new StrongBox<TValue>(this.Data.Value);
                newNode.Right = (Right == null) ? null : Right.DeepCopy();
                newNode.Left = (Left == null) ? null : Left.DeepCopy();
                return newNode;
            }
        }
    }
}
