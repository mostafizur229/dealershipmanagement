using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IMSWEB
{
    public class PaymentOptionVM
    {
        public int Id { get; set; }
        public int ConcernId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Charge { get; set; }
        public int PaymentBankID { get; set; }
        public string BankName { get; set; }
    }
}