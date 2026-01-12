using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CityBudget
{
    public partial class PageTax : Page
    {
        private TaxSettings _settings;
        private Action<TaxSettings> _onSettingsChanged;

        private CityPopulationFunction _cityManagerForPreview;

        private bool _isInitializing = true;

        public PageTax()
        {
            InitializeComponent();
        }

        public PageTax(double currentBudget, TaxSettings settings, CityPopulationFunction manager, Action<TaxSettings> onSettingsChanged)
        {
            InitializeComponent();
            _settings = settings;
            _cityManagerForPreview = manager;
            _onSettingsChanged = onSettingsChanged;

            _isInitializing = true;

            BudgetDisplay.Text = $"{currentBudget:N0} PLN";

            SliderPIT.Value = _settings.PIT;
            SliderVAT.Value = _settings.VAT;
            SliderProp.Value = _settings.PropertyTax;

            UpdateLabels();
            UpdatePreview();

            _isInitializing = false;
        }

        private void Sliders_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isInitializing) return;

            if (_settings == null) return;

            _settings.PIT = SliderPIT.Value;
            _settings.VAT = SliderVAT.Value;
            _settings.PropertyTax = SliderProp.Value;

            UpdateLabels();

            _onSettingsChanged?.Invoke(_settings);

            UpdatePreview();
        }

        private void UpdateLabels()
        {
            if (TextPIT != null) TextPIT.Text = $"{_settings.PIT * 100:F0}%";
            if (TextVAT != null) TextVAT.Text = $"{_settings.VAT * 100:F0}%";
            if (TextProp != null) TextProp.Text = $"{_settings.PropertyTax:N0} PLN";
        }

        private void UpdatePreview()
        {
            if (_cityManagerForPreview == null) return;

            var report = _cityManagerForPreview.CalculateFinances(_settings);

            ValEdu.Text = $"-{report.ExpenseEducation:N0} PLN";
            ValHealth.Text = $"-{report.ExpenseHealthcare:N0} PLN";
            ValSec.Text = $"-{report.ExpenseSecurity:N0} PLN";
            ValInfra.Text = $"-{report.ExpenseInfrastructure:N0} PLN";
            ValAdmin.Text = $"-{report.ExpenseAdministration:N0} PLN";

            ValTotalExp.Text = $"-{report.TotalExpenses:N0} PLN";
            ValTotalInc.Text = $"+{report.TotalIncome:N0} PLN";

            double balance = report.Balance;
            ValBalance.Text = $"{balance:N0} PLN";
            ValBalance.Foreground = balance >= 0 ? Brushes.LimeGreen : Brushes.Red;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }
    }
}