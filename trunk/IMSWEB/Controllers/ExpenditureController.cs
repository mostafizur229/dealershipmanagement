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
    [Authorize]
    [RoutePrefix("expense-item")]
    public class ExpenditureController : CoreController
    {
        IExpenditureService _expenditureService;
        IExpenseItemService _expenseItemService;
        IMapper _mapper;
        IMiscellaneousService<Expenditure> _miscellService;
        ISisterConcernService _SisterConcernService;
        ISystemInformationService _SysInfoService;

        public ExpenditureController(IErrorService errorService,
            IExpenditureService expenditureService, IExpenseItemService expenseItemService, IMapper mapper,
            IMiscellaneousService<Expenditure> miscellService, ISisterConcernService SisterConcernService, ISystemInformationService sysInfoService)
            : base(errorService)
        {
            _expenditureService = expenditureService;
            _expenseItemService = expenseItemService;
            _mapper = mapper;
            _miscellService = miscellService;
            _SisterConcernService = SisterConcernService;
            _SysInfoService = sysInfoService;
        }

        [HttpGet]
        [Authorize]
        [Route("index")]
        public async Task<ActionResult> Index()
        {
            var DateRange = GetFirstAndLastDateOfMonth(DateTime.Today);
            ViewBag.FromDate = DateRange.Item1;
            ViewBag.ToDate = DateRange.Item2;

            if (User.IsInRole(ConstantData.ROLE_MOBILE_USER))
            {
                var expenditureList = _expenditureService.GetAllExpenditureByUserIDAsync(User.Identity.GetUserId<int>(), ViewBag.FromDate, ViewBag.ToDate);
                var vmodel = _mapper.Map<IEnumerable<Expenditure>, IEnumerable<CreateExpenditureViewModel>>(await expenditureList);
                return View(vmodel);
            }
            else
            {
                var expenditureList = _expenditureService.GetAllExpenditureAsync(ViewBag.FromDate, ViewBag.ToDate);
                var vmodel = _mapper.Map<IEnumerable<Expenditure>, IEnumerable<CreateExpenditureViewModel>>(await expenditureList);
                return View(vmodel);
            }
        }
        [HttpPost]
        [Authorize]
        [Route("index")]
        public async Task<ActionResult> Index(FormCollection formCollection)
        {
            if (!string.IsNullOrEmpty(formCollection["FromDate"]))
                ViewBag.FromDate = Convert.ToDateTime(formCollection["FromDate"]);
            if (!string.IsNullOrEmpty(formCollection["ToDate"]))
                ViewBag.ToDate = Convert.ToDateTime(formCollection["ToDate"]);

            if (User.IsInRole(ConstantData.ROLE_MOBILE_USER))
            {
                var expenditureList = _expenditureService.GetAllExpenditureByUserIDAsync(User.Identity.GetUserId<int>(), ViewBag.FromDate, ViewBag.ToDate);
                var vmodel = _mapper.Map<IEnumerable<Expenditure>, IEnumerable<CreateExpenditureViewModel>>(await expenditureList);
                return View(vmodel);
            }
            else
            {
                var expenditureList = _expenditureService.GetAllExpenditureAsync(ViewBag.FromDate, ViewBag.ToDate);
                var vmodel = _mapper.Map<IEnumerable<Expenditure>, IEnumerable<CreateExpenditureViewModel>>(await expenditureList);
                return View(vmodel);
            }
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
        public ActionResult Create(CreateExpenditureViewModel newExpenditure, FormCollection formcollection, string returnUrl)
        {
            CheckAndAddModelError(newExpenditure, formcollection);

            if (!ModelState.IsValid)
                return View(newExpenditure);

            if (newExpenditure != null)
            {
                newExpenditure.CreateDate = DateTime.Today.ToString();
                newExpenditure.CreatedBy = (User.Identity.GetUserId<string>());
                newExpenditure.ConcernID = User.Identity.GetConcernId().ToString();
                var expenditure = _mapper.Map<CreateExpenditureViewModel, Expenditure>(newExpenditure);
                _expenditureService.AddExpenditure(expenditure);
                _expenditureService.SaveExpenditure();
                TempData["POSSOrderID"] = expenditure.ExpenditureID;
                TempData["IsPOSInvoiceReady"] = true;
                TempData["IsPOSShow"] = _SysInfoService.GetSystemInformationByConcernId(User.Identity.GetConcernId()).IsPosInvoiceShow > 0 ? true : false;

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
        public ActionResult Edit(CreateExpenditureViewModel newExpenditure, FormCollection formcollection, string returnUrl)
        {
            CheckAndAddModelError(newExpenditure, formcollection);

            if (!ModelState.IsValid)
                return View("Create", newExpenditure);

            if (newExpenditure != null)
            {
                var expenditure = _expenditureService.GetExpenditureById(int.Parse(newExpenditure.Id));

                expenditure.Amount = decimal.Parse(newExpenditure.Amount);
                expenditure.Purpose = newExpenditure.Purpose;
                expenditure.ExpenseItemID = int.Parse(newExpenditure.ExpenseItemID);
                expenditure.ModifiedBy = User.Identity.GetUserId<int>();
                expenditure.ModifiedDate = DateTime.Now;
                expenditure.EntryDate = Convert.ToDateTime(newExpenditure.EntryDate);
                _expenditureService.UpdateExpenditure(expenditure);
                _expenditureService.SaveExpenditure();
                TempData["ExpenditureID"] = expenditure.ExpenditureID;
                AddToastMessage("", "Item has been updated successfully.", ToastType.Success);
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
        public ActionResult IncomeReport()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult MoneyReceipt(int id)
        {
            TempData["ExpenditureID"] = id;
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public ActionResult TrialBalance()
        {
            if (User.IsInRole(EnumUserRoles.superadmin.ToString()))
                PopulateConcernsDropdown();
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult ProfitLossAccount()
        {
            if (User.IsInRole(EnumUserRoles.superadmin.ToString()))
                PopulateConcernsDropdown();
            return View();
        }


        [HttpGet]
        [Authorize]
        public ActionResult BalanceSheet()
        {
            if (User.IsInRole(EnumUserRoles.superadmin.ToString()))
                PopulateConcernsDropdown();
            return View();
        }

        void PopulateConcernsDropdown()
        {
            ViewBag.Concerns = new SelectList(_SisterConcernService.GetAll(), "ConcernID", "Name");
        }



        [HttpGet]
        [Authorize]
        public ActionResult PosMoneyReceipt(int Id) 
        {
            TempData["POSSOrderID"] = Id;
            TempData["IsPOSInvoiceReady"] = true;
            TempData["IsPOSShow"] = _SysInfoService.GetSystemInformationByConcernId(User.Identity.GetConcernId()).IsPosInvoiceShow > 0 ? true : false;
            return RedirectToAction("Index");
        }
    }
}