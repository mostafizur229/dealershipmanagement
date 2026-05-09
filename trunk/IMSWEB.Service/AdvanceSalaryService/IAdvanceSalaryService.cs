using IMSWEB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Service
{
  public  interface IAdvanceSalaryService
    {
        void Add(AdvanceSalary AdvanceSalary);
        void Update(AdvanceSalary AdvanceSalary);
        void Save();
        IEnumerable<AdvanceSalary> GetAll();
        Task<IEnumerable<Tuple<int, int, string, string, string, string, string, Tuple<decimal, DateTime, string>>>> GetAllAsync(DateTime fromDate, DateTime toDate);
        AdvanceSalary GetById(int id);
        void Delete(int id);
    }
}
