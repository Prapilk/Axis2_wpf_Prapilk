using Axis2.WPF.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Text.Json.Serialization;

namespace Axis2.WPF.ViewModels.Settings
{
    public class FilePathItem : BindableBase
    {
        private string _fileName;
        private string _filePath;
        private string _originalFilePath; // New property

        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public string OriginalFilePath // New property
        {
            get => _originalFilePath;
            set => SetProperty(ref _originalFilePath, value);
        }
    }

    public class SettingsOverridePathsViewModel : BindableBase
    {
        private ObservableCollection<FilePathItem> _filePaths;
        private FilePathItem _selectedFilePathItem;
        private SettingsFilePathsViewModel _filePathsSettings;

        public ObservableCollection<FilePathItem> FilePaths
        {
            get => _filePaths;
            set => SetProperty(ref _filePaths, value);
        }

        public FilePathItem SelectedFilePathItem
        {
            get => _selectedFilePathItem;
            set => SetProperty(ref _selectedFilePathItem, value);
        }

        [JsonIgnore]
        public ICommand BrowsePathCommand { get; }
        [JsonIgnore]
        public ICommand ResetPathCommand { get; }
        [JsonIgnore]
        public ICommand ResetAllPathsCommand { get; }

        public SettingsOverridePathsViewModel()
        {
            // Initialize properties with default values
            FilePaths = new ObservableCollection<FilePathItem>();
            InitializeDefaultFilePaths("C:/UO/"); // Default UO path

            BrowsePathCommand = new RelayCommand(BrowsePath, CanExecuteBrowsePath);
            ResetPathCommand = new RelayCommand(ResetPath, CanExecuteResetPath);
            ResetAllPathsCommand = new RelayCommand(ResetAllPaths);
        }

