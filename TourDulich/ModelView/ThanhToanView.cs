using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TourDulich.Models;

namespace TourDulich.ModelView
{
    public class ThanhToanView
    {
        public int? ID_DatTour { get; set; }
        public List<TourDaDatTamThoi> DanhSachTour { get; set; }
        public NguoiDung UserInfo { get; set; }
        public string PhuongThucThanhToan { get; set; }
    }
}
