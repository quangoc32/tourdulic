namespace TourDulich.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("YeuCauHuy")]
    public partial class YeuCauHuy
    {
        [Key]
        public int ID_YeuCauHuy { get; set; }

        public int ID_DatTour { get; set; }

        public DateTime NgayGui { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string LyDo { get; set; }

        [StringLength(30)]
        public string TrangThai { get; set; } = "Chờ xử lý"; // Chờ xử lý / Chấp thuận / Từ chối

        public int? PhanTramHoan { get; set; }

        public decimal? TienHoan { get; set; }

        public DateTime? NgayXuLy { get; set; }

        [StringLength(500)]
        public string GhiChuAdmin { get; set; }

        public virtual DatTour DatTour { get; set; }
    }
}
