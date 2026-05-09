using IMSWEB.Model.SPModel;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace IMSWEB.Data
{
    public class BalanceSheetRepository : IBalanceSheetRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["IMSWEB"].ConnectionString;

        public BalanceSheetSpModel GetBalanceSheetData(int concernId, DateTime from, DateTime to)
        {
            BalanceSheetSpModel model = new BalanceSheetSpModel();

            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("GetBalanceSheetData", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.AddWithValue("@ConcernId", concernId);
                cmd.Parameters.AddWithValue("@ConcernId", concernId);
                cmd.Parameters.AddWithValue("@FromDate", from);
                cmd.Parameters.AddWithValue("@ToDate", to);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        model.StockValue = Convert.ToDecimal(reader["StockValue"]);
                        model.CustomerDue = Convert.ToDecimal(reader["CustomerDue"]);
                        model.SupplierAdvancePayment = Convert.ToDecimal(reader["SupplierAdvancePayment"]);
                        model.BankBalance = Convert.ToDecimal(reader["BankBalance"]);
                        model.CashInHand = Convert.ToDecimal(reader["CashInHand"]);
                        model.TotalAssets = Convert.ToDecimal(reader["TotalAssets"]);

                        model.PurchaseCost = Convert.ToDecimal(reader["PurchaseCost"]);
                        model.TotalInvestment = Convert.ToDecimal(reader["TotalInvestment"]);
                        model.TotalExpense = Convert.ToDecimal(reader["TotalExpense"]);
                        model.SupplierDue = Convert.ToDecimal(reader["SupplierDue"]);
                        model.LoansOtherLiabilities = Convert.ToDecimal(reader["LoansOtherLiabilities"]);
                        model.OwnersDrawings = Convert.ToDecimal(reader["OwnersDrawings"]);
                        model.TotalLiabilitiesExpenses = Convert.ToDecimal(reader["TotalLiabilitiesExpenses"]);

                        model.SalesRevenue = Convert.ToDecimal(reader["SalesRevenue"]);
                        model.OtherIncome = Convert.ToDecimal(reader["OtherIncome"]);
                        model.TotalGrossProfit = Convert.ToDecimal(reader["TotalGrossProfit"]);

                        model.NetProfit = Convert.ToDecimal(reader["NetProfit"]);


                    }
                }
            }

            return model;
        }
    }
}
