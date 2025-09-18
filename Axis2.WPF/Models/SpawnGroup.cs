using System.Collections.Generic;

namespace Axis2.WPF.Models
{
    public class SpawnEntry
    {
        public string DefName { get; set; }
        public int Amount { get; set; }
    }

    public class SpawnGroup
    {
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? SubSection { get; set; }
        public string? Description { get; set; }
        public List<string> NpcIds { get; set; } = new List<string>();
        public List<SpawnEntry> SpawnEntries { get; set; } = new List<SpawnEntry>();
    }
}
