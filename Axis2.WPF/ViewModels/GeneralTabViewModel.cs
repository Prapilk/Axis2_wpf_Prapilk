using System.Windows.Input;
using Axis2.WPF.Mvvm;
using Axis2.WPF.Services;
using System.Threading.Tasks;

namespace Axis2.WPF.ViewModels
{
    public class GeneralTabViewModel : BindableBase
    {
        private readonly UoClientCommunicator _uoClientCommunicator;
        private readonly IDialogService _dialogService;

        public ICommand AdminCommand { get; }
        public ICommand InfoCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ClientsCommand { get; }
        public ICommand ServerInfoCommand { get; }
        public ICommand VersionCommand { get; }
        public ICommand LinkCommand { get; }
        public ICommand FlipCommand { get; }
        public ICommand ShrinkCommand { get; }
        public ICommand DupeCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand NukeCommand { get; }
        public ICommand BuyCommand { get; }
        public ICommand SellCommand { get; }
        public ICommand InventoryCommand { get; }
        public ICommand PurchasesCommand { get; }
        public ICommand SamplesCommand { get; }
        public ICommand RestockCommand { get; }
        public ICommand SnowCommand { get; }
        public ICommand RainCommand { get; }
        public ICommand DryCommand { get; }
        public ICommand SetLightCommand { get; }
        public ICommand InvulnerableCommand { get; }
        public ICommand AllmoveCommand { get; }
        public ICommand InvisibleCommand { get; }
        public ICommand FixCommand { get; }
        public ICommand TeleCommand { get; }
        public ICommand HearAllCommand { get; }
        public ICommand GmToggleCommand { get; }
        public ICommand DetailsCommand { get; }
        public ICommand NightSightCommand { get; }
        public ICommand DebugCommand { get; }
        public ICommand JailCommand { get; }
        public ICommand ForgiveCommand { get; }
        public ICommand KillCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand ResurrectCommand { get; }
        public ICommand PageOnCommand { get; }
        public ICommand PageListCommand { get; }
        public ICommand PagePlayerCommand { get; }
        public ICommand PageDisconnectCommand { get; }
        public ICommand PageKickCommand { get; }
        public ICommand PageOffCommand { get; }
        public ICommand PageQueueCommand { get; }
        public ICommand PageOriginCommand { get; }
        public ICommand PageJailCommand { get; }
        public ICommand PageDeleteCommand { get; }
        public ICommand WorldSaveCommand { get; }
        public ICommand SaveStaticsCommand { get; }
        public ICommand ResyncCommand { get; }
        public ICommand RestockAllCommand { get; }

        private int _lightLevel;
        public int LightLevel
        {
            get => _lightLevel;
            set
            {
                if (_lightLevel != value)
                {
                    _lightLevel = value;
                }
            }
        }

