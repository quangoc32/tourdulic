namespace TourDulich.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("NguoiDung")]
    public partial class NguoiDung
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NguoiDung()
        {
            DanhGias = new HashSet<DanhGia>();
            DatTours = new HashSet<DatTour>();
        }

        [Key]
        public int ID_NguoiDung { get; set; }

        [StringLength(100)]
        
        public string HoTen { get; set; }

        [StringLength(100)]
       
        public string Email { get; set; }

        [StringLength(100)]
        public string TaiKhoan { get; set; }

       
        [StringLength(100)]
        public string MatKhau { get; set; }

        public int? PhanQuyen { get; set; }

        
        [StringLength(20)]
        
        public string SoDienThoai { get; set; }

        [StringLength(255)]
        public string DiaChi { get; set; }

        public DateTime? NgayTao { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DanhGia> DanhGias { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DatTour> DatTours { get; set; }
    }
}
