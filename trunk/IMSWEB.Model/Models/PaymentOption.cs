using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Model
{
    public class PaymentOption : AuditTrailModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Charge { get; set; }
        public int ConcernId { get; set; }
        public int PaymentBankID { get; set; }
        
    }
}
