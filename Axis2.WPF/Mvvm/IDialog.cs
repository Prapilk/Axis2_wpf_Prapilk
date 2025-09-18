using System.Threading.Tasks;
using System;

namespace Axis2.WPF.Mvvm
{
    public interface IDialog
    {
        string Title { get; }
        object Content { get; }
        bool? DialogResult { get; }
        event EventHandler CloseRequested;
        void Close();
    }
}