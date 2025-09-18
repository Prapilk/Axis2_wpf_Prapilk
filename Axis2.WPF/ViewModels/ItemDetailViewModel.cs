using Axis2.WPF.Models;
using Axis2.WPF.Mvvm;
using System.Windows;
using System.Windows.Input;

namespace Axis2.WPF.ViewModels
{
    public class ItemDetailViewModel : BindableBase
    {
        private SObject _item;
        public SObject Item
        {
            get => _item;
            set => SetProperty(ref _item, value);
        }

        public string? ItemId
        {
            get => Item?.Id;
            set
            {
                if (Item != null && value != null)
                {
                    Item.Id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Description
        {
            get => Item?.Description;
            set
            {
                if (Item != null && value != null)
                {
                    Item.Description = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Color
        {
            get => Item?.Color;
            set
            {
                if (Item != null && value != null)
                {
                    Item.Color = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? ItemType
        {
            get => Item?.Type.ToString();
            set
            {
                if (Item != null && value != null && System.Enum.TryParse(value, out SObjectType type))
                {
                    Item.Type = type;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public ItemDetailViewModel(SObject item)
        {
            _item = item; // Initialize _item in the constructor
            OkCommand = new RelayCommand(OnOk);
            CancelCommand = new RelayCommand(OnCancel);
        }

        private void OnOk()
        {
            // This should be handled by the DialogService now
        }

        private void OnCancel()
        {
            // This should be handled by the DialogService now
        }
    }
}