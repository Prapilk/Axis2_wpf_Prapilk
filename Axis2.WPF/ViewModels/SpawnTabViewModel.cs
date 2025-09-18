using Axis2.WPF.Models;
using Axis2.WPF.Mvvm;
using Axis2.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Input;
using Axis2.WPF.Extensions;
using System.Threading.Tasks;

namespace Axis2.WPF.ViewModels
{
    // Wrapper class for displaying spawn group members with amounts
    public class SpawnGroupMemberViewModel
    {
        public SObject Npc { get; set; }
        public int Amount { get; set; }
    }

    public class SpawnTabViewModel : BindableBase, IHandler<ProfileLoadedEvent>
    {
        private readonly MulFileManager _mulFileManager;
        private readonly ScriptParser _scriptParser;
        private readonly IUoClient _uoClient;
        private readonly MobTypesService _mobTypesService;
        private readonly AnimationManager _animationManager;
        private readonly BodyDefService _bodyDefService; // Ajout du service
        private string _scriptsPath;
        private bool _isMemberSelection = false;

        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<SObject> DisplayedItems { get; } = new();
        public ObservableCollection<SpawnGroupMemberViewModel> SpawnGroupMembers { get; } = new();

        private object _selectedTreeItem;

        public object SelectedTreeItem
        {
            get => _selectedTreeItem;
            set
            {
                if (_isProgrammaticTreeSelection)
                {
                    SetProperty(ref _selectedTreeItem, value);
                    return;
                }

                bool selectionChanged = SetProperty(ref _selectedTreeItem, value);

                if (_isShowingSearchResults)
                {
                    _isShowingSearchResults = false;
                    UpdateDisplayedItems(); // Always update to clear search
                }
                else if (selectionChanged)
                {
                    UpdateDisplayedItems(); // Update only if selection changed
                }
            }
        }

