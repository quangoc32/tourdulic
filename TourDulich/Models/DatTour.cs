namespace TourDulich.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DatTour")]
    public partial class DatTour
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DatTour()
        {
            ChiTietDatTours = new HashSet<ChiTietDatTour>();
        }

        [Key]
        public int ID_DatTour { get; set; }

        public int? ID_NguoiDung { get; set; }

        public DateTime? NgayDat { get; set; }

        public decimal? TongTien { get; set; }

        [StringLength(50)]
        public string TrangThai { get; set; }

        [StringLength(500)]
        public string GhiChu { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietDatTour> ChiTietDatTours { get; set; }

        public virtual NguoiDung NguoiDung { get; set; }
    }
}
