using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows; // Import pour MessageBox

namespace Axis2.WPF.Services
{
    public class BodyDef
    {
        public ushort OriginalId { get; set; }
        public ushort NewId { get; set; }
        public int Hue { get; set; } // Ajout de la propriété Hue
        public int MulFile { get; set; }
    }

    public class BodyDefService
    {
        private readonly List<BodyDef> _bodyDefs = new List<BodyDef>();
        private readonly List<BodyDef> _bodyConv = new List<BodyDef>();

        public BodyDefService()
        {
            // Constructeur vide
        }

        public void Load(string bodyDefPath, string bodyConvPath)
        {
            _bodyDefs.Clear();
            _bodyConv.Clear();
            LoadBodyDef(bodyDefPath);
            LoadBodyConv(bodyConvPath);
        }

        public BodyDef? GetBodyDef(ushort id)
        {
            var result = _bodyDefs.FirstOrDefault(d => d.OriginalId == id);
            return result;
        }

        public BodyDef? GetBodyConv(ushort id)
        {
            var result = _bodyConv.FirstOrDefault(d => d.OriginalId == id);
            return result;
        }

        private void LoadBodyDef(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            if (!File.Exists(path))
            {
                return;
            }

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var cleanLine = line.Split('#')[0].Trim();
                if (string.IsNullOrEmpty(cleanLine))
                {
                    continue;
                }

                var parts = cleanLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    continue;
                }

                if (ushort.TryParse(parts[0], out ushort originalId))
                {                   
                    var newIdPart = parts[1].Trim('{', '}');
                    if (ushort.TryParse(newIdPart, out ushort newId))
                    {
                        int hue = 0;
                        if (parts.Length > 2)
                        {
                            int.TryParse(parts[2], out hue);
                        }
                        _bodyDefs.Add(new BodyDef { OriginalId = originalId, NewId = newId, Hue = hue });
                    }
                }
            }
        }

        private void LoadBodyConv(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return;
            }

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var cleanLine = line.Split('#')[0].Trim();
                if (string.IsNullOrEmpty(cleanLine))
                
                {
                    continue;
                }

                var parts = cleanLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) // Doit avoir au moins un ID original et une valeur
                {
                    continue;
                }

                if (ushort.TryParse(parts[0], out ushort originalId))
                {
                    // La nouvelle logique parcourt les colonnes pour trouver la première animation valide.
                    // La position de la colonne détermine le fichier Mul.
                    // parts[1] -> anim2.mul (MulFile = 2)
                    // parts[2] -> anim3.mul (MulFile = 3)
                    // etc.
                    for (int i = 1; i < parts.Length; i++)
                    {
                        if (ushort.TryParse(parts[i], out ushort newId) && newId != ushort.MaxValue && parts[i] != "-1")
                        {
                            _bodyConv.Add(new BodyDef
                            {
                                OriginalId = originalId,
                                NewId = newId,
                                MulFile = i + 1 // L'index de la colonne + 1 est le numéro du fichier Mul
                            });
                            break; // On a trouvé la première correspondance, on passe à la ligne suivante
                        }
                    }
                }
            }
        }
    }
}
