using IMSWEB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMSWEB.Model;

namespace IMSWEB.Service
{
    public class VehicleService : IVehicleService
    {
        private readonly IBaseRepository<Vehicle> _vehicleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public VehicleService(IBaseRepository<Vehicle> vehicleRepository, IUnitOfWork unitOfWork)
        {
            _vehicleRepository = vehicleRepository;
            _unitOfWork = unitOfWork;
        }

        public void Add(Vehicle color)
        {
            _vehicleRepository.Add(color);
        }

        public void Update(Vehicle color)
        {
            _vehicleRepository.Update(color);
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }

        public IEnumerable<Vehicle> GetAll()
        {
            return _vehicleRepository.All.ToList();
        }
        public IQueryable<Vehicle> GetAllIQueryable()
        {
            return _vehicleRepository.All;
        }
        public async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            return await _vehicleRepository.GetAllAsync();
        }

        public Vehicle GetById(int id)
        {
            return _vehicleRepository.FindBy(x=>x.VehicleID == id).First();
        }

        public void Delete(int id)
        {
            _vehicleRepository.Delete(x => x.VehicleID == id);
        }

                
        public IQueryable<Vehicle> GetAllIQuerayble()
        {
            return _vehicleRepository.All;
        }

       
    }
}
