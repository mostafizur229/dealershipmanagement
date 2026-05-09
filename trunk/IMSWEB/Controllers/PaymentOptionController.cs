
using AutoMapper;
using IMSWEB.Model;
using IMSWEB.Model.TO;
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
    [RoutePrefix("payment-option")]
    public class PaymentOptionController : CoreController
    {
        private readonly IPaymentOptionService _paymentOptionService;
        IMiscellaneousService<PaymentOption> _miscellaneousService;
        IMapper _mapper;

        public PaymentOptionController(IErrorService errorService,
            IPaymentOptionService paymentOptionService, IMiscellaneousService<PaymentOption> miscellaneousService, IMapper mapper)
            : base(errorService)
        {
            _paymentOptionService = paymentOptionService;
            _miscellaneousService = miscellaneousService;
            _mapper = mapper;


        }

        [HttpGet]
        [Authorize]
        [Route("index")]
        public async Task<ActionResult> Index()
        {
            var payOptionsAsync = _paymentOptionService.GetAllAsync();
            var vmodel = _mapper.Map<IEnumerable<PaymentOption>, IEnumerable<PaymentOptionVM>>(await payOptionsAsync);
            return View(vmodel);
        }

        [HttpGet]
        [Authorize]
        [Route("create")]
        public ActionResult Create()
        {
            string code = _miscellaneousService.GetUniqueKey(x => int.Parse(x.Code));
            return View(new PaymentOptionVM { Code = code });
        }

        [HttpPost]
        [Authorize]
        [Route("create/returnUrl")]
        public ActionResult Create(PaymentOptionVM newOption, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(newOption);
            int concernId = User.Identity.GetConcernId();
            newOption.ConcernId = concernId;

            if (newOption != null)
            {
                var existingOption = _miscellaneousService.GetDuplicateEntry(c => c.Name == newOption.Name);
                if (existingOption != null)
                {
                    AddToastMessage("", "A Payment option with same name already exists in the system. Please try with a different.", ToastType.Error);
                    return View(newOption);
                }


                var paymentOption = _mapper.Map<PaymentOptionVM, PaymentOption>(newOption);

                AddAuditTrail(paymentOption, true);

                _paymentOptionService.Add(paymentOption);
                
                if (_paymentOptionService.Save())
                    AddToastMessage("", "Payment option has been save successfully.", ToastType.Success);
                
                return RedirectToAction("Create");
            }
            else
            {
                AddToastMessage("", "No Payment option found to create.", ToastType.Error);
                return RedirectToAction("Create");
            }
        }




        [HttpGet]
        [Authorize]
        [Route("edit/{id}")]
        public ActionResult Edit(int id)
        {
            PaymentOptionVM vmodel = null;

            var paymentOptionQuery = _paymentOptionService.GetAllPaymentOption(id);
            if (paymentOptionQuery == null || !paymentOptionQuery.Any())
            {
                var paymentOption = _paymentOptionService.GetById(id);
                vmodel = _mapper.Map<PaymentOption, PaymentOptionVM>(paymentOption);
            }
            else
            {
                var paymentOption = paymentOptionQuery.FirstOrDefault();
                vmodel = _mapper.Map<MultiPaymentOptionTO, PaymentOptionVM>(paymentOption);
            }
            return View("Create", vmodel);
        }


        [HttpPost]
        [Authorize]
        [Route("edit/returnUrl")]
        public ActionResult Edit(PaymentOptionVM newPaymentOption, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View("Create", newPaymentOption);
                

            if (newPaymentOption != null)
            {
                var paymentOption = _paymentOptionService.GetById(newPaymentOption.Id);

                paymentOption.Code = newPaymentOption.Code;
                paymentOption.Name = newPaymentOption.Name;
                paymentOption.Charge = newPaymentOption.Charge;
                paymentOption.PaymentBankID = newPaymentOption.PaymentBankID;

                AddAuditTrail(paymentOption, false);

                _paymentOptionService.Update(paymentOption);
                if (_paymentOptionService.Save())
                    AddToastMessage("", "Payment Option has been updated successfully.", ToastType.Success);
                return RedirectToAction("Index");
            }
            else
            {
                AddToastMessage("", "No Payment Option found to update.", ToastType.Error);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Authorize]
        [Route("delete/{id}")]
        public ActionResult Delete(int id)
        {
            _paymentOptionService.Delete(id);
            if (_paymentOptionService.Save())
                AddToastMessage("", "Payment Option has been deleted successfully.", ToastType.Success);
            return RedirectToAction("Index");
        }

    }
}