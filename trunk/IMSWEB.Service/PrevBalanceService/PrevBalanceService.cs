using IMSWEB.Data;
using IMSWEB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Service
{
    public class PrevBalanceService : IPrevBalanceService
    {
        private readonly IBaseRepository<PrevBalance> _PrevBalanceRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IBaseRepository<POrder> _pOrderRepository;
        private readonly IBaseRepository<Expenditure> _expenditureRepository;
        private readonly IBaseRepository<SOrder> _baseSalesOrderRepository;
        private readonly IBaseRepository<CashCollection> _cashCollectionRepository;
        private readonly IBaseRepository<ExpenseItem> _ExpenseItemRepository;
        private readonly IBaseRepository<CreditSale> _CreditSaleRepository;
        private readonly IBaseRepository<CreditSalesSchedule> _CreditSalesScheduleRepository;
        private readonly IBaseRepository<BankTransaction> _BankTransactionRepository;
        private readonly IBaseRepository<CustomerOpeningDue> _CustomerOpeningDueRepository;
        private readonly IBaseRepository<Customer> _CustomerRepository;
        private readonly IBaseRepository<ROrder> _ROrderRepository;
        private readonly IBaseRepository<ShareInvestment> _ShareInvestmentRepository;
        private readonly IBaseRepository<ShareInvestmentHead> _ShareInvestmentHeadRepository;
        private readonly IBaseRepository<PaymentOptionDetailsForSale> _paymentDetailsRepository;
        private readonly IBaseRepository<PaymentOption> _paymentHeadRepository;



        public PrevBalanceService(IBaseRepository<PrevBalance> prevBalanceRepository, IBaseRepository<SOrder> baseSalesOrderRepository,
            IBaseRepository<CashCollection> cashCollectionRepository, IBaseRepository<POrder> PorderRepository, IBaseRepository<ExpenseItem> ExpenseItemRepository,
            IBaseRepository<Expenditure> expenditureRepository, IBaseRepository<CreditSale> CreditSaleRepository,
            IBaseRepository<CreditSalesSchedule> CreditSalesScheduleRepository, IBaseRepository<BankTransaction> BankTransactionRepository,
            IUnitOfWork unitOfWork, IBaseRepository<CustomerOpeningDue> CustomerOpeningDueRepository,
            IBaseRepository<Customer> CustomerRepository, IBaseRepository<ROrder> ROrderRepository, IBaseRepository<ShareInvestment> ShareInvestmentRepository,
            IBaseRepository<ShareInvestmentHead> ShareInvestmentHeadRepository, IBaseRepository<PaymentOptionDetailsForSale> paymentDetailsRepository,
            IBaseRepository<PaymentOption> paymentHeadRepository)
        {
            _PrevBalanceRepository = prevBalanceRepository;
            _baseSalesOrderRepository = baseSalesOrderRepository;
            _cashCollectionRepository = cashCollectionRepository;
            _pOrderRepository = PorderRepository;
            _unitOfWork = unitOfWork;
            _ExpenseItemRepository = ExpenseItemRepository;
            _expenditureRepository = expenditureRepository;
            _CreditSaleRepository = CreditSaleRepository;
            _CreditSalesScheduleRepository = CreditSalesScheduleRepository;
            _BankTransactionRepository = BankTransactionRepository;
            _CustomerOpeningDueRepository = CustomerOpeningDueRepository;
            _CustomerRepository = CustomerRepository;
            _ROrderRepository = ROrderRepository;
            _ShareInvestmentRepository = ShareInvestmentRepository;
            _ShareInvestmentHeadRepository = ShareInvestmentHeadRepository;
            _paymentDetailsRepository = paymentDetailsRepository;
            _paymentHeadRepository = paymentHeadRepository;
        }
        public void AddPrevBalance(PrevBalance model)
        {
            _PrevBalanceRepository.Add(model);
        }



        public void UpdatePrevBalance(PrevBalance model)
        {
            _PrevBalanceRepository.Update(model);
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }

        public IEnumerable<PrevBalance> GetAllPrevBalance()
        {
            return _PrevBalanceRepository.GetAll();
        }

        //public async Task<IEnumerable<PrevBalance>> GetAllPrevBalancelAsync()
        //{
        //    return await _PrevBalanceRepository.GetAllPrevBalancelAsync();
        //}

        public PrevBalance GetPrevBalanceById(int id)
        {
            return _PrevBalanceRepository.FindBy(i => i.ID == id).FirstOrDefault();
        }

        public void DeletePrevBalance(int id)
        {
            _PrevBalanceRepository.Delete(i => i.ID == id);
        }
        public List<PrevBalance> DailyBalanceProcess(int ConcernID)
        {
            return _PrevBalanceRepository.DailyBalanceProcess(_baseSalesOrderRepository, _pOrderRepository, _cashCollectionRepository,
                _expenditureRepository, _ExpenseItemRepository, _CreditSaleRepository, _CreditSalesScheduleRepository, _BankTransactionRepository, _ROrderRepository, _ShareInvestmentRepository, _ShareInvestmentHeadRepository, _paymentDetailsRepository, _paymentHeadRepository, ConcernID);
        }
    }
}
