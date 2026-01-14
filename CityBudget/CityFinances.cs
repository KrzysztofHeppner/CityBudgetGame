using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CityBudget
{
    /// <summary>
    /// Ustawienia podatkowe miasta
    /// </summary>
    public class TaxSettings
    {
        public double PIT { get; set; } = 0.18;
        public double VAT { get; set; } = 0.23;
        public double PropertyTax { get; set; } = 50;
    }
    /// <summary>
    /// Polityka budżetowa miasta
    /// </summary>
    public class BudgetPolicy
    {
        public double Education { get; set; } = 1.0;
        public double Healthcare { get; set; } = 1.0;
        public double Police { get; set; } = 1.0;
        public double FireDept { get; set; } = 1.0;
        public double Roads { get; set; } = 1.0;
        public double Administration { get; set; } = 1.0;

        public double ChildBenefitAmount { get; set; } = 0;
    }
    /// <summary>
    /// Raport finansowy miasta
    /// </summary>
    public class FinanceReport
    {
        public double IncomePIT { get; set; }
        public double IncomeVAT { get; set; }
        public double IncomeProperty { get; set; }
        public double TotalIncome => IncomePIT + IncomeVAT + IncomeProperty;

        public double ExpenseEducation { get; set; }
        public double ExpenseHealthcare { get; set; }
        public double ExpensePolice { get; set; }
        public double ExpenseFireDept { get; set; }
        public double ExpenseRoads { get; set; }
        public double ExpenseAdministration { get; set; }
        public double ExpenseSocial { get; set; }

        public double TotalExpenses => ExpenseEducation + ExpenseHealthcare + ExpensePolice + ExpenseFireDept + ExpenseRoads + ExpenseAdministration + ExpenseSocial;
        public double Balance => TotalIncome - TotalExpenses;
    }
    /// <summary>
    /// Decyzja miejska
    /// </summary>
    public class CityDecision
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Func<List<Person>, double> CalculateCost { get; set; }
        public double CurrentCost { get; set; }
        public double HappinessEffect { get; set; }
        public bool IsHard { get; set; }
        public Predicate<Person> TargetFilter { get; set; }
        public string TargetDescription { get; set; } = "Wszyscy Mieszkańcy";

        public DecisionFrequency Frequency { get; set; }
        public DateTime? LastUsedDate { get; set; }

        public Action<List<Person>> InstantEffect { get; set; }

        public string CostText => CurrentCost > 0 ? $"-{CurrentCost:N0} PLN" : $"+{Math.Abs(CurrentCost):N0} PLN";
        public Brush CostColor => CurrentCost > 0 ? Brushes.Red : Brushes.LightGreen;

        public string FrequencyText
        {
            get
            {
                switch (Frequency)
                {
                    case DecisionFrequency.OneTime: return "JEDNORAZOWA";
                    case DecisionFrequency.Monthly: return "CO MIESIĄC";
                    case DecisionFrequency.Yearly: return "CO ROK";
                    default: return "";
                }
            }
        }
    }
    /// <summary>
    /// Częstotliwość decyzji miejskiej
    /// </summary>
    public enum DecisionFrequency
    {
        OneTime,
        Monthly,
        Yearly
    }
}