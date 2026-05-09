using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Model

{
    public class PaySlip
    {
        public string Allowance { get; set; }
        public decimal AllowanceAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Deduction { get; set; }
        public decimal DeductionAmount { get; set; }
    }
}
