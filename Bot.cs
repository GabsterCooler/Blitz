using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Application
{
    public class Bot
    {
        public const string NAME = "MyBot C♭";
        private static readonly List<string> _directions = new List<string>
        {
            "N", "NE", "E", "SE", "S", "SW", "W", "NW"
        };

        Node TargetNode;

        public Bot()
        {
            Console.WriteLine("Initializing your super mega bot!");
        }

        /// <summary>
        /// Here is where the magic happens, for now the moves are random. I bet you can do better ;)
        /// </summary>
        public Move GetNextMove(Tick tick)
        {
            if(tick.CurrentLocation == null)
            {
                Position spawnPoint = FindWhereToSpawn(tick);
                BotState.LastPosition = spawnPoint;
                return new Move { Kind = MoveKind.Spawn, Position = spawnPoint};
            }
            else
            {
                Position closestPort = FindClosestPort(tick);
                Console.WriteLine("Current tick: " + tick.CurrentTick);
                Console.WriteLine($"Target => x : {closestPort.Column} et y : {closestPort.Row}");
                return FindNextAction(tick.CurrentLocation, closestPort, tick);
            }
        }

        private Position FindClosestPort(Tick tick)
        {
            foreach (Position port in tick.Map.Ports)
                if(!tick.VisitedPortIndices.Contains(tick.Map.Ports.IndexOf(port)))
                    if (tick.CurrentLocation.Column == port.Column && tick.CurrentLocation.Row == port.Row)
                        return port;

            List<Position> ports = tick.Map.Ports;
            List<int> portsVisites = tick.VisitedPortIndices;

            if (portsVisites.Count == ports.Count)
                return tick.Map.Ports[tick.VisitedPortIndices[0]];

            Position closestPort = new Position();
            double closestDistance = 0;

            Console.WriteLine($"x : {tick.CurrentLocation.Column} et y : {tick.CurrentLocation.Row}");



            foreach(var direction in ports)
            {
                double distance = Math.Sqrt(Math.Pow(Math.Abs(direction.Column - tick.CurrentLocation.Column), 2) + Math.Pow(Math.Abs(direction.Row - tick.CurrentLocation.Row), 2));
                if(distance < closestDistance || closestDistance == 0)
                {
                    if (!tick.VisitedPortIndices.Contains(ports.IndexOf(direction))) {
                        closestPort = direction;
                        closestDistance = distance;
                    }
                }
            }

            return closestPort;
        }

        private Position FindWhereToSpawn(Tick tick)
        {
            return tick.Map.Ports[0];
        }


        private void CompilePath(Node startNode)
        {
            BotState.Path = new List<Position>();
            startNode.G = 0;
            bool success = AStarSearch(startNode);
            if(success)
            {
                Console.WriteLine("Following the path backwards:");
                Node node = TargetNode;
                while (node.ParentNode != null)
                {
                    Console.WriteLine($"node: {node.Position.Column}, {node.Position.Row}");
                    BotState.Path.Add(node.Position);
                    node = node.ParentNode;
                }
                BotState.Path.Reverse();
            }
            else
            {
                Console.WriteLine("Path not found");
            }
        }

        public void Setup(Tick tick)
        {
            BotState.Nodes = new List<List<Node>>();
            for (int i = 0; i < tick.Map.Topology.Count; i++)
            {
                List<Node> nodeColumn = new List<Node>();
                for (int j = 0; j < tick.Map.Topology[i].Count; j++)
                {
                    if (tick.Map.Topology[j][i] >= tick.Map.TideLevels.Max)
                    {
                        // Ces cases seront TOUJOURS non navigables
                        Node newNode = new (i, j, false, int.MaxValue);
                        nodeColumn.Add(newNode);
                    }
                    else
                    {
                        float numberOfTideLevels = tick.Map.TideLevels.Max - tick.Map.TideLevels.Min + 1;
                        float topologyLevel = tick.Map.Topology[j][i];
                        float traversalCost;
                        if (topologyLevel < tick.Map.TideLevels.Min)
                        {
                            traversalCost = 2 / numberOfTideLevels;
                        }
                        else
                        {
                            float h = topologyLevel - tick.Map.TideLevels.Min + 2;
                            float constanteBase = 1 / numberOfTideLevels;
                            float coefficient = 1 - constanteBase;

                            traversalCost = (float)2 * (coefficient * (float)(Math.Pow(h, 2) / Math.Pow(numberOfTideLevels, 2)) + constanteBase);

                            //traversalCost = (float)2 *     ((topologyLevel - tick.Map.TideLevels.Min + 2) / numberOfTideLevels);
                        }
                        // Ces cases seront TOUJOURS navigables
                        Console.WriteLine($" Node: {i}, {j} TraversalCost: {traversalCost}, topologyLevel: {topologyLevel}, noTideLevels: {numberOfTideLevels}, min: {tick.Map.TideLevels.Min}, max: {tick.Map.TideLevels.Max}");
                        Node newNode = new Node(i, j, true, traversalCost);
                        nodeColumn.Add(newNode);
                    }
                }
                BotState.Nodes.Add(nodeColumn);
            }
        }

        public string FindNextMove(Position currentPosition, Position target, Tick tick)
        {
            if(BotState.Path == null || BotState.Path.Count == 0)
            {
                Setup(tick);
                TargetNode = BotState.Nodes[target.Column][target.Row];
                CompilePath(BotState.Nodes[currentPosition.Column][currentPosition.Row]);

            }
            if(BotState.Path.Count > 0)
            {
                Console.WriteLine("LastPosition:" + BotState.LastPosition);
                if (BotState.LastPosition.Row != tick.CurrentLocation.Row || BotState.LastPosition.Column != tick.CurrentLocation.Column)
                {
                    Console.WriteLine($"I have moved, so I remove point {BotState.Path[0].Column}, {BotState.Path[0].Row} from path");
                    BotState.Path.RemoveAt(0);
                }
                if(BotState.Path.Count == 0)
                {
                    Setup(tick);
                    TargetNode = BotState.Nodes[target.Column][target.Row];
                    CompilePath(BotState.Nodes[currentPosition.Column][currentPosition.Row]);
                }
                string direction = FindStringDirection(currentPosition, BotState.Path[0]);
                BotState.LastPosition = tick.CurrentLocation;
                return direction;
            }
            return "N";
        }

        private List<Position> GetAdjacentPositions(Position position)
        {
            List<Position> result = new List<Position>();

            for (int i = -1; i <= 1; i++)
            {
                Position adjacentPosition = new Position();
                adjacentPosition.Row = position.Row -1;
                adjacentPosition.Column = position.Column + i;
                result.Add(adjacentPosition);
            }

            for (int i = -1; i <= 1; i+=2)
            {
                Position adjacentPosition = new Position();
                adjacentPosition.Row = position.Row;
                adjacentPosition.Column = position.Column + i;
                result.Add(adjacentPosition);
            }

            for (int i = -1; i <= 1; i++)
            {
                Position adjacentPosition = new Position();
                adjacentPosition.Row = position.Row + 1;
                adjacentPosition.Column = position.Column + i;
                result.Add(adjacentPosition);
            }
            return result;
        }

        private List <Node> GetAdjacentSailableNodes (Node fromNode)
        {
            List<Node> sailableNodes = new List<Node>();
            IEnumerable<Position> nextPositions = GetAdjacentPositions(fromNode.Position);
            
            foreach (Position position in nextPositions)
            {
                int x = position.Column;
                int y = position.Row;

                if (x < 0 || x >= BotState.Nodes.Count || y < 0 || y >= BotState.Nodes[0].Count)
                {
                    //Console.WriteLine($"Position {position.Column}, {position.Row} ouside map");
                    continue;
                }
                    

                Node node = BotState.Nodes[x][y];

                if (!node.IsSailable)
                {
                    //Console.WriteLine($"Node {node.Position.Column}, {node.Position.Row} not sailable");
                    continue;
                }
                    

                if (node.State == NodeState.Closed)
                {
                   //Console.WriteLine($"Node {node.Position.Column}, {node.Position.Row} closed");
                    continue;
                }
                    

                if(node.State == NodeState.Open)
                {
                    //Console.WriteLine($"Node {node.Position.Column}, {node.Position.Row} Open");
                    float traversalCost = node.TraversalCost;
                    float gTemp = fromNode.G + traversalCost;
                    if (gTemp < node.G)
                    {
                        node.G = gTemp;
                        node.H = Node.GetTraversalCost(node.Position, TargetNode.Position);
                        node.ParentNode = fromNode;
                        BotState.Nodes[x][y] = node;
                        sailableNodes.Add(node);
                        Console.WriteLine("Gtemp smaller");
                    }
                    else
                    {
                        Console.WriteLine("Gtemp not smaller");
                    }
                }
                else
                {
                    //Console.WriteLine($"Node {node.Position.Column}, {node.Position.Row} Untested");
                    node.ParentNode = fromNode;
                    node.State = NodeState.Open;
                    float traversalCost = node.TraversalCost;
                    node.G = fromNode.G + traversalCost;
                    node.H = Node.GetTraversalCost(node.Position, TargetNode.Position);
                        BotState.Nodes[x][y] = node;

                    sailableNodes.Add(node);
                }
            }
            return sailableNodes;
        }

        private bool AStarSearch(Node currentNode)
        {
            Console.WriteLine($"Search on Node:{currentNode.Position.Column}, {currentNode.Position.Row} Node.G = {currentNode.G}, H = {currentNode.H}, F = {currentNode.F}, TraversalCost = {currentNode.TraversalCost}");

            BotState.Nodes[currentNode.Position.Column][currentNode.Position.Row].State = NodeState.Closed;
            List<Node> nextNodes = GetAdjacentSailableNodes(currentNode);
            nextNodes.Sort((node1, node2) => node1.F.CompareTo(node2.F));
            foreach (Node nextNode in nextNodes)
            {
                Console.WriteLine($"nextNode on Node:{nextNode.Position.Column}, {nextNode.Position.Row}");

                if (nextNode.Position.Row == TargetNode.Position.Row && nextNode.Position.Column == TargetNode.Position.Column)
                {
                    Console.WriteLine("path found!!!");
                    return true;
                }
                else
                {
                    if (AStarSearch(nextNode))
                        return true;
                }
            }
            return false;
        }

        public string FindStringDirection(Position currentPosition, Position target)
        {
           if(currentPosition.Row < target.Row && currentPosition.Column < target.Column)
           {
                return "SE";
           }
           else if (currentPosition.Row < target.Row && currentPosition.Column > target.Column)
           {
                return "SW";
           }
           else if (currentPosition.Row > target.Row && currentPosition.Column > target.Column)
           {
                return "NW";
           }
           else if (currentPosition.Row > target.Row && currentPosition.Column < target.Column)
           {
                return "NE";
           }
           else if(currentPosition.Row == target.Row && currentPosition.Column < target.Column)
           {
                return "E";
           }
           else if(currentPosition.Row == target.Row && currentPosition.Column > target.Column)
           {
                return "W";
           }
           else if(currentPosition.Row < target.Row && currentPosition.Column == target.Column)
           {
                return "S";
           }
           else if(currentPosition.Row > target.Row && currentPosition.Column == target.Column)
           {
                return "N";
           }
           else 
           {
                return "Arrivé";
           }
        }

        public Move FindNextAction(Position currentPosition, Position target, Tick tick)
        {
            if(currentPosition.Row == target.Row && currentPosition.Column == target.Column)
            {
                Console.WriteLine("Docking!");
                return new Move { Kind = MoveKind.Dock };
            }
            else
            {
                Console.WriteLine("Sailing!");
                return new Move { Kind = MoveKind.Sail, Direction = FindNextMove(currentPosition, target, tick) };
            }
        }
    }
}