using IMSWEB.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMSWEB.Model.SPModel;
using System.Data;

namespace IMSWEB.Service
{
    public interface IBankTransactionService
    {

        void AddBankTransaction(BankTransaction bankTransaction);
        void UpdateBankTransaction(BankTransaction bankTransaction);
        void SaveBankTransaction();

        Task<IEnumerable<Tuple<int, string, string, string, string, string, string, Tuple<decimal, DateTime?, string, string, string, string, string>>>>
           GetAllBankTransactionAsync(DateTime fromDate, DateTime toDate,
               List<EnumWFStatus> enumWFStatuses);

        IEnumerable<Tuple<int, string, string, string, string, string, string, Tuple<decimal, DateTime?, string, string, string, string, string, Tuple<string, string, string, string>>>> GetAllBankTransaction();
        IEnumerable<Tuple<int, string, string, string, string, string, string,
            Tuple<decimal, DateTime?, string, string, string, string, string,
                Tuple<string, string, string, string, EnumCustomerType>>>>
            GetBankTransactionData(DateTime fromDate, DateTime toDate, int concernID, int customerId,
            int SupplierID, int EmployeeID, EnumTransactionType Status,  EnumCustomerType customerType = 0);

        //IEnumerable<Tuple<int, string, string, decimal, string, string, string, Tuple<decimal?, string, decimal, int, int, string, string, Tuple<int, int, int, int>>>> GetAllProductFromDetail();
        //IEnumerable<Tuple<int, string, string, decimal, string, string, string, Tuple<decimal?, string, decimal, int, int, string, string>>> SRWiseGetAllProductFromDetail(int EmployeeID);
        BankTransaction GetBankTransactionById(int id);
        void DeleteBankTransaction(int id);

        List<BankLedgerModel> BankLedgerUsingSP(DateTime fromdate, DateTime todate, int ConcernID, int BankID);


        //IEnumerable<BankSummaryReportModel> GetBankSummary();

        //IEnumerable<Tuple<int, string, string,decimal, string, string, string, Tuple<decimal?, string, decimal, int, int, string, string>>> GetAllSalesProductFromDetailByCustomerID();
        //GetAllProductFromDetail
        //IEnumerable<Tuple<int, string, string,
        // decimal, string, string, string, Tuple<decimal?, string, decimal, int, int, string, string>>> GetAllSalesBankTransactionFromDetailByCustomerID(int CustomerID);
        //IEnumerable<Tuple<int, string, string, decimal, string, string, string, Tuple<decimal?, string, decimal, int, int, string, string, Tuple<decimal, int, int, int, int>>>> GetAllBankTransactionFromDetailForCredit();
        //IEnumerable<Tuple<int, string, string, string, string>> GetBankTransactionDetails();
        List<BankTransReportModel> BankTransactionsReport(DateTime fromdate, DateTime todate, int BankID, int concernId);
        List<BankTransReportModel> BankLedger(DateTime fromdate, DateTime todate, int BankID);
        IQueryable<CashCollectionReportModel> AdminCashCollectionByBank(int ConcernID, DateTime fromDate,
            DateTime toDate,
                                    EnumCustomerType customerType, int customerID);

        IEnumerable<Tuple<int, string, string, string, string, string, string,
        Tuple<decimal, DateTime?, string, string, string, string, string,
        Tuple<string, string, string, string, EnumCustomerType>>>>
        GetBankTransactionDataForAll(DateTime fromDate, DateTime toDate, int concernID, int customerId,
        int SupplierID, int EmployeeID, EnumTransactionType Status, bool IsAdmin, EnumCustomerType customerType = 0);

        DataTable CreateBankTransDataTable(DateTime TranDate, string TransactionNo,
        EnumTransactionType transactionType, decimal DepositAmt, int BankID, int ConcernID,
        string Remarks);
        IQueryable<BankTransaction> GetAll();
    }
}
