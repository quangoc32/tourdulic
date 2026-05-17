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
            YeuCauHuys = new HashSet<YeuCauHuy>();
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

        public bool CoYeuCauHuy { get; set; } = false;

        /// <summary>Khách lẻ / Đoàn</summary>
        [StringLength(20)]
        public string LoaiDat { get; set; } = AppConstants.LoaiDat.KhachLe;

        [StringLength(100)]
        public string TruongDoan { get; set; }

        [StringLength(20)]
        public string SdtTruongDoan { get; set; }

        [StringLength(500)]
        public string GhiChuDoan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietDatTour> ChiTietDatTours { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<YeuCauHuy> YeuCauHuys { get; set; }

        public virtual NguoiDung NguoiDung { get; set; }
    }
}
