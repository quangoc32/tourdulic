using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TourDulich.ModelView
{
    public class ChiTietLichSuTourView
    {
        public string TenTour { get; set; }
        public DateTime? NgayKhoiHanh { get; set; }
        public int? SoLuongNguoi { get; set; }
        public decimal? Gia { get; set; }
        public string PhuongThucThanhToan { get; set; }
    }
}