using AutoMapper;
using IMSWEB.Model;
using IMSWEB.Service;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Data;
using IMSWEB.Report;
using log4net;
using IMSWEB.Model.TOs;
using System.Web.UI.WebControls;
using IMSWEB.Model.TO;
using log4net.Util;

namespace IMSWEB.Controllers
{
    [Authorize]
    [RoutePrefix("salesorder")]
    public class SalesOrderController : CoreController
    {
        ISalesOrderService _salesOrderService;
        ISalesOrderDetailService _salesOrderDetailService;
        IStockService _stockService;
        IStockDetailService _stockDetailService;
        ICustomerService _customerService;
        IEmployeeService _employeeService;
        ITransactionalReport _transactionalReportService;
        IMiscellaneousService<SOrder> _miscellaneousService;
        IProductService _productService;
        IMapper _mapper;
        ISisterConcernService _SisterConcern;
        ISRVisitService _SRVisitService;
        IUserService _UserService;
        ISystemInformationService _SysInfoService;
        ISMSStatusService _SMSStatusService;
        private readonly IProductUnitTypeService _productUnitTypeService;
        private readonly ISizeService _sizeService;
        private readonly IVehicleService _vehicleService;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ISMSBillPaymentBkashService _smsBillPaymentBkashService;
        private readonly IPaymentOptionService _paymentOptionService;
        private readonly IPaymentDetailsForSaleService _paymentDetailService;
        IDOService _dOService;
        public SalesOrderController(IErrorService errorService,
            ISalesOrderService salesOrderService, ISalesOrderDetailService salesOrderDetailService,
            IStockService stockService, IStockDetailService stockDetailService,
            ICustomerService customerService, IEmployeeService employeeService,
            ITransactionalReport transactionalReportService,
            IMiscellaneousService<SOrder> miscellaneousService, IMapper mapper,
            ISRVisitService SRVisitService, IUserService UserService,
            IProductService productService, ISisterConcernService sisterConcern,
            ISystemInformationService SysInfoService, ISMSStatusService SMSStatusService, IProductUnitTypeService productUnitTypeService, ISizeService sizeService, ISMSBillPaymentBkashService smsBillPaymentBkashService, IPaymentOptionService paymentOptionService, IPaymentDetailsForSaleService paymentDetailService,
            IDOService dOService, IVehicleService vehicleService)
            : base(errorService)
        {
            _salesOrderService = salesOrderService;
            _salesOrderDetailService = salesOrderDetailService;
            _stockService = stockService;
            _stockDetailService = stockDetailService;
            _customerService = customerService;
            _employeeService = employeeService;
            _transactionalReportService = transactionalReportService;
            _miscellaneousService = miscellaneousService;
            _productService = productService;
            _mapper = mapper;
            _SisterConcern = sisterConcern;
            _SRVisitService = SRVisitService;
            _UserService = UserService;
            _SysInfoService = SysInfoService;
            _productUnitTypeService = productUnitTypeService;
            _sizeService = sizeService;
            _SMSStatusService = SMSStatusService;
            _smsBillPaymentBkashService = smsBillPaymentBkashService;
            _paymentOptionService = paymentOptionService;
            _paymentDetailService = paymentDetailService;
            _dOService = dOService;
            _vehicleService = vehicleService;
        }

        [HttpGet]
        [Authorize]
        [Route("index")]
        public async Task<ActionResult> Index()
        {
            int userId = System.Web.HttpContext.Current.User.Identity.GetUserId<int>();
            TempData["salesOrderViewModel"] = null;
            //if(userId==1014 || userId==1015 || userId==1016 || userId==1017|| userId==1018)
            var DateRange = GetFirstAndLastDateOfMonth(DateTime.Today);
            ViewBag.FromDate = DateRange.Item1;
            ViewBag.ToDate = DateRange.Item2;
            if (User.IsInRole(ConstantData.ROLE_MOBILE_USER))
            {
                var customSO = _salesOrderService.GetAllSalesOrderAsyncByUserID(userId, DateRange.Item1, DateRange.Item2, EnumSalesType.Sales);
                var vmSO = _mapper.Map<IEnumerable<Tuple<int, string, DateTime, string, string, decimal, EnumSalesType, Tuple<string>>>,
                IEnumerable<GetSalesOrderViewModel>>(await customSO);
                return View(vmSO);

            }
            else
            {
                List<EnumSalesType> status = new List<EnumSalesType>();
                status.Add(EnumSalesType.Sales);
                status.Add(EnumSalesType.Pending);
                var customSO = _salesOrderService.GetAllSalesOrderAsync(DateRange.Item1, DateRange.Item2, status, IsVATManager(), User.Identity.GetConcernId());
                var vmSO = _mapper.Map<IEnumerable<Tuple<int, string, DateTime, string, string, decimal, EnumSalesType, Tuple<string>>>,
                IEnumerable<GetSalesOrderViewModel>>(await customSO);
                return View(vmSO);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Index(FormCollection formCollection)
        {
            TempData["salesOrderViewModel"] = null;
            string InvoiceNo = string.Empty, ContactNo = "", CustomerName = "", AccountNo = "";
            DateTime fromDate = DateTime.MinValue;
            DateTime toDate = DateTime.MinValue;

            if (!string.IsNullOrEmpty(formCollection["FromDate"]))
                fromDate = Convert.ToDateTime(formCollection["FromDate"]);
            if (!string.IsNullOrEmpty(formCollection["ToDate"]))
                toDate = Convert.ToDateTime(formCollection["ToDate"]);

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            if (!string.IsNullOrEmpty(formCollection["InvoiceNo"]))
                InvoiceNo = formCollection["InvoiceNo"].Trim();
            if (!string.IsNullOrEmpty(formCollection["ContactNo"]))
                ContactNo = formCollection["ContactNo"].Trim();
            if (!string.IsNullOrEmpty(formCollection["CustomerName"]))
                CustomerName = formCollection["CustomerName"].Trim();

            if (!string.IsNullOrEmpty(formCollection["AccountNo"]))
                AccountNo = formCollection["AccountNo"].Trim();

            if (User.IsInRole(ConstantData.ROLE_MOBILE_USER))
            {
                int userId = System.Web.HttpContext.Current.User.Identity.GetUserId<int>();
                var customSO = _salesOrderService.GetAllSalesOrderAsyncByUserID(userId, ViewBag.FromDate, ViewBag.ToDate, EnumSalesType.Sales,
                    InvoiceNo, ContactNo, CustomerName, AccountNo);
                var vmSO = _mapper.Map<IEnumerable<Tuple<int, string, DateTime, string, string, decimal, EnumSalesType, Tuple<string>>>,
                IEnumerable<GetSalesOrderViewModel>>(await customSO);
                return View("Index", vmSO);
            }
            else

            {
                List<EnumSalesType> status = new List<EnumSalesType>();
                status.Add(EnumSalesType.Sales);
                status.Add(EnumSalesType.Pending);

                var customSO = _salesOrderService.GetAllSalesOrderAsync(fromDate, toDate,
                    status, IsVATManager(), User.Identity.GetConcernId(), InvoiceNo, ContactNo, CustomerName, AccountNo);
                var vmSO = _mapper.Map<IEnumerable<Tuple<int, string, DateTime, string, string, decimal, EnumSalesType, Tuple<string>>>,
                IEnumerable<GetSalesOrderViewModel>>(await customSO);
                return View("Index", vmSO);
            }
        }


        [HttpGet]
        [Authorize]
        [Route("create")]
        public ActionResult Create()
        {
            return ReturnCreateViewWithTempData();
        }

        [HttpPost]
        [Authorize]
        [Route("create/returnUrl")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SalesOrderViewModel newSalesOrder, FormCollection formCollection, string returnUrl)
        {
            return HandleSalesOrder(newSalesOrder, formCollection);
        }

        [HttpGet]
        [Authorize]
        [Route("edit/{orderId}")]
        public ActionResult Edit(int orderId, string previousAction)
        {
            ViewBag.Vehicle = GetAllVehicleDDL();
            ViewBag.ProductIds = GetAllProductsForDDL();

            ViewBag.CustomerIds = GetAllCustomerForDDL();
            int ConcernID = User.Identity.GetConcernId();
            var systemInfo = _SysInfoService.GetSystemInformationByConcernId(ConcernID);
            ViewBag.UnderRateSales = systemInfo.UnderPoRateSalesAllow;
            ViewBag.IsVulcanizing = systemInfo.IsVulcanizing;
            bool Result = _salesOrderService.IsSoReturn(orderId);
            //bool IsIsDoSales = _salesOrderService.IsDoSales(orderId);
            bool IsIsDoSales = false;
            var soDetail = _salesOrderDetailService.GetSOrderDetailsBySOrderID(orderId);
            foreach (var item in soDetail)
            {
                if (item.DOrderDetailID > 0)
                {
                    IsIsDoSales = true;
                }
            }
            if (Result == true)
            {
                AddToastMessage("", "Edit Not Possible Return. Found.", ToastType.Error);
                return RedirectToAction("Index");
            }
            if (IsIsDoSales == true)
            {
                AddToastMessage("", "Edit Not Possible It's DO Sales Product.", ToastType.Error);
                return RedirectToAction("Index");
            }

            if (TempData["salesOrderViewModel"] == null || string.IsNullOrEmpty(previousAction))
            {
                var salesOrder = _salesOrderService.GetSalesOrderById(orderId);
                var soDetails = _salesOrderDetailService.GetSalesOrderDetailByOrderId(orderId);



                var vmSalesOrder = _mapper.Map<SOrder, CreateSalesOrderViewModel>(salesOrder);

                var customer = _customerService.GetCustomerById(Convert.ToInt32(vmSalesOrder.CustomerId));
                vmSalesOrder.CurrentDue = customer.TotalDue.ToString();

                var vmSoDetails = _mapper.Map<IEnumerable<Tuple<int, int, int, int, string, string, string,
                    Tuple<decimal, decimal, decimal, decimal, decimal, decimal, int, Tuple<string, decimal, int, int, string, string, string, Tuple<int, string, int>>>>>, IEnumerable<CreateSalesOrderDetailViewModel>>(soDetails).ToList();
                log.Info(new { vmSalesOrder, vmSoDetails });
                var gDetails = (from d in vmSoDetails
                                group d by new
                                {
                                    d.ProductId,
                                    d.ProductCode,
                                    d.ProductName,
                                    d.ColorName,
                                    d.ColorId,
                                    d.IMENo,
                                    d.GodownID,
                                    d.CategoryName,
                                    d.SizeName,
                                    d.StockDetailId,
                                    d.VehicleID,
                                    d.VehicleNo,
                                    d.OrderIndex
                                } into g
                                select new CreateSalesOrderDetailViewModel
                                {
                                    ProductId = g.Key.ProductId,
                                    ProductCode = g.Key.ProductCode,
                                    ProductName = g.Key.ProductName,
                                    ColorId = g.Key.ColorId,
                                    ColorName = g.Key.ColorName,
                                    GodownID = g.Key.GodownID,
                                    CategoryName = g.Key.CategoryName,
                                    VehicleID = g.Key.VehicleID,
                                    VehicleNo = g.Key.VehicleNo,
                                    SizeName = g.Key.SizeName,
                                    ConvertValue = g.Select(i => i.ConvertValue).FirstOrDefault(),
                                    IMENo = g.Key.IMENo,
                                    UnitPrice = g.Select(i => i.UnitPrice).FirstOrDefault(),
                                    MRPRate = g.Select(i => i.MRPRate).FirstOrDefault(),
                                    SODetailId = g.Select(i => i.SODetailId).FirstOrDefault(),
                                    SalesOrderId = g.Select(i => i.SalesOrderId).FirstOrDefault(),
                                    StockDetailId = g.Key.StockDetailId,
                                    //PPDAmount = g.Select(i => i.PPDAmount).FirstOrDefault(),
                                    //PPDPercentage = g.Select(i => i.PPDPercentage).FirstOrDefault(),                             
                                    OrderIndex = g.Key.OrderIndex,
                                    PPOffer = g.Select(i => i.PPOffer).FirstOrDefault(),
                                    Quantity = g.Sum(i => decimal.Parse(i.Quantity)).ToString(),
                                    UTAmount = g.Sum(i => decimal.Parse(i.UTAmount)).ToString(),
                                    PPDAmount = g.Sum(i => decimal.Parse(i.PPDAmount)).ToString(),
                                    PPDPercentage = g.Sum(i => decimal.Parse(i.PPDPercentage)).ToString(),
                                    TempPPDiscountAmount = g.Sum(i => decimal.Parse(i.PPDAmount)).ToString()
                                }).OrderBy(d => d.OrderIndex).ToList();
                //var vm = new SalesOrderViewModel
                //{
                //    SODetail = new CreateSalesOrderDetailViewModel(),
                //    SODetails = gDetails,        
                //    SalesOrder = vmSalesOrder                    

                //};

                IEnumerable<PaymentOptionDetailsTO> paymentOptionDetails = _paymentDetailService.GetMultiPaymentDetailsById(orderId);
                SalesOrderViewModel salesOrderPaymentDetails = new SalesOrderViewModel();
                salesOrderPaymentDetails.PaymentOptionDetails = paymentOptionDetails.ToList();
                int idCounter = 1;
                foreach (var paymentOptionDetail in paymentOptionDetails)
                {
                    paymentOptionDetail.Id = idCounter;
                    idCounter++;
                }
                salesOrderPaymentDetails.PaymentOptionDetails = paymentOptionDetails.ToList();
                if (salesOrderPaymentDetails.PaymentOptionDetails.Count == 0)
                {
                    paymentOptionDetails = new List<PaymentOptionDetailsTO> { new PaymentOptionDetailsTO { Id = 1 } };
                    ViewBag.PaymentOptionIds = _paymentOptionService.GetAllForDDL();
                    salesOrderPaymentDetails.PaymentOptionDetails = paymentOptionDetails.ToList();
                }
                ViewBag.PaymentOptionIds = _paymentOptionService.GetAllForDDL();

                var vm = new SalesOrderViewModel
                {
                    SODetail = new CreateSalesOrderDetailViewModel(),
                    SODetails = gDetails,
                    SalesOrder = vmSalesOrder,
                    PaymentOptionDetails = salesOrderPaymentDetails.PaymentOptionDetails
                 .Select(p => new PaymentOptionDetailsTO
                 {
                     Id = p.Id,
                     Name = p.Name,
                     PaymentOptionId = p.PaymentOptionId,
                     PaidAmount = p.PaidAmount,
                     Charge = p.Charge,
                     bankID = p.bankID,
                     ChecqueNo = p.ChecqueNo
                 })
                 .ToList()
                };

                TempData["salesOrderViewModel"] = vm;
                return View("Create", vm);
            }
            else
            {
                return ReturnCreateViewWithTempData();
            }

            //}


        }

        [HttpPost]
        [Authorize]
        [Route("edit/returnUrl")]
        public ActionResult Edit(SalesOrderViewModel newSalesOrder, FormCollection formCollection, string returnUrl)
        {
            return HandleSalesOrder(newSalesOrder, formCollection);
        }

        [HttpGet]
        [Authorize]
        [Route("delete/{orderId}")]
        public ActionResult Delete(int orderId)
        {
            var Sodetailss = _salesOrderDetailService.GetSalesOrderDetailsById(orderId);
            var Sales = _salesOrderService.GetSalesOrderById(orderId);

            if (!IsDateValid(Sales.InvoiceDate))
            {
                return RedirectToAction("Index");
            }

            if (Sodetailss.RQuantity > 0)
            {
                AddToastMessage("", "Deleted Failed Some Qty has been Return, Have any confusion contact with support team", ToastType.Error);
                return RedirectToAction("Index");
            }
            else
            {
                _salesOrderService.DeleteSalesOrderUsingSP(orderId, User.Identity.GetUserId<int>());
                log.Info(new { SOrderID = orderId });
                AddToastMessage("", "Item has been deleted successfully", ToastType.Success);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        [Route("editfromview/{id}/{detailId}")]
        public ActionResult EditFromView(int id, int detailId, string previousAction)
        {
            SalesOrderViewModel salesOrder = (SalesOrderViewModel)TempData.Peek("salesOrderViewModel");
            if (salesOrder == null)
            {
                AddToastMessage("", "Item has been expired to edit", ToastType.Error);
                if (IsForEdit(previousAction))
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Create");
            }

            CreateSalesOrderDetailViewModel itemToEdit =
                salesOrder.SODetails.Where(x => int.Parse(x.ProductId) == id &&
                             int.Parse(x.StockDetailId) == detailId).FirstOrDefault();
            if (itemToEdit != null)
            {
                itemToEdit.ParentQuantity = decimal.Parse(itemToEdit.Quantity) / itemToEdit.ConvertValue;
                itemToEdit.RatePerArea = decimal.Parse(GetDefaultIfNull(itemToEdit.UnitPrice)) * itemToEdit.ConvertValue;

                decimal gTotal = (decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.GrandTotal)) - (decimal.Parse(GetDefaultIfNull(itemToEdit.UTAmount)) + (decimal.Parse(GetDefaultIfNull(itemToEdit.PPDAmount)) * itemToEdit.ParentQuantity)));

                salesOrder.SalesOrder.GrandTotal = (decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.GrandTotal)) -
                    (decimal.Parse(GetDefaultIfNull(itemToEdit.UTAmount)) + decimal.Parse(GetDefaultIfNull(itemToEdit.PPDAmount)) * itemToEdit.ParentQuantity)).ToString();

                salesOrder.SalesOrder.PPDiscountAmount = (decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.PPDiscountAmount)) -
                    (decimal.Parse(GetDefaultIfNull(itemToEdit.PPDAmount)) * itemToEdit.ParentQuantity)).ToString();

