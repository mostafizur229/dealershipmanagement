using IMSWEB.Data;
using IMSWEB.Model;
using IMSWEB.Model.TOs;
using IMSWEB.Model.SPModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Service
{
    public class CashCollectionService : ICashCollectionService
    {
        private readonly IBaseRepository<CashCollection> _BaseCashCollectionRepository;
        private readonly IBaseRepository<BankTransaction> _BaseBankTransactionRepository;
        private readonly IBaseRepository<Bank> _BaseBankRepository;
        private readonly ICashCollectionRepository _cashCollectionRepository;
        private readonly IBaseRepository<Customer> _CustomerRepository;
        private readonly IBaseRepository<Supplier> _SupplierRepository;
        private readonly IBaseRepository<Employee> _EmployeeRepository;
        private readonly IBaseRepository<SisterConcern> _SisterConcernRepository;


        private readonly IBaseRepository<POrderDetail> _POrderDetailRepository;
        private readonly IBaseRepository<POrder> _POrderRepository;
        private readonly IBaseRepository<SOrderDetail> _SOrderDetailRepository;
        private readonly IBaseRepository<SOrder> _SOrderRepository;

        private readonly IBaseRepository<CreditSale> _CreditSaleRepository;
        private readonly IBaseRepository<CreditSaleDetails> _CreditSaleDetailsRepository;
        private readonly IBaseRepository<CreditSalesSchedule> _CreditSalesScheduleRepository;
        private readonly IBaseRepository<Stock> _StockRepository;
        private readonly IBaseRepository<StockDetail> _StockDetailRepository;

        private readonly IBaseRepository<ExpenseItem> _ExpenseItemRepository;
        private readonly IBaseRepository<Expenditure> _ExpenditureRepository;
        private readonly IBaseRepository<Bank> _BankRepository;
        private readonly IBaseRepository<BankTransaction> _BankTransactionRepository;
        private readonly IBaseRepository<ROrder> _ROrderRepository;
        private readonly IBaseRepository<ROrderDetail> _ROrderDetailRepository;
        private readonly IBaseRepository<PrevBalance> _previousBalanceRepository;
        private readonly IBaseRepository<ShareInvestment> _shareInvestmentRepository;
        private readonly IBaseRepository<ShareInvestmentHead> _ShareInvestmentHeadRepository;
        private readonly IBaseRepository<PaymentOptionDetailsForSale> _paymentDetailsRepository;
        private readonly IBaseRepository<PaymentOption> _paymentHeadRepository;

        private readonly IUnitOfWork _unitOfWork;


        public CashCollectionService(IBaseRepository<CashCollection> baseCashCollectionRepository,
            IBaseRepository<BankTransaction> BaseBankTransactionRepository,
             IBaseRepository<Bank> BaseBankRepository, IBaseRepository<SisterConcern> SisterConcernRepository,
            ICashCollectionRepository cashCollectionRepository, IBaseRepository<Customer> customerRepository, IBaseRepository<Employee> EmployeeRepository,
            IBaseRepository<Supplier> supplierRepository,

                IBaseRepository<POrder> POrderRepository,
            IBaseRepository<POrderDetail> POrderDetailRepository,
            IBaseRepository<SOrder> SOrderRepository,
            IBaseRepository<SOrderDetail> SOrderDetailRepository,

             IBaseRepository<CreditSale> CreditSaleRepository,
         IBaseRepository<CreditSaleDetails> CreditSaleDetailsRepository,
         IBaseRepository<CreditSalesSchedule> CreditSalesScheduleRepository,
         IBaseRepository<Stock> StockRepository,
         IBaseRepository<StockDetail> StockDetailRepository,

          IBaseRepository<ExpenseItem> ExpenseItemRepository,
         IBaseRepository<Expenditure> ExpenditureRepository,
          IBaseRepository<Bank> BankRepository,
          IBaseRepository<BankTransaction> BankTransactionRepository,

            IBaseRepository<ROrder> ROrderRepository,
          IBaseRepository<ROrderDetail> ROrderDetailRepository,
            IUnitOfWork unitOfWork, IBaseRepository<PrevBalance> previousBalanceRepository, IBaseRepository<ShareInvestment> shareInvestmentRepository, IBaseRepository<ShareInvestmentHead> shareInvestmentHeadRepository, IBaseRepository<PaymentOptionDetailsForSale> paymentDetailsRepository,
            IBaseRepository<PaymentOption> paymentHeadRepository)
        {
            _BaseCashCollectionRepository = baseCashCollectionRepository;
            _cashCollectionRepository = cashCollectionRepository;
            _SupplierRepository = supplierRepository;
            _CustomerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _EmployeeRepository = EmployeeRepository;
            _BaseBankTransactionRepository = BaseBankTransactionRepository;
            _BaseBankRepository = BaseBankRepository;
            _SisterConcernRepository = SisterConcernRepository;
            _SOrderRepository = SOrderRepository;

            _POrderDetailRepository = POrderDetailRepository;
            _POrderRepository = POrderRepository;
            _SOrderDetailRepository = SOrderDetailRepository;
            _SOrderRepository = SOrderRepository;

            _CreditSaleRepository = CreditSaleRepository;
            _CreditSaleDetailsRepository = CreditSaleDetailsRepository;
            _CreditSalesScheduleRepository = CreditSalesScheduleRepository;
            _StockRepository = StockRepository;
            _StockDetailRepository = StockDetailRepository;

            _ExpenseItemRepository = ExpenseItemRepository;
            _ExpenditureRepository = ExpenditureRepository;
            _BankRepository = BankRepository;
            _BankTransactionRepository = BankTransactionRepository;

            _ROrderRepository = ROrderRepository;
            _ROrderDetailRepository = ROrderDetailRepository;
            _previousBalanceRepository = previousBalanceRepository;
            _shareInvestmentRepository = shareInvestmentRepository;
            _ShareInvestmentHeadRepository = shareInvestmentHeadRepository;
            _paymentDetailsRepository = paymentDetailsRepository;
            _paymentHeadRepository = paymentHeadRepository;


        }

        public void AddCashCollection(CashCollection cashCollection)
        {
            _BaseCashCollectionRepository.Add(cashCollection);
        }

        public void UpdateCashCollection(CashCollection cashCollection)
        {
            _BaseCashCollectionRepository.Update(cashCollection);
        }

        public void SaveCashCollection()
        {
            _unitOfWork.Commit();
        }

        public void UpdateTotalDue(int CustomerID, int SupplierID, int BankID, int BankWithdrawID, decimal TotalDue)
        {
            _cashCollectionRepository.UpdateTotalDue(CustomerID, SupplierID, BankID, BankWithdrawID, TotalDue);
        }

        public IEnumerable<CashCollection> GetAllCashCollection()
        {
            return _BaseCashCollectionRepository.All.ToList();
        }
        public IQueryable<CashCollection> GetAllIQueryable()
        {
            return _BaseCashCollectionRepository.All;
        }
        public async Task<IEnumerable<Tuple<int, DateTime, string, string,
         string, string, string, Tuple<string, string>>>> GetAllCashCollAsync(DateTime fromDate, DateTime toDate)
        {
            return await _BaseCashCollectionRepository.GetAllCashCollAsync(_CustomerRepository, fromDate, toDate);
        }

        public async Task<IEnumerable<Tuple<int, DateTime, string, string,
        string, string, string>>> GetAllCashDelivaeryAsync(DateTime fromDate, DateTime toDate)
        {
            return await _BaseCashCollectionRepository.GetAllCashDelivaeryAsync(_SupplierRepository, fromDate, toDate);
        }

        public async Task<IEnumerable<CashCollection>> GetAllCashCollectionAsync()
        {
            return await _BaseCashCollectionRepository.GetAllCashCollectionAsync();
        }

        public CashCollection GetCashCollectionById(int id)
        {
            return _BaseCashCollectionRepository.FindBy(x => x.CashCollectionID == id).First();
        }

        public void DeleteCashCollection(int id)
        {
            _BaseCashCollectionRepository.Delete(x => x.CashCollectionID == id);
        }

        public IEnumerable<Tuple<DateTime, string, string, string, decimal, decimal, decimal, Tuple<decimal, string, string, string, string, string>>>
        GetCashCollectionData(DateTime fromDate, DateTime toDate, int ConcernId, int CustomerID, int reportType)
        {
            return _BaseCashCollectionRepository.GetCashCollectionData(_CustomerRepository, fromDate, toDate, ConcernId, CustomerID, reportType);
        }

        public IEnumerable<Tuple<DateTime, string, string, string, decimal, decimal, decimal, Tuple<decimal, string, string, string, string, string>>>
        GetCashDeliveryData(DateTime fromDate, DateTime toDate, int ConcernId, int SupplierID, int reportType)
        {
            return _BaseCashCollectionRepository.GetCashDeliveryData(_SupplierRepository, fromDate, toDate, ConcernId, SupplierID, reportType);
        }



        public IEnumerable<DailyCashBookLedgerModel> DailyCashBookLedger(DateTime fromDate, DateTime toDate, int ConcernID)
        {
            return _cashCollectionRepository.DailyCashBookLedger(fromDate, toDate, ConcernID);
        }

        public async Task<IEnumerable<Tuple<int, DateTime, string, string, string, string, string, Tuple<string, string>>>> GetAllCashCollByEmployeeIDAsync(int EmployeeID)
        {
            return await _BaseCashCollectionRepository.GetAllCashCollByEmployeeIDAsync(_CustomerRepository, EmployeeID);
        }


        public IEnumerable<Tuple<DateTime, string, string, string, decimal, decimal, decimal, Tuple<decimal, string, string, string, string, string, string>>> GetSRWiseCashCollectionReportData(DateTime fromDate, DateTime toDate, int concernID, int EmployeeID)
        {
            return _BaseCashCollectionRepository.GetSRWiseCashCollectionReportData(_CustomerRepository, _EmployeeRepository, _BaseBankRepository, _BaseBankTransactionRepository, fromDate, toDate, concernID, EmployeeID);
        }

        public void UpdateTotalDueWhenEdit(int CustomerID, int SupplierID, int BankTransactionID, int CashCollectionID, decimal TotalRecAmt)
        {
            _cashCollectionRepository.UpdateTotalDueWhenEdit(CustomerID, SupplierID, BankTransactionID, CashCollectionID, TotalRecAmt);
        }
        public IQueryable<CashCollectionReportModel> AdminCashCollectionReport(DateTime fromDate, DateTime toDate, int ConcernID)
        {
            return _BaseCashCollectionRepository.AdminCashCollectionReport(_CustomerRepository, _SisterConcernRepository, fromDate, toDate, ConcernID);
        }

        public IEnumerable<CashInHandReportModel> CashInHandReport(DateTime fromDate, DateTime toDate, int ReportType, int ConcernID, int CustomerType)
        {
            return _cashCollectionRepository.CashInHandReport(fromDate, toDate, ReportType, ConcernID, CustomerType);
        }
        public bool IsCommissionApplicable(DateTime fromDate, DateTime toDate, int EmployeeID)
        {
            return _BaseCashCollectionRepository.IsCommissionApplicable(_CustomerRepository, _BaseBankTransactionRepository, _SOrderRepository, fromDate, toDate, EmployeeID);
        }

        public List<CashInHandModel> CashInHandReport(DateTime fromDate, DateTime toDate, int ConcernID)
        {
            return _BaseCashCollectionRepository.CashInHandReport(



                _POrderRepository,
                _POrderDetailRepository,
                _SOrderRepository,
                _SOrderDetailRepository,
                _CreditSaleRepository,
                _CreditSaleDetailsRepository,
                _CreditSalesScheduleRepository,

                _StockRepository,
                _StockDetailRepository,
                 _ExpenseItemRepository,

               _ExpenditureRepository,
               _BankRepository,
               _BankTransactionRepository,

              _ROrderRepository,
             _ROrderDetailRepository,

                 _CustomerRepository,



                _SisterConcernRepository,


                fromDate,
                toDate,
                ConcernID
                );
        }



        public List<CashInHandModel> ProfitAndLossReport(DateTime fromDate, DateTime toDate, int ConcernID)
        {
            return _BaseCashCollectionRepository.ProfitAndLossReport(



                _POrderRepository,
                _POrderDetailRepository,
                _SOrderRepository,
                _SOrderDetailRepository,
                _CreditSaleRepository,
                _CreditSaleDetailsRepository,
                _CreditSalesScheduleRepository,

                _StockRepository,
                _StockDetailRepository,
                 _ExpenseItemRepository,

               _ExpenditureRepository,
               _BankRepository,
               _BankTransactionRepository,
                _ROrderRepository,
             _ROrderDetailRepository,
                 _CustomerRepository,



                _SisterConcernRepository,


                fromDate,
                toDate,
                ConcernID
                );
        }
        public List<SummaryReportModel> SummaryReport(DateTime fromDate, DateTime toDate, decimal OpeningCashInHand, decimal CurrentCashInHand, decimal ClosingCashInHand, int ConcernID)
        {
            return _BaseCashCollectionRepository.SummaryReport(



                _POrderRepository,
                _POrderDetailRepository,
                _SOrderRepository,
                _SOrderDetailRepository,
                _CreditSaleRepository,
                _CreditSaleDetailsRepository,
                _CreditSalesScheduleRepository,

                _StockRepository,
                _StockDetailRepository,
                 _ExpenseItemRepository,

               _ExpenditureRepository,
               _BankRepository,
               _BankTransactionRepository,

                 _CustomerRepository,



                _SisterConcernRepository,


                fromDate,
                toDate,
               OpeningCashInHand,
               CurrentCashInHand,
               ClosingCashInHand,
                ConcernID
                );
        }


        public List<TransactionReportModel> MonthlyTransactionReport(DateTime fromDate, DateTime toDate, int ConcernID)
        {
            return _BaseCashCollectionRepository.MonthlyTransactionReport(
               _POrderRepository,
               _POrderDetailRepository,
               _SOrderRepository,
               _SOrderDetailRepository,
               _CreditSaleRepository,
               _CreditSaleDetailsRepository,
               _CreditSalesScheduleRepository,
               _StockRepository,
               _StockDetailRepository,
               _ExpenseItemRepository,
               _ExpenditureRepository,
               _BankRepository,
               _BankTransactionRepository,
               _ROrderRepository,
               _ROrderDetailRepository,
               _CustomerRepository,
               _SisterConcernRepository,
                fromDate,
                toDate,
                ConcernID
                );
        }


        public List<TransactionReportModel> MonthlyAdminTransactionReport(DateTime fromDate, DateTime toDate, int ConcernID)
        {
            return _BaseCashCollectionRepository.MonthlyAdminTransactionReport(_POrderRepository,
               _POrderDetailRepository,
               _SOrderRepository,
               _SOrderDetailRepository,
               _CreditSaleRepository,
               _CreditSaleDetailsRepository,
               _CreditSalesScheduleRepository,
               _StockRepository,
               _StockDetailRepository,
               _ExpenseItemRepository,
               _ExpenditureRepository,
               _BankRepository,
               _BankTransactionRepository,
               _ROrderRepository,
               _ROrderDetailRepository,
               _CustomerRepository,
               _SisterConcernRepository,
                fromDate,
                toDate,
                ConcernID
                );
        }



        public List<RPTPayRecTO> GetReceiptPaymentReportNew(DateTime fromDate, DateTime toDate)
        {
            return _BaseCashCollectionRepository.GetReceiptPaymentReportNew(_POrderRepository,
               _POrderDetailRepository,
               _SOrderRepository,
               _SOrderDetailRepository,
               _CreditSaleRepository,
               _CreditSaleDetailsRepository,
               _CreditSalesScheduleRepository,
               _StockRepository,
               _StockDetailRepository,
               _ExpenseItemRepository,
               _ExpenditureRepository,
               _BankRepository,
               _BankTransactionRepository,
               _ROrderRepository,
               _ROrderDetailRepository,
               _CustomerRepository,
               _SisterConcernRepository, _SupplierRepository, _ExpenseItemRepository, _previousBalanceRepository, _shareInvestmentRepository, _ShareInvestmentHeadRepository, _SupplierRepository, _paymentHeadRepository, _paymentDetailsRepository, fromDate, toDate);
        }




        public void UpdateTotalDueForInvestment(int SIHID, int SIHId, int BankID, int BankWithdrawID, decimal TotalDue)
        {
            _cashCollectionRepository.UpdateTotalDueForInvestment(SIHID, SIHId, BankID, BankWithdrawID, TotalDue);
        }

        public void UpdateTotalDueForExpenditure(int ExpenseItemID, int BankID, int BankWithdrawID, decimal TotalDue, int ConcernID, int userId, string Remarks, DateTime EntryDate)
        {
            _cashCollectionRepository.UpdateTotalDueForExpenditure(ExpenseItemID, BankID, BankWithdrawID, TotalDue, ConcernID, userId, Remarks, EntryDate);
        }
        public IEnumerable<Tuple<DateTime, string, string, string, decimal, decimal, decimal,
        Tuple<decimal, string, string, string, string, string, EnumCustomerType>>>
        GetCashCollectionDataFoAll(DateTime fromDate, DateTime toDate, int ConcernId, int CustomerID, EnumCustomerType customerType)
        {
            return _BaseCashCollectionRepository.GetCashCollectionDataForAll(_CustomerRepository, fromDate, toDate, ConcernId, CustomerID, customerType);
        }

    }
}
