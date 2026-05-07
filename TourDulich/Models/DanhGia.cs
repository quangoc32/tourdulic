namespace TourDulich.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DanhGia")]
    public partial class DanhGia
    {
        [Key]
        public int ID_DanhGia { get; set; }

        public int? ID_Tour { get; set; }

        public int? ID_NguoiDung { get; set; }

        [StringLength(100)]
        public string HoTen { get; set; }

        [StringLength(1000)]
        public string NoiDung { get; set; }

        public int? SoSao { get; set; }

        public DateTime? NgayDanhGia { get; set; }

        public virtual NguoiDung NguoiDung { get; set; }

        public virtual Tour Tour { get; set; }
    }
}
