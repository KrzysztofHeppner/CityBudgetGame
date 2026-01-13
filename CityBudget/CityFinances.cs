using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBudget
{
    public class TaxSettings
    {
        public double PIT { get; set; } = 0.18;
        public double VAT { get; set; } = 0.23;
        public double PropertyTax { get; set; } = 50;
    }

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

    public class CityDecision
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public double Cost { get; set; }
        public double HappinessEffect { get; set; }
        public bool IsHard { get; set; }

        public string CostText => Cost > 0 ? $"-{Cost:N0} PLN" : $"+{Math.Abs(Cost):N0} PLN";
        public System.Windows.Media.Brush CostColor => Cost > 0 ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.LightGreen;
    }
}