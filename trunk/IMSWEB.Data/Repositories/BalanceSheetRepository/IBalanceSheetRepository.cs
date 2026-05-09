using IMSWEB.Model.SPModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Data
{
    public interface IBalanceSheetRepository
    {
        BalanceSheetSpModel GetBalanceSheetData(int concernId, DateTime from, DateTime to);
    }

}
