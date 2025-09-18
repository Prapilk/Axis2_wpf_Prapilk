using System;
using System.Globalization;

namespace Axis2.WPF.Extensions
{
    public static class StringExtensions
    {
        public static uint AllToUInt(this string str)
        {
            // Supprimer les espaces blancs
            str = str.Trim();

            // Gérer le préfixe "0x"
            if (str.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                str = str.Substring(2);
            }

            // Gérer les accolades (logique existante)
            if (str.Contains('{'))
            {
                int startIndex = str.IndexOf('{');
                int endIndex = str.LastIndexOf('}');
                if (startIndex != -1 && endIndex != -1 && startIndex < endIndex)
                {
                    string subString = str.Substring(startIndex + 1, endIndex - startIndex - 1);
                    return subString.AllToUInt(); // appel récursif
                }
                return 0;
            }

            // Gérer les plages de nombres aléatoires
            if (str.Contains(' '))
            {
                string[] parts = str.Split(' ');
                if (parts.Length == 2 && uint.TryParse(parts[0], out uint min) && uint.TryParse(parts[1], out uint max))
                {
                    Random random = new Random();
                    return (uint)random.Next((int)min, (int)max + 1);
                }
                return 0;
            }

            // Traiter tout comme hexadécimal
            if (uint.TryParse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result))
            {
                return result;
            }
            else
            {
                // Tenter de parser en décimal si l'hex ne marche pas
                if (uint.TryParse(str, out uint decResult))
                {
                    return decResult;
                }
                return 0;
            }
        }
    }
}
