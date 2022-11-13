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
                ? new Move { Kind = MoveKind.Spawn, Position = tick.Map.Ports[0] }
                : new Move { Kind = MoveKind.Sail, Direction = _directions[tick.CurrentTick % _directions.Count] };
        }
    }
}