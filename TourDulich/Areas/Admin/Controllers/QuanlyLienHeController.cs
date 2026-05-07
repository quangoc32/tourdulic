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
    public class QuanlyLienHeController : Controller
    {
        private ModelDB _contextDB = new ModelDB();

        public ActionResult Index(string searchString)
        {
            var query = _contextDB.LienHes.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(lh => lh.HoTen.Contains(searchString) || 
                                          lh.Email.Contains(searchString) || 
                                          lh.TieuDe.Contains(searchString));
            }

            var list = query.OrderByDescending(x => x.NgayGui).ToList();
            ViewBag.SearchString = searchString;

            return View(list);
        }

        [HttpPost]
        public JsonResult DanhDauDaXuLy(int id)
        {
            try
            {
                var lh = _contextDB.LienHes.Find(id);
                if (lh != null)
                {
                    lh.TrangThai = "Đã xử lý";
                    _contextDB.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy yêu cầu liên hệ." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult XoaLienHe(int id)
        {
            try
            {
                var lh = _contextDB.LienHes.Find(id);
                if (lh != null)
                {
                    _contextDB.LienHes.Remove(lh);
                    _contextDB.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy yêu cầu liên hệ." });
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
