using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TourDulich.ModelView
{
    public class TourDaDatTamThoi
    {
        public int TourId { get; set; }
        public string TenTour { get; set; }
        public string HinhAnh { get; set; }
        public DateTime NgayDi { get; set; }
        public int SoLuong { get; set; }
        public decimal? Gia { get; set; }          // Giá gốc từ Tour.Gia
        public decimal? GiaThucTe { get; set; }    // Giá sau khi áp mùa
        public string TenMua { get; set; }         // Tên mùa đang áp dụng (null nếu không có)
        public decimal? ThanhTien => SoLuong * (GiaThucTe ?? Gia);

        // Thông tin loại đặt
        public string LoaiDat { get; set; } = "Khách lẻ";
        public bool LaDoan => LoaiDat == "Đoàn";
        public string TruongDoan { get; set; }
        public string SdtTruongDoan { get; set; }
        public string GhiChuDoan { get; set; }
    }
}