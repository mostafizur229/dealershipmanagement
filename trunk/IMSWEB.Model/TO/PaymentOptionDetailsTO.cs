using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Model.TO
{
    public class PaymentOptionDetailsTO
    {
        public int Id { get; set; }
        public int PaymentOptionId { get; set; }
        public string Name { get; set; }
        public decimal Charge { get; set; }
        public decimal PaidAmount { get; set; }

        public string Description { get; set; }
        public string Status { get; set; }
        public string HeadType { get; set; }


        public string PaymentTypeName { get; set; }
        public string ChecqueNo { get; set; }

        public int bankID { get; set; }
        public int BankTranID { get; set; }
        public string CheckNo { get; set; }
        public string bankName { get; set; }
        public decimal BankBalance { get; set; }
        public decimal PaidAmountAfterCharge { get; set; }
    }
}
