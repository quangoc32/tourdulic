using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TourDulich.ModelView
{
    public class LichTrinhTourView
    {
         public int ID_LichTrinhTour { get; set; }

         public int? ID_Tour { get; set; }

         public int? NgayThu { get; set; }

         public string TieuDe { get; set; }

         public string NoiDung { get; set; }
         public bool DaCoLichTrinh { get; set; }
    }

}
