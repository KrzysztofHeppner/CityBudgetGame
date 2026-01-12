using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CityBudget
{
    public partial class MainWindow : Window
    {
        private Timer? _timer;
        DateTime currentDate = new(2000, 1, 1);
        bool isRunning = false;
        bool canClose = true; bool wantClose = false;


        PageInfo pageInfo = new PageInfo();
        PageTax pageTax = new PageTax();
        PageGraph pageGraph = new PageGraph();

        private CityPopulationFunction _cityManager;
        private double _cityBudget = 100000;
        private double _taxRate = 0.15;
        private TaxSettings _currentTaxSettings = new TaxSettings();

        public MainWindow()
        {
            InitializeComponent();
            _cityManager = new CityPopulationFunction();
            _cityManager.MakeNewPopulation(10000);
            

            _timer = new Timer(MainTimerTick, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(2));
            MainFrame.Visibility = Visibility.Visible;
            UpdateUI();
        }

        private void MainTimerTick(object? state)
        {
            if (isRunning)
            {
                currentDate = currentDate.AddHours(6);
            }

            if (currentDate.Hour == 0 && isRunning)
            {
                MainNewDay();
                if (currentDate.Day == 1)
                {
                    MainNewMonth();
                    if (currentDate.Month == 1)
                    {
                        MainNewYear();
                    }
                }
            }
        }

        private void MainNewDay()
        {
            UpdateUI();
        }

        private void MainNewMonth()
        {
            FinanceReport report = _cityManager.CalculateFinances(_currentTaxSettings);

            _cityBudget += report.Balance;

            Dispatcher.Invoke(() =>
            {
                if (MainFrame.Content == pageTax)
                {
                    ButtonYellow_Click(null, null);
                }
            });
        }

        private void MainNewYear()
        {
            _cityManager.SimulateYear();

            _cityManager.UpdateHappiness(_taxRate);
            
            Dispatcher.Invoke(() =>
            {
                if (MainFrame.Content == pageGraph)
                {
                    ButtonBlue_Click(null, null);
                }
            });
            
        }

        private void UpdateUI()
        {
            if (!wantClose)
            {
                canClose = false;
                Dispatcher.Invoke(() =>
                {
                    TimeText.Content = $"{currentDate.Day:00}.{currentDate.Month:00}.{currentDate.Year}";
                    pageInfo.textBlockInfo.Text = isRunning ? "Symulacja działa" : "Pauza";

                });
                canClose = true;
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            _timer?.Dispose();
            _timer = null;
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
            wantClose = true;
            if (canClose)
                App.Current.Shutdown();
        }

        private void MinExit_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }



        #endregion

        private void ButtonBlue_Click(object? sender, RoutedEventArgs? e)
        {
            var populationSnapshot = _cityManager.GetPopulationSnapshot();

            pageGraph = new PageGraph(populationSnapshot);
            MainFrame.Navigate(pageGraph);
        }

        private void ButtonYellow_Click(object? sender, RoutedEventArgs? e)
        {
            pageTax = new PageTax(_cityBudget, _currentTaxSettings, _cityManager, (newSettings) =>
            {
                _currentTaxSettings = newSettings;
            });

            MainFrame.Navigate(pageTax);
            //MainFrame.Navigate(pageInfo);
        }
    }
}