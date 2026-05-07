using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TourDulich.ModelView
{
    public class DanhGiaView
    {
        public int ID_DanhGia { get; set; }
        public int ID_Tour { get; set; }
        public int? ID_NguoiDung { get; set; } 
        public string HoTen { get; set; }
        public string NoiDung { get; set; }
        public int? SoSao { get; set; }
        public DateTime? NgayDanhGia { get; set; } = DateTime.Now;
    }
}