                salesOrder.SalesOrder.NetDiscount = (decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.NetDiscount)) -
                    (decimal.Parse(GetDefaultIfNull(itemToEdit.PPDAmount)) * itemToEdit.ParentQuantity) - decimal.Parse(GetDefaultIfNull(itemToEdit.PPOffer))).ToString();

                salesOrder.SalesOrder.TotalAmount = (decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.TotalAmount)) -
                    (decimal.Parse(GetDefaultIfNull(itemToEdit.UTAmount)))).ToString();

                salesOrder.SalesOrder.PaymentDue = ((decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.PaymentDue)) + decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.RecieveAmount))) -
                    (decimal.Parse(GetDefaultIfNull(itemToEdit.UTAmount)))).ToString();

                //For Total Offer Calculation
                salesOrder.SalesOrder.TotalOffer = (decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.TotalOffer)) -
                decimal.Parse(GetDefaultIfNull(itemToEdit.PPOffer))).ToString();

                if (IsForEdit(previousAction) && !string.IsNullOrEmpty(itemToEdit.SODetailId))
                {
                    itemToEdit.Status = EnumStatus.Deleted;
                    //int sorderDetailId = int.Parse(itemToEdit.SODetailId);
                    //int userId = User.Identity.GetUserId<int>();
                    //_salesOrderService.DeleteSalesOrderDetailUsingSP(sorderDetailId, userId);
                }
                else
                {
                    salesOrder.SODetails.Remove(itemToEdit);
                }

                salesOrder.SODetail = itemToEdit;
                TempData["salesOrderViewModel"] = salesOrder;

                if (IsForEdit(previousAction))
                    return RedirectToAction("Edit", new { orderId = default(int), previousAction = "Edit" });
                else
                    return RedirectToAction("Create");
            }
            else
            {
                AddToastMessage("", "No item found to edit", ToastType.Info);
                if (IsForEdit(previousAction))
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Create");
            }
        }

        [HttpGet]
        [Authorize]
        [Route("deletefromview/{id}/{detailId}")]
        public ActionResult DeleteFromView(int id, int detailId, string previousAction)
        {
            SalesOrderViewModel salesOrder = (SalesOrderViewModel)TempData.Peek("salesOrderViewModel");
            if (salesOrder == null)
            {
                AddToastMessage("", "Item has been expired to delete", ToastType.Error);
                if (IsForEdit(previousAction))
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Create");
            }

            CreateSalesOrderDetailViewModel itemToDelete =
                salesOrder.SODetails.Where(x => int.Parse(x.ProductId) == id &&
                             int.Parse(x.StockDetailId.Trim(',')) == detailId).FirstOrDefault();
            if (itemToDelete != null)
            {
                salesOrder.SalesOrder.GrandTotal = (decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.GrandTotal)) -
                    (decimal.Parse(GetDefaultIfNull(itemToDelete.UTAmount)) + decimal.Parse(GetDefaultIfNull(itemToDelete.PPDAmount)) + itemToDelete.FractionAmt)).ToString();

                salesOrder.SalesOrder.PPDiscountAmount = (decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.PPDiscountAmount)) -
                    decimal.Parse(GetDefaultIfNull(itemToDelete.PPDAmount))).ToString();

                salesOrder.SalesOrder.NetDiscount = (decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.NetDiscount)) -
                    decimal.Parse(GetDefaultIfNull(itemToDelete.PPDAmount))).ToString();

                salesOrder.SalesOrder.TotalAmount = (decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.TotalAmount)) -
                    (decimal.Parse(GetDefaultIfNull(itemToDelete.UTAmount)) + itemToDelete.FractionAmt)).ToString();

                //salesOrder.SalesOrder.PaymentDue = ((decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.PaymentDue)) + decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.RecieveAmount))) -
                //    (decimal.Parse(GetDefaultIfNull(itemToDelete.UTAmount)))).ToString();

                salesOrder.SalesOrder.PaymentDue = ((decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.PaymentDue))) -
          (decimal.Parse(GetDefaultIfNull(itemToDelete.UTAmount)) + itemToDelete.FractionAmt)).ToString();

                salesOrder.SalesOrder.TotalFractionAmt = salesOrder.SalesOrder.TotalFractionAmt - itemToDelete.FractionAmt;

                //For Offer Purpose
                salesOrder.SalesOrder.TotalOffer = (decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.TotalOffer)) -
               (decimal.Parse(GetDefaultIfNull(itemToDelete.PPOffer)))).ToString();

                if (IsForEdit(previousAction) && !string.IsNullOrEmpty(itemToDelete.SODetailId))
                {
                    itemToDelete.Status = EnumStatus.Deleted;
                    //int sorderDetailId = int.Parse(itemToDelete.SODetailId);
                    //int userId = User.Identity.GetUserId<int>();
                    //_salesOrderService.DeleteSalesOrderDetailUsingSP(sorderDetailId, userId);
                }
                else
                {
                    salesOrder.SODetails.Remove(itemToDelete);
                }

                salesOrder.SODetail = new CreateSalesOrderDetailViewModel();
                TempData["salesOrderViewModel"] = salesOrder;
                AddToastMessage("", "Item has been removed successfully", ToastType.Success);

                if (IsForEdit(previousAction))
                    return RedirectToAction("Edit", new { orderId = default(int), previousAction = "Edit" });
                else
                    return RedirectToAction("Create");
            }
            else
            {
                AddToastMessage("", "No item found to remove", ToastType.Info);
                if (IsForEdit(previousAction))
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Create");
            }
        }

        private void CheckAndAddModelErrorForAdd(SalesOrderViewModel newSalesOrder,
            SalesOrderViewModel salesOrder, FormCollection formCollection)
        {
            if (string.IsNullOrEmpty(formCollection["OrderDate"]))
                ModelState.AddModelError("SalesOrder.OrderDate", "Sales Date is required");

            if (string.IsNullOrEmpty(formCollection["CustomerId"]))
                ModelState.AddModelError("SalesOrder.CustomerId", "Customer is required");
            else
            {
                newSalesOrder.SalesOrder.CustomerId = formCollection["CustomerId"];
            }


            if (string.IsNullOrEmpty(formCollection["SODetail.ProductId"]))
                ModelState.AddModelError("SODetail.ProductId", "Product is required");
            else
            {
                newSalesOrder.SODetail.ProductId = formCollection["SODetail.ProductId"];
                salesOrder.SODetail.ProductId = formCollection["SODetail.ProductId"];
                if (newSalesOrder.SODetail.DOID > 0)
                {
                    int ProductID = Convert.ToInt32(salesOrder.SODetail.ProductId);
                    int ColorID = string.IsNullOrEmpty(formCollection["ColorsId"]) ? 0 : Convert.ToInt32(formCollection["ColorsId"]);

                    var Detail = _dOService.GetDetailsById(newSalesOrder.SODetail.DOID)
                                    .FirstOrDefault(i => i.ProductID == ProductID);
                    decimal qty = Convert.ToDecimal(newSalesOrder.SODetail.Quantity);
                    if (Detail == null)
                        ModelState.AddModelError("SODetail.Quantity", "This product is not found in this DO");
                    else if ((Detail.DOQty - Detail.GivenQty) < qty)
                        ModelState.AddModelError("SODetail.Quantity", "DO qty(" + (Detail.DOQty - Detail.GivenQty) + ") is not available");
                }
            }


            if (string.IsNullOrEmpty(newSalesOrder.SODetail.Quantity) || Convert.ToInt32(double.Parse(newSalesOrder.SODetail.Quantity)) <= 0)
            {
                ModelState.AddModelError("SODetail.Quantity", "Quantity is required");
            }

            if (string.IsNullOrEmpty(newSalesOrder.SalesOrder.InvoiceNo))
                ModelState.AddModelError("SalesOrder.InvoiceNo", "Invoice No. is required");

            if (string.IsNullOrEmpty(newSalesOrder.SODetail.MRPRate))
                ModelState.AddModelError("SODetail.MRPRate", "Purchase Rate is required");

            if (string.IsNullOrEmpty(newSalesOrder.SODetail.UnitPrice))
                ModelState.AddModelError("SODetail.UnitPrice", "Sales Rate is required");

            if (string.IsNullOrEmpty(newSalesOrder.SODetail.IMENo))
            {
                ModelState.AddModelError("SODetail.IMENo", "IMENo/Barcode is required");
            }
            else
            {
                var product = _productService.GetProductById(int.Parse(GetDefaultIfNull(formCollection["SODetail.ProductId"])));
                //int SDetailID = int.Parse(GetDefaultIfNull(formCollection["SODetail.StockDetailId"]));

                int SDetailID = int.Parse(newSalesOrder.SODetail.StockDetailId);
                var stockDeatilCount = _stockDetailService.GetById(SDetailID);
                newSalesOrder.SODetail.GodownID = stockDeatilCount.GodownID;
                if (product.ProductType == (int)EnumProductType.NoBarcode)
                {
                    CreateSalesOrderDetailViewModel PreQty = null;
                    decimal StockQty = 0m;
                    if (salesOrder.SODetails != null)
                    {
                        PreQty = salesOrder.SODetails
                                     .FirstOrDefault(i => i.ProductId.Equals(newSalesOrder.SODetail.ProductId)
                                        && i.ColorId.Equals(formCollection["SODetail.ColorId"])
                        && i.GodownID == newSalesOrder.SODetail.GodownID);
                    }

                    var StockCount = _stockService.GetStockById(stockDeatilCount.StockID);
                    StockQty = StockCount.Quantity;

                    decimal remainingQty = _stockDetailService.GetRemainingQuantityForSRVisit(stockDeatilCount.ProductID, stockDeatilCount.ColorID, User.Identity.GetConcernId(), stockDeatilCount.GodownID, 0, stockDeatilCount.SDetailID);

                    if (remainingQty < Convert.ToDecimal(newSalesOrder.SODetail.Quantity))
                    {
                        string msg = $"Those Qty not found in stock for this product. Available Qty: {remainingQty} for selected product";
                        //return Json(new { status = false, msg = msg }, JsonRequestBehavior.AllowGet);
                        ModelState.AddModelError("SODetail.Quantity", msg);
                    }

                    //if (StockQty < decimal.Parse(newSalesOrder.SODetail.Quantity))
                    //    ModelState.AddModelError("SODetail.Quantity", "Stock is not available. Stock Quantity: " + StockQty);

                    //if (PreQty != null)
                    //{
                    //    StockQty = StockCount.Quantity;

                    //    if (StockQty < decimal.Parse(newSalesOrder.SODetail.Quantity))
                    //        ModelState.AddModelError("SODetail.Quantity", "Stock is not available. Stock Quantity: " + StockQty);
                    //}
                    //if (PreQty != null)
                    //    StockQty += Convert.ToDecimal(PreQty.Quantity);

                    //if (StockQty < decimal.Parse(newSalesOrder.SODetail.Quantity))
                    //    ModelState.AddModelError("SODetail.ParentQuantity", "Stock is not available. Stock Quantity: " + StockQty);
                }

            }
        }

        private void CheckAndAddModelErrorForSave(SalesOrderViewModel newSalesOrder, SalesOrderViewModel salesOrder, FormCollection formCollection)
        {
            if (string.IsNullOrEmpty(newSalesOrder.SalesOrder.GrandTotal) ||
                decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.GrandTotal)) <= 0)
                ModelState.AddModelError("SalesOrder.GrandTotal", "Grand Total is required");

            //if (string.IsNullOrEmpty(newSalesOrder.SalesOrder.TotalAmount) ||
            //    decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.TotalAmount)) <= 0)
            //    ModelState.AddModelError("SalesOrder.TotalAmount", "Net Total is required");


            //if (string.IsNullOrEmpty(newSalesOrder.SalesOrder.RecieveAmount) ||
            //    decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.RecieveAmount)) <= 0)
            //    ModelState.AddModelError("SalesOrder.RecieveAmount", "Pay Amount is required");
            if (newSalesOrder.SalesOrder.RecieveAmount == null || newSalesOrder.SalesOrder.RecieveAmount == "")
            {
                newSalesOrder.SalesOrder.RecieveAmount = "0";
                salesOrder.SalesOrder.RecieveAmount = "0";
            }

            #region Customer and Employee Due Limit Check
            Customer customer = _customerService.GetCustomerById(int.Parse(salesOrder.SalesOrder.CustomerId));
            Employee employee = _employeeService.GetEmployeeById(customer.EmployeeID);
            var sysInfo = _SysInfoService.GetSystemInformationByConcernId(User.Identity.GetConcernId());
            if (sysInfo != null)
            {
                if (sysInfo.CustomerDueLimitApply == 1)
                {
                    if (decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.PaymentDue)) > customer.CusDueLimit)
                        ModelState.AddModelError("SalesOrder.PaymentDue", "Customer due limit is exceeding");
                }
                if (sysInfo.EmployeeDueLimitApply == 1)
                {
                    if (decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.PaymentDue)) > employee.SRDueLimit)
                        ModelState.AddModelError("SalesOrder.PaymentDue", "SR due limit is exceeding");
                }
            }

            #endregion

            var distinctIMEI = salesOrder.SODetails
                                .GroupBy(i => i.StockDetailId)
                                .Select(g => g.First())
                                .ToList();

            //if (distinctIMEI.Count() != salesOrder.SODetails.Count())
            //{
            //    ModelState.AddModelError("SODetail.IMENo", "");
            //    AddToastMessage("", "Duplicate IMEI added.", ToastType.Error);
            //}
            salesOrder.SalesOrder.OrderDate = formCollection["OrderDate"];
            if (!IsDateValid(Convert.ToDateTime(salesOrder.SalesOrder.OrderDate)))
            {
                ModelState.AddModelError("SalesOrder.OrderDate", "Back dated entry is not valid.");
            }
        }

        private void AddToOrder(SalesOrderViewModel newSalesOrder,
            SalesOrderViewModel salesOrder, FormCollection formCollection)
        {

            int lastIndex = 0;
            if (salesOrder.SODetails != null && salesOrder.SODetails.Any())
            {
                lastIndex = salesOrder.SODetails
                .Select(x =>
                {
                    int indexVal;
                    return int.TryParse(x.OrderIndex, out indexVal) ? indexVal : 0;
                })
                .DefaultIfEmpty(0)
                .Max();
            }

            int nextIndex = lastIndex + 1;
            //DateTime RemindDate = DateTime.MinValue;
            decimal quantity = decimal.Parse(GetDefaultIfNull(newSalesOrder.SODetail.Quantity));
            decimal totalDisAmount = decimal.Parse(GetDefaultIfNull(newSalesOrder.SODetail.PPDAmount)) * newSalesOrder.SODetail.ParentQuantity;
            decimal ppDiscountAmount = decimal.Parse(GetDefaultIfNull(newSalesOrder.SODetail.PPDAmount));
            decimal totalOffer = quantity * decimal.Parse(GetDefaultIfNull(newSalesOrder.SODetail.PPOffer));

            salesOrder.SalesOrder.GrandTotal = (decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.GrandTotal)) +
                decimal.Parse(GetDefaultIfNull(newSalesOrder.SODetail.UTAmount)) + totalDisAmount + totalOffer
                + newSalesOrder.SODetail.FractionAmt
                ).ToString();

            //salesOrder.SalesOrder.PPDiscountAmount = (decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.PPDiscountAmount)) + ppDiscountAmount).ToString();

            salesOrder.SalesOrder.PPDiscountAmount = (decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.PPDiscountAmount)) + totalDisAmount).ToString();

            salesOrder.SalesOrder.TotalDiscountPercentage = decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.TotalDiscountPercentage)).ToString();
            salesOrder.SalesOrder.TotalDiscountAmount = decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.TotalDiscountAmount)).ToString();
            salesOrder.SalesOrder.TempFlatDiscountAmount = salesOrder.SalesOrder.TotalDiscountAmount;

            salesOrder.SalesOrder.VATPercentage = decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.VATPercentage)).ToString();
            salesOrder.SalesOrder.VATAmount = decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.VATAmount)).ToString();
            salesOrder.SalesOrder.LabourCost = decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.LabourCost)).ToString();

            salesOrder.SalesOrder.AdjAmount = decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.AdjAmount)).ToString();

            //salesOrder.SalesOrder.NetDiscount = (decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.NetDiscount)) + decimal.Parse(GetDefaultIfNull(newSalesOrder.SODetail.PPDAmount)) +
            //    decimal.Parse(GetDefaultIfNull(newSalesOrder.SODetail.PPOffer))).ToString();
            // decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.TotalDiscountAmount)) +

            salesOrder.SalesOrder.NetDiscount = (decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.NetDiscount)) + totalDisAmount + totalOffer).ToString();

            var netTotal = ((decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.GrandTotal)) + decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.VATAmount))
                + decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.LabourCost))) -
                (decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.NetDiscount)) + decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.AdjAmount))
                ));

            salesOrder.SalesOrder.TotalFractionAmt = salesOrder.SalesOrder.TotalFractionAmt + newSalesOrder.SODetail.FractionAmt;

            // For Total Offer Purpose



            salesOrder.SalesOrder.TotalOffer = (decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.TotalOffer)) + totalOffer).ToString();

            salesOrder.SalesOrder.TotalAmount = netTotal.ToString();
            salesOrder.SalesOrder.PaymentDue = (netTotal - decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.RecieveAmount))).ToString();
            salesOrder.SalesOrder.RecieveAmount = GetDefaultIfNull(newSalesOrder.SalesOrder.RecieveAmount);

            salesOrder.SalesOrder.OrderDate = formCollection["OrderDate"];
            //if (Convert.ToDateTime(salesOrder.SalesOrder.RemindDate) > Convert.ToDateTime(salesOrder.SalesOrder.OrderDate))
            //{
            //    salesOrder.SalesOrder.RemindDate = formCollection["RemindDate"];

            //}
            //else
            //{
            //    salesOrder.SalesOrder.RemindDate = null;

            //}
            salesOrder.SalesOrder.RemindDate = formCollection["RemindDate"];
            salesOrder.SalesOrder.CustomerId = formCollection["CustomerId"];

            salesOrder.SODetail.OrderIndex = nextIndex.ToString();
            salesOrder.SODetail.SODetailId = newSalesOrder.SODetail.SODetailId;
            salesOrder.SODetail.ProductId = formCollection["SODetail.ProductId"];
            salesOrder.SODetail.ColorId = formCollection["SODetail.ColorId"];
            salesOrder.SODetail.GodownID = newSalesOrder.SODetail.GodownID;
            salesOrder.SODetail.ColorName = formCollection["SODetail.ColorName"];
            //salesOrder.SODetail.StockDetailId = formCollection["SODetail.StockDetailId"];
            var stksid = formCollection["SODetail.StockDetailId"];
            salesOrder.SODetail.StockDetailId = stksid.Split(',').Distinct().First();
            salesOrder.SODetail.ProductCode = formCollection["SODetail.ProductCode"];
            salesOrder.SODetail.IMENo = newSalesOrder.SODetail.IMENo;
            salesOrder.SODetail.Quantity = newSalesOrder.SODetail.Quantity;
            salesOrder.SODetail.PPDPercentage = newSalesOrder.SODetail.PPDPercentage;
            salesOrder.SODetail.ConvertValue = newSalesOrder.SODetail.ConvertValue;

            salesOrder.SODetail.RatePerArea = newSalesOrder.SODetail.RatePerArea;
            salesOrder.SODetail.TotalArea = newSalesOrder.SODetail.TotalArea;
            salesOrder.SODetail.FractionQty = newSalesOrder.SODetail.FractionQty;
            salesOrder.SODetail.FractionAmt = newSalesOrder.SODetail.FractionAmt;

            //salesOrder.SODetail.PPDAmount = newSalesOrder.SODetail.PPDAmount;
            salesOrder.SODetail.PPDAmount = ppDiscountAmount.ToString();
            salesOrder.SODetail.UnitPrice = newSalesOrder.SODetail.UnitPrice;
            salesOrder.SODetail.MRPRate = newSalesOrder.SODetail.MRPRate;
            salesOrder.SODetail.UTAmount = newSalesOrder.SODetail.UTAmount;
            salesOrder.SODetail.ProductName = formCollection["SODetail.ProductName"];
            salesOrder.SODetail.Status = newSalesOrder.SODetail.Status == default(int) ? EnumStatus.New : newSalesOrder.SODetail.Status;
            //salesOrder.SODetail.PPOffer = newSalesOrder.SODetail.PPOffer;
            salesOrder.SODetail.PPOffer = totalOffer.ToString();
            salesOrder.SODetail.CompressorWarrentyMonth = newSalesOrder.SODetail.CompressorWarrentyMonth;
            salesOrder.SODetail.MotorWarrentyMonth = newSalesOrder.SODetail.MotorWarrentyMonth;
            salesOrder.SODetail.PanelWarrentyMonth = newSalesOrder.SODetail.PanelWarrentyMonth;
            salesOrder.SODetail.SparePartsWarrentyMonth = newSalesOrder.SODetail.SparePartsWarrentyMonth;
            salesOrder.SODetail.ServiceWarrentyMonth = newSalesOrder.SODetail.ServiceWarrentyMonth;
            salesOrder.SODetail.CategoryName = newSalesOrder.SODetail.CategoryName;
            salesOrder.SODetail.SizeName = newSalesOrder.SODetail.SizeName;
            salesOrder.SODetail.UnitType = newSalesOrder.SODetail.UnitType;
            salesOrder.SODetail.DONo = newSalesOrder.SODetail.DONo;
            salesOrder.SODetail.DOID = newSalesOrder.SODetail.DOID;
            salesOrder.SODetail.VehicleID = string.IsNullOrEmpty(newSalesOrder.SODetail.VehicleID) ? "0" : newSalesOrder.SODetail.VehicleID;
            salesOrder.SODetail.VehicleNo = newSalesOrder.SODetail.VehicleNo;


            salesOrder.SODetails = salesOrder.SODetails ?? new List<CreateSalesOrderDetailViewModel>();
            salesOrder.SODetails.Add(salesOrder.SODetail);

            SalesOrderViewModel vm = new SalesOrderViewModel
            {
                SODetail = new CreateSalesOrderDetailViewModel(),
                SODetails = salesOrder.SODetails,
                SalesOrder = salesOrder.SalesOrder
            };

            TempData["salesOrderViewModel"] = vm;
            salesOrder.SODetail = new CreateSalesOrderDetailViewModel();
            AddToastMessage("", "Order has been added successfully.", ToastType.Success);
        }

        private bool SaveOrder(SalesOrderViewModel newSalesOrder,
            SalesOrderViewModel salesOrder, FormCollection formCollection)
        {
            bool Result = false;
            DateTime RemindDate = DateTime.MinValue;
            var Customer = _customerService.GetCustomerById(Convert.ToInt32(salesOrder.SalesOrder.CustomerId));
            salesOrder.SalesOrder.PrevDue = Customer.TotalDue;

            salesOrder.SalesOrder.NetDiscount = GetDefaultIfNull(newSalesOrder.SalesOrder.NetDiscount);
            salesOrder.SalesOrder.TotalAmount = decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.TotalAmount)).ToString();
            salesOrder.SalesOrder.PaymentDue = decimal.Parse(GetDefaultIfNull(newSalesOrder.SalesOrder.PaymentDue)).ToString();

            salesOrder.SalesOrder.TotalDiscountPercentage = newSalesOrder.SalesOrder.TotalDiscountPercentage;
            salesOrder.SalesOrder.TotalDiscountAmount = newSalesOrder.SalesOrder.TotalDiscountAmount;
            salesOrder.SalesOrder.RecieveAmount = newSalesOrder.SalesOrder.RecieveAmount;
            salesOrder.SalesOrder.VATPercentage = newSalesOrder.SalesOrder.VATPercentage;
            salesOrder.SalesOrder.VATAmount = newSalesOrder.SalesOrder.VATAmount;
            salesOrder.SalesOrder.AdjAmount = newSalesOrder.SalesOrder.AdjAmount;
            salesOrder.SalesOrder.Remarks = newSalesOrder.SalesOrder.Remarks;
            salesOrder.SalesOrder.OrderDate = formCollection["OrderDate"];
            string rdate = null;
            rdate = formCollection["RemindDate"];

            if (Convert.ToDateTime(rdate) > Convert.ToDateTime(salesOrder.SalesOrder.OrderDate))
            {
                salesOrder.SalesOrder.RemindDate = formCollection["RemindDate"];

            }
            else
            {
                salesOrder.SalesOrder.RemindDate = null;

            }
            salesOrder.SalesOrder.CustomerId = formCollection["CustomerId"];
            salesOrder.SalesOrder.TotalFractionAmt = newSalesOrder.SalesOrder.TotalFractionAmt;
            salesOrder.SalesOrder.IsSmsEnable = Convert.ToBoolean(newSalesOrder.SalesOrder.IsSmsEnable ? 1 : 0);
            salesOrder.SalesOrder.LabourCost = newSalesOrder.SalesOrder.LabourCost;

            //removing unchanged previous order
            salesOrder.SODetails.Where(x => !string.IsNullOrEmpty(x.SODetailId) && x.Status == default(int)).ToList()
                .ForEach(x => salesOrder.SODetails.Remove(x));

            if (!ControllerContext.RouteData.Values["action"].ToString().ToLower().Equals("edit"))
            {
                string invNo = _miscellaneousService.GetUniqueKey(x => int.Parse(x.InvoiceNo));
                salesOrder.SalesOrder.InvoiceNo = invNo;
            }
            if (Convert.ToDateTime(salesOrder.SalesOrder.RemindDate) > Convert.ToDateTime(salesOrder.SalesOrder.OrderDate))
                RemindDate = Convert.ToDateTime(salesOrder.SalesOrder.RemindDate);
            DataTable dtSalesOrder = CreateSalesOrderDataTable(salesOrder);
            DataTable dtSalesOrderDetail = CreateSODetailDataTable(salesOrder);
            DataTable dtPaymentDetail = CreatePaymentDetailsDataTable(newSalesOrder);
            var SystemInfo = _SysInfoService.GetSystemInformationByConcernId(User.Identity.GetConcernId());
            #region Log
            log.Info(new { SalesOrder = salesOrder.SalesOrder, SODetails = salesOrder.SODetails });
            #endregion
            if (ControllerContext.RouteData.Values["action"].ToString().ToLower().Equals("edit"))
            {
                Result = _salesOrderService.UpdateSalesOrderUsingSP(User.Identity.GetUserId<int>(), int.Parse(salesOrder.SalesOrder.SalesOrderId),
                 dtSalesOrder, dtSalesOrderDetail, dtPaymentDetail);
                if (Result)
                {
                    var invoiceSalesOrder = _mapper.Map<CreateSalesOrderViewModel, SOrder>(salesOrder.SalesOrder);
                    invoiceSalesOrder.SOrderDetails = _mapper.Map<ICollection<CreateSalesOrderDetailViewModel>,
                        ICollection<SOrderDetail>>(salesOrder.SODetails);

                    TempData["IsInvoiceReadyById"] = true;
                    TempData["OrderId"] = int.Parse(salesOrder.SalesOrder.SalesOrderId);

                }
            }

            else
            {



                int ordrId = 0;
                ordrId = _salesOrderService.AddSalesOrderUsingSP(dtSalesOrder, dtSalesOrderDetail, RemindDate, dtPaymentDetail);
                Result = ordrId > 0;

                if (Result)
                {
                    var invoiceSalesOrder = _mapper.Map<CreateSalesOrderViewModel, SOrder>(salesOrder.SalesOrder);
                    invoiceSalesOrder.SOrderDetails = _mapper.Map<ICollection<CreateSalesOrderDetailViewModel>,
                        ICollection<SOrderDetail>>(salesOrder.SODetails);
                    invoiceSalesOrder.SOrderID = ordrId;
                    TempData["salesInvoiceData"] = invoiceSalesOrder;

                    #region Sales SMS Service           

                    if (SystemInfo.IsRetailSMSEnable == 1 && salesOrder.SalesOrder.IsSmsEnable == true)
                    {

                        var _oCustomer = _customerService.GetCustomerById(invoiceSalesOrder.CustomerID);
                        List<SMSRequest> sms = new List<SMSRequest>();
                        sms.Add(new SMSRequest()
                        {
                            MobileNo = _oCustomer.ContactNo,
                            CustomerID = _oCustomer.CustomerID,
                            CustomerName = _oCustomer.Name,
                            TransNumber = invoiceSalesOrder.InvoiceNo,
                            Date = (DateTime)invoiceSalesOrder.InvoiceDate,
                            PreviousDue = _oCustomer.TotalDue,
                            ReceiveAmount = (decimal)invoiceSalesOrder.RecAmount,
                            PresentDue = _oCustomer.TotalDue + invoiceSalesOrder.PaymentDue,
                            SMSType = EnumSMSType.SalesTime,
                            SalesAmount = invoiceSalesOrder.TotalAmount,
                            CustomerCode = _oCustomer.Code,
                            //ProductNameList = ProductNameList.Select(i=>i.ProductName).ToList()
                        });

                        if (SystemInfo.SMSSendToOwner == 1)
                        {
                            sms.Add(new SMSRequest()
                            {
                                MobileNo = SystemInfo.InsuranceContactNo,
                                CustomerID = _oCustomer.CustomerID,
                                CustomerName = _oCustomer.Name,
                                TransNumber = invoiceSalesOrder.InvoiceNo,
                                Date = (DateTime)invoiceSalesOrder.InvoiceDate,
                                PreviousDue = _oCustomer.TotalDue,
                                ReceiveAmount = (decimal)invoiceSalesOrder.RecAmount,
                                PresentDue = _oCustomer.TotalDue + invoiceSalesOrder.PaymentDue,
                                SMSType = EnumSMSType.SalesTime,
                                SalesAmount = invoiceSalesOrder.TotalAmount,
                                CustomerCode = _oCustomer.Code,
                                //ProductNameList = ProductNameList.Select(i=>i.ProductName).ToList()
                            });
                        }

                        int concernId = User.Identity.GetConcernId();
                        decimal previousBalance;
                        SMSPaymentMaster smsAmountDetails = _smsBillPaymentBkashService.GetByConcernId(concernId);
                        previousBalance = smsAmountDetails.TotalRecAmt;
                        var sysInfos = _SysInfoService.GetSystemInformationByConcernId(User.Identity.GetConcernId());
                        decimal smsFee = sysInfos.smsCharge;

                        if (smsAmountDetails.TotalRecAmt > 1)
                        {
                            var response = SMSHTTPService.SendSMS(EnumOnnoRokomSMSType.NumberSms, sms, previousBalance, SystemInfo, User.Identity.GetUserId<int>());

                            if (response != null || response.Count > 0)
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
                    TempData["IsInvoiceReady"] = true;
                }
            }



            //_salesOrderService.CorrectionStockData(User.Identity.GetConcernId());
            #region For POS Invoice
            //PrintInvoice oPriInvoice = new PrintInvoice();
            //oPriInvoice.print(salesOrder, _SisterConcern);
            #endregion

            if (Result)
                AddToastMessage("", "Order has been saved successfully.", ToastType.Success);
            else
                AddToastMessage("", "Order has been failed.", ToastType.Error);

            return Result;
        }

        private DataTable CreateSalesOrderDataTable(SalesOrderViewModel salesOrder)
        {


            DataTable dtSalesOrder = new DataTable();
            dtSalesOrder.Columns.Add("InvoiceDate", typeof(DateTime));
            dtSalesOrder.Columns.Add("InvoiceNo", typeof(string));
            dtSalesOrder.Columns.Add("VatPercentage", typeof(decimal));
            dtSalesOrder.Columns.Add("VatAmount", typeof(decimal));
            dtSalesOrder.Columns.Add("GrandTotal", typeof(decimal));
            dtSalesOrder.Columns.Add("TDiscountPercentage", typeof(decimal));
            dtSalesOrder.Columns.Add("TDiscountAmount", typeof(decimal));
            dtSalesOrder.Columns.Add("RecAmt", typeof(decimal));
            dtSalesOrder.Columns.Add("PaymentDue", typeof(decimal));
            dtSalesOrder.Columns.Add("TotalAmount", typeof(decimal));
            dtSalesOrder.Columns.Add("TotalDue", typeof(decimal));
            dtSalesOrder.Columns.Add("AdjAmount", typeof(decimal));
            dtSalesOrder.Columns.Add("Status", typeof(int));
            dtSalesOrder.Columns.Add("CustomerId", typeof(int));
            dtSalesOrder.Columns.Add("ConcernId", typeof(int));
            dtSalesOrder.Columns.Add("CreatedBy", typeof(int));
            dtSalesOrder.Columns.Add("CreatedDate", typeof(DateTime));
            dtSalesOrder.Columns.Add("TotalOffer", typeof(decimal));
            dtSalesOrder.Columns.Add("NetDiscount", typeof(decimal));
            dtSalesOrder.Columns.Add("Remarks", typeof(string));
            dtSalesOrder.Columns.Add("TotalFractionAmt", typeof(decimal));
            dtSalesOrder.Columns.Add("LabourCost", typeof(decimal));
            dtSalesOrder.Columns.Add("PrevDue", typeof(decimal));

            DataRow row = null;

            row = dtSalesOrder.NewRow();
            row["InvoiceDate"] = salesOrder.SalesOrder.OrderDate;
            row["InvoiceNo"] = salesOrder.SalesOrder.InvoiceNo;
            row["VatPercentage"] = GetDefaultIfNull(salesOrder.SalesOrder.VATPercentage);
            row["VatAmount"] = GetDefaultIfNull(salesOrder.SalesOrder.VATAmount);
            row["GrandTotal"] = GetDefaultIfNull(salesOrder.SalesOrder.GrandTotal);
            row["TDiscountPercentage"] = GetDefaultIfNull(salesOrder.SalesOrder.TotalDiscountPercentage);
            row["TDiscountAmount"] = GetDefaultIfNull(salesOrder.SalesOrder.TotalDiscountAmount);
            row["PaymentDue"] = GetDefaultIfNull(salesOrder.SalesOrder.PaymentDue);
            row["RecAmt"] = GetDefaultIfNull(salesOrder.SalesOrder.RecieveAmount);
            row["TotalAmount"] = GetDefaultIfNull(salesOrder.SalesOrder.TotalAmount);
            row["TotalDue"] = (decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.PaymentDue)) - decimal.Parse(GetDefaultIfNull(salesOrder.SalesOrder.TotalDue)));
            row["AdjAmount"] = GetDefaultIfNull(salesOrder.SalesOrder.AdjAmount);
            row["Status"] = EnumSalesType.Sales;
            row["CustomerId"] = salesOrder.SalesOrder.CustomerId;
            row["ConcernId"] = User.Identity.GetConcernId();
            row["CreatedDate"] = DateTime.Now;
            row["CreatedBy"] = User.Identity.GetUserId<int>();
            row["TotalOffer"] = GetDefaultIfNull(salesOrder.SalesOrder.TotalOffer);
            row["NetDiscount"] = GetDefaultIfNull(salesOrder.SalesOrder.NetDiscount);
            row["Remarks"] = salesOrder.SalesOrder.Remarks;
            row["TotalFractionAmt"] = salesOrder.SalesOrder.TotalFractionAmt;
            row["LabourCost"] = GetDefaultIfNull(salesOrder.SalesOrder.LabourCost);
            row["PrevDue"] = salesOrder.SalesOrder.PrevDue;


            dtSalesOrder.Rows.Add(row);

            return dtSalesOrder;
        }

        private DataTable CreateSODetailDataTable(SalesOrderViewModel salesOrder)
        {
            //var stkid = salesOrder.SODetail.StockDetailId.Trim(',');
            //int stockdetailsid = Convert.ToInt32(stkid);
            DataTable dtSalesOrderDetail = new DataTable();
            dtSalesOrderDetail.Columns.Add("SOrderDetailID", typeof(int));
            dtSalesOrderDetail.Columns.Add("ProductId", typeof(int));
            dtSalesOrderDetail.Columns.Add("StockDetailId", typeof(int));
            dtSalesOrderDetail.Columns.Add("ColorId", typeof(int));
            dtSalesOrderDetail.Columns.Add("Status", typeof(int));
            dtSalesOrderDetail.Columns.Add("Quantity", typeof(decimal));
            dtSalesOrderDetail.Columns.Add("UnitPrice", typeof(decimal));
            dtSalesOrderDetail.Columns.Add("TAmount", typeof(decimal));
            dtSalesOrderDetail.Columns.Add("PPDisPer", typeof(decimal));
            dtSalesOrderDetail.Columns.Add("PPDisAmt", typeof(decimal));
            dtSalesOrderDetail.Columns.Add("MrpRate", typeof(decimal));
            dtSalesOrderDetail.Columns.Add("PPOffer", typeof(decimal));
            dtSalesOrderDetail.Columns.Add("Compressor", typeof(int));
            dtSalesOrderDetail.Columns.Add("Motor", typeof(int));
            dtSalesOrderDetail.Columns.Add("Panel", typeof(int));
            dtSalesOrderDetail.Columns.Add("Spareparts", typeof(int));
            dtSalesOrderDetail.Columns.Add("Service", typeof(int));

            dtSalesOrderDetail.Columns.Add("SFTRate", typeof(decimal));
            dtSalesOrderDetail.Columns.Add("TotalSFT", typeof(decimal));
            dtSalesOrderDetail.Columns.Add("FractionQty", typeof(decimal));
            dtSalesOrderDetail.Columns.Add("FractionAmt", typeof(decimal));
            dtSalesOrderDetail.Columns.Add("ActualSFT", typeof(decimal));
            dtSalesOrderDetail.Columns.Add("DOrderDetailId", typeof(int));
            dtSalesOrderDetail.Columns.Add("VehicleID", typeof(int));
            dtSalesOrderDetail.Columns.Add("VehicleNo", typeof(string));
            dtSalesOrderDetail.Columns.Add("OrderIndex", typeof(int));


            DataRow row = null;
            ProductWisePurchaseModel oProduct = null;
            int ProductID = 0;
            //int index = 1;
            foreach (var item in salesOrder.SODetails)
            {


                //var stockdetailid = item.StockDetailId.Split(',').Distinct().First();

                //int stockdetailsid = Convert.ToInt32(stockdetailid);
                row = dtSalesOrderDetail.NewRow();
                row["OrderIndex"] = item.OrderIndex;
                if (!string.IsNullOrEmpty(item.SODetailId))
                    row["SOrderDetailID"] = item.SODetailId;
                ProductID = Convert.ToInt32(item.ProductId);
                oProduct = _productService.GetAllProductIQueryable().FirstOrDefault(i => i.ProductID == ProductID);
                row["ProductId"] = item.ProductId;
                row["StockDetailId"] = item.StockDetailId;
                row["ColorId"] = item.ColorId;
                row["Status"] = item.Status;
                row["Quantity"] = item.Quantity;
                row["UnitPrice"] = Convert.ToDecimal(item.UnitPrice);/*Math.Round(Convert.ToDecimal(item.UnitPrice), 4);*/
                row["TAmount"] = item.UTAmount;
                row["PPDisPer"] = GetDefaultIfNull(item.PPDPercentage);
                row["PPDisAmt"] = GetDefaultIfNull(item.PPDAmount);
                row["MrpRate"] = Math.Round(Convert.ToDecimal(item.MRPRate), 4);
                row["PPOffer"] = GetDefaultIfNull(item.PPOffer);
                row["Compressor"] = item.GodownID;
                row["Motor"] = 0;
                row["Panel"] = 0;
                row["Spareparts"] = 0;
                row["Service"] = 0;
                row["SFTRate"] = item.RatePerArea;
                row["TotalSFT"] = item.TotalArea;
                row["FractionQty"] = item.FractionQty;
                row["FractionAmt"] = item.FractionAmt;
                row["ActualSFT"] = Math.Round(Convert.ToDecimal(item.Quantity) * (oProduct.SalesCSft / oProduct.ConvertValue), 4);
                if (item.DOID > 0)
                {
                    int DOProductID = Convert.ToInt32(item.ProductId);
                    var DoDetail = _dOService.GetDetailsById(item.DOID).FirstOrDefault(i => i.ProductID == DOProductID);
                    if (DoDetail != null)
                        row["DOrderDetailId"] = DoDetail.DODID;
                }
                else
                    row["DOrderDetailId"] = "0";
                row["VehicleID"] = string.IsNullOrEmpty(item.VehicleID) ? "" : item.VehicleID;
                row["VehicleNo"] = item.VehicleNo;
                dtSalesOrderDetail.Rows.Add(row);
            }

            return dtSalesOrderDetail;
        }


        private DataTable CreatePaymentDetailsDataTable(SalesOrderViewModel salesOrder)
        {
            DataTable dtPaymentDetails = new DataTable();
            dtPaymentDetails.Columns.Add("PaymentOptionId", typeof(int));
            dtPaymentDetails.Columns.Add("BankId", typeof(int));
            dtPaymentDetails.Columns.Add("PaidAmount", typeof(decimal));
            dtPaymentDetails.Columns.Add("ChequeNo", typeof(string));
            dtPaymentDetails.Columns.Add("PaidAmountAfterCharge", typeof(decimal));

            DataRow row = null;
            if (salesOrder.PaymentOptionDetails != null)
            {
                foreach (var item in salesOrder.PaymentOptionDetails)
                {
                    if (item.PaymentOptionId > 0)
                    {
                        var PaymentTypeName = _paymentDetailService.GetByIds(item.PaymentOptionId);
                        row = dtPaymentDetails.NewRow();
                        row["PaymentOptionId"] = item.PaymentOptionId;
                        if (PaymentTypeName.PaymentBankID == 0 && PaymentTypeName.Name == "Cash")
                        {
                            item.bankID = 0;
                        }
                        //if (PaymentTypeName.PaymentBankID == 0 && PaymentTypeName.Name == "Bank")
                        //{
                        //    item.bankID = item.bankID;
                        //}
                        if (PaymentTypeName.PaymentBankID > 0)
                        {
                            item.bankID = PaymentTypeName.PaymentBankID;
                        }
                        row["BankId"] = item.bankID;
                        row["PaidAmount"] = item.PaidAmount;
                        row["ChequeNo"] = item.ChecqueNo;
                        row["PaidAmountAfterCharge"] = item.PaidAmount - (item.PaidAmount * PaymentTypeName.Charge) / 100;
                        dtPaymentDetails.Rows.Add(row);

                    }
                    else
                    {

                        row = dtPaymentDetails.NewRow();
                        row["PaymentOptionId"] = DBNull.Value;
                        row["BankId"] = DBNull.Value;
                        row["PaidAmount"] = DBNull.Value;
                        row["ChequeNo"] = DBNull.Value;
                        row["PaidAmountAfterCharge"] = DBNull.Value;
                        dtPaymentDetails.Rows.Add(row);

                    }

                }
            }


            return dtPaymentDetails;
        }
        private bool IsForEdit(string previousAction)
        {
            return previousAction.Equals("edit");
        }

        private ActionResult ReturnCreateViewWithTempData()
        {
            ViewBag.Vehicle = GetAllVehicleDDL();
            ViewBag.ProductIds = GetAllProductsForDDL();
            ViewBag.CustomerIds = GetAllCustomerForDDL();
            int ConcernID = User.Identity.GetConcernId();

            var systemInfo = _SysInfoService.GetSystemInformationByConcernId(ConcernID);

            ViewBag.UnderRateSales = systemInfo.UnderPoRateSalesAllow;
            ViewBag.IsVulcanizing = systemInfo.IsVulcanizing;

            SalesOrderViewModel salesOrder = (SalesOrderViewModel)TempData.Peek("salesOrderViewModel");


            if (salesOrder != null)
            {
                TempData["salesOrderViewModel"] = salesOrder;

                var ListPaymentDetail = salesOrder.PaymentOptionDetails;
                if (ListPaymentDetail != null)
                {
                    if (ListPaymentDetail.Any())
                    {
                        int paymentId = ListPaymentDetail[0].PaymentOptionId;
                        if (paymentId == 0)
                        {
                            foreach (var key in ModelState.Keys.ToList())
                            {
                                if (key.EndsWith("PaymentOptionId"))
                                {
                                    ModelState.Remove(key);
                                }
                                if (key.EndsWith("Charge"))
                                {
                                    ModelState.Remove(key);
                                }
                                if (key.EndsWith("PaidAmount"))
                                {
                                    ModelState.Remove(key);
                                }
                            }
                            ModelState.Clear();
                            List<PaymentOptionDetailsTO> paymentOptionDetails = new List<PaymentOptionDetailsTO> { new PaymentOptionDetailsTO { Id = 1 } };
                            ViewBag.PaymentOptionIds = _paymentOptionService.GetAllForDDL();
                            salesOrder.PaymentOptionDetails = paymentOptionDetails;
                        }
                        else
                        {
                            ViewBag.PaymentOptionIds = _paymentOptionService.GetAllForDDL();
                        }
                    }
                    else
                    {
                        salesOrder.SODetails = salesOrder.SODetails ?? new List<CreateSalesOrderDetailViewModel>();
                        List<PaymentOptionDetailsTO> paymentOptionDetail = new List<PaymentOptionDetailsTO>();
                        paymentOptionDetail.Add(new PaymentOptionDetailsTO { Id = 1 });
                        ViewBag.PaymentOptionIds = _paymentOptionService.GetAllForDDL();
                        salesOrder.PaymentOptionDetails = paymentOptionDetail;

                        return View("Create", salesOrder);
                    }
                }
                else
                {
                    salesOrder.SODetails = salesOrder.SODetails ?? new List<CreateSalesOrderDetailViewModel>();
                    List<PaymentOptionDetailsTO> paymentOptionDetail = new List<PaymentOptionDetailsTO>();
                    paymentOptionDetail.Add(new PaymentOptionDetailsTO { Id = 1 });
                    ViewBag.PaymentOptionIds = _paymentOptionService.GetAllForDDL();
                    salesOrder.PaymentOptionDetails = paymentOptionDetail;

                    return View("Create", salesOrder);
                }


                return View("Create", salesOrder);
            }
            else
            {
                int ConcernId = System.Web.HttpContext.Current.User.Identity.GetConcernId();

                List<TOIdNameDDL> customers = GetAllCustomerForDDL();
                // Selecting the first customer by default
                int defaultCustomerId = customers.FirstOrDefault()?.Id ?? 0;
                var customer = _customerService.GetCustomerById(defaultCustomerId);
                decimal cDue = customer.TotalDue;
                string invNo = _miscellaneousService.GetUniqueKey(x => int.Parse(x.InvoiceNo));
                List<PaymentOptionDetailsTO> paymentOptionDetails = new List<PaymentOptionDetailsTO>();
                paymentOptionDetails.Add(new PaymentOptionDetailsTO { Id = 1 });
                ViewBag.PaymentOptionIds = _paymentOptionService.GetAllForDDL();

                if (ConcernId == 24)
                {
                    return View(new SalesOrderViewModel
                    {
                        SODetail = new CreateSalesOrderDetailViewModel(),
                        SODetails = new List<CreateSalesOrderDetailViewModel>(),
                        SalesOrder = new CreateSalesOrderViewModel { InvoiceNo = invNo, CustomerId = defaultCustomerId.ToString(), CurrentDue = Convert.ToString(cDue) },

                        PaymentOptionDetails = paymentOptionDetails
                    });
                }
                else
                    return View(new SalesOrderViewModel
                    {
                        SODetail = new CreateSalesOrderDetailViewModel(),
                        SODetails = new List<CreateSalesOrderDetailViewModel>(),
                        SalesOrder = new CreateSalesOrderViewModel { InvoiceNo = invNo },

                        PaymentOptionDetails = paymentOptionDetails
                    });



            }
        }


        private List<TOIdNameDDL> GetAllCustomerForDDL()
        {
            int CuserId = System.Web.HttpContext.Current.User.Identity.GetUserId<int>();
            int CEmpID = 0;

            if (User.IsInRole(ConstantData.ROLE_MOBILE_USER))
            {
                var user = _UserService.GetUserById(CuserId);
                CEmpID = user.EmployeeID;

                var Ccustomers = _customerService.GetAllCustomerByEmpNew(CEmpID);
                var vmCustomers = _mapper.Map<IEnumerable<Customer>, IEnumerable<CreateCustomerViewModel>>(Ccustomers).Select(s => new TOIdNameDDL
                {
                    Id = int.Parse(s.Id),
                    Name = s.Name + "(" + s.ContactNo + ")"
                }).ToList();
                return vmCustomers;
            }
            else
            {
                var customers = _customerService.GetAllCustomerNew(User.Identity.GetConcernId()).Select(s => new TOIdNameDDL
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

                var Ccustomers = _customerService.GetAllCustomerByEmpNew(CEmpID, customerId);
                var vmCustomers = _mapper.Map<IEnumerable<Customer>, IEnumerable<CreateCustomerViewModel>>(Ccustomers).FirstOrDefault();
                return Json(vmCustomers, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var customers = _customerService.GetAllCustomerNew(User.Identity.GetConcernId(), customerId).FirstOrDefault();
                return Json(customers, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        [Authorize]
        public JsonResult GetProductInfoById(int productId, int godownId)
        {
            var products = _productService.GetAllProductFromDetailByIdAndGid(productId, godownId);

            var vmProductDetails = _mapper.Map<IEnumerable<Tuple<int, string, string, decimal, string, string, string, Tuple<decimal?, string, decimal, int, int, string, string, Tuple<string, string, string, string, string, string, int, Tuple<int, decimal, decimal, decimal, decimal, decimal, int>>>>>, IEnumerable<GetProductViewModel>>(products).ToList();

            var updatedMrp = _productService.GetUpdatedMRP(productId);

            var vmProductGroupBY = (from vm in vmProductDetails
                                    join pu in _productUnitTypeService.GetAll() on vm.ProUnitTypeID equals pu.ProUnitTypeID
                                    join s in _sizeService.GetAll() on vm.SizeID equals s.SizeID
                                    group vm by new
                                    {
                                        vm.IMENo,
                                        vm.ProductId,
                                        vm.CategoryID,
                                        vm.ProductName,
                                        vm.ProductCode,

                                        vm.ColorId,
                                        vm.CategoryName,
                                        vm.ColorName,
                                        vm.ModelName,
                                        vm.GodownID,
                                        vm.GodownName,
                                        vm.CompanyName,
                                        vm.ProUnitTypeID,
                                        ChildUnit = pu.UnitName,
                                        ParentUnit = pu.Description,
                                        ConvertValue = vm.BundleQty == 0 ? pu.ConvertValue : vm.BundleQty,
                                        s.SizeID,
                                        SizeName = s.Description,
                                        vm.PurchaseCSft,
                                        vm.SalesCSft,
                                        vm.TotalSFT,
                                        vm.AdvSRate

                                    } into g
                                    select new GetProductViewModel
                                    {
                                        IMENo = g.Key.IMENo,
                                        ProductId = g.Key.ProductId,
                                        ProductCode = g.Key.ProductCode,
                                        ProductName = g.Key.ProductName,
                                        CategoryID = g.Key.CategoryID,

                                        CategoryName = g.Key.CategoryName,
                                        ColorName = g.Key.ColorName,
                                        ColorId = g.Key.ColorId,
                                        ModelName = g.Key.ModelName,
                                        StockDetailsId = g.Select(o => o.StockDetailsId).FirstOrDefault(),

                                        //MRPRate = (g.Select(o => o.MRPRate).FirstOrDefault()),
                                        MRPRate = g.Select(o => o.MRPRate).FirstOrDefault() * g.Key.ConvertValue,
                                        AdvSRate = g.Select(o => o.MRPRate).FirstOrDefault(),
                                        //ParentMRP = g.Select(o => o.MRPRate * g.Key.ConvertValue).FirstOrDefault(),
                                        ParentMRP = updatedMrp,
                                        MRPRate12 = g.Select(o => o.MRPRate12).FirstOrDefault(),
                                        CashSalesRate = g.Select(o => o.CashSalesRate).FirstOrDefault(),
                                        PWDiscount = g.Select(o => o.PWDiscount).FirstOrDefault(),
                                        PicturePath = g.Select(o => o.PicturePath).FirstOrDefault(),

                                        PreStock = g.Select(o => o.PreStock).FirstOrDefault(),
                                        OfferDescription = g.Select(o => o.OfferDescription).FirstOrDefault(),
                                        ProductType = g.Select(o => o.ProductType).FirstOrDefault(),
                                        CompressorWarrentyMonth = g.Select(o => o.CompressorWarrentyMonth).FirstOrDefault(),
                                        PanelWarrentyMonth = g.Select(o => o.PanelWarrentyMonth).FirstOrDefault(),

                                        MotorWarrentyMonth = g.Select(o => o.MotorWarrentyMonth).FirstOrDefault(),
                                        SparePartsWarrentyMonth = g.Select(o => o.SparePartsWarrentyMonth).FirstOrDefault(),
                                        ServiceWarrentyMonth = g.Select(o => o.ServiceWarrentyMonth).FirstOrDefault(),
                                        IsSelect = g.Select(o => o.IsSelect).FirstOrDefault(),
                                        Status = g.Select(o => o.Status).FirstOrDefault(),

                                        Quantity = g.Select(o => o.Quantity).FirstOrDefault(), //e.g.; gm
                                        GodownName = g.Key.GodownName,
                                        GodownID = g.Key.GodownID,
                                        SizeID = g.Key.SizeID,
                                        SizeName = g.Key.SizeName,
                                        CompanyName = g.Key.CompanyName,

                                        ChildUnit = g.Key.ChildUnit,
                                        ConvertValue = g.Key.ConvertValue,
                                        ParentUnit = g.Key.ParentUnit,
                                        PurchaseCSft = g.Key.PurchaseCSft,
                                        SalesCSft = g.Key.SalesCSft,
                                        ParentQty = (int)Math.Truncate(g.Select(o => o.PreStock).FirstOrDefault() / g.Key.ConvertValue), //e.g. KG
                                        ChildQty = (int)(g.Select(o => o.PreStock).FirstOrDefault() % g.Key.ConvertValue), //e.g. gm

                                        TotalSFT = g.Key.TotalSFT
                                    }).OrderBy(p => p.ProductId).FirstOrDefault();


            return Json(vmProductGroupBY, JsonRequestBehavior.AllowGet);

        }


        // Modified by Rizve
        //======================================================================




        private List<TOIdNameDDL> GetAllProductsForDDL()
        {
            var products = _productService.GetAllProductFromDetailById();

            var vmProductDetails = _mapper.Map<IEnumerable<Tuple<int, string, string, decimal, string, string, string, Tuple<decimal?, string, decimal, int, int, string, string, Tuple<string, string, string, string, string, string, int, Tuple<int, decimal, decimal, decimal, decimal, decimal, int>>>>>, IEnumerable<GetProductViewModel>>(products).ToList();

            foreach (var product in vmProductDetails)
            {
                decimal updatedMRP = _productService.GetUpdatedMRP(product.ProductId);

                product.MRPRate = updatedMRP;
            }

            var vmProductGroupBY = (from vm in vmProductDetails
                                    join pu in _productUnitTypeService.GetAll() on vm.ProUnitTypeID equals pu.ProUnitTypeID
                                    join s in _sizeService.GetAll() on vm.SizeID equals s.SizeID
                                    group vm by new
                                    {
                                        vm.IMENo,
                                        vm.ProductId,
                                        //vm.CategoryID,
                                        vm.ProductName,
                                        vm.ProductCode,
                                        vm.GodownID,

                                        vm.ColorId,
                                        vm.CategoryName,
                                        vm.ColorName,
                                        //vm.ModelName,
                                        vm.GodownName,
                                        vm.CompanyName,
                                        //vm.ProUnitTypeID,
                                        //ChildUnit = pu.UnitName,
                                        ParentUnit = pu.Description,
                                        ConvertValue = vm.BundleQty == 0 ? pu.ConvertValue : vm.BundleQty,
                                        s.SizeID,
                                        SizeName = s.Description,
                                        //vm.PurchaseCSft,
                                        //vm.SalesCSft,
                                        //vm.TotalSFT,
                                        //vm.AdvSRate,
                                        vm.MRPRate,
                                        vm.PreStock,
                                        Unit = pu.Description

                                    } into g
                                    orderby g.Key.ProductCode
                                    select new TOIdNameDDL
                                    {
                                        Id = g.Key.ProductId,
                                        Name = $"{g.Key.ProductName}({g.Key.ProductCode}), {g.Key.SizeName}, {g.Key.CompanyName}, Godown: {string.Join(", ", g.Select(p => p.GodownName).Distinct())}, Qty: {FormatQuantity(g.Key.PreStock, g.Key.Unit)}, MRP: {g.Key.MRPRate}",

                                        GodownID = g.Key.GodownID
                                    }).ToList();

            //MRP: { (g.Select(x => (x.MRPRate * g.Key.ConvertValue).ToString("0.00"))).FirstOrDefault()}
            return vmProductGroupBY;
        }




        private string FormatQuantity(decimal quantity, string unit)
        {
            if (unit.ToLower() == "kg")
            {
                decimal quantityKg = quantity / 1000;
                return $"{quantityKg} kg";
            }
            else
            {

                return $"{quantity} qty";
            }
        }









        [HttpGet]
        [Authorize]
        public JsonResult GetEmployeeInfoById(int employeeId)
        {

            var employees = _employeeService.GetAllEmployeeNew(User.Identity.GetConcernId(), employeeId).FirstOrDefault();
            return Json(employees, JsonRequestBehavior.AllowGet);

        }
        private List<TOIdNameDDL> GetAllEmployeeForDDL()
        {

            var employees = _employeeService.GetAllEmployeeNew(User.Identity.GetConcernId()).Select(s => new TOIdNameDDL
            {
                Id = s.Id,
                Name = s.Name + "(" + s.Code + ")"
            }).ToList();
            return employees;

        }


        private ActionResult HandleSalesOrder(SalesOrderViewModel newSalesOrder, FormCollection formCollection)
        {
            ViewBag.ProductIds = GetAllProductsForDDL();
            ViewBag.CustomerIds = GetAllCustomerForDDL();
            ViewBag.Vehicle = GetAllVehicleDDL();
            int ConcernID = User.Identity.GetConcernId();
            var systemInfo = _SysInfoService.GetSystemInformationByConcernId(ConcernID);
            ViewBag.IsVulcanizing = systemInfo.IsVulcanizing;

            if (newSalesOrder != null)
            {
                SalesOrderViewModel salesOrder = (SalesOrderViewModel)TempData.Peek("salesOrderViewModel");
                salesOrder = salesOrder ?? new SalesOrderViewModel()
                {
                    SalesOrder = newSalesOrder.SalesOrder
                };
                salesOrder.SODetail = new CreateSalesOrderDetailViewModel();

                if (formCollection.Get("addButton") != null)
                {
                    CheckAndAddModelErrorForAdd(newSalesOrder, salesOrder, formCollection);
                    foreach (var key in ModelState.Keys.ToList())
                    {
                        if (key.EndsWith("PaymentOptionId"))
                        {
                            ModelState.Remove(key);
                        }
                        if (key.EndsWith("Charge"))
                        {
                            ModelState.Remove(key);
                        }
                        if (key.EndsWith("bankID"))
                        {
                            ModelState.Remove(key);
                        }
                        if (key.EndsWith("PaidAmount"))
                        {
                            ModelState.Remove(key);
                        }
                    }


                    if (!ModelState.IsValid)
                    {
                        salesOrder.SODetails = salesOrder.SODetails ?? new List<CreateSalesOrderDetailViewModel>();

                        List<PaymentOptionDetailsTO> paymentOptionDetail = new List<PaymentOptionDetailsTO>();
                        paymentOptionDetail.Add(new PaymentOptionDetailsTO { Id = 1 });

                        ViewBag.PaymentOptionIds = _paymentOptionService.GetAllForDDL();
                        salesOrder.PaymentOptionDetails = paymentOptionDetail;

                        return View("Create", salesOrder);
                    }

                    var product = _productService.GetProductById(int.Parse(newSalesOrder.SODetail.ProductId));


                    if (salesOrder.SODetails != null &&
                        salesOrder.SODetails.Any(x => x.Status != EnumStatus.Updated && x.Status != EnumStatus.Deleted
                        && x.ProductId.Equals(newSalesOrder.SODetail.ProductId)))
                    {
                        AddToastMessage(string.Empty, "This product already exists in the order", ToastType.Error);
                        List<PaymentOptionDetailsTO> paymentOptionDetailss = newSalesOrder.PaymentOptionDetails;
                        ViewBag.PaymentOptionIds = _paymentOptionService.GetAllForDDL();
                        salesOrder.PaymentOptionDetails = paymentOptionDetailss;
                        int idCounters = 1;
                        foreach (var paymentOptionDetail in paymentOptionDetailss)
                        {
                            paymentOptionDetail.Id = idCounters;
                            idCounters++;
                        }
                        return View("Create", salesOrder);
                    }

                    AddToOrder(newSalesOrder, salesOrder, formCollection);
                    ModelState.Clear();
                    salesOrder.SODetail.DONo = newSalesOrder.SODetail.DONo;
                    salesOrder.SODetail.DOID = newSalesOrder.SODetail.DOID;
                    //ModelState.Clear();
                    List<PaymentOptionDetailsTO> paymentOptionDetails = newSalesOrder.PaymentOptionDetails;
                    ViewBag.PaymentOptionIds = _paymentOptionService.GetAllForDDL();
                    salesOrder.PaymentOptionDetails = paymentOptionDetails;
                    int idCounter = 1;
                    foreach (var paymentOptionDetail in paymentOptionDetails)
                    {
                        paymentOptionDetail.Id = idCounter;
                        idCounter++;
                    }

                    return View("Create", salesOrder);
                }
                else if (formCollection.Get("submitButton") != null)
                {

                    foreach (var key in ModelState.Keys.ToList())
                    {
                        if (key.EndsWith("PaymentOptionId"))
                        {
                            ModelState.Remove(key);
                        }
                        if (key.EndsWith("Charge"))
                        {
                            ModelState.Remove(key);
                        }
                        if (key.EndsWith("PaidAmount"))
                        {
                            ModelState.Remove(key);
                        }
                    }
                    CheckAndAddModelErrorForSave(newSalesOrder, salesOrder, formCollection);
                    decimal calGrandtotal = salesOrder.SODetails.Where(i => i.Status != EnumStatus.Deleted).Sum(i => Convert.ToDecimal(i.UnitPrice) * Convert.ToDecimal(i.Quantity)) + newSalesOrder.SalesOrder.TotalFractionAmt;

                    decimal calGrandTotal = Convert.ToDecimal(newSalesOrder.SalesOrder.TotalAmount) + Convert.ToDecimal(newSalesOrder.SalesOrder.NetDiscount) + Convert.ToDecimal(newSalesOrder.SalesOrder.AdjAmount) - Convert.ToDecimal(newSalesOrder.SalesOrder.VATAmount) - Convert.ToDecimal(newSalesOrder.SalesOrder.LabourCost);

                    if (Convert.ToDecimal(newSalesOrder.SalesOrder.GrandTotal) != calGrandTotal)
                    {
                        TempData["salesOrderViewModel"] = null;
                        AddToastMessage("", "Order has been failed. Please try again.", ToastType.Error);
                        return RedirectToAction("Index");
                    }

                    if (!ModelState.IsValid)
                    {
                        salesOrder.SODetails = salesOrder.SODetails ?? new List<CreateSalesOrderDetailViewModel>();

                        ModelState.Clear();
                        List<PaymentOptionDetailsTO> paymentOptionDetails = new List<PaymentOptionDetailsTO> { new PaymentOptionDetailsTO { Id = 1 } };

                        ViewBag.PaymentOptionIds = _paymentOptionService.GetAllForDDL();
                        salesOrder.PaymentOptionDetails = paymentOptionDetails;

                        //TempData["ToastrMessage"] = "Payment Details Not Found ";

                        return View("Create", salesOrder);
                    }
                    bool Result = SaveOrder(newSalesOrder, salesOrder, formCollection);
                    ModelState.Clear();
                    TempData["salesOrderViewModel"] = null;

                    return RedirectToAction("Index");
                }
                else
                {
                    return View("Create", new PurchaseOrderViewModel
                    {
                        PODetail = new CreatePurchaseOrderDetailViewModel(),
                        PODetails = new List<CreatePurchaseOrderDetailViewModel>(),
                        PurchaseOrder = new CreatePurchaseOrderViewModel()
                    });
                }
            }
            else
            {
                AddToastMessage("", "No order data found to save.", ToastType.Error);
                return RedirectToAction("Create");
            }
        }



        [HttpGet]
        [Authorize]
        public ActionResult Invoice(int orderId)
        {
            TempData["IsInvoiceReadyById"] = true;
            TempData["OrderId"] = orderId;
            return RedirectToAction("Index");
        }


        //[HttpGet]
        //[Authorize]
        //public JsonResult GetProductInfoById(int productId)
        //{
        //    var products = _productService.GetAllProductFromDetailById(productId);

        //    var vmProductDetails = _mapper.Map<IEnumerable<Tuple<int, string, string, decimal, string, string, string, Tuple<decimal?, string, decimal, int, int, string, string, Tuple<string, string, string, string, string, string, int, Tuple<int, decimal, decimal, decimal, decimal, decimal>>>>>, IEnumerable<GetProductViewModel>>(products).ToList();


        //    var vmProductGroupBY = (from vm in vmProductDetails
        //                            join pu in _productUnitTypeService.GetAll() on vm.ProUnitTypeID equals pu.ProUnitTypeID
        //                            join s in _sizeService.GetAll() on vm.SizeID equals s.SizeID
        //                            group vm by new
        //                            {
        //                                vm.IMENo,
        //                                vm.ProductId,
        //                                vm.CategoryID,
        //                                vm.ProductName,
        //                                vm.ProductCode,

        //                                vm.ColorId,
        //                                vm.CategoryName,
        //                                vm.ColorName,
        //                                vm.ModelName,
        //                                vm.GodownName,
        //                                vm.CompanyName,
        //                                vm.ProUnitTypeID,
        //                                ChildUnit = pu.UnitName,
        //                                ParentUnit = pu.Description,
        //                                ConvertValue = vm.BundleQty == 0 ? pu.ConvertValue : vm.BundleQty,
        //                                s.SizeID,
        //                                SizeName = s.Description,
        //                                vm.PurchaseCSft,
        //                                vm.SalesCSft,
        //                                vm.TotalSFT,
        //                                vm.AdvSRate

        //                            } into g
        //                            select new GetProductViewModel
        //                            {
        //                                IMENo = g.Key.IMENo,
        //                                ProductId = g.Key.ProductId,
        //                                ProductCode = g.Key.ProductCode,
        //                                ProductName = g.Key.ProductName,
        //                                CategoryID = g.Key.CategoryID,

        //                                CategoryName = g.Key.CategoryName,
        //                                ColorName = g.Key.ColorName,
        //                                ColorId = g.Key.ColorId,
        //                                ModelName = g.Key.ModelName,
        //                                StockDetailsId = g.Select(o => o.StockDetailsId).FirstOrDefault(),

        //                                MRPRate = ((g.Select(o => o.AdvSRate).FirstOrDefault()) / g.Key.ConvertValue),
        //                                AdvSRate = g.Select(o => o.AdvSRate).FirstOrDefault(),
        //                                ParentMRP = g.Select(o => o.AdvSRate).FirstOrDefault(),
        //                                MRPRate12 = g.Select(o => o.MRPRate12).FirstOrDefault(),
        //                                CashSalesRate = g.Select(o => o.CashSalesRate).FirstOrDefault(),
        //                                PWDiscount = g.Select(o => o.PWDiscount).FirstOrDefault(),
        //                                PicturePath = g.Select(o => o.PicturePath).FirstOrDefault(),

        //                                PreStock = g.Select(o => o.PreStock).FirstOrDefault(),
        //                                OfferDescription = g.Select(o => o.OfferDescription).FirstOrDefault(),
        //                                ProductType = g.Select(o => o.ProductType).FirstOrDefault(),
        //                                CompressorWarrentyMonth = g.Select(o => o.CompressorWarrentyMonth).FirstOrDefault(),
        //                                PanelWarrentyMonth = g.Select(o => o.PanelWarrentyMonth).FirstOrDefault(),

        //                                MotorWarrentyMonth = g.Select(o => o.MotorWarrentyMonth).FirstOrDefault(),
        //                                SparePartsWarrentyMonth = g.Select(o => o.SparePartsWarrentyMonth).FirstOrDefault(),
        //                                ServiceWarrentyMonth = g.Select(o => o.ServiceWarrentyMonth).FirstOrDefault(),
        //                                IsSelect = g.Select(o => o.IsSelect).FirstOrDefault(),
        //                                Status = g.Select(o => o.Status).FirstOrDefault(),

        //                                Quantity = g.Select(o => o.Quantity).FirstOrDefault(), //e.g.; gm
        //                                GodownName = g.Key.GodownName,
        //                                SizeID = g.Key.SizeID,
        //                                SizeName = g.Key.SizeName,
        //                                CompanyName = g.Key.CompanyName,

        //                                ChildUnit = g.Key.ChildUnit,
        //                                ConvertValue = g.Key.ConvertValue,
        //                                ParentUnit = g.Key.ParentUnit,
        //                                PurchaseCSft = g.Key.PurchaseCSft,
        //                                SalesCSft = g.Key.SalesCSft,
        //                                ParentQty = (int)Math.Truncate(g.Select(o => o.PreStock).FirstOrDefault() / g.Key.ConvertValue), //e.g. KG
        //                                ChildQty = (int)(g.Select(o => o.PreStock).FirstOrDefault() % g.Key.ConvertValue), //e.g. gm

        //                                TotalSFT = g.Key.TotalSFT
        //                            }).OrderBy(p => p.ProductId).FirstOrDefault();



        //    return Json(vmProductGroupBY, JsonRequestBehavior.AllowGet);

        //}


        [HttpGet]
        [Authorize]
        public ActionResult Challan(int orderId)
        {
            TempData["IsChallanReadyById"] = true;
            TempData["OrderId"] = orderId;
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public ActionResult DailySalesReport()
        {
            return View("DailySalesReport");
        }

        [HttpGet]
        [Authorize]
        public ActionResult MonthlySalesReport()
        {
            return View("MonthlySalesReport");
        }

        [HttpGet]
        [Authorize]
        public ActionResult YearlySalesReport()
        {
            return View("YearlySalesReport");
        }

        [HttpGet]
        [Authorize]
        public ActionResult CustomerSalesReport()
        {
            return View("CustomerSalesReport");
        }

        [HttpGet]
        [Authorize]
        public ActionResult MOWiseSalesReport()
        {
            return View("MOWiseSalesReport");
        }

        [HttpGet]
        [Authorize]
        public ActionResult MOWiseCustomerDue()
        {
            return View("MOWiseCustomerDue");
        }

        [HttpGet]
        [Authorize]
        public JsonResult GetProductDetailByIMEINo(string imeiNo)
        {
            if (!string.IsNullOrEmpty(imeiNo))
            {

                if (User.IsInRole(ConstantData.ROLE_MOBILE_USER))
                {
                    //int EmployeeID = ConstantData.GetEmployeeIDByUSerID(User.Identity.GetUserId<int>());
                    var user = _UserService.GetUserById(User.Identity.GetUserId<int>());
                    int EmployeeID = user.EmployeeID;
                    var customProductDetails = _productService.SRWiseGetAllProductFromDetail(EmployeeID);
                    var vmProductDetails = _mapper.Map<IEnumerable<Tuple<int, string, string, decimal, string, string, string, Tuple<decimal?, string, decimal, int, int, string, string>>>, IEnumerable<GetProductViewModel>>(customProductDetails);

                    var vmProduct = vmProductDetails.FirstOrDefault(x => x.IMENo.ToLower().Equals(imeiNo.Trim().ToLower()));
                    if (vmProduct != null)
                    {
                        return Json(new
                        {
                            Code = vmProduct.ProductCode,
                            Name = vmProduct.ProductName,
                            Id = vmProduct.ProductId,
                            StockDetailId = vmProduct.StockDetailsId,
                            ColorId = vmProduct.ColorId,
                            ColorName = vmProduct.ColorName,
                            MrpRate = vmProduct.MRPRate,
                            IMEINo = vmProduct.IMENo,
                            OfferDescription = vmProduct.OfferDescription,
                        }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(false, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    var customProductDetails = _productService.GetAllProductFromDetail();
                    var vmProductDetails = _mapper.Map<IEnumerable<Tuple<int, string, string, decimal, string, string, string,
                        Tuple<decimal?, string, decimal, int, int, string, string, Tuple<string, string, string, string, string, string, int, Tuple<int, decimal, decimal, decimal, decimal>>>>>, IEnumerable<GetProductViewModel>>(customProductDetails);

                    var vmProduct = vmProductDetails.FirstOrDefault(x => x.IMENo.ToLower().Equals(imeiNo.Trim().ToLower()));
                    if (vmProduct != null)
                    {
                        return Json(new
                        {
                            Code = vmProduct.ProductCode,
                            Name = vmProduct.ProductName,
                            Id = vmProduct.ProductId,
                            StockDetailId = vmProduct.StockDetailsId,
                            ColorId = vmProduct.ColorId,
                            ColorName = vmProduct.ColorName,
                            MrpRate = vmProduct.MRPRate,
                            IMEINo = vmProduct.IMENo,
                            OfferDescription = vmProduct.OfferDescription,
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }


            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpGet]
        [Authorize]
        public ActionResult MOWiseSDetailsReport()
        {
            return View("MOWiseSDetailsReport");
        }

        [HttpGet]
        [Authorize]
        public ActionResult SRWiseCustomerSalesSummary()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult DailyWorkSheetReport()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult MonthlyBenefitReport()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult ProductWiseBenefitReport()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult ProductWiseSalesReport()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult ProductWiseSalesDetailsReport()
        {
            return View();
        }

        public ActionResult AdminSalesReport()
        {
            @ViewBag.Concerns = new SelectList(_SisterConcern.GetAll(), "ConcernID", "Name");
            return View();
        }

        public ActionResult SalesBenefitReport()
        {

            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult PrintPOSInvoice(int orderId)
        {
            TempData["POSSOrderID"] = orderId;
            TempData["IsPOSInvoiceReady"] = true;
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public ActionResult MoneyReceipt(int orderId, bool isPosRecipt)
        {
            TempData["SorderId"] = orderId;
            TempData["isPosRecipt"] = isPosRecipt;
            TempData["IsMoneyReceiptById"] = true;
            return RedirectToAction("Index");
        }


        [HttpGet]
        [Authorize]
        [Route("index")]
        public async Task<ActionResult> SOrder(int id)
        {
            int userId = System.Web.HttpContext.Current.User.Identity.GetUserId<int>();
            var Concern = _SisterConcern.GetSisterConcernById(id);
            var DateRange = GetFirstAndLastDateOfMonth(DateTime.Today);
            ViewBag.FromDate = DateRange.Item1;
            ViewBag.ToDate = DateRange.Item2;
            ViewBag.ConcernID = id;
            ViewBag.ConcernName = Concern.Name;
            if (TempData.ContainsKey("ConcernSOrderData"))
            {
                var Sorder = (List<GetSalesOrderViewModel>)TempData["ConcernSOrderData"];
                return View(Sorder);
            }
            var customSO = _salesOrderService.GetAllSalesOrderAsyncByUserID(userId, DateRange.Item1, DateRange.Item2, EnumSalesType.Sales);
            var vmSO = _mapper.Map<IEnumerable<Tuple<int, string, DateTime, string, string, decimal, EnumSalesType, Tuple<string>>>,
        IEnumerable<GetSalesOrderViewModel>>(await customSO);
            return View(vmSO);

        }
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> SOrder(FormCollection formCollection)
        {
            int ConcernID = 0;
            if (!string.IsNullOrEmpty(formCollection["FromDate"]))
                ViewBag.FromDate = Convert.ToDateTime(formCollection["FromDate"]);
            if (!string.IsNullOrEmpty(formCollection["ToDate"]))
                ViewBag.ToDate = Convert.ToDateTime(formCollection["ToDate"]);
            if (!string.IsNullOrEmpty(formCollection["ConcernID"]))
                ConcernID = Convert.ToInt32(formCollection["ConcernID"]);
            var DateRange = GetFirstAndLastDateOfMonth(DateTime.Today);
            ViewBag.FromDate = DateRange.Item1;
            ViewBag.ToDate = DateRange.Item2;

            List<EnumSalesType> status = new List<EnumSalesType>();
            status.Add(EnumSalesType.Sales);
            status.Add(EnumSalesType.Pending);
            var customSO = _salesOrderService.GetAllSalesOrderAsync(DateRange.Item1, DateRange.Item2, status, IsVATManager(), User.Identity.GetConcernId());
            var vmSO = _mapper.Map<IEnumerable<Tuple<int, string, DateTime, string, string, decimal, EnumSalesType, Tuple<string>>>,
            IEnumerable<GetSalesOrderViewModel>>(await customSO);
            return View(vmSO);
            //return RedirectToAction("SOrder", ConcernID);
        }


        #region Get Add New Payment
        public ActionResult AddNew(int Id)
        {
            int concernId = User.Identity.GetConcernId();
            ViewBag.PaymentOptionIds = _paymentOptionService.GetAllForDDL();
            PaymentOptionDetailsTO objOptions = new PaymentOptionDetailsTO() { Id = Id };
            return View("~/Views/SalesOrder/_PaymentOptionDetails.cshtml", objOptions);
        }


        #endregion



        private List<TOIdNameDDL> GetAllVehicleDDL()
        {
            var vmProductGroupBY = _vehicleService.GetAllIQueryable().Select(s => new TOIdNameDDL
            {
                Id = s.VehicleID,
                Name = s.Name,
            }).ToList();
            return vmProductGroupBY.ToList();
        }

    }
}