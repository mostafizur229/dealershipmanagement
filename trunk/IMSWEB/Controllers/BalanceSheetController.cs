using IMSWEB.Model;
using IMSWEB.Model.SPModel;
using IMSWEB.Report.DataSets;
using IMSWEB.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IMSWEB.Controllers
{
    [Authorize]
    [RoutePrefix("balance-sheet")]
    public class BalanceSheetController : Controller
    {
        private readonly IBalanceSheetService _balanceSheetService;
        ISystemInformationService _systemInformationService;
        public BalanceSheetController(IBalanceSheetService balanceSheetService, ISystemInformationService systemInformationService) { 
            _balanceSheetService = balanceSheetService;
            _systemInformationService = systemInformationService;
        }


        private BalanceSheetSpModel.CompanyDetailsViewModel GetCompanyDetails(int concernID)
        {
            var info = _systemInformationService.GetSystemInformationByConcernId(concernID);

            string companyName;
            if (info.Name.Contains("\r"))
            {
                var parts = info.Name.Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);
                companyName = string.Join(Environment.NewLine, parts);
            }
            else
            {
                companyName = info.Name;
            }

            return new BalanceSheetSpModel.CompanyDetailsViewModel
            {
                CompanyName = companyName,
                TelephoneNo = info.TelephoneNo,
                Address = info.Address
            };
        }



        public ActionResult Index(DateTime? fromDate, DateTime? toDate)
        {
            int concernId = User.Identity.GetConcernId();

            var companyDetails = GetCompanyDetails(concernId);

            DateTime from = fromDate ?? DateTime.Now;
            DateTime to = toDate ?? DateTime.Now;

            var model = _balanceSheetService.GetBalanceSheetData(concernId, from, to);
            model.CompanyDetails = companyDetails;

            return View(model);
        }


        //public ActionResult Index()
        //{
        //    int concernId = User.Identity.GetConcernId(); 
        //    var model = _balanceSheetService.GetBalanceSheetData(concernId);
        //    return View(model);
        //}
    }
}