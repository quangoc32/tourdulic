using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TourDulich.ModelView
{
    public class LichSuDatTourView
    {
        public int ID_DatTour { get; set; }
        public DateTime? NgayDat { get; set; }
        public decimal? TongTien { get; set; }
        public string TrangThai { get; set; }
        public List<ChiTietLichSuTourView> ChiTietTours { get; set; }
    }
}