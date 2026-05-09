using IMSWEB.Model;
using IMSWEB.Model.TOs;
using IMSWEB.Report;
using IMSWEB.Service;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IMSWEB.Controllers
{
    public class HomeController : CoreController
    {
        ICreditSalesOrderService _salesOrderService;
        ITransactionalReport _transactionalReportService;
        ICustomerService _CustomerService;
        ISystemInformationService _SysInfoService;
        ISMSStatusService _SMSStatusService;
        private readonly IServiceChargeService _serviceChargeService;
        private readonly IServiceChargeDetailsService _serviceChargeDetailsService;
        private readonly ISisterConcernService _sisterConcernService;
        private readonly IProductService _productService;
        private readonly IUserService _user;
        private readonly IPrevBalanceService _PrevBalanceService;

        public HomeController(IErrorService errorService, ITransactionalReport transactionalReportService,
            ICreditSalesOrderService SaleOrderService, ICustomerService CustomerService,
              ISystemInformationService SysInfoService, ISMSStatusService SMSStatusService, IServiceChargeService serviceChargeService, IServiceChargeDetailsService serviceChargeDetailsService, ISisterConcernService sisterConcernService, IProductService productService, IUserService user, IPrevBalanceService prevBalanceService)
            : base(errorService)
        {
            _salesOrderService = SaleOrderService;
            _transactionalReportService = transactionalReportService;
            _CustomerService = CustomerService;
            _SysInfoService = SysInfoService;
            _SMSStatusService = SMSStatusService;
            _serviceChargeService = serviceChargeService;
            _serviceChargeDetailsService = serviceChargeDetailsService;
            _sisterConcernService = sisterConcernService;
            _productService = productService;
            _user = user;
            _PrevBalanceService = prevBalanceService;
        }

        [Authorize]
        public async Task<ActionResult> Index()
        {
                        
            List<UpcommingScheduleReport> Schedules = new List<UpcommingScheduleReport>();

            //if (User.Identity.GetConcernId() == (int)EnumSisterConcern.SAMSUNG_ELECTRA_CONCERNID || User.Identity.GetConcernId() == (int)EnumSisterConcern.HAVEN_ENTERPRISE_CONCERNID || User.Identity.GetConcernId() == (int)EnumSisterConcern.HAWRA_ENTERPRISE_CONCERNID)
            //{
            //    _salesOrderService.CalculatePenaltySchedules(User.Identity.GetConcernId());
            //}

            TempData["UserConcern"] = User.Identity.GetConcernId();
            DateTime LocalDate = GetLocalDateTime();
            ViewBag.LocalDateTime = new DateTime(LocalDate.Year, LocalDate.Month, LocalDate.Day);
            //var upcomingInstallemnts = _salesOrderService.GetUpcomingSchedule(ViewBag.LocalDateTime, ViewBag.LocalDateTime);
            //foreach (var item in upcomingInstallemnts)
            //{
            //    item.DefaultAmount = _salesOrderService.GetDefaultAmount(item.CreditSalesID, DateTime.Today);
            //    item.InstallmentAmount += item.DefaultAmount;
            //    Schedules.Add(item);
            //}


            #region Opening Save for cash in hand report
            var pb = _PrevBalanceService.DailyBalanceProcess(User.Identity.GetConcernId());
            if (pb.Count != 0)
            {
                foreach (var item in pb)
                {
                    _PrevBalanceService.AddPrevBalance(item);
                }
            }
            _PrevBalanceService.Save();
            TempData["UserConcern"] = User.Identity.GetConcernId();

            #endregion


            #region SMS
            //SMSRequest smslist = new SMSRequest();
            //var SysInfo = _SysInfoService.GetSystemInformationByConcernId(User.Identity.GetConcernId());
            //int DaysBeforeSendSMS = SysInfo.DaysBeforeSendSMS;
            //DateTime Instfromday = LocalDate.AddDays(DaysBeforeSendSMS).Date;
            //DateTime Instto = Instfromday.AddHours(23).AddMinutes(59);
            //var forSMSIntallments = _salesOrderService.GetUpcomingSchedule(Instfromday, Instto);

            //DateTime fromdate = new DateTime(LocalDate.Year, LocalDate.Month, LocalDate.Day);
            //DateTime toDate = fromdate.AddHours(23).AddMinutes(59);

            //var alreadySendSMSs = from ss in _SMSStatusService.GetAllIQueryable()
            //                      where ss.EntryDate >= fromdate && ss.EntryDate <= toDate
            //                      select ss.CustomerID;

            //var TomorrowInstallment = (from C in forSMSIntallments.Where(i => !(alreadySendSMSs.Contains(i.CustomerID)))
            //                           select new SMSRequest
            //                           {
            //                               CustomerID = C.CustomerID,
            //                               CustomerCode = C.CustomerCode,
            //                               CustomerName = C.CustomerName,
            //                               MobileNo = C.CustomerConctact,
            //                               CustomerAddress = C.CustomerAddress,
            //                               PresentDue = C.InstallmentAmount,
            //                               Date = (DateTime)C.PaymentDate,
            //                               SMSType = EnumSMSType.InstallmentAlert
            //                           }).OrderBy(x => x.CustomerID).ToList();
            //if (TomorrowInstallment.Count() > 0)
            //{
            //    decimal counter = Math.Ceiling(Convert.ToDecimal(TomorrowInstallment.Count()) / 5);
            //    for (int i = 0; i < counter; i++)
            //    {
            //        var GroupSends = TomorrowInstallment.Skip(i * 5).Take(5).ToList();
            //        var Response = await SMSHTTPService.SendSMSAsync(EnumOnnoRokomSMSType.ListSms, GroupSends, SysInfo, User.Identity.GetUserId<int>());
            //        if (Response.Count() > 0)
            //        {
            //            Response.Select(x => { x.ConcernID = User.Identity.GetConcernId(); return x; }).ToList();
            //            _SMSStatusService.AddRange(Response);
            //            _SMSStatusService.Save();
            //        }
            //    }

            //}
            #endregion

            #region check if payment info exists or not, if not then insert data
            AddPaymentInfoData();
            #endregion
            return View(Schedules);
        }


        private void AddPaymentInfoData()
        {
            int concernId = User.Identity.GetConcernId();
            DateTime todaysDate = GetLocalDateTime();

            List<SisterConcern> sisterConcerns = _sisterConcernService.GetFamilyTree(concernId);
            if (sisterConcerns.Any())
            {
                foreach (var concern in sisterConcerns)
                {
                    ServiceCharge serviceCharge = _serviceChargeService.GetByYearAndConcern(concern.ConcernID, todaysDate.Year);
                    if (serviceCharge == null)
                    {

                        ServiceCharge newServiceCharge = new ServiceCharge
                        {
                            ServiceYear = todaysDate.Year,
                            ConcernId = concern.ConcernID,
                            TotalServiceCollection = 0m,
                            CreateDate = DateTime.Now,
                            CreatedBy = User.Identity.GetUserId<int>()
                        };
                        _serviceChargeService.Add(newServiceCharge);
                        bool isServiceInserted = _serviceChargeService.Save();
                        if (isServiceInserted)
                        {
                            serviceCharge = _serviceChargeService.GetByYearAndConcern(concern.ConcernID, todaysDate.Year);
                        }
                    }

                    List<ServiceChargeDetails> serviceChargeDetails = _serviceChargeDetailsService.GetAllByServiceId(serviceCharge.Id);
                    SisterConcern sisterConcern = _sisterConcernService.GetSisterConcernById(concern.ConcernID);
                    List<ServiceChargeDetails> addChargeList = new List<ServiceChargeDetails>();
                    if (serviceChargeDetails.Any())
                    {
                        ServiceChargeDetails lastCharge = serviceChargeDetails.OrderByDescending(d => d.Month).ThenByDescending(d => d.ServiceChargeId).FirstOrDefault();

                        if (lastCharge.Month < todaysDate.Month)
                        {
                            int differentsOfMonth = todaysDate.Month - lastCharge.Month;
                            int month = lastCharge.Month;
                            for (int i = 0; i < differentsOfMonth; i++)
                            {
                                ServiceChargeDetails serviceDetail = new ServiceChargeDetails
                                {
                                    ServiceChargeId = serviceCharge.Id,
                                    ExpectedServiceCharge = sisterConcern.ServiceCharge,
                                    Month = month + i + 1,
                                    IsPaid = false,
                                    PaidServiceCharge = 0m
                                };
                                addChargeList.Add(serviceDetail);
                            }
                        }
                    }
                    else
                    {
                        int differentsOfMonth = todaysDate.Month;
                        for (int i = 1; i <= differentsOfMonth; i++)
                        {
                            ServiceChargeDetails serviceDetail = new ServiceChargeDetails
                            {
                                ServiceChargeId = serviceCharge.Id,
                                ExpectedServiceCharge = sisterConcern.ServiceCharge,
                                Month = i,
                                IsPaid = false,
                                PaidServiceCharge = 0m
                            };
                            addChargeList.Add(serviceDetail);
                        }
                    }

                    if (addChargeList.Any())
                    {
                        _serviceChargeDetailsService.AddMultiple(addChargeList);
                        _serviceChargeDetailsService.Save();
                    }
                }
            }


        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpPost]
        [Authorize]
        public PartialViewResult UpComingScheduleReport(FormCollection formCollection)
        {
            byte[] bytes = _transactionalReportService.UpComingScheduleReport(DateTime.Today, DateTime.Today, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return PartialView("~/Views/Shared/_ReportViewer.cshtml");
        }

        [HttpGet]
        public ActionResult GetLimitedStockProducts()
        {

            var limitedStockProducts = _productService.GetProductIQueryableForLStock().ToList();
            return Json(limitedStockProducts, JsonRequestBehavior.AllowGet);

        }




        [HttpPost]
        public JsonResult ChangeRemindDate(int CustomerID, DateTime RemindDate)
        {
            if (CustomerID > 0)
            {
                var customer = _CustomerService.GetCustomerById(CustomerID);
                customer.RemindDate = RemindDate;
                _CustomerService.UpdateCustomer(customer);
                _CustomerService.SaveCustomer();
                return Json(true);
            }
            return Json(false);
        }






        [HttpPost]
        public JsonResult getConcernName()
        {
            var userId = Convert.ToInt32(User.Identity.GetUserId());
            var userName = _user.GetUserNameById(userId);
            if (userName != null)
            {
                return Json(new { Status = true, Name = userName }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Status = false, Name = "" }, JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        [Authorize]
        public JsonResult GetDailySalesAmt(string dataLength)
        {
            int ConcernID = User.Identity.GetConcernId();
            List<TOHomeWidget> widgetData = new List<TOHomeWidget>();
            if (ConcernID != 9)
            {
                widgetData = _SysInfoService.GetHomeWidgeSales(dataLength, User.Identity.GetConcernId());
            }
                
            return Json(widgetData, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        [Authorize]
        public JsonResult GetYearlyData(string dataLength)
        {
            int ConcernID = User.Identity.GetConcernId();
            List<TOHomeWidget> widgetData = new List<TOHomeWidget>();

            if (ConcernID != 9)
            {
                widgetData = _SysInfoService.GetYearlyData(dataLength, ConcernID);
            }

            return Json(widgetData, JsonRequestBehavior.AllowGet);
        }


    }
}