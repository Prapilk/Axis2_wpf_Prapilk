using Axis2.WPF.Models;

namespace Axis2.WPF.Mvvm
{
    public class SObjectSelectedEvent
    {
        public SObject SelectedObject { get; }

        public SObjectSelectedEvent(SObject selectedObject)
        {
            SelectedObject = selectedObject;
        }
    }
}