        public void SetFilePathsSettings(SettingsFilePathsViewModel filePathsSettings)
        {
            _filePathsSettings = filePathsSettings;
            _filePathsSettings.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(SettingsFilePathsViewModel.DefaultMulPath))
                {
                    UpdateDefaultPaths(_filePathsSettings.DefaultMulPath);
                }
            };
            // Update paths immediately with the injected settings
            UpdateDefaultPaths(_filePathsSettings.DefaultMulPath);
        }

        private bool CanExecuteBrowsePath()
        {
            return SelectedFilePathItem != null;
        }

        private void BrowsePath()
        {
            if (SelectedFilePathItem != null)
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Title = $"Select {SelectedFilePathItem.FileName}";
                openFileDialog.FileName = SelectedFilePathItem.FileName; // Set initial file name

                // Set initial directory if the current path is valid
                string initialDirectory = System.IO.Path.GetDirectoryName(SelectedFilePathItem.FilePath);
                if (System.IO.Directory.Exists(initialDirectory))
                {
                    openFileDialog.InitialDirectory = initialDirectory;
                }
                else
                {
                    // Fallback to a common directory if the initial one doesn't exist
                    openFileDialog.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                }

                if (openFileDialog.ShowDialog() == true)
                {
                    SelectedFilePathItem.FilePath = openFileDialog.FileName;
                }
            }
        }

        private bool CanExecuteResetPath()
        {
            return SelectedFilePathItem != null;
        }

        public void ResetPath() // Changed to public
        {
            if (SelectedFilePathItem != null)
            {
                SelectedFilePathItem.FilePath = SelectedFilePathItem.OriginalFilePath;
            }
        }

        public void ResetAllPaths()
        {
            InitializeDefaultFilePaths("C:/UO/"); // Reset to original default
        }

        public void UpdateDefaultPaths(string newMulPath)
        {
            InitializeDefaultFilePaths(newMulPath);
        }

        private void InitializeDefaultFilePaths(string baseMulPath)
        {
            FilePaths.Clear();
            FilePaths.Add(new FilePathItem { FileName = "radarcol.mul", FilePath = baseMulPath + "radarcol.mul", OriginalFilePath = baseMulPath + "radarcol.mul" });

            for (int i = 0; i <= 5; i++)
            {
                FilePaths.Add(new FilePathItem { FileName = $"map{i}.mul", FilePath = baseMulPath + $"map{i}.mul", OriginalFilePath = baseMulPath + $"map{i}.mul" });
                FilePaths.Add(new FilePathItem { FileName = $"statics{i}.mul", FilePath = baseMulPath + $"statics{i}.mul", OriginalFilePath = baseMulPath + $"statics{i}.mul" });
                FilePaths.Add(new FilePathItem { FileName = $"staidx{i}.mul", FilePath = baseMulPath + $"staidx{i}.mul", OriginalFilePath = baseMulPath + $"staidx{i}.mul" });
            }
            
            FilePaths.Add(new FilePathItem { FileName = "art.mul", FilePath = baseMulPath + "art.mul", OriginalFilePath = baseMulPath + "art.mul" });
            FilePaths.Add(new FilePathItem { FileName = "anim.mul", FilePath = baseMulPath + "anim.mul", OriginalFilePath = baseMulPath + "anim.mul" });
            FilePaths.Add(new FilePathItem { FileName = "gump.mul", FilePath = baseMulPath + "gump.mul", OriginalFilePath = baseMulPath + "gump.mul" });
            FilePaths.Add(new FilePathItem { FileName = "sound.mul", FilePath = baseMulPath + "sound.mul", OriginalFilePath = baseMulPath + "sound.mul" });
            FilePaths.Add(new FilePathItem { FileName = "soundidx.mul", FilePath = baseMulPath + "soundidx.mul", OriginalFilePath = baseMulPath + "soundidx.mul" });
            FilePaths.Add(new FilePathItem { FileName = "multi.mul", FilePath = baseMulPath + "multi.mul", OriginalFilePath = baseMulPath + "multi.mul" });
            FilePaths.Add(new FilePathItem { FileName = "hues.mul", FilePath = baseMulPath + "hues.mul", OriginalFilePath = baseMulPath + "hues.mul" });
            FilePaths.Add(new FilePathItem { FileName = "light.mul", FilePath = baseMulPath + "light.mul", OriginalFilePath = baseMulPath + "light.mul" });
            FilePaths.Add(new FilePathItem { FileName = "lightidx.mul", FilePath = baseMulPath + "lightidx.mul", OriginalFilePath = baseMulPath + "lightidx.mul" });
            FilePaths.Add(new FilePathItem { FileName = "tiledata.mul", FilePath = baseMulPath + "tiledata.mul", OriginalFilePath = baseMulPath + "tiledata.mul" });
            FilePaths.Add(new FilePathItem { FileName = "animdata.mul", FilePath = baseMulPath + "animdata.mul", OriginalFilePath = baseMulPath + "animdata.mul" });
            FilePaths.Add(new FilePathItem { FileName = "body.def", FilePath = baseMulPath + "body.def", OriginalFilePath = baseMulPath + "body.def" });
            FilePaths.Add(new FilePathItem { FileName = "bodyconv.def", FilePath = baseMulPath + "bodyconv.def", OriginalFilePath = baseMulPath + "bodyconv.def" });

            // Add OrionData files
            FilePaths.Add(new FilePathItem { FileName = "light_colors.txt", FilePath = System.IO.Path.Combine(baseMulPath, "OrionData", "light_colors.txt"), OriginalFilePath = System.IO.Path.Combine(baseMulPath, "OrionData", "light_colors.txt") });
            FilePaths.Add(new FilePathItem { FileName = "draw_config.txt", FilePath = System.IO.Path.Combine(baseMulPath, "OrionData", "draw_config.txt"), OriginalFilePath = System.IO.Path.Combine(baseMulPath, "OrionData", "draw_config.txt") });

            for (int i = 0; i <= 4; i++)
            {
                FilePaths.Add(new FilePathItem { FileName = $"mapdif{i}.mul", FilePath = baseMulPath + $"mapdif{i}.mul", OriginalFilePath = baseMulPath + $"mapdif{i}.mul" });
                FilePaths.Add(new FilePathItem { FileName = $"stadif{i}.mul", FilePath = baseMulPath + $"stadif{i}.mul", OriginalFilePath = baseMulPath + $"stadif{i}.mul" });
            }

            FilePaths.Add(new FilePathItem { FileName = "verdata.mul", FilePath = baseMulPath + "verdata.mul", OriginalFilePath = baseMulPath + "verdata.mul" });
            FilePaths.Add(new FilePathItem { FileName = "mobtypes.txt", FilePath = baseMulPath + "mobtypes.txt", OriginalFilePath = baseMulPath + "mobtypes.txt" });
            FilePaths.Add(new FilePathItem { FileName = "AnimationSequence.uop", FilePath = baseMulPath + "AnimationSequence.uop", OriginalFilePath = baseMulPath + "AnimationSequence.uop" });
            FilePaths.Add(new FilePathItem { FileName = "AnimationFrame1.uop", FilePath = baseMulPath + "AnimationFrame1.uop", OriginalFilePath = baseMulPath + "AnimationFrame1.uop" });
            FilePaths.Add(new FilePathItem { FileName = "AnimationFrame2.uop", FilePath = baseMulPath + "AnimationFrame2.uop", OriginalFilePath = baseMulPath + "AnimationFrame2.uop" });
            FilePaths.Add(new FilePathItem { FileName = "AnimationFrame3.uop", FilePath = baseMulPath + "AnimationFrame3.uop", OriginalFilePath = baseMulPath + "AnimationFrame3.uop" });
            FilePaths.Add(new FilePathItem { FileName = "AnimationFrame4.uop", FilePath = baseMulPath + "AnimationFrame4.uop", OriginalFilePath = baseMulPath + "AnimationFrame4.uop" });
        }
    }
}