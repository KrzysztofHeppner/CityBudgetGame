using System;
using System.Collections.Generic;
using System.Linq;

namespace CityBudget
{
    public class CityPopulationFunction
    {
        private List<Person> _population;

        private readonly object _populationLock = new object();

        private Random _random = new Random();
        
        public CityPopulationFunction()
        {
            _population = new List<Person>();
        }

        public List<Person> GetPopulationSnapshot()
        {
            lock (_populationLock)
            {
                return new List<Person>(_population);
            }
        }

        public int PopulationCount
        {
            get
            {
                lock (_populationLock)
                {
                    return _population.Count;
                }
            }
        }


        /// <summary>
        /// Tworzy startową populację miasta.
        /// </summary>
        /// <param name="initialCount">Liczba mieszkańców na start</param>
        public void MakeNewPopulation(int initialCount)
        {
            lock (_populationLock)
            {
                _population.Clear();

                for (int i = 0; i < initialCount; i++)
                {
                    int age = _random.Next(0, 80);

                    Gender gender = (Gender)_random.Next(0, 2);

                    Person newPerson = new Person(age, gender);

                    if (newPerson.IsWorkingAge())
                    {
                        newPerson.IsEmployed = _random.NextDouble() > 0.2;
                        if (newPerson.IsEmployed)
                        {
                            newPerson.Income = _random.Next(2000, 5000);
                        }
                    }

                    _population.Add(newPerson);
                }
            }
        }


        /// <summary>
        /// Symuluje upływ jednego roku (starzenie, zgony, narodziny).
        /// </summary>
        public void SimulateYear()
        {
            lock (_populationLock)
            {
                List<Person> peopleToDie = new List<Person>();

                foreach (var person in _population)
                {
                    person.Age++;

                    double deathChance = 0;
                    if (person.Age > 75) deathChance = 0.05;
                    if (person.Age > 90) deathChance = 0.20;

                    if (_random.NextDouble() < deathChance)
                    {
                        peopleToDie.Add(person);
                    }
                }

                foreach (var dead in peopleToDie)
                {
                    _population.Remove(dead);
                }

                int potentialMothers = _population.Count(p => p.Gender == Gender.Female && p.Age >= 18 && p.Age <= 40);
                int newBabies = 0;

                foreach (var mother in _population.Where(p => p.Gender == Gender.Female && p.Age >= 18 && p.Age <= 40))
                {
                    double baseChance = 0.15;

                    if (mother.Happiness < 30) baseChance = 0.02;
                    else if (mother.Happiness > 80) baseChance = 0.25;

                    if (_random.NextDouble() < baseChance)
                    {
                        newBabies++;
                    }
                }

                for (int i = 0; i < newBabies; i++)
                {
                    Gender babyGender = (Gender)_random.Next(0, 2);
                    _population.Add(new Person(0, babyGender));
                }
            }
        }

        /// <summary>
        /// Oblicza całkowity wpływ z podatków od pracujących mieszkańców.
        /// </summary>
        /// <param name="taxRate">Stawka podatku (np. 0.19 dla 19%)</param>
        /// <returns>Kwota do budżetu</returns>
        public double CalculateTaxIncome(double taxRate)
        {
            lock (_populationLock)
            {
                double totalIncome = 0;
                foreach (var person in _population)
                {
                    if (person.IsEmployed)
                    {
                        totalIncome += person.Income * taxRate;
                    }
                }
                return totalIncome;
            }
        }

        /// <summary>
        /// Aktualizuje zadowolenie mieszkańców na podstawie podatków.
        /// Wywołuj to CO MIESIĄC, żeby gracz szybko widział reakcję.
        /// </summary>
        public void UpdateHappiness(TaxSettings taxes)
        {
            lock (_populationLock)
            {
                foreach (var person in _population)
                {
                    if (person.Happiness > 50) person.Happiness--;
                    if (person.Happiness < 50) person.Happiness++;

                    if (person.IsEmployed)
                    {
                        if (taxes.PIT > 0.20) person.Happiness -= 3;
                        else if (taxes.PIT < 0.10) person.Happiness += 2;
                    }

                    if (taxes.VAT > 0.23) person.Happiness -= 2;
                    else if (taxes.VAT < 0.15) person.Happiness += 1;

                    if (person.Age >= 18)
                    {
                        if (taxes.PropertyTax > 100) person.Happiness -= 2;
                        else if (taxes.PropertyTax < 20) person.Happiness += 1;
                    }

                    if (person.Age >= 18 && person.Age < 65 && !person.IsEmployed)
                    {
                        person.Happiness -= 2;
                    }

                    person.Happiness = Math.Clamp(person.Happiness, 0, 100);
                }
            }
        }