        public GeneralTabViewModel(UoClientCommunicator uoClientCommunicator, IDialogService dialogService)
        {
            _uoClientCommunicator = uoClientCommunicator;
            _dialogService = dialogService;

            AdminCommand = new RelayCommand(async () => await Admin());
            InfoCommand = new RelayCommand(async () => await Info());
            EditCommand = new RelayCommand(async () => await Edit());
            ClientsCommand = new RelayCommand(async () => await Clients());
            ServerInfoCommand = new RelayCommand(async () => await ServerInfo());
            VersionCommand = new RelayCommand(async () => await Version());
            LinkCommand = new RelayCommand(async () => await Link());
            FlipCommand = new RelayCommand(async () => await Flip());
            ShrinkCommand = new RelayCommand(async () => await Shrink());
            DupeCommand = new RelayCommand(async () => await Dupe());
            RemoveCommand = new RelayCommand(async () => await Remove());
            NukeCommand = new RelayCommand(async () => await Nuke());
            BuyCommand = new RelayCommand(async () => await Buy());
            SellCommand = new RelayCommand(async () => await Sell());
            InventoryCommand = new RelayCommand(async () => await Inventory());
            PurchasesCommand = new RelayCommand(async () => await Purchases());
            SamplesCommand = new RelayCommand(async () => await Samples());
            RestockCommand = new RelayCommand(async () => await Restock());
            SnowCommand = new RelayCommand(async () => await Snow());
            RainCommand = new RelayCommand(async () => await Rain());
            DryCommand = new RelayCommand(async () => await Dry());
            SetLightCommand = new RelayCommand(async () => await SetLight());
            InvulnerableCommand = new RelayCommand(async () => await Invulnerable());
            AllmoveCommand = new RelayCommand(async () => await Allmove());
            InvisibleCommand = new RelayCommand(async () => await Invisible());
            FixCommand = new RelayCommand(async () => await Fix());
            TeleCommand = new RelayCommand(async () => await Tele());
            HearAllCommand = new RelayCommand(async () => await HearAll());
            GmToggleCommand = new RelayCommand(async () => await GmToggle());
            DetailsCommand = new RelayCommand(async () => await Details());
            NightSightCommand = new RelayCommand(async () => await Nightsight());
            DebugCommand = new RelayCommand(async () => await Debug());
            JailCommand = new RelayCommand(async () => await Jail());
            ForgiveCommand = new RelayCommand(async () => await Forgive());
            KillCommand = new RelayCommand(async () => await Kill());
            DisconnectCommand = new RelayCommand(async () => await Disconnect());
            ResurrectCommand = new RelayCommand(async () => await Resurrect());
            PageOnCommand = new RelayCommand(async () => await PageOn());
            PageListCommand = new RelayCommand(async () => await PageList());
            PagePlayerCommand = new RelayCommand(async () => await PagePlayer());
            PageDisconnectCommand = new RelayCommand(async () => await PageDisconnect());
            PageKickCommand = new RelayCommand(async () => await PageKick());
            PageOffCommand = new RelayCommand(async () => await PageOff());
            PageQueueCommand = new RelayCommand(async () => await PageQueue());
            PageOriginCommand = new RelayCommand(async () => await PageOrigin());
            PageJailCommand = new RelayCommand(async () => await PageJail());
            PageDeleteCommand = new RelayCommand(async () => await PageDelete());
            WorldSaveCommand = new RelayCommand(async () => await WorldSave());
            SaveStaticsCommand = new RelayCommand(async () => await SaveStatics());
            ResyncCommand = new RelayCommand(async () => await Resync());
            RestockAllCommand = new RelayCommand(async () => await RestockAll());
        }

