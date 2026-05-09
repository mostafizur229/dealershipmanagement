using IMSWEB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMSWEB.Model.TOs;
namespace IMSWEB.Service
{
    public interface ICustomerService
    {
        void AddCustomer(Customer customer);
        void UpdateCustomer(Customer customer);
        void SaveCustomer();
        IEnumerable<Customer> GetAllCustomer();
        IEnumerable<Customer> GetAllBothCustomer();
        // added by mostafizur for customer picker load faster
        List<TOCustomer> GetAllCustomerNew(int concernId);
        IQueryable<Customer> GetAll();
        IQueryable<Customer> GetAll(int ConcernID);
        IEnumerable<Customer> GetAllCustomerByEmp(int EmpID);

        Task<IEnumerable<Customer>> GetAllCustomerAsync();

        Task<IEnumerable<Customer>> GetAllCustomerAsyncByEmpID(int EmpID);

        IEnumerable<Customer> GetAllCustomerByEmpNew(int EmpID, int customerId = 0);
        List<TOCustomer> GetAllCustomerNew(int concernId, int customerId = 0);

        IEnumerable<Tuple<string, string, string, string, string, string, decimal, Tuple<string, decimal>>>
        CustomerCategoryWiseDueRpt(int nConcernId, int nCustomerId, int nReportType, int DueType);

        Customer GetCustomerById(int id);
        void DeleteCustomer(int id);
        IQueryable<SRWiseCustomerStatusReportModel> AdminCustomerDueReport(int concernID, int CustomerType, int DueType);
        string GetUniqueCodeByType(EnumCustomerType customerType);
        //string GetUniqueMemberIDByType(EnumCustomerType customerType);
        IQueryable<Customer> GetAllCustomer(int ConcernID);
        //IQueryable<Customer> GetAllShowrooms();

        //IQueryable<Customer> GetAll();

        bool IsCustomerSalesOrCollectionExists(int customerID);
        bool IsCollectionFound(int customerId);
        IQueryable<Customer> GetAllIQueryable();
        IEnumerable<Tuple<string, string, string, string, string, string, decimal, Tuple<string, decimal, string>>>
        ZoneWiseCustomerCategoryWiseDueRpt(int concernId, int customerId, int reportType, int DueType, int ZoneID);
    }
}
