using Axis2.WPF.Mvvm;

namespace Axis2.WPF.Models
{
    public class Sound : BindableBase
    {
        private int _id;
        private string _name = string.Empty;
        private int _startOffset;
        private int _length;

        public int ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public int DisplayID // New property for display purposes
        {
            get => ID - 1;
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int StartOffset
        {
            get => _startOffset;
            set => SetProperty(ref _startOffset, value);
        }

        public int Length
        {
            get => _length;
            set => SetProperty(ref _length, value);
        }
    }
}