        /// <summary>
        /// Zwraca średnią wieku w mieście.
        /// </summary>
        public double GetAverageAge()
        {
            lock (_populationLock)
            {
                if (_population.Count == 0) return 0;
                return _population.Average(p => p.Age);
            }
        }

        /// <summary>
        /// Oblicza szczegółowy bilans miesięczny.
        /// </summary>
        public FinanceReport CalculateFinances(TaxSettings taxes)
        {
            var report = new FinanceReport();

            lock (_populationLock)
            {
                foreach (var person in _population)
                {
                    if (person.IsEmployed)
                    {
                        double taxValue = person.Income * taxes.PIT;
                        report.IncomePIT += taxValue;

                        double netIncome = person.Income - taxValue;
                        double spending = netIncome * 0.8;
                        report.IncomeVAT += spending * taxes.VAT;
                    }

                    if (person.Age >= 18)
                    {
                        report.IncomeProperty += taxes.PropertyTax;
                    }
                }

                int students = _population.Count(p => p.Age >= 6 && p.Age <= 24);
                report.ExpenseEducation = students * 300;

                int seniors = _population.Count(p => p.Age > 65);
                int babies = _population.Count(p => p.Age < 5);
                int others = _population.Count - (seniors + babies);
                report.ExpenseHealthcare = (seniors * 400) + (babies * 200) + (others * 50);

                report.ExpenseSecurity = _population.Count * 40;

                report.ExpenseInfrastructure = 50000 + (_population.Count * 10);

                report.ExpenseAdministration = 20000;
            }

            return report;
        }

        /// <summary>
        /// Symuluje migrację ludności na podstawie zadowolenia.
        /// Wywołuj co rok lub co miesiąc.
        /// </summary>
        public string HandleMigration()
        {
            int leftCity = 0;
            int cameToCity = 0;

            lock (_populationLock)
            {
                List<Person> peopleLeaving = new List<Person>();

                foreach (var person in _population)
                {
                    if (person.Happiness < 20)
                    {
                        if (_random.NextDouble() < 0.10)
                        {
                            peopleLeaving.Add(person);
                        }
                    }
                }

                foreach (var p in peopleLeaving)
                {
                    _population.Remove(p);
                }
                leftCity = peopleLeaving.Count;


                double avgHappiness = _population.Count > 0 ? _population.Average(p => p.Happiness) : 0;

                if (avgHappiness > 60)
                {
                    int newPeopleCount = (int)((avgHappiness - 50) * (_population.Count * 0.005));

                    for (int i = 0; i < newPeopleCount; i++)
                    {
                        int age = _random.Next(18, 40);
                        Gender gender = (Gender)_random.Next(0, 2);
                        var newPerson = new Person(age, gender);
                        newPerson.Happiness = 50;

                        newPerson.IsEmployed = _random.NextDouble() > 0.3;
                        if (newPerson.IsEmployed) newPerson.Income = _random.Next(2500, 6000);

                        _population.Add(newPerson);
                    }
                    cameToCity = newPeopleCount;
                }
            }

            if (leftCity > cameToCity) return $"Kryzys! {leftCity} osób wyjechało.";
            if (cameToCity > leftCity) return $"Rozwój! {cameToCity} nowych mieszkańców.";
            return "Stabilizacja populacji.";
        }

        public double GetAverageHappiness()
        {
            lock (_populationLock)
            {
                if (_population.Count == 0) return 0;
                return _population.Average(p => p.Happiness);
            }
        }

        /// <summary>
        /// Podmienia całą populację na tę wczytaną z pliku.
        /// </summary>
        public void LoadPopulation(List<Person> loadedPopulation)
        {
            lock (_populationLock)
            {
                _population = new List<Person>(loadedPopulation);
            }
        }
    }
}