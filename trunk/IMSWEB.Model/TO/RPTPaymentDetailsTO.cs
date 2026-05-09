using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Model.TO
{
    public class RPTPaymentDetailsTO
    {
        public string PaymentTypeName { get; set; }
        public decimal PaidAmount { get; set; }
        public bool IsEMI { get; set; }
        public string BankName { get; set; }
    }
}
