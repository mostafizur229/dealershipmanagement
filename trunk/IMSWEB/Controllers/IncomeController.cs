using AutoMapper;
using IMSWEB.Model;
using IMSWEB.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Web;
using System.Web.Mvc;

namespace IMSWEB.Controllers
{
    public class IncomeController : CoreController
    {
        IExpenditureService _expenditureService;
        IExpenseItemService _expenseItemService;
        IMapper _mapper;
        IMiscellaneousService<Expenditure> _miscellService;

        public IncomeController(IErrorService errorService,
            IExpenditureService expenditureService, IExpenseItemService expenseItemService, IMapper mapper, IMiscellaneousService<Expenditure> miscellService)
            : base(errorService)
        {
            _expenditureService = expenditureService;
            _expenseItemService = expenseItemService;
            _mapper = mapper;
            _miscellService = miscellService;

        }
        public async Task<ActionResult> Index()
        {
            var DateRange = GetFirstAndLastDateOfMonth(DateTime.Today);
            ViewBag.FromDate = DateRange.Item1;
            ViewBag.ToDate = DateRange.Item2;

            var itemsAsync = _expenditureService.GetAllIncomeAsync(ViewBag.FromDate, ViewBag.ToDate);
            var vmodel = _mapper.Map<IEnumerable<Expenditure>, IEnumerable<CreateExpenditureViewModel>>(await itemsAsync);
            return View(vmodel);
        }

        [HttpPost]
        public async Task<ActionResult> Index(FormCollection formCollection)
        {
            if (!string.IsNullOrEmpty(formCollection["FromDate"]))
                ViewBag.FromDate = Convert.ToDateTime(formCollection["FromDate"]);
            if (!string.IsNullOrEmpty(formCollection["ToDate"]))
                ViewBag.ToDate = Convert.ToDateTime(formCollection["ToDate"]);

            var itemsAsync = _expenditureService.GetAllIncomeAsync(ViewBag.FromDate, ViewBag.ToDate);
            var vmodel = _mapper.Map<IEnumerable<Expenditure>, IEnumerable<CreateExpenditureViewModel>>(await itemsAsync);
            return View(vmodel);
        }

        [HttpGet]
        [Authorize]
        [Route("create")]
        public ActionResult Create()
        {
            var voucherNo = _miscellService.GetUniqueKey(i => i.ExpenditureID);
            return View(new CreateExpenditureViewModel() { VoucherNo = voucherNo });
        }

        [HttpPost]
        [Authorize]
        [Route("create/returnUrl")]
        public ActionResult Create(CreateExpenditureViewModel newIncome, FormCollection formcollection, string returnUrl)
        {
            CheckAndAddModelError(newIncome, formcollection);

            if (!ModelState.IsValid)
                return View(newIncome);

            if (newIncome != null)
            {
                newIncome.CreateDate = DateTime.Today.ToString();
                newIncome.CreatedBy = (User.Identity.GetUserId<string>());
                newIncome.ConcernID = User.Identity.GetConcernId().ToString();
                var expenditure = _mapper.Map<CreateExpenditureViewModel, Expenditure>(newIncome);
                _expenditureService.AddExpenditure(expenditure);
                _expenditureService.SaveExpenditure();
                TempData["ExpenditureID"] = expenditure.ExpenditureID;
                AddToastMessage("", "Item has been saved successfully.", ToastType.Success);
                return RedirectToAction("Index");
            }
            else
            {
                AddToastMessage("", "No Item data found to create.", ToastType.Error);
                return RedirectToAction("Index");
            }
        }
        private void CheckAndAddModelError(CreateExpenditureViewModel newExpenditure, FormCollection formcollection)
        {
            if (!string.IsNullOrEmpty(formcollection["EntryDate"]))
                newExpenditure.EntryDate = formcollection["EntryDate"].ToString();

            if (!IsDateValid(Convert.ToDateTime(newExpenditure.EntryDate)))
                ModelState.AddModelError("EntryDate", "Back dated entry is not valid");

            if (string.IsNullOrEmpty(newExpenditure.VoucherNo))
                ModelState.AddModelError("VoucherNo", "VoucherNo is required.");

        }
        [HttpGet]
        [Authorize]
        [Route("edit/{id}")]
        public ActionResult Edit(int id)
        {
            var expenditure = _expenditureService.GetExpenditureById(id);
            var vmodel = _mapper.Map<Expenditure, CreateExpenditureViewModel>(expenditure);
            return View("Create", vmodel);
        }

        [HttpPost]
        [Authorize]
        [Route("edit/returnUrl")]
        public ActionResult Edit(CreateExpenditureViewModel newIncome, FormCollection formcollection, string returnUrl)
        {
            CheckAndAddModelError(newIncome, formcollection);

            if (!ModelState.IsValid)
                return View("Create", newIncome);

            if (newIncome != null)
            {
                var objincome = _expenditureService.GetExpenditureById(int.Parse(newIncome.Id));

                objincome.Amount = decimal.Parse(newIncome.Amount);
                objincome.Purpose = newIncome.Purpose;
                objincome.ExpenseItemID = int.Parse(newIncome.ExpenseItemID);
                objincome.ModifiedBy = User.Identity.GetUserId<int>();
                objincome.ModifiedDate = DateTime.Now;
                objincome.EntryDate = Convert.ToDateTime(newIncome.EntryDate);
                _expenditureService.UpdateExpenditure(objincome);
                _expenditureService.SaveExpenditure();
                TempData["ExpenditureID"] = objincome.ExpenditureID;
                AddToastMessage("", "Income has been updated successfully.", ToastType.Success);
                return RedirectToAction("Index");
            }
            else
            {
                AddToastMessage("", "No Item data found to update.", ToastType.Error);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Authorize]
        [Route("delete/{id}")]
        public ActionResult Delete(int id)
        {
            var model = _expenditureService.GetExpenditureById(id);
            if (!IsDateValid(model.EntryDate))
            {
                return RedirectToAction("Index");
            }
            _expenditureService.DeleteExpenditure(id);
            _expenditureService.SaveExpenditure();
            AddToastMessage("", "Item has been deleted successfully.", ToastType.Success);
            return RedirectToAction("Index");
        }


        [HttpGet]
        [Authorize]
        [Route("Expenditure-report")]
        public ActionResult MiscellaneousReport()
        {
            return View("MiscellaneousReport");
        }

        [HttpGet]
        [Authorize]
        public ActionResult MoneyReceipt(int id)
        {
            TempData["ExpenditureID"] = id;
            return RedirectToAction("Index");
        }
    }
}