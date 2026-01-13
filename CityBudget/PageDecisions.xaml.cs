using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CityBudget
{
    public partial class PageDecisions : Page
    {
        private List<CityDecision> _decisionsList;
        private double _currentBudget;
        private Action<double> _onBudgetChanged;
        private CityPopulationFunction _cityManager;

        public DateTime CurrentGameDate { get; set; }

        public PageDecisions()
        {
            InitializeComponent();
        }
        public PageDecisions(List<CityDecision> decisions, double currentBudget, CityPopulationFunction cityManager, Action<double> onBudgetChanged)
        {
            InitializeComponent();
            _decisionsList = decisions;
            _currentBudget = currentBudget;
            _cityManager = cityManager;
            _onBudgetChanged = onBudgetChanged;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshLists();
        }

        private void RefreshLists()
        {
            ListEasy.ItemsSource = null;
            ListEasy.ItemsSource = _decisionsList.Where(d => !d.IsHard).ToList();

            ListHard.ItemsSource = null;
            ListHard.ItemsSource = _decisionsList.Where(d => d.IsHard).ToList();
        }

        private void ButtonDecision_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var decision = button.Tag as CityDecision;
            if (decision == null) return;

            if (decision.CurrentCost > 0 && _currentBudget < decision.CurrentCost)
            {
                //MessageBox.Show("Brak środków!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentBudget -= decision.CurrentCost;
            _onBudgetChanged?.Invoke(_currentBudget);

            int affected = _cityManager.ApplyDecisionEffect(decision);

            _cityManager.ExecuteCustomEffect(decision.InstantEffect);

            decision.LastUsedDate = CurrentGameDate;

            _decisionsList.Remove(decision);
            RefreshLists();

            string type = decision.CurrentCost > 0 ? "Wydano" : "Zarobiono";
            //MessageBox.Show($"{type}: {Math.Abs(decision.CurrentCost):N0} PLN\nDotyczyło: {affected} osób.\nDecyzja: {decision.FrequencyText}", "Sukces");
        }
    }
}