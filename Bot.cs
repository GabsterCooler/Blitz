using System;
using System.Collections.Generic;

namespace Application
{
    public class Bot
    {
        public const string NAME = "MyBot C♭";

        Node TargetNode;

        private static readonly List<string> _directions = new List<string>
        {
            "N", "NE", "E", "SE", "S", "SW", "W", "NW"
        };

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
                Setup(tick);
                return new Move { Kind = MoveKind.Spawn, Position = FindWhereToSpawn(tick)};
            }
            else
            {
                Position closestPort = FindClosestPort(tick);
                Console.WriteLine($"Target => x : {closestPort.Column} et y : {closestPort.Row}");
                return FindNextAction(tick.CurrentLocation, closestPort);
            }
        }

        private Position FindClosestPort(Tick tick)
        {
            foreach(Position port in tick.Map.Ports)
                if(!tick.VisitedPortIndices.Contains(tick.Map.Ports.IndexOf(port)))
                    if (tick.CurrentLocation.Column == port.Column && tick.CurrentLocation.Row == port.Row)
                        return port;

            List<Position> ports = tick.Map.Ports;
            List<int> portsVisites = tick.VisitedPortIndices;

            Position closestPort = new Position();
            double closestDistance = 0;

            foreach(var indexPort in portsVisites)
                ports.RemoveAt(indexPort);

            Console.WriteLine($"Current Location => x : {tick.CurrentLocation.Column} et y : {tick.CurrentLocation.Row}");

            foreach(var direction in ports)
            {
                double distance = Math.Sqrt(Math.Pow(Math.Abs(direction.Column - tick.CurrentLocation.Column), 2) + Math.Pow(Math.Abs(direction.Row - tick.CurrentLocation.Row), 2));
                if(distance < closestDistance || closestDistance == 0)
                {
                    closestPort = direction;
                    closestDistance = distance;
                }
            }

            return closestPort;
        }

        private Position FindWhereToSpawn(Tick tick)
        {
            return tick.Map.Ports[0];
        }

        private List<Position> CompilePath(Node startNode)
        {
            List<Position> path = new List<Position>();
            bool success = AStarSearch(startNode);
            if(success)
            {
                Node node = TargetNode;
                while (node.ParentNode != null)
                {
                    path.Add(node.Position);
                    node = node.ParentNode;
                }
                path.Reverse();
            }
            return path;
        }

        public void Setup(Tick tick)
        {
            BotState.Nodes = new List<List<Node>>();
            for (int i = 0; i < tick.Map.Topology.Count; i++)
            {
                List<Node> nodeColumn = new List<Node>();
                for (int j = 0; j < tick.Map.Topology[i].Count; j++)
                {
                    if (tick.Map.Topology[i][j] > tick.Map.TideLevels.Min)
                    {
                        // Ces cases seront PARFOIS non navigables
                        Console.WriteLine("nodecolumn" + nodeColumn.Count);
                        Node newNode = new (i, j, false);
                        Console.WriteLine(newNode);
                        nodeColumn.Add(newNode);
                    }
                    else
                    {
                        // Ces cases seront TOUJOURS navigables


                        Console.WriteLine("nodecolumn" +nodeColumn.Count);
                        Console.WriteLine("i" + i);
                        Console.WriteLine("j" + j);


                        Node newNode = new (i, j, true);


                        Console.WriteLine(newNode);
                        nodeColumn.Add(newNode);
                    }
                }
                BotState.Nodes.Add(nodeColumn);
            }
        }

        public string FindNextMove(Position currentPosition, Position target)
        {
            TargetNode = BotState.Nodes[target.Column][target.Row];
            List<Position> path = CompilePath(BotState.Nodes[currentPosition.Column][currentPosition.Row]);
            if(path.Count == 0)
            {
                Console.WriteLine("path nul");
                return "N";
            }
            return FindStringDirection(currentPosition, path[0]);

        }

        private List<Position> GetAdjacentPositions(Position position)
        {
            List<Position> result = new List<Position>();

            for (int i = -1; i <= 1; i++)
            {
                Position adjacentPosition = new Position();
                adjacentPosition.Row = position.Row -1;
                adjacentPosition.Column = position.Column + i;
                result.Add(new Position());
            }

            for (int i = -1; i <= 1; i+=2)
            {
                Position adjacentPosition = new Position();
                adjacentPosition.Row = position.Row;
                adjacentPosition.Column = position.Column + i;
                result.Add(new Position());
            }

            for (int i = -1; i <= 1; i++)
            {
                Position adjacentPosition = new Position();
                adjacentPosition.Row = position.Row + 1;
                adjacentPosition.Column = position.Column + i;
                result.Add(new Position());
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
                    continue;

                Node node = BotState.Nodes[x][y];
                node.H = Node.GetTraversalCost(node.Position, TargetNode.Position);

                if (!node.IsSailable)
                    continue;

                if (node.State == NodeState.Closed)
                    continue;

                if(node.State == NodeState.Open)
                {
                    float traversalCost = Node.GetTraversalCost(node.Position, fromNode.Position);
                    float gTemp = fromNode.G + traversalCost;
                    if (gTemp < node.G)
                    {
                        node.G = gTemp;
                        node.ParentNode = fromNode;
                        sailableNodes.Add(node);
                    }
                }
                else
                {
                    node.ParentNode = fromNode;
                    node.State = NodeState.Open;
                    sailableNodes.Add(node);
                }
            }
            return sailableNodes;
        }

        private bool AStarSearch(Node currentNode)
        {
            currentNode.State = NodeState.Closed;
            List<Node> nextNodes = GetAdjacentSailableNodes(currentNode);
            nextNodes.Sort((node1, node2) => node1.F.CompareTo(node2.F));
            foreach (Node nextNode in nextNodes)
            {
                if(nextNode.Position.Row == TargetNode.Position.Row && nextNode.Position.Column == TargetNode.Position.Column)
                {
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

        public Move FindNextAction(Position currentPosition, Position target)
        {
            if(currentPosition.Row == target.Row && currentPosition.Column == target.Column)
            {
                Console.WriteLine("Docking!");
                return new Move { Kind = MoveKind.Dock };
            }
            else
            {
                Console.WriteLine("Sailing!");
                return new Move { Kind = MoveKind.Sail, Direction = FindNextMove(currentPosition, target) };
            }
        }
    }
}