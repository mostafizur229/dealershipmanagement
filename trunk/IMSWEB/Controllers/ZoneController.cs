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
    [RoutePrefix("Zone")]
    public class ZoneController : CoreController
    {
        IZoneService _zoneService;
        IMiscellaneousService<Zone> _miscellaneousService;
        IMapper _mapper;

        public ZoneController(IErrorService errorService,
            IZoneService zoneService, IMiscellaneousService<Zone> miscellaneousService, IMapper mapper)
            : base(errorService)
        {
            _zoneService = zoneService;
            _miscellaneousService = miscellaneousService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize]
        [Route("index")]
        public async Task<ActionResult> Index()
        {
            var zonesAsync = _zoneService.GetAllZoneAsync();
            var vmodel = _mapper.Map<IEnumerable<Zone>, IEnumerable<CreateZoneViewModel>>(await zonesAsync);
            return View(vmodel);
        }

        [HttpGet]
        [Authorize]
        [Route("create")]
        public ActionResult Create()
        {
            string code = _miscellaneousService.GetUniqueKey(x => int.Parse(x.Code));
            return View(new CreateZoneViewModel { Code = code });
        }

        [HttpPost]
        [Authorize]
        [Route("create/returnUrl")]
        public ActionResult Create(CreateZoneViewModel newZone, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(newZone);

            if (newZone != null)
            {

                var existingZone = _miscellaneousService.GetDuplicateEntry(c => c.ZoneName == newZone.Name);
                if (existingZone != null)
                {
                    AddToastMessage("", "A Zone with same name already exists in the system. Please try with a different name.", ToastType.Error);
                    return View(existingZone);
                }

                var model = _mapper.Map<CreateZoneViewModel, Zone>(newZone);
                model.ConcernID = User.Identity.GetConcernId();
                _zoneService.AddZone(model);
                _zoneService.SaveZone();

                AddToastMessage("", "Zone has been saved successfully.", ToastType.Success);
                return RedirectToAction("Create");
            }
            else
            {
                AddToastMessage("", "No Zone data found to create.", ToastType.Error);
                return RedirectToAction("Create");
            }
        }

        [HttpGet]
        [Authorize]
        [Route("edit/{id}")]
        public ActionResult Edit(int id)
        {
            var model = _zoneService.GetZoneById(id);
            var vmodel = _mapper.Map<Zone, CreateZoneViewModel>(model);
            return View("Create", vmodel);
        }

        [HttpPost]
        [Authorize]
        [Route("edit/returnUrl")]
        public ActionResult Edit(CreateZoneViewModel newZone, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(newZone);

            if (newZone != null)
            {
                var model = _zoneService.GetZoneById(int.Parse(newZone.Id));

                model.Code = newZone.Code;
                model.ZoneName = newZone.Name;
                model.ConcernID = User.Identity.GetConcernId(); ;

                _zoneService.UpdateZone(model);
                _zoneService.SaveZone();

                AddToastMessage("", "Zone has been updated successfully.", ToastType.Success);
                return RedirectToAction("Index");
            }
            else
            {
                AddToastMessage("", "No Zone data found to update.", ToastType.Error);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Authorize]
        [Route("delete/{id}")]
        public ActionResult Delete(int id)
        {
            _zoneService.DeleteZone(id);
            _zoneService.SaveZone();
            AddToastMessage("", "Zone has been deleted successfully.", ToastType.Success);
            return RedirectToAction("Index");
        }
    }
}