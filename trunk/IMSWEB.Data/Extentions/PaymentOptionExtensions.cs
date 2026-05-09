using IMSWEB.Model;
using IMSWEB.Model.TO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Data
{
    public static class PaymentOptionExtensions
    {
        public static async Task<IEnumerable<PaymentOption>> GetAllAsync(this IBaseRepository<PaymentOption> _paymentOptionRepository)
        {
            return await _paymentOptionRepository.All.ToListAsync();
        }

         
         
        public static IEnumerable<PaymentOptionDetailsTO> GetMultipaymentDetailsId(
                                                          this IBaseRepository<PaymentOption> paymentOptionsRepository, IBaseRepository<SOrder> salesOrderRepository, IBaseRepository<CreditSale> CreditSaleRepository, IBaseRepository<CashCollection> CashCollectionRepository, IBaseRepository<PaymentOptionDetailsForSale> paymentDetailsForSaleRepository, int Id
          )
        
        {
            var oPaymentDetailSOrderData = (from PT in paymentOptionsRepository.All
                                            join PD in paymentDetailsForSaleRepository.All on PT.Id equals PD.PaymentOptionId
                                            join SO in salesOrderRepository.All on PD.SalesOrderId equals SO.SOrderID
                                            where (PD.SalesOrderId == Id)
                                            select new PaymentOptionDetailsTO
                                            {                                                  
                                                PaymentOptionId = PT.Id,                                                 
                                                Name = PT.Name,
                                                PaidAmount  = PD.PaidAmount,
                                                bankID = PD.BankId,
                                                ChecqueNo = PD.ChequeNo
                                            }).ToList();
           

            return oPaymentDetailSOrderData;
        }



        public static IEnumerable<PTypeCashCollectionDetailsReportModel> GetMultipaymentDetails(
                                                           this IBaseRepository<PaymentOption> paymentOptionsRepository, IBaseRepository<SOrder> salesOrderRepository, IBaseRepository<CreditSale> CreditSaleRepository, IBaseRepository<CashCollection> CashCollectionRepository, IBaseRepository<PaymentOptionDetailsForSale> paymentDetailsForSaleRepository, 
                                                           DateTime fromDate, DateTime toDate, int PaymentOptionId
           )
        {
            var oPaymentDetailSOrderData = (from PT in paymentOptionsRepository.All 
                                    join PD in paymentDetailsForSaleRepository.All on PT.Id equals PD.PaymentOptionId
                                    join SO in salesOrderRepository.All on PD.SalesOrderId equals SO.SOrderID                       where (SO.InvoiceDate >= fromDate && SO.InvoiceDate <= toDate && SO.Status == (int)EnumSalesType.Sales && PT.Id == PaymentOptionId && SO.IsReplacement != 1)
                                    select new PTypeCashCollectionDetailsReportModel
                                    {
                                        CustomerID = SO.CustomerID,
                                        CustomerName = SO.Customer.Name,
                                        CustomerCode = SO.Customer.Code,
                                        CustomerAddress = SO.Customer.Address,
                                        CustomerContactNo = SO.Customer.ContactNo,
                                        CustCompanyName = SO.Customer.CompanyName,                                       
                                        SOrderID = SO.SOrderID,
                                        InvoiceNo = SO.InvoiceNo,
                                        InvoiceDate = SO.InvoiceDate,
                                        //Grandtotal = SO.GrandTotal,                                        
                                        RecAmount = (decimal)PD.PaidAmount,                                        
                                        CustomerType = SO.Customer.CustomerType,
                                        CustomerNID = SO.Customer.NID,
                                        PaymentTypeName = PT.Name,                                        
                                    }).OrderByDescending(x => x.SOrderID).ToList();

            var oCashCollectionDetailData  = (from PT in paymentOptionsRepository.All
                                          join PD in paymentDetailsForSaleRepository.All on PT.Id equals PD.PaymentOptionId
                                          join C in CashCollectionRepository.All on PD.CashCollectionId equals C.CashCollectionID
                                          where (C.EntryDate >= fromDate && C.EntryDate <= toDate && PT.Id == PaymentOptionId)
                                          select new PTypeCashCollectionDetailsReportModel
                                          {
                                              CustomerID = C.Customer.CustomerID,
                                              CustomerName = C.Customer.Name,
                                              CustomerCode = C.Customer.Code,
                                              CustomerAddress = C.Customer.Address,
                                              CustomerContactNo = C.Customer.ContactNo,
                                              CustCompanyName = C.Customer.CompanyName,                                      
                                              CashCollectionId= C.CashCollectionID,
                                              InvoiceNo = C.ReceiptNo,
                                              InvoiceDate = (DateTime)C.EntryDate,           
                                              RecAmount = (decimal)PD.PaidAmount,                                             
                                              CustomerType = C.Customer.CustomerType,
                                              CustomerNID = C.Customer.NID,
                                              PaymentTypeName = PT.Name,
                                          }).OrderByDescending(x => x.CashCollectionId).ToList();

            oPaymentDetailSOrderData.AddRange(oCashCollectionDetailData);

            return oPaymentDetailSOrderData;
        }



        public static IEnumerable<PaymentOptionDetailsTO> GetAllPaymentDetails(this IBaseRepository<PaymentOptionDetailsForSale> _employeeRepository, int id)
        {
            List<PaymentOptionDetailsTO> data = (from e in _employeeRepository.GetAll()
                                                      where e.CashCollectionId == id
                                                      select new PaymentOptionDetailsTO
                                                      {
                                                          Id = e.Id,
                                                          bankID = e.BankId,
                                                          PaidAmountAfterCharge = e.PaidAmountAfterCharge
                                                      }).ToList(); 

            return data;
        }



        public static IQueryable<MultiPaymentOptionTO> GetAllMultiPaymentsOptions(this IBaseRepository<PaymentOption> paymentOptionRepository,
     IBaseRepository<Bank> bankRepository,
     int id)
        {
            var data = from e in paymentOptionRepository.GetAll()
                       join b in bankRepository.All on e.PaymentBankID equals b.BankID
                       where e.Id == id
                       select new MultiPaymentOptionTO
                       {
                           Id = e.Id,
                           Code = e.Code,
                           Name = e.Name,
                           Charge = e.Charge,
                           PaymentBankID = b.BankID,
                           BankName = b.BankName
                       };
            //var test = products.Where(p => p.ProductID == 200675).ToList();
            return data;
        }


    }
}
