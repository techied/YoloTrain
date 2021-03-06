﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace YoloTrain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point _origMouseDownPoint;
        private bool _isLeftMouseButtonDown;
        private double x, y, width, height;
        private System.Drawing.Bitmap _img;

        public MainWindow()
        {
            InitializeComponent();

            _img = new System.Drawing.Bitmap(System.Drawing.Image.FromFile(@"F:\spartiates\sharks_vs_ella_3star\sharks_vs_ella_frames\scene01501.png"));
        }

        private void imgTrain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isLeftMouseButtonDown = true;
                _origMouseDownPoint = e.GetPosition(imgTrain);

                imgTrain.CaptureMouse();

                e.Handled = true;
            }
        }

        private void imgTrain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isLeftMouseButtonDown = false;
                imgTrain.ReleaseMouseCapture();
                dragSelectionCanvas.Visibility = Visibility.Collapsed;
                e.Handled = true;

                var imgOffset = GetAbsolutePlacement(imgTrain);

                var realx = x - imgOffset.X;
                var realy = y - imgOffset.Y;
                var scaley = _img.Height / imgTrain.ActualHeight;
                var scalex = _img.Width / imgTrain.ActualWidth;
                var imgy = realy * scaley;
                var imgx = realx * scalex;

                var rect = new System.Drawing.Rectangle((int)imgx, (int)imgy, (int)(width * scalex), (int)(height * scaley));
                var bmp = new System.Drawing.Bitmap(_img);
                var newImg = bmp.Clone(rect, bmp.PixelFormat);
                newImg.Save(@"f:\crop.bmp");
            }
        }

        private void imgTrain_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isLeftMouseButtonDown)
            {
                Point curMouseDownPoint = e.GetPosition(imgTrain);

                var imgOffset = GetAbsolutePlacement(imgTrain);

                var tmpx = Math.Min(_origMouseDownPoint.X, curMouseDownPoint.X) + imgOffset.X;
                var tmpy = Math.Min(_origMouseDownPoint.Y, curMouseDownPoint.Y) + imgOffset.Y;
                double overx = 0, overy = 0;
                if (tmpx < imgOffset.X)
                {
                    overx = imgOffset.X - tmpx;
                    tmpx = imgOffset.X;
                }
                if (tmpy < imgOffset.Y)
                {
                    overy = imgOffset.Y - tmpy;
                    tmpy = imgOffset.Y;
                }

                var tmpwidth = Math.Abs(_origMouseDownPoint.X - curMouseDownPoint.X) - overx;
                var tmpheight = Math.Abs(_origMouseDownPoint.Y - curMouseDownPoint.Y) - overy;

                if (tmpx + tmpwidth - imgOffset.X > imgTrain.ActualWidth)
                    tmpwidth = imgTrain.ActualWidth - tmpx + imgOffset.X;
                if (tmpy + tmpheight - imgOffset.Y > imgTrain.ActualHeight)
                    tmpheight = imgTrain.ActualHeight - tmpy + imgOffset.Y;

                x = tmpx;
                y = tmpy;
                width = tmpwidth;
                height = tmpheight;

                Canvas.SetLeft(dragSelectionBorder, x);
                Canvas.SetTop(dragSelectionBorder, y);
                dragSelectionBorder.Width = width;
                dragSelectionBorder.Height = height;

                dragSelectionCanvas.Visibility = Visibility.Visible;

                var realx = x - imgOffset.X;
                var realy = y - imgOffset.Y;
                var scaley = _img.Height / imgTrain.ActualHeight;
                var scalex = _img.Width / imgTrain.ActualWidth;

                int imgy = (int)(realy * scaley);
                int imgx = (int)(realx * scalex);
                int rwidth = (int)(width * scalex);
                int rheight = (int)(height * scaley);

                e.Handled = true;

                if (rwidth == 0 || rheight == 0)
                {
                    txtCoords.Text = "";
                    return;
                }

                double dataX, dataY, dataHeight, dataWidth;
                dataX = (rwidth / 2.0 + imgx) / (double)_img.Width;
                dataY = (rheight / 2.0 + imgy) / (double)_img.Height;
                dataHeight = (double)rheight / _img.Height;
                dataWidth = (double)rwidth / _img.Width;

                rwidth = (int)(dataWidth * _img.Width);
                rheight = (int)(dataHeight * _img.Height);
                imgx = (int)(dataX * _img.Width - rwidth / 2.0);
                imgy = (int)(dataY * _img.Height - rheight / 2.0);

                txtCoords.Text = string.Format("x: {0:0.000000} y: {1:0.000000} w: {2:0.000000} h: {3:0.000000}", dataX, dataY, dataWidth, dataHeight);

                var rect = new System.Drawing.Rectangle(imgx, imgy, rwidth, rheight);
                //var bmp = new System.Drawing.Bitmap(_img);
                var newImg = _img.Clone(rect, _img.PixelFormat);

                var bmpsource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   newImg.GetHbitmap(),
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromWidthAndHeight(rwidth, rheight));
                imgPreview.Source = bmpsource;
            }
        }

        public static Point GetAbsolutePlacement(FrameworkElement element)
        {
            var absolutePos = element.PointToScreen(new Point(0, 0));
            var posMW = Application.Current.MainWindow.PointToScreen(new Point(0, 0));
            var relativePos = new Point(absolutePos.X - posMW.X, absolutePos.Y - posMW.Y);
            return relativePos;
        }
    }
}
