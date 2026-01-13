using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CityBudget
{
    public partial class PageDecisions : Page
    {
        private double _currentBudget;
        private Action<double> _onBudgetChanged;
        private CityPopulationFunction _cityManager;

        private List<CityDecision> _easyDecisions = new List<CityDecision>();
        private List<CityDecision> _hardDecisions = new List<CityDecision>();

        public PageDecisions()
        {
            InitializeComponent();
        }
        public PageDecisions(double currentBudget, CityPopulationFunction cityManager, Action<double> onBudgetChanged)
        {
            InitializeComponent();
            _currentBudget = currentBudget;
            _cityManager = cityManager;
            _onBudgetChanged = onBudgetChanged;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateDecisions();
            RefreshLists();
        }

        private void GenerateDecisions()
        {
            _easyDecisions.Clear();
            _hardDecisions.Clear();

            _easyDecisions.Add(new CityDecision
            {
                Title = "Festyn Pieroga",
                Description = "Zorganizuj lokalny festyn. Ludzie to pokochają.",
                Cost = 5000,
                HappinessEffect = 3.0,
                IsHard = false
            });

            _easyDecisions.Add(new CityDecision
            {
                Title = "Nowe Ławki w Parku",
                Description = "Stare są połamane. Wymiana poprawi estetykę.",
                Cost = 2000,
                HappinessEffect = 1.5,
                IsHard = false
            });

            _easyDecisions.Add(new CityDecision
            {
                Title = "Mandaty za Parkowanie",
                Description = "Zaostrz kontrole. Zarobimy, ale kierowcy będą wściekli.",
                Cost = -8000,
                HappinessEffect = -2.0,
                IsHard = false
            });

            _easyDecisions.Add(new CityDecision
            {
                Title = "Promocja Miasta",
                Description = "Kup reklamy w internecie. Może ktoś przyjedzie.",
                Cost = 10000,
                HappinessEffect = 0.5,
                IsHard = false
            });


            _hardDecisions.Add(new CityDecision
            {
                Title = "Sprzedaż Parku Deweloperowi",
                Description = "Deweloper oferuje fortunę za teren parku miejskiego. Mieszkańcy stracą zieleń.",
                Cost = -200000,
                HappinessEffect = -15.0,
                IsHard = true
            });

            _hardDecisions.Add(new CityDecision
            {
                Title = "Budowa Aquaparku",
                Description = "Ogromna inwestycja. Mieszkańcy będą zachwyceni, ale budżet zapłacze.",
                Cost = 150000,
                HappinessEffect = 20.0,
                IsHard = true
            });

            _hardDecisions.Add(new CityDecision
            {
                Title = "Składowisko Odpadów",
                Description = "Przyjmij śmieci z sąsiedniego miasta za opłatą. Śmierdzi, ale płacą.",
                Cost = -50000,
                HappinessEffect = -8.0,
                IsHard = true
            });

            _hardDecisions.Add(new CityDecision
            {
                Title = "Remont Generalny Dróg",
                Description = "Zablokuje miasto na miesiąc, ale potem będzie cudownie.",
                Cost = 80000,
                HappinessEffect = 10.0,
                IsHard = true
            });
        }

        private void RefreshLists()
        {
            ListEasy.ItemsSource = null;
            ListEasy.ItemsSource = _easyDecisions;

            ListHard.ItemsSource = null;
            ListHard.ItemsSource = _hardDecisions;
        }

        private void ButtonDecision_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var decision = button.Tag as CityDecision;

            if (decision == null) return;

            if (decision.Cost > 0 && _currentBudget < decision.Cost)
            {
                MessageBox.Show("Nie stać Cię na tę inwestycję!", "Brak funduszy", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentBudget -= decision.Cost;
            _onBudgetChanged?.Invoke(_currentBudget);

            _cityManager.ApplyDirectHappinessChange(decision.HappinessEffect);

            if (decision.IsHard) _hardDecisions.Remove(decision);
            else _easyDecisions.Remove(decision);

            RefreshLists();

            string type = decision.Cost > 0 ? "Wydano" : "Zarobiono";
        }
    }
}