using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSWEB.Model.TO
{
    public class CitizenChattarTO
    {
        public int Id { get; set; }
        public string ServicesType { get; set; }
        public string DescriptionofServices { get; set; }
        public string ServiceDeliveryProcess { get; set; }
        public string ServiceCost { get; set; }
        public string ServicedeliveryPeriod { get; set; }
        public string ResponsibleStaff { get; set; }
        public string RoomNo { get; set; }
        public string Remarks { get; set; }

    }
}
