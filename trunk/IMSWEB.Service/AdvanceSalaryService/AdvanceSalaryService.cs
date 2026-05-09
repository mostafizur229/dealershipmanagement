using IMSWEB.Data;
using IMSWEB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Service
{
    public class AdvanceSalaryService : IAdvanceSalaryService
    {
        private readonly IBaseRepository<AdvanceSalary> _baseRepository;
        private readonly IBaseRepository<Employee> _EmployeeRepository;
        private readonly IBaseRepository<Department> _DepartmentRepository;
        private readonly IBaseRepository<Designation> _DesignationRepository;
        private readonly IBaseRepository<Grade> _GradeRepository;
        private readonly IUnitOfWork _unitOfWork;


        public AdvanceSalaryService(IBaseRepository<AdvanceSalary> baseRepository,
            IBaseRepository<Employee> EmployeeRepository, IBaseRepository<Department> DepartmentRepository,
            IBaseRepository<Designation> DesignationRepository, IBaseRepository<Grade> GradeRepository,
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _baseRepository = baseRepository;
            _EmployeeRepository = EmployeeRepository;
            _DepartmentRepository = DepartmentRepository;
            _DesignationRepository = DesignationRepository;
            _GradeRepository = GradeRepository;
        }

        public void Add(AdvanceSalary AdvanceSalary)
        {
            _baseRepository.Add(AdvanceSalary);
        }

        public void Update(AdvanceSalary AdvanceSalary)
        {
            _baseRepository.Update(AdvanceSalary);
        }

        public void Save()
        {
            _unitOfWork.Commit(); ;
        }

        public IEnumerable<AdvanceSalary> GetAll()
        {
            return _baseRepository.All.ToList();
        }

        public async Task<IEnumerable<Tuple<int, int, string, string, string, string, string, Tuple<decimal, DateTime, string>>>> GetAllAsync(DateTime fromDate, DateTime toDate)
        {
            return await _baseRepository.GetAllAsync(_DepartmentRepository,_DesignationRepository,_GradeRepository,_EmployeeRepository,fromDate,toDate);
        }

        public AdvanceSalary GetById(int id)
        {
            return _baseRepository.FindBy(x => x.ID == id).First();

        }

        public void Delete(int id)
        {
            _baseRepository.Delete(x => x.ID == id);
        }
    }
}
