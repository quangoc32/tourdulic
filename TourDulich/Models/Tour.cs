namespace TourDulich.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Tour")]
    public partial class Tour
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tour()
        {
            ChiTietDatTours = new HashSet<ChiTietDatTour>();
            DanhGias = new HashSet<DanhGia>();
            HinhAnhTours = new HashSet<HinhAnhTour>();
            LichTrinhTours = new HashSet<LichTrinhTour>();
        }

        [Key]
        public int ID_Tour { get; set; }

        [StringLength(200)]
        public string TenTour { get; set; }

        public string MoTa { get; set; }

        public decimal? Gia { get; set; }

        [StringLength(100)]
        public string DiemKhoiHanh { get; set; }

        [StringLength(10)]
        public string SoNgay { get; set; }

        public int? SoLuongToiDa { get; set; }

        [StringLength(20)]
        public string PhuongTien { get; set; }

        public int? ID_DanhMuc { get; set; }

        public int? ID_DiaDiem { get; set; }

        public DateTime? NgayTao { get; set; }

        public bool TrangThaiHoatDong { get; set; } = true;

        public bool DaXoa { get; set; } = false;

        public bool IsGiaTot { get; set; } = false;

        public bool IsUuDai { get; set; } = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietDatTour> ChiTietDatTours { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DanhGia> DanhGias { get; set; }

        public virtual DanhMuc DanhMuc { get; set; }

        public virtual DiaDiem DiaDiem { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HinhAnhTour> HinhAnhTours { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LichTrinhTour> LichTrinhTours { get; set; }
    }
}
