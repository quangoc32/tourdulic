namespace TourDulich.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("HinhAnhTour")]
    public partial class HinhAnhTour
    {
        [Key]
        public int ID_HinhAnhTour { get; set; }

        public int? ID_Tour { get; set; }

        [StringLength(255)]
        public string HinhAnh { get; set; }

        public int? HienThi { get; set; }

        public virtual Tour Tour { get; set; }
    }
}
