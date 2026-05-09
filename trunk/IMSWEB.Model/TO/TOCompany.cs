using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Model.TO
{
    public class TOCompany
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }
        public string ConcernId { get; set; }

        public TOCompany(Company company)
        {
            Id = company.CompanyID;
            Name = company.Name;
            Code = company.Code;
            ConcernId = company.ConcernID.ToString();
        }

    }
}
