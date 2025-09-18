using System.Collections.Generic;
using Axis2.WPF.Mvvm;

namespace Axis2.WPF.Models
{
    public class Spell : BindableBase
    {
        private int _id;
        private string _defName = string.Empty;
        private string _name = string.Empty;
        private string _resources = string.Empty;

        public int ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string DefName
        {
            get => _defName;
            set => SetProperty(ref _defName, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Resources
        {
            get => _resources;
            set => SetProperty(ref _resources, value);
        }
    }
}