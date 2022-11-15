using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public class Node
    {
        public Position Position { get; set; }
        public bool IsSailable { get; set; }
        public float G { get; set; }
        public float H { get; set; }
        public float F { get { return G + H; } }
        public NodeState State { get; set; }
        public Node ParentNode { get; set; }

        public static float GetTraversalCost(Position from, Position to)
        {
            return (float)Math.Sqrt(Math.Pow(Math.Abs(from.Column - to.Column), 2) + Math.Pow(Math.Abs(from.Row - to.Row), 2));
        }

        public Node(int x, int y, bool isSailable)
        {
            Console.WriteLine("Inside node constructor");
            Position.Column = x;
            Position.Row = y;
            IsSailable = isSailable;
            G = int.MaxValue;
            H = int.MaxValue;
            State = NodeState.Untested;
            ParentNode = null;
        }
    }



    public enum NodeState { Untested, Open, Closed }

    
}
