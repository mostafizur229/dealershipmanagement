using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Model
{
    public class PTypeCashCollectionDetailsReportModel
    {
        public int SOrderID { get; set; }
        public int CashCollectionId { get; set; }
        public int CreditSalesID { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceNo { get; set; }
        public int CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }  
        public decimal RecAmount { get; set; }       

        public string CustomerAddress { get; set; }
        public string CustomerContactNo { get; set; }
        public string CustCompanyName { get; set; }
        public string CustomerNID { get; set; }
        public string PaymentTypeName { get; set; }
        public decimal CustomerTotalDue { get; set; }
        public EnumCustomerType CustomerType { get; set; }
        public int ConcernID { get; set; }
        public string ConcernName { get; set; }

    }
}
