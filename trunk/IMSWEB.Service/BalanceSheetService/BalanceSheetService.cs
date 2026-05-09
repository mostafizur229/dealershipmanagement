using IMSWEB.Data;
using IMSWEB.Model.SPModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Service
{
    public class BalanceSheetService : IBalanceSheetService
    {

        private readonly IBalanceSheetRepository _balanceSheetRepository;

        public BalanceSheetService(IBalanceSheetRepository balanceSheetRepository)
        {
            _balanceSheetRepository = balanceSheetRepository;
        }

        public BalanceSheetSpModel GetBalanceSheetData(int ConcernId, DateTime from, DateTime to)
        {
            return _balanceSheetRepository.GetBalanceSheetData(ConcernId, from, to);
        }
    }
    
}
