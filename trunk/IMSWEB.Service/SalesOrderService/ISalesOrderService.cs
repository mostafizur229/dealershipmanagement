using IMSWEB.Model;
using IMSWEB.Model.SPModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Service
{
    public interface ISalesOrderService
    {
        Task<IEnumerable<Tuple<int, string, DateTime, string,
                      string, decimal, EnumSalesType, Tuple<string>>>>
                      GetAllSalesOrderAsync(DateTime fromDate, DateTime toDate,
                      List<EnumSalesType> SalesType, bool IsVATManager, int concernID,
                      string InvoiceNo = "", string ContactNo = "", string CustomerName = "", string AccountNo = "");

        Task<IEnumerable<Tuple<int, string, DateTime, string,
  string, decimal, EnumSalesType, Tuple<string>>>>
      GetAllSalesOrderAsyncByUserID(int UserID, DateTime fromDate, DateTime toDate,
      EnumSalesType SalesType, string InvoiceNo = "", string ContactNo = "", string CustomerName = "", string AccountNo = "");



        IQueryable<SOrder> GetAllIQueryable();
        void AddSalesOrder(SOrder salesOrder);
        int AddSalesOrderUsingSP(DataTable dtSalesOrder, DataTable dtSalesOrderDetail, DateTime RemindDate, DataTable dtPaymentDetails);
        void AddReplacementOrderUsingSP(DataTable dtSalesOrder, DataTable dtSalesOrderDetail);
        void SaveSalesOrder();
        SOrder GetSalesOrderById(int id);
        void DeleteSalesOrder(int id);

        IEnumerable<SOredersReportModel> GetforSalesReport(DateTime fromDate, DateTime toDate, int EmployeeID, int CustomerID);

        IEnumerable<Tuple<DateTime, string, string, decimal, decimal, decimal, decimal, Tuple<decimal, decimal, decimal, decimal, decimal, string>>>
           GetforSalesDetailReport(DateTime fromDate, DateTime toDate);

        IEnumerable<SOredersReportModel>GetforSalesDetailReportByMO(DateTime fromDate, DateTime toDate,int MOID);

        IEnumerable<Tuple<string, string, DateTime, string, decimal, decimal, decimal, Tuple<decimal, decimal, decimal, decimal>>>
            GetSalesReportByConcernID(DateTime fromDate, DateTime toDate, int concernID, int CustomerType);

        IEnumerable<ProductWiseSalesReportModel>
           GetSalesDetailReportByConcernID(DateTime fromDate, DateTime toDate,int concernID);

        IEnumerable<ProductWiseSalesReportModel>
         GetSalesDetailReportAdminByConcernID(DateTime fromDate, DateTime toDate, int concernID,int CustomerType);

        IEnumerable<SOredersReportModel>GetSalesDetailReportByCustomerID(DateTime fromDate, DateTime toDate, int Customer);

        IEnumerable<Tuple<string, DateTime, string, string, decimal, decimal, Tuple<decimal, decimal, decimal, decimal>>>
            GetSalesDetailReportByMOID(DateTime fromDate, DateTime toDate, int concernId,int MOID,int RptType);

        IEnumerable<Tuple<string, string, string, string, string, decimal>>
           GetMOWiseCustomerDueRpt(int concernId, int MOID, int RptType);

        IEnumerable<Tuple<DateTime, string, string, decimal, decimal>> GetSalesByProductID(DateTime fromDate, DateTime toDate, int ConcernId, int productID);
        bool UpdateSalesOrderUsingSP(int userId, int salesOrderId, DataTable dtSalesOrder, DataTable dtSODetail, DataTable dtPaymentDetails);

        void DeleteSalesOrderUsingSP(int orderId, int userId);

        void DeleteSalesOrderDetailUsingSP(int orderId, int userId);

        void CorrectionStockData(int concermID);

        List<SRWiseCustomerSalesSummaryVM> SRWiseCustomerSalesSummary(DateTime fromdate, DateTime todate, int ConcernID, int EmployeeID);
        List<CustomerLedgerModel> CustomerLedger(DateTime fromdate, DateTime todate, int ConcernID, int CustomerID);
        List<LedgerAccountReportModel> CustomerLedger(DateTime fromdate, DateTime todate, int CustomerID);
        List<LedgerAccountReportModel> BothCustomerLedger(DateTime fromdate, DateTime todate, int CustomerID);
        List<CustomerDueReportModel> CustomerDue(DateTime fromdate, DateTime todate, int ConcernID, int CustomerID, int IsOnlyDue);

        Task<IEnumerable<Tuple<int, string, DateTime, string, string, decimal, EnumSalesType>>> GetReplacementOrdersByAsync(int EmployeeID);
        Task<IEnumerable<Tuple<int, string, DateTime, string, string, decimal, EnumSalesType>>> GetReturnOrdersByAsync();

        List<ReplaceOrderDetail> GetReplaceOrderInvoiceReportByID(int OrderID);
        bool AddReturnOrderUsingSP(DataTable dtSalesOrder, DataTable dtSalesOrderDetail);
        List<ReplaceOrderDetail> GetReturnOrderInvoiceReportByID(int OrderID);
        List<DailyWorkSheetReportModel> DailyWorkSheetReport(DateTime fromdate, DateTime todate, int ConcernID);
        List<ReplacementReportModel> ReplacementOrderReport(DateTime fromdate, DateTime todate, int ConcernID, int CustomerID);
        List<ReturnReportModel> ReturnOrderReport(DateTime fromdate, DateTime todate, int ConcernID, int CustomerID);
        List<MonthlyBenefitReport> MonthlyBenefitReport(DateTime fromdate, DateTime todate, int ConcernID);
        List<ProductWiseBenefitModel> ProductWiseBenefitReport(DateTime fromdate, DateTime todate, int ConcernID);
        List<ProductWiseSalesReportModel> ProductWiseSalesReport(DateTime fromDate, DateTime toDate, int ConcernID, int CustomerID);
        List<ReplacementReportModel> DamageProductReport(DateTime fromdate, DateTime todate, int ConcernID, int CustomerID);
        List<ProductWiseSalesReportModel> ProductWiseSalesDetailsReport(int CompanyID, int CategoryID, int ProductID, DateTime fromDate, DateTime toDate);
        SOrder GetLastSalesOrderByCustomerID(int CustomerID);
        decimal GetAllCollectionAmountByDateRange(DateTime fromDate, DateTime toDate);
        decimal GetVoltageStabilizerCommission(DateTime fromDate, DateTime toDate);
        decimal GetExtraCommission(DateTime fromDate, DateTime toDate,int ConcernID);
        bool IsIMEIAlreadyReplaced(int StockDetailID);
        List<SOredersReportModel> GetAdminSalesReport(int ConcernID, DateTime fromDate, DateTime toDate);

        bool IsSoReturn(int SoId);
        bool IsDoSales(int SoId);

        IEnumerable<SOredersReportModel> GetforSalesReportForAll(DateTime fromDate, DateTime toDate, int EmployeeID, EnumCustomerType customerType);
    }
}
