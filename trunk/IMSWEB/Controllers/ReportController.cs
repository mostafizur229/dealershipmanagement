using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IMSWEB.Report;
using AutoMapper;
using IMSWEB.Service;
using System.Threading.Tasks;
using System.IO;
using IMSWEB.Model;


namespace IMSWEB.Controllers
{
    [Authorize]
    [RoutePrefix("report")]
    public class ReportController : CoreController
    {
        IBasicReport _basicReportService;
        ITransactionalReport _transactionalReportService;
        IMapper _mapper;
        ISisterConcernService _SisterConcernService;
        ISystemInformationService _systemInformationService;
        public ReportController(IErrorService errorService,
            IBasicReport basicReportService, ITransactionalReport transactionalReportService,
            IMapper mapper, ISisterConcernService SisterConcernService,
            ISystemInformationService systemInformationService)
            : base(errorService)
        {
            _basicReportService = basicReportService;
            _transactionalReportService = transactionalReportService;
            _mapper = mapper;
            _SisterConcernService = SisterConcernService;
            _systemInformationService = systemInformationService;
        }

        //[HttpGet]
        //public ActionResult RenderReport()
        //{
        //    byte[] bytes = null;
        //    if (TempData["ReportData"] != null)
        //        bytes = (byte[])TempData["ReportData"];
        //    return File(bytes, "application/pdf");
        //}

