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
        public decimal? Gia { get; set; }
        public decimal? ThanhTien => SoLuong * Gia;
    }
}