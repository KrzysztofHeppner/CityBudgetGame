using System;
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
using System.Windows.Shapes;

namespace CityBudget
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void ButtonNewGame_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new();
            this.Hide();
            mainWindow.Show();
            Application.Current.MainWindow = mainWindow;
            this.Close();
        }
        private void ButtonLoadGame_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void ButtonLeave_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void ButtonNewGame_MouseEnter(object sender, MouseEventArgs e)
        {
            BorderNewGame.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x88, 0x88, 0x88));
            ButtonNewGame.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x1e, 0x1e, 0x1e));
            Cursor = Cursors.Hand;
        }

        private void ButtonNewGame_MouseLeave(object sender, MouseEventArgs e)
        {
            BorderNewGame.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x1e, 0x1e, 0x1e));
            ButtonNewGame.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x88, 0x88, 0x88));
            Cursor = Cursors.Arrow;
        }
        private void ButtonLoadGame_MouseEnter(object sender, MouseEventArgs e)
        {
            BorderLoadGame.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x88, 0x88, 0x88));
            ButtonLoadGame.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x1e, 0x1e, 0x1e));
            Cursor = Cursors.Hand;
        }

        private void ButtonLoadGame_MouseLeave(object sender, MouseEventArgs e)
        {
            BorderLoadGame.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x1e, 0x1e, 0x1e));
            ButtonLoadGame.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x88, 0x88, 0x88));
            Cursor = Cursors.Arrow;
        }
        private void ButtonLeave_MouseEnter(object sender, MouseEventArgs e)
        {
            BorderLeave.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x88, 0x88, 0x88));
            ButtonLeave.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x1e, 0x1e, 0x1e));
            Cursor = Cursors.Hand;
        }

        private void ButtonLeave_MouseLeave(object sender, MouseEventArgs e)
        {
            BorderLeave.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x1e, 0x1e, 0x1e));
            ButtonLeave.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x88, 0x88, 0x88));
            Cursor = Cursors.Arrow;
        }
    }
}
