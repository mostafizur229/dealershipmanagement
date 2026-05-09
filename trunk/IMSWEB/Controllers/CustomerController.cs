using AutoMapper;
using IMSWEB.Model;
using IMSWEB.Report;
using IMSWEB.Service;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMSWEB.Controllers
{
    [Authorize]
    [RoutePrefix("customer")]
    public class CustomerController : CoreController
    {
        ICustomerService _customerService;
        IEmployeeService _employeeService;
        IMiscellaneousService<Customer> _miscellaneousService;
        IMiscellaneousService<Supplier> _smiscellaneousService;
        IMapper _mapper;
        string _photoPath = "~/Content/photos/customers";
        IUserService _UserService;
        ISalesOrderService _SalesOrderService;
        ICreditSalesOrderService _creditSalesService;
        ISisterConcernService _SisterConcern;
        ISystemInformationService _SysInfoService;
        IBasicReport _BasicReportService;
        ICashCollectionService _cashCollectionService;
        ISupplierService _supplierService;
        IZoneService _zoneService;

        public CustomerController(IErrorService errorService,
            ICustomerService customerService, IMiscellaneousService<Customer> miscellaneousService, IMiscellaneousService<Supplier> smiscellaneousService, IMapper mapper, IEmployeeService employeeService
            , ISalesOrderService SalesOrderService, ISisterConcernService SisterConcern,
              ICreditSalesOrderService creditSalesService, IUserService UserService,
             ISystemInformationService SysInfoService, IBasicReport transactionalReportService, ICashCollectionService cashCollectionService, ISupplierService supplierService, IZoneService zoneService
            )
            : base(errorService)
        {
            _customerService = customerService;
            _miscellaneousService = miscellaneousService;
            _smiscellaneousService = smiscellaneousService;
            _mapper = mapper;
            _employeeService = employeeService;
            _UserService = UserService;
            _SalesOrderService = SalesOrderService;
            _creditSalesService = creditSalesService;
            _SisterConcern = SisterConcern;
            _SysInfoService = SysInfoService;
            _BasicReportService = transactionalReportService;
            _cashCollectionService = cashCollectionService;
            _supplierService = supplierService;
            _zoneService = zoneService;
        }

        //[HttpGet]
        //[Authorize]
        //[Route("index")]
        //public ActionResult Index(int? Page)
        //{
        //    int PageSize = 15;
        //    int Pages = Page.HasValue ? Convert.ToInt32(Page) : 1;
        //    int EmpID = 0;

        //    if (User.IsInRole(ConstantData.ROLE_MOBILE_USER))
        //    {
        //        //EmpID = ConstantData.GetEmployeeIDByUSerID(userId);
        //        var user = _UserService.GetUserById(User.Identity.GetUserId<int>());
        //        EmpID = user.EmployeeID;
        //    }




        //    var customersAsync = _customerService.GetAll();
        //    var vmodel = _mapper.Map<IQueryable<Customer>, List<GetCustomerViewModel>>(customersAsync);
        //    var pagelist = vmodel.ToPagedList(Pages, PageSize);
        //    return View(pagelist);


        //}

        //[HttpPost]
        //[Authorize]

        //public ActionResult Index(FormCollection formCollection)
        //{
        //    int PageSize = 15;
        //    int Pages = 1;
        //    int EmpID = 0;

        //    if (User.IsInRole(ConstantData.ROLE_MOBILE_USER))
        //    {
        //        //EmpID = ConstantData.GetEmployeeIDByUSerID(userId);
        //        var user = _UserService.GetUserById(User.Identity.GetUserId<int>());
        //        EmpID = user.EmployeeID;
        //    }




        //    IQueryable<Customer> customersAsync = null;
        //    if (!string.IsNullOrEmpty(formCollection["ContactNo"]))
        //    {
        //        string contactNo = formCollection["ContactNo"];
        //        customersAsync = _customerService.GetAllIQueryable().Where(i => i.ContactNo.Contains(contactNo));
        //    }
        //    if (!string.IsNullOrEmpty(formCollection["Name"]))
        //    {
        //        string name = formCollection["Name"];
        //        customersAsync = _customerService.GetAllIQueryable().Where(i => i.Name.Contains(name));
        //    }
        //    var vmodel = _mapper.Map<IQueryable<Customer>, List<GetCustomerViewModel>>(customersAsync);
        //    var pagelist = vmodel.ToPagedList(Pages, PageSize);
        //    return View(pagelist);


        //}

        [HttpGet]
        [Route("Index")]
        public ActionResult Index()
        {
            return View();
        }


        
        [HttpGet]
        [Route("GetCustomer")]
        public JsonResult GetCustomer()
        {
            try
            {

                int draw = Convert.ToInt32(Request.QueryString["draw"]);
                int start = Convert.ToInt32(Request.QueryString["start"]);
                int length = Convert.ToInt32(Request.QueryString["length"]);
                string searchValue = Request.QueryString["search[value]"];


                var query = _customerService.GetAll();


                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(c => c.Name.Contains(searchValue) ||
                                             c.Code.Contains(searchValue) ||
                                             c.ContactNo.Contains(searchValue) ||
                                             c.Address.Contains(searchValue)
                                             );
                }


                int totalRecords = query.Count();


                var customers = query
                    .OrderBy(c => c.CustomerID)
                    .Skip(start)
                    .Take(length)
                    .ToList();

                var mappedCustomers = _mapper.Map<List<Customer>, List<GetCustomerViewModel>>(customers);

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = mappedCustomers
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public JsonResult CreateCustomer(GetCustomerViewModel newCustomer)
        {
            try
            {
                string code = _customerService.GetUniqueCodeByType(newCustomer.CustomerType);
                if (_customerService.GetAll().Any(i => i.ContactNo.Equals(newCustomer.ContactNo)))
                {
                    return Json(new { Result = false, ErrorMsg = "Customer with same contact number already exists in the system." });
                }
                else
                {
                    Customer oNewCustomer = new Customer();
                    oNewCustomer.Code = code;
                    oNewCustomer.Name = newCustomer.Name;
                    if (newCustomer.CustomerType != EnumCustomerType.Hire)
                    {
                        oNewCustomer.TotalDue = Convert.ToDecimal(newCustomer.TotalDue);
                        oNewCustomer.OpeningDue = Convert.ToDecimal(newCustomer.TotalDue);
                    }
                    else
                    {
                        oNewCustomer.TotalDue = 0m;
                        oNewCustomer.OpeningDue = 0m;
                        //oNewCustomer.CreditDue = 0m;
                    }

                    oNewCustomer.Address = newCustomer.Address;
                    oNewCustomer.ContactNo = newCustomer.ContactNo;
                    int ConcernID = User.Identity.GetConcernId();
                    oNewCustomer.EmployeeID = _employeeService.GetAllEmployeeIQueryable().FirstOrDefault(i => i.ConcernID == ConcernID).EmployeeID;
                    oNewCustomer.ZoneID = _zoneService.GetAllZoneIQueryable().FirstOrDefault(i => i.ConcernID == ConcernID).ZoneID;
                    oNewCustomer.CustomerType = newCustomer.CustomerType;
                    AddAuditTrail(oNewCustomer, true);
                    _customerService.AddCustomer(oNewCustomer);
                    _customerService.SaveCustomer();
                    GetCustomerViewModel customer = new GetCustomerViewModel();
                    customer.Id = oNewCustomer.CustomerID.ToString();
                    customer.Name = oNewCustomer.Name;
                    customer.Code = oNewCustomer.Code;
                    //customer.MemberID = oNewCustomer.MemberID;
                    customer.TotalDue = oNewCustomer.TotalDue.ToString();
                    return Json(new { Result = true, ErrorMsg = "Customer saved successfully.", Customer = customer });
                }
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, ErrorMsg = ex.Message });
            }
        }


        [HttpGet]
        [Route("GetCustomerById")]
        public JsonResult GetCustomerById(int id)
        {
            var customer = _customerService.GetAll().FirstOrDefault(c => c.CustomerID == id);
            if (customer == null) return Json(null, JsonRequestBehavior.AllowGet);

            var vm = _mapper.Map<GetCustomerViewModel>(customer);
            return Json(vm, JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        [Route("GetCustomers")]
        public JsonResult GetCustomers(string search, int page = 1, int pageSize = 3) 
        {
            try
            {               

                var query = _customerService.GetAll();

                if (!string.IsNullOrEmpty(search)) 
                {
                    query = query.Where(c => c.Name.Contains(search) ||
                                             c.Code.Contains(search) ||
                                             c.Address.Contains(search) ||
                                             c.ContactNo.Contains(search));
                }

                int totalRecords = query.Count();

                // Calculate 'start' for Skip based on page and pageSize
                int skip = (page - 1) * pageSize;

                var customers = query
                    .OrderBy(c => c.CustomerID)
                    .Skip(skip)
                    .Take(pageSize) // Use pageSize for Take
                    .ToList();

                var mappedCustomers = _mapper.Map<List<Customer>, List<GetCustomerViewModel>>(customers);

                return Json(new
                {
                    // draw = draw, // No longer needed for Select2
                    recordsTotal = totalRecords,
                    // recordsFiltered = totalRecords, // Often same as recordsTotal for simple search
                    data = mappedCustomers,
                    pageSize = pageSize // Return pageSize to the client for pagination logic
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("create")]
        public ActionResult Create()
        {
            string code = _customerService.GetUniqueCodeByType(EnumCustomerType.Dealer);
            ViewBag.IsSalesOrCollectionExist = false;
            var AllEmployees = _employeeService.GetAllEmployee();
            var AllZones = _zoneService.GetAllZone();
            var FirstEmolyee = AllEmployees.FirstOrDefault();
            var FirstZone = AllZones.FirstOrDefault();
            return View(new CreateCustomerViewModel { Code = code, CustomerType = EnumCustomerType.Dealer, EmployeeId = FirstEmolyee.EmployeeID.ToString(), ZoneID = FirstZone.ZoneID.ToString() });
        }

        [HttpPost]
        [Authorize]
        [Route("create/returnUrl")]
        public ActionResult Create(CreateCustomerViewModel newCustomer, FormCollection formCollection,
            HttpPostedFileBase photo, string returnUrl)
        {
            ViewBag.IsSalesOrCollectionExist = _customerService.IsCustomerSalesOrCollectionExists(Convert.ToInt32(GetDefaultIfNull(newCustomer.Id)));

            CheckAndAddModelError(newCustomer, formCollection);
            if (!ModelState.IsValid)
                return View(newCustomer);

            if (newCustomer != null)
            {
                newCustomer.Code = CheakAndIncrement(newCustomer.Code);
                var existingCustomer = _miscellaneousService.GetDuplicateEntry(c => c.ContactNo == newCustomer.ContactNo);
                if (existingCustomer != null)
                {
                    AddToastMessage("", "A Customer with same contact no already exists in the system. Please try with a different contact no.", ToastType.Error);
                    return View(newCustomer);
                }

                MapFormCollectionValueWithNewEntity(newCustomer, formCollection);

                if (photo != null)
                {
                    var photoName = newCustomer.Code + "_" + newCustomer.Name;
                    newCustomer.PhotoPath = SaveHttpPostedImageFile(photoName, Server.MapPath(_photoPath), photo);
                }
                newCustomer.Code = newCustomer.Code.Trim();
                //newCustomer.MemberID = newCustomer.MemberID.Trim();
                var customer = _mapper.Map<CreateCustomerViewModel, Customer>(newCustomer);
                customer.ConcernID = User.Identity.GetConcernId();


                customer.OpeningDue = decimal.Parse(GetDefaultIfNull(newCustomer.OpeningDue));
                customer.CreatedDate = GetLocalDateTime();
                customer.CreatedBy = User.Identity.GetUserId<int>();

                int ConcernID = User.Identity.GetConcernId();
                var sisterConcern = _SisterConcern.GetSisterConcernById(ConcernID);

                if (ConcernID == 25)
                {
                    //customer.MemberID = newCustomer.MemberID;
                }

                _customerService.AddCustomer(customer);
                _customerService.SaveCustomer();

                if (newCustomer.CustomerType == EnumCustomerType.Both)
                {
                    CreateSupplierFromCustomer(customer);
                }

                AddToastMessage("", "Customer has been saved successfully.", ToastType.Success);
                return RedirectToAction("Create");
            }
            else
            {
                AddToastMessage("", "No Customer data found to save.", ToastType.Error);
                return RedirectToAction("Create");
            }
        }

        public void CreateSupplierFromCustomer(Customer customer)
        {
            var code = _smiscellaneousService.GetUniqueKey(x => int.Parse(x.Code));
            var supplier = new Supplier
            {
                Code = code,
                Name = customer.Name,
                ContactNo = customer.ContactNo,
                Address = customer.Address,
                PhotoPath = customer.PhotoPath,
                //OpeningDue = customer.OpeningDue,
                TotalDue = -customer.TotalDue,
                Remarks = customer.Remarks,
                ConcernID = customer.ConcernID,
                CustomerID = customer.CustomerID
            };

            _supplierService.AddSupplier(supplier);
            _supplierService.SaveSupplier();
        }


        private bool CodeExist(string Code)
        {
            var exSuplier = _customerService.GetAllCustomer().FirstOrDefault(c => c.Code.Equals(Code, StringComparison.OrdinalIgnoreCase));
            return exSuplier != null;
        }

        private string CheakAndIncrement(string suplierCode)
        {
            if (CodeExist(suplierCode))
            {
                int maxCode = _customerService.GetAllCustomer().Select(c => int.Parse(c.Code)).Max();
                int currentCode = maxCode + 1;

                return currentCode.ToString("00000");
            }
            return suplierCode;
        }


        [HttpGet]
        [Authorize]
        public ActionResult Status(int id)
        {
            var customer = _customerService.GetCustomerById(id);
            customer.Status = Convert.ToInt32(CustomrStatus.Deactive);

            _customerService.UpdateCustomer(customer);
            _customerService.SaveCustomer();

            return View("Index");
        }



        [HttpGet]
        [Authorize]
        [Route("edit/{id}")]
        public ActionResult Edit(int id)
        {

            var customer = _customerService.GetCustomerById(id);
            var vmodel = _mapper.Map<Customer, CreateCustomerViewModel>(customer);

            ViewBag.IsEditMode = true;
            ViewBag.CustomerType = customer.CustomerType;
            ViewBag.IsSalesOrCollectionExist = _customerService.IsCustomerSalesOrCollectionExists(customer.CustomerID);
            TempData["customerData"] = vmodel;
            return View("Create", vmodel);
        }

        [HttpPost]
        [Authorize]
        [Route("edit/returnUrl")]
        public ActionResult Edit(CreateCustomerViewModel newCustomer, FormCollection formCollection,
            HttpPostedFileBase photo, string returnUrl)
        {
            ViewBag.IsSalesOrCollectionExist = _customerService.IsCustomerSalesOrCollectionExists(Convert.ToInt32(GetDefaultIfNull(newCustomer.Id)));
            CheckAndAddModelError(newCustomer, formCollection);
            if (!ModelState.IsValid)
                return View("Create", newCustomer);

            if (newCustomer != null)
            {
                var customer = _mapper.Map<CreateCustomerViewModel, Customer>(newCustomer);
                var existingCustomer = _customerService.GetCustomerById(int.Parse(newCustomer.Id));
                MapFormCollectionValueWithExistingEntity(existingCustomer, formCollection);

                if (photo != null)
                {
                    var photoName = newCustomer.Code + "_" + newCustomer.Name;
                    existingCustomer.PhotoPath = SaveHttpPostedImageFile(photoName, Server.MapPath(_photoPath), photo);
                }

                existingCustomer.Code = newCustomer.Code.Trim();
                existingCustomer.Name = newCustomer.Name;
                existingCustomer.Remarks = newCustomer.Remarks;
                existingCustomer.ContactNo = newCustomer.ContactNo;


                existingCustomer.OpeningDue = decimal.Parse(GetDefaultIfNull(newCustomer.OpeningDue));
                existingCustomer.TotalDue = decimal.Parse(GetDefaultIfNull(newCustomer.TotalDue));


                //existingCustomer.TotalDue = decimal.Parse(newCustomer.TotalDue);
                if (!existingCustomer.CustomerType.Equals(newCustomer.CustomerType))
                {
                    if ((_SalesOrderService.GetAllIQueryable().Any(i => i.Status != (int)EnumSalesType.Return && i.CustomerID == existingCustomer.CustomerID))
                        || (_creditSalesService.GetAllIQueryable().Any(i => i.Status != (int)EnumSalesType.Return && i.CustomerID == existingCustomer.CustomerID)))
                    {
                        AddToastMessage("", "Type can't be changed.Because this customer has transactions.", ToastType.Error);
                    }
                    else
                    {
                        existingCustomer.CustomerType = newCustomer.CustomerType;
                    }
                }


                existingCustomer.CusDueLimit = decimal.Parse(newCustomer.CusDueLimit);
                existingCustomer.FName = newCustomer.FName;
                existingCustomer.CompanyName = newCustomer.CompanyId;
                existingCustomer.EmailID = newCustomer.EmailId;
                existingCustomer.NID = newCustomer.NId;
                existingCustomer.Address = newCustomer.Address;
                existingCustomer.RefName = newCustomer.RefName;
                existingCustomer.RefContact = newCustomer.RefContact;
                existingCustomer.RefFName = newCustomer.RefFName;
                existingCustomer.RefAddress = newCustomer.RefAddress;
                existingCustomer.ConcernID = User.Identity.GetConcernId();
                existingCustomer.ModifiedBy = User.Identity.GetUserId<int>();
                existingCustomer.ModifiedDate = GetLocalDateTime();
                existingCustomer.IsSupplier = Convert.ToInt32(newCustomer.IsSupplier);

                _customerService.UpdateCustomer(existingCustomer);
                _customerService.SaveCustomer();

                if (existingCustomer.IsSupplier == 1)
                {
                    CreateSupplierFromCustomer(existingCustomer);
                }

                AddToastMessage("", "Customer has been updated successfully.", ToastType.Success);
                return RedirectToAction("Index");
            }
            else
            {
                AddToastMessage("", "No Customer data found to update.", ToastType.Error);
                return RedirectToAction("Index");
            }
        }



        [HttpGet]
        [Authorize]
        [Route("delete/{id}")]
        public ActionResult Delete(int id)
        {
            if (_customerService.IsCollectionFound(id))
            {
                AddToastMessage("", "Cash collection found for selected customer, can't delete!", ToastType.Error);
                return RedirectToAction("Index");
            }

            _customerService.DeleteCustomer(id);
            _customerService.SaveCustomer();

            AddToastMessage("", "Customer has been deleted successfully.", ToastType.Success);
            return RedirectToAction("Index");
        }



        private void CheckAndAddModelError(CreateCustomerViewModel newCustomer, FormCollection formCollection)
        {
            if (string.IsNullOrEmpty(formCollection["EmployeesId"]))
                ModelState.AddModelError("EmployeeId", "Employee is required");
            else
                newCustomer.EmployeeId = formCollection["EmployeesId"];

            if (string.IsNullOrEmpty(formCollection["ZonesId"]))
                ModelState.AddModelError("ZoneID", "Zone is required");
            else
                newCustomer.ZoneID = formCollection["ZonesId"];

            if (!newCustomer.Code.Substring(0, 1).Equals(newCustomer.CustomerType.ToString().Substring(0, 1)))
                ModelState.AddModelError("Code", "Customer code and type don't match.");
            //if (!newCustomer.MemberID.Substring(0, 1).Equals(newCustomer.CustomerType.ToString().Substring(0, 1)))
            //    ModelState.AddModelError("MemberID", "Customer MemberID and type don't match.");
            var sysInfo = _SysInfoService.GetSystemInformationByConcernId(User.Identity.GetConcernId());
            if (sysInfo != null)
            {
                if (sysInfo.CustomerDueLimitApply == 1)
                {
                    //if (Convert.ToDecimal(newCustomer.CusDueLimit) <= 0m)
                    //    ModelState.AddModelError("CusDueLimit", "Customer Due Limit is Required.");
                }
            }
        }

        private void MapFormCollectionValueWithNewEntity(CreateCustomerViewModel newCustomer,
            FormCollection formCollection)
        {
            newCustomer.EmployeeId = formCollection["EmployeesId"];

            newCustomer.ZoneID = formCollection["ZonesId"];
        }

        private void MapFormCollectionValueWithExistingEntity(Customer customer,
            FormCollection formCollection)
        {
            customer.EmployeeID = int.Parse(formCollection["EmployeesId"]);
            customer.ZoneID = int.Parse(formCollection["ZonesId"]);
        }

        [HttpGet]
        [Authorize]
        public ActionResult ConcernWiseCustomerDueRpt()
        {
            return View("ConcernWiseCustomerDueRpt");
        }

        [HttpGet]
        public JsonResult GetEmployeeByCode(string Code)
        {
            var employee = _employeeService.GetAllEmployee().FirstOrDefault(i => i.Code.Equals(Code.PadLeft(5, '0')));
            if (employee == null)
                return Json(false, JsonRequestBehavior.AllowGet);
            return Json(employee, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetEmployeeByID(int ID)
        {
            var employee = _employeeService.GetEmployeeById(ID);
            if (employee == null)
                return Json(false, JsonRequestBehavior.AllowGet);
            return Json(employee, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult CustomerLedger()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult CustomerDueReport()
        {
            ViewBag.Title = "Customer Due Report";
            return View();
        }
        [HttpGet]
        [Authorize]
        public ActionResult AdminCustomerDueReport()
        {
            @ViewBag.Concerns = new SelectList(_SisterConcern.GetAll(), "ConcernID", "Name");
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult BothCustomerLedger()
        {
            return View();
        }


        [HttpGet]
        public JsonResult GetUniqueCodeByType(int CustomerType)
        {
            string code = string.Empty;
            if (CustomerType != default(int))
            {
                code = _customerService.GetUniqueCodeByType((EnumCustomerType)CustomerType);
            }
            return Json(code, JsonRequestBehavior.AllowGet);
        }



        //[HttpGet]
        //public JsonResult GetUniqueMemberIDByType(int CustomerType)
        //{
        //    string memberid = string.Empty;
        //    if (CustomerType != default(int))
        //    {
        //        memberid = _customerService.GetUniqueMemberIDByType((EnumCustomerType)CustomerType);
        //    }
        //    return Json(memberid, JsonRequestBehavior.AllowGet);
        //}


        


        [HttpGet]
        public JsonResult GetCustomerByConcernID(int ConcernID)
        {
            var Customers = _customerService.GetAllCustomer(ConcernID);
            if (Customers != null)
            {
                var vmCustomers = _mapper.Map<IQueryable<Customer>, IEnumerable<GetCustomerViewModel>>(Customers);
                return Json(new { Result = true, Data = vmCustomers }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Result = false, Data = Customers }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [Authorize]
        public PartialViewResult DetailsReport(FormCollection formCollection)
        {
            int CustomerID = 0;

            if (!string.IsNullOrEmpty(formCollection["txtCustomerName"]))
            {
                CustomerID = Convert.ToInt32(formCollection["txtCustomerName"]);
            }
            byte[] bytes = _BasicReportService.GetCustomerDetails(User.Identity.Name, User.Identity.GetConcernId(), CustomerID);
            TempData["ReportData"] = bytes;
            return PartialView("~/Views/Shared/_ReportViewer.cshtml");
        }

        [HttpGet]
        [Authorize]
        public ActionResult ZoneWiseCustomerDueRpt()
        {
            return View("ZoneWiseCustomerDueRpt");
        }

    }
}