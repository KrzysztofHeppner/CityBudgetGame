using System;
using System.Collections.Generic;

namespace CityBudget
{
    public class GameState
    {
        public double Budget { get; set; }
        public DateTime CurrentDate { get; set; }
        public TaxSettings Taxes { get; set; }
        public BudgetPolicy Policy { get; set; }
        public List<Person> Population { get; set; }
        public Dictionary<string, DateTime> DecisionsHistory { get; set; }
    }
}