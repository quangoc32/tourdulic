using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TourDulich.Models;

namespace TourDulich.ModelView
{
    public class DatTourView
    {
        public int ID_DatTour { get; set; }
        public string TenNguoiDung { get; set; }
        public DateTime? NgayDat { get; set; }
        public decimal? TongTien { get; set; }
        public string TrangThai { get; set; }
        public string GhiChu { get; set; }

        // Loại đặt (Khách lẻ / Đoàn)
        public string LoaiDat { get; set; }
        public bool LaDoan => LoaiDat == AppConstants.LoaiDat.Doan;
        public string TruongDoan { get; set; }
        public string SdtTruongDoan { get; set; }
        public string GhiChuDoan { get; set; }

        // Chi tiết
        public List<ChiTietDatTour> ChiTietTours { get; set; }
    }


}
