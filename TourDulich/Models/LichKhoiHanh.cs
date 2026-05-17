namespace TourDulich.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("LichKhoiHanh")]
    public partial class LichKhoiHanh
    {
        [Key]
        public int ID_LichKhoiHanh { get; set; }

        public int ID_Tour { get; set; }

        [Column(TypeName = "date")]
        public DateTime NgayKhoiHanh { get; set; }

        public int SoLuongToiDa { get; set; } = 20;

        public int SoLuongDaDat { get; set; } = 0;

        [NotMapped]
        public int SoLuongConLai => SoLuongToiDa - SoLuongDaDat;

        [StringLength(20)]
        public string TrangThai { get; set; } = AppConstants.TrangThaiLichKhoiHanh.Mo; // Mở / Đóng / Hết chỗ

        [StringLength(200)]
        public string GhiChu { get; set; }

        public virtual Tour Tour { get; set; }
    }
}
