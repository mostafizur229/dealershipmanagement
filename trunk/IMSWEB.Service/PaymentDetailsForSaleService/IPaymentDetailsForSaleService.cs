using IMSWEB.Model;
using IMSWEB.Model.TO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Service
{
    public interface IPaymentDetailsForSaleService
    {
        List<RPTPaymentDetailsTO> GetAllPaymentDetailsBySalesId(int salesOrderId);
        List<RPTPaymentDetailsTO> GetAllPaymentDetailsByCreditSalesId(int creditSalesOrderId);
        List<RPTPaymentDetailsTO> GetAllPaymentDetailsByCashCollectionId(int cashCollectionId);
        PaymentOptionDetailsForSale GetById(int id);
        PaymentOption GetByIds(int id);
        PaymentOption GetByIdPaymentOption(int id);
        IQueryable<PaymentOptionDetailsForSale> GetAllIQueryable();
        IQueryable<PaymentOption> GetAllIQueryable(int id, int concernId);

        //IEnumerable<PTypeCashCollectionDetailsReportModel> GetMultiPaymentDetailsById(int Id); 
        IEnumerable<PaymentOptionDetailsTO> GetMultiPaymentDetailsById(int Id); 
        IEnumerable<PTypeCashCollectionDetailsReportModel> GetMultiPaymentDetailsByPaymentId(DateTime fromDate, DateTime toDate, int PaymentId);

        void Add(PaymentOptionDetailsForSale model);
        void Update(PaymentOptionDetailsForSale model);
        bool Save();
        void Delete(int id);
        void DeleteByCashCollectionId(int id);               
        IEnumerable<PaymentOptionDetailsTO> GetPaymentDetailsCollectionId(int id);
    }
}
