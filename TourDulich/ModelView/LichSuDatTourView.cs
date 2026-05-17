using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TourDulich.Models;

namespace TourDulich.ModelView
{
    public class LichSuDatTourView
    {
        public int ID_DatTour { get; set; }
        public DateTime? NgayDat { get; set; }
        public decimal? TongTien { get; set; }
        public string TrangThai { get; set; }
        public bool CoYeuCauHuy { get; set; }
        public decimal? TienHoan { get; set; }

        public string LoaiDat { get; set; }
        public bool LaDoan => LoaiDat == AppConstants.LoaiDat.Doan;
        public string TruongDoan { get; set; }
        public string SdtTruongDoan { get; set; }
        public string GhiChuDoan { get; set; }
        public bool CoTheThanhToan => TrangThai == AppConstants.TrangThaiDatTour.ChoThanhToan;

        public List<ChiTietLichSuTourView> ChiTietTours { get; set; }

        // Ngày khởi hành gần nhất trong đơn
        public DateTime? NgayKhoiHanhSom =>
            ChiTietTours?.Where(c => c.NgayKhoiHanh.HasValue)
                         .OrderBy(c => c.NgayKhoiHanh)
                         .Select(c => c.NgayKhoiHanh)
                         .FirstOrDefault();

        // Điều kiện 2: Còn ít nhất 7 ngày trước khởi hành
        public bool ConHon7NgayTruocKH => NgayKhoiHanhSom.HasValue &&
            (NgayKhoiHanhSom.Value.Date - DateTime.Today).TotalDays >= 7;

        // Tổng hợp: được phép yêu cầu hủy
        public bool CoTheHuy => ConHon7NgayTruocKH
                                 && TrangThai != AppConstants.TrangThaiDatTour.DaHuy
                                 && !CoYeuCauHuy;

        // Thông báo lý do không được hủy (hiển thị tooltip cho user)
        public string LyDoKhongTheHuy
        {
            get
            {
                if (TrangThai == AppConstants.TrangThaiDatTour.DaHuy || CoYeuCauHuy) return null;
                if (!ConHon7NgayTruocKH)
                {
                    return "Đã quá thời hạn hủy (phải hủy trước 7 ngày khởi hành)";
                }
                return null;
            }
        }
    }
}
