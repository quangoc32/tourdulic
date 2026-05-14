using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TourDulich.Models;
using TourDulich.ModelView;
using TourDulich.Areas.Admin.Filters;

namespace TourDulich.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class QuanlyLichKhoiHanhController : Controller
    {
        private ModelDB _db = new ModelDB();

        // GET: Admin/QuanlyLichKhoiHanh
        public ActionResult Index(int? tourId, int? thang, int? nam)
        {
            var tours = _db.Tours.Where(t => !t.DaXoa && t.TrangThaiHoatDong)
                                 .OrderBy(t => t.TenTour).ToList();
            ViewBag.Tours = new SelectList(tours, "ID_Tour", "TenTour", tourId);

            ViewBag.Thang = thang;
            ViewBag.Nam = nam;

            var query = _db.LichKhoiHanhs.AsQueryable();
            if (tourId.HasValue)
                query = query.Where(l => l.ID_Tour == tourId.Value);

            if (thang.HasValue)
                query = query.Where(l => l.NgayKhoiHanh.Month == thang.Value);

            if (nam.HasValue)
                query = query.Where(l => l.NgayKhoiHanh.Year == nam.Value);

            var activeSeasons = _db.MuaGias.Where(m => m.IsActive).ToList();

            var listRaw = query.OrderBy(l => l.ID_Tour).ThenBy(l => l.NgayKhoiHanh)
                .Select(l => new 
                {
                    ID_LichKhoiHanh = l.ID_LichKhoiHanh,
                    ID_Tour = l.ID_Tour,
                    TenTour = l.Tour.TenTour,
                    NgayKhoiHanh = l.NgayKhoiHanh,
                    SoLuongToiDa = l.SoLuongToiDa,
                    SoLuongDaDat = l.SoLuongDaDat,
                    TrangThai = l.TrangThai,
                    GhiChu = l.GhiChu,
                    GiaGoc = l.Tour.Gia ?? 0
                }).ToList();

            var list = listRaw.Select(l => 
            {
                var muaApDung = activeSeasons
                    .Where(m => m.NgayBatDau <= l.NgayKhoiHanh && m.NgayKetThuc >= l.NgayKhoiHanh)
                    .OrderByDescending(m => m.HeSoGia)
                    .FirstOrDefault();

                return new LichKhoiHanhView
                {
                    ID_LichKhoiHanh = l.ID_LichKhoiHanh,
                    ID_Tour = l.ID_Tour,
                    TenTour = l.TenTour,
                    NgayKhoiHanh = l.NgayKhoiHanh,
                    SoLuongToiDa = l.SoLuongToiDa,
                    SoLuongDaDat = l.SoLuongDaDat,
                    TrangThai = l.TrangThai,
                    GhiChu = l.GhiChu,
                    GiaGoc = l.GiaGoc,
                    TenMua = muaApDung != null ? muaApDung.TenMua : "",
                    HeSoGia = muaApDung != null ? muaApDung.HeSoGia : 1.0m
                };
            }).ToList();

            ViewBag.TourId = tourId;
            return View(list);
        }

        // GET: Admin/QuanlyLichKhoiHanh/Create
        public ActionResult Create()
        {
            var tours = _db.Tours.Where(t => !t.DaXoa && t.TrangThaiHoatDong)
                                 .OrderBy(t => t.TenTour).ToList();
            ViewBag.Tours = new SelectList(tours, "ID_Tour", "TenTour");
            return View(new LichKhoiHanh());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LichKhoiHanh model)
        {
            // Kiểm tra trùng ngày
            bool trung = _db.LichKhoiHanhs.Any(l =>
                l.ID_Tour == model.ID_Tour &&
                l.NgayKhoiHanh == model.NgayKhoiHanh);

            if (trung)
                ModelState.AddModelError("NgayKhoiHanh", "Tour này đã có lịch khởi hành ngày đó rồi.");

            if (ModelState.IsValid)
            {
                _db.LichKhoiHanhs.Add(model);
                _db.SaveChanges();
                TempData["Success"] = "Thêm lịch khởi hành thành công!";
                return RedirectToAction("Index");
            }

            var tours = _db.Tours.Where(t => !t.DaXoa && t.TrangThaiHoatDong).ToList();
            ViewBag.Tours = new SelectList(tours, "ID_Tour", "TenTour", model.ID_Tour);
            return View(model);
        }

        // POST: Thêm nhiều ngày cùng lúc (Bulk Add)
        [HttpPost]
        public JsonResult BulkCreate(int tourId, DateTime ngayBatDau, DateTime ngayKetThuc,
                                     int soLuongToiDa, string ngayTrongTuan)
        {
            try
            {
                // ngayTrongTuan: "1,2,3,4,5,6,0" (0=CN, 1=Thứ 2, ...)
                var daysOfWeek = string.IsNullOrEmpty(ngayTrongTuan)
                    ? new List<int>()
                    : ngayTrongTuan.Split(',').Select(int.Parse).ToList();

                int soLuongThem = 0;
                var current = ngayBatDau;
                while (current <= ngayKetThuc)
                {
                    bool isSuitableDay = !daysOfWeek.Any() ||
                                         daysOfWeek.Contains((int)current.DayOfWeek);

                    if (isSuitableDay)
                    {
                        bool exists = _db.LichKhoiHanhs.Any(l =>
                            l.ID_Tour == tourId && l.NgayKhoiHanh == current);

                        if (!exists)
                        {
                            _db.LichKhoiHanhs.Add(new LichKhoiHanh
                            {
                                ID_Tour = tourId,
                                NgayKhoiHanh = current,
                                SoLuongToiDa = soLuongToiDa,
                                SoLuongDaDat = 0,
                                TrangThai = "Mở"
                            });
                            soLuongThem++;
                        }
                    }
                    current = current.AddDays(1);
                }

                _db.SaveChanges();
                return Json(new { success = true, soLuongThem });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Admin/QuanlyLichKhoiHanh/Edit/5
        public ActionResult Edit(int id)
        {
            var lich = _db.LichKhoiHanhs.Find(id);
            if (lich == null) return HttpNotFound();

            var tours = _db.Tours.Where(t => !t.DaXoa && t.TrangThaiHoatDong).ToList();
            ViewBag.Tours = new SelectList(tours, "ID_Tour", "TenTour", lich.ID_Tour);
            return View(lich);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LichKhoiHanh model)
        {
            if (ModelState.IsValid)
            {
                var lich = _db.LichKhoiHanhs.Find(model.ID_LichKhoiHanh);
                if (lich == null) return HttpNotFound();

                lich.SoLuongToiDa = model.SoLuongToiDa;
                lich.TrangThai = model.TrangThai;
                lich.GhiChu = model.GhiChu;
                _db.SaveChanges();

                TempData["Success"] = "Cập nhật lịch thành công!";
                return RedirectToAction("Index");
            }

            var tours = _db.Tours.Where(t => !t.DaXoa && t.TrangThaiHoatDong).ToList();
            ViewBag.Tours = new SelectList(tours, "ID_Tour", "TenTour", model.ID_Tour);
            return View(model);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var lich = _db.LichKhoiHanhs.Find(id);
                if (lich == null)
                    return Json(new { success = false, message = "Không tìm thấy lịch." });

                if (lich.SoLuongDaDat > 0)
                    return Json(new { success = false, message = "Lịch này đã có người đặt, không thể xóa." });

                _db.LichKhoiHanhs.Remove(lich);
                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Method moved to HomeController for public access
    }
}
