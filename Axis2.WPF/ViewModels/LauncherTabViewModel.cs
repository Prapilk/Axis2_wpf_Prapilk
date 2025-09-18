using Axis2.WPF.Models;
using Axis2.WPF.Mvvm;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;
using Axis2.WPF.Services;

namespace Axis2.WPF.ViewModels
{
    public class LauncherTabViewModel : ViewModelBase
    {
        private ObservableCollection<LauncherAccount> _accounts;
        private LauncherAccount? _selectedAccount;
        private bool _isEditing;

        private readonly SettingsService _settingsService;

        public LauncherTabViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _accounts = new ObservableCollection<LauncherAccount>();
            LoadAccounts();

            NewCommand = new RelayCommand(NewAccount);
            EditCommand = new RelayCommand(EditAccount, CanEditOrDelete);
            DeleteCommand = new RelayCommand(DeleteAccount, CanEditOrDelete);
            SaveCommand = new RelayCommand(SaveAccount, CanSave);
            CancelCommand = new RelayCommand(CancelEdit, CanCancel);
            BrowseCommand = new RelayCommand(Browse);
            Launch2DCommand = new RelayCommand(Launch2D, CanLaunch);
            Launch3DCommand = new RelayCommand(Launch3D, CanLaunch);
            LaunchAltCommand = new RelayCommand(LaunchAlt, CanLaunch);
            LaunchAutoCommand = new RelayCommand(LaunchAuto, CanLaunchAuto);
        }

