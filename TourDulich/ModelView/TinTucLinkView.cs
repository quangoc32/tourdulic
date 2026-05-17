using System;
using System.ComponentModel.DataAnnotations;

namespace TourDulich.ModelView
{
    public class TinTucLinkView
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
        [StringLength(200)]
        public string TieuDe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả ngắn.")]
        [StringLength(500)]
        public string MoTaNgan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập link bài viết.")]
        [Url(ErrorMessage = "Link bài viết không hợp lệ.")]
        [StringLength(1000)]
        public string LinkBaiViet { get; set; }

        [Url(ErrorMessage = "Link ảnh không hợp lệ.")]
        [StringLength(1000)]
        public string HinhAnh { get; set; }

        [StringLength(100)]
        public string Nguon { get; set; }

        public bool LaTinHot { get; set; } = true;

        public bool HienThi { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
