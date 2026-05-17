using System;
using System.Linq;
using System.Web.Mvc;
using TourDulich.Models;
using TourDulich.ModelView;
using TourDulich.Areas.Admin.Filters;

namespace TourDulich.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class QuanlyYeuCauHuyController : Controller
    {
        private ModelDB _db = new ModelDB();

        public ActionResult Index(string trangThai)
        {
            var query = _db.YeuCauHuys.AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(y => y.TrangThai == trangThai);

            var list = query.OrderByDescending(y => y.NgayGui)
                .Select(y => new YeuCauHuyView
                {
                    ID_YeuCauHuy    = y.ID_YeuCauHuy,
                    ID_DatTour      = y.ID_DatTour,
                    TenNguoiDung    = y.DatTour.NguoiDung.HoTen,
                    Email           = y.DatTour.NguoiDung.Email,
                    SoDienThoai     = y.DatTour.NguoiDung.SoDienThoai,
                    NgayGui         = y.NgayGui,
                    LyDo            = y.LyDo,
                    TenNganHang     = y.TenNganHang,
                    SoTaiKhoanHoanTien = y.SoTaiKhoanHoanTien,
                    TenChuTaiKhoan  = y.TenChuTaiKhoan,
                    TrangThai       = y.TrangThai,
                    PhanTramHoan    = y.PhanTramHoan,
                    TienHoan        = y.TienHoan,
                    TongTienDatTour = y.DatTour.TongTien,
                    NgayXuLy        = y.NgayXuLy,
                    GhiChuAdmin     = y.GhiChuAdmin,
                    NgayKhoiHanhSom = y.DatTour.ChiTietDatTours
                                       .Select(c => c.NgayKhoiHanh)
                                       .OrderBy(d => d)
                                       .FirstOrDefault()
                }).ToList();

            ViewBag.TrangThai = trangThai;
            ViewBag.SoPending = _db.YeuCauHuys.Count(y => y.TrangThai == AppConstants.TrangThaiYeuCauHuy.ChoXuLy);
            return View(list);
        }

        [HttpPost]
        public JsonResult XuLy(XuLyYeuCauHuyViewModel model)
        {
            try
            {
                var ycHuy = _db.YeuCauHuys.Find(model.ID_YeuCauHuy);
                if (ycHuy == null)
                    return Json(new { success = false, message = "Không tìm thấy yêu cầu." });

                if (ycHuy.TrangThai != AppConstants.TrangThaiYeuCauHuy.ChoXuLy)
                    return Json(new { success = false, message = "Yêu cầu này đã được xử lý trước đó." });

                ycHuy.TrangThai    = model.TrangThai;
                ycHuy.PhanTramHoan = model.PhanTramHoan;
                ycHuy.TienHoan     = model.TienHoan;
                ycHuy.GhiChuAdmin  = model.GhiChuAdmin;
                ycHuy.NgayXuLy     = DateTime.Now;

                var datTour = _db.DatTours.Find(ycHuy.ID_DatTour);
                if (datTour != null)
                {
                    datTour.CoYeuCauHuy = false;

                    if (model.TrangThai == AppConstants.TrangThaiYeuCauHuy.ChapThuan)
                    {
                        datTour.TrangThai = AppConstants.TrangThaiDatTour.DaHuy;
                        datTour.GhiChu    = $"Đã hủy theo yêu cầu. Hoàn {model.PhanTramHoan}% = {model.TienHoan:N0}đ. " + model.GhiChuAdmin;

                        // Trả lại slot cho LichKhoiHanh
                        foreach (var chiTiet in datTour.ChiTietDatTours)
                        {
                            var lich = _db.LichKhoiHanhs.FirstOrDefault(l =>
                                l.ID_Tour == chiTiet.ID_Tour &&
                                l.NgayKhoiHanh == chiTiet.NgayKhoiHanh);

                            if (lich != null && lich.SoLuongDaDat > 0)
                            {
                                lich.SoLuongDaDat -= (chiTiet.SoLuongNguoi ?? 0);
                                if (lich.SoLuongDaDat < 0) lich.SoLuongDaDat = 0;

                                // Mở lại lịch nếu đang "Hết chỗ"
                                if (lich.TrangThai == AppConstants.TrangThaiLichKhoiHanh.HetCho)
                                    lich.TrangThai = AppConstants.TrangThaiLichKhoiHanh.Mo;
                            }
                        }
                    }
                }

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
