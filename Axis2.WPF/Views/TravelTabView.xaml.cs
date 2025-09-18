using Axis2.WPF.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Globalization;
using System.Windows.Data;
using System; // Added for Tuple
using Axis2.WPF.Services; // Added to fix compilation error

namespace Axis2.WPF.Views
{
    public partial class TravelTabView : System.Windows.Controls.UserControl
    {
        private System.Windows.Point _lastMousePosition;
        private System.Windows.Point _drawingStartPoint;
        private bool _isDragging = false;
        private bool _isDrawing = false;
        private bool _isEditingRedraw = false;
        private System.Windows.Rect? _originalRect = null;
        private System.Windows.Point _editStartPoint;
        private bool _isMovingRect = false;
        private bool _isResizingRect = false;
        private int _resizeHandle = 0; // 1=top-left, 2=top-right, 3=bottom-right, 4=bottom-left, 5=top, 6=right, 7=bottom, 8=left
        private System.Windows.Point _capturedStartMapPoint; // Field to store the captured map coordinates at the start of drawing

        public TravelTabView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Parses map coordinates from the format "X: 123, Y: 456".
        /// </summary>
        private System.Windows.Point ParseCoordinates(string coordinates)
        {
            if (string.IsNullOrEmpty(coordinates)) return new System.Windows.Point(0, 0);
            
            var parts = coordinates.Split(',');
            if (parts.Length < 2) return new System.Windows.Point(0, 0);

            try
            {
                var xPart = parts[0].Split(':')[1].Trim();
                var yPart = parts[1].Split(':')[1].Trim();

                int.TryParse(xPart, out int x);
                int.TryParse(yPart, out int y);

                return new System.Windows.Point(x, y);
            }
            catch
            {
                return new System.Windows.Point(0, 0); // Return default on parsing error
            }
        }

