using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;

namespace TourDulich.ModelView
{
    public class TourView
    {
        public int ID_Tour { get; set; }
        [DisplayName("Tên Tour")]
        public string TenTour { get; set; }
        [DisplayName("Hình ảnh")]
        public string HinhAnh { get; set; }
        public string MoTa {  get; set; }
        [DisplayName("Giá Tour")]
        public decimal? Gia {  get; set; }
        [DisplayName("Số ngày")]
        public string SoNgay { get; set; }
        [DisplayName("Nơi khởi hành")]
        public string DiemKhoiHanh { get; set; }
        public string Hinh {  get; set; }
        public int? ID_DanhMuc { get; set; }
        [DisplayName("Tên danh mục")]
        public string TenDanhMuc {  get; set; }
        public string PhuongTien { get; set; }
        public int? ID_DiaDiem {  get; set; }
        [DisplayName("Điểm đến")]
        public string DiemDen {  get; set; }
        public int? HienThi { get; set; }
        public int? SoLuongToiDa { get; set; }
        public int SoSao {  get; set; }
        public DateTime? NgayTao {  get; set; }
        public bool TrangThaiHoatDong { get; set; } = true;
        public bool IsGiaTot { get; set; } = false;
        public bool IsUuDai { get; set; } = false;
        public SelectList DanhMucSelectList { get; set; }
        public SelectList DiaDiemSelectList { get; set; }

        public List<string> HinhAnhKhac { get; set; }
        public List<HinhAnhTour> DanhSachHinhAnhTour { get; set; }
        public List<LichTrinhTourView> LichTrinhTours { get; set; }

    }
}