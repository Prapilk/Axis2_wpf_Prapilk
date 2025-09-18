using Axis2.WPF.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Axis2.WPF.Services
{
    public class SpellService
    {
        public List<Spell> ParseSpells(string scriptContent)
        {
            List<Spell> spells = new List<Spell>();

            // Regex to find [SPELL ID] blocks
            // This regex captures the ID and the content of the spell block until the next [SPELL] or end of string
                        Regex spellBlockRegex = new Regex(@"(?m)^\s*\[SPELL\s*(\d+)\]\s*$(.*?)(?=^\s*\[SPELL\s*\d+\]|\Z)", RegexOptions.Singleline);

            foreach (Match blockMatch in spellBlockRegex.Matches(scriptContent))
            {
                int id = int.Parse(blockMatch.Groups[1].Value);
                string blockContent = blockMatch.Groups[2].Value;

                Spell spell = new Spell { ID = id };

                // Regex to find properties within the block
                Regex propertyRegex = new Regex(@"^\s*(DEFNAME|NAME|RESOURCES)=(.*)$\s*", RegexOptions.Multiline);
                foreach (Match propMatch in propertyRegex.Matches(blockContent))
                {
                    string propName = propMatch.Groups[1].Value;
                    string propValue = propMatch.Groups[2].Value;

                    switch (propName)
                    {
                        case "DEFNAME":
                            spell.DefName = propValue;
                            break;
                        case "NAME":
                            spell.Name = propValue;
                            break;
                        case "RESOURCES":
                            spell.Resources = propValue;
                            break;
                    }
                }
                spells.Add(spell);
            }

            return spells;
        }
    }
}