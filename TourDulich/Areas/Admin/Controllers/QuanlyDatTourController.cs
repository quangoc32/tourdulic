using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;
using TourDulich.ModelView;
using TourDulich.Areas.Admin.Filters;

namespace TourDulich.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class QuanlyDatTourController : Controller
    {
        private ModelDB _contextDB = new ModelDB();

        // GET: Admin/QuanlyDatTour
        public ActionResult DanhSachDatTour(string searchString)
        {
            var query = _contextDB.DatTours.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(dt => dt.NguoiDung.HoTen.Contains(searchString) || 
                                          dt.NguoiDung.Email.Contains(searchString) || 
                                          dt.NguoiDung.SoDienThoai.Contains(searchString));
            }

            var list = query
                .Select(dt => new DatTourView
                {
                    ID_DatTour    = dt.ID_DatTour,
                    TenNguoiDung  = dt.NguoiDung.HoTen,
                    NgayDat       = dt.NgayDat,
                    TongTien      = dt.TongTien,
                    TrangThai     = dt.TrangThai,
                    GhiChu        = dt.GhiChu,
                    LoaiDat       = dt.LoaiDat,
                    TruongDoan    = dt.TruongDoan,
                    SdtTruongDoan = dt.SdtTruongDoan,
                    GhiChuDoan    = dt.GhiChuDoan
                }).ToList();


            ViewBag.SearchString = searchString;

            return View(list);
        }

        public ActionResult GetChiTietDatTourPartial(int id)
        {
            var chiTiet = _contextDB.ChiTietDatTours
                .Where(c => c.ID_DatTour == id)
                .ToList();

            return PartialView("_ChiTietDatTourPartial", chiTiet);
        }
        public JsonResult GetTrangThaiDatTour(int id)
        {
            var datTour = _contextDB.DatTours.Find(id);
            if (datTour == null)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = true,
                ID_DatTour = datTour.ID_DatTour,
                TrangThai = datTour.TrangThai,
                GhiChu = datTour.GhiChu
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CapNhatTrangThai(int ID_DatTour, string TrangThai, string GhiChu)
        {
            try
            {
                var datTour = _contextDB.DatTours.Find(ID_DatTour);
                if (datTour == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đặt tour." });
                }

                datTour.TrangThai = TrangThai;
                datTour.GhiChu = GhiChu;
                _contextDB.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult XacNhanDiemDon(int idChiTietDatTour, decimal phuThuDiemDon, string ghiChuDiemDon)
        {
            try
            {
                var chiTiet = _contextDB.ChiTietDatTours.Find(idChiTietDatTour);
                if (chiTiet == null)
                    return Json(new { success = false, message = "Không tìm thấy chi tiết đặt tour." });

                chiTiet.PhuThuDiemDon = phuThuDiemDon;
                chiTiet.GhiChuDiemDon = ghiChuDiemDon;
                chiTiet.CanXacNhanDiemDon = false;

                var datTour = _contextDB.DatTours.Find(chiTiet.ID_DatTour);
                if (datTour != null)
                {
                    bool conDiemDonCanXacNhan = _contextDB.ChiTietDatTours
                        .Any(c => c.ID_DatTour == datTour.ID_DatTour && c.ID_ChiTietDatTour != idChiTietDatTour && c.CanXacNhanDiemDon);

                    var chiTiets = _contextDB.ChiTietDatTours
                        .Where(c => c.ID_DatTour == datTour.ID_DatTour)
                        .ToList();

                    datTour.TongTien = chiTiets.Sum(c => ((c.Gia ?? 0) + (c.PhuThuDiemDon ?? 0)) * (c.SoLuongNguoi ?? 0));
                    datTour.GhiChu = string.IsNullOrWhiteSpace(ghiChuDiemDon)
                        ? "Admin đã xác nhận điểm đón. Chờ khách thanh toán."
                        : ghiChuDiemDon;

                    if (!conDiemDonCanXacNhan)
                    {
                        datTour.TrangThai = AppConstants.TrangThaiDatTour.ChoThanhToan;
                    }
                }

                _contextDB.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult XacNhanDiemDonTheoDon(int idDatTour, decimal phuThuDiemDon, string ghiChuDiemDon)
        {
            try
            {
                var datTour = _contextDB.DatTours.Find(idDatTour);
                if (datTour == null)
                    return Json(new { success = false, message = "Không tìm thấy đơn đặt tour." });

                var chiTiets = _contextDB.ChiTietDatTours
                    .Where(c => c.ID_DatTour == idDatTour)
                    .ToList();

                var diemDonCanXacNhan = chiTiets.Where(c => c.CanXacNhanDiemDon).ToList();
                if (!diemDonCanXacNhan.Any())
                    return Json(new { success = false, message = "Đơn này không có điểm đón cần xác nhận." });

                foreach (var chiTiet in diemDonCanXacNhan)
                {
                    chiTiet.PhuThuDiemDon = phuThuDiemDon;
                    chiTiet.GhiChuDiemDon = ghiChuDiemDon;
                    chiTiet.CanXacNhanDiemDon = false;
                }

                datTour.TongTien = chiTiets.Sum(c => ((c.Gia ?? 0) + (c.PhuThuDiemDon ?? 0)) * (c.SoLuongNguoi ?? 0));
                datTour.TrangThai = AppConstants.TrangThaiDatTour.ChoThanhToan;
                datTour.GhiChu = string.IsNullOrWhiteSpace(ghiChuDiemDon)
                    ? "Admin đã xác nhận điểm đón. Chờ khách thanh toán."
                    : ghiChuDiemDon;

                _contextDB.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult XoaDatTour(int id)
        {
            try
            {
                var datTour = _contextDB.DatTours.Find(id);
                if (datTour == null)
                    return Json(new { success = false, message = "Không tìm thấy đơn đặt tour." });

                var chiTiets = _contextDB.ChiTietDatTours.Where(c => c.ID_DatTour == id).ToList();
                _contextDB.ChiTietDatTours.RemoveRange(chiTiets);

                _contextDB.DatTours.Remove(datTour);
                _contextDB.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
