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
    public class HomeAdminController : Controller
    {
        private ModelDB _contextDB = new ModelDB();

        // GET: Admin/HomeAdmin
        public ActionResult Index()
        {
            ViewBag.TotalTours = _contextDB.Tours.Count();
            ViewBag.TourMienBac = _contextDB.Tours.Count(t => t.DanhMuc != null && t.DanhMuc.TenDanhMuc.Contains("Bắc"));
            ViewBag.TourMienTrung = _contextDB.Tours.Count(t => t.DanhMuc != null && t.DanhMuc.TenDanhMuc.Contains("Trung"));
            ViewBag.TourMienNam = _contextDB.Tours.Count(t => t.DanhMuc != null && t.DanhMuc.TenDanhMuc.Contains("Nam"));

            ViewBag.TotalDatTours = _contextDB.DatTours.Count();
            ViewBag.PendingDatTours = _contextDB.DatTours.Count(d => d.TrangThai == "Chờ xử lý");

            var now = DateTime.Now;
            ViewBag.MonthlyRevenue = _contextDB.DatTours
                .Where(d => d.NgayDat.HasValue && d.NgayDat.Value.Year == now.Year
                    && d.NgayDat.Value.Month == now.Month && d.TrangThai == "Đã xác nhận")
                .Sum(d => (decimal?)d.TongTien) ?? 0;

            var recentDatTours = _contextDB.DatTours
                .OrderByDescending(d => d.NgayDat)
                .Take(5)
                .Select(d => new DatTourView
                {
                    ID_DatTour = d.ID_DatTour,
                    TenNguoiDung = d.NguoiDung.HoTen,
                    NgayDat = d.NgayDat,
                    TongTien = d.TongTien,
                    TrangThai = d.TrangThai,
                    GhiChu = d.GhiChu
                })
                .ToList();

            var currentYear = now.Year;
            ViewBag.MonthLabels = Enumerable.Range(1, 12).Select(m => m.ToString("00")).ToList();
            ViewBag.MonthData = Enumerable.Range(1, 12).Select(i =>
                _contextDB.DatTours.Count(d =>
                    d.NgayDat.HasValue &&
                    d.NgayDat.Value.Year == currentYear &&
                    d.NgayDat.Value.Month == i)
            ).ToArray();

            var monthlyRevenueList = new List<decimal>();
            for (int i = 1; i <= 12; i++)
            {
                var rev = _contextDB.DatTours
                    .Where(d => d.NgayDat.HasValue && d.NgayDat.Value.Year == currentYear && d.NgayDat.Value.Month == i && d.TrangThai == "Đã xác nhận")
                    .Sum(d => (decimal?)d.TongTien) ?? 0;
                monthlyRevenueList.Add(rev);
            }
            ViewBag.MonthRevenueList = monthlyRevenueList;

            return View(recentDatTours);
        }

        [HttpPost]
        public JsonResult GetDoanhThuTheoNam(int year)
        {
            try
            {
                var monthlyRevenueList = new List<decimal>();
                for (int i = 1; i <= 12; i++)
                {
                    var rev = _contextDB.DatTours
                        .Where(d => d.NgayDat.HasValue && d.NgayDat.Value.Year == year && d.NgayDat.Value.Month == i && d.TrangThai == "Đã xác nhận")
                        .Sum(d => (decimal?)d.TongTien) ?? 0;
                    monthlyRevenueList.Add(rev);
                }
                return Json(new { success = true, data = monthlyRevenueList });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _contextDB.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
