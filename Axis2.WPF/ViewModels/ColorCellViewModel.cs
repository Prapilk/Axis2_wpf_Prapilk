using Axis2.WPF.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Axis2.WPF.ViewModels
{
    public class ColorCellViewModel : BindableBase
    {
        private System.Windows.Media.Color _color;
        public System.Windows.Media.Color Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }

        private ushort _colorIndex;
        public ushort ColorIndex
        {
            get { return _colorIndex; }
            set { SetProperty(ref _colorIndex, value); }
        }

        public ObservableCollection<SolidColorBrush> MiniSpectrum { get; } = new ObservableCollection<SolidColorBrush>();
    }
}
