using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CityBudget
{
    public partial class PageTax : Page
    {
        private TaxSettings _taxes;
        private BudgetPolicy _policy;

        private Action<TaxSettings, BudgetPolicy> _onSettingsChanged;

        private CityPopulationFunction _cityManagerForPreview;
        private bool _isInitializing = true;

        public PageTax()
        {
            InitializeComponent();
        }
        public PageTax(double currentBudget, TaxSettings taxes, BudgetPolicy policy, CityPopulationFunction manager, Action<TaxSettings, BudgetPolicy> onSettingsChanged)
        {
            InitializeComponent();
            _taxes = taxes;
            _policy = policy;
            _cityManagerForPreview = manager;
            _onSettingsChanged = onSettingsChanged;

            _isInitializing = true;

            BudgetDisplay.Text = $"{currentBudget:N0} PLN";

            SliderPIT.Value = _taxes.PIT;
            SliderVAT.Value = _taxes.VAT;
            SliderProp.Value = _taxes.PropertyTax;

            Sl_Edu.Value = _policy.Education;
            Sl_Health.Value = _policy.Healthcare;
            Sl_Police.Value = _policy.Police;
            Sl_Fire.Value = _policy.FireDept;
            Sl_Roads.Value = _policy.Roads;
            Sl_Admin.Value = _policy.Administration;

            UpdateLabels();
            UpdatePreview();

            _isInitializing = false;
        }

        private void Sliders_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isInitializing) return;
            if (_taxes == null || _policy == null) return;

            _taxes.PIT = SliderPIT.Value;
            _taxes.VAT = SliderVAT.Value;
            _taxes.PropertyTax = SliderProp.Value;

            _policy.Education = Sl_Edu.Value;
            _policy.Healthcare = Sl_Health.Value;
            _policy.Police = Sl_Police.Value;
            _policy.FireDept = Sl_Fire.Value;
            _policy.Roads = Sl_Roads.Value;
            _policy.Administration = Sl_Admin.Value;

            UpdateLabels();

            _onSettingsChanged?.Invoke(_taxes, _policy);

            UpdatePreview();
        }

        private void UpdateLabels()
        {
            if (TextPIT != null) TextPIT.Text = $"{_taxes.PIT * 100:F0}%";
            if (TextVAT != null) TextVAT.Text = $"{_taxes.VAT * 100:F0}%";
            if (TextProp != null) TextProp.Text = $"{_taxes.PropertyTax:N0} PLN";

            if (Tx_Edu != null) Tx_Edu.Text = $"{_policy.Education * 100:F0}%";
            if (Tx_Health != null) Tx_Health.Text = $"{_policy.Healthcare * 100:F0}%";
            if (Tx_Police != null) Tx_Police.Text = $"{_policy.Police * 100:F0}%";
            if (Tx_Fire != null) Tx_Fire.Text = $"{_policy.FireDept * 100:F0}%";
            if (Tx_Roads != null) Tx_Roads.Text = $"{_policy.Roads * 100:F0}%";
            if (Tx_Admin != null) Tx_Admin.Text = $"{_policy.Administration * 100:F0}%";
        }

        private void UpdatePreview()
        {
            if (_cityManagerForPreview == null) return;

            var report = _cityManagerForPreview.CalculateFinances(_taxes, _policy);

            if (ValTotalExp != null) ValTotalExp.Text = $"-{report.TotalExpenses:N0} PLN";
            if (ValTotalInc != null) ValTotalInc.Text = $"+{report.TotalIncome:N0} PLN";

            if (ValBalance != null)
            {
                double balance = report.Balance;
                ValBalance.Text = $"{balance:N0} PLN";
                ValBalance.Foreground = balance >= 0 ? Brushes.LimeGreen : Brushes.Red;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }
    }
}