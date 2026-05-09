using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Model.SPModel
{
    
    public class BalanceSheetSpModel
    {
        public CompanyDetailsViewModel CompanyDetails { get; set; }

        public class CompanyDetailsViewModel
        {
            public string CompanyName { get; set; }
            public string TelephoneNo { get; set; }
            public string Address { get; set; }
        }

        


        // Assets
        public decimal StockValue { get; set; }
        public decimal CustomerDue { get; set; }
        public decimal SupplierAdvancePayment { get; set; }
        public decimal BankBalance { get; set; }
        public decimal CashInHand { get; set; }
        public decimal TotalAssets { get; set; }

        // Liabilities & Expenses
        public decimal PurchaseCost { get; set; }
        public decimal TotalInvestment { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal SupplierDue { get; set; }
        public decimal LoansOtherLiabilities { get; set; }
        public decimal OwnersDrawings { get; set; }
        public decimal TotalLiabilitiesExpenses { get; set; }

        // Income
        public decimal SalesRevenue { get; set; }
        public decimal OtherIncome { get; set; }
        public decimal TotalGrossProfit { get; set; }

        // Summary
        public decimal NetProfit { get; set; }



        
    }
}
