using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
            Dispatcher.Invoke(() =>
            {
                TimeText.Content = $"{currentDate.Day}.{currentDate.Month}.{currentDate.Year}";
                TextBlock.Text = isRunning ? "Running" : "Paused";
            });
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
    }
}