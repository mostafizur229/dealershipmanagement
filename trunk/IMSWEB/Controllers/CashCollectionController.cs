using AutoMapper;
using IMSWEB.Model;
using IMSWEB.Model.TOs;
using IMSWEB.Service;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace IMSWEB.Controllers
{
    [Authorize]
    [RoutePrefix("Collection-item")]

    public class CashCollectionController : CoreController
    {

        ICashCollectionService _CashCollectionService;
        ICustomerService _CustomerService;
        ISupplierService _SupplierService;
        IMiscellaneousService<CashCollection> _miscellaneousService;
        IMapper _mapper;
        IUserService _UserService;
        ISisterConcernService _SisterConcern;
        ISMSStatusService _SMSService;
        ISystemInformationService _SysService;
        ISMSStatusService _SMSStatusService;
        IRoleService roleService;
        private readonly ISMSBillPaymentBkashService _smsBillPaymentBkashService;

        public CashCollectionController(IErrorService errorService,
            ICashCollectionService cashCollectionService, ICustomerService customerService,
            ISupplierService supplierService, IMiscellaneousService<CashCollection> miscellaneousService,
            IMapper mapper, IUserService UserService, ISisterConcernService SisterConcern, ISMSStatusService SMSService, ISystemInformationService SysService,
            ISMSStatusService SMSStatusService, IRoleService _roleService, ISMSBillPaymentBkashService smsBillPaymentBkashService)
            : base(errorService)
        {
            _CashCollectionService = cashCollectionService;
            _CustomerService = customerService;
            _SupplierService = supplierService;
            _miscellaneousService = miscellaneousService;
            _mapper = mapper;
            _UserService = UserService;
            _SisterConcern = SisterConcern;
            _SMSService = SMSService;
            _SMSStatusService = SMSStatusService;
            _SysService = SysService;
            roleService = _roleService;
            _smsBillPaymentBkashService = smsBillPaymentBkashService;
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
                var user = _UserService.GetUserById(User.Identity.GetUserId<int>());
                int EmployeeID = user.EmployeeID;
                var EMPitemsAsync = _CashCollectionService.GetAllCashCollByEmployeeIDAsync(EmployeeID);
                var EMPvmodel = _mapper.Map<IEnumerable<Tuple<int, DateTime, string, string, string,
                    string, string, Tuple<string, string>>>, IEnumerable<GetCashCollectionViewModel>>(await EMPitemsAsync);
                return View(EMPvmodel);
            }
            else
            {
                var itemsAsync = _CashCollectionService.GetAllCashCollAsync(ViewBag.FromDate, ViewBag.ToDate);
                var vmodel = _mapper.Map<IEnumerable<Tuple<int, DateTime, string, string, string,
                    string, string, Tuple<string, string>>>, IEnumerable<GetCashCollectionViewModel>>(await itemsAsync);
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
                //int EmployeeID = ConstantData.GetEmployeeIDByUSerID(User.Identity.GetUserId<int>());
                var user = _UserService.GetUserById(User.Identity.GetUserId<int>());
                int EmployeeID = user.EmployeeID;
                var EMPitemsAsync = _CashCollectionService.GetAllCashCollByEmployeeIDAsync(EmployeeID);
                var EMPvmodel = _mapper.Map<IEnumerable<Tuple<int, DateTime, string, string, string,
                    string, string, Tuple<string, string>>>, IEnumerable<GetCashCollectionViewModel>>(await EMPitemsAsync);
                return View(EMPvmodel);
            }
            else
            {
                var itemsAsync = _CashCollectionService.GetAllCashCollAsync(ViewBag.FromDate, ViewBag.ToDate);
                var vmodel = _mapper.Map<IEnumerable<Tuple<int, DateTime, string, string, string,
                    string, string, Tuple<string, string>>>, IEnumerable<GetCashCollectionViewModel>>(await itemsAsync);
                return View(vmodel);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("create")]
        public ActionResult Create()
        {
            //ViewBag.CustomerIds = GetAllCustomerForDDL();
            string recptNo = _miscellaneousService.GetUniqueKey(x => int.Parse(x.ReceiptNo));
            return View(new CreateCashCollectionViewModel { Type = EnumDropdownTranType.FromCustomer, ReceiptNo = recptNo });
        }

        [HttpPost]
        [Authorize]
        [Route("create/returnUrl")]
        public async Task<ActionResult> Create(CreateCashCollectionViewModel newCashCollection, FormCollection formCollection, string returnUrl)
        {

            AddModelError(newCashCollection, formCollection);
            if (!ModelState.IsValid)
                return View(newCashCollection);

            if (newCashCollection != null)
            {
                newCashCollection.CreateDate = DateTime.Now.ToString();
                newCashCollection.CreatedBy = (User.Identity.GetUserId<string>());
                newCashCollection.ConcernID = User.Identity.GetConcernId().ToString();
                newCashCollection.SupplierID = "0";

                if (newCashCollection.Type == EnumDropdownTranType.FromCustomer)
                    newCashCollection.TransactionType = EnumTranType.FromCustomer;
                else if (newCashCollection.Type == EnumDropdownTranType.CollectionReturn)
                    newCashCollection.TransactionType = EnumTranType.CollectionReturn;

                //newCashCollection.ModifiedBy = "0";
                //newCashCollection.ModifiedDate = DateTime.Now.ToString();

                if (newCashCollection.AccountNo == null)
                    newCashCollection.AccountNo = "No A/C";

                var cashCollection = _mapper.Map<CreateCashCollectionViewModel, CashCollection>(newCashCollection);



                #region Total Due Update
                var oCustomer = _CustomerService.GetCustomerById(Convert.ToInt32(newCashCollection.CustomerID));
                if (newCashCollection.TransactionType == EnumTranType.FromCustomer)
                    oCustomer.TotalDue = oCustomer.TotalDue - (cashCollection.Amount + cashCollection.AdjustAmt);
                else if (newCashCollection.TransactionType == EnumTranType.CollectionReturn)
                    oCustomer.TotalDue = oCustomer.TotalDue + (cashCollection.Amount + cashCollection.AdjustAmt);
                newCashCollection.BalanceDue = oCustomer.TotalDue.ToString();
                #endregion


                #region Total Due Update for Supplier

                if (oCustomer.CustomerType == EnumCustomerType.Both)
                {
                    var oSup = _SupplierService.GetSupplierByCsId(Convert.ToInt32(newCashCollection.CustomerID));
                    if (newCashCollection.TransactionType == EnumTranType.FromCustomer)
                        oSup.TotalDue = oSup.TotalDue + (cashCollection.Amount + cashCollection.AdjustAmt);
                    else if (newCashCollection.TransactionType == EnumTranType.CollectionReturn)
                        oSup.TotalDue = oSup.TotalDue - (cashCollection.Amount + cashCollection.AdjustAmt);
                    newCashCollection.BalanceDue = oSup.TotalDue.ToString();

                    _SupplierService.UpdateSupplier(oSup);
                    _SupplierService.SaveSupplier();
                }

                #endregion

                #region Remind Date Update
                if (Convert.ToDateTime(newCashCollection.RemindDate) > Convert.ToDateTime(newCashCollection.EntryDate))
                {
                    if (oCustomer != null)
                    {
                        oCustomer.RemindDate = Convert.ToDateTime(newCashCollection.RemindDate);
                    }
                }
                #endregion
                bool Status = false;
                try
                {
                    _CashCollectionService.AddCashCollection(cashCollection);
                    _CashCollectionService.SaveCashCollection();

                    TempData["MoneyReceiptData"] = cashCollection;
                    TempData["IsMoneyReceiptReady"] = true;
                    TempData["CashCollectionID"] = cashCollection.CashCollectionID;
                    TempData["IsMoneyReceiptById"] = true;
                    TempData["isPosRecipt"] = true;
                    TempData["IsPOSShow"] = _SysService.GetSystemInformationByConcernId(User.Identity.GetConcernId()).IsPosInvoiceShow > 0 ? true : false;

                    Status = true;
                }
                catch (Exception)
                {
                    Status = false;
                }

                if (Status)
                {
                    _CustomerService.UpdateCustomer(oCustomer);
                    _CustomerService.SaveCustomer();
                }


                //_CashCollectionService.UpdateTotalDue(Convert.ToInt32(newCashCollection.CustomerID), 0, 0, 0, Convert.ToDecimal(Convert.ToDecimal(newCashCollection.Amount) + Convert.ToDecimal(newCashCollection.AdjustAmt)));

              

                AddToastMessage("", "Item has been saved successfully.", ToastType.Success);

                //#region SMS Service
                //var SystemInfo = _SysService.GetSystemInformationByConcernId(User.Identity.GetConcernId());

                //if (SystemInfo.IsCashcollSMSEnable == 1 && newCashCollection.IsSmsEnable == true)
                //{

                //    List<SMSRequest> sms = new List<SMSRequest>();
                //    if (cashCollection.TransactionType == EnumTranType.FromCustomer)
                //    {
                //        sms.Add(new SMSRequest()
                //        {
                //            MobileNo = oCustomer.ContactNo,
                //            CustomerID = oCustomer.CustomerID,
                //            CustomerCode = oCustomer.Code,
                //            TransNumber = cashCollection.ReceiptNo,
                //            Date = (DateTime)cashCollection.EntryDate,
                //            PreviousDue = oCustomer.TotalDue + (decimal)cashCollection.Amount + cashCollection.AdjustAmt,
                //            ReceiveAmount = (decimal)cashCollection.Amount,
                //            PresentDue = oCustomer.TotalDue,
                //            SMSType = EnumSMSType.CashCollection
                //        });

                //        //if (SystemInfo.SMSSendToOwner == 1)
                //        //{
                //        //    sms.Add(new SMSRequest()
                //        //    {
                //        //        MobileNo = SystemInfo.InsuranceContactNo,
                //        //        CustomerID = oCustomer.CustomerID,
                //        //        CustomerCode = oCustomer.Code,
                //        //        TransNumber = cashCollection.ReceiptNo,
                //        //        Date = (DateTime)cashCollection.EntryDate,
                //        //        PreviousDue = oCustomer.TotalDue + (decimal)cashCollection.Amount + cashCollection.AdjustAmt,
                //        //        ReceiveAmount = (decimal)cashCollection.Amount,
                //        //        PresentDue = oCustomer.TotalDue,
                //        //        SMSType = EnumSMSType.CashCollection
                //        //    });
                //        //}
                //    }
                //    else if (cashCollection.TransactionType == EnumTranType.CollectionReturn)
                //    {
                //        sms.Add(new SMSRequest()
                //        {
                //            MobileNo = oCustomer.ContactNo,
                //            CustomerID = oCustomer.CustomerID,
                //            CustomerCode = oCustomer.Code,
                //            TransNumber = cashCollection.ReceiptNo,
                //            Date = (DateTime)cashCollection.EntryDate,
                //            PreviousDue = oCustomer.TotalDue - (decimal)cashCollection.Amount + cashCollection.AdjustAmt,
                //            ReceiveAmount = (decimal)cashCollection.Amount,
                //            PresentDue = oCustomer.TotalDue,
                //            SMSType = EnumSMSType.CashCollectionReturn
                //        });

                //        //if (SystemInfo.SMSSendToOwner == 1)
                //        //{
                //        //    sms.Add(new SMSRequest()
                //        //    {
                //        //        MobileNo = SystemInfo.InsuranceContactNo,
                //        //        CustomerID = oCustomer.CustomerID,
                //        //        CustomerCode = oCustomer.Code,
                //        //        TransNumber = cashCollection.ReceiptNo,
                //        //        Date = (DateTime)cashCollection.EntryDate,
                //        //        PreviousDue = oCustomer.TotalDue - (decimal)cashCollection.Amount + cashCollection.AdjustAmt,
                //        //        ReceiveAmount = (decimal)cashCollection.Amount,
                //        //        PresentDue = oCustomer.TotalDue,
                //        //        SMSType = EnumSMSType.CashCollectionReturn
                //        //    });
                //        //}
                //    }

                //    var response = await Task.Run(() => SMSHTTPService.SendSMSAsync(EnumOnnoRokomSMSType.NumberSms, sms, SystemInfo, User.Identity.GetUserId<int>()));
                //    if (response.Count > 0)
                //    {
                //        response.Select(x => { x.ConcernID = User.Identity.GetConcernId(); return x; }).ToList();
                //        _SMSStatusService.AddRange(response);
                //        _SMSStatusService.Save();
                //    }
                //}
                //#endregion

                #region SMS Service
                var SystemInfo = _SysService.GetSystemInformationByConcernId(User.Identity.GetConcernId());

                if (SystemInfo.IsCashcollSMSEnable == 1 && newCashCollection.IsSmsEnable == true)
                {


                    List<SMSRequest> sms = new List<SMSRequest>();
                    sms.Add(new SMSRequest()
                    {
                        MobileNo = oCustomer.ContactNo,
                        CustomerID = oCustomer.CustomerID,
                        CustomerCode = oCustomer.Code,
                        CustomerName = oCustomer.Name,
                        TransNumber = cashCollection.ReceiptNo,
                        Date = (DateTime)cashCollection.EntryDate,
                        PreviousDue = oCustomer.TotalDue + (decimal)cashCollection.Amount + cashCollection.AdjustAmt,
                        ReceiveAmount = (decimal)cashCollection.Amount,
                        PresentDue = oCustomer.TotalDue,
                        SMSType = EnumSMSType.CashCollection
                    });

                    if (SystemInfo.SMSSendToOwner == 1)
                    {
                        sms.Add(new SMSRequest()
                        {
                            MobileNo = SystemInfo.InsuranceContactNo,
                            CustomerID = oCustomer.CustomerID,
                            CustomerCode = oCustomer.Code,
                            CustomerName = oCustomer.Name,
                            TransNumber = cashCollection.ReceiptNo,
                            Date = (DateTime)cashCollection.EntryDate,
                            PreviousDue = oCustomer.TotalDue + (decimal)cashCollection.Amount + cashCollection.AdjustAmt,
                            ReceiveAmount = (decimal)cashCollection.Amount,
                            PresentDue = oCustomer.TotalDue,
                            SMSType = EnumSMSType.CashCollection
                        });
                    }

                    int concernId = User.Identity.GetConcernId();
                    int paymentMasterId;
                    decimal previousBalance;
                    SMSPaymentMaster smsAmountDetails = _smsBillPaymentBkashService.GetByConcernId(concernId);
                    paymentMasterId = smsAmountDetails.SMSPaymentMasterID;
                    previousBalance = smsAmountDetails.TotalRecAmt;
                    var sysInfos = _SysService.GetSystemInformationByConcernId(User.Identity.GetConcernId());
                    decimal smsFee = sysInfos.smsCharge;
                    if (smsAmountDetails.TotalRecAmt > 1)
                    {
                        var response = await Task.Run(() => SMSHTTPService.SendSMS(EnumOnnoRokomSMSType.NumberSms, sms, previousBalance, SystemInfo, User.Identity.GetUserId<int>()));
                        if (response.Count > 0)
                        {
                            decimal smsBalanceCount = 0m;
                            foreach (var item in response)
                            {
                                smsBalanceCount = smsBalanceCount + item.NoOfSMS;

                            }

                            #region udpate payment info                
                            decimal sysLastPayUpdateDate = smsBalanceCount * smsFee;
                            smsAmountDetails.TotalRecAmt = smsAmountDetails.TotalRecAmt - Convert.ToDecimal(sysLastPayUpdateDate);
                            _smsBillPaymentBkashService.Update(smsAmountDetails);
                            _smsBillPaymentBkashService.Save();
                            #endregion
                            response.Select(x => { x.ConcernID = User.Identity.GetConcernId(); return x; }).ToList();
                            _SMSStatusService.AddRange(response);
                            _SMSStatusService.Save();
                        }
                    }
                    else
                    {
                        AddToastMessage("", "SMS Balance is Low Plz Recharge your SMS Balance.", ToastType.Error);
                    }

                }
                #endregion
                return RedirectToAction("Create");
            }
            else
            {
                AddToastMessage("", "No Item data found to create.", ToastType.Error);
                return RedirectToAction("Create");
            }
        }


        private void AddModelError(CreateCashCollectionViewModel newCashCollection, FormCollection formCollection)
        {
            if (string.IsNullOrEmpty(formCollection["CustomerId"]))
                ModelState.AddModelError("CustomerID", "Customer is Required.");
            else
                newCashCollection.CustomerID = formCollection["CustomerId"];

            if (decimal.Parse(GetDefaultIfNull(newCashCollection.Amount)) < 0m)
                ModelState.AddModelError("Amount", "Amount can't be negative");
            if (decimal.Parse(GetDefaultIfNull(newCashCollection.AdjustAmt)) < 0m)
                ModelState.AddModelError("AdjustAmt", "Adjustment can't be negative");

            int CCID = Convert.ToInt32(newCashCollection.CashCollectionID);
            if (_CashCollectionService.GetAllCashCollection().Any(i => i.ReceiptNo.Equals(newCashCollection.ReceiptNo) && i.CashCollectionID != CCID))
                ModelState.AddModelError("ReceiptNo", "This ReceiptNo is already exists.");

            if (!IsDateValid(Convert.ToDateTime(newCashCollection.EntryDate)))
            {
                ModelState.AddModelError("EntryDate", "Back dated entry is not valid.");
            }

            if (newCashCollection.Type == 0)
                ModelState.AddModelError("Type", "Trans. Type is Required.");
        }


        private List<TOIdNameDDL> GetAllCustomerForDDL()
        {
            int CuserId = System.Web.HttpContext.Current.User.Identity.GetUserId<int>();
            int CEmpID = 0;

            if (User.IsInRole(ConstantData.ROLE_MOBILE_USER))
            {
                var user = _UserService.GetUserById(CuserId);
                CEmpID = user.EmployeeID;

                var Ccustomers = _CustomerService.GetAllCustomerByEmpNew(CEmpID);
                var vmCustomers = _mapper.Map<IEnumerable<Customer>, IEnumerable<CreateCustomerViewModel>>(Ccustomers).Select(s => new TOIdNameDDL
                {
                    Id = int.Parse(s.Id),
                    Name = s.Name + "(" + s.ContactNo + ")"
                }).ToList();
                return vmCustomers;
            }
            else
            {
                var customers = _CustomerService.GetAllCustomerNew(User.Identity.GetConcernId()).Select(s => new TOIdNameDDL
                {
                    Id = s.Id,
                    Name = s.Name + "," + s.ContactNo + "," + s.Address
                }).ToList();
                return customers;
            }
        }


        [HttpGet]
        [Authorize]
        public JsonResult GetCustomerInfoById(int customerId)
        {
            int CuserId = System.Web.HttpContext.Current.User.Identity.GetUserId<int>();
            int CEmpID = 0;

            if (User.IsInRole(ConstantData.ROLE_MOBILE_USER))
            {
                var user = _UserService.GetUserById(CuserId);
                CEmpID = user.EmployeeID;

                var Ccustomers = _CustomerService.GetAllCustomerByEmpNew(CEmpID, customerId);
                var vmCustomers = _mapper.Map<IEnumerable<Customer>, IEnumerable<CreateCustomerViewModel>>(Ccustomers).FirstOrDefault();
                return Json(vmCustomers, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var customers = _CustomerService.GetAllCustomerNew(User.Identity.GetConcernId(), customerId).FirstOrDefault();
                return Json(customers, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        [Authorize]
        [Route("edit/{id}")]
        public ActionResult Edit(int id)
        {
            //var customerItems = _CustomerService.GetAllCustomer().Select(cusItem
            //   => new SelectListItem { Text = cusItem.Name, Value = cusItem.CustomerID.ToString() }).ToList();

            //var supplierItems = _SupplierService.GetAllSupplier().Select(suppItem
            //    => new SelectListItem { Text = suppItem.Name, Value = suppItem.SupplierID.ToString() }).ToList();

            //ViewBag.CustomerIds = GetAllCustomerForDDL();
            var cashCollection = _CashCollectionService.GetCashCollectionById(id);
            if (!IsDateValid(Convert.ToDateTime(cashCollection.EntryDate)))
                return RedirectToAction("Index");
            var vmodel = _mapper.Map<CashCollection, CreateCashCollectionViewModel>(cashCollection);
            var Customer = _CustomerService.GetCustomerById((int)cashCollection.CustomerID);
            vmodel.CurrentDue = Customer.TotalDue.ToString();
            vmodel.RemindDate = Customer.RemindDate.ToString();
            //vmodel.CustomerItems = customerItems;
            //vmodel.SupplierItems = supplierItems;
            return View("Create", vmodel);
        }

        [HttpPost]
        [Authorize]
        [Route("edit/returnUrl")]
        public ActionResult Edit(CreateCashCollectionViewModel newCashCollection, FormCollection formCollection, string returnUrl)
        {
            AddModelError(newCashCollection, formCollection);

            if (!ModelState.IsValid)
                return View("Create", newCashCollection);

            if (newCashCollection != null)
            {
                var cashCollection = _CashCollectionService.GetCashCollectionById(int.Parse(newCashCollection.CashCollectionID));

                cashCollection.PaymentType = newCashCollection.PaymentType;
                cashCollection.BankName = newCashCollection.BankName;
                cashCollection.BranchName = newCashCollection.BranchName;
                cashCollection.EntryDate = Convert.ToDateTime(newCashCollection.EntryDate);
                cashCollection.Amount = decimal.Parse(newCashCollection.Amount);
                cashCollection.AccountNo = newCashCollection.AccountNo;
                cashCollection.MBAccountNo = newCashCollection.MBAccountNo;
                cashCollection.BKashNo = newCashCollection.BKashNo;
                cashCollection.Remarks = newCashCollection.Remarks;
                //cashCollection.TransactionType = newCashCollection.TransactionType;
                cashCollection.AdjustAmt = decimal.Parse(newCashCollection.AdjustAmt);
                cashCollection.BalanceDue = decimal.Parse(newCashCollection.BalanceDue);
                cashCollection.CustomerID = int.Parse(formCollection["CustomerId"]);
                if (newCashCollection.Type == EnumDropdownTranType.FromCustomer)
                    newCashCollection.TransactionType = EnumTranType.FromCustomer;
                else if (newCashCollection.Type == EnumDropdownTranType.CollectionReturn)
                    newCashCollection.TransactionType = EnumTranType.CollectionReturn;
                cashCollection.ModifiedBy = User.Identity.GetUserId<int>();
                cashCollection.ModifiedDate = DateTime.Now;
                //cashCollection.SupplierID = int.Parse(newCashCollection.SupplierID);
                cashCollection.InterestAmt = decimal.Parse(GetDefaultIfNull(newCashCollection.InterestAmt));

                _CashCollectionService.UpdateTotalDueWhenEdit((int)cashCollection.CustomerID, 0, 0, cashCollection.CashCollectionID, (cashCollection.Amount + cashCollection.AdjustAmt - cashCollection.InterestAmt));
                cashCollection.BalanceDue = _CustomerService.GetCustomerById((int)cashCollection.CustomerID).TotalDue;
                _CashCollectionService.UpdateCashCollection(cashCollection);
                _CashCollectionService.SaveCashCollection();



                #region Remind Date Update
                if (Convert.ToDateTime(newCashCollection.RemindDate) > Convert.ToDateTime(newCashCollection.EntryDate))
                {
                    var customer = _CustomerService.GetCustomerById(Convert.ToInt32(cashCollection.CustomerID));
                    if (customer != null)
                    {
                        customer.RemindDate = Convert.ToDateTime(newCashCollection.RemindDate);
                        _CustomerService.UpdateCustomer(customer);
                        _CustomerService.SaveCustomer();
                    }
                }
                #endregion

                TempData["MoneyReceiptData"] = cashCollection;
                TempData["IsMoneyReceiptReady"] = true;


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
            var oldCashCollection = _CashCollectionService.GetCashCollectionById(id);
            decimal Amount = 0m, AdjAmt = 0m, IntAmt = 0m;
            Amount = oldCashCollection.Amount;
            AdjAmt = oldCashCollection.AdjustAmt;
            IntAmt = oldCashCollection.InterestAmt;

            if (!IsDateValid(Convert.ToDateTime(oldCashCollection.EntryDate)))
            {
                return RedirectToAction("Index");
            }

            var oCustomer = _CustomerService.GetCustomerById(Convert.ToInt32(oldCashCollection.CustomerID));
            if (oldCashCollection.TransactionType == EnumTranType.FromCustomer)
                oCustomer.TotalDue = oCustomer.TotalDue + (Amount + AdjAmt) - IntAmt;
            else if (oldCashCollection.TransactionType == EnumTranType.CollectionReturn)
                oCustomer.TotalDue = oCustomer.TotalDue - (Amount + AdjAmt);

            if (oCustomer.CustomerType == EnumCustomerType.Both)
            {
                var oSup = _SupplierService.GetSupplierByCsId(Convert.ToInt32(oldCashCollection.CustomerID));
                if (oldCashCollection.TransactionType == EnumTranType.FromCustomer)
                    oSup.TotalDue = oSup.TotalDue - (Amount + AdjAmt);
                else if (oldCashCollection.TransactionType == EnumTranType.CollectionReturn)
                    oSup.TotalDue = oSup.TotalDue + (Amount + AdjAmt);
                _SupplierService.UpdateSupplier(oSup);
                _SupplierService.SaveSupplier();
            }


            //_CashCollectionService.UpdateTotalDue(Convert.ToInt32(CashCollection.CustomerID), 0, 0, 0, -(Convert.ToDecimal(Convert.ToDecimal(CashCollection.Amount) + Convert.ToDecimal(CashCollection.AdjustAmt))));
            bool Status = false;
            try
            {
                _CashCollectionService.DeleteCashCollection(id);
                _CashCollectionService.SaveCashCollection();
                Status = true;
            }
            catch (Exception)
            {
                Status = false;
            }

            if (Status)
            {
                _CustomerService.UpdateCustomer(oCustomer);
                _CustomerService.SaveCustomer();
            }
            AddToastMessage("", "Item has been deleted successfully.", ToastType.Success);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public ActionResult CashCollectionReport()//CashCollectionReport
        {
            return View("CashCollectionReport");
        }

        [HttpGet]
        [Authorize]
        public ActionResult DailyCashBookLedgerReport()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult SRWiseCashCollectionReport()//CashCollectionReport
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult MoneyReceipt(int id)
        {
            TempData["CashCollectionID"] = id;
            TempData["IsMoneyReceiptById"] = true;
            TempData["isPosRecipt"] = false;
            return RedirectToAction("Index");
        }



        [HttpGet]
        [Authorize]
        public ActionResult PosMoneyReceipt(int id)
        {
            TempData["CashCollectionID"] = id;
            TempData["IsMoneyReceiptById"] = true;
            TempData["isPosRecipt"] = true;
            TempData["IsPOSShow"] = _SysService.GetSystemInformationByConcernId(User.Identity.GetConcernId()).IsPosInvoiceShow > 0 ? true : false;
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public ActionResult AdminCashcolletionReport()//CashCollectionReport
        {
            @ViewBag.Concerns = new SelectList(_SisterConcern.GetAll(), "ConcernID", "Name");
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult CashInHandReport()
        {

            #region Opening Save for cash in hand report
            //var pb = _PrevBalanceService.DailyBalanceProcess(User.Identity.GetConcernId());
            //if (pb.Count != 0)
            //{
            //    foreach (var item in pb)
            //    {
            //        _PrevBalanceService.AddPrevBalance(item);
            //    }
            //}
            //_PrevBalanceService.Save();
            #endregion
            return View();
        }



        [HttpGet]
        public ActionResult ReceiptPayment()
        {
            return View();
        }


        [HttpGet]
        [Authorize]
        public ActionResult TypeWiseCashInHand()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult ProfitAndLossReport()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult MonthlyTransactionReport()
        {
            if (User.IsInRole(EnumUserRoles.Admin.ToString()) || User.IsInRole(EnumUserRoles.superadmin.ToString()))
                @ViewBag.Concerns = new SelectList(_SisterConcern.GetAll(), "ConcernID", "Name");
            return View();
        }

        public ActionResult AdminCashInhand()
        {
            @ViewBag.Concerns = new SelectList(_SisterConcern.GetAll(), "ConcernID", "Name");
            return View();
        }

        [HttpGet]
        [Authorize]
        public JsonResult Approved(int orderId)
        {
            var CashCollection = _CashCollectionService.GetCashCollectionById(orderId);
            {
                AddToastMessage("", "Cash collection is not pending.");
                return Json(false, JsonRequestBehavior.AllowGet);

            }

            decimal NetAmount = (Convert.ToDecimal(Convert.ToDecimal(CashCollection.Amount) + Convert.ToDecimal(CashCollection.AdjustAmt)))
                - Convert.ToDecimal(CashCollection.InterestAmt);
            _CashCollectionService.UpdateTotalDue(Convert.ToInt32(CashCollection.CustomerID), 0, 0, 0, NetAmount);

            CashCollection.TransactionType = EnumTranType.FromCustomer;
            _CashCollectionService.UpdateCashCollection(CashCollection);
            _CashCollectionService.SaveCashCollection();
            AddToastMessage("", "Item has been approved successfully.", ToastType.Success);

            return Json(true, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        [Authorize]
        public ActionResult AdjustmentReport()
        {
            return View();
        }
    }
}