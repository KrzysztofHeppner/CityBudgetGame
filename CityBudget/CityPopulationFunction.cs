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
                    int age = _random.Next(0, 90);

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
                    double baseChance = 0.10;

                    if (mother.Happiness < 30) baseChance = 0.01;
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
        /// </summary>
        public void UpdateHappiness(TaxSettings taxes, BudgetPolicy policy)
        {
            lock (_populationLock)
            {
                foreach (var person in _population)
                {
                    double change = 0.0;

                    if (person.IsEmployed)
                    {
                        change -= (taxes.PIT * 20.0);
                        if(taxes.PIT > 0.25)
                        {
                            change -= (taxes.PIT - 0.25) * 20.0;
                        }
                    }

                    change -= (taxes.VAT * 20.0);
                    if (taxes.VAT > 0.25)
                    {
                        change -= (taxes.VAT - 0.25) * 20.0;
                    }

                    if (person.Age >= 18)
                    {
                        change -= (taxes.PropertyTax / 200.0);
                        if (taxes.PropertyTax / 200.0 > 1.0)
                        {
                            change -= (taxes.PropertyTax / 200.0 - 1.0) * 20.0;
                        }
                    }

                    change += (policy.Education) * 2.0;

                    double healthWeight = person.Age > 60 ? 5.0 : 2.0;
                    change += (policy.Healthcare) * healthWeight;

                    change += (policy.Police) * 0.7;
                    change += (policy.FireDept) * 0.7;

                    if (person.IsEmployed)
                    {
                        change += (policy.Roads) * 2;
                    }
                    else
                    {
                        change += (policy.Roads) * 0.1;
                    }

                    change += (policy.Administration) * 0.5;


                    if (person.Happiness > 60)
                    {
                        change -= 3;
                        if (change < 0)
                        {
                            change = 0;
                        }
                    }
                    if (person.Happiness < 40)
                    {
                        change += 3;
                        if (change > 0)
                        {
                            change = 0;
                        }
                    }
                    if (person.IsEmployed) 
                    { 
                    }
                    person.Happiness += change;

                    person.Happiness = Math.Clamp(person.Happiness, 0.0, 100.0);
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
        public FinanceReport CalculateFinances(TaxSettings taxes, BudgetPolicy policy)
        {
            var report = new FinanceReport();

            lock (_populationLock)
            {
                foreach (var person in _population)
                {
                    if (person.IsEmployed)
                    {
                        double pitVal = person.Income * taxes.PIT;
                        report.IncomePIT += pitVal;
                        double netIncome = person.Income - pitVal;
                        report.IncomeVAT += (netIncome * 0.8) * taxes.VAT;
                    }
                    if (person.Age >= 18) report.IncomeProperty += taxes.PropertyTax;
                }

                int students = _population.Count(p => p.Age >= 6 && p.Age <= 24);
                report.ExpenseEducation = students * 300 * policy.Education;

                int seniors = _population.Count(p => p.Age > 65);
                int babies = _population.Count(p => p.Age < 5);
                int others = _population.Count - (seniors + babies);
                double baseHealthCost = (seniors * 1000) + (babies * 400) + (others * 50);
                report.ExpenseHealthcare = baseHealthCost * policy.Healthcare;

                report.ExpensePolice = _population.Count * 25 * policy.Police;

                report.ExpenseFireDept = _population.Count * 15 * policy.FireDept;

                double baseRoads = 30000 + (_population.Count * 10);
                report.ExpenseRoads = baseRoads * policy.Roads;

                report.ExpenseAdministration = 200000 * policy.Administration;
            }

            return report;
        }

        /// <summary>
        /// Symuluje migrację ludności na podstawie zadowolenia.
        /// Dorośli zabierają ze sobą dzieci.
        /// </summary>
        public string HandleMigration()
        {
            int adultsLeft = 0;
            int childrenLeft = 0;
            int cameToCity = 0;

            lock (_populationLock)
            {
                var unhappyAdults = _population
                    .Where(p => p.Age >= 18 && p.Happiness < 25)
                    .ToList();

                List<Person> peopleToLeave = new List<Person>();

                var childrenPool = new Queue<Person>(_population.Where(p => p.Age < 18));

                foreach (var adult in unhappyAdults)
                {
                    if (_random.NextDouble() < 0.05)
                    {
                        peopleToLeave.Add(adult);
                        adultsLeft++;

                        if (adult.Age >= 20 && adult.Age <= 50)
                        {
                            double childChance = _random.NextDouble();
                            int kidsToTake = 0;

                            if (childChance > 0.4 && childChance <= 0.8) kidsToTake = 1;
                            else if (childChance > 0.8) kidsToTake = 2;

                            for (int i = 0; i < kidsToTake; i++)
                            {
                                if (childrenPool.Count > 0)
                                {
                                    var child = childrenPool.Dequeue();
                                    peopleToLeave.Add(child);
                                    childrenLeft++;
                                }
                            }
                        }
                    }
                }

                foreach (var p in peopleToLeave)
                {
                    _population.Remove(p);
                }


                double avgHappiness = _population.Count > 0 ? _population.Average(p => p.Happiness) : 0;

                if (avgHappiness > 65)
                {
                    int newAdultsCount = (int)((avgHappiness - 65) * (_population.Count * 0.0005));

                    for (int i = 0; i < newAdultsCount; i++)
                    {
                        int age = _random.Next(20, 80);
                        Gender gender = (Gender)_random.Next(0, 2);
                        var newPerson = new Person(age, gender);
                        newPerson.Happiness = 60;
                        newPerson.IsEmployed = _random.NextDouble() > 0.2;
                        if (newPerson.IsEmployed) newPerson.Income = _random.Next(2500, 6000);

                        _population.Add(newPerson);
                        cameToCity++;

                        if (_random.NextDouble() < 0.30)
                        {
                            var newChild = new Person(_random.Next(0, 10), (Gender)_random.Next(0, 2));
                            newChild.Happiness = 60;
                            _population.Add(newChild);
                            cameToCity++;
                        }
                    }
                }
            }

            int totalLeft = adultsLeft + childrenLeft;

            if (totalLeft > cameToCity)
                return $"Kryzys! Wyjechało {adultsLeft} dorosłych i {childrenLeft} dzieci.";

            if (cameToCity > totalLeft)
                return $"Rozwój! Przybyło {cameToCity} nowych mieszkańców.";

            return "Migracja stabilna.";
        }

        /// <summary>
        /// Średnie zadowolenie mieszkańców.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Aplikuje jednorazowy efekt decyzji (zmiana zadowolenia).
        /// </summary>
        public void ApplyDirectHappinessChange(double amount)
        {
            lock (_populationLock)
            {
                foreach (var person in _population)
                {
                    person.Happiness += amount;
                    person.Happiness = Math.Clamp(person.Happiness, 0.0, 100.0);
                }
            }
        }


    }
}