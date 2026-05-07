namespace TourDulich.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LienHe")]
    public partial class LienHe
    {
        [Key]
        public int ID_LienHe { get; set; }

        [StringLength(100)]
        public string HoTen { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(200)]
        public string TieuDe { get; set; }

        public string NoiDung { get; set; }

        public DateTime? NgayGui { get; set; }

        [StringLength(50)]
        public string TrangThai { get; set; }
    }
}
