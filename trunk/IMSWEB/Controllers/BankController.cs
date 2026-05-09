
using AutoMapper;
using IMSWEB.Model;
using IMSWEB.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IMSWEB.Controllers
{
    [Authorize]
    [RoutePrefix("bank")]
    public class BankController : CoreController
    {
        IBankService _bankService;
        IMiscellaneousService<Bank> _miscellaneousService;
        IMapper _mapper;

        public BankController(IErrorService errorService,
            IBankService bankService, IMiscellaneousService<Bank> miscellaneousService,
            IMapper mapper)
            : base(errorService)
        {
            _bankService = bankService;
            _miscellaneousService = miscellaneousService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize]
        [Route("index")]
        public async Task<ActionResult> Index()
        {
            var banksAsync = _bankService.GetAllBankAsync();
            var vmodel = _mapper.Map<IEnumerable<Bank>, IEnumerable<CreateBankViewModel>>(await banksAsync);
            return View(vmodel);
        }

        [HttpGet]
        [Authorize]
        [Route("create")]
        public ActionResult Create()
        {
            string code = _miscellaneousService.GetUniqueKey(x => int.Parse(x.Code));
            return View(new CreateBankViewModel { Code = code });
        }

        [HttpPost]
        [Authorize]
        [Route("create/returnUrl")]
        public ActionResult Create(CreateBankViewModel newBank, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(newBank);

            if (newBank != null)
            {
                newBank.Code = CheakAndIncrement(newBank.Code);
                var existingBank = _miscellaneousService.GetDuplicateEntry(c =>
                                    c.Code == newBank.Code &&
                                    c.BankName == newBank.BankName &&
                                    c.AccountNo == newBank.AccountNo);


                //var existingBank = _miscellaneousService.GetDuplicateEntry(c => c.BankName == newBank.BankName && c.AccountNo == newBank.AccountNo);
                if (existingBank != null)
                {
                    AddToastMessage("", "A Bank with same name and account number already exists in the system. Please try with a different.", ToastType.Error);
                    return View(newBank);
                }

                var bank = _mapper.Map<CreateBankViewModel, Bank>(newBank);

                bank.IsAdvancedDueLimitApplicable = newBank.IsAdvancedDueLimitApplicable ? 1 : 0;
                bank.AdvancedAmountLimit = newBank.AdvancedAmountLimit;

                AddAuditTrail(bank, true);
                _bankService.AddBank(bank);
                _bankService.SaveBank();

                AddToastMessage("", "Bank has been saved successfully.", ToastType.Success);
                return RedirectToAction("Create");
            }
            else
            {
                AddToastMessage("", "No Bank data found to create.", ToastType.Error);
                return RedirectToAction("Create");
            }
        }



        private bool CodeExist(string Code)
        {
            var exSuplier = _bankService.GetAllBank().FirstOrDefault(c => c.Code.Equals(Code, StringComparison.OrdinalIgnoreCase));
            return exSuplier != null;
        }


        private string CheakAndIncrement(string suplierCode)
        {
            if (CodeExist(suplierCode))
            {
                int maxCode = _bankService.GetAllBank().Select(c => int.Parse(c.Code)).Max();
                int currentCode = maxCode + 1;

                return currentCode.ToString("00000");
            }
            return suplierCode;
        }




        [HttpGet]
        [Authorize]
        [Route("edit/{id}")]
        public ActionResult Edit(int id)
        {
            var bank = _bankService.GetBankById(id);
            var vmodel = _mapper.Map<Bank, CreateBankViewModel>(bank);
            return View("Create", vmodel);
        }

        [HttpPost]
        [Authorize]
        [Route("edit/returnUrl")]
        public ActionResult Edit(CreateBankViewModel newBank, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View("Create", newBank);

            if (newBank != null)
            {
                var bank = _bankService.GetBankById(int.Parse(newBank.Id));

                bank.Code = newBank.Code;
                bank.BankName = newBank.BankName;
                bank.AccountName = newBank.AccountName;
                bank.BranchName = newBank.BranchName;
                bank.IsAdvancedDueLimitApplicable = newBank.IsAdvancedDueLimitApplicable ? 1 : 0;
                bank.AdvancedAmountLimit = newBank.AdvancedAmountLimit;
                bank.AccountNo = newBank.AccountNo;
                AddAuditTrail(bank, true);
                _bankService.UpdateBank(bank);
                _bankService.SaveBank();

                AddToastMessage("", "Bank has been updated successfully.", ToastType.Success);
                return RedirectToAction("Index");
            }
            else
            {
                AddToastMessage("", "No Bank found to update.", ToastType.Error);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Authorize]
        [Route("delete/{id}")]
        public ActionResult Delete(int id)
        {
            _bankService.DeleteBank(id);
            _bankService.SaveBank();
            AddToastMessage("", "Bank has been deleted successfully.", ToastType.Success);
            return RedirectToAction("Index");
        }


        public JsonResult GetBankByName(string prefix)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                var banks = from c in _bankService.GetAllBank()
                            where c.BankName.ToLower().Contains(prefix.ToLower())
                            select new
                            {
                                ID = c.BankID,
                                Name = c.BankName
                            };
                if (banks.Count() > 0)
                    return Json(banks, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var banks = (from c in _bankService.GetAllBank()
                             select new
                             {
                                 ID = c.BankID,
                                 Name = c.BankName
                             }).Take(10);
                if (banks.Count() > 0)
                    return Json(banks, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);

        }
    }

}