using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Axis2.WPF.Models;

namespace Axis2.WPF.Services
{
    public class ScriptDataLoaderService : IScriptDataLoaderService
    {
        private List<CSObject> _allItems = new List<CSObject>();

        // Constructeur
        public ScriptDataLoaderService(string scriptPath)
        {
            LoadScripts(scriptPath);
        }

        // La m�thode doit �tre publique pour l'interface
        public void LoadScripts(string scriptPath)
        {
            System.Windows.MessageBox.Show($"D�but du chargement des scripts depuis: {scriptPath}", "Parsing Info", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

            if (!Directory.Exists(scriptPath))
            {
                System.Windows.MessageBox.Show($"Le r�pertoire n'existe pas: {scriptPath}", "Erreur Parsing", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var files = Directory.GetFiles(scriptPath, "*.txt");
            System.Windows.MessageBox.Show($"Nombre de fichiers .txt trouv�s: {files.Length}", "Parsing Info", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

            foreach (var file in files)
            {
                System.Windows.MessageBox.Show($"Traitement du fichier: {Path.GetFileName(file)}", "Parsing Info", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                ParseScriptFile(file);
            }

            System.Windows.MessageBox.Show($"Parsing termin�. Total d'objets charg�s: {_allItems.Count}", "Parsing Complet", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void ParseScriptFile(string filepath)
        {
            var lines = File.ReadAllLines(filepath);
            int validItemsCount = 0;
            int invalidLinesCount = 0;

            System.Windows.MessageBox.Show($"Lecture du fichier {Path.GetFileName(filepath)} - {lines.Length} lignes trouv�es", "Parsing Fichier", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

            foreach (var line in lines)
            {
                var item = ParseLineToCSObject(line);
                if (item != null)
                {
                    _allItems.Add(item);
                    validItemsCount++;
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    invalidLinesCount++;
                }
            }

            System.Windows.MessageBox.Show($"Fichier {Path.GetFileName(filepath)} trait�:\n" +
                          $"- Objets valides: {validItemsCount}\n" +
                          $"- Lignes invalides: {invalidLinesCount}\n" +
                          $"- Total objets actuels: {_allItems.Count}",
                          "R�sultat Parsing Fichier", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private CSObject ParseLineToCSObject(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            var parts = line.Split(';'); // adapter selon votre format
            if (parts.Length >= 3)
            {
                var csObject = new CSObject
                {
                    ID = parts[0],
                    Color = parts[1],
                    Name = parts[2]
                };

                // MessageBox pour chaque objet cr�� (attention: peut �tre tr�s verbeux!)
                // D�commentez la ligne suivante si vous voulez voir chaque objet individuel
                // System.Windows.MessageBox.Show($"Objet cr��: ID={csObject.ID}, Color={csObject.Color}, Name={csObject.Name}", "Nouvel Objet", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                return csObject;
            }
            return null;
        }

        public ObservableCollection<CCategory> LoadItemCategories()
        {
            System.Windows.MessageBox.Show($"Cr�ation des cat�gories avec {_allItems.Count} objets", "Cat�gorisation", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

            var categories = CategorizeObjects(_allItems);

            System.Windows.MessageBox.Show($"Cat�gorisation termin�e. {categories.Count} cat�gories cr��es", "Cat�gorisation Termin�e", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

            return categories;
        }

        private ObservableCollection<CCategory> CategorizeObjects(List<CSObject> items)
        {
            var categories = new ObservableCollection<CCategory>();
            var category = new CCategory { Name = "Default" };
            category.ItemList = new List<CSObject>(items);
            categories.Add(category);

            System.Windows.MessageBox.Show($"Cat�gorie 'Default' cr��e avec {items.Count} objets", "Cat�gorie Info", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

            return categories;
        }
    }
}
