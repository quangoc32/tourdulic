using System;

namespace TourDulich.ModelView
{
    public class LichKhoiHanhView
    {
        public int ID_LichKhoiHanh { get; set; }
        public int ID_Tour { get; set; }
        public string TenTour { get; set; }
        public DateTime NgayKhoiHanh { get; set; }
        public int SoLuongToiDa { get; set; }
        public int SoLuongDaDat { get; set; }
        public int SoLuongConLai => SoLuongToiDa - SoLuongDaDat;
        public string TrangThai { get; set; }
        public string GhiChu { get; set; }
        
        // Giá mùa
        public decimal GiaGoc { get; set; }
        public string TenMua { get; set; }
        public decimal HeSoGia { get; set; }
        public decimal GiaThucTe => Math.Round(GiaGoc * HeSoGia, 0);

        // Hiển thị badge màu
        public string BadgeClass
        {
            get
            {
                if (TrangThai == "Đóng") return "badge-secondary";
                if (SoLuongConLai <= 0) return "badge-danger";
                if (SoLuongConLai <= 5) return "badge-warning";
                return "badge-success";
            }
        }
    }

    public class YeuCauHuyView
    {
        public int ID_YeuCauHuy { get; set; }
        public int ID_DatTour { get; set; }
        public string TenNguoiDung { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public DateTime? NgayGui { get; set; }
        public string LyDo { get; set; }
        public string TrangThai { get; set; }
        public int? PhanTramHoan { get; set; }
        public decimal? TienHoan { get; set; }
        public decimal? TongTienDatTour { get; set; }
        public DateTime? NgayXuLy { get; set; }
        public string GhiChuAdmin { get; set; }

        // Ngày khởi hành gần nhất của đơn
        public DateTime? NgayKhoiHanhSom { get; set; }

        // Số ngày còn lại trước khởi hành
        public int? SoNgayConLai => NgayKhoiHanhSom.HasValue
            ? (int)(NgayKhoiHanhSom.Value - DateTime.Today).TotalDays
            : (int?)null;
    }

    public class GuiYeuCauHuyViewModel
    {
        public int ID_DatTour { get; set; }
        public string LyDo { get; set; }
    }

    public class XuLyYeuCauHuyViewModel
    {
        public int ID_YeuCauHuy { get; set; }
        public string TrangThai { get; set; }   // Chấp thuận / Từ chối
        public int PhanTramHoan { get; set; }
        public decimal TienHoan { get; set; }
        public string GhiChuAdmin { get; set; }
    }
}
