namespace TourDulich.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DiaDiem")]
    public partial class DiaDiem
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DiaDiem()
        {
            Tours = new HashSet<Tour>();
        }

        [Key]
        public int ID_DiaDiem { get; set; }

        [StringLength(200)]
        public string TenDiaDiem { get; set; }

        [StringLength(200)]
        public string Hinh { get; set; }
        [StringLength(500)]
        public string MoTa { get; set; }

        [StringLength(100)]
        public string TinhThanh { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tour> Tours { get; set; }
    }
}



 