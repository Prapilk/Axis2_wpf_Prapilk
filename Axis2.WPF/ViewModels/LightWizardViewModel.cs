using Axis2.WPF.Models;
using Axis2.WPF.Mvvm;
using Axis2.WPF.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using Axis2.WPF.Extensions;
using System;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Diagnostics;

namespace Axis2.WPF.ViewModels
{
    public class LightWizardViewModel : BindableBase, IDialog
    {
        private readonly LightDataService _lightDataService;
        private readonly IUoArtService _uoArtService;
        private readonly DispatcherTimer _animationTimer;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private int _currentColorIndex;

        public ObservableCollection<LightColorItemViewModel> LightColorItems { get; set; } = new ObservableCollection<LightColorItemViewModel>();

        private LightColorItemViewModel? _selectedLightColorItem;
        public LightColorItemViewModel? SelectedLightColorItem
        {
            get => _selectedLightColorItem;
            set
            {
                if (SetProperty(ref _selectedLightColorItem, value))
                {
                    _animationTimer.Stop();
                    _stopwatch.Reset();
                    if (_selectedLightColorItem?.ItemTileDataItem != null)
                    {
                        PreviewItem = new SObject { Id = $"0x{_selectedLightColorItem.ItemTileDataItem.Id:X4}" };
                        _currentColorIndex = 0;
                        _animationTimer.Start();
                        _stopwatch.Start();
                    }
                    UpdatePreview();
                }
            }
        }

        private SObject? _previewItem;
        public SObject? PreviewItem
        {
            get => _previewItem;
            set => SetProperty(ref _previewItem, value);
        }

        private BitmapSource? _lightPreviewImage;
        public BitmapSource? LightPreviewImage
        {
            get => _lightPreviewImage;
            set => SetProperty(ref _lightPreviewImage, value);
        }

        private double _currentZoom = 1.0;
        public double CurrentZoom
        {
            get => _currentZoom;
            set => SetProperty(ref _currentZoom, value);
        }

        private double _currentAngle = 0.0;
        public double CurrentAngle
        {
            get => _currentAngle;
            set => SetProperty(ref _currentAngle, value);
        }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public string Title => "Light Wizard";
        public object Content => this;
        public bool? DialogResult { get; private set; }

        public event EventHandler? CloseRequested;

        public LightWizardViewModel(LightDataService lightDataService, IUoArtService uoArtService, SObject? previewItem)
        {
            _lightDataService = lightDataService;
            _uoArtService = uoArtService;
            PreviewItem = previewItem;

            _animationTimer = new DispatcherTimer(DispatcherPriority.Render);
            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 fps
            _animationTimer.Tick += AnimationTimer_Tick;

            LoadLightData();

            if (PreviewItem != null && ushort.TryParse((PreviewItem.DisplayId ?? PreviewItem.Id).Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out ushort previewId))
            {
                _lightDataService.TileDataItems.TryGetValue(previewId, out var tileDataItem);
                if (tileDataItem != null)
                {
                    byte previewLayer = tileDataItem.Quality;
                    SelectedLightColorItem = LightColorItems.FirstOrDefault(item => item.Layer == previewLayer) ?? LightColorItems.FirstOrDefault();
                }
                else
                {
                    SelectedLightColorItem = LightColorItems.FirstOrDefault();
                }
            }
            else
            {
                SelectedLightColorItem = LightColorItems.FirstOrDefault();
            }

            OkCommand = new RelayCommand(OnOk);
            CancelCommand = new RelayCommand(OnCancel);
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (SelectedLightColorItem?.DrawConfigEntry != null)
            {
                var drawConfig = SelectedLightColorItem.DrawConfigEntry;
                float elapsedTime = _stopwatch.ElapsedMilliseconds / 1000.0f;

                // Color alternation
                if (drawConfig.Alternance > 0 && drawConfig.ColorIds.Count > 1)
                {
                    int newIndex = (int)(elapsedTime / drawConfig.Alternance) % drawConfig.ColorIds.Count;
                    if (newIndex != _currentColorIndex)
                    {
                        _currentColorIndex = newIndex;
                        UpdatePreview();
                    }
                }

                // Zoom
                if (drawConfig.TimeZoom > 0)
                {
                    CurrentZoom = drawConfig.Dezoom + ((drawConfig.Zoom - drawConfig.Dezoom) / 2) * (0.5f * (1 + Math.Sin(elapsedTime * drawConfig.TimeZoom)));
                }
                else
                {
                    CurrentZoom = drawConfig.Zoom;
                }
                if (CurrentZoom < drawConfig.Dezoom)
                {
                    CurrentZoom = drawConfig.Dezoom;
                }

                // Rotation
                if (drawConfig.Rotation > 0)
                {
                    float rotationProgress = (float)fmod(elapsedTime, drawConfig.Rotation);
                    CurrentAngle = (rotationProgress / drawConfig.Rotation) * 360.0f;
                }
                else
                {
                    CurrentAngle = 0.0f;
                }
            }
        }
        
        private double fmod(double a, double b)
        {
            return a - Math.Floor(a / b) * b;
        }


        private void LoadLightData()
        {
            LightColorItems.Clear();
            
            var lightMulItems = _lightDataService.LightMulItems.GroupBy(lm => lm.Id).ToDictionary(g => g.Key, g => g.First());
            var drawConfigs = _lightDataService.DrawConfigs.ToDictionary(dc => dc.Id);

            foreach (var lightColor in _lightDataService.LightColors.Values)
            {
                _lightDataService.TileDataItems.TryGetValue(lightColor.Id, out var itemTileDataItem);
                LightMulItem? lightMulItem = null;
                if (itemTileDataItem != null)
                {
                    lightMulItems.TryGetValue((ushort)itemTileDataItem.AnimId, out lightMulItem);
                }
                drawConfigs.TryGetValue(lightColor.DrawConfigId, out var drawConfig);

                if (itemTileDataItem != null && lightMulItem != null && (itemTileDataItem.Flags & 0x00800000) != 0)
                {
                    LightColorItems.Add(new LightColorItemViewModel
                    {
                        ItemTileDataItem = itemTileDataItem,
                        LightMulItem = lightMulItem,
                        DrawConfigEntry = drawConfig,
                        Comment = lightColor.Comment
                    });
                }
            }
        }

        private void UpdatePreview()
        {
            if (SelectedLightColorItem == null || SelectedLightColorItem.DrawConfigEntry == null)
            {
                LightPreviewImage = null;
                return;
            }

            var drawConfig = SelectedLightColorItem.DrawConfigEntry;

            if (drawConfig.ColorIds.Any())
            {
                ushort colorId = drawConfig.ColorIds[_currentColorIndex];
                ushort lightId = (ushort)(SelectedLightColorItem.Layer > 0 ? SelectedLightColorItem.Layer : 0);
                LightPreviewImage = _uoArtService.GetLightImage(lightId, colorId);
            }
            else
            {
                LightPreviewImage = null;
            }
        }

        private void OnOk()
        {
            DialogResult = true;
            Close();
        }

        private void OnCancel()
        {
            DialogResult = false;
            Close();
        }

        public void Close()
        {
            _animationTimer.Stop();
            _stopwatch.Stop();
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}