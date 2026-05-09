using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Model
{
    public class PaymentOptionDetailsForSale
    {
        [Key]
        public int Id { get; set; }
        public int? SalesOrderId { get; set; }
        public int? CashCollectionId { get; set; }
        public int BankId { get; set; } 
        public bool IsSales { get; set; }      
        public bool IsCashCollection { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Charge { get; set; }
        public int PaymentOptionId { get; set; }
        public decimal PaidAmountAfterCharge { get; set; }    

        public string ChequeNo { get; set; }
        [ForeignKey("PaymentOptionId")]
        public virtual PaymentOption PaymentOption { get; set; }
 
    }
}
