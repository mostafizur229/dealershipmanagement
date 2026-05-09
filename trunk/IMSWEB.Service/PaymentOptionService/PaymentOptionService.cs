using IMSWEB.Data;
using IMSWEB.Model;
using IMSWEB.Model.TO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Service
{
    public class PaymentOptionService : IPaymentOptionService
    {
        private readonly IBaseRepository<PaymentOption> _paymentOptionRepository;
        private readonly IBaseRepository<Bank> _bankRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentOptionService(IBaseRepository<PaymentOption> paymentOptionRepository, IBaseRepository<Bank> bankRepository, IUnitOfWork unitOfWork)
        {
            _paymentOptionRepository = paymentOptionRepository;
            _unitOfWork = unitOfWork;
            _bankRepository = bankRepository;
        }

        public void Add(PaymentOption model)
        {
            _paymentOptionRepository.Add(model);
        }

        public void Update(PaymentOption model)
        {
            _paymentOptionRepository.Update(model);
        }

        public bool Save()
        {
            try
            {
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }

        public IEnumerable<PaymentOption> GetAll()
        {
            return _paymentOptionRepository.All.ToList();
        }

        public IEnumerable<IdNameDDLTO> GetAllForDDL()
        {
            return _paymentOptionRepository.All.Select(s => new IdNameDDLTO
            {
                Id = s.Id,
                Name = s.Name 
            }).ToList();
        }

        public async Task<IEnumerable<PaymentOption>> GetAllAsync()
        {
            return await _paymentOptionRepository.GetAllAsync();
        }

        public PaymentOption GetById(int id)
        {
            return _paymentOptionRepository.FindBy(x => x.Id == id).First();
        }

        public IQueryable<MultiPaymentOptionTO> GetAllPaymentOption(int id)
        {
            return _paymentOptionRepository.GetAllMultiPaymentsOptions(_bankRepository, id);
        }

        public void Delete(int id)
        {
            _paymentOptionRepository.Delete(x => x.Id == id);
        }
    }
}