        [HttpGet]
        public ActionResult RenderReport()
        {
            byte[] bytes = TempData["ReportData"] as byte[];

            if (bytes == null || bytes.Length == 0)
            {
                return Content("The report data is unavailable or expired. Please try again.");
            }

            string userAgent = Request.UserAgent ?? string.Empty;

            // Detect Windows (desktop)
            bool isWindows = userAgent.IndexOf("Windows", StringComparison.OrdinalIgnoreCase) >= 0;

            if (isWindows)
            {
                // Desktop → inline preview in iframe
                return File(bytes, "application/pdf");
            }
            else
            {
                // Mobile → unique timestamp-based filename
                string fileName = $"RenderReport_{DateTime.Now:yyyyMMdd_HHmmssfff}.pdf";

                return File(bytes, "application/pdf", fileName);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("employeeinformation-report")]
        public async Task<PartialViewResult> EmployeeInformationReport()
        {
            byte[] bytes = await _basicReportService.EmployeeInformationReport(User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        private PartialViewResult CustomPartialView()
        {
            string userAgent = Request.UserAgent;
            if (userAgent.ToLower().Contains("windows"))
                return PartialView("~/Views/Shared/_ReportViewer.cshtml");
            else
                return PartialView("~/Views/Shared/_ReportViewMobile.cshtml");
        }

        [HttpPost]
        [Authorize]
        public PartialViewResult DailySalesReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            int reportType = Convert.ToInt32(formCollection["ReportType"].ToString());

            int CustomerType = 0;
            if (!string.IsNullOrEmpty(formCollection["CustomerType"]))
                CustomerType = Convert.ToInt32(formCollection["CustomerType"]);

            byte[] bytes = _transactionalReportService.SalesReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), reportType, "Daily", CustomerType);
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public PartialViewResult SalesBenefitReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            int reportType = Convert.ToInt32(formCollection["ReportType"].ToString());

            int CustomerType = 0;
            if (!string.IsNullOrEmpty(formCollection["CustomerType"]))
                CustomerType = Convert.ToInt32(formCollection["CustomerType"]);

            byte[] bytes = _transactionalReportService.SalesBenefitReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), reportType, "Daily", CustomerType);
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }


        //[HttpPost]
        //[Authorize]
        //public PartialViewResult AdminDailySalesReportDetails(FormCollection formCollection)
        //{
        //    DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
        //    DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
        //    int reportType = Convert.ToInt32(formCollection["ReportType"].ToString());

        //    int CustomerType = 0;
        //    if (!string.IsNullOrEmpty(formCollection["CustomerType"]))
        //        CustomerType = Convert.ToInt32(formCollection["CustomerType"]);

        //    byte[] bytes = _transactionalReportService.SalesReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), reportType, "Daily", CustomerType);
        //    TempData["ReportData"] = bytes;
        //    return PartialView("~/Views/Shared/_ReportViewer.cshtml");

        //}
        [HttpPost]
        [Authorize]
        public PartialViewResult MonthlySalesReport(FormCollection formCollection)
        {
            DateTime date = Convert.ToDateTime(formCollection["FromDate"].ToString());
            //DateTime fromDate =new DateTime(date.Year,date.Month,1,0,0,0);// Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            //DateTime toDate =new DateTime(date.Year,date.Month,30,23,59,59);// Convert.ToDateTime(formCollection["FromDate"].ToString() + " 11:59:59 PM");

            var fromDate = new DateTime(date.Year, date.Month, 1);
            var toDate = fromDate.AddMonths(1).AddDays(-1);
            int reportType = Convert.ToInt32(formCollection["ReportType"].ToString());

            //var valueResult = ValueProvider.GetValue("ReportType");


            byte[] bytes = _transactionalReportService.SalesReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), reportType, "Monthly", 0);
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }
        [HttpPost]
        [Authorize]
        public PartialViewResult YearlySalesReport(FormCollection formCollection)
        {
            int year = Convert.ToInt32(formCollection["FromDate"].ToString());
            DateTime fromDate = new DateTime(year, 1, 1, 0, 0, 0);// Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = new DateTime(year, 12, 31, 23, 59, 59);
            int reportType = Convert.ToInt32(formCollection["ReportType"].ToString());

            byte[] bytes = _transactionalReportService.SalesReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), reportType, "Yearly", 0);
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }


        [HttpPost]
        [Authorize]
        public PartialViewResult StockReport(FormCollection formCollection)
        {
            int reportType = 0, CompanyID = 0, CategoryID = 0, ProductsId = 0, GodownsId = 0;

            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                reportType = Convert.ToInt32(formCollection["ReportType"].ToString());
            if (!string.IsNullOrEmpty(formCollection["CompaniesId"]))
                CompanyID = Convert.ToInt32(formCollection["CompaniesId"].ToString());
            if (!string.IsNullOrEmpty(formCollection["CategoriesId"]))
                CategoryID = Convert.ToInt32(formCollection["CategoriesId"].ToString());
            if (!string.IsNullOrEmpty(formCollection["ProductsId"]))
                ProductsId = Convert.ToInt32(formCollection["ProductsId"].ToString());

            if (!string.IsNullOrEmpty(formCollection["GodownsId"]))
                GodownsId = Convert.ToInt32(formCollection["GodownsId"].ToString());

            byte[] stock = _transactionalReportService.StockDetailReport(User.Identity.Name, User.Identity.GetConcernId(), reportType, CompanyID, CategoryID, ProductsId, GodownsId);
            TempData["ReportData"] = stock;
            return CustomPartialView();
        }

        [HttpPost]
        [Authorize]
        public PartialViewResult StockSummaryReport(FormCollection formCollection)
        {
            int reportType = 0, CompanyID = 0, CategoryID = 0, ProductsId = 0, GodownsId = 0, StockType = 0;
            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                reportType = Convert.ToInt32(formCollection["ReportType"].ToString());
            if (!string.IsNullOrEmpty(formCollection["CompaniesId"]))
                CompanyID = Convert.ToInt32(formCollection["CompaniesId"].ToString());
            if (!string.IsNullOrEmpty(formCollection["CategoriesId"]))
                CategoryID = Convert.ToInt32(formCollection["CategoriesId"].ToString());
            if (!string.IsNullOrEmpty(formCollection["ProductsId"]))
                ProductsId = Convert.ToInt32(formCollection["ProductsId"].ToString());
            if (!string.IsNullOrEmpty(formCollection["GodownsId"]))
                GodownsId = Convert.ToInt32(formCollection["GodownsId"].ToString());

            if (!string.IsNullOrEmpty(formCollection["stockType"]))
                StockType = Convert.ToInt32(formCollection["stockType"]);

            byte[] stock = _transactionalReportService.StockSummaryReport(User.Identity.Name, User.Identity.GetConcernId(), reportType, CompanyID, CategoryID, ProductsId, GodownsId, StockType);
            TempData["ReportData"] = stock;
            return CustomPartialView();
        }


        [HttpPost]
        [Authorize]
        public PartialViewResult DailyPurchaseReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            int reportType = Convert.ToInt32(formCollection["ReportType"].ToString());

            EnumPurchaseType PurchaseType = 0;
            if (!string.IsNullOrEmpty(formCollection["PurchaseType"]))
                Enum.TryParse(formCollection["PurchaseType"], out PurchaseType);

            byte[] bytes = _transactionalReportService.PurchaseReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), reportType, "Daily", PurchaseType);
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }
        [HttpPost]
        [Authorize]
        public PartialViewResult MonthlyPurchaseReport(FormCollection formCollection)
        {
            DateTime date = Convert.ToDateTime(formCollection["FromDate"].ToString());
            //DateTime fromDate = new DateTime(date.Year, date.Month, 1, 0, 0, 0);// Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            //DateTime toDate = new DateTime(date.Year, date.Month, 30, 23, 59, 59);// Convert.ToDateTime(formCollection["FromDate"].ToString() + " 11:59:59 PM");
            var fromDate = new DateTime(date.Year, date.Month, 1);
            var toDate = fromDate.AddMonths(1).AddDays(-1);
            int reportType = Convert.ToInt32(formCollection["ReportType"].ToString());

            EnumPurchaseType PurchaseType = 0;
            if (!string.IsNullOrEmpty(formCollection["PurchaseType"]))
                Enum.TryParse(formCollection["PurchaseType"], out PurchaseType);

            byte[] bytes = _transactionalReportService.PurchaseReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), reportType, "Monthly", PurchaseType);
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }
        [HttpPost]
        [Authorize]
        public PartialViewResult YearlyPurchaseReport(FormCollection formCollection)
        {
            int year = Convert.ToInt32(formCollection["FromDate"].ToString());
            DateTime fromDate = new DateTime(year, 1, 1, 0, 0, 0);// Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = new DateTime(year, 12, 31, 23, 59, 59);
            int reportType = Convert.ToInt32(formCollection["ReportType"].ToString());

            EnumPurchaseType PurchaseType = 0;
            if (!string.IsNullOrEmpty(formCollection["PurchaseType"]))
                Enum.TryParse(formCollection["PurchaseType"], out PurchaseType);

            byte[] bytes = _transactionalReportService.PurchaseReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), reportType, "Yearly", PurchaseType);
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }


        [HttpGet]
        [Authorize]
        public PartialViewResult SalesInvoiceById()
        {
            int orderId = (int)TempData["OrderId"];

            TempData["OrderId"] = orderId;
            byte[] bytes = _transactionalReportService.SalesInvoiceReport(orderId, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpGet]
        [Authorize]
        public PartialViewResult ChallanById()
        {
            int orderId = (int)TempData["OrderId"];
            byte[] bytes = _transactionalReportService.ChallanReport(orderId, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }


        [HttpGet]
        [Authorize]
        public PartialViewResult SalesInvoice()
        {
            SOrder sorder = (SOrder)TempData["salesInvoiceData"];
            TempData["salesInvoiceData"] = sorder;
            int orderId = sorder.SOrderID;
            //byte[] bytes = _transactionalReportService.SalesInvoiceReport(orderId, User.Identity.Name, User.Identity.GetConcernId());

            byte[] bytes = _transactionalReportService.SalesInvoiceReport(sorder, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpGet]
        [Authorize]
        public PartialViewResult Challan()
        {
            SOrder sorder = (SOrder)TempData["salesInvoiceData"];
            int orderId = sorder.SOrderID;
            //byte[] bytes = _transactionalReportService.SalesInvoiceReport(orderId, User.Identity.Name, User.Identity.GetConcernId());

            byte[] bytes = _transactionalReportService.ChallanReport(sorder, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpGet]
        [Authorize]
        public PartialViewResult SRVisitInvoice()
        {
            //SRVisit sorder = (SRVisit)TempData["SRVisitData"];
            SRVisitViewModel sorder = (SRVisitViewModel)TempData["SRVisitData"];
            string ChallanNo = sorder.SRVisit.ChallanNo;

            //byte[] bytes = _transactionalReportService.SalesInvoiceReport(orderId, User.Identity.Name, User.Identity.GetConcernId());

            byte[] bytes = _transactionalReportService.SRInvoiceReportByChallanNo(ChallanNo, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpGet]
        [Authorize]
        public PartialViewResult SRVisitInvoiceByID()
        {
            int orderId = (int)TempData["OrderId"];
            byte[] bytes = _transactionalReportService.SRInvoiceReport(orderId, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpGet]
        [Authorize]
        public PartialViewResult CreditSalesInvoiceReportByID()
        {
            int orderId = (int)TempData["OrderId"];
            byte[] bytes = _transactionalReportService.CreditSalesInvoiceReportByID(orderId, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpGet]
        [Authorize]
        public PartialViewResult CreditSalesInvoice()
        {
            CreditSale sorder = (CreditSale)TempData["CreditSalesInvoiceData"];
            byte[] bytes = _transactionalReportService.CreditSalesInvoiceReport(sorder, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpPost]
        [Authorize]
        public PartialViewResult CustomerSalesReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            int reportType = 1;//Convert.ToInt32(formCollection["ReportType"].ToString());
            int CustomerId = 0;

            if (!string.IsNullOrEmpty(formCollection["CustomerId"]))
                CustomerId = Convert.ToInt32(formCollection["CustomerId"]);

            if (CustomerId == 0)
            {
                AddToastMessage("Customer Sales", "Please at first select Customer.", ToastType.Warning);
            }

            //var valueResult = ValueProvider.GetValue("ReportType");

            byte[] bytes = _transactionalReportService.CustomeWiseSalesReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), reportType, CustomerId);
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult MOWiseSalesReport(FormCollection formCollection)
        {

            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            int reportType = 0;
            int EmployeeId = 0;

            if (formCollection["ReportType"] != null)
                reportType = Convert.ToInt32(formCollection["ReportType"].ToString());
            else
                reportType = 0;

            if (formCollection["EmployeesId"] != null && formCollection["EmployeesId"] != "")
                EmployeeId = Convert.ToInt32(formCollection["EmployeesId"]);
            else
            {
                EmployeeId = 0;
            }

            byte[] bytes = _transactionalReportService.MOWiseSalesReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), EmployeeId, reportType);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult MOWiseCustomerDue(FormCollection formCollection)
        {
            int reportType = 0;
            int EmployeeId = 0;

            if (formCollection["ReportType"] != null)
                reportType = Convert.ToInt32(formCollection["ReportType"].ToString());
            else
                reportType = 0;

            if (formCollection["EmployeesId"] != null && formCollection["EmployeesId"] != "")
                EmployeeId = Convert.ToInt32(formCollection["EmployeesId"]);
            else
            {
                EmployeeId = 0;
            }

            byte[] bytes = _transactionalReportService.MOWiseCustomerDueRpt(User.Identity.Name, User.Identity.GetConcernId(), EmployeeId, reportType);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public PartialViewResult ExpenditureReport(FormCollection formCollection)
        {
            int ExpenseIncomeItemID = 0;
            if (!string.IsNullOrEmpty(formCollection["ExpenseItemsId"]))
                ExpenseIncomeItemID = Convert.ToInt32(formCollection["ExpenseItemsId"]);

            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            int reportType = Convert.ToInt32(formCollection["ReportType"].ToString());
            byte[] bytes = _transactionalReportService.ExpenditureReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), reportType, ExpenseIncomeItemID);
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpPost]
        [Authorize]
        public PartialViewResult UpComingScheduleReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            //int reportType = Convert.ToInt32(formCollection["ReportType"].ToString());
            byte[] bytes = _transactionalReportService.UpComingScheduleReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpGet]
        [Authorize]
        public PartialViewResult DefaultingCustomerReportOld(FormCollection formCollection)
        {

            byte[] bytes = _transactionalReportService.DefaultingCustomerReport(DateTime.Today, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpPost]
        [Authorize]
        public PartialViewResult DefaultingCustomerReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            byte[] bytes = _transactionalReportService.DefaultingCustomerReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpPost]
        [Authorize]
        public PartialViewResult InstallmentCollectionReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            //int reportType = Convert.ToInt32(formCollection["ReportType"].ToString());
            byte[] bytes = _transactionalReportService.InstallmentCollectionReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpPost]
        [Authorize]
        public PartialViewResult SuplierWisePurchaseReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            int reportType = 1;
            int SupplierId = Convert.ToInt32(formCollection["SuppliersId"]);

            byte[] bytes = _transactionalReportService.SuplierWisePurchaseReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), reportType, SupplierId);
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public PartialViewResult ConcernWiseCustomerDueRpt(FormCollection formCollection)
        {
            int reportType = 0, DueType = 0, CustomerId = 0;

            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                reportType = Convert.ToInt32(formCollection["ReportType"].ToString());

            if (!string.IsNullOrEmpty(formCollection["CustomerId"]))
                CustomerId = Convert.ToInt32(formCollection["CustomerId"]);

            if (!string.IsNullOrEmpty(formCollection["DueType"]))
                DueType = Convert.ToInt32(formCollection["DueType"]);


            byte[] bytes = _basicReportService.CustomerCategoryWiseDueRpt(User.Identity.Name, User.Identity.GetConcernId(), CustomerId, reportType, DueType);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public PartialViewResult ConcernWiseSupplierDueRpt(FormCollection formCollection)
        {
            int reportType = 0;
            int SupplierId = 0;

            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                reportType = Convert.ToInt32(formCollection["ReportType"].ToString());

            if (!string.IsNullOrEmpty(formCollection["SuppliersId"]))
                SupplierId = Convert.ToInt32(formCollection["SuppliersId"]);

            byte[] bytes = _basicReportService.ConcernWiseSupplierDueRpt(User.Identity.Name, User.Identity.GetConcernId(), SupplierId, reportType);
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public PartialViewResult CashCollectionReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            int reportType = 0;

            int CustomerId = 0;

            //if (formCollection["ReportType"] != null)
            //{
            //    reportType = Convert.ToInt32(formCollection["ReportType"].ToString());
            //    formCollection["CustomerId"] = "0";

            //}
            //else
            //{
            if (!string.IsNullOrEmpty(formCollection["CustomerId"]))
            {
                reportType = 0;
                CustomerId = Convert.ToInt32(formCollection["CustomerId"]);
            }
            else
                CustomerId = 0;

            //}
            byte[] bytes = _transactionalReportService.CashCollectionReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), CustomerId, 0);
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public PartialViewResult CashDeliveryReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            int reportType = 0, SupplierId = 0;

            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                reportType = Convert.ToInt32(formCollection["ReportType"].ToString());

            if (!string.IsNullOrEmpty(formCollection["SuppliersId"]))
                SupplierId = Convert.ToInt32(formCollection["SuppliersId"]);

            byte[] bytes = _transactionalReportService.CashDeliverReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), SupplierId, reportType);
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult MOWiseSDetailsReport(FormCollection formCollection)
        {

            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            int EmployeeId = 0;

            if (formCollection["EmployeesId"] != null && formCollection["EmployeesId"] != "")
                EmployeeId = Convert.ToInt32(formCollection["EmployeesId"]);
            else
            {
                EmployeeId = 0;
            }

            byte[] bytes = _transactionalReportService.MOWiseSDetailReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), EmployeeId);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult ProductWisePriceProtection(FormCollection formCollection)
        {

            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            byte[] bytes = _transactionalReportService.ProductWisePriceProtection(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId());

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }
        [HttpPost]
        [Authorize]
        public PartialViewResult ProductWisePandSReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            int productId = Convert.ToInt32(formCollection["ProductsId"]);

            byte[] bytes = _transactionalReportService.ProductWisePandSReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), productId);
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult SRVisitStatusReport(FormCollection formCollection)
        {

            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            int EmployeeId = 0;

            if (formCollection["EmployeesId"] != null && formCollection["EmployeesId"] != "")
                EmployeeId = Convert.ToInt32(formCollection["EmployeesId"]);
            else
            {
                EmployeeId = 0;
            }

            byte[] bytes = _transactionalReportService.SRVisitStatusReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), EmployeeId);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult SRWiseCustomerSalesSummary(FormCollection formCollection)
        {
            int EmployeeID = 0;
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            if (!string.IsNullOrEmpty(formCollection["EmployeesId"]))
                EmployeeID = Convert.ToInt32(formCollection["EmployeesId"].ToString());
            byte[] bytes = _transactionalReportService.SRWiseCustomerSalesSummary(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), EmployeeID);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }



        [HttpPost]
        [Authorize]
        public ActionResult BankLedger(FormCollection formCollection)
        {
            int BankID = 0;
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            if (!string.IsNullOrEmpty(formCollection["BanksId"]))
                BankID = Convert.ToInt32(formCollection["BanksId"]);
            byte[] bytes = _transactionalReportService.BankLedgerReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), BankID);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult CustomerDueReport(FormCollection formCollection)
        {
            int CustomerID = 0, IsOnlyDue = 0;
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            if (!string.IsNullOrEmpty(formCollection["CustomerId"]))
                CustomerID = Convert.ToInt32(formCollection["CustomerId"]);

            if (!string.IsNullOrEmpty(formCollection["DueType"]))
                IsOnlyDue = Convert.ToInt32(formCollection["DueType"]);
            byte[] bytes = _transactionalReportService.CustomerDueReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), CustomerID, IsOnlyDue);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult DailyStockVSSalesSummary(FormCollection formCollection)
        {
            int ProductID = 0;
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            if (!string.IsNullOrEmpty(formCollection["ProductsId"]))
                ProductID = Convert.ToInt32(formCollection["ProductsId"].ToString());
            byte[] bytes = _transactionalReportService.DailyStockVSSalesSummary(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), ProductID);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }



        [HttpPost]
        [Authorize]
        public ActionResult BankSummaryReport(FormCollection formCollection)
        {
            int ProductID = 0;
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            if (!string.IsNullOrEmpty(formCollection["ProductsId"]))
                ProductID = Convert.ToInt32(formCollection["ProductsId"].ToString());
            byte[] bytes = _transactionalReportService.BankSummaryReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), ProductID);

            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }
        //[HttpPost]
        //[Authorize]
        //public ActionResult DailyCashBookLedgerReport(FormCollection formCollection)
        //{
        //    DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
        //    DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
        //    byte[] bytes = _transactionalReportService.DailyCashBookLedger(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId());

        //    TempData["ReportData"] = bytes;
        //    return PartialView("~/Views/Shared/_ReportViewer.cshtml");

        //}

        [HttpGet]
        [Authorize]
        public PartialViewResult ReplacementInvoice()
        {
            IEnumerable<ReplaceOrderDetail> rorderdetails = (IEnumerable<ReplaceOrderDetail>)TempData["ReplacementInvoicedetails"];
            ReplaceOrder ROrder = (ReplaceOrder)TempData["ReplacementInvoice"];
            byte[] bytes = _transactionalReportService.ReplacementInvoiceReport(rorderdetails, ROrder, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }


        [HttpGet]
        [Authorize]
        public PartialViewResult ReplaceInvoiceById()
        {
            int orderId = (int)TempData["OrderId"];
            byte[] bytes = _transactionalReportService.ReplaceInvoiceReportByID(orderId, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpGet]
        [Authorize]
        public PartialViewResult ReturnInvoice()
        {
            IEnumerable<ReplaceOrderDetail> rorderdetails = (IEnumerable<ReplaceOrderDetail>)TempData["ReturnInvoicedetails"];
            ReplaceOrder ROrder = (ReplaceOrder)TempData["ReturnInvoice"];
            byte[] bytes = _transactionalReportService.ReturnInvoiceReport(rorderdetails, ROrder, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }


        [HttpGet]
        [Authorize]
        public PartialViewResult ReturnInvoiceById()
        {
            int orderId = (int)TempData["OrderId"];
            byte[] bytes = _transactionalReportService.ReturnInvoiceReportByID(orderId, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }


        [HttpPost]
        [Authorize]
        public ActionResult DailyWorkSheet(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            byte[] bytes = _transactionalReportService.DailyWorkSheet(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId());

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult SRVisitReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            int EmployeeID = 0, ReportType = 0;
            if (!string.IsNullOrEmpty(formCollection["EmployeesId"]))
                EmployeeID = int.Parse(formCollection["EmployeesId"]);

            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                ReportType = int.Parse(formCollection["ReportType"]);
            byte[] bytes = _transactionalReportService.SRVisitReportDetails(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), EmployeeID, ReportType);

            //bytes = _transactionalReportService.SRVisitReportUsingSP(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), EmployeeID);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult SRWiseCustomerStatusReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            int EmployeeID = 0;
            if (!string.IsNullOrEmpty(formCollection["EmployeesId"]))
            {
                EmployeeID = int.Parse(formCollection["EmployeesId"]);
            }
            byte[] bytes = _transactionalReportService.SRWiseCustomerStatusReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), EmployeeID);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult ReplacementReport(FormCollection formCollection)
        {
            int CustomerID = 0;
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 00:00:00.000");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 00:00:00.000");
            if (!string.IsNullOrEmpty(formCollection["CustomerId"]))
                CustomerID = Convert.ToInt32(formCollection["CustomerId"]);
            byte[] bytes = _transactionalReportService.ReplacementReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), CustomerID);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult ReturnReport(FormCollection formCollection)
        {
            int CustomerID = 0;
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            if (!string.IsNullOrEmpty(formCollection["CustomerId"]))
                CustomerID = Convert.ToInt32(formCollection["CustomerId"]);
            byte[] bytes = _transactionalReportService.ReturntReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), CustomerID);

            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpGet]
        [Authorize]
        public ActionResult MoneyReceipt()
        {
            bool isPos = false;
            var CashCollection = (CashCollection)TempData["MoneyReceiptData"];                
            byte[] bytes = _transactionalReportService.CashCollectionMoneyReceipt(CashCollection, User.Identity.Name, User.Identity.GetConcernId(), isPos);
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpGet]
        [Authorize]
        public ActionResult MoneyReceiptByID()
        {
           bool isPos = false;

            var CashCollectionID = (int)TempData["CashCollectionID"];     
            byte[] bytes = _transactionalReportService.CashCollectionMoneyReceiptByID(CashCollectionID, User.Identity.Name, User.Identity.GetConcernId(), isPos);
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpGet]
        [Authorize]
        public ActionResult CashDeliveryReceipt()
        {
            var CashCollectionID = (int)TempData["CashCollectionID"];
            byte[] bytes = _transactionalReportService.CashDeliveryMoneyReceiptPrint(CashCollectionID, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpGet]
        [Authorize]
        public ActionResult CreditMoneyReceipt()
        {
            var creditSales = (CreditSale)TempData["MoneyReceiptData"];
            var details = (List<CreditSaleDetails>)TempData["Details"];
            var CreditSalesSchedules = (CreditSalesSchedule)TempData["creditsalesSchedules"];

            byte[] bytes = _transactionalReportService.CrditSalesMoneyReceipt(creditSales, details, CreditSalesSchedules, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }
        [HttpGet]
        [Authorize]
        public ActionResult CreditSalesMoneyReceiptByID()
        {
            int CreditSalesID = (int)TempData["OrderId"];

            byte[] bytes = _transactionalReportService.CrditSalesMoneyReceiptByID(CreditSalesID, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpPost]
        [Authorize]
        public ActionResult MonthlyBenefit(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            byte[] bytes = _transactionalReportService.MonthlyBenefit(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }


        [HttpPost]
        [Authorize]
        public ActionResult ProductWiseBenefit(FormCollection formCollection)
        {
            int ProductID = 0;
            if (!string.IsNullOrEmpty(formCollection["ProductsId"]))
                ProductID = Convert.ToInt32(formCollection["ProductsId"]);
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            byte[] bytes = _transactionalReportService.ProductWiseBenefitReport(fromDate, toDate, ProductID, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpPost]
        [Authorize]
        public ActionResult ProductWiseSalesReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            int CustomerID = 0;

            if (!string.IsNullOrEmpty(formCollection["CustomerId"]))
                CustomerID = Convert.ToInt32(formCollection["CustomerId"]);
            byte[] bytes = _transactionalReportService.ProductWiseSalesReport(fromDate, toDate, CustomerID, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult ProductWisePurchaseReport(FormCollection formCollection)
        {

            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            int SupplierID = 0;

            if (!string.IsNullOrEmpty(formCollection["SuppliersId"]))
                SupplierID = Convert.ToInt32(formCollection["SuppliersId"]);

            EnumPurchaseType PurchaseType = 0;
            if (!string.IsNullOrEmpty(formCollection["PurchaseType"]))
                Enum.TryParse(formCollection["PurchaseType"], out PurchaseType);

            byte[] bytes = _transactionalReportService.ProductWisePurchaseReport(fromDate, toDate, SupplierID, User.Identity.Name, User.Identity.GetConcernId(), PurchaseType);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }


        [HttpPost]
        [Authorize]
        public ActionResult DamageProductReport(FormCollection formCollection)
        {
            int CustomerID = 0;
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 00:00:00.000");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 00:00:00.000");
            if (!string.IsNullOrEmpty(formCollection["CustomerId"]))
                CustomerID = Convert.ToInt32(formCollection["CustomerId"]);
            byte[] bytes = _transactionalReportService.DamageProductReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), CustomerID);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public PartialViewResult SRWiseCashCollectionReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            int EmployeeID = 0;
            if (!string.IsNullOrEmpty(formCollection["EmployeesId"]))
            {
                EmployeeID = int.Parse(formCollection["EmployeesId"]);
            }

            byte[] bytes = _transactionalReportService.SRWiseCashCollectionReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), EmployeeID);
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public PartialViewResult ProductwiseSalesDetails(FormCollection formCollection)
        {
            int reportType = 0, CompanyID = 0, CategoryID = 0, ProductsId = 0, IsSummaryReport = 0;

            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                reportType = Convert.ToInt32(formCollection["ReportType"].ToString());
            if (!string.IsNullOrEmpty(formCollection["CompaniesId"]))
                CompanyID = Convert.ToInt32(formCollection["CompaniesId"].ToString());
            if (!string.IsNullOrEmpty(formCollection["CategoriesId"]))
                CategoryID = Convert.ToInt32(formCollection["CategoriesId"].ToString());
            if (!string.IsNullOrEmpty(formCollection["ProductsId"]))
                ProductsId = Convert.ToInt32(formCollection["ProductsId"].ToString());

            if (!string.IsNullOrEmpty(formCollection["IsSummaryReport"]))
                IsSummaryReport = Convert.ToInt32(formCollection["IsSummaryReport"].ToString());
            byte[] stock = null;
            if (IsSummaryReport == 2)
                stock = _transactionalReportService.ProductwiseSalesDetails(User.Identity.Name, User.Identity.GetConcernId(), reportType, CompanyID, CategoryID, ProductsId, fromDate, toDate);
            else
                stock = _transactionalReportService.ProductwiseSalesSummary(User.Identity.Name, User.Identity.GetConcernId(), reportType, CompanyID, CategoryID, ProductsId, fromDate, toDate);

            TempData["ReportData"] = stock;
            return CustomPartialView();
        }

        [HttpPost]
        [Authorize]
        public PartialViewResult ProductWisePurchaseDetailsReport(FormCollection formCollection)
        {
            int reportType = 0, CompanyID = 0, CategoryID = 0, ProductsId = 0;

            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                reportType = Convert.ToInt32(formCollection["ReportType"].ToString());
            if (!string.IsNullOrEmpty(formCollection["CompaniesId"]))
                CompanyID = Convert.ToInt32(formCollection["CompaniesId"].ToString());
            if (!string.IsNullOrEmpty(formCollection["CategoriesId"]))
                CategoryID = Convert.ToInt32(formCollection["CategoriesId"].ToString());
            if (!string.IsNullOrEmpty(formCollection["ProductsId"]))
                ProductsId = Convert.ToInt32(formCollection["ProductsId"].ToString());

            EnumPurchaseType PurchaseType = 0;
            if (!string.IsNullOrEmpty(formCollection["PurchaseType"]))
                Enum.TryParse(formCollection["PurchaseType"], out PurchaseType);

            byte[] stock = _transactionalReportService.ProductWisePurchaseDetailsReport(User.Identity.Name, User.Identity.GetConcernId(), reportType, CompanyID, CategoryID, ProductsId, fromDate, toDate, PurchaseType);
            TempData["ReportData"] = stock;
            return CustomPartialView();
        }

        [HttpPost]
        [Authorize]
        public ActionResult DailyCashBookLedgerReport(FormCollection formCollection)
        {
            // DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");


            DateTime fromDate = DateTime.MinValue;
            DateTime ToDate = DateTime.MinValue;
            int ReportType = 0;
            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                ReportType = Convert.ToInt32(formCollection["ReportType"]);
            if (ReportType == 1)
            {
                fromDate = Convert.ToDateTime(formCollection["FromDate"]);
                ToDate = fromDate;
            }
            else if (ReportType == 2)
            {
                var DateRange = GetFirstAndLastDateOfMonth(Convert.ToDateTime(formCollection["Month"]));
                fromDate = DateRange.Item1;
                ToDate = DateRange.Item2;
            }
            else if (ReportType == 3)
            {

                var DateRange = GetFirstAndLastDateOfYear(Convert.ToInt32(formCollection["Year"]));
                fromDate = DateRange.Item1;
                ToDate = DateRange.Item2;


                //fromDate = Convert.ToDateTime(formCollection["Year"]);
                //fromDate = new DateTime(fromDate.Year, 1, 1);
                //fromDate = new DateTime(fromDate.Year, 12, 31);
            }

            //   DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            byte[] bytes = _transactionalReportService.DailyCashBookLedger(fromDate, ToDate, User.Identity.Name, User.Identity.GetConcernId());

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult ProfitAndLossReport(FormCollection formCollection)
        {
            // DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");


            DateTime fromDate = DateTime.MinValue;
            DateTime ToDate = DateTime.MinValue;
            int ReportType = 0;
            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                ReportType = Convert.ToInt32(formCollection["ReportType"]);
            if (ReportType == 1)
            {
                fromDate = Convert.ToDateTime(formCollection["FromDate"]);
                ToDate = fromDate;
            }
            else if (ReportType == 2)
            {
                var DateRange = GetFirstAndLastDateOfMonth(Convert.ToDateTime(formCollection["Month"]));
                fromDate = DateRange.Item1;
                ToDate = DateRange.Item2;
            }
            else if (ReportType == 3)
            {

                var DateRange = GetFirstAndLastDateOfYear(Convert.ToInt32(formCollection["Year"]));
                fromDate = DateRange.Item1;
                ToDate = DateRange.Item2;


                //fromDate = Convert.ToDateTime(formCollection["Year"]);
                //fromDate = new DateTime(fromDate.Year, 1, 1);
                //fromDate = new DateTime(fromDate.Year, 12, 31);
            }

            //   DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            byte[] bytes = _transactionalReportService.ProfitAndLossReport(fromDate, ToDate, User.Identity.Name, User.Identity.GetConcernId());

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult SummaryReport(FormCollection formCollection)
        {
            // DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");


            DateTime fromDate = DateTime.MinValue;
            DateTime ToDate = DateTime.MinValue;
            int ReportType = 0;
            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                ReportType = Convert.ToInt32(formCollection["ReportType"]);
            if (ReportType == 1)
            {
                fromDate = Convert.ToDateTime(formCollection["FromDate"]);
                ToDate = fromDate;
            }
            else if (ReportType == 2)
            {
                var DateRange = GetFirstAndLastDateOfMonth(Convert.ToDateTime(formCollection["Month"]));
                fromDate = DateRange.Item1;
                ToDate = DateRange.Item2;
            }
            else if (ReportType == 3)
            {

                var DateRange = GetFirstAndLastDateOfYear(Convert.ToInt32(formCollection["Year"]));
                fromDate = DateRange.Item1;
                ToDate = DateRange.Item2;


                //fromDate = Convert.ToDateTime(formCollection["Year"]);
                //fromDate = new DateTime(fromDate.Year, 1, 1);
                //fromDate = new DateTime(fromDate.Year, 12, 31);
            }

            //   DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            byte[] bytes = _transactionalReportService.SummaryReport(fromDate, ToDate, User.Identity.Name, User.Identity.GetConcernId());

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }


        //public PartialViewResult BankTransactionReport(FormCollection formCollection)
        //{
        //    int reportType = 0, BankID = 0, CategoryID = 0, ProductsId = 0;

        //    DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
        //    DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

        //    if (!string.IsNullOrEmpty(formCollection["BanksId"]))
        //        BankID = Convert.ToInt32(formCollection["BanksId"].ToString());
        //    byte[] stock = _transactionalReportService.BankTransactionReport(User.Identity.Name, User.Identity.GetConcernId(), reportType, BankID, fromDate, toDate);
        //    TempData["ReportData"] = stock;
        //    return PartialView("~/Views/Shared/_ReportViewer.cshtml");
        //}
        public PartialViewResult BankTransactionReport(FormCollection formCollection, bool isBT = false)
        {
            int BankID = 0;

            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            if (!string.IsNullOrEmpty(formCollection["BanksId"]))
                BankID = Convert.ToInt32(formCollection["BanksId"].ToString());
            int concernId = User.Identity.GetConcernId();
            int reportConcernId = 0;
            if (!string.IsNullOrEmpty(formCollection["ConcernId"]))
                reportConcernId = Convert.ToInt32(formCollection["ConcernId"].ToString());
            reportConcernId = isBT ? concernId : reportConcernId;
            byte[] stock = _transactionalReportService.NewBankTransactionsReport(fromDate, toDate, BankID, User.Identity.Name, concernId, reportConcernId);
            TempData["ReportData"] = stock;
            return CustomPartialView();
        }

        //[Authorize]
        //[HttpGet]
        //public ActionResult PurchaseInvoice()
        //{
        //    POrder PorderData = (POrder)TempData["POInvoiceData"];
        //    //byte[] bytes = _transactionalReportService.POInvoice(PorderData, User.Identity.Name, User.Identity.GetConcernId());
        //    byte[] bytes = _transactionalReportService.POInvoiceByID(PorderData.POrderID, User.Identity.Name, User.Identity.GetConcernId());
        //    TempData["ReportData"] = bytes;
        //    return CustomPartialView();
        //}

        //public ActionResult PurchaseInvoiceById()
        //{
        //    int POrderID = (int)TempData["POrderID"];
        //    byte[] bytes = _transactionalReportService.POInvoiceByID(POrderID, User.Identity.Name, User.Identity.GetConcernId());

        //    TempData["ReportData"] = bytes;
        //    return CustomPartialView();
        //}

        [Authorize]
        [HttpGet]
        public ActionResult PurchaseInvoice()
        {
            POrder PorderData = (POrder)TempData["POInvoiceData"];
            bool isPreview = TempData.ContainsKey("IsPreview") ? (bool)TempData["IsPreview"] : false;
            byte[] bytes = _transactionalReportService.POInvoice(PorderData, User.Identity.Name, User.Identity.GetConcernId(), isPreview);

            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        public ActionResult PurchaseInvoiceById()
        {
            if (TempData["POrderID"] == null)
            {
                return HttpNotFound("POrderID is not available in TempData.");
            }

            int POrderID;
            if (!int.TryParse(TempData["POrderID"].ToString(), out POrderID))
            {
                return HttpNotFound("Invalid POrderID in TempData.");
            }

            byte[] bytes = _transactionalReportService.POInvoiceByID(POrderID, User.Identity.Name, User.Identity.GetConcernId());

            if (bytes == null || bytes.Length == 0)
            {
                return HttpNotFound("Report data not found for the given POrderID.");
            }

            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpPost]
        [Authorize]
        public ActionResult GetDamagePOReport(FormCollection formCollection)
        {
            int SupplierID = 0;
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            if (!string.IsNullOrEmpty(formCollection["SuppliersId"]))
                SupplierID = Convert.ToInt32(formCollection["SuppliersId"]);

            byte[] bytes = _transactionalReportService.GetDamagePOReport(User.Identity.Name, User.Identity.GetConcernId(), SupplierID, fromDate, toDate);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult GetDamageReturnPOReport(FormCollection formCollection)
        {
            int SupplierID = 0;
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            if (!string.IsNullOrEmpty(formCollection["SuppliersId"]))
                SupplierID = Convert.ToInt32(formCollection["SuppliersId"]);

            byte[] bytes = _transactionalReportService.GetDamageReturnPOReport(User.Identity.Name, User.Identity.GetConcernId(), SupplierID, fromDate, toDate);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult GetSalarySheet(FormCollection formCollection)
        {
            List<int> EmployeeIDList = new List<int>();
            int EmployeeID = 0, DepartmentID = 0;
            DateTime SalaryMonth = DateTime.MinValue;
            if (!string.IsNullOrEmpty(formCollection["EmployeeIdList"]))
            {
                EmployeeIDList = formCollection["EmployeeIdList"].Split(new char[] { ',' }).Select(Int32.Parse).Distinct().ToList();
            }

            if (!string.IsNullOrEmpty(formCollection["EmployeesId"]))
            {
                EmployeeID = Convert.ToInt32(formCollection["EmployeesId"]);
            }
            if (!string.IsNullOrEmpty(formCollection["SalaryMonth"]))
                SalaryMonth = Convert.ToDateTime(formCollection["SalaryMonth"]);
            if (!string.IsNullOrEmpty(formCollection["DepartmentsId"]))
                DepartmentID = Convert.ToInt32(formCollection["DepartmentsId"]);
            var DateRange = GetFirstAndLastDateOfMonth(SalaryMonth);
            byte[] bytes = _transactionalReportService.GetSalarySheet(SalaryMonth, EmployeeID, DepartmentID, EmployeeIDList, User.Identity.Name, User.Identity.GetConcernId(), DateRange);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }
        [HttpPost]
        [Authorize]
        public ActionResult GetPaySlip(FormCollection formCollection)
        {
            int EmployeeID = 0, DepartmentID = 0;
            DateTime SalaryMonth = DateTime.MinValue;

            if (!string.IsNullOrEmpty(formCollection["EmployeesId"]))
            {
                EmployeeID = Convert.ToInt32(formCollection["EmployeesId"]);
            }
            if (!string.IsNullOrEmpty(formCollection["SalaryMonth"]))
                SalaryMonth = Convert.ToDateTime(formCollection["SalaryMonth"]);

            var DateRange = GetFirstAndLastDateOfMonth(SalaryMonth);
            byte[] bytes = _transactionalReportService.GetPaySlip(SalaryMonth, EmployeeID, User.Identity.Name, User.Identity.GetConcernId(), DateRange);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public PartialViewResult AdminDailySalesReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            int reportType = Convert.ToInt32(formCollection["ReportType"].ToString());
            int CustomerType = 0;
            if (!string.IsNullOrEmpty(formCollection["CustomerType"]))
                CustomerType = Convert.ToInt32(formCollection["CustomerType"]);


            int ConcernID = 0;
            if (!string.IsNullOrEmpty(formCollection["ConcernID"]))
                ConcernID = Convert.ToInt32(formCollection["ConcernID"]);

            byte[] bytes = _transactionalReportService.SalesReportAdmin(fromDate, toDate, User.Identity.Name, ConcernID, User.Identity.GetConcernId(), CustomerType, reportType);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }
        [HttpPost]
        [Authorize]
        public PartialViewResult AdminPOReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            int ConcernID = 0;
            if (!string.IsNullOrEmpty(formCollection["ConcernID"]))
                ConcernID = Convert.ToInt32(formCollection["ConcernID"]);

            byte[] bytes = _transactionalReportService.AdminPurchaseReport(fromDate, toDate, User.Identity.Name, ConcernID, User.Identity.GetConcernId());

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }
        [HttpPost]
        [Authorize]
        public PartialViewResult AdminCustomerDueRpt(FormCollection formCollection)
        {
            int CustomerType = 0, DueType = 0, ConcernID = 0;

            if (!string.IsNullOrEmpty(formCollection["CustomerType"]))
                CustomerType = Convert.ToInt32(formCollection["CustomerType"].ToString());

            if (!string.IsNullOrEmpty(formCollection["ConcernID"]))
                ConcernID = Convert.ToInt32(formCollection["ConcernID"]);

            if (!string.IsNullOrEmpty(formCollection["DueType"]))
                DueType = Convert.ToInt32(formCollection["DueType"]);


            byte[] bytes = _transactionalReportService.AdminCustomerDueRpt(User.Identity.Name, ConcernID, User.Identity.GetConcernId(), CustomerType, DueType);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }
        [HttpPost]
        [Authorize]
        public PartialViewResult AdminCashColletions(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            int ConcernID = 0;
            if (!string.IsNullOrEmpty(formCollection["ConcernID"]))
                ConcernID = Convert.ToInt32(formCollection["ConcernID"]);

            byte[] bytes = _transactionalReportService.AdminCashCollectionReport(User.Identity.Name, ConcernID, User.Identity.GetConcernId(), fromDate, toDate);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }


        public PartialViewResult CashInHandReport(FormCollection formCollection)
        {
            DateTime fromDate = DateTime.MinValue;
            DateTime ToDate = DateTime.MinValue;
            int ReportType = 0, CustomerType = 0;
            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                ReportType = Convert.ToInt32(formCollection["ReportType"]);

            if (!string.IsNullOrEmpty(formCollection["CustomerType"]))
                CustomerType = Convert.ToInt32(formCollection["CustomerType"]);

            if (ReportType == 1)
            {
                fromDate = Convert.ToDateTime(formCollection["FromDate"]);
                ToDate = fromDate;
            }
            else if (ReportType == 2)
            {
                var DateRange = GetFirstAndLastDateOfMonth(Convert.ToDateTime(formCollection["Month"]));
                fromDate = DateRange.Item1;
                ToDate = DateRange.Item2;
            }
            else if (ReportType == 3)
            {
                int year = Convert.ToInt32(formCollection["Year"]);
                fromDate = new DateTime(year, 1, 1);
                ToDate = new DateTime(year, 12, 31, 23, 59, 59);
            }

            byte[] bytes = _transactionalReportService.CashInHandReport(User.Identity.Name, User.Identity.GetConcernId(), ReportType, fromDate, ToDate, CustomerType);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }


        public PartialViewResult CashInHandReportAdmin(FormCollection formCollection)
        {
            DateTime fromDate = DateTime.MinValue;
            DateTime ToDate = DateTime.MinValue;


            int CustomerType = 0;
            if (!string.IsNullOrEmpty(formCollection["CustomerType"]))
                CustomerType = Convert.ToInt32(formCollection["CustomerType"]);

            int ReportType = 0;
            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                ReportType = Convert.ToInt32(formCollection["ReportType"]);
            if (ReportType == 1)
            {
                fromDate = Convert.ToDateTime(formCollection["FromDate"]);
                ToDate = fromDate;
            }
            else if (ReportType == 2)
            {
                var DateRange = GetFirstAndLastDateOfMonth(Convert.ToDateTime(formCollection["Month"]));
                fromDate = DateRange.Item1;
                ToDate = DateRange.Item2;
            }
            else if (ReportType == 3)
            {
                fromDate = Convert.ToDateTime(formCollection["Year"]);
                fromDate = new DateTime(fromDate.Year, 1, 1);
                fromDate = new DateTime(fromDate.Year, 12, 31);
            }

            byte[] bytes = _transactionalReportService.CashInHandReportAdmin(User.Identity.Name, User.Identity.GetConcernId(), ReportType, fromDate, ToDate, CustomerType);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpGet]
        [Authorize]
        public PartialViewResult BankTransMoneyReceipt()
        {
            int BankTranID = (int)TempData["BankTranID"];

            byte[] bytes = _transactionalReportService.BankTransMoneyReceipt(User.Identity.Name, User.Identity.GetConcernId(), BankTranID);
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }
        [HttpGet]
        [Authorize]
        public PartialViewResult ExpenseMoneyReceipt()
        {
            int ExpenditureID = (int)TempData["ExpenditureID"];

            byte[] bytes = _transactionalReportService.ExpenseIncomeMoneyReceipt(User.Identity.Name, User.Identity.GetConcernId(), ExpenditureID, true);
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }
        [HttpGet]
        [Authorize]
        public PartialViewResult IncomeMoneyReceipt()
        {
            int ExpenditureID = (int)TempData["ExpenditureID"];

            byte[] bytes = _transactionalReportService.ExpenseIncomeMoneyReceipt(User.Identity.Name, User.Identity.GetConcernId(), ExpenditureID, false);
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public PartialViewResult DailyAttendence(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            int DepartmentID = 0;
            bool IsPresent = false, IsAbsent = false;
            if (!string.IsNullOrEmpty(formCollection["DepartmentsId"]))
                DepartmentID = Convert.ToInt32(formCollection["DepartmentsId"]);
            if (!string.IsNullOrEmpty(formCollection["IsPresent"]))
                IsPresent = Convert.ToInt32(formCollection["IsPresent"]) > 0 ? true : false;
            if (!string.IsNullOrEmpty(formCollection["IsAbsent"]))
                IsAbsent = Convert.ToInt32(formCollection["IsAbsent"]) > 0 ? true : false;

            byte[] bytes = _transactionalReportService.DailyAttendence(User.Identity.Name, User.Identity.GetConcernId(), DepartmentID, fromDate, IsPresent, IsAbsent);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        //[HttpPost]
        //[Authorize]
        //public PartialViewResult StockLedgerReport(FormCollection formCollection)
        //{
        //    int reportType = 0, CompanyID = 0, CategoryID = 0, ProductsId = 0;
        //    DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
        //    DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

        //    if (!string.IsNullOrEmpty(formCollection["ReportType"]))
        //        reportType = Convert.ToInt32(formCollection["ReportType"].ToString());
        //    if (!string.IsNullOrEmpty(formCollection["CompaniesId"]))
        //        CompanyID = Convert.ToInt32(formCollection["CompaniesId"].ToString());
        //    if (!string.IsNullOrEmpty(formCollection["CategoriesId"]))
        //        CategoryID = Convert.ToInt32(formCollection["CategoriesId"].ToString());
        //    if (!string.IsNullOrEmpty(formCollection["ProductsId"]))
        //        ProductsId = Convert.ToInt32(formCollection["ProductsId"].ToString());
        //    byte[] stock = _transactionalReportService.StockLedgerReport(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), reportType, CompanyID, CategoryID, ProductsId);
        //    TempData["ReportData"] = stock;
        //    return CustomPartialView();
        //}

        [HttpPost]
        [Authorize]
        public PartialViewResult StockLedgerReport(FormCollection formCollection)
        {
            int reportType = 0;
            string ProductName = string.Empty, CategoryName = string.Empty, CompanyName = string.Empty;

            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                reportType = Convert.ToInt32(formCollection["ReportType"].ToString());

            if (!string.IsNullOrEmpty(formCollection["CompanyName"]))
                CompanyName = formCollection["CompanyName"];

            if (!string.IsNullOrEmpty(formCollection["CategoriesName"]))
                CategoryName = formCollection["CategoriesName"];
            if (!string.IsNullOrEmpty(formCollection["ProductsName"]))
                ProductName = formCollection["ProductsName"];

            int ConcernID = 0;
            if (!string.IsNullOrEmpty(formCollection["ConcernID"]))
                ConcernID = Convert.ToInt32(formCollection["ConcernID"]);
            else
                ConcernID = User.Identity.GetConcernId();

            byte[] stock = _transactionalReportService.StockLedgerReport(fromDate, toDate, User.Identity.Name, ConcernID, reportType, CompanyName, CategoryName, ProductName);
            TempData["ReportData"] = stock;
            return CustomPartialView();
        }



        [HttpPost]
        [Authorize]
        public PartialViewResult SupplierLedger(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            int SupplierID = 0;
            if (!string.IsNullOrEmpty(formCollection["SuppliersId"]))
                SupplierID = Convert.ToInt32(formCollection["SuppliersId"]);

            byte[] bytes = _transactionalReportService.SupplierLedger(User.Identity.Name, User.Identity.GetConcernId(), fromDate, toDate, SupplierID);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }


        [HttpPost]
        [Authorize]
        public ActionResult CustomerLedger(FormCollection formCollection)
        {
            int CustomerID = 0, ReportType = 0;
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            if (!string.IsNullOrEmpty(formCollection["CustomerId"]))
                CustomerID = Convert.ToInt32(formCollection["CustomerId"]);
            if (!string.IsNullOrEmpty(formCollection["IsSummaryReport"]))
                ReportType = Convert.ToInt32(formCollection["IsSummaryReport"]);
            byte[] bytes = null;

            //if (ReportType == 1) //Summary
            //    bytes = _transactionalReportService.CustomerLedgerSummary(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), CustomerID);
            //else
            //    bytes = _transactionalReportService.CustomerLedgerDetails(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), CustomerID);


            bytes = _transactionalReportService.CustomerLedger(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), CustomerID);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult BothCustomerLedger(FormCollection formCollection)
        {
            int CustomerID = 0, ReportType = 0;
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            if (!string.IsNullOrEmpty(formCollection["CustomerId"]))
                CustomerID = Convert.ToInt32(formCollection["CustomerId"]);
            if (!string.IsNullOrEmpty(formCollection["IsSummaryReport"]))
                ReportType = Convert.ToInt32(formCollection["IsSummaryReport"]);
            byte[] bytes = null;

            //if (ReportType == 1) //Summary
            //    bytes = _transactionalReportService.CustomerLedgerSummary(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), CustomerID);
            //else
            //    bytes = _transactionalReportService.CustomerLedgerDetails(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), CustomerID);


            bytes = _transactionalReportService.BothCustomerLedger(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), CustomerID);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        public async Task<JsonResult> GetConcernName(DateTime ClientDateTime)
        {
            TempData["ClientDateTime"] = ClientDateTime;
            string ConcernName = string.Empty, expiremsg = string.Empty;
            if (!TempData.ContainsKey("ConcernName"))
            {
                var Concern = await Task.Run(() => _SisterConcernService.GetSisterConcernById(User.Identity.GetConcernId()));
                TempData["ConcernName"] = Concern.Name;
                ConcernName = Concern.Name;

            }
            else
            {
                ConcernName = TempData.Peek("ConcernName").ToString();
            }

            if (!TempData.ContainsKey("ExpireMessage"))
            {
                var sysinfo = _systemInformationService.GetSystemInformationByConcernId(User.Identity.GetConcernId());
                DateTime currentDate = GetLocalDateTime();
                if (sysinfo != null && sysinfo.ExpireDate > currentDate)
                {
                    if ((currentDate.Day >= 1 && currentDate.Day <= sysinfo.ExpireDate.Value.Day) && sysinfo.ExpireDate.Value.Month == currentDate.Month)
                    {
                        expiremsg = sysinfo.WarningMsg;
                        TempData["ExpireMessage"] = sysinfo.WarningMsg;
                    }
                }
                else
                {
                    TempData["ExpireMessage"] = "";
                }

            }
            else
            {
                expiremsg = TempData.Peek("ExpireMessage").ToString();
            }

            if (Session["SystemInfo"] == null)
            {
                var sysInfo = _systemInformationService.GetSystemInformationByConcernId(User.Identity.GetConcernId());
                Session["SystemInfo"] = _mapper.Map<SystemInformation, CreateSystemInformationViewModel>(sysInfo);
            }
            return Json(new { ConcernName, expiremsg }, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult TransferInvoiceByID()
        //{
        //    int TransferID = (int)TempData["TransferID"];
        //    byte[] bytes = _transactionalReportService.TransferInvoiceByID(TransferID, User.Identity.Name, User.Identity.GetConcernId());

        //    TempData["ReportData"] = bytes;
        //    return CustomPartialView();
        //}

        [HttpGet]
        [Authorize]
        public PartialViewResult SalesReturnChallan()
        {
            int ROrderID = (int)TempData["ROrderId"];
            byte[] bytes = _transactionalReportService.SalesReturnChallan(ROrderID, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public PartialViewResult TrialBalance(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            string ClientDateTime = formCollection["ClientDateTime"];
            int ConcernID = string.IsNullOrEmpty(formCollection["ConcernID"]) ? User.Identity.GetConcernId() : Convert.ToInt32(formCollection["ConcernID"]);

            byte[] bytes = _transactionalReportService.GetTrialBalance(fromDate, toDate, User.Identity.Name, ConcernID, ClientDateTime);
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }


        [HttpPost]
        [Authorize]
        public PartialViewResult ProfitLossAccount(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            string ClientDateTime = formCollection["ClientDateTime"];
            int ConcernID = string.IsNullOrEmpty(formCollection["ConcernID"]) ? User.Identity.GetConcernId() : Convert.ToInt32(formCollection["ConcernID"]);

            byte[] bytes = _transactionalReportService.ProfitLossAccount(fromDate, toDate, User.Identity.Name, ConcernID, ClientDateTime);
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }




        [HttpPost]
        [Authorize]
        public ActionResult GetRecieptPayment(FormCollection formCollection)
        {

            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString());
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString());

            byte[] bytes = _transactionalReportService.ReceiptPaymentReport(User.Identity.Name, User.Identity.GetConcernId(), fromDate, toDate);

            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }


        [HttpPost]
        [Authorize]
        public PartialViewResult BalanceSheet(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            string ClientDateTime = formCollection["ClientDateTime"];
            int ConcernID = string.IsNullOrEmpty(formCollection["ConcernID"]) ? User.Identity.GetConcernId() : Convert.ToInt32(formCollection["ConcernID"]);

            byte[] bytes = _transactionalReportService.BalanceSheet(fromDate, toDate, User.Identity.Name, ConcernID, ClientDateTime);
            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult MonthlyTransactionReport(FormCollection formCollection)
        {
            // DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");


            DateTime fromDate = DateTime.MinValue;
            DateTime ToDate = DateTime.MinValue;
            int ReportType = 0;
            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                ReportType = Convert.ToInt32(formCollection["ReportType"]);
            if (ReportType == 1)
            {
                fromDate = Convert.ToDateTime(formCollection["FromDate"]);
                ToDate = fromDate;
            }
            else if (ReportType == 2)
            {
                var DateRange = GetFirstAndLastDateOfMonth(Convert.ToDateTime(formCollection["Month"]));
                fromDate = DateRange.Item1;
                ToDate = DateRange.Item2;
            }
            else if (ReportType == 3)
            {

                var DateRange = GetFirstAndLastDateOfYear(Convert.ToInt32(formCollection["Year"]));
                fromDate = DateRange.Item1;
                ToDate = DateRange.Item2;


                //fromDate = Convert.ToDateTime(formCollection["Year"]);
                //fromDate = new DateTime(fromDate.Year, 1, 1);
                //fromDate = new DateTime(fromDate.Year, 12, 31);
            }

            //   DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            int ConcernID = 0;
            if (!string.IsNullOrEmpty(formCollection["ConcernID"]))
                ConcernID = Convert.ToInt32(formCollection["ConcernID"]);
            else
                ConcernID = User.Identity.GetConcernId();

            byte[] bytes = _transactionalReportService.MonthlyTransactionReport(fromDate, ToDate, User.Identity.Name, ConcernID);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public PartialViewResult LiabilityReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");
            int HeadID = 0;
            if (!string.IsNullOrEmpty(formCollection["SIHID"]))
                HeadID = Convert.ToInt32(formCollection["SIHID"]);
            string OnlyHead = "";
            if (!string.IsNullOrWhiteSpace(formCollection["OnlyHead"]))
                OnlyHead = formCollection["OnlyHead"] == "1" ? OnlyHead : "";

            byte[] bytes = _transactionalReportService.VoucherTransactionLedger(fromDate, toDate, User.Identity.Name, User.Identity.GetConcernId(), HeadID, OnlyHead);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

        [HttpPost]
        [Authorize]
        public ActionResult DOInvoiceReport(FormCollection formCollection)
        {
            int DOID = (int)TempData["DOID"];
            byte[] bytes = _transactionalReportService.DOInvoiceReport(User.Identity.Name, User.Identity.GetConcernId(), DOID);

            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }



        [HttpPost]
        [Authorize]
        public ActionResult DOReport(FormCollection formCollection)
        {
            DateTime fromDate = Convert.ToDateTime(formCollection["FromDate"].ToString() + " 12:00:00 AM");
            DateTime toDate = Convert.ToDateTime(formCollection["ToDate"].ToString() + " 11:59:59 PM");

            int CustomerID = 0, SupplierID = 0, POType = 0;

            if (!string.IsNullOrEmpty(formCollection["CustomerId"]))
                int.TryParse(formCollection["CustomerId"], out CustomerID);

            if (!string.IsNullOrEmpty(formCollection["SuppliersId"]))
                int.TryParse(formCollection["SuppliersId"], out SupplierID);

            if (!string.IsNullOrEmpty(formCollection["chkDOType"]))
                int.TryParse(formCollection["chkDOType"], out POType);

            byte[] bytes = _transactionalReportService.DOReport(User.Identity.Name, User.Identity.GetConcernId(), fromDate, toDate, CustomerID, SupplierID, POType);

            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }




        [HttpGet]
        [Authorize]
        public ActionResult PrintPOSInvoice()
        {

            SOrder sorder = (SOrder)TempData["salesInvoiceData"];
            bool isPreview = TempData.ContainsKey("IsPreview") ? (bool)TempData["IsPreview"] : false;
            TempData["salesInvoiceData"] = sorder;
            int orderId = sorder.SOrderID;

            byte[] bytes = _transactionalReportService.PrintPOSInvoice(orderId, User.Identity.Name, User.Identity.GetConcernId());

            TempData["ReportData"] = bytes;
            return PartialView("~/Views/Shared/_ReportViewer.cshtml");
        }

        [HttpGet]
        [Authorize]
        public ActionResult PrintPOSInvoiceById()
        {

            int POrderID = (int)TempData["POSSOrderID"];

            byte[] bytes = _transactionalReportService.PrintPOSInvoice(POrderID, User.Identity.Name, User.Identity.GetConcernId());

            TempData["ReportData"] = bytes;
            return PartialView("~/Views/Shared/_ReportViewer.cshtml");
        }




        [HttpGet]
        [Authorize]
        public ActionResult PrintPOSInvoiceForPo()
        {

            SOrder sorder = (SOrder)TempData["salesInvoiceData"];
            bool isPreview = TempData.ContainsKey("IsPreview") ? (bool)TempData["IsPreview"] : false;
            TempData["salesInvoiceData"] = sorder;
            int orderId = sorder.SOrderID;

            byte[] bytes = _transactionalReportService.PrintPOSInvoiceForPo(orderId, User.Identity.Name, User.Identity.GetConcernId());

            TempData["ReportData"] = bytes;
            return PartialView("~/Views/Shared/_ReportViewer.cshtml");
        }

        [HttpGet]
        [Authorize]
        public ActionResult PrintPOSInvoiceForPOById()
        {

            int POrderID = (int)TempData["POSSOrderID"];

            byte[] bytes = _transactionalReportService.PrintPOSInvoiceForPo(POrderID, User.Identity.Name, User.Identity.GetConcernId());

            TempData["ReportData"] = bytes;
            return PartialView("~/Views/Shared/_ReportViewer.cshtml");
        }


        [HttpGet]
        [Authorize]
        public PartialViewResult SalesMoneyReceipt()
        {
            SOrder Sorder = (SOrder)TempData["MoneyReceiptData"];
            TempData["MoneyReceiptData"] = Sorder;
            byte[] bytes = _transactionalReportService.SalesOrderMoneyReceipt(Sorder, User.Identity.Name, User.Identity.GetConcernId());
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }


        [HttpGet]
        [Authorize]
        public PartialViewResult SalesMoneyReceiptByID()
        {
            int SOrderID = (int)TempData["SOrderID"];
            bool isPosMoney = (bool)TempData["isPosRecipt"];
            TempData["SOrderID"] = SOrderID;
            byte[] bytes = _transactionalReportService.SOrderMoneyReceiptByID(SOrderID, User.Identity.Name, User.Identity.GetConcernId(), isPosMoney);
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }



        [HttpGet]
        [Authorize]
        public ActionResult MoneyReceiptByIDOfSo()
        {
            var SorderId = (int)TempData["SorderId"];
            bool isPosRecipt = (bool)TempData["isPosRecipt"];
            byte[] bytes = _transactionalReportService.SOrderMoneyReceiptByID(SorderId, User.Identity.Name, User.Identity.GetConcernId(), isPosRecipt);
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }


        [HttpGet]
        [Authorize]
        public ActionResult PosMoneyReceiptByID() 
        {
            bool isPosRecipt = true;
            var CashCollectionID = (int)TempData["CashCollectionID"];
            byte[] bytes = _transactionalReportService.CashCollectionMoneyReceiptByID(CashCollectionID, User.Identity.Name, User.Identity.GetConcernId(), isPosRecipt);
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }


        [HttpGet]
        [Authorize]
        public ActionResult PrintPOSInvoiceForExpenseById()
        {

            int POrderID = (int)TempData["POSSOrderID"];

            byte[] bytes = _transactionalReportService.PrintPOSInvoiceForExpenseById(POrderID, User.Identity.Name, User.Identity.GetConcernId());

            TempData["ReportData"] = bytes;
            return PartialView("~/Views/Shared/_ReportViewer.cshtml");
        }



        [HttpGet]
        [Authorize]
        public ActionResult PosMoneyReceiptCashDeliveryByID() 
        {
            bool isPosRecipt = true;
            var CashCollectionID = (int)TempData["CashCollectionID"];
            byte[] bytes = _transactionalReportService.CashCollectionMoneyReceiptDeliveryByID(CashCollectionID, User.Identity.Name, User.Identity.GetConcernId(), isPosRecipt);
            TempData["ReportData"] = bytes;
            return CustomPartialView();
        }

        [HttpPost]
        [Authorize]
        public PartialViewResult ZoneWiseCustomerDueRpt(FormCollection formCollection)
        {
            int reportType = 0, DueType = 0, CustomerId = 0, ZoneID = 0;

            if (!string.IsNullOrEmpty(formCollection["ReportType"]))
                reportType = Convert.ToInt32(formCollection["ReportType"].ToString());

            if (!string.IsNullOrEmpty(formCollection["CustomerId"]))
                CustomerId = Convert.ToInt32(formCollection["CustomerId"]);

            if (!string.IsNullOrEmpty(formCollection["DueType"]))
                DueType = Convert.ToInt32(formCollection["DueType"]);

            if (!string.IsNullOrEmpty(formCollection["ZonesID"]))
                ZoneID = Convert.ToInt32(formCollection["ZonesID"]);


            byte[] bytes = _basicReportService.ZoneWiseCustomerDueRpt(User.Identity.Name, User.Identity.GetConcernId(), CustomerId, reportType, DueType, ZoneID);

            TempData["ReportData"] = bytes;
            return CustomPartialView();

        }

    }
}