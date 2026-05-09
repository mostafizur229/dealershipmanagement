using IMSWEB.Model.SPModel;
using IMSWEB.SPViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Service
{
    public interface IBalanceSheetService
    {
        BalanceSheetSpModel GetBalanceSheetData(int ConcernId, DateTime from, DateTime to);
    }
}
