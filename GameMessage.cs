using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Application
{
    public class Tick
    {
        public int CurrentTick { get; set; }
        public int TotalTicks { get; set; }
        public Map Map { get; set; }
        public Position CurrentLocation { get; set; }
        public Position SpawnLocation { get; set; }
        public List<int> VisitedPortIndices { get; set; }
        public List<int> TideSchedule { get; set; }
        public bool IsOver { get; set; }
    }

    public class Map
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int Depth { get; set; }
        public TideLevels TideLevels { get; set; }
        public List<List<int>> Topology { get; set; }
        public List<Position> Ports { get; set; }
    }

    public class Position
    {
        public int Row { get; set; }
        public int Column { get; set; }
    }

    public class TideLevels
    {
        public int Min { get; set; }
        public int Max { get; set; }
    }

    public class Move
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MoveKind Kind { get; set; }
        public Position Position { get; set; }
        public string Direction { get; set; }
    }

    public enum MoveKind
    {
        Spawn,
        Sail,
        Anchor,
        Dock
    }
}