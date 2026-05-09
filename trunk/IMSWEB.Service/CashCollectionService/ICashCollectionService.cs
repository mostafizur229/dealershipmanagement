using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMSWEB.Model;
using IMSWEB.Model.TOs;
using IMSWEB.Model.SPModel;

namespace IMSWEB.Service

{
    public interface ICashCollectionService
    {
        void AddCashCollection(CashCollection oCashCollection);
        void UpdateCashCollection(CashCollection oCashCollection);
        void SaveCashCollection();
        IEnumerable<CashCollection> GetAllCashCollection();
        IQueryable<CashCollection> GetAllIQueryable();
        Task<IEnumerable<CashCollection>> GetAllCashCollectionAsync();

        Task<IEnumerable<Tuple<int, DateTime, string, string, string, string, string, Tuple<string, string>>>> GetAllCashCollAsync(DateTime fromDate, DateTime toDate);

        Task<IEnumerable<Tuple<int, DateTime, string,
        string, string, string, string>>> GetAllCashDelivaeryAsync(DateTime fromDate, DateTime toDate);

        IEnumerable<Tuple<DateTime, string, string, string, decimal, decimal, decimal, Tuple<decimal, string, string, string, string, string>>> GetCashCollectionData(DateTime fromDate, DateTime toDate, int concernID, int customerId, int repotType);
        IEnumerable<Tuple<DateTime, string, string, string, decimal, decimal, decimal, Tuple<decimal, string, string, string, string, string>>> GetCashDeliveryData(DateTime fromDate, DateTime toDate, int concernID, int supplierId, int reportType);
        CashCollection GetCashCollectionById(int id);
        void DeleteCashCollection(int id);

        void UpdateTotalDue(int CustomerID, int SupplierID, int BankID, int BankWithdrawID, decimal TotalDue);
        IEnumerable<DailyCashBookLedgerModel> DailyCashBookLedger(DateTime fromDate, DateTime toDate, int ConcernID);

        Task<IEnumerable<Tuple<int, DateTime, string, string, string, string, string, Tuple<string, string>>>> GetAllCashCollByEmployeeIDAsync(int EmployeeID);
        IEnumerable<Tuple<DateTime, string, string, string, decimal, decimal, decimal, Tuple<decimal, string, string, string, string, string, string>>> GetSRWiseCashCollectionReportData(DateTime fromDate, DateTime toDate, int concernID, int EmployeeID);

        void UpdateTotalDueWhenEdit(int CustomerID, int SupplierID, int BankTransactionID, int CashCollectionID, decimal TotalRecAmt);
        IQueryable<CashCollectionReportModel> AdminCashCollectionReport(DateTime fromDate, DateTime toDate, int ConcernID);
        IEnumerable<CashInHandReportModel> CashInHandReport(DateTime fromDate, DateTime toDate, int ReportType, int ConcernID, int CustomerType);
        bool IsCommissionApplicable(DateTime fromDate, DateTime toDate, int EmployeeID);
        List<CashInHandModel> CashInHandReport(DateTime fromDate, DateTime toDate, int ConcernID);

        List<CashInHandModel> ProfitAndLossReport(DateTime fromDate, DateTime toDate, int ConcernID);

        List<SummaryReportModel> SummaryReport(DateTime fromDate, DateTime toDate, decimal OpeningCashInHand, decimal CurrentCashInHand, decimal ClosingCashInHand, int ConcernID);
        List<TransactionReportModel> MonthlyTransactionReport(DateTime fromDate, DateTime toDate, int ConcernID);
        List<TransactionReportModel> MonthlyAdminTransactionReport(DateTime fromDate, DateTime toDate, int ConcernID);


        List<RPTPayRecTO> GetReceiptPaymentReportNew(DateTime asOnDate, DateTime toDate);


        void UpdateTotalDueForInvestment(int SIHID, int SIHId, int BankID, int BankWithdrawID, decimal TotalDue);
        void UpdateTotalDueForExpenditure(int ExpenseItemID, int BankID, int BankWithdrawID, decimal TotalDue, int ConcernID, int userId, string Remarks, DateTime EntryDate);
        IEnumerable<Tuple<DateTime, string, string, string, decimal, decimal, decimal,
            Tuple<decimal, string, string, string, string, string, EnumCustomerType>>>
        GetCashCollectionDataFoAll(DateTime fromDate, DateTime toDate, int ConcernId, int CustomerID, EnumCustomerType customerType);
    }
}
