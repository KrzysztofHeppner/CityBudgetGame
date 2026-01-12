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

                foreach (var _ in Enumerable.Range(0, potentialMothers))
                {
                    if (_random.NextDouble() < 0.1)
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
        /// Aktualizuje poziom zadowolenia w zależności od podatków.
        /// </summary>
        public void UpdateHappiness(double taxRate)
        {
            lock (_populationLock)
            {
                foreach (var person in _population)
                {
                    if (taxRate > 0.20) person.Happiness -= 5;
                    else if (taxRate < 0.10) person.Happiness += 2;

                    if (!person.IsEmployed && person.IsWorkingAge())
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
    }
}