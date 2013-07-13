using System;
using System.Collections;
using System.Collections.Generic;

namespace DataStructExtensions.Algo
{
    public partial class SkipList<TK, TV> : IEnumerable<KeyValuePair<TK,TV>>
        where TK : IComparable<TK>
    {
        private readonly IComparer<TK> _comparer;
        private readonly Random _rnd = new Random();
        private Node _head;

        public SkipList()
            : this(Comparer<TK>.Default)
        {
        }

        public SkipList(IComparer<TK> comparer)
        {
            _comparer = comparer;
            _head = Node.CreateNode(default(TK), default(TV), 1);
        }

        /// <summary>
        /// Removes the given <param name="key"/>, return true if the key was found
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(TK key)
        {
            Node[] updates = FindUpdates(key);
            Node node = updates[0].Forward[0];

            //if the key > node.Key or node is at the end of the linked list (null)
            if (null == node || 0 != _comparer.Compare(node.Key, key))
            {
                return false;
            }

            //remove reference to the found node in all of the levels containing this node
            for (int i = 0; i < node.NbLevels; ++i)
            {
                updates[i].Forward[i] = node.Forward[i];
            }

            //if the top level has the head pointing directly to null then remove this level
            if (1 < _head.NbLevels && null == _head.Forward[_head.NbLevels - 1])
            {
                Node newHead = Node.CreateNode(_head.Key, _head.Data.Value, _head.NbLevels - 1);
                for (int i = 0; i < _head.NbLevels - 1; ++i)
                {
                    newHead.Forward[i] = _head.Forward[i];
                }
                _head = newHead;
            }

            return true;
        }

        /// <summary>
        /// Adds a pair of (key, value), if key exists do update with the given value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TK key, TV value)
        {
            Node[] updates = FindUpdates(key);

            //if key is found then update with the given value
            if (null != updates[0].Forward[0] && 0 == _comparer.Compare(updates[0].Forward[0].Key, key))
            {
                updates[0].Forward[0].Data.Value = value;
                return;
            }

            // create and insert the node
            Node node = Node.CreateNode(key, value, ChooseRandomHeight());
            int min = node.NbLevels;

            //capping to the length of updates vector
            if (updates.Length < min)
            {
                min = updates.Length;
            }

            //insert new node
            for (int i = 0; i < min; ++i)
            {
                node.Forward[i] = updates[i].Forward[i];
                updates[i].Forward[i] = node;
            }

            // update head level
            if (node.NbLevels > _head.NbLevels)
            {
                Node newHead = Node.CreateNode(_head.Key, _head.Data.Value, node.NbLevels);
                for (int i = 0; i < _head.NbLevels; ++i)
                {
                    newHead.Forward[i] = _head.Forward[i];
                }
                newHead.Forward[_head.NbLevels] = node;
                _head = newHead;
            }
        }

        /// <summary>
        /// Gets a value for the given <param name="key"></param> in O(logN) time
        /// <exception cref="KeyNotFoundException"></exception>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TV this[TK key]
        {
            get
            {
                Node current = _head;
                for (int level = current.NbLevels - 1; level >= 0; --level)
                {
                    int cmp = -1;
                    while (null != current.Forward[level] && (cmp = _comparer.Compare(current.Forward[level].Key, key)) < 0)
                    {
                        current = current.Forward[level];
                    }

                    if (0 == cmp && current.Forward[0].Data !=null)
                    {
                        return current.Forward[0].Data.Value;
                    }
                }

                throw new KeyNotFoundException(string.Format("The given key {0} was not found", key));                
            }
            set
            {
                Add(key,value);             
            }
        }
        /// <summary>
        /// Checks whether <param name="key"/> exists
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TK key)
        {
            Node current = _head;
            for (int level = current.NbLevels - 1; level >= 0; --level)
            {
                int cmp = -1;
                while (null != current.Forward[level] && (cmp = _comparer.Compare(current.Forward[level].Key, key)) < 0)
                {
                    current = current.Forward[level];
                }

                if (0 == cmp)
                {
                    return true;
                }
            }

            return false;
        }

        private Node[] FindUpdates(TK key)
        {
            Node[] updates = new Node[_head.NbLevels];
            Node current = _head;
            for (int level = current.NbLevels - 1; level >= 0; --level)
            {
                while (null != current.Forward[level] && _comparer.Compare(current.Forward[level].Key, key) < 0)
                {
                    current = current.Forward[level];
                }
                //finds the node with the biggest key less than the given key (upper limit)
                updates[level] = current;
            }
            return updates;
        }

        /*
         * In a skiplist , the probability that a node reaches a level k , follows a geometrical law with a parameter p=0.5.
         *  The theorical maximum average level in the skip list is = int(log2(N))+1 where N is the maximum number 
         *  of elements in the list ==> int(log2(int.MaxValue = 2^31 -1 ))+1 = 32
         * */
        protected virtual int ChooseRandomHeight()
        {
            const int maxLevel = 32;
            int level = 0;
            while (_rnd.NextDouble() < 0.5 && level < maxLevel)
            {
                level++;
            }

            return level + 1;
        }

        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            Node current = _head;

            while (current!=null)
            {
                yield return new KeyValuePair<TK, TV>(current.Key, current.Data.Value);
                current = current.Forward[0];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
