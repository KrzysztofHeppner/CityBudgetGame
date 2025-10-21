using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CityBudget
{
    public partial class MainWindow : Window
    {
        private readonly Stopwatch _stopwatch = new();
        private Timer? _timer;
        DateTime currentDate = new(2000, 1, 1);
        bool isRunning = false;
        double zadowolenie = 50.0;
        bool canClose = true;
        public MainWindow()
        {
            InitializeComponent();

            _stopwatch.Start();
            _timer = new Timer(MainTimerTick, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));

        }

        private void MainTimerTick(object? state)
        {
            //_ = DateTime.IsLeapYear(225) ? 366 : 365;
            //_ = DateTime.DaysInMonth(2024, 2);



            if (isRunning)
            {
                currentDate = currentDate.AddDays(1);                
            }

            if (currentDate.Hour == 0)
            {
                MainNewDay();
                if (currentDate.Day == 1)
                {
                    MainNewMonth();
                }
            }
            

        }

        private void MainNewDay()
        {
            canClose = false;
            Dispatcher.Invoke(() =>
            {
                TimeText.Content = $"{currentDate.Day}.{currentDate.Month}.{currentDate.Year}";
                TextBlock.Text = isRunning ? "Running" : "Paused";
            });
            canClose = true;
        }

        private void MainNewMonth()
        {
            
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Dispose();
            _timer = null;
            _stopwatch.Stop();
            base.OnClosed(e);
        }

        private void PlayStopButton_Click(object sender, RoutedEventArgs e)
        {
            var storyboard = isRunning
                ? (Storyboard)FindResource("PlayToStop")
                : (Storyboard)FindResource("StopToPlay");

            storyboard.Begin();
            isRunning = !isRunning;
        }

        #region TaskBar
        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void BtnExit_MouseEnter(object sender, MouseEventArgs e)
        {
            BtnExitBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3d, 0x3d, 0x3d));
        }

        private void BtnExit_MouseLeave(object sender, MouseEventArgs e)
        {
            BtnExitBorder.Background = new SolidColorBrush(Color.FromArgb(0x00, 0x1f, 0x1f, 0x1f));
        }

        private void MinExit_MouseEnter(object sender, MouseEventArgs e)
        {
            MinExit.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x3d, 0x3d, 0x3d));
        }

        private void MinExit_MouseLeave(object sender, MouseEventArgs e)
        {
            MinExit.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x1f, 0x1f, 0x1f));
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            if(canClose)
                App.Current.Shutdown();
        }

        private void MinExit_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        #endregion
    }
}