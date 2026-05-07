using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;
using TourDulich.Areas.Admin.Filters;
using System.IO;

namespace TourDulich.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class QuanlyDiaDiemController : Controller
    {
        private ModelDB _contextDB = new ModelDB();

        public ActionResult Index(string searchString)
        {
            var query = _contextDB.DiaDiems.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d => d.TenDiaDiem.Contains(searchString));
            }

            ViewBag.SearchString = searchString;

            var list = query.ToList();
            return View(list);
        }

        [HttpPost]
        public JsonResult ThemDiaDiemAjax(DiaDiem model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.TenDiaDiem))
                {
                    return Json(new { success = false, message = "Tên địa điểm không được để trống" });
                }

                _contextDB.DiaDiems.Add(new DiaDiem
                {
                    TenDiaDiem = model.TenDiaDiem,
                    MoTa = model.MoTa,
                    TinhThanh = model.TinhThanh,
                    Hinh = model.Hinh ?? "default.jpg"
                });

                _contextDB.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult SuaDiaDiemPartial(int id)
        {
            var dd = _contextDB.DiaDiems.Find(id);
            if (dd == null) return HttpNotFound();
            return PartialView("_SuaDiaDiemPartial", dd);
        }

        [HttpPost]
        public JsonResult LuuSuaDiaDiem(DiaDiem model)
        {
            try
            {
                var existing = _contextDB.DiaDiems.Find(model.ID_DiaDiem);
                if (existing == null) return Json(new { success = false, message = "Không tìm thấy địa điểm" });

                existing.TenDiaDiem = model.TenDiaDiem;
                existing.MoTa = model.MoTa;
                existing.TinhThanh = model.TinhThanh;

                _contextDB.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult XoaDiaDiem(int id)
        {
            try
            {
                var existing = _contextDB.DiaDiems.Find(id);
                if (existing == null) return Json(new { success = false, message = "Không tìm thấy địa điểm" });

                bool isUsed = _contextDB.Tours.Any(t => t.ID_DiaDiem == id);
                if (isUsed)
                {
                    return Json(new { success = false, message = "Không thể xóa địa điểm này vì đã có Tour thuộc địa điểm này." });
                }

                _contextDB.DiaDiems.Remove(existing);
                _contextDB.SaveChanges();
                return Json(new { success = true });
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
