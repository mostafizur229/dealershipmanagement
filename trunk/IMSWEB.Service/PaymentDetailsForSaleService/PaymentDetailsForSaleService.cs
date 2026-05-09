using IMSWEB.Data;
using IMSWEB.Model;
using IMSWEB.Model.TO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Service
{
    public class PaymentDetailsForSaleService : IPaymentDetailsForSaleService
    {
        private readonly IBaseRepository<PaymentOptionDetailsForSale> _paymentDetailsForSaleRepository;
        private readonly IBaseRepository<PaymentOption> _paymentOptionsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBaseRepository<SOrder> _salesOrderRepository;
        private readonly IBaseRepository<CreditSale> _creditSalesRepository;
        private readonly IBaseRepository<CashCollection> _cashCollectionRepository;

        public PaymentDetailsForSaleService(IBaseRepository<PaymentOptionDetailsForSale> paymentDetailsForSaleRepository, IUnitOfWork unitOfWork, IBaseRepository<PaymentOption> paymentOptionRepository, IBaseRepository<SOrder> salesOrderRepository, IBaseRepository<CreditSale> creditSalesRepository, IBaseRepository<CashCollection> cashCollectionRepository)
        {
            _paymentDetailsForSaleRepository = paymentDetailsForSaleRepository;
            _paymentOptionsRepository = paymentOptionRepository;
            _unitOfWork = unitOfWork;
            _salesOrderRepository = salesOrderRepository;
            _creditSalesRepository = creditSalesRepository;
            _cashCollectionRepository = cashCollectionRepository;
        }

        //public List<PaymentOptionDetailsForSale> GetAllPaymentDetailsBySalesId(int salesOrderId)
        //{
        //    return _paymentDetailsForSaleRepository.All.Where(d => d.IsSales && d.SalesOrderId == salesOrderId).ToList();
        //}

        public List<RPTPaymentDetailsTO> GetAllPaymentDetailsBySalesId(int salesOrderId)
        {
            string query = string.Format(@"SELECT PO.Name PaymentTypeName, PD.PaidAmount FROM PaymentOptionDetailsForSales PD
                                            JOIN PaymentOptions PO ON PD.PaymentOptionId = PO.Id
                                            WHERE PD.IsSales = 1 AND PD.SalesOrderId IS NOT NULL
                                            AND PD.SalesOrderId = {0}", salesOrderId);
            return _paymentDetailsForSaleRepository.SQLQueryList<RPTPaymentDetailsTO>(query).ToList();
        }

        public List<RPTPaymentDetailsTO> GetAllPaymentDetailsByCreditSalesId(int creditSalesOrderId)
        {
            string query = string.Format(@"SELECT PO.Name PaymentTypeName, PD.PaidAmount FROM PaymentOptionDetailsForSales PD
                                            JOIN PaymentOptions PO ON PD.PaymentOptionId = PO.Id
                                            WHERE PD.IsCreditSales = 1 AND PD.CreditSalesOrderId IS NOT NULL
                                            AND PD.CreditSalesOrderId = {0}", creditSalesOrderId);
            return _paymentDetailsForSaleRepository.SQLQueryList<RPTPaymentDetailsTO>(query).ToList();
        }

        public List<RPTPaymentDetailsTO> GetAllPaymentDetailsByCashCollectionId(int cashCollectionId)
        {
            string query = string.Format(@"SELECT PO.Name PaymentTypeName, PD.PaidAmount, PD.IsEMI, e.BankName FROM PaymentOptionDetailsForSales PD
                                            JOIN PaymentOptions PO ON PD.PaymentOptionId = PO.Id
											LEFT JOIN EMIBanks e ON PD.EMIBankId = e.Id
                                            WHERE PD.IsCashCollection = 1 AND PD.CashCollectionId IS NOT NULL
                                            AND PD.CashCollectionId = {0}", cashCollectionId);
            return _paymentDetailsForSaleRepository.SQLQueryList<RPTPaymentDetailsTO>(query).ToList();
        }


        public PaymentOptionDetailsForSale GetById(int id)
        {
            return _paymentDetailsForSaleRepository.FindBy(x => x.PaymentOptionId == id).First();
        }

        public PaymentOption GetByIdPaymentOption(int id)
        {
            return _paymentOptionsRepository.FindBy(x => x.Id == id).First();
        }

        public PaymentOption GetByIds(int id)
        {
            return _paymentOptionsRepository.FindBy(x => x.Id == id).FirstOrDefault();
        }

        public IQueryable<PaymentOptionDetailsForSale> GetAllIQueryable()
        {
            return _paymentDetailsForSaleRepository.All;
        }

        public IQueryable<PaymentOption> GetAllIQueryable(int id, int concernId)
        {
            return _paymentOptionsRepository.FindBy(x => x.Id == id && x.ConcernId == concernId).AsQueryable();
        }


        public IEnumerable<PaymentOptionDetailsTO> GetMultiPaymentDetailsById(int Id)
        {
            return _paymentOptionsRepository.GetMultipaymentDetailsId(_salesOrderRepository, _creditSalesRepository, _cashCollectionRepository, _paymentDetailsForSaleRepository, Id); 
        } 


        public IEnumerable<PTypeCashCollectionDetailsReportModel> GetMultiPaymentDetailsByPaymentId(DateTime fromDate, DateTime toDate, int PaymentId)
        {
            return _paymentOptionsRepository.GetMultipaymentDetails(_salesOrderRepository, _creditSalesRepository, _cashCollectionRepository, _paymentDetailsForSaleRepository, fromDate, toDate, PaymentId);
        }


        public void Add(PaymentOptionDetailsForSale model)
        {
            _paymentDetailsForSaleRepository.Add(model);
        }

        public void Update(PaymentOptionDetailsForSale model)
        {
            _paymentDetailsForSaleRepository.Update(model);
        }

        public void Delete(int id)
        {
            _paymentDetailsForSaleRepository.Delete(x => x.Id == id);
        }
        public void DeleteByCashCollectionId(int id)
        {
            _paymentDetailsForSaleRepository.Delete(x => x.CashCollectionId == id);
        }

        public bool Save()
        {
            try
            {
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

    

        public IEnumerable<PaymentOptionDetailsTO> GetPaymentDetailsCollectionId(int id)
        {
            return  _paymentDetailsForSaleRepository.GetAllPaymentDetails(id);
        }

    }

}
