using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CityBudget
{
    /// <summary>
    /// Interaction logic for PageDecisions.xaml
    /// </summary>
    public partial class PageDecisions : Page
    {
        private List<CityDecision> _decisionsList;
        private double _currentBudget;
        private Action<double> _onBudgetChanged;
        private CityPopulationFunction _cityManager;
        private Action<string, bool> _onNewsAdded;

        public DateTime CurrentGameDate { get; set; }

        public PageDecisions()
        {
            InitializeComponent();
        }
        public PageDecisions(List<CityDecision> decisions, double currentBudget, CityPopulationFunction cityManager, Action<double> onBudgetChanged, Action<string, bool> onNewsAdded)
        {
            InitializeComponent();
            _decisionsList = decisions;
            _currentBudget = currentBudget;
            _cityManager = cityManager;
            _onBudgetChanged = onBudgetChanged;
            _onNewsAdded = onNewsAdded;
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
                return;
            }

            _currentBudget -= decision.CurrentCost;
            _onBudgetChanged?.Invoke(_currentBudget);

            int affected = _cityManager.ApplyDecisionEffect(decision);

            _cityManager.ExecuteCustomEffect(decision.InstantEffect);

            decision.LastUsedDate = CurrentGameDate;

            _decisionsList.Remove(decision);
            RefreshLists();

            bool isGood = decision.HappinessEffect >= 0;

            string message = $"Podjęto: {decision.Title}.\n";
            if (decision.CurrentCost > 0) message += $"Wydano: {decision.CurrentCost:N0} PLN.";
            else message += $"Zarobiono: {Math.Abs(decision.CurrentCost):N0} PLN.";

            _onNewsAdded?.Invoke(message, isGood);
        }
    }
    /// <summary>
    /// Przedstawia pojedynczą wiadomość w systemie wiadomości gry.
    /// </summary>
    public class NewsItem
    {
        public string Text { get; set; }
        public string BorderColor { get; set; }
        public string Time { get; set; }
    }
}