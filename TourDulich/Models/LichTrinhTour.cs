namespace TourDulich.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LichTrinhTour")]
    public partial class LichTrinhTour
    {
        [Key]
        public int ID_LichTrinhTour { get; set; }

        public int? ID_Tour { get; set; }

        public int? NgayThu { get; set; }

        [StringLength(200)]
        public string TieuDe { get; set; }

        public string NoiDung { get; set; }

        public virtual Tour Tour { get; set; }
    }
}
