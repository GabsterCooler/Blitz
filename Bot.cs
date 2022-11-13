using System;
using System.Collections.Generic;

namespace Application
{
    public class Bot
    {
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
        /// Here is where the magic happens, for now the moves are random. I bet you can do better ;)
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
                return FindNextAction(tick.CurrentLocation, closestPort);
            }
        }

        private Position FindClosestPort(Tick tick)
        {
            List<Position> ports = tick.Map.Ports;
            List<int> portsVisites = tick.VisitedPortIndices;

            Position closestPort = new Position();
            double closestDistance = 0;

            foreach(var indexPort in portsVisites)
                ports.RemoveAt(indexPort);

            Console.WriteLine($"x : {tick.CurrentLocation.Column} et y : {tick.CurrentLocation.Row}");

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

        public string FindNextMove(Position currentPosition, Position target)
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
                return new Move { Kind = MoveKind.Dock };
            }
            else
            {
                return new Move { Kind = MoveKind.Sail, Direction = FindNextMove(currentPosition, target) };
            }
        }
    }
}