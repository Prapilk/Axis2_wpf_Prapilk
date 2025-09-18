using Axis2.WPF.Mvvm;
using System.Windows;

namespace Axis2.WPF.ViewModels.Travel
{
    public class AddEditRectViewModel : ViewModelBase
    {
        private int _x1;
        private int _y1;
        private int _x2;
        private int _y2;
        private string _name; // New property

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int X1
        {
            get => _x1;
            set => SetProperty(ref _x1, value);
        }

        public int Y1
        {
            get => _y1;
            set => SetProperty(ref _y1, value);
        }

        public int X2
        {
            get => _x2;
            set => SetProperty(ref _x2, value);
        }

        public int Y2
        {
            get => _y2;
            set => SetProperty(ref _y2, value);
        }

        public Rect ToRect()
        {
            return new Rect(new System.Windows.Point(X1, Y1), new System.Windows.Point(X2, Y2));
        }

        public void FromRect(System.Windows.Rect rect)
        {
            X1 = (int)rect.X;
            Y1 = (int)rect.Y;
            X2 = (int)rect.Right;
            Y2 = (int)rect.Bottom;
        }
    }
}