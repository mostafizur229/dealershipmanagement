using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMSWEB.Model
{
    public partial class Vehicle
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Vehicle()
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VehicleID { get; set; }

        [Required]
        [StringLength(250)]
        public string Name { get; set; }

        [Required]
        [StringLength(250)]
        public string Code { get; set; }

        public int ConcernID { get; set; }

        [ForeignKey("ConcernID")]
        public virtual SisterConcern SisterConcern { get; set; }
    }
}
