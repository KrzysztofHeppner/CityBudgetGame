using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace CityBudget
{
    /// <summary>
    /// Interaction logic for PageGraph.xaml
    /// </summary>
    public partial class PageGraph : Page
    {

        private IEnumerable<Person> citizens;

        public PageGraph(IEnumerable<Person> citizens)
        {
            InitializeComponent();
            this.citizens = citizens;
        }

        public PageGraph()
        {
            InitializeComponent();
        }

        private void BtnAge_Click(object sender, RoutedEventArgs e) => DrawPrettyGraph("wiek");
        private void BtnWealth_Click(object sender, RoutedEventArgs e) => DrawPrettyGraph("majatek");
        private void BtnHappiness_Click(object sender, RoutedEventArgs e) => DrawPrettyGraph("poziomZadowolenia");

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            GraphWindow.Visibility = Visibility.Hidden;
        }

        public void DrawPrettyGraph(string propertyName)
        {
            ChartCanvas.Children.Clear();
            AxisXCanvas.Children.Clear();
            AxisYCanvas.Children.Clear();

            FieldInfo field = typeof(Person).GetField(propertyName);
            if (field == null) return;

            var values = citizens.Select(p => Convert.ToDouble(field.GetValue(p))).OrderBy(x => x).ToList();
            if (!values.Any()) return;

            double min = values.First();
            double max = values.Last();

            bool isSmallInt = (max - min) < 120 && (field.FieldType == typeof(sbyte) || field.FieldType == typeof(byte));
            int bucketCount = isSmallInt ? (int)(max - min) + 1 : 40;
            double step = (max - min) / bucketCount;
            if (step == 0) step = 1;

            int[] buckets = new int[bucketCount];
            foreach (var val in values)
            {
                int idx = (int)((val - min) / step);
                if (idx >= bucketCount) idx = bucketCount - 1;
                buckets[idx]++;
            }

            int maxCount = buckets.Max();

            double w = ChartCanvas.ActualWidth;
            double h = ChartCanvas.ActualHeight;

            if (w == 0 || h == 0) { w = 800; h = 350; }

            int gridLinesCount = 5;
            for (int i = 0; i <= gridLinesCount; i++)
            {
                double yRatio = i / (double)gridLinesCount;
                double yPos = h - (yRatio * h);
                double labelValue = (maxCount * yRatio);

                Line gridLine = new Line
                {
                    X1 = 0,
                    Y1 = yPos,
                    X2 = w,
                    Y2 = yPos,
                    Stroke = i == 0 ? Brushes.Black : Brushes.LightGray,
                    StrokeThickness = i == 0 ? 2 : 1,
                    StrokeDashArray = i == 0 ? null : new DoubleCollection { 4, 2 }
                };
                ChartCanvas.Children.Add(gridLine);

                TextBlock label = new TextBlock
                {
                    Text = Math.Round(labelValue).ToString(),
                    FontSize = 11,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };

                AxisYCanvas.Children.Add(label);
                Canvas.SetRight(label, 5);
                Canvas.SetTop(label, yPos - 8);
            }

            double barWidth = (w / bucketCount);
            double gap = Math.Max(1, barWidth * 0.1);
            double effectiveBarWidth = barWidth - gap;

            LinearGradientBrush barBrush = new LinearGradientBrush();
            barBrush.StartPoint = new Point(0, 1);
            barBrush.EndPoint = new Point(0, 0);
            barBrush.GradientStops.Add(new GradientStop(Color.FromRgb(66, 165, 245), 0.0));
            barBrush.GradientStops.Add(new GradientStop(Color.FromRgb(21, 101, 192), 1.0));

            for (int i = 0; i < bucketCount; i++)
            {
                int count = buckets[i];
                if (count == 0) continue;

                double barHeight = (count / (double)maxCount) * h;

                Rectangle rect = new Rectangle
                {
                    Width = Math.Max(1, effectiveBarWidth),
                    Height = barHeight,
                    Fill = barBrush,
                    RadiusX = 2,
                    RadiusY = 2,
                    ToolTip = $"Wartość: {Math.Round(min + i * step):N0}\nIlość: {count}"
                };

                Canvas.SetLeft(rect, (i * barWidth) + (gap / 2));
                Canvas.SetTop(rect, h - barHeight);

                ChartCanvas.Children.Add(rect);

                int stepSkip = (int)(50 / barWidth) + 1;

                if (i % stepSkip == 0 || i == bucketCount - 1)
                {
                    double val = min + (i * step);
                    string txtVal = val >= 1000 ? (val / 1000.0).ToString("0.k") : val.ToString("0");

                    TextBlock xLabel = new TextBlock
                    {
                        Text = txtVal,
                        FontSize = 10,
                        Foreground = Brushes.DimGray
                    };

                    AxisXCanvas.Children.Add(xLabel);
                    Canvas.SetLeft(xLabel, (i * barWidth));
                    Canvas.SetTop(xLabel, 5);
                }
            }
        }

    }
}
