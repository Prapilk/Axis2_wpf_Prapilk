using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Axis2.WPF.Views
{
    public partial class RoomViewControl : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty ItemImageWidthProperty =
            DependencyProperty.Register("ItemImageWidth", typeof(double), typeof(RoomViewControl), new PropertyMetadata(0.0, OnItemImageSizeChanged));

        public double ItemImageWidth
        {
            get { return (double)GetValue(ItemImageWidthProperty); }
            set { SetValue(ItemImageWidthProperty, value); }
        }

        public static readonly DependencyProperty ItemImageHeightProperty =
            DependencyProperty.Register("ItemImageHeight", typeof(double), typeof(RoomViewControl), new PropertyMetadata(0.0, OnItemImageSizeChanged));

        public double ItemImageHeight
        {
            get { return (double)GetValue(ItemImageHeightProperty); }
            set { SetValue(ItemImageHeightProperty, value); }
        }

        public static readonly DependencyProperty XOffsetProperty =
            DependencyProperty.Register("XOffset", typeof(double), typeof(RoomViewControl), new PropertyMetadata(0.0, OnOffsetChanged));

        public double XOffset
        {
            get { return (double)GetValue(XOffsetProperty); }
            set { SetValue(XOffsetProperty, value); }
        }

        public static readonly DependencyProperty YOffsetProperty =
            DependencyProperty.Register("YOffset", typeof(double), typeof(RoomViewControl), new PropertyMetadata(0.0, OnOffsetChanged));

        public double YOffset
        {
            get { return (double)GetValue(YOffsetProperty); }
            set { SetValue(YOffsetProperty, value); }
        }

        public RoomViewControl()
        {
            InitializeComponent();
        }

        private static void OnItemImageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RoomViewControl)d).DrawRoomViewGrid();
        }

        private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RoomViewControl)d).DrawRoomViewGrid();
        }

        private void RoomViewControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            DrawRoomViewGrid();
        }

        private void RoomViewCanvas_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            DrawRoomViewGrid();
        }

        private void DrawRoomViewGrid()
        {
            RoomViewCanvas.Children.Clear();

            if (ItemImageWidth <= 0 || ItemImageHeight <= 0) return;

            // Constants from UOart.cpp for isometric projection
            double tileWidth = 44; // Width of an isometric tile
            double tileHeight = 44; // Height of an isometric tile
            double halfTileWidth = tileWidth / 2;
            double halfTileHeight = tileHeight / 2;

            // The image is centered within its container, so we need to adjust the grid drawing
            // to align with the image's top-left corner relative to the canvas.
            double imageDisplayX = (RoomViewCanvas.ActualWidth - ItemImageWidth) / 2;
            double imageDisplayY = (RoomViewCanvas.ActualHeight - ItemImageHeight) / 2;

            // Draw a single tile (floor outline and vertical lines)
            // The central point for the grid, adjusted by the item's image center and the offsets.
            // This represents the (0,0) point of our isometric grid in screen coordinates.
            double tileCenterX = imageDisplayX + (ItemImageWidth / 2) + XOffset;
            double tileCenterY = imageDisplayY + ItemImageHeight + YOffset;

            double wallHeight = -85; // Assuming a fixed wall height for now, can be adjusted

            System.Windows.Media.SolidColorBrush lineBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            double lineThickness = 1;
            double lineLength = wallHeight; // Lines extend downwards by wall height

            // Points of the diamond (floor outline)
            System.Windows.Point pTop = new System.Windows.Point(tileCenterX, tileCenterY - tileHeight);
            System.Windows.Point pLeft = new System.Windows.Point(tileCenterX - halfTileWidth, tileCenterY - halfTileHeight);
            System.Windows.Point pBottom = new System.Windows.Point(tileCenterX, tileCenterY);
            System.Windows.Point pRight = new System.Windows.Point(tileCenterX + halfTileWidth, tileCenterY - halfTileHeight);

            // Draw floor outline lines
            RoomViewCanvas.Children.Add(CreateLine(pTop, pLeft, lineBrush, lineThickness));
            RoomViewCanvas.Children.Add(CreateLine(pLeft, pBottom, lineBrush, lineThickness));
            RoomViewCanvas.Children.Add(CreateLine(pBottom, pRight, lineBrush, lineThickness));
            RoomViewCanvas.Children.Add(CreateLine(pRight, pTop, lineBrush, lineThickness)); // Close the diamond

            // Draw vertical lines from corners
            RoomViewCanvas.Children.Add(CreateLine(pTop, new System.Windows.Point(pTop.X, pTop.Y + lineLength), lineBrush, lineThickness));
            RoomViewCanvas.Children.Add(CreateLine(pLeft, new System.Windows.Point(pLeft.X, pLeft.Y + lineLength), lineBrush, lineThickness));
            RoomViewCanvas.Children.Add(CreateLine(pRight, new System.Windows.Point(pRight.X, pRight.Y + lineLength), lineBrush, lineThickness));
            RoomViewCanvas.Children.Add(CreateLine(pBottom, new System.Windows.Point(pBottom.X, pBottom.Y + lineLength), lineBrush, lineThickness));

            // Points of the bottom diamond
            System.Windows.Point pTopBottom = new System.Windows.Point(pTop.X, pTop.Y + lineLength);
            System.Windows.Point pLeftBottom = new System.Windows.Point(pLeft.X, pLeft.Y + lineLength);
            System.Windows.Point pBottomBottom = new System.Windows.Point(pBottom.X, pBottom.Y + lineLength);
            System.Windows.Point pRightBottom = new System.Windows.Point(pRight.X, pRight.Y + lineLength);

            // Draw bottom outline lines (only visible ones)
            RoomViewCanvas.Children.Add(CreateLine(pLeftBottom, pBottomBottom, lineBrush, lineThickness));
            RoomViewCanvas.Children.Add(CreateLine(pBottomBottom, pRightBottom, lineBrush, lineThickness));
        }

        private System.Windows.Shapes.Line CreateLine(System.Windows.Point p1, System.Windows.Point p2, System.Windows.Media.SolidColorBrush stroke, double thickness)
        {
            return new System.Windows.Shapes.Line
            {
                X1 = p1.X,
                Y1 = p1.Y,
                X2 = p2.X,
                Y2 = p2.Y,
                Stroke = stroke,
                StrokeThickness = thickness
            };
        }
    }
}