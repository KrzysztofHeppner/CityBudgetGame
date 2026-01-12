using System;
using System.Windows.Controls;

namespace CityBudget
{
    public partial class PageTax : Page
    {
        private Action<double> _onTaxChanged;

        public PageTax(double currentBudget, double currentTax, Action<double> onTaxChanged)
        {
            InitializeComponent();

            _onTaxChanged = onTaxChanged;

            BudgetDisplay.Text = $"{currentBudget:N2} PLN";
            TaxSlider.Value = currentTax;
            TaxValueText.Text = $"{currentTax * 100:F0}%";
        }

        public PageTax() { InitializeComponent(); }

        private void TaxSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (TaxValueText != null)
            {
                TaxValueText.Text = $"{e.NewValue * 100:F0}%";

                _onTaxChanged?.Invoke(e.NewValue);
            }
        }
    }
}