using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Axis2.WPF.Models;
using Axis2.WPF.Mvvm;

namespace Axis2.WPF.ViewModels.Travel
{
    public class FindAreaViewModel : ViewModelBase
    {
        private string _searchText;
        private readonly IEnumerable<RoomDefinition> _allRooms;

        public FindAreaViewModel(IEnumerable<RoomDefinition> allRooms)
        {
            _allRooms = allRooms;
            SearchResults = new ObservableCollection<RoomDefinition>();
            _searchText = string.Empty;
            SelectedRoom = null!;
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    UpdateSearchResults();
                }
            }
        }

        public ObservableCollection<RoomDefinition> SearchResults { get; }
        public RoomDefinition SelectedRoom { get; set; }

        private void UpdateSearchResults()
        {
            SearchResults.Clear();
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return;
            }

            var searchTextLower = SearchText.ToLower();

            // Improved coordinate parsing
            var match = Regex.Match(searchTextLower, @"(-?\d+)[,\s]+(-?\d+)");

            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int x) && int.TryParse(match.Groups[2].Value, out int y))
                {
                    var results = _allRooms.OrderBy(r =>
                    {
                        var dist1 = System.Math.Sqrt(System.Math.Pow(r.P.X - x, 2) + System.Math.Pow(r.P.Y - y, 2));
                        var dist2 = System.Math.Sqrt(System.Math.Pow(r.P.X - y, 2) + System.Math.Pow(r.P.Y - x, 2));
                        return System.Math.Min(dist1, dist2);
                    }).Take(20);
                    foreach (var room in results)
                    {
                        SearchResults.Add(room);
                    }
                }
            }
            else
            {
                var results = _allRooms.Where(r => r.Name.ToLower().Contains(searchTextLower)).Take(20);
                foreach (var room in results)
                {
                    SearchResults.Add(room);
                }
            }
        }
    }
}