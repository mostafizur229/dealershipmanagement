using IMSWEB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Service
{
    public interface IVehicleService
    {
        void Add(Vehicle color);
        void Update(Vehicle color);
        void Save();
        IEnumerable<Vehicle> GetAll();
        IQueryable<Vehicle> GetAllIQueryable();
        Task<IEnumerable<Vehicle    >> GetAllAsync();
        Vehicle GetById(int id);
        void Delete(int id);        
        IQueryable<Vehicle> GetAllIQuerayble();
    }
}
