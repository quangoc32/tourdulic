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
        public string LoaiDiemDon { get; set; }
        public string DiemDon { get; set; }
        public string DiaChiDon { get; set; }
        public string TinhThanhDon { get; set; }
        public decimal? PhuThuDiemDon { get; set; }
        public string GhiChuDiemDon { get; set; }
        public bool CanXacNhanDiemDon { get; set; }
    }
}
