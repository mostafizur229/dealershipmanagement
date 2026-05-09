using IMSWEB.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IMSWEB
{
    public class CreateExpenditureViewModel : IValidatableObject
    {
        public string Id { get; set; }
        public string EntryDate { get; set; }

      
        public string Purpose { get; set; }

        public string Amount { get; set; }

        [Display(Name = "ExpenseItem Name")]
        public string ExpenseItemID { get; set; }

        public string ConcernID { get; set; }

        public string VoucherNo { get; set; }
        
        public string CreatedBy { get; set; }

        public string CreateDate { get; set; }

        public string ModifiedBy { get; set; }
       
        public string ModifiedDate { get; set; }

        [Display(Name = "Head")]
        public string ExpenseItemName { get; set; }
        
        [Display(Name = "Expense Item")]
        public ICollection<System.Web.Mvc.SelectListItem> ExpenseItems { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validator = new CreateExpenditureViewModelValidator();
            var result = validator.Validate(this);
            return result.Errors.Select(item => new ValidationResult(item.ErrorMessage, new[] { item.PropertyName }));
        }
    }
}