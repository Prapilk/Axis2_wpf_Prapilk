using Axis2.WPF.Mvvm;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Text.Json.Serialization;
using System.Windows.Interop;
using System.Windows;
using System;
using Axis2.WPF.ViewModels;

namespace Axis2.WPF.ViewModels.Settings
{
    public class SettingsFilePathsViewModel : BindableBase
    {
        private string _artIdx;
        public string ArtIdx { get => _artIdx; set => SetProperty(ref _artIdx, value); }

        private string _artMul;
        public string ArtMul { get => _artMul; set => SetProperty(ref _artMul, value); }

        private string _animIdx;
        public string AnimIdx { get => _animIdx; set => SetProperty(ref _animIdx, value); }

        private string _animMul;
        public string AnimMul { get => _animMul; set => SetProperty(ref _animMul, value); }

        private string _huesMul;
        public string HuesMul { get => _huesMul; set => SetProperty(ref _huesMul, value); }

        private string _lightColorsTxt;
        public string LightColorsTxt { get => _lightColorsTxt; set => SetProperty(ref _lightColorsTxt, value); }

        private string _drawConfigTxt;
        public string DrawConfigTxt { get => _drawConfigTxt; set => SetProperty(ref _drawConfigTxt, value); }

        private string _scriptsPath;
        public string ScriptsPath { get => _scriptsPath; set => SetProperty(ref _scriptsPath, value); }

        private bool _samePathAsClient;
        private string _defaultClientPath;
        private string _defaultMulPath;

        public bool SamePathAsClient
        {
            get => _samePathAsClient;
            set
            {
                if (SetProperty(ref _samePathAsClient, value) && value)
                {
                    UpdatePathsFromClientPath();
                }
            }
        }

        public string DefaultClientPath
        {
            get => _defaultClientPath;
            set
            {
                if (SetProperty(ref _defaultClientPath, value) && SamePathAsClient)
                {
                    UpdatePathsFromClientPath();
                }
            }
        }

        public string DefaultMulPath
        {
            get => _defaultMulPath;
            set => SetProperty(ref _defaultMulPath, value);
        }

        [JsonIgnore]
        public ICommand BrowseClientPathCommand { get; }
        [JsonIgnore]
        public ICommand BrowseMulPathCommand { get; }
        [JsonIgnore]
        public ICommand BrowseScriptsPathCommand { get; }
        [JsonIgnore]
        public ICommand ResetPathsSettingsCommand { get; }

        public SettingsFilePathsViewModel()
        {
            // Initialize properties with default values
            SamePathAsClient = false;
            DefaultClientPath = "";
            DefaultMulPath = "";
            ScriptsPath = ""; // Should be set by the user

            UpdateMulPaths(DefaultMulPath);

            BrowseClientPathCommand = new RelayCommand(BrowseClientPath);
            BrowseMulPathCommand = new RelayCommand(BrowseMulPath);
            BrowseScriptsPathCommand = new RelayCommand(BrowseScriptsPath);
            ResetPathsSettingsCommand = new RelayCommand(ResetPathsSettings);
        }

        private void UpdatePathsFromClientPath()
        {
            if (!string.IsNullOrEmpty(DefaultClientPath) && File.Exists(DefaultClientPath))
            {
                DefaultMulPath = Path.GetDirectoryName(DefaultClientPath) + "\\";
                UpdateMulPaths(DefaultMulPath);
            }
        }

        private void UpdateMulPaths(string mulPath)
        {
            ArtIdx = Path.Combine(mulPath, "artidx.mul");
            ArtMul = Path.Combine(mulPath, "art.mul");
            HuesMul = Path.Combine(mulPath, "hues.mul");
            AnimIdx = Path.Combine(mulPath, "anim.idx");
            AnimMul = Path.Combine(mulPath, "anim.mul");

            // OrionData files
            // string orionDataPath = Path.Combine(mulPath, "OrionData"); // Removed duplicate declaration
            LightColorsTxt = Path.Combine(Path.Combine(mulPath, "OrionData"), "light_colors.txt");
            DrawConfigTxt = Path.Combine(Path.Combine(mulPath, "OrionData"), "draw_config.txt");

            // OrionData files
            string orionDataPath = Path.Combine(mulPath, "OrionData");
            // Ensure OrionData directory exists if needed, or handle its absence
            // For now, just combine paths
            LightColorsTxt = Path.Combine(orionDataPath, "light_colors.txt");
            DrawConfigTxt = Path.Combine(orionDataPath, "draw_config.txt");
        }

        private void BrowseClientPath()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                DefaultClientPath = openFileDialog.FileName;
            }
        }

        private void BrowseMulPath()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog(new Wpf32Window(System.Windows.Application.Current.MainWindow)) == DialogResult.OK)
            {
                DefaultMulPath = folderBrowserDialog.SelectedPath + "\\";
                UpdateMulPaths(DefaultMulPath);
            }
        }

        private void BrowseScriptsPath()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog(new Wpf32Window(System.Windows.Application.Current.MainWindow)) == DialogResult.OK)
            {
                ScriptsPath = folderBrowserDialog.SelectedPath;
            }
        }

        public void ResetPathsSettings()
        {
            SamePathAsClient = false;
            DefaultClientPath = "";
            DefaultMulPath = "";
            ScriptsPath = "";
            UpdateMulPaths(DefaultMulPath);
        }
    }
}