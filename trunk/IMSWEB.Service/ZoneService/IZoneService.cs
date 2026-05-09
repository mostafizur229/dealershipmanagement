using IMSWEB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Service
{
    public interface IZoneService
    {
        void AddZone(Zone zone);
        void UpdateZone(Zone zone);
        void SaveZone();
        IEnumerable<Zone> GetAllZone();
        IQueryable<Zone> GetAll();
        Task<IEnumerable<Zone>> GetAllZoneAsync();
        Zone GetZoneById(int id);
        void DeleteZone(int id);

        Zone GetZoneByConcernAndZoneName(int ConcernID, string ColorName);

        IQueryable<Zone> GetAllZoneIQueryable();
    }
}
