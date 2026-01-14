using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CityBudget
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer? _timer;
        DateTime currentDate = new(2000, 1, 1);
        bool isRunning = false;
        bool canClose = true; bool wantClose = false;
        private int _monthsUnder20Percent = 0;
        private int _monthsUnder50Percent = 0;
        private int _currentSpeedLevel = 3;
        private string _currentWallpaperName = "";

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

        public ObservableCollection<NewsItem> RecentNews { get; set; } = new ObservableCollection<NewsItem>();

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            _cityManager = new CityPopulationFunction();
            _cityManager.MakeNewPopulation(10000);
            InitializeDecisions();

            _timer = new Timer(MainTimerTick, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            MainFrame.Visibility = Visibility.Visible;
            AddNews("Witaj w symulatorze miasta! Rozpocznij zarządzanie.", false, null);
            SetGameSpeed(3);
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
            if (migrationStatus.Contains("Kryzys")) AddNews(migrationStatus, false, null);
            else if (migrationStatus.Contains("Rozwój")) AddNews(migrationStatus, true, null);

            if (_cityBudget < 0)
            {
                double interestRate = 0.05*(1/12.0);
                double interestCost = Math.Abs(_cityBudget) * interestRate;

                _cityBudget -= interestCost;
                
                if (interestCost > 0)
                {
                    _cityManager.ApplyDirectHappinessChange(-3);
                    AddNews($"Karne odsetki: -{interestCost:N0} PLN.", false, null);
                    AddNews("Brak środków w kasie obniża zadowolenie mieszkańców!", false, null);
                }
            }
            CheckWinLossConditions();
            UpdateWallpaper();
            canClose = false;
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
            canClose = true;
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

                    if (BudgetLabel != null)
                    {
                        BudgetLabel.Content = $"Budżet: {_cityBudget:N0} PLN";

                        if (_cityBudget < 0)
                        {
                            BudgetLabel.Foreground = System.Windows.Media.Brushes.Red;
                            BudgetLabel.FontWeight = FontWeights.Bold;
                        }
                        else
                        {
                            BudgetLabel.Foreground = System.Windows.Media.Brushes.LightGreen;
                            BudgetLabel.FontWeight = FontWeights.Normal;
                        }
                    }
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
            {
                Dispatcher.Invoke(() =>
                {
                    App.Current.Shutdown();
                });
                
            }
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
                    monthsUnder20Percent = _monthsUnder20Percent,
                    monthsUnder50Percent = _monthsUnder50Percent,
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
                        _monthsUnder20Percent = state.monthsUnder20Percent;
                        _monthsUnder50Percent = state.monthsUnder50Percent;

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
                        _currentWallpaperName = "";
                        UpdateWallpaper();
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
                },
                (message, isGood) => AddNews(message, isGood, null)
            );
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

                        person.Income = rnd.Next(5000, 12000);

                        person.Happiness = 100;
                    }
                }
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "hard_grant_program",
                Frequency = DecisionFrequency.Yearly,
                Title = "Lokalny Program Grantów",
                Description = "Zainwestuj w innowacje i start-upy. Każdy pracujący ma 5% szansy na awans i wzrost pensji o 10%.",

                CalculateCost = (p) => 50000 + (p.Count(x => x.IsEmployed) * 200.0),

                HappinessEffect = 5.0,
                IsHard = true,
                TargetFilter = p => p.IsEmployed,
                TargetDescription = "Pracujący (Loteria zarobkowa)",

                InstantEffect = (population) =>
                {
                    Random rnd = new Random();
                    foreach (var person in population)
                    {
                        if (person.IsEmployed)
                        {
                            if (rnd.NextDouble() < 0.05)
                            {
                                person.Income *= 1.10;

                                person.Happiness = 100;
                            }
                        }
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

            _allDecisions.Add(new CityDecision
            {
                Id = "env_graffiti",
                Frequency = DecisionFrequency.Yearly,
                Title = "Usuwanie Graffiti",
                Description = "Wyczyść pomazane mury w centrum. Miasto zyska na estetyce.",
                CalculateCost = (p) => 5000 + (p.Count * 2.0),
                HappinessEffect = 3.0,
                IsHard = false,
                TargetDescription = "Wszyscy"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "infra_sidewalk",
                Frequency = DecisionFrequency.Yearly,
                Title = "Remont Chodników",
                Description = "Wymiana płyt chodnikowych na kostkę brukową.",
                CalculateCost = (p) => 10000 + (p.Count * 10.0),
                HappinessEffect = 2.5,
                IsHard = false,
                TargetDescription = "Wszyscy"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "env_flowers",
                Frequency = DecisionFrequency.Yearly,
                Title = "Miejskie Rabaty Kwiatowe",
                Description = "Posadź bratki i begonie na rondach.",
                CalculateCost = (p) => 5000,
                HappinessEffect = 3.0,
                IsHard = false,
                TargetFilter = p => p.Age >= 60,
                TargetDescription = "Głównie Seniorzy"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "trans_night",
                Frequency = DecisionFrequency.Monthly,
                Title = "Nocne Autobusy",
                Description = "Uruchom dodatkowe linie nocne w weekendy.",
                CalculateCost = (p) => 3000 + (p.Count * 0.5),
                HappinessEffect = 3.0,
                IsHard = false,
                TargetFilter = p => p.Age >= 18 && p.Age <= 30,
                TargetDescription = "Młodzi Dorośli (18-30)"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "event_xmas",
                Frequency = DecisionFrequency.Yearly,
                Title = "Iluminacja Świąteczna",
                Description = "Udekoruj latarnie i postaw choinkę na rynku.",
                CalculateCost = (p) => 15000 + (p.Count * 2.0),
                HappinessEffect = 7.0,
                IsHard = false,
                TargetDescription = "Wszyscy"
            });

            _allDecisions.Add(new CityDecision
            {
                Id = "hard_factory",
                Frequency = DecisionFrequency.Yearly,
                Title = "Fabryka Chemiczna",
                Description = "Inwestor chce zbudować fabrykę. Śmierdzi, ale zapłaci fortunę za grunt.",
                CalculateCost = (p) => -5000000,
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

        public void AddNews(string message, bool isGoodNews, string? superColor)
        {
            string color = isGoodNews ? "#4CAF50" : "#F44336";
            if (message.Contains("Witaj")) color = "#FF505050";

            if (superColor != null)
            {
                color = superColor;
            }
            
            var news = new NewsItem
            {
                Text = message,
                BorderColor = color,
                Time = currentDate.ToShortDateString()
            };
            canClose = false;
            Dispatcher.Invoke(() =>
            {
                RecentNews.Insert(0, news);
            });

            if (RecentNews.Count > 6)
            {
                Dispatcher.Invoke(() =>
                {
                    RecentNews.RemoveAt(6);
                });
            }
            canClose = true;
        }

        private void CheckWinLossConditions()
        {
            double avgHappiness = _cityManager.GetAverageHappiness();
            int population = _cityManager.PopulationCount;

            if (population >= 100000 && _cityBudget > 0)
            {
                EndGame("ZWYCIĘSTWO!\n\nStworzyłeś potężną metropolię, która prosperuje finansowo. Twoja kadencja przejdzie do historii!", true);
                return;
            }

            if (avgHappiness < 20.0)
            {
                _monthsUnder20Percent++;
                if (_monthsUnder20Percent < 3)
                {
                    AddNews($"OSTRZEŻENIE! Skrajnie niskie zadowolenie ({_monthsUnder20Percent}/3 mies.)!", false, "#FF5500");
                }
            }
            else
            {
                _monthsUnder20Percent = 0;
            }

            if (_monthsUnder20Percent >= 3)
            {
                EndGame("PORAŻKA!\n\nWybuchły zamieszki. Mieszkańcy wywieźli Cię na taczce. Zadowolenie było krytyczne przez zbyt długi czas.", false);
                return;
            }

            if (avgHappiness < 50.0)
            {
                _monthsUnder50Percent++;
                if (_monthsUnder50Percent >= 2)
                {
                    if (_monthsUnder50Percent == 11)
                        AddNews($"UWAGA! Ludzie są nieszczęśliwi od 11 miesięcy. Grozi Ci odwołanie. Za 1 miesiąc wybory", false, "#FF5500");
                    else if (_monthsUnder50Percent < 11 && _monthsUnder50Percent > 8)
                        AddNews($"UWAGA! Ludzie są nieszczęśliwi od {_monthsUnder50Percent} miesięcy. Grozi Ci odwołanie. Za {12 - _monthsUnder50Percent} miesiące wybory", false, "#FF5500");
                    else
                        AddNews($"UWAGA! Ludzie są nieszczęśliwi od {_monthsUnder50Percent} miesięcy. Grozi Ci odwołanie. Za {12 - _monthsUnder50Percent} miesiący wybory", false, "#FF5500");

                }
            }
            else
            {
                _monthsUnder50Percent = 0;
            }

            if (_monthsUnder50Percent >= 12)
            {
                EndGame("PORAŻKA!\n\nPrzegrałeś wybory. Mieszkańcy byli niezadowoleni przez cały rok.", false);
                return;
            }
        }

        private void EndGame(string message, bool isWin)
        {
            isRunning = false;

            string title = isWin ? "WYGRANA" : "KONIEC GRY";
            MessageBoxImage icon = isWin ? MessageBoxImage.Information : MessageBoxImage.Error;
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(this, message, title, MessageBoxButton.OK, icon);
            });
            ButtonSave_Click(null, null);
            BtnExit_Click(null, null);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                StartWindow startWindow = new();
                this.Hide();
                startWindow.Show();
                Application.Current.MainWindow = startWindow;
                this.Close();
            });
        }


        private void ButtonSave_MouseEnter(object sender, MouseEventArgs e)
        {
            BorderSave.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x88, 0x88, 0x88));
            ButtonSave.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x1e, 0x1e, 0x1e));
            Cursor = Cursors.Hand;
        }

        private void ButtonSave_MouseLeave(object sender, MouseEventArgs e)
        {
            BorderSave.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x1e, 0x1e, 0x1e));
            ButtonSave.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x88, 0x88, 0x88));
            Cursor = Cursors.Arrow;
        }
        private void ButtonLoad_MouseEnter(object sender, MouseEventArgs e)
        {
            BorderLoad.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x88, 0x88, 0x88));
            ButtonLoad.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x1e, 0x1e, 0x1e));
            Cursor = Cursors.Hand;
        }

        private void ButtonLoad_MouseLeave(object sender, MouseEventArgs e)
        {
            BorderLoad.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x1e, 0x1e, 0x1e));
            ButtonLoad.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x88, 0x88, 0x88));
            Cursor = Cursors.Arrow;
        }

        private void SpeedBar_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle rect && int.TryParse(rect.Tag.ToString(), out int level))
            {
                SetGameSpeed(level);
            }
        }
        private void SetGameSpeed(int level)
        {
            _currentSpeedLevel = level;

            Rectangle[] bars = { SpeedBar1, SpeedBar2, SpeedBar3, SpeedBar4, SpeedBar5 };

            var colorActive = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)); // Zielony
            var colorInactive = new SolidColorBrush(Color.FromRgb(0x50, 0x50, 0x50)); // Szary
            var colorDanger = new SolidColorBrush(Color.FromRgb(0xFF, 0x33, 0x33)); // Czerwony

            for (int i = 0; i < bars.Length; i++)
            {
                int barLevel = i + 1;

                if (barLevel <= _currentSpeedLevel)
                {
                    if (barLevel == 5 && _currentSpeedLevel == 5)
                    {
                        bars[i].Fill = colorDanger;
                    }
                    else
                    {
                        bars[i].Fill = colorActive;
                    }
                }
                else
                {
                    bars[i].Fill = colorInactive;
                }
            }

            if (_timer != null)
            {
                int period = 100;

                switch (level)
                {
                    case 1: period = 250; break;
                    case 2: period = 100; break;
                    case 3: period = 50; break;
                    case 4: period = 20; break;
                    case 5: period = 5; break;
                }
                
                _timer.Change(0, period);
            }
        }
        private void UpdateWallpaper()
        {
            int population = _cityManager.PopulationCount;
            double happiness = _cityManager.GetAverageHappiness();

            string sizePrefix = population >= 20000 ? "big" : "small";
            string moodSuffix = "Normal";

            if (happiness < 25) moodSuffix = "Riot";
            else if (happiness < 45) moodSuffix = "Sad";
            else if (happiness > 75) moodSuffix = "Happy";

            string fileName = $"{sizePrefix}{moodSuffix}.png";

            if (_currentWallpaperName == fileName) return;

            Dispatcher.Invoke(() =>
            {
                try
                {
                    string packUri = $"pack://application:,,,/Resource/{fileName}";

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(packUri);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    if (ImageBackground != null)
                    {
                        ImageBackground.ImageSource = bitmap;
                    }

                    _currentWallpaperName = fileName;
                }
                catch (Exception ex)
                {
                }
            });
        }
    }
    
}