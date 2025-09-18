using Axis2.WPF.Mvvm;

namespace Axis2.WPF.Models
{
    public class MusicTrack : BindableBase
    {
        private int _id; // New property
        private string _name = string.Empty;
        private string _filePath = string.Empty;

        public int ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }
    }
}