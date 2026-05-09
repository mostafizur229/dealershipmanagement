using IMSWEB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Data
{
    public static class VehicleExtensions
    {
        public static async Task<IEnumerable<Vehicle>> GetAllAsync(this IBaseRepository<Vehicle> VehicleRepository)
        {
            return await VehicleRepository.All.ToListAsync();
        }
    }
}
