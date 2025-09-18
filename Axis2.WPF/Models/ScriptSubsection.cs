using System.Collections.Generic;

namespace Axis2.WPF.Models
{
    public class ScriptSubsection
    {
        public string Name { get; set; } = string.Empty;
        public List<ScriptObject> Items { get; } = new List<ScriptObject>();
    }
}