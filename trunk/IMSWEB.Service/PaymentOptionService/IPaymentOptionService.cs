using IMSWEB.Model;
using IMSWEB.Model.TO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Service
{
    public interface IPaymentOptionService
    {
        void Add(PaymentOption model);
        void Update(PaymentOption model);
        bool Save();
        IEnumerable<PaymentOption> GetAll();
        IEnumerable<IdNameDDLTO> GetAllForDDL();
        Task<IEnumerable<PaymentOption>> GetAllAsync();
        PaymentOption GetById(int id);

        IQueryable<MultiPaymentOptionTO> GetAllPaymentOption(int id);
        void Delete(int id);
    }
}
