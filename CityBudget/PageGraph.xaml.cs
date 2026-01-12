using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CityBudget
{
    public partial class PageGraph : Page
    {
        private List<Person> _population;

        public PageGraph() { InitializeComponent(); }
        public PageGraph(List<Person> population)
        {
            InitializeComponent();
            _population = population;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DrawAgeChart();
        }

        private void DrawAgeChart()
        {
            GraphCanvas.Children.Clear();
            if (_population == null || _population.Count == 0) return;

            int[] ageGroups = new int[10];
            foreach (var p in _population)
            {
                int groupIndex = p.Age / 10;
                if (groupIndex >= ageGroups.Length) groupIndex = ageGroups.Length - 1;
                ageGroups[groupIndex]++;
            }

            double canvasWidth = GraphCanvas.ActualWidth;
            double canvasHeight = GraphCanvas.ActualHeight;
            double barWidth = (canvasWidth / ageGroups.Length) - 5;
            int maxCount = ageGroups.Max();
            if (maxCount == 0) maxCount = 1;

            for (int i = 0; i < ageGroups.Length; i++)
            {
                double barHeight = (double)ageGroups[i] / maxCount * canvasHeight;

                Rectangle rect = new Rectangle
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = new SolidColorBrush(Color.FromRgb(100, 149, 237)),
                    ToolTip = $"Wiek {i * 10}-{(i * 10) + 9}: {ageGroups[i]} osób"
                };

                Canvas.SetLeft(rect, i * (canvasWidth / ageGroups.Length));
                Canvas.SetTop(rect, canvasHeight - barHeight);

                GraphCanvas.Children.Add(rect);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            DrawAgeChart();
        }
    }
}