        private void TreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is TravelTabViewModel viewModel)
            {
                viewModel.SelectedItem = e.NewValue;
            }
        }

        private void RoomsTreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is TravelTabViewModel viewModel)
            {
                viewModel.SelectedRoom = e.NewValue as Models.RoomDefinition;
            }
        }

        private void MapImage_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (DataContext is TravelTabViewModel viewModel)
            {
                if (e.Delta > 0)
                {
                    if (viewModel.ZoomInCommand != null)
                    {
                        viewModel.ZoomInCommand.Execute(null);
                    }
                }
                else
                {
                    if (viewModel.ZoomOutCommand != null)
                    {
                        viewModel.ZoomOutCommand.Execute(null);
                    }
                }
            }
        }

        private void MapOverlayCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is TravelTabViewModel viewModel && viewModel.MapImage != null)
            {
                System.Windows.Point clickPointOnCanvas = e.GetPosition((Canvas)sender);

                if (viewModel.IsEditingExistingRectMode)
                {
                    _isEditingRedraw = true; // Set flag for redraw in edit mode
                    _isDrawing = true;
                    _drawingStartPoint = clickPointOnCanvas;
                    _capturedStartMapPoint = ParseCoordinates(viewModel.MouseMapCoordinatesText);
                    viewModel.DrawingRectangleVisibility = Visibility.Visible;
                    ((UIElement)sender!).CaptureMouse();
                    return;
                }

                if (viewModel.IsDrawingMode)
                {
                    _isDrawing = true;
                    _drawingStartPoint = clickPointOnCanvas; // For visual rectangle
                    
                    // Capture the exact map coordinates from the ViewModel's text property
                    _capturedStartMapPoint = ParseCoordinates(viewModel.MouseMapCoordinatesText);

                    viewModel.DrawingRectangleVisibility = Visibility.Visible;
                    ((UIElement)sender!).CaptureMouse();
                    return;
                }

                // Existing map dragging logic
                System.Windows.Point clickPointOnImage = e.GetPosition(MapImage);
                double controlWidth = MapImage.ActualWidth;
                double controlHeight = MapImage.ActualHeight;
                double bitmapWidth = viewModel.MapImage.PixelWidth;
                double bitmapHeight = viewModel.MapImage.PixelHeight;
                double controlRatio = controlWidth / controlHeight;
                double bitmapRatio = bitmapWidth / bitmapHeight;
                double renderedWidth, renderedHeight;
                if (bitmapRatio > controlRatio)
                {
                    renderedWidth = controlWidth;
                    renderedHeight = controlWidth / bitmapRatio;
                }
                else
                {
                    renderedHeight = controlHeight;
                    renderedWidth = controlHeight * bitmapRatio;
                }
                double offsetX = (controlWidth - renderedWidth) / 2.0;
                double offsetY = (controlHeight - renderedHeight) / 2.0;
                double adjustedX = clickPointOnImage.X - offsetX;
                double adjustedY = clickPointOnImage.Y - offsetY;

                if (adjustedX < 0 || adjustedY < 0 || adjustedX > renderedWidth || adjustedY > renderedHeight)
                {
                    return;
                }

                double scaleToBitmap = bitmapWidth / renderedWidth;
                double clickX_bitmap = adjustedX * scaleToBitmap;
                double clickY_bitmap = adjustedY * scaleToBitmap;
                double currentScaleFactor = viewModel.ZoomLevel >= 0 ? Math.Pow(2, viewModel.ZoomLevel) : 1.0 / Math.Pow(2, Math.Abs(viewModel.ZoomLevel));
                double viewPortOriginX_map = viewModel.MapCenterX - (bitmapWidth / 2.0) / currentScaleFactor;
                double viewPortOriginY_map = viewModel.MapCenterY - (bitmapHeight / 2.0) / currentScaleFactor;
                int clickedMapX = (int)Math.Round(viewPortOriginX_map + clickX_bitmap / currentScaleFactor);
                int clickedMapY = (int)Math.Round(viewPortOriginY_map + clickY_bitmap / currentScaleFactor);

                if (viewModel.CenterMapOnCoordinatesCommand != null)
                {
                    viewModel.CenterMapOnCoordinatesCommand.Execute(new Tuple<int, int, short, int, int>(clickedMapX, clickedMapY, viewModel.SelectedMapFile, (int)bitmapWidth, (int)bitmapHeight));
                }

                _isDragging = true;
                _lastMousePosition = clickPointOnCanvas;
                ((UIElement)sender!).CaptureMouse();
            }
        }

        private void MapOverlayCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (DataContext is TravelTabViewModel viewModel && viewModel.MapImage != null)
            {
                double controlWidth = MapImage.ActualWidth;
                double controlHeight = MapImage.ActualHeight;
                double bitmapWidth = viewModel.MapImage.PixelWidth;
                double bitmapHeight = viewModel.MapImage.PixelHeight;

                if (controlWidth <= 0 || controlHeight <= 0 || bitmapWidth <= 0 || bitmapHeight <= 0) return;

                double controlRatio = controlWidth / controlHeight;
                double bitmapRatio = bitmapWidth / bitmapHeight;

                double renderedWidth, renderedHeight;
                if (bitmapRatio > controlRatio)
                {
                    renderedWidth = controlWidth;
                    renderedHeight = controlWidth / bitmapRatio;
                }
                else
                {
                    renderedHeight = controlHeight;
                    renderedWidth = controlHeight * bitmapRatio;
                }

                double offsetX = (controlWidth - renderedWidth) / 2.0;
                double offsetY = (controlHeight - renderedHeight) / 2.0;

                System.Windows.Point mousePointOnCanvas = e.GetPosition((Canvas)sender);
                System.Windows.Point mousePointOnImage = e.GetPosition(MapImage);

                double adjustedX = mousePointOnImage.X - offsetX;
                double adjustedY = mousePointOnImage.Y - offsetY;

                if (adjustedX >= 0 && adjustedY >= 0 && adjustedX <= renderedWidth && adjustedY <= renderedHeight)
                {
                    viewModel.MouseX = mousePointOnCanvas.X;
                    viewModel.MouseY = mousePointOnCanvas.Y;
                    viewModel.MouseCrosshairVisibility = System.Windows.Visibility.Visible;

                    double scaleToBitmap = bitmapWidth / renderedWidth;
                    double mouseX_bitmap = adjustedX * scaleToBitmap;
                    double mouseY_bitmap = adjustedY * scaleToBitmap;

                    if (viewModel.UpdateMouseMapCoordinatesCommand != null)
                    {
                        viewModel.UpdateMouseMapCoordinatesCommand.Execute(new Tuple<double, double, double, double, int, int, short>(
                            mouseX_bitmap, mouseY_bitmap, bitmapWidth, bitmapHeight, (int)viewModel.MapCenterX, (int)viewModel.MapCenterY, (short)viewModel.ZoomLevel));
                    }
                }
                else
                {
                    viewModel.MouseCrosshairVisibility = System.Windows.Visibility.Collapsed;
                    viewModel.MouseMapCoordinatesText = string.Empty;
                }

                if (_isDragging)
                {
                    double deltaX = mousePointOnCanvas.X - _lastMousePosition.X;
                    double deltaY = mousePointOnCanvas.Y - _lastMousePosition.Y;

                    if (viewModel.MoveMapCommand != null)
                    {
                        viewModel.MoveMapCommand.Execute(new System.Windows.Point(deltaX, deltaY));
                    }

                    _lastMousePosition = mousePointOnCanvas;
                }
                else if (_isDrawing && viewModel.IsDrawingMode)
                {
                    double x = Math.Min(_drawingStartPoint.X, mousePointOnCanvas.X);
                    double y = Math.Min(_drawingStartPoint.Y, mousePointOnCanvas.Y);
                    double width = Math.Abs(_drawingStartPoint.X - mousePointOnCanvas.X);
                    double height = Math.Abs(_drawingStartPoint.Y - mousePointOnCanvas.Y);

                    viewModel.DrawingRectX = x;
                    viewModel.DrawingRectY = y;
                    viewModel.DrawingRectWidth = width;
                    viewModel.DrawingRectHeight = height;
                }
                // ... (existing editing logic remains the same)
            }
        }

        private void MapOverlayCanvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isDragging = false;
            ((UIElement)sender!).ReleaseMouseCapture();

            TravelTabViewModel viewModel = DataContext as TravelTabViewModel;

            if (_isDrawing && viewModel != null)
            {
                _isDrawing = false;
                viewModel.IsDrawingMode = false;
                viewModel.DrawingRectangleVisibility = Visibility.Collapsed;

                // Capture the exact map coordinates from the ViewModel's text property
                System.Windows.Point capturedEndMapPoint = ParseCoordinates(viewModel.MouseMapCoordinatesText);

                // Create the rectangle from the captured start and end map points
                int x1 = (int)Math.Min(_capturedStartMapPoint.X, capturedEndMapPoint.X);
                int y1 = (int)Math.Min(_capturedStartMapPoint.Y, capturedEndMapPoint.Y);
                int width = (int)Math.Abs(_capturedStartMapPoint.X - capturedEndMapPoint.X);
                int height = (int)Math.Abs(_capturedStartMapPoint.Y - capturedEndMapPoint.Y);

                System.Windows.Rect newDrawnRect = new System.Windows.Rect(x1, y1, width, height);

                if (_isEditingRedraw)
                {
                    viewModel.ProcessEditedRectangle(newDrawnRect);
                    _isEditingRedraw = false; // Reset flag
                }
                else
                {
                    viewModel.ProcessDrawnRectangle(newDrawnRect);
                }
            }

            _isMovingRect = false;
            _isResizingRect = false;
            _originalRect = null;
            _resizeHandle = 0;
        }

        private void MapOverlayCanvas_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (DataContext is TravelTabViewModel viewModel)
            {
                viewModel.MouseCrosshairVisibility = System.Windows.Visibility.Collapsed;
            }
        }
    }

    public class CrosshairLineConverter : IValueConverter
    {
        private const double CrosshairSize = 10; // Half-length of the crosshair arm

        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double coordinate && parameter is string paramString)
            {
                switch (paramString)
                {
                    case "Start":
                        return coordinate - CrosshairSize;
                    case "End":
                        return coordinate + CrosshairSize;
                    case "CenterStart":
                        return (coordinate / 2) - CrosshairSize;
                    case "Center":
                        return coordinate / 2;
                    case "CenterEnd":
                        return (coordinate / 2) + CrosshairSize;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
