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
    [RoutePrefix("supplier")]
    public class SupplierController : CoreController
    {
        ISupplierService _supplierService;
        ICustomerService _customerService;
        IEmployeeService _employeeService;
        IMiscellaneousService<Supplier> _miscellaneousService;
        IZoneService _zoneService;
        IMapper _mapper;
        string _photoPath = "~/Content/photos/suppliers";

        public SupplierController(IErrorService errorService,
            ISupplierService supplierService, ICustomerService customerService, IEmployeeService employeeService, IMiscellaneousService<Supplier> miscellaneousService, IMapper mapper, IZoneService zoneService)
            : base(errorService)
        {
            _supplierService = supplierService;
            _customerService = customerService;
            _employeeService = employeeService;
            _miscellaneousService = miscellaneousService;
            _mapper = mapper;
            _zoneService = zoneService;
        }

        [HttpGet]
        [Authorize]
        [Route("index")]
        public async Task<ActionResult> Index()
        {
            var suppliersAsync = _supplierService.GetAllSupplierAsync();
            var vmodel = _mapper.Map<IEnumerable<Supplier>, IEnumerable<GetSupplierViewModel>>(await suppliersAsync);
            return View(vmodel);
        }

        [HttpGet]
        [Authorize]
        [Route("create")]
        public ActionResult Create()
        {
            string code = _miscellaneousService.GetUniqueKey(x => int.Parse(x.Code));
            return View(new CreateSupplierViewModel { Code = code });
        }


        [HttpPost]
        [Authorize]
        [Route("create/returnUrl")]
        public ActionResult Create(CreateSupplierViewModel newSupplier, FormCollection formCollection,
            HttpPostedFileBase photo, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(newSupplier);

            if (newSupplier != null)
            {
                newSupplier.Code = CheakAndIncrement(newSupplier.Code);
                var existingSupplier = _miscellaneousService.GetDuplicateEntry(s => s.Code == newSupplier.Code);
                if (existingSupplier != null)
                {
                    //AddToastMessage("", "A Supplier with same contact no. already exists in the system. Please try with a different contact no.", ToastType.Error);

                    return View(newSupplier);
                }

                if (photo != null)
                {
                    var photoName = newSupplier.Code + "_" + newSupplier.Name;
                    newSupplier.PhotoPath = SaveHttpPostedImageFile(photoName, Server.MapPath(_photoPath), photo);
                }

                var supplier = _mapper.Map<CreateSupplierViewModel, Supplier>(newSupplier);
                supplier.ConcernID = User.Identity.GetConcernId();
                supplier.OpeningDue = decimal.Parse(GetDefaultIfNull(newSupplier.OpeningDue));
                //supplier.IsBoth = Convert.ToInt32(newSupplier.IsBoth ? 1 : 0);



                _supplierService.AddSupplier(supplier);
                _supplierService.SaveSupplier();

                if (supplier.IsBoth == 1)
                {
                    CreateCustomerFromSupplier(supplier);
                }

                AddToastMessage("", "Supplier has been saved successfully.", ToastType.Success);
                return RedirectToAction("Create");
            }
            else
            {
                AddToastMessage("", "No Supplier data found to save.", ToastType.Error);
                return RedirectToAction("Create");
            }
        }

        public void CreateCustomerFromSupplier(Supplier supplier)
        {
            string code = _customerService.GetUniqueCodeByType(EnumCustomerType.Both);
            int empId = _employeeService.GetAllEmployee().FirstOrDefault().EmployeeID;
            int zoneId = _zoneService.GetAllZone().FirstOrDefault().ZoneID;



            var customer = new Customer
            {
                Code = code,
                Name = supplier.Name,
                CustomerType = EnumCustomerType.Both,
                ContactNo = supplier.ContactNo,
                Address = supplier.Address,
                ConcernID = supplier.ConcernID,
                TotalDue = -supplier.TotalDue,
                Remarks = supplier.Remarks,
                EmployeeID = empId,
                ZoneID = zoneId,
                CreatedDate = DateTime.Now
            };
            _customerService.AddCustomer(customer);
            _customerService.SaveCustomer();

            #region set CustomerId to Supplier for Both identification

            var getSupplier = _supplierService.GetSupplierById(supplier.SupplierID);
            getSupplier.CustomerID = customer.CustomerID;

            _supplierService.UpdateSupplier(getSupplier);
            _supplierService.SaveSupplier();

            #endregion
        }

        private bool CodeExist(string Code)
        {
            var exSuplier = _supplierService.GetAllSupplier().FirstOrDefault(c => c.Code.Equals(Code, StringComparison.OrdinalIgnoreCase));
            return exSuplier != null;
        }

        private string CheakAndIncrement(string suplierCode)
        {
            if (CodeExist(suplierCode))
            {
                int maxCode = _supplierService.GetAllSupplier().Select(c => int.Parse(c.Code)).Max();
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
            var supplier = _supplierService.GetSupplierById(id);
            var vmodel = _mapper.Map<Supplier, CreateSupplierViewModel>(supplier);
            return View("Create", vmodel);
        }

        [HttpPost]
        [Authorize]
        [Route("edit/returnUrl")]
        public ActionResult Edit(CreateSupplierViewModel newSupplier, FormCollection formCollection,
            HttpPostedFileBase photo, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(newSupplier);

            if (newSupplier != null)
            {
                var existingSupplier = _supplierService.GetSupplierById(int.Parse(newSupplier.Id));
                if (photo != null)
                {
                    var photoName = newSupplier.Code + "_" + newSupplier.Name;
                    existingSupplier.PhotoPath = SaveHttpPostedImageFile(photoName, Server.MapPath(_photoPath), photo);
                }

                existingSupplier.Code = newSupplier.Code;
                existingSupplier.Name = newSupplier.Name;
                existingSupplier.ContactNo = newSupplier.ContactNo;
                existingSupplier.OpeningDue = decimal.Parse(newSupplier.OpeningDue);
                existingSupplier.TotalDue = decimal.Parse(newSupplier.TotalDue);
                existingSupplier.OwnerName = newSupplier.OwnerName;
                existingSupplier.ContactNo = newSupplier.ContactNo;
                existingSupplier.Address = newSupplier.Address;
                existingSupplier.Remarks = newSupplier.Remarks;
                existingSupplier.IsBoth = Convert.ToInt32(newSupplier.IsBoth);
                existingSupplier.ConcernID = User.Identity.GetConcernId();
                existingSupplier.CustomerID = Convert.ToInt32(newSupplier.CustomerID);

                _supplierService.UpdateSupplier(existingSupplier);
                _supplierService.SaveSupplier();

                if (existingSupplier.IsBoth == 1)
                {
                    var customer = _customerService.GetCustomerById(existingSupplier.CustomerID);
                    if (customer != null)
                    {
                        customer.Name = existingSupplier.Name;
                        customer.Address = existingSupplier.Address;
                        customer.ContactNo = existingSupplier.ContactNo;
                        customer.TotalDue = -existingSupplier.TotalDue;
                        customer.Remarks = existingSupplier.Remarks;
                        customer.EmployeeID = customer.EmployeeID;
                        _customerService.UpdateCustomer(customer);
                        _customerService.SaveCustomer();
                    }
                    else
                    {
                        CreateCustomerFromSupplier(existingSupplier);
                    }

                }

                AddToastMessage("", "Supplier has been updated successfully.", ToastType.Success);
                return RedirectToAction("Index");
            }
            else
            {
                AddToastMessage("", "No Supplier data found to update.", ToastType.Error);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Authorize]
        [Route("delete/{id}")]
        public ActionResult Delete(int id)
        {
            _supplierService.DeleteSupplier(id);
            _supplierService.SaveSupplier();
            AddToastMessage("", "Supplier has been deleted successfully.", ToastType.Success);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public ActionResult ConcernWiseSupplierDueRpt()
        {
            return View("ConcernWiseSupplierDueRpt");
        }

    }
}