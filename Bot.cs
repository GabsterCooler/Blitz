using System;
using System.Collections.Generic;

namespace Application
{
    public class CoordinatedDirections
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Direction { get; set; }
    }

    public class Bot
    {
        public const int nbOfPortsAccosted = 16;
        public const string NAME = "MyBot C♭";
        private static readonly List<string> _directions = new List<string>
        {
            "N", "NE", "E", "SE", "S", "SW", "W", "NW"
        };

        public Bot()
        {
            Console.WriteLine("Initializing your super mega bot!");
        }

        /// <summary>
        /// Handle the turn here, returning a move
        /// </summary>
        public Move GetNextMove(Tick tick)
        {
            if(tick.CurrentLocation == null)
            {
                return new Move { Kind = MoveKind.Spawn, Position = FindWhereToSpawn(tick)};
            }
            else
            {
                Position closestPort = FindClosestPort(tick);
                //Console.WriteLine("Current tick: " + tick.CurrentTick);
                //Console.WriteLine($"x : {closestPort.Column} et y : {closestPort.Row}");
                return FindNextAction(tick, closestPort);
            }
        }

        private LinkedListNode<CoordinatedDirections> GetNodeAtIndex(int index, LinkedList<CoordinatedDirections> list) {
            LinkedListNode<CoordinatedDirections> node = list.First;
            for (int i = 0; i < index; i++)
                node = node.Next;
            return node;
        }

        private string VerifyMove(int n, Tick tick)
        {
            LinkedList<CoordinatedDirections> directions = new LinkedList<CoordinatedDirections>();
            directions.AddLast(new CoordinatedDirections { X = 0, Y = -1, Direction = "N"});
            directions.AddLast(new CoordinatedDirections { X = 1, Y = -1, Direction = "NE"});
            directions.AddLast(new CoordinatedDirections { X = 1, Y = 0, Direction = "E"});
            directions.AddLast(new CoordinatedDirections { X = 1, Y = 1, Direction = "SE"});
            directions.AddLast(new CoordinatedDirections { X = 0, Y = 1, Direction = "S"});
            directions.AddLast(new CoordinatedDirections { X = -1, Y = 1, Direction = "SW"});
            directions.AddLast(new CoordinatedDirections { X = -1, Y = 0, Direction = "W"});
            directions.AddLast(new CoordinatedDirections { X = -1, Y = -1, Direction = "NW"});

            LinkedListNode<CoordinatedDirections> currentDirection = GetNodeAtIndex(n, directions);

            for (int i = 0; i < 8; i++) {
                if (tick.Map.Topology[tick.CurrentLocation.Row + currentDirection.Value.Y][tick.CurrentLocation.Column + currentDirection.Value.X] < tick.Map.TideLevels.Max){
                    return currentDirection.Value.Direction;
                }
                currentDirection = currentDirection.Next ?? directions.First;
            }
            
            return "E"; // TODO: on est bloqué, direction arbitraire
        }

        private Position FindClosestPort(Tick tick)
        {
            if(tick.VisitedPortIndices.Count == nbOfPortsAccosted)
                return tick.Map.Ports[tick.VisitedPortIndices[0]];

            foreach(Position port in tick.Map.Ports)
                if(!tick.VisitedPortIndices.Contains(tick.Map.Ports.IndexOf(port)))
                    if (tick.CurrentLocation.Column == port.Column && tick.CurrentLocation.Row == port.Row)
                        return port;

            List<Position> ports = tick.Map.Ports;
            List<int> portsVisites = tick.VisitedPortIndices;

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

        public string FindNextMove(Tick tick, Position target)
        {
           if(tick.CurrentLocation.Row < target.Row && tick.CurrentLocation.Column < target.Column)
           {
                return VerifyMove(3, tick); //SE
           }
           else if (tick.CurrentLocation.Row < target.Row && tick.CurrentLocation.Column > target.Column)
           {
                return VerifyMove(5, tick); //SW
           }
           else if (tick.CurrentLocation.Row > target.Row && tick.CurrentLocation.Column > target.Column)
           {
                return VerifyMove(7, tick); //NW
           }
           else if (tick.CurrentLocation.Row > target.Row && tick.CurrentLocation.Column < target.Column)
           {
                return VerifyMove(1, tick); //NE
           }
           else if(tick.CurrentLocation.Row == target.Row && tick.CurrentLocation.Column < target.Column)
           {
                return VerifyMove(2, tick); //E
           }
           else if(tick.CurrentLocation.Row == target.Row && tick.CurrentLocation.Column > target.Column)
           {
                return VerifyMove(6, tick); //W
           }
           else if(tick.CurrentLocation.Row < target.Row && tick.CurrentLocation.Column == target.Column)
           {
                return VerifyMove(4, tick); //S
           }
           else if(tick.CurrentLocation.Row > target.Row && tick.CurrentLocation.Column == target.Column)
           {
                return VerifyMove(0, tick); //N
           }
           else 
           {
                return "Arrivé";
           }
        }

        public Move FindNextAction(Tick tick, Position target)
        {
            if(tick.CurrentLocation.Row == target.Row && tick.CurrentLocation.Column == target.Column)
            {
                Console.WriteLine("Docking!");
                return new Move { Kind = MoveKind.Dock };
            }
            else
            {
                Console.WriteLine("Sailing!");
                return new Move { Kind = MoveKind.Sail, Direction = FindNextMove(tick, target) };
            }
        }
    }
}