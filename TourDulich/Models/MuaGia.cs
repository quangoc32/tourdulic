namespace TourDulich.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("MuaGia")]
    public partial class MuaGia
    {
        [Key]
        public int ID_MuaGia { get; set; }

        [Required]
        [StringLength(100)]
        public string TenMua { get; set; }

        [Column(TypeName = "date")]
        public DateTime NgayBatDau { get; set; }

        [Column(TypeName = "date")]
        public DateTime NgayKetThuc { get; set; }

        public decimal HeSoGia { get; set; } = 1.00m; // 1.30 = +30%, 0.80 = -20%

        [StringLength(300)]
        public string MoTa { get; set; }

        public bool IsActive { get; set; } = true;

        // Tiện ích hiển thị
        [NotMapped]
        public string HeSoGiaDisplay => HeSoGia >= 1
            ? $"+{(HeSoGia - 1) * 100:0}%"
            : $"-{(1 - HeSoGia) * 100:0}%";

        [NotMapped]
        public bool DangApDung => IsActive
            && DateTime.Today >= NgayBatDau
            && DateTime.Today <= NgayKetThuc;
    }
}
