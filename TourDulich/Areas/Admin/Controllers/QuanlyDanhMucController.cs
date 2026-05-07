using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;
using TourDulich.Areas.Admin.Filters;

namespace TourDulich.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class QuanlyDanhMucController : Controller
    {
        private ModelDB _contextDB = new ModelDB();

        public ActionResult Index()
        {
            var list = _contextDB.DanhMucs.ToList();
            return View(list);
        }

        [HttpPost]
        public JsonResult ThemDanhMucAjax(DanhMuc model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.TenDanhMuc))
                {
                    return Json(new { success = false, message = "Tên danh mục không được để trống" });
                }

                _contextDB.DanhMucs.Add(new DanhMuc
                {
                    TenDanhMuc = model.TenDanhMuc,
                    MoTa = model.MoTa
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
        public ActionResult SuaDanhMucPartial(int id)
        {
            var dm = _contextDB.DanhMucs.Find(id);
            if (dm == null) return HttpNotFound();
            return PartialView("_SuaDanhMucPartial", dm);
        }

        [HttpPost]
        public JsonResult LuuSuaDanhMuc(DanhMuc model)
        {
            try
            {
                var existing = _contextDB.DanhMucs.Find(model.ID_DanhMuc);
                if (existing == null) return Json(new { success = false, message = "Không tìm thấy danh mục" });

                existing.TenDanhMuc = model.TenDanhMuc;
                existing.MoTa = model.MoTa;

                _contextDB.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult XoaDanhMuc(int id)
        {
            try
            {
                var existing = _contextDB.DanhMucs.Find(id);
                if (existing == null) return Json(new { success = false, message = "Không tìm thấy danh mục" });

                bool isUsed = _contextDB.Tours.Any(t => t.ID_DanhMuc == id);
                if (isUsed)
                {
                    return Json(new { success = false, message = "Không thể xóa danh mục này vì đã có Tour thuộc danh mục này." });
                }

                _contextDB.DanhMucs.Remove(existing);
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
