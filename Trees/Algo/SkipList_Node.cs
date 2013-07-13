using System;
using System.Runtime.CompilerServices;

namespace DataStructExtensions.Algo
{
    /// <summary>
    /// represents a set ordered by keys
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    public partial class SkipList<TK,TV>
        where TK : IComparable<TK>
    {
       private sealed class Node
       {
           private Node(int nbLevels, TK key, TV value)
           {
               NbLevels = nbLevels;
               Key = key;
               Forward = new ForwardNodesCollection(nbLevels);
               Data = new StrongBox<TV>(value);
           }

           public int NbLevels { get; private set; }
           public ForwardNodesCollection Forward { get; private set; }
           public TK Key { get; private set; }
           public StrongBox<TV> Data { get; private set; }

           public static Node CreateNode(TK key, TV value, int nbLevels)
           {
               return new Node(nbLevels, key, value);
           }
           
       }

       //wraps the nodes array
       private sealed class ForwardNodesCollection
       {
           private readonly Node[] _nodes;

           public ForwardNodesCollection(int level)
           {
               _nodes = new Node[level];
           }

           public Node this[int level]
           {
               get { return _nodes[level]; }
               set { _nodes[level] = value; }
           }
       }
    }
}
