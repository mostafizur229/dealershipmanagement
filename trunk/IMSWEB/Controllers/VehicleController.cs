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
    [RoutePrefix("color")]
    public class VehicleController : CoreController
    {
        IVehicleService _vehicleService;
        IMiscellaneousService<Vehicle> _miscellaneousService;
        IMapper _mapper;
          
        public VehicleController(IErrorService errorService,
            IVehicleService vehicleService, IMiscellaneousService<Vehicle> miscellaneousService, IMapper mapper)
            : base(errorService)
        {
            _vehicleService = vehicleService;
            _miscellaneousService = miscellaneousService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize]
        [Route("index")]
        public async Task<ActionResult> Index()
        {
            var colorsAsync = _vehicleService.GetAllAsync();
            var vmodel = _mapper.Map<IEnumerable<Vehicle>, IEnumerable<CreateColorViewModel>>(await colorsAsync);
            return View(vmodel);
        }

        [HttpGet]
        [Authorize]
        [Route("create")]
        public ActionResult Create()
        {
            string code = _miscellaneousService.GetUniqueKey(x => int.Parse(x.Code));
            return View(new CreateColorViewModel { Code = code });
        }

        [HttpPost]
        [Authorize]
        [Route("create/returnUrl")]
        public ActionResult Create(CreateColorViewModel newColor, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(newColor);

            if (newColor != null)
            {

                var existingColor = _miscellaneousService.GetDuplicateEntry(c => c.Name == newColor.Name);
                if (existingColor != null)
                {
                    AddToastMessage("", "A Color with same name already exists in the system. Please try with a different name.", ToastType.Error);
                    return View(newColor);
                }

                var model = _mapper.Map<CreateColorViewModel, Vehicle>(newColor);
                model.ConcernID = User.Identity.GetConcernId();
                _vehicleService.Add(model);
                _vehicleService.Save();

                AddToastMessage("", "Vehicle has been saved successfully.", ToastType.Success);
                return RedirectToAction("Create");
            }
            else
            {
                AddToastMessage("", "No Vehicle data found to create.", ToastType.Error);
                return RedirectToAction("Create");
            }
        }

        [HttpGet]
        [Authorize]
        [Route("edit/{id}")]
        public ActionResult Edit(int id)
        {
            var model = _vehicleService.GetById(id);
            var vmodel = _mapper.Map<Vehicle, CreateColorViewModel>(model);
            return View("Create", vmodel);
        }

        [HttpPost]
        [Authorize]
        [Route("edit/returnUrl")]
        public ActionResult Edit(CreateColorViewModel newColor, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(newColor);

            if (newColor != null)
            {
                var model = _vehicleService.GetById(int.Parse(newColor.Id));

                model.Code = newColor.Code;
                model.Name = newColor.Name;
                model.ConcernID = User.Identity.GetConcernId(); ;

                _vehicleService.Update(model);
                _vehicleService.Save();

                AddToastMessage("", "Vehicle has been updated successfully.", ToastType.Success);
                return RedirectToAction("Index");
            }
            else
            {
                AddToastMessage("", "No Vehicle data found to update.", ToastType.Error);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Authorize]
        [Route("delete/{id}")]
        public ActionResult Delete(int id)
        {
            _vehicleService.Delete(id);
            _vehicleService.Save();
            AddToastMessage("", "Vehicle has been deleted successfully.", ToastType.Success);
            return RedirectToAction("Index");
        }
    }
}