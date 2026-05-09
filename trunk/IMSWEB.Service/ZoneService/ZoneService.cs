using IMSWEB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMSWEB.Model;

namespace IMSWEB.Service
{
    public class ZoneService : IZoneService
    {

        private readonly IBaseRepository<Zone> _zoneRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ZoneService(IBaseRepository<Zone> zoneRepository, IUnitOfWork unitOfWork)
        {
            _zoneRepository = zoneRepository;
            _unitOfWork = unitOfWork;
        }

        public void AddZone(Zone zone)
        {
            _zoneRepository.Add(zone);
        }

        public void UpdateZone(Zone zone)
        {
            _zoneRepository.Update(zone);
        }

        public void SaveZone()
        {
            _unitOfWork.Commit();
        }

        public IEnumerable<Zone> GetAllZone()
        {
            return _zoneRepository.All.ToList();
        }

        public async Task<IEnumerable<Zone>> GetAllZoneAsync()
        {
            return await _zoneRepository.GetAllZoneAsync();
        }

        public Zone GetZoneById(int id)
        {
            return _zoneRepository.FindBy(x => x.ZoneID == id).First();
        }

        public void DeleteZone(int id)
        {
            _zoneRepository.Delete(x => x.ZoneID == id);
        }


        public Zone GetZoneByConcernAndZoneName(int ConcernID, string ZoneName)
        {
            return _zoneRepository.GetAll().FirstOrDefault(i => i.ConcernID == ConcernID && i.ZoneName.ToLower().Equals(ZoneName));
        }

        public IQueryable<Zone> GetAll()
        {
            return _zoneRepository.All;
        }

        public IQueryable<Zone> GetAllZoneIQueryable()
        {
            return _zoneRepository.All;
        }
    }
}
