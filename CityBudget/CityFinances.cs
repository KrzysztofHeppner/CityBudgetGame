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

    public class FinanceReport
    {
        public double IncomePIT { get; set; }
        public double IncomeVAT { get; set; }
        public double IncomeProperty { get; set; }
        public double TotalIncome => IncomePIT + IncomeVAT + IncomeProperty;

        public double ExpenseEducation { get; set; } // Szkoły
        public double ExpenseHealthcare { get; set; } // Szpitale
        public double ExpenseSecurity { get; set; }   // Policja/Straż
        public double ExpenseInfrastructure { get; set; } // Drogi
        public double ExpenseAdministration { get; set; } // Urzędy
        public double TotalExpenses => ExpenseEducation + ExpenseHealthcare + ExpenseSecurity + ExpenseInfrastructure + ExpenseAdministration;

        public double Balance => TotalIncome - TotalExpenses;
    }
}
