using System.Collections.Generic;

namespace Axis2.WPF.Models
{
    public class ScriptCategory
    {
        public string Name { get; set; } = string.Empty;
        public List<ScriptSubsection> Subsections { get; } = new List<ScriptSubsection>();
    }
}