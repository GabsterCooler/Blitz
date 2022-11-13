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
            return tick.CurrentLocation == null
                ? new Move { Kind = MoveKind.Spawn, Position = FindClosestPort(tick.Map.Ports) }
                : new Move { Kind = MoveKind.Sail, Direction = _directions[tick.CurrentTick % _directions.Count] };
        }

        private Position FindClosestPort(List<Position> ports)
        {
            Position closestPort = new Position();
            double closestDistance = 0;

            foreach(var direction in ports)
            {
                double distance = Math.Sqrt(Math.Pow(direction.Column, 2) + Math.Pow(direction.Row, 2));
                if(distance < closestDistance || closestDistance == 0)
                {
                    closestPort = direction;
                    closestDistance = distance;
                }
            }

            return closestPort;
        }
        
        public string FindNextMove(Position currentPosition, Position target)
        {
           if(currentPosition.Row < target.Row && currentPosition.Column < target.Column)
           {
                return "SE";
           }
           else if (currentPosition.Row < target.Row && currentPosition.Column > target.Column)
           {
                return "SW"
           }
           else if (currentPosition.Row > target.Row && currentPosition.Column > target.Column)
           {
                return "NW"
           }
           else if (currentPosition.Row > target.Row && currentPosition.Column < target.Column)
           {
                return "NE"
           }
           else if(currentPosition.Row == target.Row && currentPosition.Column < target.Column)
           {
                return "E"
           }
           else if(currentPosition.Row == target.Row && currentPosition.Column > target.Column)
           {
                return "W"
           }
           else if(currentPosition.Row < target.Row && currentPosition.Column == target.Column)
           {
                return "S"
           }
           else if(currentPosition.Row > target.Row && currentPosition.Column == target.Column)
           {
                return "N"
           }
           else 
           {
                return "Arrivé"
           }
        }
    }
}