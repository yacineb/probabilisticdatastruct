using System;
using System.Collections;
using System.Collections.Generic;

namespace DataStructExtensions.Algo
{
    /// <summary>
    ///  a Binary Tree combined with a Heap using random priorities to balance the tree
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public partial class Treap<TKey, TValue> : IEnumerable<KeyValuePair<TKey,TValue>>, ICloneable
        where TKey : IComparable<TKey>
    {
        // Comparer for keys
        private readonly IComparer<TKey> _comparer;

        // random priority to keep the tree balanced (heap)
        private readonly Random _rndPriority = new Random();

        // Treap Root Node
        private TreapNode _root = null;

        private int _nodesCount;
        private bool _updateCount;

        public Treap(IComparer<TKey> comparer)
        {
            _comparer = comparer;
        }

        public Treap() : this(Comparer<TKey>.Default)
        {
        }

        /// <summary>
        /// Insert a <param name="key"/> with a <param name="value"/> associated
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            // create New node
            TreapNode node = new TreapNode(key,_rndPriority.Next());
            _updateCount = true;
            _root = InsertNode(node, _root);
            if (_updateCount)
                ++_nodesCount;
        }

        /// <summary>
        /// Removes the given <param name="key"/>  if found
        /// </summary>
        /// <param name="key"></param>
        public void Remove(TKey key)
        {
            _root = Remove(key, _root);
        }
        private TreapNode Remove(TKey key, TreapNode tree)
        {
            if (tree == null)
                return null;

            int result = _comparer.Compare(key, tree.Key);
            if (result < 0)
                tree.Left = Remove(key, tree.Left);
            if (result > 0)
                tree.Right = Remove(key, tree.Right);
            if (result == 0)//key is found
            {
                --_nodesCount;
                tree = tree.DeleteRoot();
            }
            return tree;
        }

        /// <summary>
        /// Removes minimum key and returns removed (key,value) in O(logN) time
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<TKey,TValue> RemoveMin()
        {
            if (_root==null)
                throw new Exception("empty treap");

            TreapNode current = _root;
            TreapNode previous;
            if (current.Left == null)
            {
                //replace the top node by its right child
                _root = current.Right;
                --_nodesCount;

                return new KeyValuePair<TKey, TValue>(current.Key, current.Data.Value);
            }
            else
            {
                do
                {
                    //find min node
                    previous = current;
                    current = current.Left;
                } while (current.Left != null);

                //remove minimum node by replacing it by the right one
                previous.Left = current.Right;
                --_nodesCount;

                return new KeyValuePair<TKey, TValue>(current.Key, current.Data.Value);
            }
        }

        /// <summary>
        /// Removes minimum key and returns removed (key,value) in O(logN) time
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<TKey, TValue> RemoveMax()
        {
            if (_root == null)
                throw new Exception("empty treap");

            TreapNode current = _root;
            TreapNode previous;
            if (current.Right == null)
            {
                //replace the top node by its right child
                _root = current.Left;
                --_nodesCount;
                return new KeyValuePair<TKey, TValue>(current.Key, current.Data.Value);
            }
            else
            {
                do
                {
                    //find max node
                    previous = current;
                    current = current.Right;
                } while (current.Right != null);

                //remove max node by replacing it by the left one
                previous.Right = current.Left;
                --_nodesCount;
                return new KeyValuePair<TKey, TValue>(current.Key, current.Data.Value);
            }
        }

        /// <summary>
        /// Gets a value for the given <param name="key"></param> in O(logN) time
        /// <exception cref="KeyNotFoundException"></exception>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get
            {
                TreapNode current = _root;
                int result;
                while (current!=null)
                {
                    result = _comparer.Compare(key, current.Key);
                    if (result == 0)
                    {
                        return current.Data.Value;
                    }
                    current = result < 0 ? current.Left : current.Right;       
                }
                throw new KeyNotFoundException(string.Format("The given key {0} was not found",key));
            }
            set
            {
                Add(key,value);
            }
        }

        /// <summary>
        /// Clears the current Treap in O(1) time
        /// </summary>
        public void Clear()
        {
            _root = null;
            _nodesCount = 0;
        }

        public KeyValuePair<TKey,TValue> Minimum
        {
            get 
            { 
                TreapNode current = _root;
                if (current == null)
                    throw new Exception("Treap is empty");

                while (current.Left !=null)
                {
                    current = current.Left;
                }
                return new KeyValuePair<TKey, TValue>(current.Key, current.Data.Value);
            }
        }

        public KeyValuePair<TKey, TValue> Maximum
        {
            get
            {
                TreapNode current = _root;
                if (current == null)
                    throw new Exception("Treap is empty");

                while (current.Right != null)
                {
                    current = current.Right;
                }
                return new KeyValuePair<TKey, TValue>(current.Key, current.Data.Value);
            }
        }

        private TreapNode InsertNode(TreapNode node, TreapNode tree)
        {
            //if treap is empty
            if (tree == null)
            {
                return node;
            }

            int result = _comparer.Compare(node.Key, tree.Key);

            //"Plant" tree in the left node
            if (result < 0)
            {
                tree.Left = InsertNode(node, tree.Left);

                //rebalance tree in order to get the lowest priority on top
                if (tree.Left.Priority < tree.Priority)
                    tree = tree.RotateRight();
            }
            else if (result > 0)
            {
                tree.Right = InsertNode(node, tree.Right);

                if (tree.Right.Priority < tree.Priority)
                    tree = tree.RotateLeft();
            }
            else //key is found
            {
                tree.Data.Value = node.Data.Value;
                _updateCount = false;
            }
            return tree;
        }

        /// <summary>
        /// Morris Inorder Tree Traversal using o(1) space complexity, it's not involving recursion
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (_root == null)
                yield break;

            TreapNode current = _root;
            TreapNode pre = null;

            while (current!=null)
            {
                if (current.Left == null)
                {
                    //step to the next iteration
                    yield return new KeyValuePair<TKey, TValue>(current.Key,current.Data.Value); 
                    current = current.Right;
                }
                else
                {
                    //Find predecessor of current node
                    pre = current.Left;
                    while (pre.Right != null && pre.Right != current)
                        pre = pre.Right;

                    //make current node as right child of its inorder predecessor
                    if (pre.Right == null)
                    {
                        pre.Right = current;
                        current = current.Left;
                    }
                    //revert changes, fix right child of predecessor
                    else
                    {
                        pre.Right = null;
                        yield return new KeyValuePair<TKey, TValue>(current.Key, current.Data.Value);
                        current = current.Right;
                    }

                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Number of elements in the tree
        /// </summary>
        public int Count
        {
            get { return _nodesCount; }
        }

        /// <summary>
        /// Deep copy of the current tree in o(n) time
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var newTreap = new Treap<TKey, TValue>(_comparer);
            if (this._root == null)
                return newTreap;

            newTreap._root = this._root.DeepCopy();
            newTreap._nodesCount = this._nodesCount;
            
            return newTreap;
        }

        
    }
}