        private async Task Admin() => await _uoClientCommunicator.SendToUOAsync("admin");
        private async Task Info() => await _uoClientCommunicator.SendToUOAsync("info");
        private async Task Edit() => await _uoClientCommunicator.SendToUOAsync("xedit");
        private async Task Clients() => await _uoClientCommunicator.SendToUOAsync("show serv.clients");
        private async Task ServerInfo() => await _uoClientCommunicator.SendToUOAsync("information");
        private async Task Version() => await _uoClientCommunicator.SendToUOAsync("version");
        private async Task Link() => await _uoClientCommunicator.SendToUOAsync("link");
        private async Task Flip() => await _uoClientCommunicator.SendToUOAsync("xflip");
        private async Task Shrink() => await _uoClientCommunicator.SendToUOAsync("shrink");
        private async Task Dupe() => await _uoClientCommunicator.SendToUOAsync("dupe");
        private async Task Remove() => await _uoClientCommunicator.SendToUOAsync("remove");
        private async Task Nuke() => await _uoClientCommunicator.SendToUOAsync("nuke");
        private async Task Buy() => await _uoClientCommunicator.SendToUOAsync("buy");
        private async Task Sell() => await _uoClientCommunicator.SendToUOAsync("sell");
        private async Task Inventory() => await _uoClientCommunicator.SendToUOAsync("bank 1a");
        private async Task Purchases() => await _uoClientCommunicator.SendToUOAsync("bank 1b");
        private async Task Samples() => await _uoClientCommunicator.SendToUOAsync("bank 1c");
        private async Task Restock() => await _uoClientCommunicator.SendToUOAsync("xrestock");
        private async Task Snow() => await _uoClientCommunicator.SendToUOAsync("sector.snow");
        private async Task Rain() => await _uoClientCommunicator.SendToUOAsync("sector.rain");
        private async Task Dry() => await _uoClientCommunicator.SendToUOAsync("sector.dry");
        private async Task SetLight() => await _uoClientCommunicator.SendToUOAsync($"sector.light {LightLevel}");
        private async Task Invulnerable() => await _uoClientCommunicator.SendToUOAsync("invulnerable");
        private async Task Allmove() => await _uoClientCommunicator.SendToUOAsync("allmove");
        private async Task Invisible() => await _uoClientCommunicator.SendToUOAsync("invisible");
        private async Task Fix() => await _uoClientCommunicator.SendToUOAsync("fix");
        private async Task Tele() => await _uoClientCommunicator.SendToUOAsync("tele");
        private async Task HearAll() => await _uoClientCommunicator.SendToUOAsync("hearall");
        private async Task GmToggle() => await _uoClientCommunicator.SendToUOAsync("gm");
        private async Task Details() => await _uoClientCommunicator.SendToUOAsync("detail");
        private async Task Nightsight() => await _uoClientCommunicator.SendToUOAsync("nightsight");
        private async Task Debug() => await _uoClientCommunicator.SendToUOAsync("debug");
        private async Task Jail() => await _uoClientCommunicator.SendToUOAsync("jail");
        private async Task Forgive() => await _uoClientCommunicator.SendToUOAsync("forgive");
        private async Task Kill() => await _uoClientCommunicator.SendToUOAsync("kill");
        private async Task Disconnect() => await _uoClientCommunicator.SendToUOAsync("xdisconnect");
        private async Task Resurrect() => await _uoClientCommunicator.SendToUOAsync("xresurrect");
        private async Task PageOn() => await _uoClientCommunicator.SendToUOAsync("page on");
        private async Task PageList() => await _uoClientCommunicator.SendToUOAsync("page list");
        private async Task PagePlayer() => await _uoClientCommunicator.SendToUOAsync("page player");
        private async Task PageDisconnect()
        {
            if (_dialogService.ShowConfirmation("Warning", "Are you sure you want to disconnect the paged player?"))
                await _uoClientCommunicator.SendToUOAsync("page disconnect");
        }
        private async Task PageKick()
        {
            if (_dialogService.ShowConfirmation("Warning", "Are you sure you want to ban the paged player?"))
                await _uoClientCommunicator.SendToUOAsync("page ban");
        }
        private async Task PageOff() => await _uoClientCommunicator.SendToUOAsync("page off");
        private async Task PageQueue() => await _uoClientCommunicator.SendToUOAsync("page queue");
        private async Task PageOrigin() => await _uoClientCommunicator.SendToUOAsync("page origin");
        private async Task PageJail()
        {
            if (_dialogService.ShowConfirmation("Warning", "Are you sure you want to jail the paged player?"))
                await _uoClientCommunicator.SendToUOAsync("page jail");
        }
        private async Task PageDelete()
        {
            if (_dialogService.ShowConfirmation("Warning", "Are you sure you want to delete the page?"))
                await _uoClientCommunicator.SendToUOAsync("page delete");
        }
        private async Task WorldSave() => await _uoClientCommunicator.SendToUOAsync("serv.save");
        private async Task SaveStatics() => await _uoClientCommunicator.SendToUOAsync("serv.savestatics");
        private async Task Resync() => await _uoClientCommunicator.SendToUOAsync("serv.resync");
        private async Task RestockAll() => await _uoClientCommunicator.SendToUOAsync("serv.restock");
    }
}