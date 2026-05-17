namespace TourDulich.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ChiTietDatTour")]
    public partial class ChiTietDatTour
    {
        [Key]
        public int ID_ChiTietDatTour { get; set; }

        public int? ID_DatTour { get; set; }

        public int? ID_Tour { get; set; }

        [Column(TypeName = "date")]
        public DateTime? NgayKhoiHanh { get; set; }

        public int? SoLuongNguoi { get; set; }
        [StringLength(50)]
        public string PhuongThucThanhToan {  get; set; }

        public decimal? Gia { get; set; }

        [StringLength(30)]
        public string LoaiDiemDon { get; set; }

        [StringLength(150)]
        public string DiemDon { get; set; }

        [StringLength(250)]
        public string DiaChiDon { get; set; }

        [StringLength(100)]
        public string TinhThanhDon { get; set; }

        public decimal? PhuThuDiemDon { get; set; }

        [StringLength(500)]
        public string GhiChuDiemDon { get; set; }

        public bool CanXacNhanDiemDon { get; set; }

        public virtual DatTour DatTour { get; set; }

        public virtual Tour Tour { get; set; }
    }
}
