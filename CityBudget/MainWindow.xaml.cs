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
using System.IO;
using System.Text.Json;
using Microsoft.Win32;

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
        PageDecisions pageDecisions = new PageDecisions();

        private CityPopulationFunction _cityManager;
        private double _cityBudget = 100000;
        private double _taxRate = 0.15;
        private TaxSettings _currentTaxSettings = new TaxSettings();
        private BudgetPolicy _currentBudgetPolicy = new BudgetPolicy();
        private List<CityDecision> _allDecisions = new List<CityDecision>();

        public MainWindow()
        {
            InitializeComponent();
            _cityManager = new CityPopulationFunction();
            _cityManager.MakeNewPopulation(10000);
            InitializeDecisions();

            _timer = new Timer(MainTimerTick, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(5));
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
            _cityManager.SimulateMonth(_currentBudgetPolicy);
            FinanceReport report = _cityManager.CalculateFinances(_currentTaxSettings, _currentBudgetPolicy);
            _cityBudget += report.Balance;

            _cityManager.UpdateHappiness(_currentTaxSettings, _currentBudgetPolicy);
            
            string migrationStatus = _cityManager.HandleMigration();

            Dispatcher.Invoke(() =>
            {
                if (MainFrame.Content == pageTax && !pageTax.isChanged)
                {
                    ButtonYellow_Click(null, null);
                }
                if (MainFrame.Content == pageGraph)
                {
                    ButtonBlue_Click(null, null);
                }
                if (MainFrame.Content == pageDecisions)
                {
                    //ButtonRed_Click(null, null);
                }
            });
        }

        private void MainNewYear()
        {
            //_cityManager.SimulateYear();
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

                    double avgHappy = _cityManager.GetAverageHappiness();
                    HappinessLabel.Content = $"Zadowolenie: {avgHappy:F0}%";

                    if (avgHappy < 40) HappinessLabel.Foreground = Brushes.Red;
                    else if (avgHappy > 70) HappinessLabel.Foreground = Brushes.LightGreen;
                    else HappinessLabel.Foreground = Brushes.White;

                    PeopleLabel.Content = $"Ludność: {_cityManager.PopulationCount}";
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
            pageTax = new PageTax(_cityBudget, _currentTaxSettings, _currentBudgetPolicy, _cityManager,
            (newTaxes, newPolicy) =>
            {
                _currentTaxSettings = newTaxes;
                _currentBudgetPolicy = newPolicy;
            });

            MainFrame.Navigate(pageTax);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveGame();
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadGame();
        }

        private void SaveGame()
        {
            bool wasRunning = isRunning;
            isRunning = false;

            try
            {
                var state = new GameState
                {
                    Budget = _cityBudget,
                    CurrentDate = currentDate,
                    Taxes = _currentTaxSettings,
                    Policy = _currentBudgetPolicy,
                    DecisionsHistory = _allDecisions
                        .Where(d => d.LastUsedDate.HasValue)
                        .ToDictionary(d => d.Id, d => d.LastUsedDate.Value),
                    Population = _cityManager.GetPopulationSnapshot()
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(state, options);

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "City Save File (*.json)|*.json";
                saveDialog.FileName = $"CitySave_{currentDate:yyyy-MM-dd}";

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, jsonString);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd zapisu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isRunning = wasRunning;
                UpdateUI();
            }
        }

        public void LoadGame()
        {
            bool wasRunning = isRunning;
            isRunning = false;

            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "City Save File (*.json)|*.json";

                if (openDialog.ShowDialog() == true)
                {
                    string jsonString = File.ReadAllText(openDialog.FileName);

                    GameState? state = JsonSerializer.Deserialize<GameState>(jsonString);

                    if (state != null)
                    {
                        _cityBudget = state.Budget;
                        currentDate = state.CurrentDate;
                        _currentTaxSettings = state.Taxes ?? new TaxSettings();
                        _currentBudgetPolicy = state.Policy ?? new BudgetPolicy();

                        if (state.Population != null)
                        {
                            _cityManager.LoadPopulation(state.Population);
                        }

                        if (state.DecisionsHistory != null)
                        {
                            foreach (var d in _allDecisions) d.LastUsedDate = null;

                            foreach (var kvp in state.DecisionsHistory)
                            {
                                var decision = _allDecisions.FirstOrDefault(d => d.Id == kvp.Key);
                                if (decision != null)
                                {
                                    decision.LastUsedDate = kvp.Value;
                                }
                            }
                        }

                        UpdateUI();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nie udało się wczytać zapisu: {ex.Message}\nPlik może być uszkodzony.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isRunning = false;
                UpdateUI();
            }
        }

        private void ButtonRed_Click(object? sender, RoutedEventArgs? e)
        {
            var activeDecisions = GetAvailableDecisions();
            pageDecisions = new PageDecisions(activeDecisions, _cityBudget, _cityManager, (newBudget) =>
            {
                _cityBudget = newBudget;
                UpdateUI();
            });
            pageDecisions.CurrentGameDate = currentDate;
            MainFrame.Navigate(pageDecisions);
        }

        private void ButtonGreen_Click(object? sender, RoutedEventArgs? e)
        {
            pageDecisions.Visibility = Visibility.Hidden;
            pageGraph.Visibility = Visibility.Hidden;
            pageInfo.Visibility = Visibility.Hidden;
            pageTax.Visibility = Visibility.Hidden;
            MainFrame.Content = null;
        }

        private void InitializeDecisions()
        {
            _allDecisions.Clear();

            _allDecisions.Add(new CityDecision
            {
                Id = "social_training",
                Frequency = DecisionFrequency.Yearly,
                Title = "Szkolenia Aktywizujące",
                Description = "Kursy zawodowe dla osób bez pracy. 10% z nich znajdzie zatrudnienie.",

                CalculateCost = (p) => p.Count(x => !x.IsEmployed && x.Age >= 18 && x.Age < 65) * 500.0,

                HappinessEffect = 15.0,
                IsHard = true,

                TargetFilter = p => !p.IsEmployed && p.Age >= 18 && p.Age < 65,
                TargetDescription = "Bezrobotni",

                InstantEffect = (population) =>
                {
                    var rnd = new Random();

                    var unemployed = population
                        .Where(p => !p.IsEmployed && p.Age >= 18 && p.Age < 65)
                        .ToList();

                    int peopleToHire = (int)(unemployed.Count * 0.10);

                    for (int i = 0; i < peopleToHire; i++)
                    {
                        var person = unemployed[i];

                        person.IsEmployed = true;

                        person.Income = rnd.Next(5000, 7000);

                        person.Happiness = 100;
                    }
                }
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "build_aquapark",
                Frequency = DecisionFrequency.OneTime,
                Title = "Budowa Aquaparku",
                Description = "Wielka inwestycja, która zostanie z nami na zawsze.",
                CalculateCost = (p) => 1000000 + (p.Count * 500),
                HappinessEffect = 25.0,
                IsHard = true,
                TargetDescription = "Wszyscy"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "yearly_fest",
                Frequency = DecisionFrequency.Yearly,
                Title = "Dni Miasta (Festyn)",
                Description = "Coroczna impreza integrująca mieszkańców.",
                CalculateCost = (p) => 50000 + (p.Count * 10),
                HappinessEffect = 5.0,
                IsHard = false,
                TargetDescription = "Wszyscy"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "monthly_fines",
                Frequency = DecisionFrequency.Monthly,
                Title = "Nagonka Mandatowa",
                Description = "Wyślij straż miejską by łatać budżet mandatami.",
                CalculateCost = (p) => -(p.Count(x => x.IsEmployed) * 50.0),
                HappinessEffect = -3.0,
                IsHard = false,
                TargetFilter = p => p.IsEmployed,
                TargetDescription = "Pracujący"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "monthly_bonus",
                Frequency = DecisionFrequency.Monthly,
                Title = "Premie dla Urzędników",
                Description = "Popraw sprawność urzędu.",
                CalculateCost = (p) => 100000,
                HappinessEffect = 1.0,
                IsHard = false,
                TargetDescription = "Urzędnicy"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "infra_bikes",
                Frequency = DecisionFrequency.OneTime,
                Title = "Sieć Rowerów Miejskich",
                Description = "Stacje z rowerami co 500 metrów. Eko i zdrowo.",
                CalculateCost = (p) => 250000 + (p.Count * 25.0),
                HappinessEffect = 4.0,
                IsHard = false,
                TargetDescription = "Wszyscy"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "infra_wifi",
                Frequency = DecisionFrequency.OneTime,
                Title = "Darmowe Wi-Fi w Centrum",
                Description = "Hotspoty na rynku i w parkach. Młodzież doceni.",
                CalculateCost = (p) => 10000 + (p.Count * 10.0),
                HappinessEffect = 3.0,
                IsHard = false,
                TargetFilter = p => p.Age < 30,
                TargetDescription = "Młodzież (<30 lat)"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "infra_cctv",
                Frequency = DecisionFrequency.OneTime,
                Title = "Monitoring Miejski (CCTV)",
                Description = "Kamery na każdym rogu. Bezpieczniej, ale mniej prywatnie.",
                CalculateCost = (p) => 150000 + (p.Count * 5.0),
                HappinessEffect = 2.0,
                IsHard = false,
                TargetDescription = "Wszyscy"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "infra_trees",
                Frequency = DecisionFrequency.Monthly,
                Title = "Sadzenie Tysiąca Drzew",
                Description = "Zazieleńmy betonową dżunglę.",
                CalculateCost = (p) => 50000,
                HappinessEffect = 2,
                IsHard = false,
                TargetDescription = "Wszyscy"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "event_fireworks",
                Frequency = DecisionFrequency.Yearly,
                Title = "Sylwester z Gwiazdami",
                Description = "Wielki pokaz fajerwerków i koncert w TV.",
                CalculateCost = (p) => 500000 + (p.Count * 5.0),
                HappinessEffect = 8.0,
                IsHard = true,
                TargetDescription = "Wszyscy"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "event_marathon",
                Frequency = DecisionFrequency.Yearly,
                Title = "Maraton Miejski",
                Description = "Zablokujemy pół miasta, ale jest prestiż.",
                CalculateCost = (p) => 8000,
                HappinessEffect = -2.0,
                TargetFilter = p => p.IsEmployed,
                TargetDescription = "Kierowcy (Pracujący)"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "event_womensday",
                Frequency = DecisionFrequency.Yearly,
                Title = "Tulipany na Dzień Kobiet",
                Description = "Symboliczny kwiatek dla każdej mieszkanki.",
                CalculateCost = (p) => p.Count(x => x.Gender == Gender.Female) * 5.0,
                HappinessEffect = 5.0,
                IsHard = false,
                TargetFilter = p => p.Gender == Gender.Female,
                TargetDescription = "Kobiety"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "event_mensday",
                Frequency = DecisionFrequency.Yearly,
                Title = "Piknik Militarny",
                Description = "Czołgi i grochówka z okazji dnia mężczyzny.",
                CalculateCost = (p) => 120000,
                HappinessEffect = 5.0,
                IsHard = false,
                TargetFilter = p => p.Gender == Gender.Male,
                TargetDescription = "Mężczyźni"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "event_vaccine",
                Frequency = DecisionFrequency.Yearly,
                Title = "Szczepienia na Grypę",
                Description = "Darmowa akcja dla seniorów przed sezonem.",
                CalculateCost = (p) => p.Count(x => x.Age >= 60) * 40.0,
                HappinessEffect = 6.0,
                IsHard = false,
                TargetFilter = p => p.Age >= 60,
                TargetDescription = "Seniorzy (60+)"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "tax_parking",
                Frequency = DecisionFrequency.Monthly,
                Title = "Rozszerzona Strefa Parkowania",
                Description = "Płatne parkowanie na osiedlach.",
                CalculateCost = (p) => -(p.Count(x => x.Age >= 18) * 15.0),
                HappinessEffect = -4.0,
                IsHard = false,
                TargetFilter = p => p.Age >= 18,
                TargetDescription = "Dorośli"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "tax_tourist",
                Frequency = DecisionFrequency.Monthly,
                Title = "Opłata Klimatyczna",
                Description = "Dodatkowa taksa doliczana do usług. Mieszkańcy też to odczują.",
                CalculateCost = (p) => -(p.Count * 20.0),
                HappinessEffect = -1.0,
                IsHard = false,
                TargetDescription = "Wszyscy"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "tax_ads",
                Frequency = DecisionFrequency.Monthly,
                Title = "Reklamy Wielkoformatowe",
                Description = "Pozwól obkleić centrum reklamami. Brzydko, ale płacą.",
                CalculateCost = (p) => -150000,
                HappinessEffect = -2.0,
                IsHard = false,
                TargetDescription = "Wszyscy (Estetyka)"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "social_library",
                Frequency = DecisionFrequency.OneTime,
                Title = "Nowe Książki do Biblioteki",
                Description = "Zakup nowości wydawniczych i komiksów.",
                CalculateCost = (p) => 3000,
                HappinessEffect = 3.0,
                IsHard = false,
                TargetFilter = p => p.Age < 25 || p.Age > 60,
                TargetDescription = "Młodzież i Seniorzy"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "social_kindergarten",
                Frequency = DecisionFrequency.OneTime,
                Title = "Remont Przedszkoli",
                Description = "Kolorowe ściany i nowe zabawki.",
                CalculateCost = (p) => 20000 + (p.Count(x => x.Age < 7) * 100.0),
                HappinessEffect = 10.0,
                IsHard = false,
                TargetFilter = p => p.Age >= 20 && p.Age <= 40,
                TargetDescription = "Rodzice (20-40 lat)"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "social_students",
                Frequency = DecisionFrequency.Yearly,
                Title = "Juwenalia (Dni Studenta)",
                Description = "Dofinansowanie imprezy studenckiej.",
                CalculateCost = (p) => 50000,
                HappinessEffect = 12.0,
                IsHard = false,
                TargetFilter = p => p.Age >= 19 && p.Age <= 24,
                TargetDescription = "Studenci"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "hard_factory",
                Frequency = DecisionFrequency.OneTime,
                Title = "Fabryka Chemiczna",
                Description = "Inwestor chce zbudować fabrykę. Śmierdzi, ale zapłaci fortunę za grunt.",
                CalculateCost = (p) => -300000,
                HappinessEffect = -25.0,
                IsHard = true,
                TargetDescription = "Wszyscy (Skażenie)"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "hard_school_close",
                Frequency = DecisionFrequency.OneTime,
                Title = "Likwidacja Małych Szkół",
                Description = "Dzieci będą dojeżdżać do molochów. Oszczędność kosztów.",
                CalculateCost = (p) => -500000,
                HappinessEffect = -20.0,
                IsHard = true,
                TargetFilter = p => p.Age < 18,
                TargetDescription = "Dzieci i Rodzice"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "hard_bridge",
                Frequency = DecisionFrequency.OneTime,
                Title = "Most",
                Description = "Strategiczna inwestycja, która rozładuje korki na zawsze.",
                CalculateCost = (p) => 2500000,
                HappinessEffect = 30.0,
                IsHard = true,
                TargetFilter = p => p.IsEmployed,
                TargetDescription = "Pracujący (Dojazdy)"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "hard_monument",
                Frequency = DecisionFrequency.OneTime,
                Title = "Pomnik Burmistrza",
                Description = "Postaw sobie pomnik z brązu na rynku.",
                CalculateCost = (p) => 50000,
                HappinessEffect = -5.0,
                IsHard = true,
                TargetDescription = "Wszyscy (Żenada)"
            });

        }

        private List<CityDecision> GetAvailableDecisions()
        {
            var available = new List<CityDecision>();

            foreach (var d in _allDecisions)
            {
                var pop = _cityManager.GetPopulationSnapshot();
                d.CurrentCost = d.CalculateCost(pop);

                if (d.LastUsedDate == null)
                {
                    available.Add(d);
                }
                else
                {
                    DateTime last = d.LastUsedDate.Value;

                    switch (d.Frequency)
                    {
                        case DecisionFrequency.OneTime:
                            break;

                        case DecisionFrequency.Monthly:
                            if (currentDate.Year > last.Year || (currentDate.Year == last.Year && currentDate.Month > last.Month))
                            {
                                available.Add(d);
                            }
                            break;

                        case DecisionFrequency.Yearly:
                            if (currentDate.Year > last.Year)
                            {
                                available.Add(d);
                            }
                            break;
                    }
                }
            }
            return available;
        }
    }
}