using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Model.TO
{
    public class MultiPaymentOptionTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Charge { get; set; }
        public int PaymentBankID { get; set; }
        public string BankName { get; set; }

    }
}
