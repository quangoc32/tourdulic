using System;
using System.ComponentModel.DataAnnotations;

namespace TourDulich.ModelView
{
    public class DiemDonTourView
    {
        public int Id { get; set; }

        [Required]
        public int ID_Tour { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên điểm đón.")]
        [StringLength(150)]
        public string TenDiemDon { get; set; }

        [StringLength(250)]
        public string DiaChi { get; set; }

        [StringLength(100)]
        public string TinhThanh { get; set; }

        public decimal PhuThu { get; set; }

        public bool LaTuTuc { get; set; }

        public bool HienThi { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