        public ObservableCollection<LauncherAccount> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
        }

        public LauncherAccount? SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                if (SetProperty(ref _selectedAccount, value))
                {
                    OnPropertyChanged(nameof(CanEditOrDelete));
                    OnPropertyChanged(nameof(CanLaunch));
                }
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (SetProperty(ref _isEditing, value))
                {
                    OnPropertyChanged(nameof(SaveCommand));
                    OnPropertyChanged(nameof(CancelCommand));
                }
            }
        }

        public ICommand NewCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseCommand { get; }
        public ICommand Launch2DCommand { get; }
        public ICommand Launch3DCommand { get; }
        public ICommand LaunchAltCommand { get; }
        public ICommand LaunchAutoCommand { get; }

        private void LoadAccounts()
        {
            string filePath = GetAccountsFilePath();
            if (File.Exists(filePath))
            {
                string json = System.IO.File.ReadAllText(filePath);
                var loadedAccounts = JsonSerializer.Deserialize<ObservableCollection<LauncherAccount>>(json);
                if (loadedAccounts != null)
                {
                    Accounts = loadedAccounts;
                }
            }
        }

        private void SaveAccounts()
        {
            string filePath = GetAccountsFilePath();
            string json = JsonSerializer.Serialize(Accounts, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(filePath, json);
        }

        private string GetAccountsFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolderPath = Path.Combine(appDataPath, "Axis2");
            Directory.CreateDirectory(appFolderPath);
            return Path.Combine(appFolderPath, "launcher_accounts.json");
        }

        private void NewAccount()
        {
            SelectedAccount = new LauncherAccount();
            IsEditing = true;
        }

        private void EditAccount()
        {
            IsEditing = true;
        }

        private void DeleteAccount()
        {
            if (SelectedAccount != null)
            {
                Accounts.Remove(SelectedAccount);
                SaveAccounts();
            }
        }

        private void SaveAccount()
        {
            if (SelectedAccount != null)
            {
                if (!Accounts.Contains(SelectedAccount))
                {
                    Accounts.Add(SelectedAccount);
                }
                SaveAccounts();
                IsEditing = false;
            }
        }

        private void CancelEdit()
        {
            IsEditing = false;
            // Reload accounts to discard changes
            LoadAccounts();
        }

        private void Browse()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                if (SelectedAccount != null)
                {
                    SelectedAccount.AlternateClientPath = openFileDialog.FileName;
                    OnPropertyChanged(nameof(SelectedAccount));
                }
            }
        }

        private void Launch2D()
        {
            LaunchClient("client.exe");
        }

        private void Launch3D()
        {
            LaunchClient("uotd.exe");
        }

        private void LaunchAlt()
        {
            if (SelectedAccount != null && !string.IsNullOrEmpty(SelectedAccount.AlternateClientPath))
            {
                LaunchClient(SelectedAccount.AlternateClientPath, true);
            }
            else
            {
                System.Windows.MessageBox.Show("No alternate client path specified for the selected account.", "Launch Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        private void LaunchAuto()
        {
            LaunchClient("uopatch.exe");
        }

        private void LaunchClient(string clientExecutable, bool isAlternate = false)
        {
            if (SelectedAccount == null) return;

            var settings = _settingsService.LoadSettings();
            string uoPath = _settingsService.LoadSettings().FilePathsSettings.DefaultMulPath;

            if (string.IsNullOrEmpty(uoPath))
            {
                System.Windows.MessageBox.Show("Ultima Online client path is not configured in settings.", "Launch Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            string loginCfgPath = Path.Combine(uoPath, "login.cfg");
            string tempLoginCfgPath = Path.Combine(uoPath, "uo.axis");

            try
            {
                // Write server IP and port to login.cfg
                System.IO.File.WriteAllText(loginCfgPath, $"; login.cfg file for the {SelectedAccount.Name} server\nLoginServer={SelectedAccount.IpAddress},{SelectedAccount.Port}\n");

                // Update login.cfg with account info
                List<string> lines = new List<string>();
                if (System.IO.File.Exists(loginCfgPath))
                {
                    lines = System.IO.File.ReadAllLines(loginCfgPath).ToList();
                }

                using (StreamWriter writer = new StreamWriter(tempLoginCfgPath))
                {
                    writer.WriteLine($"; LOGIN.CFG for the {SelectedAccount.Name} server");
                    foreach (string line in lines)
                    {
                        if (!line.StartsWith(";") &&
                            !line.StartsWith("SavePassword=") &&
                            !line.StartsWith("AcctID=") &&
                            !line.StartsWith("AcctPassword=") &&
                            !line.StartsWith("RememberAcctPW="))
                        {
                            writer.WriteLine(line);
                        }
                    }
                    writer.WriteLine("SavePassword=on");
                    writer.WriteLine($"AcctID={SelectedAccount.AccountName}");
                    writer.WriteLine($"AcctPassword={EncryptUO(SelectedAccount.Password)}");
                    writer.WriteLine("RememberAcctPW=on");
                }

                System.IO.File.Delete(loginCfgPath);
                System.IO.File.Move(tempLoginCfgPath, loginCfgPath);

                // Launch the client
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = isAlternate ? clientExecutable : Path.Combine(uoPath, clientExecutable);
                startInfo.WorkingDirectory = uoPath;
                startInfo.UseShellExecute = true; // Use shell execute to allow launching .exe directly

                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to launch client: {ex.Message}", "Launch Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private string EncryptUO(string password)
        {
            // This is a placeholder for the actual encryption logic.
            // In the C++ code, there's an EncryptUO function.
            // For a real application, you would need to replicate that encryption.
            // For now, returning the password as-is.
            return password;
        }

        private bool CanEditOrDelete()
        {
            return SelectedAccount != null && !IsEditing;
        }

        private bool CanLaunch()
        {
            return SelectedAccount != null && !IsEditing;
        }

        private bool CanSave()
        {
            return IsEditing && SelectedAccount != null &&
                   !string.IsNullOrWhiteSpace(SelectedAccount.Name) &&
                   !string.IsNullOrWhiteSpace(SelectedAccount.IpAddress) &&
                   SelectedAccount.Port > 0;
        }

        private bool CanCancel()
        {
            return IsEditing;
        }

        private bool CanLaunchAuto()
        {
            return !IsEditing;
        }
    }
}