        private SObject _selectedItem;
        public SObject SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value) && value != null)
                {
                    UpdateItemImage();
                    _preserveImageOnUpdate = true;
                    if (!_isMemberSelection)
                    {
                        ExpandAndSelectInTreeView(value);
                    }
                    _preserveImageOnUpdate = false;

                    if (!_isMemberSelection)
                    {
                        SpawnGroupMembers.Clear();
                        if (value.Type == SObjectType.SpawnGroup && value.Group != null)
                        {
                            foreach (var entry in value.Group.SpawnEntries)
                            {
                                if (_scriptParser.DefNameToObjectMap.TryGetValue(entry.DefName, out var npcObject))
                                {
                                    SpawnGroupMembers.Add(new SpawnGroupMemberViewModel { Npc = npcObject, Amount = entry.Amount });
                                }
                            }
                        }
                    }
                }
            }
        }

        private SpawnGroupMemberViewModel _selectedMember;
        public SpawnGroupMemberViewModel SelectedMember
        {
            get => _selectedMember;
            set
            {
                if (SetProperty(ref _selectedMember, value) && value != null)
                {
                    _isMemberSelection = true;
                    SelectedItem = value.Npc;
                    _isMemberSelection = false;
                }
            }
        }

        private BitmapSource _itemImage;
        public BitmapSource ItemImage
        {
            get => _itemImage;
            set => SetProperty(ref _itemImage, value);
        }

        private int _direction;
        public int Direction
        {
            get => _direction;
            set
            {
                if (SetProperty(ref _direction, value))
                {
                    UpdateItemImage();
                }
            }
        }

        private string _homeDist = "10";
        public string HomeDist
        {
            get => _homeDist;
            set => SetProperty(ref _homeDist, value);
        }

        private string _maxDist = "10";
        public string MaxDist
        {
            get => _maxDist;
            set => SetProperty(ref _maxDist, value);
        }

        private string _amount = "1";
        public string Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        private string _minTime = "10";
        public string MinTime
        {
            get => _minTime;
            set => SetProperty(ref _minTime, value);
        }

        private string _maxTime = "20";
        public string MaxTime
        {
            get => _maxTime;
            set => SetProperty(ref _maxTime, value);
        }

        public ICommand SummonCommand { get; private set; }
        public ICommand PlaceSpawnCommand { get; private set; }
        public ICommand InitSpawnCommand { get; private set; }

        public ICommand SetHomeCommand { get; private set; }
        public ICommand SetHomeDistCommand { get; private set; }
        public ICommand RemoveCommand { get; private set; }
        public ICommand FreezeCommand { get; private set; }
        public ICommand ShrinkCommand { get; private set; }
        public ICommand FindNpcCommand { get; private set; }
        private bool _isShowingSearchResults; // New flag
        private bool _isProgrammaticTreeSelection; // New flag
        private bool _preserveImageOnUpdate = false; // New flag to prevent image clearing

        private Category? _selectedCategoryInTree;
        public Category? SelectedCategoryInTree
        {
            get => _selectedCategoryInTree;
            set => SetProperty(ref _selectedCategoryInTree, value);
        }

        private SubCategory? _selectedSubCategoryInTree;
        public SubCategory? SelectedSubCategoryInTree
        {
            get => _selectedSubCategoryInTree;
            set => SetProperty(ref _selectedSubCategoryInTree, value);
        }

        public int Frame { get; private set; }
        public short ArtType { get; private set; }

        public SpawnTabViewModel(MulFileManager mulFileManager, ScriptParser scriptParser, EventAggregator eventAggregator, IUoClient uoClient, MobTypesService mobTypesService, AnimationManager animationManager, BodyDefService bodyDefService)
        {
            _mulFileManager = mulFileManager;
            _scriptParser = scriptParser;
            _uoClient = uoClient;
            _mobTypesService = mobTypesService;
            _animationManager = animationManager;
            _bodyDefService = bodyDefService; // Assignation du service
            eventAggregator.Subscribe(this);

            SummonCommand = new RelayCommand(OnSummon, () => SelectedItem != null);
            PlaceSpawnCommand = new RelayCommand(OnPlaceSpawn, () => SelectedItem != null);
            InitSpawnCommand = new RelayCommand(OnInitSpawn, () => SelectedItem != null);
            SetHomeCommand = new RelayCommand(OnSetHome);
            SetHomeDistCommand = new RelayCommand(OnSetHomeDist);
            RemoveCommand = new RelayCommand(OnRemove);
            FreezeCommand = new RelayCommand(OnFreeze);
            ShrinkCommand = new RelayCommand(OnShrink);
            FindNpcCommand = new RelayCommand(OnFindNpc);
        }

        private void OnSummon()
        {
            if (SelectedItem == null) return;

            if (uint.TryParse(SelectedItem.Id, out _))
            {
                _uoClient.SendToClient($"addnpc {SelectedItem.Id}");
            }
            else
            {
                _uoClient.SendToClient($"addnpc {SelectedItem.Id}");
            }
        }

        private async void OnPlaceSpawn()
        {
            if (SelectedItem == null)
            {
                System.Windows.MessageBox.Show("No creature is selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            string idToSpawn = SelectedItem.Id;

            if (string.IsNullOrEmpty(idToSpawn))
            {
                System.Windows.MessageBox.Show("No ID to spawn.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (!int.TryParse(Amount, out int iAmount))
            {
                iAmount = 1;
            }
            if (iAmount == 0)
            {
                iAmount = 1;
            }

            if (!int.TryParse(MaxDist, out int iMaxDist))
            {
                iMaxDist = 0;
            }

            if (!int.TryParse(MinTime, out int iMinTime))
            {
                iMinTime = 10;
            }

            if (!int.TryParse(MaxTime, out int iMaxTime))
            {
                iMaxTime = 20;
            }

            if (iMaxTime <= iMinTime)
            {
                iMaxTime = iMinTime + 1;
            }

            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);
            _uoClient.SendToClient("add 01ea7");
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);
            _uoClient.SendToClient($"hear Initialisation du Spawn pour {idToSpawn}");
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);

            for (double i = 3.0; i >= 0.0; i -= 0.5)
            {
                _uoClient.SendToClient($"hear {i:F1}");
                await Task.Delay(250);
            }

            _uoClient.SendToClient($"act.type {Constants.ITEM_SPAWN_CHAR}");
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);

            _uoClient.SendToClient($"act.amount {iAmount}");
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);

            _uoClient.SendToClient($"act.more {idToSpawn}");
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);

            _uoClient.SendToClient($"act.morep {iMinTime} {iMaxTime} {iMaxDist}");
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);

            _uoClient.SendToClient($"act.attr {Constants.ATTR_INVIS | Constants.ATTR_MAGIC | Constants.ATTR_MOVE_NEVER:X4}");
            await Task.Delay(Constants.SPAWN_MESSAGE_DELAY);

            _uoClient.SendToClient($"act.timer 1");
        }

        private void OnInitSpawn()
        {
            if (SelectedItem == null) return;

            _uoClient.SendToClient($"xspawn {SelectedItem.Id} {Amount} {MinTime} {MaxTime} {MaxDist}");
        }

        private void OnSetHome()
        {
            _uoClient.SendToClient("set home");
        }

        private void OnSetHomeDist()
        {
            _uoClient.SendToClient($"set homedist {HomeDist}");
        }

        private void OnRemove()
        {
            _uoClient.SendToClient("remove");
        }

        private void OnFreeze()
        {
            _uoClient.SendToClient("set flags 4");
        }

        private void OnShrink()
        {
            _uoClient.SendToClient("shrink");
        }

        private void OnFindNpc()
        {
            _uoClient.SendToClient("findnpc");
        }

        public void Handle(ProfileLoadedEvent message)
        {
            if (message?.LoadedProfile == null)
            {
                return;
            }
            _scriptsPath = message.LoadedProfile.BaseDirectory;
            LoadScripts();
        }

        private void LoadScripts()
        {
            Categories.Clear();
            DisplayedItems.Clear();
            ItemImage = null;

            if (string.IsNullOrEmpty(_scriptsPath) || !Directory.Exists(_scriptsPath))
            {
                return;
            }

            var allItems = new List<SObject>();
            var scriptFiles = Directory.GetFiles(_scriptsPath, "*.scp", SearchOption.AllDirectories);

            foreach (var file in scriptFiles)
            {
                allItems.AddRange(_scriptParser.ParseFile(file));
            }

            var categorizedItems = _scriptParser.Categorize(allItems.Where(item => item.Type == SObjectType.Npc || item.Type == SObjectType.SpawnGroup).ToList());

            foreach (var category in categorizedItems)
            {
                Categories.Add(category);
            }
        }

        private void UpdateDisplayedItems()
        {
            if (_isShowingSearchResults)
            {
                return;
            }

            DisplayedItems.Clear();
            if (!_preserveImageOnUpdate)
            {
                ItemImage = null;
            }

            if (SelectedTreeItem is SubCategory subCategory)
            {
                var sortedItems = subCategory.Items.OrderBy(item => item.Description);
                foreach (var item in sortedItems)
                {
                    DisplayedItems.Add(item);
                }
            }
            else if (SelectedTreeItem is SObject sObjectItem && sObjectItem.Type == SObjectType.SpawnGroup)
            {
                DisplayedItems.Add(sObjectItem);
            }
            else if (SelectedTreeItem is Category)
            {
            }
        }

        private void UpdateItemImage()
        {
            if (SelectedItem == null)
            {
                ItemImage = null;
                return;
            }

            string idString = SelectedItem.DisplayId ?? SelectedItem.Id;
            uint itemId = 0;

            Logger.Log($"DEBUG: UpdateItemImage - SelectedItem.Id: {SelectedItem.Id}, SelectedItem.DisplayId: {SelectedItem.DisplayId}");

            if (!string.IsNullOrEmpty(idString))
            {
                itemId = idString.AllToUInt();
                Logger.Log($"DEBUG: UpdateItemImage - Converted itemId: {itemId}");
            }

            if (itemId > 0)
            {
                int hue = 0;

                // Vérification du BodyDef pour la redirection d'ID et de couleur
                var bodyDef = _bodyDefService.GetBodyDef((ushort)itemId);
                if (bodyDef != null)
                {
                    Logger.Log($"DEBUG: BodyDef remapped ID {itemId} to {bodyDef.NewId} with Hue {bodyDef.Hue}");
                    itemId = bodyDef.NewId; // Utiliser le nouvel ID
                    hue = bodyDef.Hue;      // Utiliser la nouvelle couleur
                }
                else if (!string.IsNullOrEmpty(SelectedItem.Color) && SelectedItem.Color.StartsWith("0x"))
                {
                    // Si pas de redirection, utiliser la couleur de l'objet SObject
                    int.TryParse(SelectedItem.Color.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out hue);
                }

                bool isUop = _mobTypesService.IsUopAnimation((int)itemId);
                Logger.Log($"DEBUG: UpdateItemImage - isUopAnimation for final itemId {itemId}: {isUop}");

                if (isUop)
                {
                    ItemImage = _animationManager.GetUopFrame(itemId, 0, Direction, 0);
                    if (ItemImage == null)
                    {
                        Logger.Log($"WARNING: UpdateItemImage - GetUopFrame returned null for itemId {itemId}");
                    }
                }
                else
                {
                    int frame = this.Frame;
                    short artType = this.ArtType;

                    var CreateNpcBitmapSource = _mulFileManager.GetBodyAnimation(itemId, Direction, 0, frame, artType, hue);
                    if (CreateNpcBitmapSource != null)
                    {
                        ItemImage = CreateNpcBitmapSource;
                    }
                    else
                    {
                        Logger.Log($"WARNING: UpdateItemImage - GetBodyAnimation returned null for itemId {itemId}");
                        ItemImage = null;
                    }
                }
            }
            else
            {
                Logger.Log($"WARNING: UpdateItemImage - itemId is 0 or less for idString: {idString}");
                ItemImage = null;
            }
        }

        public void FilterItems(Axis2.WPF.Shared.SearchCriteria searchCriteria)
        {
            DisplayedItems.Clear();
            var allItems = Categories.SelectMany(c => c.SubSections.SelectMany(s => s.Items));

            IEnumerable<SObject> filteredItems = allItems;

            if (!string.IsNullOrWhiteSpace(searchCriteria.SearchTerm))
            {
                switch (searchCriteria.SelectedSearchField.DisplayName)
                {
                    case "Name":
                    case "Description":
                        filteredItems = filteredItems.Where(item =>
                            (item.Description?.Contains(searchCriteria.SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
                        break;
                    case "ID":
                        filteredItems = filteredItems.Where(item =>
                            (item.Id?.Contains(searchCriteria.SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
                        break;
                    case "Type":
                        if (searchCriteria.SelectedSObjectType.HasValue)
                        {
                            filteredItems = filteredItems.Where(item => item.Type == searchCriteria.SelectedSObjectType.Value);
                        }
                        break;
                }
            }

            foreach (var item in filteredItems)
            {
                DisplayedItems.Add(item);
            }
                                    _isShowingSearchResults = true; // Set flag after search
        }
        
    public IEnumerable<string> GetUniqueScriptTypes()
        {
            return Categories.SelectMany(c => c.SubSections.SelectMany(s => s.Items))
                             .Where(item => !string.IsNullOrEmpty(item.ScriptType))
                             .Select(item => item.ScriptType)
                             .Distinct()
                             .OrderBy(type => type);
        }

        private void ExpandAndSelectInTreeView(SObject itemToSelect)
        {
            if (itemToSelect == null) return;

            _isProgrammaticTreeSelection = true;
            try
            {
                foreach (var category in Categories)
                {
                    category.IsExpanded = false;
                    foreach (var subCategory in category.SubSections)
                    {
                        subCategory.IsExpanded = false;
                    }
                }

                foreach (var category in Categories)
                {
                    foreach (var subCategory in category.SubSections)
                    {
                        if (subCategory.Items.Contains(itemToSelect))
                        {
                            category.IsExpanded = true;
                            subCategory.IsExpanded = true;

                            SelectedTreeItem = subCategory;

                            SelectedCategoryInTree = category;
                            SelectedSubCategoryInTree = subCategory;
                            return;
                        }
                    }
                }

                SelectedCategoryInTree = null;
                SelectedSubCategoryInTree = null;
            }
            finally
            {
                _isProgrammaticTreeSelection = false;
            }
        }
    }
}
